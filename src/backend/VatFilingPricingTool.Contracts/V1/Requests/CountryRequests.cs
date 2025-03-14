using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations v6.0.0
using System.Text.Json.Serialization; // System.Text.Json v6.0.0
using VatFilingPricingTool.Domain.Enums; // For FilingFrequency enum

namespace VatFilingPricingTool.Contracts.V1.Requests
{
    /// <summary>
    /// Request model for creating a new country with VAT filing requirements.
    /// </summary>
    public class CreateCountryRequest
    {
        /// <summary>
        /// ISO country code (e.g., "GB" for United Kingdom).
        /// </summary>
        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Full name of the country.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Standard VAT rate applied in the country (e.g., 20.00 for 20%).
        /// </summary>
        [Required]
        [Range(0, 100, ErrorMessage = "Standard VAT rate must be between 0 and 100")]
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// ISO currency code used in the country (e.g., "EUR" for Euro).
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// List of available filing frequencies in the country.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one filing frequency must be specified")]
        public List<FilingFrequency> AvailableFilingFrequencies { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCountryRequest"/> class.
        /// </summary>
        public CreateCountryRequest()
        {
            AvailableFilingFrequencies = new List<FilingFrequency>();
        }
    }

    /// <summary>
    /// Request model for updating an existing country with VAT filing requirements.
    /// </summary>
    public class UpdateCountryRequest
    {
        /// <summary>
        /// ISO country code (e.g., "GB" for United Kingdom). Cannot be modified once created.
        /// </summary>
        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Full name of the country.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Standard VAT rate applied in the country (e.g., 20.00 for 20%).
        /// </summary>
        [Required]
        [Range(0, 100, ErrorMessage = "Standard VAT rate must be between 0 and 100")]
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// ISO currency code used in the country (e.g., "EUR" for Euro).
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// List of available filing frequencies in the country.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one filing frequency must be specified")]
        public List<FilingFrequency> AvailableFilingFrequencies { get; set; }

        /// <summary>
        /// Indicates whether the country is active in the system.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCountryRequest"/> class.
        /// </summary>
        public UpdateCountryRequest()
        {
            AvailableFilingFrequencies = new List<FilingFrequency>();
            IsActive = true;
        }
    }

    /// <summary>
    /// Request model for retrieving a specific country by its country code.
    /// </summary>
    public class GetCountryRequest
    {
        /// <summary>
        /// ISO country code (e.g., "GB" for United Kingdom).
        /// </summary>
        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }
    }

    /// <summary>
    /// Request model for retrieving a filtered and paginated list of countries.
    /// </summary>
    public class GetCountriesRequest
    {
        /// <summary>
        /// Flag to retrieve only active countries.
        /// </summary>
        public bool ActiveOnly { get; set; }

        /// <summary>
        /// Optional list of country codes to filter results.
        /// If empty, returns countries based on other criteria.
        /// </summary>
        public List<string> CountryCodes { get; set; }

        /// <summary>
        /// Page number for pagination (1-based).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1")]
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page for pagination.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetCountriesRequest"/> class.
        /// </summary>
        public GetCountriesRequest()
        {
            CountryCodes = new List<string>();
            ActiveOnly = true;
            Page = 1;
            PageSize = 10;
        }
    }

    /// <summary>
    /// Request model for deleting a country by its country code.
    /// </summary>
    public class DeleteCountryRequest
    {
        /// <summary>
        /// ISO country code (e.g., "GB" for United Kingdom).
        /// </summary>
        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }
    }
}