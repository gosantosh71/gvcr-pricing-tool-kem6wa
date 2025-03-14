using System;
using System.Net.Http;
using Bunit; // bunit version 1.12.6
using Bunit.TestDoubles; // bunit version 1.12.6
using Microsoft.Extensions.DependencyInjection; // Microsoft.Extensions.DependencyInjection version 6.0.0
using Microsoft.AspNetCore.Components.Authorization; // Microsoft.AspNetCore.Components.Authorization version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging version 6.0.0
using Moq; // Moq version 4.18.2
using System.Net.Http.Json; // System.Net.Http.Json version 6.0.0
using VatFilingPricingTool.Web.Tests.Helpers;
using VatFilingPricingTool.Web.Tests.Mock;

namespace VatFilingPricingTool.Web.Tests.Helpers
{
    /// <summary>
    /// Static class providing utility methods for rendering and testing Blazor components
    /// in the VAT Filing Pricing Tool web application.
    /// </summary>
    public static class RenderComponent
    {
        /// <summary>
        /// Creates a test context for Blazor component testing with common services registered.
        /// </summary>
        /// <returns>A configured Bunit TestContext with registered services.</returns>
        public static TestContext CreateTestContext()
        {
            var context = new TestContext();
            
            // Register mock authentication state provider
            var authStateProvider = new MockAuthenticationStateProvider();
            context.Services.AddScoped<AuthenticationStateProvider>(sp => authStateProvider);
            
            // Register mock logger
            context.Services.AddLogging();
            
            // Register other common services
            context.Services.AddAuthorizationCore();
            
            return context;
        }

        /// <summary>
        /// Configures the test context with an authenticated user.
        /// </summary>
        /// <param name="context">The test context to configure.</param>
        /// <param name="userId">The user ID to assign to the authenticated user.</param>
        /// <param name="email">The email to assign to the authenticated user.</param>
        /// <returns>The configured authentication state provider.</returns>
        public static MockAuthenticationStateProvider SetupAuthenticatedUser(TestContext context, string userId, string email)
        {
            var authStateProvider = context.Services.GetRequiredService<AuthenticationStateProvider>() as MockAuthenticationStateProvider;
            var user = TestData.CreateTestUser(userId, email);
            authStateProvider.SetAuthenticatedState(user);
            
            return authStateProvider;
        }

        /// <summary>
        /// Configures the test context with an administrator user.
        /// </summary>
        /// <param name="context">The test context to configure.</param>
        /// <returns>The configured authentication state provider.</returns>
        public static MockAuthenticationStateProvider SetupAdminUser(TestContext context)
        {
            var authStateProvider = context.Services.GetRequiredService<AuthenticationStateProvider>() as MockAuthenticationStateProvider;
            var adminUser = TestData.CreateTestAdminUser();
            authStateProvider.SetAuthenticatedState(adminUser);
            
            return authStateProvider;
        }

        /// <summary>
        /// Configures the test context with an unauthenticated user.
        /// </summary>
        /// <param name="context">The test context to configure.</param>
        /// <returns>The configured authentication state provider.</returns>
        public static MockAuthenticationStateProvider SetupUnauthenticatedUser(TestContext context)
        {
            var authStateProvider = context.Services.GetRequiredService<AuthenticationStateProvider>() as MockAuthenticationStateProvider;
            authStateProvider.SetUnauthenticatedState();
            
            return authStateProvider;
        }

        /// <summary>
        /// Creates and configures an HttpClient with a mock message handler for testing.
        /// </summary>
        /// <param name="context">The test context to register the HttpClient with.</param>
        /// <returns>A tuple containing the configured HttpClient and its mock handler.</returns>
        public static Tuple<HttpClient, MockHttpMessageHandler> SetupHttpClient(TestContext context)
        {
            var mockHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("https://api.vatfilingpricingtool.com/")
            };
            
            context.Services.AddSingleton(httpClient);
            
            return new Tuple<HttpClient, MockHttpMessageHandler>(httpClient, mockHandler);
        }

        /// <summary>
        /// Configures a mock HTTP response with the provided data.
        /// </summary>
        /// <typeparam name="T">The type of data to return in the response.</typeparam>
        /// <param name="handler">The mock HTTP handler to configure.</param>
        /// <param name="requestUri">The request URI to mock.</param>
        /// <param name="responseData">The data to return in the response.</param>
        /// <param name="method">The HTTP method to mock (defaults to GET).</param>
        public static void SetupMockDataResponse<T>(MockHttpMessageHandler handler, string requestUri, T responseData, HttpMethod method = null)
        {
            method ??= HttpMethod.Get;
            handler.When(requestUri)
                  .RespondWithApiResponse(responseData);
        }

        /// <summary>
        /// Configures a mock HTTP error response.
        /// </summary>
        /// <param name="handler">The mock HTTP handler to configure.</param>
        /// <param name="requestUri">The request URI to mock.</param>
        /// <param name="errorMessage">The error message to include in the response.</param>
        /// <param name="statusCode">The HTTP status code to return.</param>
        /// <param name="method">The HTTP method to mock (defaults to GET).</param>
        public static void SetupMockErrorResponse(MockHttpMessageHandler handler, string requestUri, string errorMessage, System.Net.HttpStatusCode statusCode, HttpMethod method = null)
        {
            method ??= HttpMethod.Get;
            handler.When(requestUri)
                  .RespondWithApiError(errorMessage, statusCode);
        }

        /// <summary>
        /// Registers a mock service in the test context.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <typeparam name="TMock">The mock implementation type.</typeparam>
        /// <param name="context">The test context to register the service with.</param>
        /// <param name="mockService">The mock service implementation.</param>
        /// <returns>The registered mock service.</returns>
        public static TMock RegisterMockService<TService, TMock>(TestContext context, TMock mockService) 
            where TService : class 
            where TMock : class, TService
        {
            context.Services.AddSingleton<TService>(mockService);
            return mockService;
        }

        /// <summary>
        /// Creates and registers a mock service in the test context.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <param name="context">The test context to register the service with.</param>
        /// <returns>The created mock service.</returns>
        public static Mock<TService> CreateMockService<TService>(TestContext context) 
            where TService : class
        {
            var mock = new Mock<TService>();
            context.Services.AddSingleton<TService>(mock.Object);
            return mock;
        }
    }
}