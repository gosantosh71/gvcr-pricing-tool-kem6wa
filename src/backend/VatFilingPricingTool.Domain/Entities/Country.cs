using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Constants;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Domain.Entities
{
    /// <summary>
    /// Represents a country or VAT jurisdiction with specific tax rates and filing requirements.
    /// This entity is a core domain model enabling country-specific pricing calculations and rule applications.
    /// </summary>
    public class Country
    {
        /// <summary>
        /// Gets the unique country code that identifies this VAT jurisdiction.
        /// </summary>
        public CountryCode Code { get; private set; }

        /// <summary>
        /// Gets the full name of the country.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the standard VAT rate applied in this country.
        /// </summary>
        public VatRate StandardVatRate { get; private set; }

        /// <summary>
        /// Gets the three-letter ISO currency code used in this country.
        /// </summary>
        public string CurrencyCode { get; private set; }

        /// <summary>
        /// Gets the collection of filing frequencies available in this country.
        /// </summary>
        public HashSet<FilingFrequency> AvailableFilingFrequencies { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this country is active for VAT filing calculations.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets the date and time when this country information was last updated.
        /// </summary>
        public DateTime LastUpdated { get; private set; }

        /// <summary>
        /// Gets the collection of VAT rules specific to this country.
        /// </summary>
        public ICollection<Rule> Rules { get; private set; }

        /// <summary>
        /// Gets the collection of calculations that include this country.
        /// </summary>
        public ICollection<CalculationCountry> CalculationCountries { get; private set; }

        /// <summary>
        /// Private constructor for the Country entity to enforce creation through factory methods
        /// </summary>
        private Country()
        {
            AvailableFilingFrequencies = new HashSet<FilingFrequency>();
            Rules = new List<Rule>();
            CalculationCountries = new List<CalculationCountry>();
            IsActive = true;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a new Country instance with validation
        /// </summary>
        /// <param name="countryCode">ISO code for the country (e.g., "GB" for United Kingdom)</param>
        /// <param name="name">Full name of the country</param>
        /// <param name="standardVatRate">Standard VAT rate as a decimal percentage</param>
        /// <param name="currencyCode">Three-letter ISO currency code</param>
        /// <returns>A new validated Country instance</returns>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static Country Create(string countryCode, string name, decimal standardVatRate, string currencyCode)
        {
            Validate(countryCode, name, standardVatRate, currencyCode);

            var country = new Country
            {
                Code = CountryCode.Create(countryCode),
                Name = name,
                StandardVatRate = VatRate.Create(standardVatRate),
                CurrencyCode = currencyCode.ToUpperInvariant()
            };

            return country;
        }

        /// <summary>
        /// Adds a filing frequency to the country's available filing frequencies
        /// </summary>
        /// <param name="frequency">The filing frequency to add</param>
        public void AddFilingFrequency(FilingFrequency frequency)
        {
            AvailableFilingFrequencies.Add(frequency);
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes a filing frequency from the country's available filing frequencies
        /// </summary>
        /// <param name="frequency">The filing frequency to remove</param>
        public void RemoveFilingFrequency(FilingFrequency frequency)
        {
            AvailableFilingFrequencies.Remove(frequency);
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the country name
        /// </summary>
        /// <param name="newName">The new name for the country</param>
        /// <exception cref="ValidationException">Thrown when the new name is invalid</exception>
        public void UpdateName(string newName)
        {
            List<string> validationErrors = new List<string>();

            if (string.IsNullOrEmpty(newName))
            {
                validationErrors.Add("Country name cannot be null or empty");
            }
            else if (newName.Length > DomainConstants.Validation.MaxNameLength)
            {
                validationErrors.Add($"Country name cannot exceed {DomainConstants.Validation.MaxNameLength} characters");
            }

            if (validationErrors.Count > 0)
            {
                throw new ValidationException("Invalid country name", validationErrors);
            }

            Name = newName;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the standard VAT rate for the country
        /// </summary>
        /// <param name="newRate">The new VAT rate as a decimal percentage</param>
        /// <exception cref="ValidationException">Thrown when the new VAT rate is invalid</exception>
        public void UpdateStandardVatRate(decimal newRate)
        {
            // VatRate value object will perform validation
            StandardVatRate = VatRate.Create(newRate);
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the currency code for the country
        /// </summary>
        /// <param name="newCurrencyCode">The new three-letter ISO currency code</param>
        /// <exception cref="ValidationException">Thrown when the new currency code is invalid</exception>
        public void UpdateCurrencyCode(string newCurrencyCode)
        {
            List<string> validationErrors = new List<string>();

            if (string.IsNullOrEmpty(newCurrencyCode))
            {
                validationErrors.Add("Currency code cannot be null or empty");
            }
            else if (newCurrencyCode.Length != DomainConstants.Validation.CurrencyCodeLength)
            {
                validationErrors.Add($"Currency code must be exactly {DomainConstants.Validation.CurrencyCodeLength} characters");
            }
            else if (!Regex.IsMatch(newCurrencyCode, DomainConstants.Validation.CurrencyCodePattern))
            {
                validationErrors.Add("Currency code must consist of three uppercase letters");
            }

            if (validationErrors.Count > 0)
            {
                throw new ValidationException("Invalid currency code", validationErrors);
            }

            CurrencyCode = newCurrencyCode.ToUpperInvariant();
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the country as active or inactive
        /// </summary>
        /// <param name="active">True to set as active, false to set as inactive</param>
        public void SetActive(bool active)
        {
            IsActive = active;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the country supports a specific filing frequency
        /// </summary>
        /// <param name="frequency">The filing frequency to check</param>
        /// <returns>True if the country supports the specified filing frequency, false otherwise</returns>
        public bool SupportsFilingFrequency(FilingFrequency frequency)
        {
            return AvailableFilingFrequencies.Contains(frequency);
        }

        /// <summary>
        /// Validates country data according to business rules
        /// </summary>
        /// <param name="countryCode">The country code to validate</param>
        /// <param name="name">The country name to validate</param>
        /// <param name="standardVatRate">The standard VAT rate to validate</param>
        /// <param name="currencyCode">The currency code to validate</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        private static void Validate(string countryCode, string name, decimal standardVatRate, string currencyCode)
        {
            List<string> validationErrors = new List<string>();

            // Country code will be further validated by the CountryCode value object
            if (string.IsNullOrEmpty(countryCode))
            {
                validationErrors.Add("Country code cannot be null or empty");
            }

            if (string.IsNullOrEmpty(name))
            {
                validationErrors.Add("Country name cannot be null or empty");
            }
            else if (name.Length > DomainConstants.Validation.MaxNameLength)
            {
                validationErrors.Add($"Country name cannot exceed {DomainConstants.Validation.MaxNameLength} characters");
            }

            // VAT rate will be further validated by the VatRate value object
            if (standardVatRate < DomainConstants.Validation.MinVatRate || standardVatRate > DomainConstants.Validation.MaxVatRate)
            {
                validationErrors.Add($"VAT rate must be between {DomainConstants.Validation.MinVatRate}% and {DomainConstants.Validation.MaxVatRate}%");
            }

            if (string.IsNullOrEmpty(currencyCode))
            {
                validationErrors.Add("Currency code cannot be null or empty");
            }
            else if (currencyCode.Length != DomainConstants.Validation.CurrencyCodeLength)
            {
                validationErrors.Add($"Currency code must be exactly {DomainConstants.Validation.CurrencyCodeLength} characters");
            }
            else if (!Regex.IsMatch(currencyCode, DomainConstants.Validation.CurrencyCodePattern))
            {
                validationErrors.Add("Currency code must consist of three uppercase letters");
            }

            if (validationErrors.Count > 0)
            {
                throw new ValidationException("Country validation failed", validationErrors);
            }
        }
    }
}