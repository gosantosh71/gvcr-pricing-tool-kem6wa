using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations v6.0.0
using System.Text.Json.Serialization; // System.Text.Json v6.0.0
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Models
{
    /// <summary>
    /// Represents a country with VAT filing requirements for pricing calculations.
    /// This model serves as a contract between the API and clients for country data.
    /// </summary>
    public class CountryModel
    {
        /// <summary>
        /// Gets or sets the country code (ISO 3166-1 alpha-2 format).
        /// </summary>
        /// <example>UK, DE, FR</example>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the full name of the country.
        /// </summary>
        /// <example>United Kingdom, Germany, France</example>
        [Required(ErrorMessage = "Country name is required")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the standard VAT rate applied in the country.
        /// </summary>
        /// <example>20.00, 19.00, 21.00</example>
        [Required(ErrorMessage = "Standard VAT rate is required")]
        [Range(0, 100, ErrorMessage = "Standard VAT rate must be between 0 and 100")]
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// Gets or sets the currency code used in the country (ISO 4217 format).
        /// </summary>
        /// <example>GBP, EUR, USD</example>
        [Required(ErrorMessage = "Currency code is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the available filing frequencies for VAT returns in this country.
        /// </summary>
        [Required(ErrorMessage = "At least one filing frequency must be specified")]
        public List<FilingFrequency> AvailableFilingFrequencies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this country is active in the system.
        /// Inactive countries are not included in pricing calculations.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this country data was last updated.
        /// </summary>
        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountryModel"/> class.
        /// </summary>
        public CountryModel()
        {
            AvailableFilingFrequencies = new List<FilingFrequency>();
            IsActive = true;
            LastUpdated = DateTime.UtcNow;
        }
    }
}