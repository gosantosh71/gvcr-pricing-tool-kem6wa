using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VatFilingPricingTool.Infrastructure.Authentication;
using VatFilingPricingTool.Infrastructure.Logging;

namespace VatFilingPricingTool.Api.Middleware
{
    /// <summary>
    /// ASP.NET Core middleware that authenticates requests by validating JWT tokens from the Authorization header
    /// </summary>
    public class AuthenticationMiddleware
    {
        private const string AUTHORIZATION_HEADER = "Authorization";
        private const string BEARER_PREFIX = "Bearer ";
        
        private readonly RequestDelegate _next;
        private readonly IJwtTokenHandler _jwtTokenHandler;
        private readonly IAzureAdAuthenticationHandler _azureAdAuthHandler;
        private readonly ILoggingService _logger;

        /// <summary>
        /// Initializes a new instance of the AuthenticationMiddleware with required dependencies
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="jwtTokenHandler">The JWT token handler</param>
        /// <param name="azureAdAuthHandler">The Azure AD authentication handler</param>
        /// <param name="logger">The logging service</param>
        public AuthenticationMiddleware(
            RequestDelegate next,
            IJwtTokenHandler jwtTokenHandler,
            IAzureAdAuthenticationHandler azureAdAuthHandler,
            ILoggingService logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _jwtTokenHandler = jwtTokenHandler ?? throw new ArgumentNullException(nameof(jwtTokenHandler));
            _azureAdAuthHandler = azureAdAuthHandler ?? throw new ArgumentNullException(nameof(azureAdAuthHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes the HTTP request, authenticates the user by validating the JWT token, and establishes user identity
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>A task representing the middleware execution</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Skip authentication for excluded paths (auth endpoints, swagger, health checks)
            if (IsExcludedPath(context.Request.Path))
            {
                _logger.LogInformation("Skipping authentication for excluded path: {Path}", context.Request.Path);
                await _next(context);
                return;
            }

            // Extract token from Authorization header
            string token = ExtractTokenFromHeader(context);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Authorization token not found in request header for path: {Path}", context.Request.Path);
                await _next(context);
                return;
            }

            bool isValidToken = false;
            ClaimsPrincipal principal = null;

            try
            {
                // Determine token type (JWT or Azure AD) and validate accordingly
                if (IsAzureAdToken(token))
                {
                    _logger.LogInformation("Processing Azure AD token for authentication");
                    isValidToken = await _azureAdAuthHandler.ValidateTokenAsync(token);
                    
                    if (isValidToken)
                    {
                        // Extract user info from Azure AD token
                        var userInfo = await _azureAdAuthHandler.GetUserInfoFromTokenAsync(token);
                        
                        // Create claims principal from user info
                        var claims = new List<Claim>();
                        foreach (var item in userInfo)
                        {
                            claims.Add(new Claim(item.Key, item.Value));
                        }
                        
                        // Create identity and principal with the appropriate authentication type
                        var identity = new ClaimsIdentity(claims, "Bearer");
                        principal = new ClaimsPrincipal(identity);
                    }
                }
                else
                {
                    _logger.LogInformation("Processing JWT token for authentication");
                    isValidToken = await _jwtTokenHandler.ValidateTokenAsync(token);
                    
                    if (isValidToken)
                    {
                        // Extract claims principal from JWT token
                        principal = await _jwtTokenHandler.GetPrincipalFromTokenAsync(token);
                    }
                }

                if (!isValidToken || principal == null)
                {
                    _logger.LogWarning("Invalid token provided for path: {Path}", context.Request.Path);
                    await _next(context);
                    return;
                }

                // Set the user principal in the HTTP context
                context.User = principal;
                
                string userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                               principal.FindFirstValue("sub") ?? 
                               "unknown";
                
                _logger.LogInformation("User authenticated successfully: {UserId} for path: {Path}", userId, context.Request.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token validation for path: {Path}", context.Request.Path);
                await _next(context);
                return;
            }

            // Continue to the next middleware
            await _next(context);
        }

        /// <summary>
        /// Extracts the JWT token from the Authorization header
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>The extracted token or null if not found</returns>
        private string ExtractTokenFromHeader(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(AUTHORIZATION_HEADER))
            {
                return null;
            }

            string authorizationHeader = context.Request.Headers[AUTHORIZATION_HEADER];
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith(BEARER_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return authorizationHeader.Substring(BEARER_PREFIX.Length).Trim();
        }

        /// <summary>
        /// Determines if the request path should be excluded from authentication
        /// </summary>
        /// <param name="path">The request path</param>
        /// <returns>True if the path should be excluded, otherwise false</returns>
        private bool IsExcludedPath(PathString path)
        {
            return path.StartsWithSegments("/api/auth") || 
                   path.StartsWithSegments("/swagger") || 
                   path.Equals("/health");
        }

        /// <summary>
        /// Determines if the token is an Azure AD token based on its structure
        /// </summary>
        /// <param name="token">The token to check</param>
        /// <returns>True if the token is an Azure AD token, otherwise false</returns>
        private bool IsAzureAdToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            try
            {
                // JWT tokens have three segments separated by dots
                var segments = token.Split('.');
                if (segments.Length != 3)
                {
                    return false;
                }

                // Base64Url decode the payload (middle segment)
                string base64Url = segments[1];
                string base64 = base64Url.Replace('-', '+').Replace('_', '/');
                
                // Add padding if needed
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }
                
                var bytes = Convert.FromBase64String(base64);
                var payloadJson = Encoding.UTF8.GetString(bytes);
                
                // Check for Azure AD specific claims
                return payloadJson.Contains("\"iss\":\"https://login.microsoftonline.com/") ||
                       payloadJson.Contains("\"tid\":") ||
                       payloadJson.Contains("\"oid\":");
            }
            catch
            {
                // If we can't decode the token, it's not a valid JWT
                return false;
            }
        }
    }

    /// <summary>
    /// Extension methods for registering the AuthenticationMiddleware in the ASP.NET Core pipeline
    /// </summary>
    public static class AuthenticationMiddlewareExtensions
    {
        /// <summary>
        /// Adds the AuthenticationMiddleware to the application's request pipeline
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}