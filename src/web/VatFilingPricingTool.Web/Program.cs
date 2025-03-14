using Microsoft.AspNetCore.Components.Web; // Microsoft.AspNetCore.Components.Web v6.0.0
using Microsoft.AspNetCore.Components.WebAssembly.Hosting; // Microsoft.AspNetCore.Components.WebAssembly v6.0.0
using Microsoft.AspNetCore.Components.WebAssembly.Authentication; // Microsoft.AspNetCore.Components.WebAssembly.Authentication v6.0.0
using Microsoft.Authentication.WebAssembly.Msal; // Microsoft.Authentication.WebAssembly.Msal v6.0.0
using Microsoft.Extensions.DependencyInjection; // Microsoft.Extensions.DependencyInjection v6.0.0
using Microsoft.Extensions.Http; // Microsoft.Extensions.Http v6.0.0
using Microsoft.Extensions.Configuration; // Microsoft.Extensions.Configuration v6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using Blazored.LocalStorage; // Blazored.LocalStorage v4.3.0
using Blazored.Toast; // Blazored.Toast v3.2.2
using VatFilingPricingTool.Web.Authentication; // Import from ./Authentication/AzureAdAuthOptions.cs
using VatFilingPricingTool.Web.Authentication; // Import from ./Authentication/AuthenticationService.cs
using VatFilingPricingTool.Web.Authentication; // Import from ./Authentication/TokenAuthenticationStateProvider.cs
using VatFilingPricingTool.Web.Clients; // Import from ./Clients/ApiClient.cs
using VatFilingPricingTool.Web.Clients; // Import from ./Clients/HttpClientFactory.cs
using VatFilingPricingTool.Web.Clients; // Import from ./Clients/ApiEndpoints.cs
using VatFilingPricingTool.Web.Handlers; // Import from ./Handlers/AuthorizationMessageHandler.cs
using VatFilingPricingTool.Web.Helpers; // Import from ./Helpers/LocalStorageHelper.cs
using VatFilingPricingTool.Web.Services.Interfaces; // Import from ./Services/Interfaces/IAuthService.cs
using VatFilingPricingTool.Web.Services.Interfaces; // Import from ./Services/Interfaces/ICountryService.cs
using VatFilingPricingTool.Web.Services.Interfaces; // Import from ./Services/Interfaces/IPricingService.cs
using VatFilingPricingTool.Web.Services.Interfaces; // Import from ./Services/Interfaces/IReportService.cs
using VatFilingPricingTool.Web.Services.Interfaces; // Import from ./Services/Interfaces/IRuleService.cs
using VatFilingPricingTool.Web.Services.Interfaces; // Import from ./Services/Interfaces/IUserService.cs
using VatFilingPricingTool.Web.Services.Implementations; // Import from ./Services/Implementations/AuthService.cs
using VatFilingPricingTool.Web.Services.Implementations; // Import from ./Services/Implementations/CountryService.cs
using VatFilingPricingTool.Web.Services.Implementations; // Import from ./Services/Implementations/PricingService.cs
using VatFilingPricingTool.Web.Services.Implementations; // Import from ./Services/Implementations/ReportService.cs
using VatFilingPricingTool.Web.Services.Implementations; // Import from ./Services/Implementations/RuleService.cs
using VatFilingPricingTool.Web.Services.Implementations; // Import from ./Services/Implementations/UserService.cs

namespace VatFilingPricingTool.Web
{
    /// <summary>
    /// Provides the entry point for the Blazor WebAssembly application.
    /// Configures the application services, sets up dependency injection, registers HTTP clients, and initializes authentication services.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the Blazor WebAssembly application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            // Create a WebAssemblyHostBuilder using WebAssemblyHostBuilder.CreateDefault(args)
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // Configure logging using builder.Logging.AddConfiguration()
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

            // Load configuration from appsettings.json
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Register root components using builder.RootComponents.Add<App>("#app")
            builder.RootComponents.Add<App>("#app");

            // Register root components using builder.RootComponents.Add<HeadOutlet>("head::after")
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Configure services using ConfigureServices method
            ConfigureServices(builder);

            // Build and run the WebAssemblyHost
            await builder.Build().RunAsync();
        }

        /// <summary>
        /// Configures the application services and dependency injection.
        /// </summary>
        /// <param name="builder">The WebAssemblyHostBuilder instance.</param>
        private static void ConfigureServices(WebAssemblyHostBuilder builder)
        {
            // Register HttpClient with base address from configuration
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Configure Azure AD authentication using builder.Services.AddMsalAuthentication()
            builder.Services.AddMsalAuthentication<RemoteAuthenticationState, AccountClaimsPrincipalFactory<RemoteAuthenticationState>>(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
                options.ProviderOptions.DefaultAccessTokenScopes.Add("profile");
                options.ProviderOptions.DefaultAccessTokenScopes.Add("email");
                options.ProviderOptions.DefaultAccessTokenScopes.Add("api://vatfilingpricingtool/user_impersonation");
                options.ProviderOptions.LoginMode = "redirect";
            });

            // Configure AzureAdAuthOptions from configuration
            builder.Services.Configure<AzureAdAuthOptions>(builder.Configuration.GetSection("AzureAd"));

            // Register Blazored.LocalStorage services
            builder.Services.AddBlazoredLocalStorage();

            // Register Blazored.Toast services
            builder.Services.AddBlazoredToast();

            // Register LocalStorageHelper as a singleton
            builder.Services.AddSingleton<LocalStorageHelper>();

            // Register TokenAuthenticationStateProvider as a singleton and as AuthenticationStateProvider
            builder.Services.AddSingleton<TokenAuthenticationStateProvider>();
            builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<TokenAuthenticationStateProvider>());

            // Register HttpClientFactory as a singleton
            builder.Services.AddSingleton<HttpClientFactory>();

            // Register AuthorizationMessageHandler as a scoped service
            builder.Services.AddScoped(sp =>
            {
                var localStorage = sp.GetRequiredService<LocalStorageHelper>();
                var authStateProvider = sp.GetRequiredService<TokenAuthenticationStateProvider>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<AuthorizationMessageHandler>();
                var authorizedUrls = new[] { builder.HostEnvironment.BaseAddress }; // Authorize requests to the base address
                return new AuthorizationMessageHandler(localStorage, authStateProvider, logger, authorizedUrls);
            });

            // Register ApiClient as a scoped service
            builder.Services.AddScoped<ApiClient>();

            // Register all service interfaces and their implementations as scoped services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICountryService, CountryService>();
            builder.Services.AddScoped<IPricingService, PricingService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IRuleService, RuleService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Configure HTTP client with base address and authorization handler
            builder.Services.AddHttpClient("BackendAPI", client =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            })
            .AddHttpMessageHandler<AuthorizationMessageHandler>();

            builder.Services.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            })
           .AddHttpMessageHandler<AuthorizationMessageHandler>();
        }
    }
}