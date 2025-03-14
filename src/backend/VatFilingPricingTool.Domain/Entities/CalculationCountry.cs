using System;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Domain.Entities
{
    /// <summary>
    /// Represents the relationship between a calculation and a country with country-specific cost information.
    /// This entity stores the cost breakdown for each country included in a VAT filing calculation.
    /// </summary>
    public class CalculationCountry
    {
        /// <summary>
        /// Gets the ID of the calculation this country is part of
        /// </summary>
        public string CalculationId { get; private set; }

        /// <summary>
        /// Gets the country code representing the VAT jurisdiction
        /// </summary>
        public CountryCode CountryCode { get; private set; }

        /// <summary>
        /// Gets the country-specific cost for this VAT filing
        /// </summary>
        public Money CountryCost { get; private set; }

        /// <summary>
        /// Gets a comma-separated list of rule IDs that were applied to calculate the cost
        /// </summary>
        public string AppliedRules { get; private set; }

        /// <summary>
        /// Gets or sets the navigation property to the parent calculation
        /// </summary>
        public Calculation Calculation { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the country
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        /// Private constructor to enforce creation through factory methods
        /// </summary>
        private CalculationCountry()
        {
            AppliedRules = string.Empty;
        }

        /// <summary>
        /// Creates a new CalculationCountry instance with validation
        /// </summary>
        /// <param name="calculationId">The ID of the parent calculation</param>
        /// <param name="countryCode">The country code (ISO 3166-1 alpha-2)</param>
        /// <param name="countryCost">The country-specific cost</param>
        /// <returns>A new validated CalculationCountry instance</returns>
        /// <exception cref="ValidationException">Thrown when input parameters are invalid</exception>
        public static CalculationCountry Create(string calculationId, string countryCode, Money countryCost)
        {
            Validate(calculationId, countryCode, countryCost);

            var calculationCountry = new CalculationCountry
            {
                CalculationId = calculationId,
                CountryCode = CountryCode.Create(countryCode),
                CountryCost = countryCost
            };

            return calculationCountry;
        }

        /// <summary>
        /// Adds a rule ID to the list of applied rules for this country calculation
        /// </summary>
        /// <param name="ruleId">The ID of the rule that was applied</param>
        /// <exception cref="ValidationException">Thrown when the rule ID is invalid</exception>
        public void AddAppliedRule(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                throw new ValidationException("Rule ID cannot be null or empty.",
                    new System.Collections.Generic.List<string> { "Invalid rule ID" });
            }

            if (string.IsNullOrEmpty(AppliedRules))
            {
                AppliedRules = ruleId;
            }
            else
            {
                AppliedRules += $",{ruleId}";
            }
        }

        /// <summary>
        /// Updates the country-specific cost for this calculation
        /// </summary>
        /// <param name="newCost">The new cost value</param>
        /// <exception cref="ValidationException">Thrown when the new cost is invalid</exception>
        public void UpdateCost(Money newCost)
        {
            if (newCost == null)
            {
                throw new ValidationException("Cost cannot be null.",
                    new System.Collections.Generic.List<string> { "Invalid cost" });
            }

            if (CountryCost != null && newCost.Currency != CountryCost.Currency)
            {
                throw new ValidationException("Currency mismatch when updating cost.",
                    new System.Collections.Generic.List<string> { $"Currency mismatch: {CountryCost.Currency} and {newCost.Currency}" });
            }

            CountryCost = newCost;
        }

        /// <summary>
        /// Gets the list of rule IDs that were applied to this country calculation
        /// </summary>
        /// <returns>Array of rule IDs that were applied</returns>
        public string[] GetAppliedRules()
        {
            if (string.IsNullOrEmpty(AppliedRules))
            {
                return Array.Empty<string>();
            }

            return AppliedRules.Split(',');
        }

        /// <summary>
        /// Checks if a specific rule was applied to this country calculation
        /// </summary>
        /// <param name="ruleId">The rule ID to check</param>
        /// <returns>True if the rule was applied, false otherwise</returns>
        public bool HasRuleApplied(string ruleId)
        {
            var appliedRules = GetAppliedRules();
            return Array.IndexOf(appliedRules, ruleId) >= 0;
        }

        /// <summary>
        /// Validates the calculation country data according to business rules
        /// </summary>
        /// <param name="calculationId">The calculation ID to validate</param>
        /// <param name="countryCode">The country code to validate</param>
        /// <param name="countryCost">The country cost to validate</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        private static void Validate(string calculationId, string countryCode, Money countryCost)
        {
            if (string.IsNullOrEmpty(calculationId))
            {
                throw new ValidationException("Calculation ID cannot be null or empty.", 
                    new System.Collections.Generic.List<string> { ErrorCodes.Pricing.InvalidParameters });
            }

            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ValidationException("Country code cannot be null or empty.", 
                    new System.Collections.Generic.List<string> { ErrorCodes.Country.InvalidCountryCode });
            }

            if (countryCost == null)
            {
                throw new ValidationException("Country cost cannot be null.", 
                    new System.Collections.Generic.List<string> { ErrorCodes.Pricing.InvalidParameters });
            }
        }
    }
}