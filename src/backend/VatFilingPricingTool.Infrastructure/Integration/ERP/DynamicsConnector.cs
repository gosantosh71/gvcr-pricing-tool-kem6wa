using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Microsoft.Extensions.Options; // Microsoft.Extensions.Options package version 6.0.0
using Microsoft.Identity.Client; // Microsoft.Identity.Client package version 4.46.0
using System; // System.Net.Http package version 6.0.0
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json; // System.Text.Json package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Infrastructure.Clients;

namespace VatFilingPricingTool.Infrastructure.Integration.ERP
{
    /// <summary>
    /// Implements a connector for Microsoft Dynamics 365 CRM/ERP systems to retrieve transaction data for VAT filing calculations.
    /// Extends the abstract ErpConnector base class with Dynamics-specific functionality.
    /// </summary>
    public class DynamicsConnector : ErpConnector
    {
        private readonly DynamicsOptions _dynamicsOptions;
        private readonly ApiClient _apiClient;
        private string _accessToken;
        private DateTime _tokenExpiryTime;
        private IConfidentialClientApplication _confidentialClient;
        private readonly string _baseApiUrl;

        /// <summary>
        /// Initializes a new instance of the DynamicsConnector class with required dependencies.
        /// </summary>
        /// <param name="logger">Logger for operation logging.</param>
        /// <param name="dynamicsOptions">Configuration options for Dynamics 365.</param>
        /// <param name="erpOptions">Common ERP configuration options.</param>
        /// <param name="httpClient">HTTP client for making requests to Dynamics 365 API.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
        public DynamicsConnector(
            ILogger<DynamicsConnector> logger,
            IOptions<DynamicsOptions> dynamicsOptions,
            IOptions<ErpOptions> erpOptions,
            HttpClient httpClient) : base(logger, erpOptions)
        {
            if (dynamicsOptions?.Value == null)
                throw new ArgumentNullException(nameof(dynamicsOptions));

            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            _dynamicsOptions = dynamicsOptions.Value;
            _apiClient = new ApiClient(httpClient, logger)
                .WithRetryPolicy(Polly.Policy
                    .Handle<Exception>(ex => ex.IsTransient())
                    .WaitAndRetryAsync(
                        _erpOptions.MaxRetryCount,
                        retryAttempt => TimeSpan.FromMilliseconds(_erpOptions.RetryDelayMilliseconds * Math.Pow(2, retryAttempt - 1)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.LogWarning("Retry {RetryCount} of {MaxRetryCount} after {RetryDelay}ms due to: {ErrorMessage}",
                                retryCount, _erpOptions.MaxRetryCount, timeSpan.TotalMilliseconds, exception.Message);
                        }))
                .WithTimeout(TimeSpan.FromSeconds(_erpOptions.ConnectionTimeoutSeconds));

            _accessToken = null;
            _tokenExpiryTime = DateTime.MinValue;
            _confidentialClient = null;
            _baseApiUrl = $"{_dynamicsOptions.OrganizationUrl}/api/data/{_dynamicsOptions.ApiVersion}/";
        }

        /// <summary>
        /// Establishes a connection to Dynamics 365 by authenticating with Azure AD and obtaining an access token.
        /// </summary>
        /// <returns>Result indicating success or failure of the connection attempt.</returns>
        public override async Task<Result> ConnectAsync()
        {
            try
            {
                _logger.LogInformation("Connecting to Dynamics 365...");

                // Initialize the MSAL client if not already initialized
                if (_confidentialClient == null)
                {
                    _confidentialClient = InitializeConfidentialClient();
                }

                // Acquire token for Dynamics 365 API
                var scopes = new[] { $"{_dynamicsOptions.OrganizationUrl}/.default" };
                var authResult = await _confidentialClient.AcquireTokenForClient(scopes).ExecuteAsync();

                _accessToken = authResult.AccessToken;
                _tokenExpiryTime = authResult.ExpiresOn.DateTime;
                IsConnected = true;

                _logger.LogInformation("Successfully connected to Dynamics 365. Token expires at: {ExpiryTime}", _tokenExpiryTime);
                return Result.Success();
            }
            catch (MsalException msalEx)
            {
                LogError(msalEx, "ConnectAsync", "Failed to acquire token from Azure AD");
                return Result.Failure($"Failed to authenticate with Dynamics 365: {msalEx.Message}");
            }
            catch (Exception ex)
            {
                LogError(ex, "ConnectAsync", "Failed to connect to Dynamics 365");
                return Result.Failure($"Failed to connect to Dynamics 365: {ex.Message}");
            }
        }

