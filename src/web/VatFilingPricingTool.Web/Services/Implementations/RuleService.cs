using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Clients;

namespace VatFilingPricingTool.Web.Services.Implementations
{
    /// <summary>
    /// Implementation of the IRuleService interface that provides functionality for managing
    /// VAT filing pricing rules in the web application.
    /// </summary>
    public class RuleService : IRuleService
    {
        private readonly ApiClient apiClient;
        private readonly ILogger<RuleService> logger;

        /// <summary>
        /// Initializes a new instance of the RuleService class with required dependencies.
        /// </summary>
        /// <param name="apiClient">The API client for communicating with the backend API.</param>
        /// <param name="logger">The logger for diagnostic information.</param>
        public RuleService(ApiClient apiClient, ILogger<RuleService> logger)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a specific rule by its unique identifier.
        /// </summary>
        /// <param name="ruleId">The unique identifier of the rule to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, containing the retrieved rule.</returns>
        public async Task<RuleModel> GetRuleByIdAsync(string ruleId)
        {
            try
            {
                logger.LogInformation("Getting rule with ID: {RuleId}", ruleId);
                var endpoint = $"{ApiEndpoints.Admin.Rules}/{ruleId}";
                return await apiClient.GetAsync<RuleModel>(endpoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting rule with ID {RuleId}: {Message}", ruleId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a paginated list of rules with optional filtering.
        /// </summary>
        /// <param name="filter">Filter criteria for the rules.</param>
        /// <returns>A task representing the asynchronous operation, containing the paginated list of rules.</returns>
        public async Task<RuleListModel> GetRulesAsync(RuleFilterModel filter)
        {
            try
            {
                logger.LogInformation("Getting rules with filter: CountryCode={CountryCode}, RuleType={RuleType}, ActiveOnly={ActiveOnly}, PageNumber={PageNumber}, PageSize={PageSize}",
                    filter.CountryCode, filter.RuleType, filter.ActiveOnly, filter.PageNumber, filter.PageSize);
                
                var endpoint = ApiEndpoints.Admin.Rules;
                
                // Add query parameters
                var queryParams = new List<string>();
                
                if (!string.IsNullOrEmpty(filter.CountryCode))
                    queryParams.Add($"countryCode={Uri.EscapeDataString(filter.CountryCode)}");
                    
                if (filter.RuleType.HasValue)
                    queryParams.Add($"ruleType={Uri.EscapeDataString(filter.RuleType.ToString())}");
                    
                queryParams.Add($"activeOnly={filter.ActiveOnly}");
                
                if (filter.EffectiveDate.HasValue)
                    queryParams.Add($"effectiveDate={Uri.EscapeDataString(filter.EffectiveDate.Value.ToString("o"))}");
                    
                queryParams.Add($"pageNumber={filter.PageNumber}");
                queryParams.Add($"pageSize={filter.PageSize}");
                
                if (queryParams.Count > 0)
                    endpoint += "?" + string.Join("&", queryParams);
                
                return await apiClient.GetAsync<RuleListModel>(endpoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting rules: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a simplified list of rules for dropdown selection components.
        /// </summary>
        /// <param name="countryCode">Optional country code to filter rules by country.</param>
        /// <param name="ruleType">Optional rule type to filter rules by type.</param>
        /// <param name="activeOnly">When true, returns only active rules.</param>
        /// <returns>A task representing the asynchronous operation, containing the list of rule summaries.</returns>
        public async Task<List<RuleSummaryModel>> GetRuleSummariesAsync(string countryCode, RuleType? ruleType, bool activeOnly)
        {
            try
            {
                logger.LogInformation("Getting rule summaries for CountryCode={CountryCode}, RuleType={RuleType}, ActiveOnly={ActiveOnly}",
                    countryCode, ruleType, activeOnly);
                
                var endpoint = $"{ApiEndpoints.Admin.Rules}/summaries";
                
                // Add query parameters
                var queryParams = new List<string>();
                
                if (!string.IsNullOrEmpty(countryCode))
                    queryParams.Add($"countryCode={Uri.EscapeDataString(countryCode)}");
                    
                if (ruleType.HasValue)
                    queryParams.Add($"ruleType={Uri.EscapeDataString(ruleType.ToString())}");
                    
                queryParams.Add($"activeOnly={activeOnly}");
                
                if (queryParams.Count > 0)
                    endpoint += "?" + string.Join("&", queryParams);
                
                return await apiClient.GetAsync<List<RuleSummaryModel>>(endpoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting rule summaries: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates a new VAT filing pricing rule.
        /// </summary>
        /// <param name="rule">The rule to create.</param>
        /// <returns>A task representing the asynchronous operation, containing the created rule.</returns>
        public async Task<RuleModel> CreateRuleAsync(CreateRuleModel rule)
        {
            try
            {
                logger.LogInformation("Creating rule: {RuleName} for country {CountryCode}, type {RuleType}",
                    rule.Name, rule.CountryCode, rule.RuleType);
                
                var endpoint = ApiEndpoints.Admin.Rules;
                return await apiClient.PostAsync<CreateRuleModel, RuleModel>(endpoint, rule);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating rule: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing VAT filing pricing rule.
        /// </summary>
        /// <param name="rule">The rule with updated values.</param>
        /// <returns>A task representing the asynchronous operation, containing the updated rule.</returns>
        public async Task<RuleModel> UpdateRuleAsync(UpdateRuleModel rule)
        {
            try
            {
                logger.LogInformation("Updating rule: {RuleId}, {RuleName}",
                    rule.RuleId, rule.Name);
                
                var endpoint = $"{ApiEndpoints.Admin.Rules}/{rule.RuleId}";
                return await apiClient.PutAsync<UpdateRuleModel, RuleModel>(endpoint, rule);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating rule {RuleId}: {Message}", rule.RuleId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Deletes a VAT filing pricing rule.
        /// </summary>
        /// <param name="ruleId">The unique identifier of the rule to delete.</param>
        /// <returns>A task representing the asynchronous operation, containing a boolean indicating success.</returns>
        public async Task<bool> DeleteRuleAsync(string ruleId)
        {
            try
            {
                logger.LogInformation("Deleting rule with ID: {RuleId}", ruleId);
                
                var endpoint = $"{ApiEndpoints.Admin.Rules}/{ruleId}";
                return await apiClient.DeleteAsync<bool>(endpoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting rule {RuleId}: {Message}", ruleId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validates a rule expression for syntax and evaluates it with sample data.
        /// </summary>
        /// <param name="model">The validation model containing the expression, parameters, and sample values.</param>
        /// <returns>A task representing the asynchronous operation, containing the validation result.</returns>
        public async Task<ValidateRuleExpressionResultModel> ValidateRuleExpressionAsync(ValidateRuleExpressionModel model)
        {
            try
            {
                logger.LogInformation("Validating rule expression: {Expression}", model.Expression);
                
                var endpoint = $"{ApiEndpoints.Admin.Rules}/validate";
                return await apiClient.PostAsync<ValidateRuleExpressionModel, ValidateRuleExpressionResultModel>(endpoint, model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating rule expression: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Imports multiple rules from an uploaded file (JSON, CSV, Excel).
        /// </summary>
        /// <param name="countryCode">The country code for the imported rules.</param>
        /// <param name="fileContent">The binary content of the uploaded file.</param>
        /// <param name="fileName">The name of the uploaded file.</param>
        /// <returns>A task representing the asynchronous operation, containing the number of successfully imported rules.</returns>
        public async Task<int> ImportRulesAsync(string countryCode, byte[] fileContent, string fileName)
        {
            try
            {
                logger.LogInformation("Importing rules for country {CountryCode} from file {FileName}, size: {FileSize} bytes",
                    countryCode, fileName, fileContent.Length);
                
                var endpoint = $"{ApiEndpoints.Admin.Rules}/import";
                
                // For file uploads, we need to use MultipartFormDataContent
                using var content = new MultipartFormDataContent();
                
                // Add the file content
                var fileContentData = new ByteArrayContent(fileContent);
                content.Add(fileContentData, "file", fileName);
                
                // Add the country code
                content.Add(new StringContent(countryCode), "countryCode");
                
                // Create a HttpClient to handle multipart form data
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ImportRulesResultModel>();
                    return result.ImportedRules;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogError("Error importing rules: HTTP {StatusCode}, {ErrorContent}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Error importing rules: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing rules: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Exports rules to a file format (JSON, CSV, Excel) for download.
        /// </summary>
        /// <param name="countryCode">Optional country code to filter rules by country.</param>
        /// <param name="ruleType">Optional rule type to filter rules by type.</param>
        /// <param name="activeOnly">When true, exports only active rules.</param>
        /// <param name="format">The export file format (json, csv, excel).</param>
        /// <returns>A task representing the asynchronous operation, containing the file content as a byte array.</returns>
        public async Task<byte[]> ExportRulesAsync(string countryCode, RuleType? ruleType, bool activeOnly, string format)
        {
            try
            {
                logger.LogInformation("Exporting rules for CountryCode={CountryCode}, RuleType={RuleType}, ActiveOnly={ActiveOnly}, Format={Format}",
                    countryCode, ruleType, activeOnly, format);
                
                var endpoint = $"{ApiEndpoints.Admin.Rules}/export";
                
                // Add query parameters
                var queryParams = new List<string>();
                
                if (!string.IsNullOrEmpty(countryCode))
                    queryParams.Add($"countryCode={Uri.EscapeDataString(countryCode)}");
                    
                if (ruleType.HasValue)
                    queryParams.Add($"ruleType={Uri.EscapeDataString(ruleType.ToString())}");
                    
                queryParams.Add($"activeOnly={activeOnly}");
                queryParams.Add($"format={Uri.EscapeDataString(format)}");
                
                if (queryParams.Count > 0)
                    endpoint += "?" + string.Join("&", queryParams);
                
                // Use HttpClient directly to handle binary response
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogError("Error exporting rules: HTTP {StatusCode}, {ErrorContent}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Error exporting rules: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error exporting rules: {Message}", ex.Message);
                throw;
            }
        }
    }
}