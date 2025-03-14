using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations v6.0.0
using System.Text.Json.Serialization; // System.Text.Json v6.0.0
using VatFilingPricingTool.Domain.Enums; // FilingFrequency enum import

namespace VatFilingPricingTool.Api.Models.Requests
{
    /// <summary>
    /// Request model for creating a new country with VAT filing requirements.
    /// </summary>
    public class CreateCountryRequest
    {
        /// <summary>
        /// ISO country code (2 letter code, e.g., "GB" for United Kingdom).
        /// This is the primary identifier for the country.
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters")]
        [RegularExpression("^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Full name of the country.
        /// </summary>
        [Required(ErrorMessage = "Country name is required")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Standard VAT rate for the country, expressed as a percentage.
        /// </summary>
        [Required(ErrorMessage = "Standard VAT rate is required")]
        [Range(0, 100, ErrorMessage = "Standard VAT rate must be between 0 and 100")]
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// ISO currency code for the country (e.g., "EUR" for Euro).
        /// </summary>
        [Required(ErrorMessage = "Currency code is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters")]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// List of filing frequencies available for this country.
        /// </summary>
        [Required(ErrorMessage = "At least one filing frequency must be specified")]
        [MinLength(1, ErrorMessage = "At least one filing frequency must be specified")]
        public List<FilingFrequency> AvailableFilingFrequencies { get; set; }

        /// <summary>
        /// Default constructor for the CreateCountryRequest.
        /// Initializes the AvailableFilingFrequencies collection.
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
        /// ISO country code (2 letter code, e.g., "GB" for United Kingdom).
        /// This is the primary identifier for the country and cannot be changed.
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters")]
        [RegularExpression("^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Full name of the country.
        /// </summary>
        [Required(ErrorMessage = "Country name is required")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Standard VAT rate for the country, expressed as a percentage.
        /// </summary>
        [Required(ErrorMessage = "Standard VAT rate is required")]
        [Range(0, 100, ErrorMessage = "Standard VAT rate must be between 0 and 100")]
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// ISO currency code for the country (e.g., "EUR" for Euro).
        /// </summary>
        [Required(ErrorMessage = "Currency code is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters")]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// List of filing frequencies available for this country.
        /// </summary>
        [Required(ErrorMessage = "At least one filing frequency must be specified")]
        [MinLength(1, ErrorMessage = "At least one filing frequency must be specified")]
        public List<FilingFrequency> AvailableFilingFrequencies { get; set; }

        /// <summary>
        /// Indicates whether the country is active in the system.
        /// Inactive countries won't be shown in standard country selections.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Default constructor for the UpdateCountryRequest.
        /// Initializes the AvailableFilingFrequencies collection and sets IsActive to true.
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
        /// ISO country code (2 letter code, e.g., "GB" for United Kingdom).
        /// This is the primary identifier for the country.
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters")]
        [RegularExpression("^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters")]
        public string CountryCode { get; set; }
    }

    /// <summary>
    /// Request model for retrieving a filtered and paginated list of countries.
    /// </summary>
    public class GetCountriesRequest
    {
        /// <summary>
        /// Indicates whether to return only active countries.
        /// When true, inactive countries are excluded from the results.
        /// </summary>
        [JsonPropertyName("activeOnly")]
        public bool ActiveOnly { get; set; }

        /// <summary>
        /// Optional list of country codes to filter by.
        /// If provided, only countries with these codes will be returned.
        /// If empty, all countries (subject to ActiveOnly filter) will be returned.
        /// </summary>
        [JsonPropertyName("countryCodes")]
        public List<string> CountryCodes { get; set; }

        /// <summary>
        /// Page number for pagination (1-based).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1")]
        [JsonPropertyName("page")]
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page for pagination.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// Default constructor for the GetCountriesRequest.
        /// Initializes the CountryCodes collection and sets default pagination values.
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
        /// ISO country code (2 letter code, e.g., "GB" for United Kingdom).
        /// This is the primary identifier for the country to be deleted.
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters")]
        [RegularExpression("^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters")]
        public string CountryCode { get; set; }
    }
}