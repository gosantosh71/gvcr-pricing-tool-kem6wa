using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using VatFilingPricingTool.Common.Models.Result;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Service.Interfaces
{
    /// <summary>
    /// Service interface for country-related operations in the VAT Filing Pricing Tool.
    /// Provides methods for retrieving, creating, updating, and deleting country data
    /// with their VAT filing requirements for pricing calculations.
    /// </summary>
    public interface ICountryService
    {
        /// <summary>
        /// Retrieves a country by its country code.
        /// </summary>
        /// <param name="request">The request containing the country code to retrieve.</param>
        /// <returns>A result containing the country data if found, or an error if not found.</returns>
        Task<Result<CountryResponse>> GetCountryAsync(GetCountryRequest request);

        /// <summary>
        /// Retrieves a paginated list of countries with optional filtering.
        /// </summary>
        /// <param name="request">The request containing pagination and filtering parameters.</param>
        /// <returns>A result containing the paginated list of countries.</returns>
        Task<Result<CountriesResponse>> GetCountriesAsync(GetCountriesRequest request);

        /// <summary>
        /// Retrieves all active countries.
        /// </summary>
        /// <returns>A result containing a list of all active countries.</returns>
        Task<Result<List<CountryResponse>>> GetActiveCountriesAsync();

        /// <summary>
        /// Retrieves countries that support a specific filing frequency.
        /// </summary>
        /// <param name="filingFrequency">The filing frequency to filter by.</param>
        /// <returns>A result containing a list of countries that support the specified filing frequency.</returns>
        Task<Result<List<CountryResponse>>> GetCountriesByFilingFrequencyAsync(FilingFrequency filingFrequency);

        /// <summary>
        /// Retrieves a simplified list of countries for dropdown menus and selection components.
        /// </summary>
        /// <returns>A result containing a list of simplified country information.</returns>
        Task<Result<List<CountrySummaryResponse>>> GetCountrySummariesAsync();

        /// <summary>
        /// Creates a new country with the specified details.
        /// </summary>
        /// <param name="request">The request containing the country details to create.</param>
        /// <returns>A result containing the creation response with the new country's details.</returns>
        Task<Result<CreateCountryResponse>> CreateCountryAsync(CreateCountryRequest request);

        /// <summary>
        /// Updates an existing country with the specified details.
        /// </summary>
        /// <param name="request">The request containing the updated country details.</param>
        /// <returns>A result containing the update response with the updated country's details.</returns>
        Task<Result<UpdateCountryResponse>> UpdateCountryAsync(UpdateCountryRequest request);

        /// <summary>
        /// Deletes a country with the specified country code.
        /// </summary>
        /// <param name="request">The request containing the country code to delete.</param>
        /// <returns>A result containing the deletion response.</returns>
        Task<Result<DeleteCountryResponse>> DeleteCountryAsync(DeleteCountryRequest request);

        /// <summary>
        /// Checks if a country with the specified country code exists.
        /// </summary>
        /// <param name="countryCode">The country code to check.</param>
        /// <returns>True if the country exists, false otherwise.</returns>
        Task<bool> CountryExistsAsync(string countryCode);
    }
}