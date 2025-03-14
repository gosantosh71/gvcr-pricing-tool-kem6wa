using System;
using System.Collections.Generic;

namespace VatFilingPricingTool.Web.Authentication
{
    /// <summary>
    /// Configuration options for Azure Active Directory authentication in the web client.
    /// This class serves as a strongly-typed configuration container for Azure AD settings
    /// used for client-side authentication.
    /// </summary>
    public class AzureAdAuthOptions
    {
        /// <summary>
        /// Gets or sets the Authority for Azure AD authentication.
        /// This is typically constructed from Instance and TenantId.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the Client ID (Application ID) for the application in Azure AD.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Tenant ID for the application in Azure AD.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Domain for the Azure AD tenant.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the Instance URL for Azure AD authentication.
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets the Redirect URI for the application after authentication.
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the Post Logout Redirect URI for the application after logout.
        /// </summary>
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the Default Scopes for the application.
        /// These are the permissions the application requests from the user.
        /// </summary>
        public List<string> DefaultScopes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to validate the authority.
        /// </summary>
        public bool ValidateAuthority { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAdAuthOptions"/> class with default values.
        /// </summary>
        public AzureAdAuthOptions()
        {
            Instance = "https://login.microsoftonline.com/";
            DefaultScopes = new List<string>
            {
                "openid",
                "profile",
                "email",
                "api://vatfilingpricingtool/user_impersonation"
            };
            ValidateAuthority = true;
            
            // Note: In a Blazor WebAssembly application, these values would typically be set
            // at runtime based on the actual application URL. The window.location.origin 
            // reference would be resolved via JavaScript interop when the application starts.
            RedirectUri = "{baseUrl}/authentication/login-callback";
            PostLogoutRedirectUri = "{baseUrl}/authentication/logout-callback";
        }
    }
}