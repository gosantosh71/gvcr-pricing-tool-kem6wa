using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Constants;
using VatFilingPricingTool.Domain.Exceptions;

namespace VatFilingPricingTool.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a country code using ISO 3166-1 alpha-2 standard.
    /// Ensures that country codes are properly validated, formatted, and can be used consistently
    /// throughout the application for identifying VAT jurisdictions.
    /// </summary>
    public class CountryCode
    {
        /// <summary>
        /// Gets the country code value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Creates a new CountryCode instance with validation.
        /// </summary>
        /// <param name="value">The country code value.</param>
        /// <exception cref="ValidationException">Thrown when the country code is invalid.</exception>
        private CountryCode(string value)
        {
            Validate(value);
            Value = value;
        }

        /// <summary>
        /// Factory method to create a new CountryCode instance.
        /// </summary>
        /// <param name="value">The country code value.</param>
        /// <returns>A new CountryCode instance.</returns>
        /// <exception cref="ValidationException">Thrown when the country code is invalid.</exception>
        public static CountryCode Create(string value)
        {
            return new CountryCode(value);
        }

        /// <summary>
        /// Factory method to create a new CountryCode instance with the default country code.
        /// </summary>
        /// <returns>A new CountryCode instance with the default country code.</returns>
        public static CountryCode CreateDefault()
        {
            return new CountryCode(DomainConstants.Defaults.DefaultCountryCode);
        }

        /// <summary>
        /// Validates that a country code value is properly formatted and supported.
        /// </summary>
        /// <param name="value">The country code value to validate.</param>
        /// <exception cref="ValidationException">Thrown when the country code is invalid.</exception>
        private static void Validate(string value)
        {
            List<string> validationErrors = new List<string>();

            if (string.IsNullOrEmpty(value))
            {
                validationErrors.Add($"Country code cannot be null or empty.");
                throw new ValidationException("Invalid country code", validationErrors);
            }

            if (value.Length != DomainConstants.Validation.CountryCodeLength)
            {
                validationErrors.Add($"Country code must be exactly {DomainConstants.Validation.CountryCodeLength} characters.");
                throw new ValidationException("Invalid country code", validationErrors);
            }

            if (!Regex.IsMatch(value, "^[A-Z]{2}$"))
            {
                validationErrors.Add("Country code must consist of two uppercase letters.");
                throw new ValidationException("Invalid country code", validationErrors);
            }

            if (!DomainConstants.CountrySpecific.CountryCurrencies.ContainsKey(value))
            {
                validationErrors.Add($"Country code '{value}' is not supported.");
                throw new ValidationException("Country not supported", validationErrors);
            }
        }

        /// <summary>
        /// Returns the string representation of the country code.
        /// </summary>
        /// <returns>The country code value.</returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the objects are equal, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (!(obj is CountryCode other))
                return false;

            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Value?.ToUpperInvariant().GetHashCode() ?? 0;
        }

        /// <summary>
        /// Equality operator for comparing two CountryCode instances.
        /// </summary>
        /// <param name="left">The first CountryCode to compare.</param>
        /// <param name="right">The second CountryCode to compare.</param>
        /// <returns>True if the country codes are equal, otherwise false.</returns>
        public static bool operator ==(CountryCode left, CountryCode right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for comparing two CountryCode instances.
        /// </summary>
        /// <param name="left">The first CountryCode to compare.</param>
        /// <param name="right">The second CountryCode to compare.</param>
        /// <returns>True if the country codes are not equal, otherwise false.</returns>
        public static bool operator !=(CountryCode left, CountryCode right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implicit conversion from CountryCode to string.
        /// </summary>
        /// <param name="countryCode">The CountryCode to convert.</param>
        /// <returns>The string value of the country code.</returns>
        public static implicit operator string(CountryCode countryCode)
        {
            return countryCode?.Value;
        }

        /// <summary>
        /// Explicit conversion from string to CountryCode.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>A new CountryCode instance.</returns>
        /// <exception cref="ValidationException">Thrown when the country code is invalid.</exception>
        public static explicit operator CountryCode(string value)
        {
            return Create(value);
        }

        /// <summary>
        /// Gets the currency code associated with this country.
        /// </summary>
        /// <returns>The currency code for the country.</returns>
        public string GetCurrencyCode()
        {
            if (DomainConstants.CountrySpecific.CountryCurrencies.TryGetValue(Value, out string currencyCode))
            {
                return currencyCode;
            }

            return DomainConstants.Defaults.DefaultCurrency;
        }

        /// <summary>
        /// Gets the standard VAT rate for this country.
        /// </summary>
        /// <returns>The standard VAT rate for the country.</returns>
        public decimal GetStandardVatRate()
        {
            if (DomainConstants.CountrySpecific.StandardVatRates.TryGetValue(Value, out decimal vatRate))
            {
                return vatRate;
            }

            return 0.0m;
        }
    }
}