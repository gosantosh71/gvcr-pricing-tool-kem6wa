using System; // version 6.0.0
using System.Collections.Generic; // version 6.0.0
using System.Net; // version 6.0.0
using System.Net.Http; // version 6.0.0
using System.Text; // version 6.0.0
using System.Text.Json; // version 6.0.0
using System.Threading; // version 6.0.0
using System.Threading.Tasks; // version 6.0.0
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Web.Tests.Mock
{
    /// <summary>
    /// A mock implementation of HttpMessageHandler for unit testing HTTP client interactions.
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, HttpResponseMessage> _responses;
        private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> _requestHandlers;
        
        /// <summary>
        /// Initializes a new instance of the MockHttpMessageHandler class.
        /// </summary>
        public MockHttpMessageHandler()
        {
            _responses = new Dictionary<string, HttpResponseMessage>();
            _requestHandlers = new Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>>();
        }
        
        /// <summary>
        /// Processes an HTTP request and returns a mocked response.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation, containing the mocked HTTP response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri.ToString();
            
            if (_responses.TryGetValue(uri, out var response))
            {
                return response;
            }
            
            if (_requestHandlers.TryGetValue(uri, out var handler))
            {
                return handler(request);
            }
            
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
        
        /// <summary>
        /// Registers a mock response for a specific request URI.
        /// </summary>
        /// <param name="uri">The URI to mock.</param>
        /// <returns>The current instance for method chaining.</returns>
        public MockHttpMessageHandler When(string uri)
        {
            return this;
        }
        
        /// <summary>
        /// Configures a response for the previously specified URI.
        /// </summary>
        /// <param name="response">The HTTP response message to return.</param>
        /// <returns>The current instance for method chaining.</returns>
        public MockHttpMessageHandler RespondWith(HttpResponseMessage response)
        {
            _responses[_currentUri] = response;
            return this;
        }
        
        /// <summary>
        /// Configures a JSON response for the previously specified URI.
        /// </summary>
        /// <typeparam name="T">The type of the content to serialize.</typeparam>
        /// <param name="content">The content to serialize to JSON.</param>
        /// <param name="statusCode">The HTTP status code to return.</param>
        /// <returns>The current instance for method chaining.</returns>
        public MockHttpMessageHandler RespondWithJson<T>(T content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var json = JsonSerializer.Serialize(content);
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            
            _responses[_currentUri] = response;
            return this;
        }
        
        /// <summary>
        /// Configures an API response for the previously specified URI.
        /// </summary>
        /// <typeparam name="T">The type of the data in the API response.</typeparam>
        /// <param name="data">The data to include in the API response.</param>
        /// <param name="statusCode">The HTTP status code to return.</param>
        /// <returns>The current instance for method chaining.</returns>
        public MockHttpMessageHandler RespondWithApiResponse<T>(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var apiResponse = ApiResponse<T>.CreateSuccess(data);
            return RespondWithJson(apiResponse, statusCode);
        }
        
        /// <summary>
        /// Configures an API error response for the previously specified URI.
        /// </summary>
        /// <param name="errorMessage">The error message to include in the API response.</param>
        /// <param name="statusCode">The HTTP status code to return.</param>
        /// <returns>The current instance for method chaining.</returns>
        public MockHttpMessageHandler RespondWithApiError(string errorMessage, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var apiResponse = ApiResponse<object>.CreateError(errorMessage);
            return RespondWithJson(apiResponse, statusCode);
        }
        
        /// <summary>
        /// Configures a custom handler function for the previously specified URI.
        /// </summary>
        /// <param name="handler">A function that processes the request and returns a response.</param>
        /// <returns>The current instance for method chaining.</returns>
        public MockHttpMessageHandler RespondWithHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _requestHandlers[_currentUri] = handler;
            return this;
        }
        
        /// <summary>
        /// Clears all configured responses and handlers.
        /// </summary>
        public void Clear()
        {
            _responses.Clear();
            _requestHandlers.Clear();
        }
    }
}