using System; // System 6.0.0 - Core .NET functionality
using System.Globalization; // System.Globalization 6.0.0 - For culture-specific formatting of numbers and currencies
using VatFilingPricingTool.Domain.Enums; // VatFilingPricingTool.Domain 1.0.0 - For accessing domain enum types like ServiceType and FilingFrequency
using VatFilingPricingTool.Web.Utils; // For accessing EnumExtensions

namespace VatFilingPricingTool.Web.Helpers
{
    /// <summary>
    /// Static helper class that provides formatting utilities for pricing-related data
    /// in the VAT Filing Pricing Tool web application.
    /// Ensures consistent formatting of pricing data across UI components.
    /// </summary>
    public static class PricingFormatter
    {
        /// <summary>
        /// Private constructor to prevent instantiation of static class
        /// </summary>
        private PricingFormatter()
        {
            // Private constructor to prevent instantiation
        }

        /// <summary>
        /// Formats a decimal value as currency with the appropriate currency symbol
        /// </summary>
        /// <param name="value">Decimal value to format</param>
        /// <param name="currencyCode">ISO currency code (e.g., EUR, USD, GBP)</param>
        /// <returns>Formatted currency string with symbol and appropriate decimal places</returns>
        public static string FormatCurrency(decimal value, string currencyCode)
        {
            // Default to EUR if currency code is null or empty
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                currencyCode = "EUR";
            }

            try
            {
                // Create a CultureInfo based on the currency code
                CultureInfo culture = GetCultureInfoForCurrency(currencyCode);
                
                // Format the decimal value using the currency format string
                return string.Format(culture, "{0:C}", value);
            }
            catch (Exception)
            {
                // Fallback to basic formatting if there's an error
                string symbol = GetCurrencySymbol(currencyCode);
                return $"{symbol}{value:N2}";
            }
        }

        /// <summary>
        /// Formats a service type enum value as a user-friendly display name
        /// </summary>
        /// <param name="serviceType">Service type enum value as integer</param>
        /// <returns>User-friendly display name for the service type</returns>
        public static string FormatServiceType(int serviceType)
        {
            ServiceType serviceTypeEnum = (ServiceType)serviceType;
            return serviceTypeEnum.GetServiceTypeDisplayName();
        }

        /// <summary>
        /// Formats a filing frequency enum value as a user-friendly display name
        /// </summary>
        /// <param name="filingFrequency">Filing frequency enum value as integer</param>
        /// <returns>User-friendly display name for the filing frequency</returns>
        public static string FormatFilingFrequency(int filingFrequency)
        {
            FilingFrequency filingFrequencyEnum = (FilingFrequency)filingFrequency;
            return filingFrequencyEnum.GetFilingFrequencyDisplayName();
        }

        /// <summary>
        /// Formats a decimal value as a percentage with the appropriate symbol
        /// </summary>
        /// <param name="value">Decimal value to format (e.g., 0.2 for 20%)</param>
        /// <returns>Formatted percentage string with % symbol</returns>
        public static string FormatPercentage(decimal value)
        {
            // Multiply by 100 to convert to percentage value
            value = value * 100;
            
            // Format with 2 decimal places and add % symbol
            return $"{value:F2}%";
        }

        /// <summary>
        /// Formats a transaction volume with thousands separators
        /// </summary>
        /// <param name="volume">Transaction volume as integer</param>
        /// <returns>Formatted transaction volume with thousands separators</returns>
        public static string FormatTransactionVolume(int volume)
        {
            // Format the integer with thousands separators using current culture
            return volume.ToString("N0", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the currency symbol for a given currency code
        /// </summary>
        /// <param name="currencyCode">ISO currency code (e.g., EUR, USD, GBP)</param>
        /// <returns>Currency symbol for the specified currency code</returns>
        public static string GetCurrencySymbol(string currencyCode)
        {
            // Default to EUR if currency code is null or empty
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                currencyCode = "EUR";
            }

            try
            {
                // Get the appropriate CultureInfo for the currency code
                CultureInfo culture = GetCultureInfoForCurrency(currencyCode);
                
                // Return the currency symbol from the NumberFormatInfo
                return culture.NumberFormat.CurrencySymbol;
            }
            catch (Exception)
            {
                // Fallback to common currency symbols if an error occurs
                switch (currencyCode.ToUpper())
                {
                    case "USD": return "$";
                    case "EUR": return "€";
                    case "GBP": return "£";
                    case "JPY": return "¥";
                    case "AUD": return "A$";
                    case "CAD": return "C$";
                    case "CHF": return "Fr";
                    case "CNY": return "¥";
                    case "SEK": return "kr";
                    case "NOK": return "kr";
                    case "DKK": return "kr";
                    default: return "€"; // Default to Euro symbol
                }
            }
        }

        /// <summary>
        /// Formats a discount name for display in the UI
        /// </summary>
        /// <param name="discountKey">Internal discount key</param>
        /// <returns>User-friendly display name for the discount</returns>
        public static string FormatDiscountName(string discountKey)
        {
            if (string.IsNullOrWhiteSpace(discountKey))
            {
                return string.Empty;
            }

            switch (discountKey.ToLower())
            {
                case "volume_discount":
                    return "Volume Discount";
                case "multi_country_discount":
                    return "Multi-Country Discount";
                case "loyalty_discount":
                    return "Loyalty Discount";
                default:
                    // Convert from camel/snake case to proper case with spaces
                    return ConvertToProperCase(discountKey);
            }
        }

        /// <summary>
        /// Gets a CultureInfo based on a currency code
        /// </summary>
        /// <param name="currencyCode">ISO currency code</param>
        /// <returns>CultureInfo for formatting</returns>
        private static CultureInfo GetCultureInfoForCurrency(string currencyCode)
        {
            // Map common currency codes to their associated culture
            switch (currencyCode.ToUpper())
            {
                case "USD": return new CultureInfo("en-US");
                case "EUR": return new CultureInfo("fr-FR"); // Using French culture for Euro
                case "GBP": return new CultureInfo("en-GB");
                case "JPY": return new CultureInfo("ja-JP");
                case "AUD": return new CultureInfo("en-AU");
                case "CAD": return new CultureInfo("en-CA");
                case "CHF": return new CultureInfo("de-CH");
                case "CNY": return new CultureInfo("zh-CN");
                case "SEK": return new CultureInfo("sv-SE");
                case "NOK": return new CultureInfo("nb-NO");
                case "DKK": return new CultureInfo("da-DK");
                case "PLN": return new CultureInfo("pl-PL");
                case "CZK": return new CultureInfo("cs-CZ");
                case "HUF": return new CultureInfo("hu-HU");
                default: return new CultureInfo("fr-FR"); // Default to Euro
            }
        }

        /// <summary>
        /// Converts a camel case or snake case string to proper case with spaces
        /// </summary>
        /// <param name="input">Input string in camel case or snake case</param>
        /// <returns>Proper case string with spaces</returns>
        private static string ConvertToProperCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Replace underscores with spaces
            input = input.Replace('_', ' ');
            
            // Insert spaces before uppercase letters
            for (int i = input.Length - 2; i >= 0; i--)
            {
                if (char.IsLower(input[i]) && char.IsUpper(input[i + 1]))
                {
                    input = input.Insert(i + 1, " ");
                }
            }
            
            // Capitalize first letter of each word
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }
    }
}