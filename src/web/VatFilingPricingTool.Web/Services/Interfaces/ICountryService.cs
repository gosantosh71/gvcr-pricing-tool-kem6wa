using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Services.Interfaces
{
    /// <summary>
    /// Defines the interface for country-related services in the VAT Filing Pricing Tool web application.
    /// This interface provides methods for retrieving country data, managing country selection,
    /// and supporting multi-country VAT filing pricing calculations.
    /// </summary>
    public interface ICountryService
    {
        /// <summary>
        /// Retrieves a specific country by its country code
        /// </summary>
        /// <param name="countryCode">The ISO country code (e.g., "GB" for United Kingdom)</param>
        /// <returns>The country model for the specified country code, or null if not found</returns>
        Task<CountryModel> GetCountryAsync(string countryCode);

        /// <summary>
        /// Retrieves a list of countries with optional filtering for active countries only
        /// </summary>
        /// <param name="activeOnly">When true, returns only active countries; otherwise, returns all countries</param>
        /// <returns>List of countries matching the filter criteria</returns>
        Task<List<CountryModel>> GetCountriesAsync(bool activeOnly = true);

        /// <summary>
        /// Retrieves all active countries
        /// </summary>
        /// <returns>List of active countries</returns>
        Task<List<CountryModel>> GetActiveCountriesAsync();

        /// <summary>
        /// Retrieves countries that support a specific filing frequency
        /// </summary>
        /// <param name="filingFrequency">The filing frequency to filter by</param>
        /// <returns>List of countries supporting the specified filing frequency</returns>
        Task<List<CountryModel>> GetCountriesByFilingFrequencyAsync(int filingFrequency);

        /// <summary>
        /// Retrieves a simplified list of countries for dropdown menus and selection components
        /// </summary>
        /// <returns>List of country summaries</returns>
        Task<List<CountrySummaryModel>> GetCountrySummariesAsync();

        /// <summary>
        /// Initializes a country selection model with available countries for the selection UI
        /// </summary>
        /// <returns>Initialized country selection model</returns>
        Task<CountrySelectionModel> InitializeCountrySelectionAsync();

        /// <summary>
        /// Searches for countries matching the provided search term
        /// </summary>
        /// <param name="searchTerm">The search term to match against country names</param>
        /// <returns>List of country options matching the search term</returns>
        Task<List<CountryOption>> SearchCountriesAsync(string searchTerm);

        /// <summary>
        /// Retrieves detailed country models for the selected country codes
        /// </summary>
        /// <param name="countryCodes">List of country codes to retrieve details for</param>
        /// <returns>List of country models for the selected country codes</returns>
        Task<List<CountryModel>> GetSelectedCountriesAsync(List<string> countryCodes);
    }
}