        /// <summary>
        /// Closes the connection to Dynamics 365 by clearing the access token.
        /// </summary>
        /// <returns>Result indicating success or failure of the disconnection attempt.</returns>
        public override async Task<Result> DisconnectAsync()
        {
            try
            {
                _logger.LogInformation("Disconnecting from Dynamics 365...");

                // Clear the access token and related authentication state
                _accessToken = null;
                _tokenExpiryTime = DateTime.MinValue;
                _confidentialClient = null;
                IsConnected = false;

                _logger.LogInformation("Successfully disconnected from Dynamics 365");
                return Result.Success();
            }
            catch (Exception ex)
            {
                LogError(ex, "DisconnectAsync", "Failed to disconnect from Dynamics 365");
                return Result.Failure($"Failed to disconnect from Dynamics 365: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests the connection to Dynamics 365 using the provided integration configuration.
        /// </summary>
        /// <param name="integration">Integration configuration to use for the test.</param>
        /// <returns>Result indicating success or failure of the connection test.</returns>
        public override async Task<Result> TestConnectionAsync(Integration integration)
        {
            if (integration == null)
                throw new ArgumentNullException(nameof(integration));

            try
            {
                _logger.LogInformation("Testing connection to Dynamics 365 with integration: {IntegrationId}", integration.IntegrationId);

                // Extract connection details from integration entity
                string clientId = integration.GetSetting("ClientId");
                string clientSecret = integration.ApiKey; // Assuming ApiKey stores the client secret
                string organizationUrl = integration.ApiEndpoint; // Assuming ApiEndpoint stores the org URL
                string tenantId = integration.GetSetting("TenantId");

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) ||
                    string.IsNullOrEmpty(organizationUrl) || string.IsNullOrEmpty(tenantId))
                {
                    return Result.Failure("Missing required Dynamics 365 connection parameters in integration configuration");
                }

                // Try to authenticate with extracted credentials
                var confidentialClientApp = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                    .WithClientSecret(clientSecret)
                    .Build();

                var scopes = new[] { $"{organizationUrl}/.default" };
                var authResult = await confidentialClientApp.AcquireTokenForClient(scopes).ExecuteAsync();

                // Make a simple API request to verify connectivity
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = await httpClient.GetAsync($"{organizationUrl}/api/data/v9.2/WhoAmI");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Connection test to Dynamics 365 successful");
                    return Result.Success();
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Connection test to Dynamics 365 failed. Status code: {StatusCode}, Response: {Response}",
                        response.StatusCode, content);
                    return Result.Failure($"Connection test failed with status code {response.StatusCode}: {content}");
                }
            }
            catch (MsalException msalEx)
            {
                LogError(msalEx, "TestConnectionAsync", $"Failed to acquire token for integration ID: {integration.IntegrationId}");
                return Result.Failure($"Failed to authenticate with Dynamics 365: {msalEx.Message}");
            }
            catch (HttpRequestException httpEx)
            {
                LogError(httpEx, "TestConnectionAsync", $"HTTP request failed for integration ID: {integration.IntegrationId}");
                return Result.Failure($"Failed to connect to Dynamics 365 API: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                LogError(ex, "TestConnectionAsync", $"Integration ID: {integration.IntegrationId}");
                return Result.Failure($"Failed to test connection to Dynamics 365: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves transaction data from Dynamics 365 based on specified parameters.
        /// </summary>
        /// <param name="startDate">The start date for the data to retrieve.</param>
        /// <param name="endDate">The end date for the data to retrieve.</param>
        /// <param name="entityType">The entity type to retrieve (e.g., invoices, transactions).</param>
        /// <param name="maxRecords">The maximum number of records to retrieve.</param>
        /// <returns>A result containing the retrieved transaction data or error information.</returns>
        public override async Task<Result<TransactionData>> GetTransactionDataAsync(
            DateTime startDate,
            DateTime endDate,
            string entityType,
            int? maxRecords)
        {
            // Validate parameters
            var validationResult = ValidateParameters(startDate, endDate);
            if (!validationResult.IsSuccess)
            {
                return Result<TransactionData>.Failure(validationResult.ErrorMessage, validationResult.ErrorCode);
            }

            try
            {
                _logger.LogInformation("Retrieving transaction data from Dynamics 365. EntityType: {EntityType}, StartDate: {StartDate}, EndDate: {EndDate}",
                    entityType, startDate, endDate);

                // Check if connected, if not try to connect
                if (!IsConnected)
                {
                    var connectResult = await ConnectAsync();
                    if (!connectResult.IsSuccess)
                    {
                        return Result<TransactionData>.Failure(connectResult.ErrorMessage, connectResult.ErrorCode);
                    }
                }

                // Ensure we have a valid token
                var tokenResult = await EnsureValidTokenAsync();
                if (!tokenResult.IsSuccess)
                {
                    return Result<TransactionData>.Failure(tokenResult.ErrorMessage, tokenResult.ErrorCode);
                }

                // Determine which entity type to use
                string actualEntityType = entityType;
                if (string.IsNullOrEmpty(entityType))
                {
                    actualEntityType = _dynamicsOptions.DefaultEntityType;
                    _logger.LogInformation("No entity type specified, using default: {DefaultEntityType}", actualEntityType);
                }
                else if (_dynamicsOptions.Entities.TryGetValue(entityType, out var mappedEntityType))
                {
                    actualEntityType = mappedEntityType;
                    _logger.LogInformation("Mapped entity type {EntityType} to {MappedEntityType}", entityType, actualEntityType);
                }

                // Build the OData query
                string query = BuildODataQuery(actualEntityType, startDate, endDate, maxRecords);
                string fullUrl = _baseApiUrl + query;
                _logger.LogInformation("Executing Dynamics 365 query: {Query}", fullUrl);

                // Create a dictionary for custom request headers
                var customHeaders = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {_accessToken}" },
                    { "OData-MaxVersion", "4.0" },
                    { "OData-Version", "4.0" },
                    { "Prefer", "odata.include-annotations=\"*\"" }
                };

                // Use the ApiClient for the request to benefit from retry policies
                var response = await _apiClient.GetAsync<JsonDocument>(fullUrl, customHeaders);
                
                if (!response.IsSuccess)
                {
                    return Result<TransactionData>.Failure(response.ErrorMessage, response.ErrorCode);
                }

                // Process the response data
                var transactionData = await ProcessDynamicsResponseAsync(response.Value.RootElement, actualEntityType);
                
                _logger.LogInformation("Successfully retrieved {RecordCount} records from Dynamics 365", 
                    transactionData.Records.Count);
                
                return Result<TransactionData>.Success(transactionData);
            }
            catch (JsonException jsonEx)
            {
                LogError(jsonEx, "GetTransactionDataAsync", "Failed to parse JSON response from Dynamics 365");
                return Result<TransactionData>.Failure($"Failed to process Dynamics 365 response: {jsonEx.Message}");
            }
            catch (HttpRequestException httpEx)
            {
                LogError(httpEx, "GetTransactionDataAsync", "HTTP request to Dynamics 365 failed");
                return Result<TransactionData>.Failure($"Failed to connect to Dynamics 365 API: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                LogError(ex, "GetTransactionDataAsync", $"EntityType: {entityType}, StartDate: {startDate}, EndDate: {endDate}");
                return Result<TransactionData>.Failure($"Failed to retrieve transaction data from Dynamics 365: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures that the access token is valid, refreshing it if necessary.
        /// </summary>
        /// <returns>Result indicating success or failure of the token validation/refresh.</returns>
        private async Task<Result> EnsureValidTokenAsync()
        {
            // Check if we have a valid token that's not about to expire
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiryTime > DateTime.UtcNow.AddMinutes(5))
            {
                return Result.Success();
            }

            // Token is missing or about to expire, refresh it
            _logger.LogInformation("Access token is missing or about to expire, refreshing...");
            return await ConnectAsync();
        }

        /// <summary>
        /// Builds an OData query string for retrieving data from Dynamics 365.
        /// </summary>
        /// <param name="entityType">The entity type to query.</param>
        /// <param name="startDate">The start date for filtering records.</param>
        /// <param name="endDate">The end date for filtering records.</param>
        /// <param name="maxRecords">The maximum number of records to retrieve.</param>
        /// <returns>The constructed OData query URL.</returns>
        private string BuildODataQuery(string entityType, DateTime startDate, DateTime endDate, int? maxRecords)
        {
            // Start with base entity set URL
            var queryBuilder = new System.Text.StringBuilder($"{entityType}s");

            // Add query parameters
            queryBuilder.Append("?$filter=");
            
            // Add date range filter - the field names might need adjustment based on actual Dynamics entity structure
            string dateField = entityType == "invoice" ? "createdon" : "transactiondate";
            string startDateStr = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            string endDateStr = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            
            queryBuilder.Append($"{dateField} ge {startDateStr} and {dateField} le {endDateStr}");
            
            // Add select statement for required fields
            queryBuilder.Append("&$select=");
            
            // The selected fields will vary based on entity type
            if (entityType == "invoice")
            {
                queryBuilder.Append("invoiceid,invoicenumber,createdon,totalamount,totalamountlessfreight,transactioncurrencyid,customerid");
            }
            else if (entityType == "salesorder")
            {
                queryBuilder.Append("salesorderid,ordernumber,createdon,totalamount,totalamountlessfreight,transactioncurrencyid,customerid");
            }
            else
            {
                // Generic fields that should work for most transaction entities
                queryBuilder.Append("activityid,subject,createdon,totalamount,transactioncurrencyid");
            }
            
            // Add orderby for consistent results
            queryBuilder.Append($"&$orderby={dateField} desc");
            
            // Add top parameter if maxRecords is specified
            if (maxRecords.HasValue && maxRecords.Value > 0)
            {
                queryBuilder.Append($"&$top={maxRecords.Value}");
            }

            // Add batch requests parameter if configured
            if (_dynamicsOptions.UseBatchRequests)
            {
                queryBuilder.Append($"&batchSize={_dynamicsOptions.BatchSize}");
            }
            
            return queryBuilder.ToString();
        }

        /// <summary>
        /// Processes the response from Dynamics 365 API and converts it to TransactionData.
        /// </summary>
        /// <param name="responseData">The JSON response data from Dynamics 365.</param>
        /// <param name="entityType">The entity type that was queried.</param>
        /// <returns>The processed transaction data.</returns>
        private async Task<TransactionData> ProcessDynamicsResponseAsync(JsonElement responseData, string entityType)
        {
            var transactionData = new TransactionData
            {
                EntityType = entityType,
                RetrievalDate = DateTime.UtcNow
            };

            // Extract the value array from the response
            if (responseData.TryGetProperty("value", out JsonElement valueArray))
            {
                foreach (JsonElement record in valueArray.EnumerateArray())
                {
                    var transactionRecord = MapDynamicsRecord(record, entityType);
                    transactionData.Records.Add(transactionRecord);
                }
                
                transactionData.TotalCount = transactionData.Records.Count;
                
                // Check if there's a next link (paging)
                if (responseData.TryGetProperty("@odata.nextLink", out JsonElement nextLink))
                {
                    _logger.LogInformation("More records available via pagination link: {NextLink}", nextLink.GetString());
                    // Note: In a real implementation, you might want to follow this link to get all records
                    // But for this implementation, we'll just acknowledge it exists
                }
            }
            else
            {
                _logger.LogWarning("No 'value' property found in Dynamics 365 response");
                transactionData.TotalCount = 0;
            }

            return transactionData;
        }

        /// <summary>
        /// Maps a Dynamics 365 record to a TransactionRecord object.
        /// </summary>
        /// <param name="record">The JSON record from Dynamics 365.</param>
        /// <param name="entityType">The entity type of the record.</param>
        /// <returns>The mapped transaction record.</returns>
        private TransactionRecord MapDynamicsRecord(JsonElement record, string entityType)
        {
            var transactionRecord = new TransactionRecord();
            
            try
            {
                // Extract ID field based on entity type
                if (entityType == "invoice" && record.TryGetProperty("invoiceid", out JsonElement invoiceId))
                {
                    transactionRecord.Id = invoiceId.GetString();
                }
                else if (entityType == "salesorder" && record.TryGetProperty("salesorderid", out JsonElement salesOrderId))
                {
                    transactionRecord.Id = salesOrderId.GetString();
                }
                else if (record.TryGetProperty("activityid", out JsonElement activityId))
                {
                    transactionRecord.Id = activityId.GetString();
                }
                
                // Extract transaction number/reference
                if (entityType == "invoice" && record.TryGetProperty("invoicenumber", out JsonElement invoiceNumber))
                {
                    transactionRecord.TransactionNumber = invoiceNumber.GetString();
                }
                else if (entityType == "salesorder" && record.TryGetProperty("ordernumber", out JsonElement orderNumber))
                {
                    transactionRecord.TransactionNumber = orderNumber.GetString();
                }
                else if (record.TryGetProperty("subject", out JsonElement subject))
                {
                    transactionRecord.TransactionNumber = subject.GetString();
                }
                
                // Extract transaction date
                if (record.TryGetProperty("createdon", out JsonElement createdOn))
                {
                    transactionRecord.TransactionDate = createdOn.GetDateTime();
                }
                else if (record.TryGetProperty("transactiondate", out JsonElement transactionDate))
                {
                    transactionRecord.TransactionDate = transactionDate.GetDateTime();
                }
                
                // Extract amount and tax amount
                if (record.TryGetProperty("totalamount", out JsonElement totalAmount))
                {
                    transactionRecord.Amount = totalAmount.GetDecimal();
                }
                
                if (record.TryGetProperty("totaltax", out JsonElement totalTax))
                {
                    transactionRecord.TaxAmount = totalTax.GetDecimal();
                }
                // If totaltax is not available, estimate it from the difference between totalamount and totalamountlessfreight
                else if (transactionRecord.Amount > 0 && 
                         record.TryGetProperty("totalamountlessfreight", out JsonElement totalAmountLessFreight))
                {
                    decimal amountLessFreight = totalAmountLessFreight.GetDecimal();
                    transactionRecord.TaxAmount = transactionRecord.Amount - amountLessFreight;
                }
                
                // Extract currency code
                if (record.TryGetProperty("transactioncurrencyid", out JsonElement currencyElement) &&
                    currencyElement.TryGetProperty("name", out JsonElement currencyName))
                {
                    transactionRecord.CurrencyCode = currencyName.GetString();
                }
                else
                {
                    // Default to organization base currency if not specified
                    transactionRecord.CurrencyCode = "USD";
                }
                
                // Extract country code - this might be derived from customer address in a real implementation
                if (record.TryGetProperty("customerid", out JsonElement customerId) &&
                    customerId.TryGetProperty("address1_country", out JsonElement country))
                {
                    transactionRecord.CountryCode = country.GetString();
                }
                else
                {
                    // Default to empty string if not found
                    transactionRecord.CountryCode = "";
                }
                
                // Set transaction type based on entity type
                transactionRecord.TransactionType = entityType;
                
                // Store any additional fields in the AdditionalData dictionary
                foreach (JsonProperty property in record.EnumerateObject())
                {
                    if (!transactionRecord.AdditionalData.ContainsKey(property.Name))
                    {
                        // For complex properties, we'll just store the raw JSON string
                        if (property.Value.ValueKind == JsonValueKind.Object || 
                            property.Value.ValueKind == JsonValueKind.Array)
                        {
                            transactionRecord.AdditionalData[property.Name] = property.Value.ToString();
                        }
                        else if (property.Value.ValueKind != JsonValueKind.Null)
                        {
                            // For simple properties, store the actual value
                            switch (property.Value.ValueKind)
                            {
                                case JsonValueKind.String:
                                    transactionRecord.AdditionalData[property.Name] = property.Value.GetString();
                                    break;
                                case JsonValueKind.Number:
                                    if (property.Value.TryGetDecimal(out decimal decimalValue))
                                        transactionRecord.AdditionalData[property.Name] = decimalValue;
                                    break;
                                case JsonValueKind.True:
                                case JsonValueKind.False:
                                    transactionRecord.AdditionalData[property.Name] = property.Value.GetBoolean();
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error mapping Dynamics record: {ErrorMessage}", ex.Message);
                // Continue with partial data rather than failing completely
            }
            
            return transactionRecord;
        }

        /// <summary>
        /// Initializes the MSAL confidential client application for authentication.
        /// </summary>
        /// <returns>The initialized confidential client application.</returns>
        private IConfidentialClientApplication InitializeConfidentialClient()
        {
            // Create a confidential client application using the Microsoft Authentication Library (MSAL)
            var app = ConfidentialClientApplicationBuilder
                .Create(_erpOptions.ClientId)
                .WithAuthority($"{_erpOptions.AuthorityUrl}{_erpOptions.TenantId}")
                .WithClientSecret(_erpOptions.ClientSecret)
                .Build();
            
            return app;
        }
    }
}