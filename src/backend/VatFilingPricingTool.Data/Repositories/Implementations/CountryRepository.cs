using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Data.Context; // Provides access to the database context
using VatFilingPricingTool.Data.Repositories.Interfaces; // Interface that this repository implements
using VatFilingPricingTool.Domain.Entities; // The Country entity that this repository manages
using VatFilingPricingTool.Domain.Enums; // Enum for filtering countries by filing frequency support

namespace VatFilingPricingTool.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Country entities that provides data access operations for country-specific VAT information
    /// </summary>
    public class CountryRepository : Repository<Country>, ICountryRepository
    {
        /// <summary>
        /// Initializes a new instance of the CountryRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Optional logger instance</param>
        public CountryRepository(IVatFilingDbContext context, ILogger<CountryRepository> logger = null)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves a country by its country code
        /// </summary>
        /// <param name="countryCode">The country code to search for</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the country if found, or null.</returns>
        public async Task<Country> GetByCodeAsync(string countryCode)
        {
            _logger?.LogInformation("Retrieving country with code {CountryCode}", countryCode);
            
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentNullException(nameof(countryCode), "Country code cannot be null or empty");
            }
            
            var country = await _dbSet
                .Include(c => c.AvailableFilingFrequencies)
                .FirstOrDefaultAsync(c => c.Code.Value.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
            
            _logger?.LogInformation("Country with code {CountryCode} {Result}", 
                countryCode, country != null ? "found" : "not found");
            
            return country;
        }

        /// <summary>
        /// Retrieves all active countries
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all active countries.</returns>
        public async Task<IEnumerable<Country>> GetActiveCountriesAsync()
        {
            _logger?.LogInformation("Retrieving all active countries");
            
            var countries = await _dbSet
                .Where(c => c.IsActive)
                .Include(c => c.AvailableFilingFrequencies)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} active countries", countries.Count);
            
            return countries;
        }

        /// <summary>
        /// Retrieves countries by their country codes
        /// </summary>
        /// <param name="countryCodes">The collection of country codes to search for</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of countries matching the provided codes.</returns>
        public async Task<IEnumerable<Country>> GetCountriesByCodesAsync(IEnumerable<string> countryCodes)
        {
            _logger?.LogInformation("Retrieving countries by codes");
            
            if (countryCodes == null)
            {
                throw new ArgumentNullException(nameof(countryCodes), "Country codes collection cannot be null");
            }
            
            // Convert to uppercase for case-insensitive comparison
            var upperCountryCodes = countryCodes.Select(c => c.ToUpperInvariant()).ToList();
            
            var countries = await _dbSet
                .Where(c => upperCountryCodes.Contains(c.Code.Value))
                .Include(c => c.AvailableFilingFrequencies)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} countries by codes", countries.Count);
            
            return countries;
        }

        /// <summary>
        /// Retrieves countries that support a specific filing frequency
        /// </summary>
        /// <param name="filingFrequency">The filing frequency to filter by</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of countries that support the specified filing frequency.</returns>
        public async Task<IEnumerable<Country>> GetCountriesByFilingFrequencyAsync(FilingFrequency filingFrequency)
        {
            _logger?.LogInformation("Retrieving countries by filing frequency {FilingFrequency}", filingFrequency);
            
            var countries = await _dbSet
                .Where(c => c.AvailableFilingFrequencies.Contains(filingFrequency))
                .Include(c => c.AvailableFilingFrequencies)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} countries supporting filing frequency {FilingFrequency}", 
                countries.Count, filingFrequency);
            
            return countries;
        }

        /// <summary>
        /// Retrieves a paginated list of countries with optional filtering for active countries only
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="activeOnly">If true, only active countries will be included</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the paginated countries and the total count.</returns>
        public async Task<(IEnumerable<Country> Countries, int TotalCount)> GetPagedCountriesAsync(int pageNumber, int pageSize, bool activeOnly)
        {
            _logger?.LogInformation("Retrieving paged countries - Page {PageNumber}, Size {PageSize}, ActiveOnly {ActiveOnly}", 
                pageNumber, pageSize, activeOnly);
            
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero");
            }
            
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero");
            }
            
            // Create query based on activeOnly flag
            var query = activeOnly ? _dbSet.Where(c => c.IsActive) : _dbSet;
            
            // Get total count matching the criteria
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var countries = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.AvailableFilingFrequencies)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved page {PageNumber} with {Count} countries out of {TotalCount} total", 
                pageNumber, countries.Count, totalCount);
            
            return (countries, totalCount);
        }

        /// <summary>
        /// Checks if a country with the specified country code exists
        /// </summary>
        /// <param name="countryCode">The country code to check</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the country exists.</returns>
        public async Task<bool> ExistsByCodeAsync(string countryCode)
        {
            _logger?.LogInformation("Checking if country with code {CountryCode} exists", countryCode);
            
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentNullException(nameof(countryCode), "Country code cannot be null or empty");
            }
            
            bool exists = await _dbSet.AnyAsync(c => c.Code.Value.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
            
            _logger?.LogInformation("Country with code {CountryCode} {Result}", 
                countryCode, exists ? "exists" : "does not exist");
            
            return exists;
        }

        /// <summary>
        /// Creates a new country in the repository
        /// </summary>
        /// <param name="country">The country to create</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created country.</returns>
        public async Task<Country> CreateAsync(Country country)
        {
            _logger?.LogInformation("Creating new country");
            
            if (country == null)
            {
                throw new ArgumentNullException(nameof(country), "Country cannot be null");
            }
            
            // Check if country with same code already exists
            bool exists = await ExistsByCodeAsync(country.Code.Value);
            if (exists)
            {
                throw new InvalidOperationException($"Country with code {country.Code.Value} already exists");
            }
            
            await _dbSet.AddAsync(country);
            await _context.SaveChangesAsync();
            
            _logger?.LogInformation("Successfully created country with code {CountryCode}", country.Code.Value);
            
            return country;
        }

        /// <summary>
        /// Updates an existing country in the repository
        /// </summary>
        /// <param name="country">The country to update</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated country.</returns>
        public async Task<Country> UpdateAsync(Country country)
        {
            _logger?.LogInformation("Updating country with code {CountryCode}", country?.Code?.Value);
            
            if (country == null)
            {
                throw new ArgumentNullException(nameof(country), "Country cannot be null");
            }
            
            // Check if country exists
            bool exists = await ExistsByCodeAsync(country.Code.Value);
            if (!exists)
            {
                throw new InvalidOperationException($"Country with code {country.Code.Value} does not exist");
            }
            
            _dbSet.Update(country);
            await _context.SaveChangesAsync();
            
            _logger?.LogInformation("Successfully updated country with code {CountryCode}", country.Code.Value);
            
            return country;
        }

        /// <summary>
        /// Deletes a country with the specified country code
        /// </summary>
        /// <param name="countryCode">The country code of the country to delete</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the deletion was successful.</returns>
        public async Task<bool> DeleteByCodeAsync(string countryCode)
        {
            _logger?.LogInformation("Deleting country with code {CountryCode}", countryCode);
            
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentNullException(nameof(countryCode), "Country code cannot be null or empty");
            }
            
            var country = await GetByCodeAsync(countryCode);
            if (country == null)
            {
                _logger?.LogWarning("Country with code {CountryCode} not found for deletion", countryCode);
                return false;
            }
            
            _dbSet.Remove(country);
            await _context.SaveChangesAsync();
            
            _logger?.LogInformation("Successfully deleted country with code {CountryCode}", countryCode);
            
            return true;
        }
    }
}