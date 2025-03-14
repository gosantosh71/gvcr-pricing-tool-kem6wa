using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.IntegrationTests.Utilities
{
    /// <summary>
    /// Authentication handler that simulates user authentication for integration tests
    /// without requiring actual Azure AD authentication.
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ILogger<TestAuthHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAuthHandler"/> class.
        /// </summary>
        /// <param name="options">The authentication scheme options.</param>
        /// <param name="logger">The logger factory.</param>
        /// <param name="encoder">The URL encoder.</param>
        /// <param name="clock">The system clock.</param>
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<TestAuthHandler>();
        }

        /// <summary>
        /// Handles authentication by creating a ClaimsPrincipal based on the Authorization header.
        /// In testing scenarios, the Authorization header contains the user role to simulate.
        /// </summary>
        /// <returns>Authentication result with claims principal representing the authenticated user.</returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                _logger.LogInformation("Handling test authentication");

                if (!Request.Headers.ContainsKey("Authorization"))
                {
                    _logger.LogInformation("No Authorization header present, returning no result");
                    return await Task.FromResult(AuthenticateResult.NoResult());
                }

                var userRole = Request.Headers["Authorization"].ToString();
                _logger.LogInformation($"Processing authentication for user role: {userRole}");

                var claims = CreateClaims(userRole);
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                _logger.LogInformation($"Successfully authenticated test user with role: {userRole}");
                return await Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test authentication handler");
                return await Task.FromResult(AuthenticateResult.Fail($"Test authentication failed: {ex.Message}"));
            }
        }

        /// <summary>
        /// Creates claims for a user based on the specified role.
        /// </summary>
        /// <param name="userRole">The user role to create claims for.</param>
        /// <returns>A list of claims for the user.</returns>
        private List<Claim> CreateClaims(string userRole)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, $"test-user-{Guid.NewGuid()}"),
                new Claim(ClaimTypes.Name, $"Test User ({userRole})"),
                new Claim(ClaimTypes.Email, $"test.{userRole.ToLowerInvariant()}@example.com")
            };

            claims.Add(new Claim(ClaimTypes.Role, userRole));

            return claims;
        }
    }

    /// <summary>
    /// Extension methods for configuring test authentication in integration tests.
    /// </summary>
    public static class TestAuthExtensions
    {
        /// <summary>
        /// Adds test authentication services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add authentication to.</param>
        /// <returns>The service collection with test authentication configured.</returns>
        public static IServiceCollection AddTestAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            return services;
        }

        /// <summary>
        /// Configures the application to use test authentication.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder with test authentication configured.</returns>
        public static IApplicationBuilder UseTestAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            return app;
        }

        /// <summary>
        /// Creates an Authorization header value for a specific user role.
        /// </summary>
        /// <param name="role">The user role to create a header for.</param>
        /// <returns>The Authorization header value.</returns>
        public static string CreateUserRoleHeader(UserRole role)
        {
            return role.ToString();
        }
    }
}