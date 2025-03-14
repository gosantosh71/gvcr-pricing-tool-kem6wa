using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Helpers;

namespace VatFilingPricingTool.Web.Clients
{
    /// <summary>
    /// Client for making HTTP requests to the VAT Filing Pricing Tool backend API.
    /// This class provides a wrapper around HttpClient with methods for performing
    /// common HTTP operations (GET, POST, PUT, DELETE) with proper error handling,
    /// serialization, and authentication.
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClientFactory httpClientFactory;
        private readonly LocalStorageHelper localStorage;
        private readonly ILogger<ApiClient> logger;
        private readonly JsonSerializerOptions jsonOptions;

        /// <summary>
        /// Initializes a new instance of the ApiClient class with required dependencies.
        /// </summary>
        /// <param name="httpClientFactory">Factory for creating configured HttpClient instances.</param>
        /// <param name="localStorage">Helper for accessing browser local storage.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public ApiClient(
            HttpClientFactory httpClientFactory,
            LocalStorageHelper localStorage,
            ILogger<ApiClient> logger)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Configure JSON serialization options
            this.jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Sends a GET request to the specified endpoint and deserializes the response to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="endpoint">The API endpoint to send the request to.</param>
        /// <param name="requiresAuth">Whether the request requires authentication.</param>
        /// <returns>A task representing the asynchronous operation, containing the deserialized response.</returns>
        public async Task<T> GetAsync<T>(string endpoint, bool requiresAuth = true)
        {
            logger.LogInformation("Sending GET request to {Endpoint}", endpoint);
            
            try
            {
                // Create an HttpClient based on authentication requirements
                HttpClient client = CreateHttpClient(requiresAuth);
                
                // Send the request
                HttpResponseMessage response = await client.GetAsync(endpoint);
                
                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response content
                    return await response.Content.ReadFromJsonAsync<T>(jsonOptions);
                }
                
                // Handle error response
                await HandleErrorResponse(response, endpoint);
                
                // This line will not be reached if HandleErrorResponse throws an exception
                // But we need to return something to satisfy the compiler
                return default;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending GET request to {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Sends a GET request to the specified endpoint without expecting a typed response.
        /// </summary>
        /// <param name="endpoint">The API endpoint to send the request to.</param>
        /// <param name="requiresAuth">Whether the request requires authentication.</param>
        /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
        public async Task<HttpResponseMessage> GetAsync(string endpoint, bool requiresAuth = true)
        {
            logger.LogInformation("Sending GET request to {Endpoint}", endpoint);
            
            try
            {
                // Create an HttpClient based on authentication requirements
                HttpClient client = CreateHttpClient(requiresAuth);
                
                // Send the request
                return await client.GetAsync(endpoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending GET request to {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Sends a POST request with the specified data to the endpoint and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type of the request data.</typeparam>
        /// <typeparam name="TResult">The type to deserialize the response to.</typeparam>
        /// <param name="endpoint">The API endpoint to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="requiresAuth">Whether the request requires authentication.</param>
        /// <returns>A task representing the asynchronous operation, containing the deserialized response.</returns>
        public async Task<TResult> PostAsync<T, TResult>(string endpoint, T data, bool requiresAuth = true)
        {
            logger.LogInformation("Sending POST request to {Endpoint}", endpoint);
            
            try
            {
                // Create an HttpClient based on authentication requirements
                HttpClient client = CreateHttpClient(requiresAuth);
                
                // Send the request with JSON content
                HttpResponseMessage response = await client.PostAsJsonAsync(endpoint, data, jsonOptions);
                
                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response content
                    return await response.Content.ReadFromJsonAsync<TResult>(jsonOptions);
                }
                
                // Handle error response
                await HandleErrorResponse(response, endpoint);
                
                // This line will not be reached if HandleErrorResponse throws an exception
                // But we need to return something to satisfy the compiler
                return default;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending POST request to {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Sends a POST request with the specified data to the endpoint without expecting a typed response.
        /// </summary>
        /// <typeparam name="T">The type of the request data.</typeparam>
        /// <param name="endpoint">The API endpoint to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="requiresAuth">Whether the request requires authentication.</param>
        /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, bool requiresAuth = true)
        {
            logger.LogInformation("Sending POST request to {Endpoint}", endpoint);
            
            try
            {
                // Create an HttpClient based on authentication requirements
                HttpClient client = CreateHttpClient(requiresAuth);
                
                // Send the request with JSON content
                return await client.PostAsJsonAsync(endpoint, data, jsonOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending POST request to {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Sends a POST request to the endpoint without a request body.
        /// </summary>
        /// <param name="endpoint">The API endpoint to send the request to.</param>
        /// <param name="requiresAuth">Whether the request requires authentication.</param>
        /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
        public async Task<HttpResponseMessage> PostAsync(string endpoint, bool requiresAuth = true)
        {
            logger.LogInformation("Sending POST request to {Endpoint}", endpoint);
            
            try
            {
                // Create an HttpClient based on authentication requirements
                HttpClient client = CreateHttpClient(requiresAuth);
                
                // Send the request with empty content
                return await client.PostAsync(endpoint, new StringContent(string.Empty));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending POST request to {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Sends a PUT request with the specified data to the endpoint and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type of the request data.</typeparam>
        /// <typeparam name="TResult">The type to deserialize the response to.</typeparam>
        /// <param name="endpoint">The API endpoint to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="requiresAuth">Whether the request requires authentication.</param>
        /// <returns>A task representing the asynchronous operation, containing the deserialized response.</returns>
        public async Task<TResult> PutAsync<T, TResult>(string endpoint, T data, bool requiresAuth = true)
        {
            logger.LogInformation("Sending PUT request to {Endpoint}", endpoint);
            
            try
            {
                // Create an HttpClient based on authentication requirements
                HttpClient client = CreateHttpClient(requiresAuth);
                
                // Send the request with JSON content
                HttpResponseMessage response = await client.PutAsJsonAsync(endpoint, data, jsonOptions);
                
                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response content
                    return await response.Content.ReadFromJsonAsync<TResult>(jsonOptions);
                }
                
                // Handle error response
                await HandleErrorResponse(response, endpoint);
                
                // This line will not be reached if HandleErrorResponse throws an exception
                // But we need to return something to satisfy the compiler
                return default;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending PUT request to {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Sends a PUT request with the specified data to the endpoint without expecting a typed response.
        /// </summary>
        /// <typeparam name="T">The type of the request data.</typeparam>
        /// <param name="endpoint">The API endpoint to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="requiresAuth">Whether the request requires authentication.</param>
        /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, bool requiresAuth = true)
        {
            logger.LogInformation("Sending PUT request to {Endpoint}", endpoint);
            
            try
            {
                // Create an HttpClient based on authentication requirements
                HttpClient client = CreateHttpClient(requiresAuth);
                
                // Send the request with JSON content
                return await client.PutAsJsonAsync(endpoint, data, jsonOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending PUT request to {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Sends a DELETE request to the specified endpoint and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="endpoint">The API endpoint to send the request to.</param>
        /// <param name="requiresAuth">Whether the request requires authentication.</param>
        /// <returns>A task representing the asynchronous operation, containing the deserialized response.</returns>
        public async Task<T> DeleteAsync<T>(string endpoint, bool requiresAuth = true)
        {
            logger.LogInformation("Sending DELETE request to {Endpoint}", endpoint);
            
            try
            {
                // Create an HttpClient based on authentication requirements
                HttpClient client = CreateHttpClient(requiresAuth);
                
                // Send the request
                HttpResponseMessage response = await client.DeleteAsync(endpoint);
                
                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response content
                    return await response.Content.ReadFromJsonAsync<T>(jsonOptions);
                }
                
                // Handle error response
                await HandleErrorResponse(response, endpoint);
                
                // This line will not be reached if HandleErrorResponse throws an exception
                // But we need to return something to satisfy the compiler
                return default;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending DELETE request to {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Sends a DELETE request to the specified endpoint without expecting a typed response.
        /// </summary>
        /// <param name="endpoint">The API endpoint to send the request to.</param>
        /// <param name="requiresAuth">Whether the request requires authentication.</param>
        /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
        public async Task<HttpResponseMessage> DeleteAsync(string endpoint, bool requiresAuth = true)
        {
            logger.LogInformation("Sending DELETE request to {Endpoint}", endpoint);
            
            try
            {
                // Create an HttpClient based on authentication requirements
                HttpClient client = CreateHttpClient(requiresAuth);
                
                // Send the request
                return await client.DeleteAsync(endpoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending DELETE request to {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Creates an appropriate HttpClient based on authentication requirements.
        /// </summary>
        /// <param name="requiresAuth">Whether the client requires authentication capabilities.</param>
        /// <returns>An HttpClient configured for the specified authentication requirements.</returns>
        private HttpClient CreateHttpClient(bool requiresAuth)
        {
            // Create the appropriate HttpClient based on authentication requirements
            return requiresAuth
                ? httpClientFactory.CreateResilientAuthenticatedClient()
                : httpClientFactory.CreateClient();
        }
        
        /// <summary>
        /// Handles error responses from the API by logging and throwing appropriate exceptions.
        /// </summary>
        /// <param name="response">The HTTP response message.</param>
        /// <param name="endpoint">The API endpoint that was called.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task HandleErrorResponse(HttpResponseMessage response, string endpoint)
        {
            logger.LogError("Error response from {Endpoint}: {StatusCode}", endpoint, response.StatusCode);
            
            // Try to read the response content
            string responseContent = await response.Content.ReadAsStringAsync();
            
            // Log the response content for debugging
            logger.LogError("Response content: {Content}", responseContent);
            
            // Throw an appropriate exception based on the status code
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException($"Unauthorized access to {endpoint}. Please check your credentials.");
                case System.Net.HttpStatusCode.Forbidden:
                    throw new UnauthorizedAccessException($"Access forbidden to {endpoint}. You do not have permission to access this resource.");
                case System.Net.HttpStatusCode.NotFound:
                    throw new InvalidOperationException($"Resource not found at {endpoint}.");
                case System.Net.HttpStatusCode.BadRequest:
                    throw new InvalidOperationException($"Bad request to {endpoint}. {responseContent}");
                default:
                    throw new HttpRequestException($"HTTP request failed with status code {response.StatusCode}. {responseContent}");
            }
        }
    }
}