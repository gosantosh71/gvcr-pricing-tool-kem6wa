using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Net.Http; // System.Net.Http package version 6.0.0
using System.Net.Http.Json; // System.Net.Http.Json package version 6.0.0
using System.Text.Json; // System.Text.Json package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using Xunit; // xunit package version 2.4.1
using FluentAssertions; // FluentAssertions package version 6.2.0
using Bogus; // Bogus package version 34.0.2
using VatFilingPricingTool.Common.Models.ApiResponse; // Import for ApiResponse class
using VatFilingPricingTool.Common.Models.Result; // Import for Result class
using VatFilingPricingTool.Domain.Enums; // Import for UserRole enum
using VatFilingPricingTool.IntegrationTests.TestServer; // Import for CustomWebApplicationFactory

namespace VatFilingPricingTool.IntegrationTests.Utilities
{
    /// <summary>
    /// Provides utility methods and helper functions for integration tests in the VAT Filing Pricing Tool,
    /// simplifying common testing operations such as API request preparation, response handling, and test data generation.
    /// </summary>
    public static class IntegrationTestHelpers
    {
        /// <summary>
        /// API version used in the tests
        /// </summary>
        private const string API_VERSION = "v1";

        /// <summary>
        /// Creates an HTTP client with authentication for a specific user role
        /// </summary>
        /// <param name="factory">The custom web application factory</param>
        /// <param name="role">The user role to authenticate with</param>
        /// <returns>An authenticated HTTP client</returns>
        public static HttpClient CreateAuthenticatedClient(CustomWebApplicationFactory factory, UserRole role)
        {
            // Call factory.CreateClient with the specified role
            var client = factory.CreateClient(role.ToString());

            // Configure default headers for the client
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // Return the configured client
            return client;
        }

        /// <summary>
        /// Asserts that an API response is successful and contains valid data
        /// </summary>
        /// <typeparam name="T">The type of the data in the response</typeparam>
        /// <param name="response">The API response to assert</param>
        /// <returns>The data from the successful response</returns>
        public static T AssertSuccessResponse<T>(ApiResponse<T> response)
        {
            // Assert that response is not null
            Assert.NotNull(response);

            // Assert that response.Success is true
            Assert.True(response.Success);

            // Assert that response.Data is not null
            Assert.NotNull(response.Data);

            // Return response.Data
            return response.Data;
        }

        /// <summary>
        /// Asserts that an API response is an error response
        /// </summary>
        /// <typeparam name="T">The type of the data in the response</typeparam>
        /// <param name="response">The API response to assert</param>
        /// <param name="expectedErrorCode">The expected error code (optional)</param>
        public static void AssertErrorResponse<T>(ApiResponse<T> response, string expectedErrorCode = null)
        {
            // Assert that response is not null
            Assert.NotNull(response);

            // Assert that response.Success is false
            Assert.False(response.Success);

            // If expectedErrorCode is provided, assert that response.ErrorCode equals expectedErrorCode
            if (expectedErrorCode != null)
            {
                Assert.Equal(expectedErrorCode, response.ErrorCode);
            }
        }

        /// <summary>
        /// Generates a test country model for integration tests
        /// </summary>
        /// <param name="countryCode">The country code (optional)</param>
        /// <param name="name">The country name (optional)</param>
        /// <param name="vatRate">The VAT rate (optional, default is 20.0m)</param>
        /// <returns>A test country model</returns>
        public static Contracts.V1.Models.CountryModel GenerateTestCountry(string countryCode = null, string name = null, decimal vatRate = 20.0m)
        {
            // Create a new CountryModel instance
            var country = new Contracts.V1.Models.CountryModel();

            // Set CountryCode to provided value or generate a random 2-letter code
            country.CountryCode = countryCode ?? TestDataGenerator.Faker.Address.CountryCode();

            // Set Name to provided value or generate a random country name
            country.Name = name ?? TestDataGenerator.Faker.Address.Country();

            // Set StandardVatRate to provided vatRate
            country.StandardVatRate = vatRate;

            // Set CurrencyCode to 'EUR'
            country.CurrencyCode = "EUR";

            // Set IsActive to true
            country.IsActive = true;

            // Return the country model
            return country;
        }

        /// <summary>
        /// Generates a test rule model for integration tests
        /// </summary>
        /// <param name="countryCode">The country code</param>
        /// <param name="ruleType">The rule type</param>
        /// <param name="expression">The rule expression</param>
        /// <returns>A test rule model</returns>
        public static Contracts.V1.Models.RuleModel GenerateTestRule(string countryCode, RuleType ruleType, string expression)
        {
            // Create a new RuleModel instance
            var rule = new Contracts.V1.Models.RuleModel();

            // Set RuleId to a new GUID string
            rule.RuleId = Guid.NewGuid().ToString();

            // Set CountryCode to provided countryCode
            rule.CountryCode = countryCode;

            // Set RuleType to provided ruleType
            rule.RuleType = ruleType;

            // Set Name to a generated name based on ruleType
            rule.Name = $"{ruleType} Rule for {countryCode}";

            // Set Expression to provided expression
            rule.Expression = expression;

            // Set EffectiveFrom to current date
            rule.EffectiveFrom = DateTime.UtcNow;

            // Set EffectiveTo to null (no end date)
            rule.EffectiveTo = null;

            // Set Priority to 100
            rule.Priority = 100;

            // Set IsActive to true
            rule.IsActive = true;

            // Return the rule model
            return rule;
        }

