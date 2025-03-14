using System; // System 6.0.0 - Core .NET functionality
using System.Collections.Generic; // System.Collections.Generic 6.0.0 - For collection types like List and Dictionary
using System.Globalization; // System.Globalization 6.0.0 - For culture-specific formatting
using System.Linq; // System.Linq 6.0.0 - For LINQ extension methods
using System.Net.Http; // System.Net.Http 6.0.0 - For HTTP client extensions
using System.Text; // System.Text 6.0.0 - For string manipulation utilities
using System.Text.Json; // System.Text.Json 6.0.0 - For JSON serialization extensions
using System.Text.RegularExpressions; // For regex operations
using System.Threading.Tasks; // For async operations

namespace VatFilingPricingTool.Web.Utils
{
    /// <summary>
    /// Static class containing extension methods for various types
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method to format a decimal as a currency string
        /// </summary>
        /// <param name="value">The decimal value to format</param>
        /// <param name="currencyCode">The currency code (e.g., USD, EUR)</param>
        /// <returns>Formatted currency string</returns>
        public static string FormatAsCurrency(this decimal value, string currencyCode)
        {
            // Create a CultureInfo based on the current culture
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            
            // Set the CultureInfo's NumberFormat.CurrencySymbol to the currency symbol for the given currency code
            culture.NumberFormat.CurrencySymbol = currencyCode;
            
            // Return the value formatted as currency using the modified CultureInfo
            return value.ToString("C", culture);
        }

        /// <summary>
        /// Extension method to format a decimal as a percentage string
        /// </summary>
        /// <param name="value">The decimal value to format</param>
        /// <returns>Formatted percentage string</returns>
        public static string FormatAsPercentage(this decimal value)
        {
            // Multiply the value by 100
            // Format the result with the percentage format from FormatConstants.PercentageFormat
            return string.Format(Constants.FormatConstants.PercentageFormat, value);
        }

