using Microsoft.JSInterop; // Microsoft.JSInterop v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0

namespace VatFilingPricingTool.Web.Helpers
{
    /// <summary>
    /// Helper class that provides methods for interacting with JavaScript functions from C# code in a Blazor WebAssembly application
    /// </summary>
    public static class JsInterop
    {
        /// <summary>
        /// Initializes a Chart.js chart on a specified canvas element
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="elementId">The ID of the canvas element</param>
        /// <param name="chartData">The data for the chart</param>
        /// <param name="chartOptions">The configuration options for the chart</param>
        /// <returns>A task representing the asynchronous operation, containing the chart instance</returns>
        public static async ValueTask<object> InitializeChartAsync(IJSRuntime jsRuntime, string elementId, object chartData, object chartOptions)
        {
            return await jsRuntime.InvokeAsync<object>("initializeChart", elementId, chartData, chartOptions);
        }

        /// <summary>
        /// Updates an existing chart with new data
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="chartInstance">The chart instance to update</param>
        /// <param name="newData">The new data for the chart</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async ValueTask UpdateChartAsync(IJSRuntime jsRuntime, object chartInstance, object newData)
        {
            await jsRuntime.InvokeVoidAsync("updateChart", chartInstance, newData);
        }

        /// <summary>
        /// Destroys a Chart.js instance and removes it from the global registry
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="elementId">The ID of the canvas element</param>
        /// <returns>A task representing the asynchronous operation, containing true if chart was successfully destroyed</returns>
        public static async ValueTask<bool> DestroyChartAsync(IJSRuntime jsRuntime, string elementId)
        {
            return await jsRuntime.InvokeAsync<bool>("destroyChart", elementId);
        }

        /// <summary>
        /// Triggers a file download in the browser
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="fileName">The name of the file to download</param>
        /// <param name="base64Content">The base64-encoded content of the file</param>
        /// <param name="contentType">The MIME type of the file</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async ValueTask DownloadFileAsync(IJSRuntime jsRuntime, string fileName, string base64Content, string contentType)
        {
            await jsRuntime.InvokeVoidAsync("downloadFile", fileName, base64Content, contentType);
        }

        /// <summary>
        /// Displays a toast notification message to the user
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="message">The message to display</param>
        /// <param name="type">The type of toast (e.g., "success", "error", "warning", "info")</param>
        /// <param name="durationMs">The duration in milliseconds to show the toast</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async ValueTask ShowToastAsync(IJSRuntime jsRuntime, string message, string type, int durationMs)
        {
            await jsRuntime.InvokeVoidAsync("showToast", message, type, durationMs);
        }

        /// <summary>
        /// Scrolls the page to bring a specific element into view
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="elementId">The ID of the element to scroll to</param>
        /// <param name="smooth">Whether to use smooth scrolling animation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async ValueTask ScrollToElementAsync(IJSRuntime jsRuntime, string elementId, bool smooth)
        {
            await jsRuntime.InvokeVoidAsync("scrollToElement", elementId, smooth);
        }

        /// <summary>
        /// Prints a specific element on the page
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="elementId">The ID of the element to print</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async ValueTask PrintElementAsync(IJSRuntime jsRuntime, string elementId)
        {
            await jsRuntime.InvokeVoidAsync("printElement", elementId);
        }

        /// <summary>
        /// Stores a value in the browser's local storage
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="key">The key to store the value under</param>
        /// <param name="value">The value to store</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async ValueTask SetLocalStorageItemAsync(IJSRuntime jsRuntime, string key, string value)
        {
            await jsRuntime.InvokeVoidAsync("setLocalStorageItem", key, value);
        }

        /// <summary>
        /// Retrieves a value from the browser's local storage
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="key">The key of the value to retrieve</param>
        /// <returns>A task representing the asynchronous operation, containing the stored value or null if not found</returns>
        public static async ValueTask<string> GetLocalStorageItemAsync(IJSRuntime jsRuntime, string key)
        {
            return await jsRuntime.InvokeAsync<string>("getLocalStorageItem", key);
        }

        /// <summary>
        /// Removes an item from the browser's local storage
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="key">The key of the item to remove</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async ValueTask RemoveLocalStorageItemAsync(IJSRuntime jsRuntime, string key)
        {
            await jsRuntime.InvokeVoidAsync("removeLocalStorageItem", key);
        }

        /// <summary>
        /// Clears all items from the browser's local storage
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async ValueTask ClearLocalStorageAsync(IJSRuntime jsRuntime)
        {
            await jsRuntime.InvokeVoidAsync("clearLocalStorage");
        }

        /// <summary>
        /// Toggles between light and dark theme modes
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="isDarkMode">Whether to enable dark mode (true) or light mode (false)</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async ValueTask ToggleDarkModeAsync(IJSRuntime jsRuntime, bool isDarkMode)
        {
            await jsRuntime.InvokeVoidAsync("toggleDarkMode", isDarkMode);
        }

        /// <summary>
        /// Formats a number as a currency string with the specified currency code
        /// </summary>
        /// <param name="jsRuntime">The JavaScript runtime instance</param>
        /// <param name="amount">The amount to format</param>
        /// <param name="currencyCode">The ISO currency code (e.g., "USD", "EUR", "GBP")</param>
        /// <returns>A task representing the asynchronous operation, containing the formatted currency string</returns>
        public static async ValueTask<string> FormatCurrencyAsync(IJSRuntime jsRuntime, decimal amount, string currencyCode)
        {
            return await jsRuntime.InvokeAsync<string>("formatCurrency", amount, currencyCode);
        }
    }
}