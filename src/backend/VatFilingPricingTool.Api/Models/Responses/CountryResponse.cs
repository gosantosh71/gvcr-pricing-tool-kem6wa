using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations v6.0.0
using System.Text.Json.Serialization; // System.Text.Json v6.0.0
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Models.Responses
{
    /// <summary>
    /// Response model for a country with VAT filing requirements, used in API responses.
    /// This model standardizes country information for client consumption.
    /// </summary>
    public class CountryResponse
    {
        /// <summary>
        /// Gets or sets the country code (ISO 3166-1 alpha-2 format).
        /// </summary>
        /// <example>UK, DE, FR</example>
        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the full name of the country.
        /// </summary>
        /// <example>United Kingdom, Germany, France</example>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the standard VAT rate applied in the country.
        /// </summary>
        /// <example>20.00, 19.00, 21.00</example>
        [JsonPropertyName("standardVatRate")]
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// Gets or sets the currency code used in the country (ISO 4217 format).
        /// </summary>
        /// <example>GBP, EUR, USD</example>
        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the available filing frequencies for VAT returns in this country.
        /// </summary>
        [JsonPropertyName("availableFilingFrequencies")]
        public List<FilingFrequency> AvailableFilingFrequencies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this country is active in the system.
        /// Inactive countries are not included in pricing calculations.
        /// </summary>
        [JsonPropertyName("isActive")]
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

        /// <summary>
        /// Creates a CountryResponse from a CountryModel.
        /// </summary>
        /// <param name="model">The country model to convert.</param>
        /// <returns>A new CountryResponse populated with data from the CountryModel.</returns>
        /// <exception cref="ArgumentNullException">Thrown when model is null.</exception>
        public static CountryResponse FromCountryModel(CountryModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Country model cannot be null.");
            }

            return new CountryResponse
            {
                CountryCode = model.CountryCode,
                Name = model.Name,
                StandardVatRate = model.StandardVatRate,
                CurrencyCode = model.CurrencyCode,
                AvailableFilingFrequencies = new List<FilingFrequency>(model.AvailableFilingFrequencies),
                IsActive = model.IsActive,
                LastUpdated = model.LastUpdated
            };
        }
    }

    /// <summary>
    /// Response model for a paginated list of countries.
    /// Includes pagination metadata along with the country items.
    /// </summary>
    public class CountriesResponse
    {
        /// <summary>
        /// Gets or sets the list of country items in the current page.
        /// </summary>
        [JsonPropertyName("items")]
        public List<CountryResponse> Items { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of items across all pages.
        /// </summary>
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a previous page available.
        /// </summary>
        [JsonPropertyName("hasPreviousPage")]
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a next page available.
        /// </summary>
        [JsonPropertyName("hasNextPage")]
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
    /// Response model for simplified country information used in dropdowns and selection components.
    /// Contains only essential information needed for country selection.
    /// </summary>
    public class CountrySummaryResponse
    {
        /// <summary>
        /// Gets or sets the country code (ISO 3166-1 alpha-2 format).
        /// </summary>
        /// <example>UK, DE, FR</example>
        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the full name of the country.
        /// </summary>
        /// <example>United Kingdom, Germany, France</example>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the standard VAT rate applied in the country.
        /// </summary>
        /// <example>20.00, 19.00, 21.00</example>
        [JsonPropertyName("standardVatRate")]
        public decimal StandardVatRate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this country is active in the system.
        /// </summary>
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountrySummaryResponse"/> class.
        /// </summary>
        public CountrySummaryResponse()
        {
            IsActive = true;
        }

        /// <summary>
        /// Creates a CountrySummaryResponse from a CountryResponse.
        /// </summary>
        /// <param name="response">The country response to convert.</param>
        /// <returns>A new CountrySummaryResponse with essential data from the CountryResponse.</returns>
        /// <exception cref="ArgumentNullException">Thrown when response is null.</exception>
        public static CountrySummaryResponse FromCountryResponse(CountryResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response), "Country response cannot be null.");
            }

            return new CountrySummaryResponse
            {
                CountryCode = response.CountryCode,
                Name = response.Name,
                StandardVatRate = response.StandardVatRate,
                IsActive = response.IsActive
            };
        }
    }
}