        /// <summary>
        /// Generates a test calculation request for integration tests
        /// </summary>
        /// <param name="countryCodes">The list of country codes</param>
        /// <param name="serviceType">The service type</param>
        /// <param name="transactionVolume">The transaction volume (optional, default is 500)</param>
        /// <param name="filingFrequency">The filing frequency (optional, default is FilingFrequency.Monthly)</param>
        /// <returns>A test calculation request</returns>
        public static Contracts.V1.Models.CalculationRequest GenerateTestCalculationRequest(List<string> countryCodes, ServiceType serviceType, int transactionVolume = 500, FilingFrequency filingFrequency = FilingFrequency.Monthly)
        {
            // Create a new CalculationRequest instance
            var request = new Contracts.V1.Models.CalculationRequest();

            // Set CountryCodes to provided countryCodes
            request.CountryCodes = countryCodes;

            // Set ServiceType to provided serviceType
            request.ServiceType = serviceType;

            // Set TransactionVolume to provided transactionVolume
            request.TransactionVolume = transactionVolume;

            // Set FilingFrequency to provided filingFrequency
            request.Frequency = filingFrequency;

            // Set AdditionalServices to an empty list
            request.AdditionalServices = new List<string>();

            // Return the calculation request
            return request;
        }

        /// <summary>
        /// Generates a test user model for integration tests
        /// </summary>
        /// <param name="role">The user role (optional, default is UserRole.Customer)</param>
        /// <param name="email">The email address (optional)</param>
        /// <returns>A test user model</returns>
        public static Contracts.V1.Models.UserModel GenerateTestUser(UserRole role = UserRole.Customer, string email = null)
        {
            // Create a new UserModel instance
            var user = new Contracts.V1.Models.UserModel();

            // Set UserId to a new GUID string
            user.UserId = Guid.NewGuid().ToString();

            // Set Email to provided email or generate a random email
            user.Email = email ?? TestDataGenerator.Faker.Internet.Email();

            // Set FirstName to a random first name
            user.FirstName = TestDataGenerator.Faker.Name.FirstName();

            // Set LastName to a random last name
            user.LastName = TestDataGenerator.Faker.Name.LastName();

            // Set Roles to a list containing the provided role
            user.Roles = new List<UserRole> { role };

            // Set IsActive to true
            user.IsActive = true;

            // Return the user model
            return user;
        }

        /// <summary>
        /// Constructs an API URL with the correct version prefix
        /// </summary>
        /// <param name="endpoint">The API endpoint</param>
        /// <returns>The full API URL</returns>
        public static string GetApiUrl(string endpoint)
        {
            // Ensure endpoint starts with a forward slash
            if (!endpoint.StartsWith("/"))
            {
                endpoint = "/" + endpoint;
            }

            // Return $"/api/{API_VERSION}{endpoint}"
            return $"/api/{API_VERSION}{endpoint}";
        }

