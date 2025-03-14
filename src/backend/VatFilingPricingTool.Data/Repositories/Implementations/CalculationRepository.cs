using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Data.Context;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;

namespace VatFilingPricingTool.Data.Repositories.Implementations
{
    /// <summary>
    /// Implements the repository pattern for Calculation entities, providing specialized data access methods
    /// for retrieving, storing, and managing VAT filing calculations with their related data.
    /// </summary>
    public class CalculationRepository : Repository<Calculation>, ICalculationRepository
    {
        /// <summary>
        /// Initializes a new instance of the CalculationRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Optional logger instance</param>
        public CalculationRepository(IVatFilingDbContext context, ILogger<CalculationRepository> logger = null) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves a calculation by ID with all related details including countries, services, and discounts
        /// </summary>
        /// <param name="id">The identifier of the calculation to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the calculation with all details if found, or null.</returns>
        public async Task<Calculation> GetByIdWithDetailsAsync(string id)
        {
            _logger?.LogInformation("Retrieving calculation with ID {CalculationId} with all details", id);

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "Calculation ID cannot be null or empty");
            }

            var calculation = await _dbSet
                .Include(c => c.User)
                .Include(c => c.Service)
                .Include(c => c.CalculationCountries)
                    .ThenInclude(cc => cc.Country)
                .Include(c => c.AdditionalServices)
                    .ThenInclude(cs => cs.AdditionalService)
                .Include(c => c.Reports)
                .FirstOrDefaultAsync(c => c.CalculationId == id);

            _logger?.LogInformation("Calculation with ID {CalculationId} {Result}", 
                id, calculation != null ? "found" : "not found");

            return calculation;
        }

        /// <summary>
        /// Retrieves all calculations for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of calculations for the specified user.</returns>
        public async Task<IEnumerable<Calculation>> GetByUserIdAsync(string userId)
        {
            _logger?.LogInformation("Retrieving calculations for user with ID {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }

            var calculations = await _dbSet
                .Include(c => c.Service)
                .Include(c => c.CalculationCountries)
                    .ThenInclude(cc => cc.Country)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CalculationDate)
                .ToListAsync();

            _logger?.LogInformation("Retrieved {Count} calculations for user with ID {UserId}", 
                calculations.Count, userId);

            return calculations;
        }

        /// <summary>
        /// Retrieves a paginated list of calculations for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of calculations.</returns>
        public async Task<PagedList<Calculation>> GetPagedByUserIdAsync(string userId, int pageNumber, int pageSize)
        {
            _logger?.LogInformation("Retrieving paged calculations for user with ID {UserId} - Page {PageNumber}, Size {PageSize}", 
                userId, pageNumber, pageSize);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }

            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero");
            }

            var query = _dbSet
                .Include(c => c.Service)
                .Include(c => c.CalculationCountries)
                    .ThenInclude(cc => cc.Country)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CalculationDate);

            var pagedList = await PagedList<Calculation>.CreateAsync(query, pageNumber, pageSize);

            _logger?.LogInformation("Retrieved page {PageNumber} of {TotalPages} with {Count} calculations for user with ID {UserId}", 
                pagedList.PageNumber, pagedList.TotalPages, pagedList.Items.Count, userId);

            return pagedList;
        }

        /// <summary>
        /// Retrieves all calculations that include a specific country
        /// </summary>
        /// <param name="countryCode">The country code to search for</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of calculations that include the specified country.</returns>
        public async Task<IEnumerable<Calculation>> GetByCountryCodeAsync(string countryCode)
        {
            _logger?.LogInformation("Retrieving calculations for country with code {CountryCode}", countryCode);

            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentNullException(nameof(countryCode), "Country code cannot be null or empty");
            }

            var calculations = await _dbSet
                .Include(c => c.Service)
                .Include(c => c.CalculationCountries)
                    .ThenInclude(cc => cc.Country)
                .Where(c => c.CalculationCountries.Any(cc => cc.CountryCode == countryCode))
                .OrderByDescending(c => c.CalculationDate)
                .ToListAsync();

            _logger?.LogInformation("Retrieved {Count} calculations for country with code {CountryCode}", 
                calculations.Count, countryCode);

            return calculations;
        }

        /// <summary>
        /// Retrieves the most recent calculations for a specific user, limited by count
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="count">The maximum number of calculations to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of the most recent calculations.</returns>
        public async Task<IEnumerable<Calculation>> GetRecentCalculationsAsync(string userId, int count)
        {
            _logger?.LogInformation("Retrieving {Count} recent calculations for user with ID {UserId}", count, userId);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");
            }

            var calculations = await _dbSet
                .Include(c => c.Service)
                .Include(c => c.CalculationCountries)
                    .ThenInclude(cc => cc.Country)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CalculationDate)
                .Take(count)
                .ToListAsync();

            _logger?.LogInformation("Retrieved {Count} recent calculations for user with ID {UserId}", 
                calculations.Count, userId);

            return calculations;
        }

        /// <summary>
        /// Gets the total number of calculations for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of calculations.</returns>
        public async Task<int> GetCalculationCountByUserIdAsync(string userId)
        {
            _logger?.LogInformation("Counting calculations for user with ID {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }

            var count = await _dbSet
                .CountAsync(c => c.UserId == userId);

            _logger?.LogInformation("User with ID {UserId} has {Count} calculations", userId, count);

            return count;
        }

        /// <summary>
        /// Archives a calculation by setting its IsArchived property to true
        /// </summary>
        /// <param name="id">The identifier of the calculation to archive</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the archiving was successful.</returns>
        public async Task<bool> ArchiveCalculationAsync(string id)
        {
            _logger?.LogInformation("Archiving calculation with ID {CalculationId}", id);

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "Calculation ID cannot be null or empty");
            }

            var calculation = await _dbSet.FindAsync(id);

            if (calculation == null)
            {
                _logger?.LogWarning("Calculation with ID {CalculationId} not found for archiving", id);
                return false;
            }

            calculation.Archive();
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Successfully archived calculation with ID {CalculationId}", id);

            return true;
        }

        /// <summary>
        /// Unarchives a calculation by setting its IsArchived property to false
        /// </summary>
        /// <param name="id">The identifier of the calculation to unarchive</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the unarchiving was successful.</returns>
        public async Task<bool> UnarchiveCalculationAsync(string id)
        {
            _logger?.LogInformation("Unarchiving calculation with ID {CalculationId}", id);

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "Calculation ID cannot be null or empty");
            }

            var calculation = await _dbSet.FindAsync(id);

            if (calculation == null)
            {
                _logger?.LogWarning("Calculation with ID {CalculationId} not found for unarchiving", id);
                return false;
            }

            calculation.Unarchive();
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Successfully unarchived calculation with ID {CalculationId}", id);

            return true;
        }
    }
}