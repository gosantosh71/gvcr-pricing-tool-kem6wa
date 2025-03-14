using FluentAssertions; // FluentAssertions, Version=6.2.0
using Microsoft.AspNetCore.Mvc.Testing; // Microsoft.AspNetCore.Mvc.Testing, Version=6.0.0
using System; // System, Version=6.0.0
using System.Net.Http; // System.Net.Http, Version=6.0.0
using System.Net.Http.Json; // System.Net.Http.Json, Version=6.0.0
using System.Text.Json; // System.Text.Json, Version=6.0.0
using System.Threading.Tasks; // System.Threading.Tasks, Version=6.0.0
using VatFilingPricingTool.Common.Models.ApiResponse; // Generic API response wrapper for handling test responses
using VatFilingPricingTool.Domain.Enums; // User role enumeration for authentication testing
using VatFilingPricingTool.IntegrationTests.TestServer.CustomWebApplicationFactory; // Factory for creating the test server and HTTP clients

namespace VatFilingPricingTool.IntegrationTests.TestServer
{
    /// <summary>
    /// Base class for integration tests providing common setup and utility methods
    /// </summary>
    public abstract class IntegrationTestBase : IDisposable
    {
        /// <summary>
        /// Gets the factory for creating the test server and HTTP clients
        /// </summary>
        protected CustomWebApplicationFactory Factory { get; }

        /// <summary>
        /// Gets the HTTP client for making requests to the test server
        /// </summary>
        protected HttpClient Client { get; }

        /// <summary>
        /// Gets the JSON serialization options for handling test responses
        /// </summary>
        protected JsonSerializerOptions JsonOptions { get; }

        /// <summary>
        /// Initializes a new instance of the IntegrationTestBase class
        /// </summary>
        public IntegrationTestBase()
        {
            // LD1: Create a new CustomWebApplicationFactory instance
            Factory = new CustomWebApplicationFactory();

            // LD1: Create a default HTTP client from the factory
            Client = Factory.CreateDefaultClient();

            // LD1: Configure JSON serialization options with camelCase naming policy
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // LD1: Set default request headers for the client
            ConfigureClient(Client);
        }

        /// <summary>
        /// Creates an HTTP client with authentication for a specific user role
        /// </summary>
        /// <param name="role">The user role to authenticate with</param>
        /// <returns>An authenticated HTTP client</returns>
        protected HttpClient CreateAuthenticatedClient(UserRole role)
        {
            // LD1: Call Factory.CreateClient with the specified role
            var client = Factory.CreateClient(role.ToString());

            // LD1: Configure default headers for the client
            ConfigureClient(client);

            // LD1: Return the configured client
            return client;
        }

        /// <summary>
        /// Sends a GET request to the specified URI and deserializes the response
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to</typeparam>
        /// <param name="uri">The URI to send the GET request to</param>
        /// <param name="client">Optional HTTP client to use. If null, the default client is used.</param>
        /// <returns>The deserialized API response</returns>
        protected async Task<ApiResponse<T>> GetAsync<T>(string uri, HttpClient? client = null)
        {
            // LD1: Use the provided client or the default Client if null
            var httpClient = client ?? Client;

            // LD1: Send a GET request to the specified URI
            var response = await httpClient.GetAsync(uri);

            // LD1: Ensure the response is successful
            response.EnsureSuccessStatusCode();

            // LD1: Deserialize the response content to ApiResponse<T>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);

            // LD1: Return the deserialized response
            return apiResponse ?? new ApiResponse<T> { Success = false, Message = "Failed to deserialize response" };
        }

        /// <summary>
        /// Sends a POST request with the specified content to the URI and deserializes the response
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content</typeparam>
        /// <typeparam name="TResponse">The type to deserialize the response to</typeparam>
        /// <param name="uri">The URI to send the POST request to</param>
        /// <param name="content">The content to send with the POST request</param>
        /// <param name="client">Optional HTTP client to use. If null, the default client is used.</param>
        /// <returns>The deserialized API response</returns>
        protected async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string uri, TRequest content, HttpClient? client = null)
        {
            // LD1: Use the provided client or the default Client if null
            var httpClient = client ?? Client;

            // LD1: Serialize the content to JSON
            var json = JsonSerializer.Serialize(content, JsonOptions);

            // LD1: Create StringContent with the JSON and appropriate content type
            var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // LD1: Send a POST request to the specified URI with the content
            var response = await httpClient.PostAsync(uri, stringContent);

            // LD1: Deserialize the response content to ApiResponse<TResponse>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(JsonOptions);

            // LD1: Return the deserialized response
            return apiResponse ?? new ApiResponse<TResponse> { Success = false, Message = "Failed to deserialize response" };
        }

        /// <summary>
        /// Sends a PUT request with the specified content to the URI and deserializes the response
        /// </summary>
        /// <typeparam name="TRequest">The type of the request content</typeparam>
        /// <typeparam name="TResponse">The type to deserialize the response to</typeparam>
        /// <param name="uri">The URI to send the PUT request to</param>
        /// <param name="content">The content to send with the PUT request</param>
        /// <param name="client">Optional HTTP client to use. If null, the default client is used.</param>
        /// <returns>The deserialized API response</returns>
        protected async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string uri, TRequest content, HttpClient? client = null)
        {
            // LD1: Use the provided client or the default Client if null
            var httpClient = client ?? Client;

            // LD1: Serialize the content to JSON
            var json = JsonSerializer.Serialize(content, JsonOptions);

            // LD1: Create StringContent with the JSON and appropriate content type
            var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // LD1: Send a PUT request to the specified URI with the content
            var response = await httpClient.PutAsync(uri, stringContent);

            // LD1: Deserialize the response content to ApiResponse<TResponse>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(JsonOptions);

            // LD1: Return the deserialized response
            return apiResponse ?? new ApiResponse<TResponse> { Success = false, Message = "Failed to deserialize response" };
        }

        /// <summary>
        /// Sends a DELETE request to the specified URI and deserializes the response
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to</typeparam>
        /// <param name="uri">The URI to send the DELETE request to</param>
        /// <param name="client">Optional HTTP client to use. If null, the default client is used.</param>
        /// <returns>The deserialized API response</returns>
        protected async Task<ApiResponse<T>> DeleteAsync<T>(string uri, HttpClient? client = null)
        {
            // LD1: Use the provided client or the default Client if null
            var httpClient = client ?? Client;

            // LD1: Send a DELETE request to the specified URI
            var response = await httpClient.DeleteAsync(uri);

            // LD1: Deserialize the response content to ApiResponse<T>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);

            // LD1: Return the deserialized response
            return apiResponse ?? new ApiResponse<T> { Success = false, Message = "Failed to deserialize response" };
        }

        /// <summary>
        /// Disposes of resources used by the test class
        /// </summary>
        public void Dispose()
        {
            // LD1: Dispose of the HTTP client
            Client.Dispose();

            // LD1: Dispose of the CustomWebApplicationFactory
            Factory.Dispose();
        }

        /// <summary>
        /// Configures default headers and settings for an HTTP client
        /// </summary>
        /// <param name="client">The HTTP client to configure</param>
        /// <returns>The configured HTTP client</returns>
        private HttpClient ConfigureClient(HttpClient client)
        {
            // LD1: Set Accept header to application/json
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // LD1: Set default timeout to 30 seconds
            client.Timeout = TimeSpan.FromSeconds(30);

            // LD1: Return the configured client
            return client;
        }
    }
}