        /// <summary>
        /// Extension method to truncate a string to a maximum length
        /// </summary>
        /// <param name="value">The string to truncate</param>
        /// <param name="maxLength">The maximum length</param>
        /// <returns>Truncated string with ellipsis if needed</returns>
        public static string TruncateString(this string value, int maxLength)
        {
            // Check if the string is null or empty, return the original string if true
            if (string.IsNullOrEmpty(value))
                return value;
            
            // Check if the string length is less than or equal to maxLength, return the original string if true
            if (value.Length <= maxLength)
                return value;
            
            // Truncate the string to maxLength - 3 characters
            // Append '...' to the truncated string
            return value.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Extension method to convert a string to title case
        /// </summary>
        /// <param name="value">The string to convert</param>
        /// <returns>Title-cased string</returns>
        public static string ToTitleCase(this string value)
        {
            // Check if the string is null or empty, return the original string if true
            if (string.IsNullOrEmpty(value))
                return value;
            
            // Create a TextInfo object from the current culture
            // Convert the string to title case using TextInfo.ToTitleCase
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Extension method to split a camelCase or PascalCase string into words
        /// </summary>
        /// <param name="value">The string to split</param>
        /// <returns>Space-separated words</returns>
        public static string SplitCamelCase(this string value)
        {
            // Check if the string is null or empty, return the original string if true
            if (string.IsNullOrEmpty(value))
                return value;
            
            // Use regex to insert spaces before uppercase letters
            string result = Regex.Replace(value, "([A-Z])", " $1");
            
            // Trim any leading space that might have been added
            return result.TrimStart();
        }

        /// <summary>
        /// Extension method to validate an email address
        /// </summary>
        /// <param name="email">The email to validate</param>
        /// <returns>True if the email is valid, otherwise false</returns>
        public static bool IsValidEmail(this string email)
        {
            // Check if the string is null or empty, return false if true
            if (string.IsNullOrEmpty(email))
                return false;
            
            // Use regex pattern from ValidationConstants.EmailRegexPattern to validate the email
            return Regex.IsMatch(email, Constants.ValidationConstants.EmailRegexPattern);
        }

        /// <summary>
        /// Extension method to validate a password
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>True if the password is valid, otherwise false</returns>
        public static bool IsValidPassword(this string password)
        {
            // Check if the string is null, return false if true
            if (password == null)
                return false;
            
            // Check if the password length is between MinPasswordLength and MaxPasswordLength
            if (password.Length < Constants.ValidationConstants.MinPasswordLength || 
                password.Length > Constants.ValidationConstants.MaxPasswordLength)
                return false;
            
            // Use regex pattern from ValidationConstants.PasswordRegexPattern to validate the password
            return Regex.IsMatch(password, Constants.ValidationConstants.PasswordRegexPattern);
        }

        /// <summary>
        /// Extension method to convert a dictionary to a URL query string
        /// </summary>
        /// <param name="parameters">The dictionary of parameters</param>
        /// <returns>URL query string</returns>
        public static string ToQueryString(this Dictionary<string, string> parameters)
        {
            // Check if the dictionary is null or empty, return empty string if true
            if (parameters == null || !parameters.Any())
                return string.Empty;
            
            // Join the key-value pairs with '=' and '&' separators
            // URL encode the keys and values
            var queryString = string.Join("&", parameters.Select(p => 
                $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            
            // Prepend '?' to the result
            return "?" + queryString;
        }

        /// <summary>
        /// Extension method to add query parameters to a URL
        /// </summary>
        /// <param name="url">The base URL</param>
        /// <param name="parameters">The dictionary of parameters</param>
        /// <returns>URL with added query parameters</returns>
        public static string AddQueryString(this string url, Dictionary<string, string> parameters)
        {
            // Check if the URL is null or empty, return the original URL if true
            if (string.IsNullOrEmpty(url))
                return url;
            
            // Check if the parameters dictionary is null or empty, return the original URL if true
            if (parameters == null || !parameters.Any())
                return url;
            
            // Check if the URL already contains a query string (has '?')
            var separator = url.Contains("?") ? "&" : "?";
            
            // Convert the parameters to a query string without the leading '?'
            var queryString = parameters.ToQueryString().TrimStart('?');
            
            // Append the query string to the URL with the appropriate separator
            return url + separator + queryString;
        }

        /// <summary>
        /// Extension method to set a timeout on an HttpClient
        /// </summary>
        /// <param name="client">The HttpClient</param>
        /// <param name="timeout">The timeout TimeSpan</param>
        /// <returns>HttpClient with the specified timeout</returns>
        public static HttpClient WithTimeout(this HttpClient client, TimeSpan timeout)
        {
            // Set the client's Timeout property to the specified timeout
            client.Timeout = timeout;
            
            // Return the modified client
            return client;
        }

        /// <summary>
        /// Extension method to serialize an object to JSON
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="indented">Whether to indent the JSON output</param>
        /// <returns>JSON string</returns>
        public static string ToJson(this object obj, bool indented = false)
        {
            // Create JsonSerializerOptions with camelCase naming policy
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = indented
            };
            
            // Serialize the object to JSON using System.Text.Json.JsonSerializer
            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// Extension method to deserialize JSON to an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="json">The JSON string</param>
        /// <returns>Deserialized object</returns>
        public static T FromJson<T>(this string json)
        {
            // Check if the JSON string is null or empty, return default(T) if true
            if (string.IsNullOrEmpty(json))
                return default;
            
            // Create JsonSerializerOptions with camelCase naming policy
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            // Deserialize the JSON string to an object of type T using System.Text.Json.JsonSerializer
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Extension method to convert a string to Base64
        /// </summary>
        /// <param name="value">The string to convert</param>
        /// <returns>Base64-encoded string</returns>
        public static string ToBase64(this string value)
        {
            // Check if the string is null or empty, return empty string if true
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            
            // Convert the string to bytes using UTF-8 encoding
            var bytes = Encoding.UTF8.GetBytes(value);
            
            // Convert the bytes to a Base64 string
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Extension method to decode a Base64 string
        /// </summary>
        /// <param name="base64">The Base64 string to decode</param>
        /// <returns>Decoded string</returns>
        public static string FromBase64(this string base64)
        {
            // Check if the Base64 string is null or empty, return empty string if true
            if (string.IsNullOrEmpty(base64))
                return string.Empty;
            
            try
            {
                // Convert the Base64 string to bytes
                var bytes = Convert.FromBase64String(base64);
                
                // Convert the bytes to a string using UTF-8 encoding
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // If there's an error (like invalid Base64), return empty string
                return string.Empty;
            }
        }

        /// <summary>
        /// Extension method to safely get a substring
        /// </summary>
        /// <param name="value">The string to get a substring from</param>
        /// <param name="startIndex">The start index</param>
        /// <param name="length">The length of the substring</param>
        /// <returns>Substring or empty string</returns>
        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            // Check if the string is null or empty, return empty string if true
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            
            // Check if startIndex is negative or greater than the string length, return empty string if true
            if (startIndex < 0 || startIndex >= value.Length)
                return string.Empty;
            
            // Calculate the actual length to use (minimum of requested length and available characters)
            var actualLength = Math.Min(length, value.Length - startIndex);
            
            // Return the substring with the calculated parameters
            return value.Substring(startIndex, actualLength);
        }

        /// <summary>
        /// Extension method to convert a string to a URL-friendly slug
        /// </summary>
        /// <param name="value">The string to convert</param>
        /// <returns>URL-friendly slug</returns>
        public static string ToSlug(this string value)
        {
            // Check if the string is null or empty, return empty string if true
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            
            // Convert the string to lowercase
            var slug = value.ToLowerInvariant();
            
            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");
            
            // Remove any characters that are not letters, numbers, or hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            
            // Remove consecutive hyphens
            slug = Regex.Replace(slug, @"-+", "-");
            
            // Trim hyphens from the beginning and end
            slug = slug.Trim('-');
            
            return slug;
        }
    }

    /// <summary>
    /// Extension methods for HttpClient to simplify common operations
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Sends a GET request and deserializes the JSON response
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="client">The HttpClient</param>
        /// <param name="requestUri">The request URI</param>
        /// <returns>Deserialized response object</returns>
        public static async Task<T> GetJsonAsync<T>(this HttpClient client, string requestUri)
        {
            // Send a GET request to the specified URI
            var response = await client.GetAsync(requestUri);
            
            // Ensure a successful response
            response.EnsureSuccessStatusCode();
            
            // Read the response content as a string
            var json = await response.Content.ReadAsStringAsync();
            
            // Deserialize the JSON response to type T
            return json.FromJson<T>();
        }

        /// <summary>
        /// Sends a POST request with JSON content and deserializes the response
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="client">The HttpClient</param>
        /// <param name="requestUri">The request URI</param>
        /// <param name="content">The content to send</param>
        /// <returns>Deserialized response object</returns>
        public static async Task<T> PostJsonAsync<T>(this HttpClient client, string requestUri, object content)
        {
            // Serialize the content object to JSON
            var json = content.ToJson();
            
            // Create StringContent with the JSON and application/json media type
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Send a POST request to the specified URI with the JSON content
            var response = await client.PostAsync(requestUri, stringContent);
            
            // Ensure a successful response
            response.EnsureSuccessStatusCode();
            
            // Read the response content as a string
            var responseJson = await response.Content.ReadAsStringAsync();
            
            // Deserialize the JSON response to type T
            return responseJson.FromJson<T>();
        }

        /// <summary>
        /// Sends a PUT request with JSON content and deserializes the response
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="client">The HttpClient</param>
        /// <param name="requestUri">The request URI</param>
        /// <param name="content">The content to send</param>
        /// <returns>Deserialized response object</returns>
        public static async Task<T> PutJsonAsync<T>(this HttpClient client, string requestUri, object content)
        {
            // Serialize the content object to JSON
            var json = content.ToJson();
            
            // Create StringContent with the JSON and application/json media type
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Send a PUT request to the specified URI with the JSON content
            var response = await client.PutAsync(requestUri, stringContent);
            
            // Ensure a successful response
            response.EnsureSuccessStatusCode();
            
            // Read the response content as a string
            var responseJson = await response.Content.ReadAsStringAsync();
            
            // Deserialize the JSON response to type T
            return responseJson.FromJson<T>();
        }

        /// <summary>
        /// Sends a DELETE request and deserializes the JSON response
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="client">The HttpClient</param>
        /// <param name="requestUri">The request URI</param>
        /// <returns>Deserialized response object</returns>
        public static async Task<T> DeleteJsonAsync<T>(this HttpClient client, string requestUri)
        {
            // Send a DELETE request to the specified URI
            var response = await client.DeleteAsync(requestUri);
            
            // Ensure a successful response
            response.EnsureSuccessStatusCode();
            
            // Read the response content as a string
            var json = await response.Content.ReadAsStringAsync();
            
            // Deserialize the JSON response to type T
            return json.FromJson<T>();
        }
    }

    /// <summary>
    /// Extension methods for IEnumerable<T> to provide additional functionality
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Checks if an IEnumerable<T> is null or empty
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="source">The IEnumerable<T> to check</param>
        /// <returns>True if the source is null or empty, otherwise false</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            // Check if the source is null
            if (source == null)
                return true;
            
            // If not null, check if the source contains any elements using Any()
            return !source.Any();
        }

        /// <summary>
        /// Performs an action on each element of an IEnumerable<T>
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="source">The IEnumerable<T> to iterate over</param>
        /// <param name="action">The action to perform on each element</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            // Check if the source is null or the action is null, return if either is null
            if (source == null || action == null)
                return;
            
            // Iterate through each element in the source
            foreach (var item in source)
            {
                // Perform the specified action on each element
                action(item);
            }
        }

        /// <summary>
        /// Joins the elements of an IEnumerable<T> into a delimited string
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="source">The IEnumerable<T> to join</param>
        /// <param name="delimiter">The delimiter to use between elements</param>
        /// <returns>Delimited string of elements</returns>
        public static string ToDelimitedString<T>(this IEnumerable<T> source, string delimiter = ", ")
        {
            // Check if the source is null or empty, return empty string if true
            if (source.IsNullOrEmpty())
                return string.Empty;
            
            // Convert each element to string and join with the specified delimiter
            return string.Join(delimiter, source);
        }
    }
}