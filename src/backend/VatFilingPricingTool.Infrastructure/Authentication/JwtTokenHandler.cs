using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.IdentityModel.Tokens.Jwt; // System.IdentityModel.Tokens.Jwt v6.15.0
using System.Security.Claims; // System.Security.Claims v6.0.0
using System.Security.Cryptography; // System.Security.Cryptography v6.0.0
using System.Text; // System.Text v6.0.0
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using Microsoft.Extensions.Options; // Microsoft.Extensions.Options v6.0.0
using Microsoft.IdentityModel.Tokens; // Microsoft.IdentityModel.Tokens v6.15.0
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Infrastructure.Authentication
{
    /// <summary>
    /// Interface for JWT token handling operations in the VAT Filing Pricing Tool
    /// </summary>
    public interface IJwtTokenHandler
    {
        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        /// <param name="user">The user model containing information for token claims</param>
        /// <returns>Tuple containing the JWT token, refresh token, and expiration date</returns>
        Task<(string token, string refreshToken, DateTime expiresAt)> GenerateTokenAsync(UserModel user);

        /// <summary>
        /// Validates a JWT token
        /// </summary>
        /// <param name="token">The JWT token to validate</param>
        /// <returns>True if the token is valid, otherwise false</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Extracts the claims principal from a JWT token
        /// </summary>
        /// <param name="token">The JWT token to extract claims from</param>
        /// <returns>Claims principal extracted from the token</returns>
        Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token);

        /// <summary>
        /// Refreshes a JWT token using a refresh token
        /// </summary>
        /// <param name="token">The expired or about-to-expire JWT token</param>
        /// <param name="refreshToken">The refresh token associated with the JWT token</param>
        /// <returns>Tuple containing the new JWT token, new refresh token, and expiration date</returns>
        Task<(string newToken, string newRefreshToken, DateTime expiresAt)> RefreshTokenAsync(string token, string refreshToken);
    }

    /// <summary>
    /// Implements JWT token generation, validation, and management for the VAT Filing Pricing Tool.
    /// This class is responsible for creating secure JWT tokens with appropriate claims,
    /// validating tokens, and handling token refresh operations.
    /// </summary>
    public class JwtTokenHandler : IJwtTokenHandler
    {
        private readonly ILogger<JwtTokenHandler> _logger;
        private readonly IOptions<AuthenticationOptions> _options;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly Dictionary<string, string> _refreshTokens;

        /// <summary>
        /// Initializes a new instance of the JwtTokenHandler class with required dependencies
        /// </summary>
        /// <param name="logger">Logger for recording token operations</param>
        /// <param name="options">Authentication configuration options</param>
        /// <exception cref="ArgumentNullException">Thrown if required dependencies are null</exception>
        public JwtTokenHandler(
            ILogger<JwtTokenHandler> logger,
            IOptions<AuthenticationOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _refreshTokens = new Dictionary<string, string>();

            // Initialize token validation parameters
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = _options.Value.ValidateIssuer,
                ValidateAudience = _options.Value.ValidateAudience,
                ValidateLifetime = _options.Value.ValidateLifetime,
                ValidateIssuerSigningKey = _options.Value.ValidateIssuerSigningKey,
                ValidIssuer = _options.Value.Issuer,
                ValidAudience = _options.Value.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecretKey)),
                ClockSkew = TimeSpan.Zero // Eliminate clock skew to enforce exact token lifetime
            };
        }

        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        /// <param name="user">The user model containing information for token claims</param>
        /// <returns>Tuple containing the JWT token, refresh token, and expiration date</returns>
        /// <exception cref="ArgumentNullException">Thrown if user is null</exception>
        public async Task<(string token, string refreshToken, DateTime expiresAt)> GenerateTokenAsync(UserModel user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _logger.LogInformation("Generating token for user {UserId} ({Email})", user.UserId, user.Email);

            // Create claims for the token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new Claim(ClaimTypes.Name, user.GetFullName())
            };

            // Add role claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            // Set token expiration
            var expires = DateTime.UtcNow.AddMinutes(_options.Value.TokenExpirationMinutes);

            // Create the JWT security token
            var token = new JwtSecurityToken(
                issuer: _options.Value.Issuer,
                audience: _options.Value.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecretKey)),
                    SecurityAlgorithms.HmacSha256)
            );

            // Generate the token string
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            
            // Store refresh token
            _refreshTokens[user.UserId] = refreshToken;

            _logger.LogInformation("Successfully generated token for user {UserId}, expires at {ExpiryTime}", 
                user.UserId, expires);

            return (tokenString, refreshToken, expires);
        }

        /// <summary>
        /// Validates a JWT token
        /// </summary>
        /// <param name="token">The JWT token to validate</param>
        /// <returns>True if the token is valid, otherwise false</returns>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token validation failed: Token is null or empty");
                return false;
            }

            _logger.LogDebug("Validating token");

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validate the token
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
                return principal != null;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning("Token validation failed: Token expired. {Message}", ex.Message);
                return false;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning("Token validation failed: Invalid signature. {Message}", ex.Message);
                return false;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Token validation failed: {Message}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                return false;
            }
        }

        /// <summary>
        /// Extracts the claims principal from a JWT token
        /// </summary>
        /// <param name="token">The JWT token to extract claims from</param>
        /// <returns>Claims principal extracted from the token or null if extraction fails</returns>
        public async Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Cannot extract principal: Token is null or empty");
                return null;
            }

            _logger.LogDebug("Extracting principal from token");

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // For principal extraction, we don't validate lifetime (since we might be refreshing an expired token)
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _options.Value.ValidateIssuer,
                    ValidateAudience = _options.Value.ValidateAudience,
                    ValidateLifetime = false, // Don't validate lifetime for extraction
                    ValidateIssuerSigningKey = _options.Value.ValidateIssuerSigningKey,
                    ValidIssuer = _options.Value.Issuer,
                    ValidAudience = _options.Value.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecretKey))
                };

                // Extract the principal from the token
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting principal from token");
                return null;
            }
        }

        /// <summary>
        /// Refreshes a JWT token using a refresh token
        /// </summary>
        /// <param name="token">The expired or about-to-expire JWT token</param>
        /// <param name="refreshToken">The refresh token associated with the JWT token</param>
        /// <returns>Tuple containing the new JWT token, new refresh token, and expiration date</returns>
        /// <exception cref="ArgumentException">Thrown if token validation fails</exception>
        /// <exception cref="SecurityTokenException">Thrown if refresh token is invalid</exception>
        public async Task<(string newToken, string newRefreshToken, DateTime expiresAt)> RefreshTokenAsync(string token, string refreshToken)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("Token and refresh token are required");
            }

            _logger.LogInformation("Processing token refresh request");

            // Get principal from expired token
            var principal = await GetPrincipalFromTokenAsync(token);
            if (principal == null)
            {
                _logger.LogWarning("Token refresh failed: Unable to extract principal from token");
                throw new SecurityTokenException("Invalid token");
            }

            // Extract user ID from the principal
            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Token refresh failed: Unable to extract user ID from token");
                throw new SecurityTokenException("Invalid token");
            }

            // Validate refresh token
            if (!_refreshTokens.TryGetValue(userId, out var storedRefreshToken) || 
                storedRefreshToken != refreshToken)
            {
                _logger.LogWarning("Token refresh failed: Invalid refresh token for user {UserId}", userId);
                throw new SecurityTokenException("Invalid refresh token");
            }

            // Create user model from claims
            var user = CreateUserFromPrincipal(principal);

            // Generate new tokens
            var result = await GenerateTokenAsync(user);

            _logger.LogInformation("Successfully refreshed token for user {UserId}", userId);

            return result;
        }

        /// <summary>
        /// Generates a cryptographically secure refresh token
        /// </summary>
        /// <returns>A secure refresh token</returns>
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

        /// <summary>
        /// Creates a user model from claims principal
        /// </summary>
        /// <param name="principal">The claims principal containing user information</param>
        /// <returns>User model populated with data from claims</returns>
        private UserModel CreateUserFromPrincipal(ClaimsPrincipal principal)
        {
            var user = new UserModel
            {
                UserId = GetClaimValue(principal, JwtRegisteredClaimNames.Sub),
                Email = GetClaimValue(principal, JwtRegisteredClaimNames.Email),
                FirstName = GetClaimValue(principal, ClaimTypes.GivenName),
                LastName = GetClaimValue(principal, ClaimTypes.Surname),
                Roles = new List<UserRole>()
            };

            // Extract roles
            var roleClaims = principal.FindAll(ClaimTypes.Role);
            foreach (var roleClaim in roleClaims)
            {
                if (Enum.TryParse<UserRole>(roleClaim.Value, out var role))
                {
                    user.Roles.Add(role);
                }
            }

            return user;
        }

        /// <summary>
        /// Gets a claim value from a claims principal
        /// </summary>
        /// <param name="principal">The claims principal</param>
        /// <param name="claimType">The type of claim to retrieve</param>
        /// <returns>The claim value or null if not found</returns>
        private string GetClaimValue(ClaimsPrincipal principal, string claimType)
        {
            return principal.FindFirst(claimType)?.Value;
        }
    }
}