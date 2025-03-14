using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VatFilingPricingTool.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for string manipulation, validation, and formatting used throughout the VAT Filing Pricing Tool application.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if a string is null or empty.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is null or empty, otherwise false.</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Checks if a string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is null, empty, or consists only of white-space characters, otherwise false.</returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Checks if a string has a value (is not null or empty).
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string has a value, otherwise false.</returns>
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Safely trims a string, handling null values.
        /// </summary>
        /// <param name="value">The string to trim.</param>
        /// <returns>The trimmed string or an empty string if the input is null.</returns>
        public static string SafeTrim(this string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        /// <summary>
        /// Safely gets a substring, handling null values and out-of-range indices.
        /// </summary>
        /// <param name="value">The string to get a substring from.</param>
        /// <param name="startIndex">The starting character position of the substring.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>The substring or an empty string if the input is null or indices are out of range.</returns>
        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (startIndex < 0 || startIndex >= value.Length)
                return string.Empty;

            int maxLength = value.Length - startIndex;
            length = Math.Min(length, maxLength);

            return value.Substring(startIndex, length);
        }

        /// <summary>
        /// Truncates a string to a specified length and adds an ellipsis if truncated.
        /// </summary>
        /// <param name="value">The string to truncate.</param>
        /// <param name="maxLength">The maximum length of the returned string including the ellipsis.</param>
        /// <returns>The truncated string with ellipsis if needed, or the original string if it's shorter than maxLength.</returns>
        public static string TruncateWithEllipsis(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Converts a string to title case using the current culture.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The string converted to title case.</returns>
        public static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Converts a string to sentence case (first letter capitalized, rest lowercase).
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The string converted to sentence case.</returns>
        public static string ToSentenceCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            value = value.ToLower();
            return char.ToUpper(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// Removes all whitespace characters from a string.
        /// </summary>
        /// <param name="value">The string to process.</param>
        /// <returns>The string with all whitespace removed.</returns>
        public static string RemoveWhitespace(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(value, @"\s+", "");
        }

        /// <summary>
        /// Removes all special characters from a string, leaving only letters, numbers, and spaces.
        /// </summary>
        /// <param name="value">The string to process.</param>
        /// <returns>The string with all special characters removed.</returns>
        public static string RemoveSpecialCharacters(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(value, @"[^a-zA-Z0-9\s]", "");
        }

        /// <summary>
        /// Checks if a string is a valid email address format.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is a valid email address, otherwise false.</returns>
        public static bool IsValidEmail(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // Regular expression pattern for email validation
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(value, pattern);
        }

        /// <summary>
        /// Checks if a string is a valid ISO 3166-1 alpha-2 country code.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is a valid country code, otherwise false.</returns>
        public static bool IsValidCountryCode(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // Check if the string is exactly 2 characters long and contains only uppercase letters
            return value.Length == 2 && value.All(c => c >= 'A' && c <= 'Z');
        }

        /// <summary>
        /// Checks if a string is a valid VAT number format for a specific country.
        /// </summary>
        /// <param name="value">The VAT number to check.</param>
        /// <param name="countryCode">The ISO country code (e.g., "GB", "DE").</param>
        /// <returns>True if the string is a valid VAT number for the specified country, otherwise false.</returns>
        public static bool IsValidVatNumber(this string value, string countryCode)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(countryCode))
                return false;

            // VAT number format patterns for different countries
            var patterns = new Dictionary<string, string>
            {
                // UK: GB followed by 9 or 12 digits
                {"GB", @"^GB[0-9]{9}([0-9]{3})?$"},
                // Germany: DE followed by 9 digits
                {"DE", @"^DE[0-9]{9}$"},
                // France: FR followed by 2 characters and 9 digits
                {"FR", @"^FR[A-Z0-9]{2}[0-9]{9}$"},
                // Spain: ES followed by 9 characters
                {"ES", @"^ES[A-Z0-9]{9}$"},
                // Italy: IT followed by 11 digits
                {"IT", @"^IT[0-9]{11}$"},
                // Default pattern for other countries (basic check)
                {"DEFAULT", @"^[A-Z]{2}[A-Z0-9]{8,15}$"}
            };

            // Get the pattern for the specified country code or use the default pattern
            string pattern = patterns.ContainsKey(countryCode) ? patterns[countryCode] : patterns["DEFAULT"];
            
            return Regex.IsMatch(value, pattern);
        }

        /// <summary>
        /// Formats a decimal value as a currency string with the specified currency code.
        /// </summary>
        /// <param name="value">The decimal value to format.</param>
        /// <param name="currencyCode">The ISO currency code (e.g., "EUR", "USD").</param>
        /// <returns>The formatted currency string.</returns>
        public static string FormatAsCurrency(this decimal value, string currencyCode)
        {
            // Create a NumberFormatInfo with the specified currency symbol
            NumberFormatInfo nfi = new NumberFormatInfo
            {
                CurrencySymbol = currencyCode
            };

            // Format the value as currency
            return value.ToString("C", nfi);
        }

        /// <summary>
        /// Formats a decimal value as a percentage string.
        /// </summary>
        /// <param name="value">The decimal value to format.</param>
        /// <param name="decimalPlaces">The number of decimal places to display.</param>
        /// <returns>The formatted percentage string.</returns>
        public static string FormatAsPercentage(this decimal value, int decimalPlaces = 2)
        {
            // Convert to percentage (multiply by 100)
            decimal percentage = value * 100;
            
            // Format with the specified number of decimal places
            string format = $"F{decimalPlaces}";
            return percentage.ToString(format) + "%";
        }

        /// <summary>
        /// Converts a string to a URL-friendly slug.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The URL-friendly slug.</returns>
        public static string ToSlug(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Convert to lowercase
            value = value.ToLowerInvariant();
            
            // Replace spaces with hyphens
            value = value.Replace(" ", "-");
            
            // Remove all non-alphanumeric characters except hyphens
            value = Regex.Replace(value, @"[^a-z0-9\-]", "");
            
            // Remove consecutive hyphens
            value = Regex.Replace(value, @"-+", "-");
            
            // Trim hyphens from the beginning and end
            value = value.Trim('-');
            
            return value;
        }

        /// <summary>
        /// Splits a camelCase or PascalCase string into separate words.
        /// </summary>
        /// <param name="value">The string to split.</param>
        /// <returns>The string with spaces inserted between words.</returns>
        public static string SplitCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Insert a space before each uppercase letter that follows a lowercase letter
            return Regex.Replace(value, "([a-z])([A-Z])", "$1 $2").Trim();
        }

        /// <summary>
        /// Truncates a string to a specified length.
        /// </summary>
        /// <param name="value">The string to truncate.</param>
        /// <param name="maxLength">The maximum length of the returned string.</param>
        /// <returns>The truncated string or the original string if it's shorter than maxLength.</returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength);
        }

        /// <summary>
        /// Checks if a string contains another string, ignoring case.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="value">The string to search for.</param>
        /// <returns>True if the source string contains the value string (ignoring case), otherwise false.</returns>
        public static bool ContainsIgnoreCase(this string source, string value)
        {
            if (source == null || value == null)
                return false;

            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Checks if two strings are equal, ignoring case.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="value">The string to compare with.</param>
        /// <returns>True if the strings are equal (ignoring case), otherwise false.</returns>
        public static bool EqualsIgnoreCase(this string source, string value)
        {
            return string.Equals(source, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts a string to a Base64-encoded string.
        /// </summary>
        /// <param name="value">The string to encode.</param>
        /// <returns>The Base64-encoded string.</returns>
        public static string ToBase64(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts a Base64-encoded string back to a regular string.
        /// </summary>
        /// <param name="value">The Base64-encoded string to decode.</param>
        /// <returns>The decoded string.</returns>
        public static string FromBase64(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            try
            {
                byte[] bytes = Convert.FromBase64String(value);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                // Return empty string if the input is not a valid Base64 string
                return string.Empty;
            }
        }

        /// <summary>
        /// Masks sensitive data in a string, showing only the first and last characters.
        /// </summary>
        /// <param name="value">The string to mask.</param>
        /// <param name="visibleStartChars">The number of characters to show at the beginning.</param>
        /// <param name="visibleEndChars">The number of characters to show at the end.</param>
        /// <returns>The masked string.</returns>
        public static string MaskSensitiveData(this string value, int visibleStartChars = 2, int visibleEndChars = 2)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= visibleStartChars + visibleEndChars)
                return new string('*', value.Length);

            string start = value.Substring(0, visibleStartChars);
            string end = value.Substring(value.Length - visibleEndChars);
            int maskedLength = value.Length - visibleStartChars - visibleEndChars;
            string masked = new string('*', maskedLength);

            return start + masked + end;
        }
    }
}