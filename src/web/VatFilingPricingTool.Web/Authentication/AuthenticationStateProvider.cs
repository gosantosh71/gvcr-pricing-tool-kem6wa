using System; // System v6.0.0
using System.Collections.Generic;
using System.Security.Claims; // System.Security.Claims v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using Microsoft.AspNetCore.Components.Authorization; // Microsoft.AspNetCore.Components.Authorization v6.0.0
using VatFilingPricingTool.Web.Helpers;
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Authentication
{
    /// <summary>
    /// Abstract base class that provides authentication state management for the VAT Filing Pricing Tool web application.
    /// This class extends the Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
    /// and implements core functionality for handling JWT token-based authentication in Blazor WebAssembly.
    /// </summary>
    public abstract class AuthenticationStateProvider : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
    {
        protected readonly LocalStorageHelper localStorage;
        protected UserModel CurrentUser;
        protected string AuthToken;
        protected DateTime? TokenExpiration;
        protected bool IsTokenExpired;

        /// <summary>
        /// Initializes a new instance of the AuthenticationStateProvider class with the required dependencies.
        /// </summary>
        /// <param name="localStorage">Helper for accessing local storage.</param>
        protected AuthenticationStateProvider(LocalStorageHelper localStorage)
        {
            this.localStorage = localStorage;
            CurrentUser = null;
            AuthToken = null;
            TokenExpiration = null;
            IsTokenExpired = false;
        }

        /// <summary>
        /// Gets the authentication state for the current user based on stored tokens.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the authentication state.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Retrieve the authentication token from local storage if not already loaded
            if (string.IsNullOrEmpty(AuthToken))
            {
                AuthToken = await localStorage.GetAuthTokenAsync();
            }

            // If token is null or empty, return an empty ClaimsPrincipal (unauthenticated)
            if (string.IsNullOrEmpty(AuthToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Retrieve the user data from local storage if not already loaded
            if (CurrentUser == null)
            {
                CurrentUser = await localStorage.GetUserDataAsync();
            }

            // If user data is null, return an empty ClaimsPrincipal (unauthenticated)
            if (CurrentUser == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Check if token expiration is not set and retrieve it from local storage
            if (!TokenExpiration.HasValue)
            {
                TokenExpiration = await localStorage.GetTokenExpirationAsync();
                // Update token expiration status
                IsTokenExpired = TokenExpiration.HasValue && TokenExpiration.Value <= DateTime.UtcNow;
            }

            // If token is expired, return an empty ClaimsPrincipal (unauthenticated)
            if (IsTokenExpired)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Create claims for the authenticated user (UserId, Email, Name, Roles)
            var claims = CreateClaimsFromUser(CurrentUser);
            // Create a ClaimsIdentity with the claims and 'jwt' authentication type
            var identity = new ClaimsIdentity(claims, "jwt");
            // Create a ClaimsPrincipal with the ClaimsIdentity
            var user = new ClaimsPrincipal(identity);
            
            // Return a new AuthenticationState with the ClaimsPrincipal
            return new AuthenticationState(user);
        }

        /// <summary>
        /// Initializes the authentication state by loading token and user data from local storage.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async Task InitializeAuthenticationStateAsync()
        {
            // Retrieve the authentication token from local storage
            AuthToken = await localStorage.GetAuthTokenAsync();
            // Retrieve the token expiration from local storage
            TokenExpiration = await localStorage.GetTokenExpirationAsync();
            // Retrieve the user data from local storage
            CurrentUser = await localStorage.GetUserDataAsync();

            // Update IsTokenExpired based on TokenExpiration and current time
            IsTokenExpired = TokenExpiration.HasValue && TokenExpiration.Value <= DateTime.UtcNow;

            // Notify authentication state changed
            NotifyAuthenticationStateChanged();
        }

        /// <summary>
        /// Creates a collection of claims from a user model.
        /// </summary>
        /// <param name="user">The user model to create claims from.</param>
        /// <returns>A list of claims representing the user.</returns>
        protected List<Claim> CreateClaimsFromUser(UserModel user)
        {
            // Create a new list of claims
            var claims = new List<Claim>
            {
                // Add claim for user ID (ClaimTypes.NameIdentifier)
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                // Add claim for email (ClaimTypes.Email)
                new Claim(ClaimTypes.Email, user.Email),
                // Add claim for name (ClaimTypes.Name)
                new Claim(ClaimTypes.Name, user.GetFullName())
            };

            // Add claims for each role (ClaimTypes.Role)
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            // Return the list of claims
            return claims;
        }

        /// <summary>
        /// Notifies subscribers that the authentication state has changed.
        /// </summary>
        protected void NotifyAuthenticationStateChanged()
        {
            // Call base.NotifyAuthenticationStateChanged with the result of GetAuthenticationStateAsync()
            base.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}