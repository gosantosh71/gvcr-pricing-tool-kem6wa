using System; // System package version 6.0.0
using System.Collections.Generic;
using System.Security.Claims; // System.Security.Claims package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using Microsoft.AspNetCore.Components.Authorization; // Microsoft.AspNetCore.Components.Authorization package version 6.0.0
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Tests.Helpers;

namespace VatFilingPricingTool.Web.Tests.Mock
{
    /// <summary>
    /// A mock implementation of the AuthenticationStateProvider for unit testing authentication-dependent components
    /// </summary>
    public class MockAuthenticationStateProvider : AuthenticationStateProvider
    {
        private UserModel _user;
        private bool _isAuthenticated;

        /// <summary>
        /// Initializes a new instance of the MockAuthenticationStateProvider class with an unauthenticated state
        /// </summary>
        public MockAuthenticationStateProvider()
        {
            _user = null;
            _isAuthenticated = false;
        }

        /// <summary>
        /// Gets the current authentication state for testing
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the authentication state</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsPrincipal principal;

            if (_isAuthenticated && _user != null)
            {
                var claims = CreateClaimsFromUser(_user);
                var identity = new ClaimsIdentity(claims, "jwt");
                principal = new ClaimsPrincipal(identity);
            }
            else
            {
                principal = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return await Task.FromResult(new AuthenticationState(principal));
        }

        /// <summary>
        /// Sets the authentication state to authenticated with the specified user
        /// </summary>
        /// <param name="user">The user to authenticate with</param>
        public void SetAuthenticatedState(UserModel user)
        {
            _user = user;
            _isAuthenticated = true;
            NotifyAuthenticationStateChanged();
        }

        /// <summary>
        /// Sets the authentication state to unauthenticated
        /// </summary>
        public void SetUnauthenticatedState()
        {
            _user = null;
            _isAuthenticated = false;
            NotifyAuthenticationStateChanged();
        }

        /// <summary>
        /// Sets the authentication state to authenticated with a default test user
        /// </summary>
        public void SetDefaultAuthenticatedState()
        {
            var user = TestData.CreateTestUser("user-123", "user@example.com");
            SetAuthenticatedState(user);
        }

        /// <summary>
        /// Sets the authentication state to authenticated with an admin test user
        /// </summary>
        public void SetAdminAuthenticatedState()
        {
            var adminUser = TestData.CreateTestAdminUser();
            SetAuthenticatedState(adminUser);
        }

        /// <summary>
        /// Creates a collection of claims from a user model
        /// </summary>
        /// <param name="user">The user model to create claims from</param>
        /// <returns>A list of claims representing the user</returns>
        private List<Claim> CreateClaimsFromUser(UserModel user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.GetFullName())
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            return claims;
        }

        /// <summary>
        /// Notifies subscribers that the authentication state has changed
        /// </summary>
        private void NotifyAuthenticationStateChanged()
        {
            base.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}