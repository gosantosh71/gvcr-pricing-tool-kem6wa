using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.Threading.Tasks; // Version 6.0.0
using VatFilingPricingTool.Domain.Entities; // The Country entity that this repository manages
using VatFilingPricingTool.Domain.Enums; // Enum for filtering countries by filing frequency support

namespace VatFilingPricingTool.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Country entity operations, providing data access methods 
    /// for country-specific VAT information.
    /// </summary>
    public interface ICountryRepository : IRepository<Country>
    {
        /// <summary>
        /// Retrieves a country by its country code
        /// </summary>
        /// <param name="countryCode">The country code to search for</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the country if found, null otherwise</returns>
        Task<Country> GetByCodeAsync(string countryCode);

        /// <summary>
        /// Retrieves all active countries
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of active countries</returns>
        Task<IEnumerable<Country>> GetActiveCountriesAsync();

        /// <summary>
        /// Retrieves countries by their country codes
        /// </summary>
        /// <param name="countryCodes">The collection of country codes to search for</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of countries matching the provided codes</returns>
        Task<IEnumerable<Country>> GetCountriesByCodesAsync(IEnumerable<string> countryCodes);

        /// <summary>
        /// Retrieves countries that support a specific filing frequency
        /// </summary>
        /// <param name="filingFrequency">The filing frequency to filter by</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of countries supporting the specified filing frequency</returns>
        Task<IEnumerable<Country>> GetCountriesByFilingFrequencyAsync(FilingFrequency filingFrequency);

        /// <summary>
        /// Retrieves a paginated list of countries with optional filtering for active countries only
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="activeOnly">If true, only active countries will be included</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated collection of countries and the total count</returns>
        Task<(IEnumerable<Country> Countries, int TotalCount)> GetPagedCountriesAsync(int pageNumber, int pageSize, bool activeOnly);

        /// <summary>
        /// Checks if a country with the specified country code exists
        /// </summary>
        /// <param name="countryCode">The country code to check</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether a matching country exists</returns>
        Task<bool> ExistsByCodeAsync(string countryCode);

        /// <summary>
        /// Creates a new country in the repository
        /// </summary>
        /// <param name="country">The country to create</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created country</returns>
        Task<Country> CreateAsync(Country country);

        /// <summary>
        /// Updates an existing country in the repository
        /// </summary>
        /// <param name="country">The country to update</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated country</returns>
        Task<Country> UpdateAsync(Country country);

        /// <summary>
        /// Deletes a country with the specified country code
        /// </summary>
        /// <param name="countryCode">The country code of the country to delete</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the deletion was successful</returns>
        Task<bool> DeleteByCodeAsync(string countryCode);
    }
}