        /// <summary>
        /// Serializes an object to JSON with consistent options
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <returns>The JSON string representation</returns>
        public static string SerializeJson<T>(T obj)
        {
            // Create JsonSerializerOptions with camelCase naming policy
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Serialize the object to JSON using System.Text.Json
            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an object with consistent options
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized object</returns>
        public static T DeserializeJson<T>(string json)
        {
            // Create JsonSerializerOptions with camelCase naming policy
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Deserialize the JSON string to type T using System.Text.Json
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Provides methods for generating test data for integration tests
        /// </summary>
        public static class TestDataGenerator
        {
            /// <summary>
            /// Faker instance for generating fake data
            /// </summary>
            public static Faker Faker { get; private set; }

            /// <summary>
            /// Static constructor that initializes the Faker instance
            /// </summary>
            static TestDataGenerator()
            {
                // Initialize Faker with a fixed seed for reproducible tests
                Faker = new Faker().UseSeed(12345);
            }

            /// <summary>
            /// Generates a list of test countries
            /// </summary>
            /// <param name="count">The number of countries to generate (optional, default is 3)</param>
            /// <returns>A list of test country models</returns>
            public static List<Contracts.V1.Models.CountryModel> GenerateCountries(int count = 3)
            {
                // Create a new list of CountryModel
                var countries = new List<Contracts.V1.Models.CountryModel>();

                // Generate 'count' number of countries using GenerateTestCountry
                for (int i = 0; i < count; i++)
                {
                    countries.Add(GenerateTestCountry());
                }

                // Return the list of countries
                return countries;
            }

            /// <summary>
            /// Generates a list of test rules for a country
            /// </summary>
            /// <param name="countryCode">The country code</param>
            /// <param name="count">The number of rules to generate (optional, default is 3)</param>
            /// <returns>A list of test rule models</returns>
            public static List<Contracts.V1.Models.RuleModel> GenerateRules(string countryCode)
            {
                // Create a new list of RuleModel
                var rules = new List<Contracts.V1.Models.RuleModel>();

                // Generate 'count' number of rules using GenerateTestRule
                rules.Add(GenerateTestRule(countryCode, RuleType.VatRate, "basePrice * 0.20"));
                rules.Add(GenerateTestRule(countryCode, RuleType.Threshold, "basePrice * (transactionVolume > 1000 ? 0.9 : 1)"));
                rules.Add(GenerateTestRule(countryCode, RuleType.Complexity, "basePrice * (serviceType == \"ComplexFiling\" ? 1.2 : 1)"));

                // Return the list of rules
                return rules;
            }

            /// <summary>
            /// Generates a list of test users with different roles
            /// </summary>
            /// <param name="count">The number of users to generate (optional, default is 3)</param>
            /// <returns>A list of test user models</returns>
            public static List<Contracts.V1.Models.UserModel> GenerateUsers()
            {
                // Create a new list of UserModel
                var users = new List<Contracts.V1.Models.UserModel>();

                // Generate 'count' number of users using GenerateTestUser
                users.Add(GenerateTestUser(UserRole.Administrator, "admin@example.com"));
                users.Add(GenerateTestUser(UserRole.Accountant, "accountant@example.com"));
                users.Add(GenerateTestUser(UserRole.Customer, "customer@example.com"));

                // Return the list of users
                return users;
            }
        }
    }

    /// <summary>
    /// Extension methods for HttpClient to simplify API requests in tests
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Sends a GET request to the API and deserializes the response
        /// </summary>
        /// <typeparam name="T">The type of the data in the response</typeparam>
        /// <param name="client">The HTTP client</param>
        /// <param name="endpoint">The API endpoint</param>
        /// <returns>The deserialized API response</returns>
        public static async Task<ApiResponse<T>> GetApiResponseAsync<T>(this HttpClient client, string endpoint)
        {
            // Construct the full API URL using GetApiUrl
            var url = GetApiUrl(endpoint);

            // Send a GET request to the endpoint
            var response = await client.GetAsync(url);

            // Ensure the response is successful
            response.EnsureSuccessStatusCode();

            // Deserialize the response content to ApiResponse<T>
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = DeserializeJson<ApiResponse<T>>(content);

            // Return the deserialized response
            return apiResponse;
        }

        /// <summary>
        /// Sends a POST request to the API with content and deserializes the response
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content</typeparam>
        /// <typeparam name="TResponse">The type of the data in the response</typeparam>
        /// <param name="client">The HTTP client</param>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="content">The request content</param>
        /// <returns>The deserialized API response</returns>
        public static async Task<ApiResponse<TResponse>> PostApiResponseAsync<TRequest, TResponse>(this HttpClient client, string endpoint, TRequest content)
        {
            // Construct the full API URL using GetApiUrl
            var url = GetApiUrl(endpoint);

            // Serialize the content to JSON
            var json = SerializeJson(content);

            // Create StringContent with the JSON and appropriate content type
            var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Send a POST request to the endpoint with the content
            var response = await client.PostAsync(url, stringContent);

            // Deserialize the response content to ApiResponse<TResponse>
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = DeserializeJson<ApiResponse<TResponse>>(responseContent);

            // Return the deserialized response
            return apiResponse;
        }

        /// <summary>
        /// Sends a PUT request to the API with content and deserializes the response
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content</typeparam>
        /// <typeparam name="TResponse">The type of the data in the response</typeparam>
        /// <param name="client">The HTTP client</param>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="content">The request content</param>
        /// <returns>The deserialized API response</returns>
        public static async Task<ApiResponse<TResponse>> PutApiResponseAsync<TRequest, TResponse>(this HttpClient client, string endpoint, TRequest content)
        {
            // Construct the full API URL using GetApiUrl
            var url = GetApiUrl(endpoint);

            // Serialize the content to JSON
            var json = SerializeJson(content);

            // Create StringContent with the JSON and appropriate content type
            var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Send a PUT request to the endpoint with the content
            var response = await client.PutAsync(url, stringContent);

            // Deserialize the response content to ApiResponse<TResponse>
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = DeserializeJson<ApiResponse<TResponse>>(responseContent);

            // Return the deserialized response
            return apiResponse;
        }

        /// <summary>
        /// Sends a DELETE request to the API and deserializes the response
        /// </summary>
        /// <typeparam name="T">The type of the data in the response</typeparam>
        /// <param name="client">The HTTP client</param>
        /// <param name="endpoint">The API endpoint</param>
        /// <returns>The deserialized API response</returns>
        public static async Task<ApiResponse<T>> DeleteApiResponseAsync<T>(this HttpClient client, string endpoint)
        {
            // Construct the full API URL using GetApiUrl
            var url = GetApiUrl(endpoint);

            // Send a DELETE request to the endpoint
            var response = await client.DeleteAsync(url);

            // Deserialize the response content to ApiResponse<T>
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = DeserializeJson<ApiResponse<T>>(content);

            // Return the deserialized response
            return apiResponse;
        }
    }
}