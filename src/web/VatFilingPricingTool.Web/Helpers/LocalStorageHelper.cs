using System; // System v6.0.0
using System.Text.Json; // System.Text.Json v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using Microsoft.JSInterop; // Microsoft.JSInterop v6.0.0
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Helpers
{
    /// <summary>
    /// Helper class that provides methods for storing and retrieving data from browser local storage,
    /// with a focus on authentication data
    /// </summary>
    public class LocalStorageHelper
    {
        private readonly IJSRuntime _jsRuntime;
        
        // Constants for local storage keys
        private const string AUTH_TOKEN_KEY = "vat_filing_auth_token";
        private const string REFRESH_TOKEN_KEY = "vat_filing_refresh_token";
        private const string TOKEN_EXPIRATION_KEY = "vat_filing_token_expiration";
        private const string USER_DATA_KEY = "vat_filing_user_data";
        private const string CALCULATION_HISTORY_KEY = "vat_filing_calculation_history";
        private const string USER_PREFERENCES_KEY = "vat_filing_user_preferences";
        
        /// <summary>
        /// Initializes a new instance of the LocalStorageHelper class with the required dependencies
        /// </summary>
        /// <param name="jsRuntime">JavaScript runtime instance for interop operations</param>
        public LocalStorageHelper(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        
        /// <summary>
        /// Saves authentication data to local storage
        /// </summary>
        /// <param name="authData">Authentication data to save</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveAuthDataAsync(AuthSuccessResponse authData)
        {
            if (authData == null)
                return;
                
            await SetItemAsync(AUTH_TOKEN_KEY, authData.Token);
            await SetItemAsync(REFRESH_TOKEN_KEY, authData.RefreshToken);
            await SetItemAsync(TOKEN_EXPIRATION_KEY, authData.ExpiresAt.ToString("o"));
            
            string userData = JsonSerializer.Serialize(authData.User);
            await SetItemAsync(USER_DATA_KEY, userData);
        }
        
        /// <summary>
        /// Retrieves the authentication token from local storage
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the authentication token or null if not found</returns>
        public async Task<string> GetAuthTokenAsync()
        {
            return await GetItemAsync(AUTH_TOKEN_KEY);
        }
        
        /// <summary>
        /// Retrieves the refresh token from local storage
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the refresh token or null if not found</returns>
        public async Task<string> GetRefreshTokenAsync()
        {
            return await GetItemAsync(REFRESH_TOKEN_KEY);
        }
        
        /// <summary>
        /// Retrieves the token expiration date from local storage
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the token expiration date or null if not found</returns>
        public async Task<DateTime?> GetTokenExpirationAsync()
        {
            string expirationStr = await GetItemAsync(TOKEN_EXPIRATION_KEY);
            
            if (string.IsNullOrEmpty(expirationStr))
                return null;
                
            if (DateTime.TryParse(expirationStr, out DateTime expiration))
                return expiration;
                
            return null;
        }
        
        /// <summary>
        /// Retrieves the user data from local storage
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the user data or null if not found</returns>
        public async Task<UserModel> GetUserDataAsync()
        {
            string userDataJson = await GetItemAsync(USER_DATA_KEY);
            
            if (string.IsNullOrEmpty(userDataJson))
                return null;
                
            try
            {
                return JsonSerializer.Deserialize<UserModel>(userDataJson);
            }
            catch (JsonException)
            {
                return null;
            }
        }
        
        /// <summary>
        /// Clears all authentication data from local storage
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ClearAuthDataAsync()
        {
            await RemoveItemAsync(AUTH_TOKEN_KEY);
            await RemoveItemAsync(REFRESH_TOKEN_KEY);
            await RemoveItemAsync(TOKEN_EXPIRATION_KEY);
            await RemoveItemAsync(USER_DATA_KEY);
        }
        
        /// <summary>
        /// Saves calculation history to local storage
        /// </summary>
        /// <param name="calculationHistory">JSON string representing calculation history</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveCalculationHistoryAsync(string calculationHistory)
        {
            await SetItemAsync(CALCULATION_HISTORY_KEY, calculationHistory);
        }
        
        /// <summary>
        /// Retrieves calculation history from local storage
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the calculation history or null if not found</returns>
        public async Task<string> GetCalculationHistoryAsync()
        {
            return await GetItemAsync(CALCULATION_HISTORY_KEY);
        }
        
        /// <summary>
        /// Saves user preferences to local storage
        /// </summary>
        /// <param name="preferences">JSON string representing user preferences</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveUserPreferencesAsync(string preferences)
        {
            await SetItemAsync(USER_PREFERENCES_KEY, preferences);
        }
        
        /// <summary>
        /// Retrieves user preferences from local storage
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the user preferences or null if not found</returns>
        public async Task<string> GetUserPreferencesAsync()
        {
            return await GetItemAsync(USER_PREFERENCES_KEY);
        }
        
        /// <summary>
        /// Sets an item in local storage with the specified key and value
        /// </summary>
        /// <param name="key">The key to store the value under</param>
        /// <param name="value">The value to store</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task SetItemAsync(string key, string value)
        {
            await JsInterop.SetLocalStorageItemAsync(_jsRuntime, key, value);
        }
        
        /// <summary>
        /// Gets an item from local storage with the specified key
        /// </summary>
        /// <param name="key">The key of the value to retrieve</param>
        /// <returns>A task representing the asynchronous operation, containing the value or null if not found</returns>
        private async Task<string> GetItemAsync(string key)
        {
            return await JsInterop.GetLocalStorageItemAsync(_jsRuntime, key);
        }
        
        /// <summary>
        /// Removes an item from local storage with the specified key
        /// </summary>
        /// <param name="key">The key of the item to remove</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task RemoveItemAsync(string key)
        {
            await JsInterop.RemoveLocalStorageItemAsync(_jsRuntime, key);
        }
        
        /// <summary>
        /// Clears all items from local storage
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ClearAsync()
        {
            await JsInterop.ClearLocalStorageAsync(_jsRuntime);
        }
    }
}