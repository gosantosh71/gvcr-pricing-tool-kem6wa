using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations v6.0.0
using System.Text.Json.Serialization; // System.Text.Json v6.0.0
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Responses
{
    /// <summary>
    /// Response model for a country with VAT filing requirements
    /// </summary>
    public class CountryResponse
    {
        /// <summary>
        /// Gets or sets the country code (ISO 3166-1 alpha-2 format).
        /// </summary>
        /// <example>UK, DE, FR</example>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the full name of the country.
        /// </summary>
        /// <example>United Kingdom, Germany, France</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the standard VAT rate applied in the country.
        /// </summary>
        /// <example>20.00, 19.00, 21.00</example>
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// Gets or sets the currency code used in the country (ISO 4217 format).
        /// </summary>
        /// <example>GBP, EUR, USD</example>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the available filing frequencies for VAT returns in this country.
        /// </summary>
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
        /// Initializes a new instance of the <see cref="CountryResponse"/> class.
        /// </summary>
        public CountryResponse()
        {
            AvailableFilingFrequencies = new List<FilingFrequency>();
            IsActive = true;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Response model for a paginated list of countries
    /// </summary>
    public class CountriesResponse
    {
        /// <summary>
        /// Gets or sets the list of country items in the current page.
        /// </summary>
        public List<CountryResponse> Items { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of items across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a next page.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountriesResponse"/> class.
        /// </summary>
        public CountriesResponse()
        {
            Items = new List<CountryResponse>();
            PageNumber = 1;
            PageSize = 10;
            TotalCount = 0;
            TotalPages = 0;
            HasPreviousPage = false;
            HasNextPage = false;
        }
    }

    /// <summary>
    /// Response model for simplified country information used in dropdowns and selection components
    /// </summary>
    public class CountrySummaryResponse
    {
        /// <summary>
        /// Gets or sets the country code (ISO 3166-1 alpha-2 format).
        /// </summary>
        /// <example>UK, DE, FR</example>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the full name of the country.
        /// </summary>
        /// <example>United Kingdom, Germany, France</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the standard VAT rate applied in the country.
        /// </summary>
        /// <example>20.00, 19.00, 21.00</example>
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this country is active in the system.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountrySummaryResponse"/> class.
        /// </summary>
        public CountrySummaryResponse()
        {
            IsActive = true;
        }
    }

    /// <summary>
    /// Response model for country creation operations
    /// </summary>
    public class CreateCountryResponse
    {
        /// <summary>
        /// Gets or sets the country code of the created country.
        /// </summary>
        /// <example>UK, DE, FR</example>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the created country.
        /// </summary>
        /// <example>United Kingdom, Germany, France</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result of the operation.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the list of errors that occurred during the operation.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCountryResponse"/> class.
        /// </summary>
        public CreateCountryResponse()
        {
            Errors = new List<string>();
            Success = false;
        }
    }

    /// <summary>
    /// Response model for country update operations
    /// </summary>
    public class UpdateCountryResponse
    {
        /// <summary>
        /// Gets or sets the country code of the updated country.
        /// </summary>
        /// <example>UK, DE, FR</example>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the updated country.
        /// </summary>
        /// <example>United Kingdom, Germany, France</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this country data was last updated.
        /// </summary>
        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result of the operation.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the list of errors that occurred during the operation.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCountryResponse"/> class.
        /// </summary>
        public UpdateCountryResponse()
        {
            Errors = new List<string>();
            Success = false;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Response model for country deletion operations
    /// </summary>
    public class DeleteCountryResponse
    {
        /// <summary>
        /// Gets or sets the country code of the deleted country.
        /// </summary>
        /// <example>UK, DE, FR</example>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result of the operation.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCountryResponse"/> class.
        /// </summary>
        public DeleteCountryResponse()
        {
            Success = false;
        }
    }
}