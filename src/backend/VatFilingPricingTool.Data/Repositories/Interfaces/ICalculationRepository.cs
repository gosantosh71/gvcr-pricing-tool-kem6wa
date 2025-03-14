using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Domain.Entities;

namespace VatFilingPricingTool.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Calculation entities with specialized query methods
    /// beyond the standard CRUD operations provided by IRepository&lt;T&gt;.
    /// </summary>
    public interface ICalculationRepository : IRepository<Calculation>
    {
        /// <summary>
        /// Retrieves a calculation by ID with all related details including countries, services, and discounts
        /// </summary>
        /// <param name="id">The identifier of the calculation to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the calculation with all details if found, null otherwise</returns>
        Task<Calculation> GetByIdWithDetailsAsync(string id);
        
        /// <summary>
        /// Retrieves all calculations for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of calculations for the specified user</returns>
        Task<IEnumerable<Calculation>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Retrieves a paginated list of calculations for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated collection of calculations</returns>
        Task<PagedList<Calculation>> GetPagedByUserIdAsync(string userId, int pageNumber, int pageSize);
        
        /// <summary>
        /// Retrieves all calculations that include a specific country
        /// </summary>
        /// <param name="countryCode">The country code to search for</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of calculations that include the specified country</returns>
        Task<IEnumerable<Calculation>> GetByCountryCodeAsync(string countryCode);
        
        /// <summary>
        /// Retrieves the most recent calculations for a specific user, limited by count
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="count">The maximum number of calculations to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of recent calculations</returns>
        Task<IEnumerable<Calculation>> GetRecentCalculationsAsync(string userId, int count);
        
        /// <summary>
        /// Gets the total number of calculations for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of calculations</returns>
        Task<int> GetCalculationCountByUserIdAsync(string userId);
        
        /// <summary>
        /// Archives a calculation by setting its IsArchived property to true
        /// </summary>
        /// <param name="id">The identifier of the calculation to archive</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the archiving was successful</returns>
        Task<bool> ArchiveCalculationAsync(string id);
        
        /// <summary>
        /// Unarchives a calculation by setting its IsArchived property to false
        /// </summary>
        /// <param name="id">The identifier of the calculation to unarchive</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the unarchiving was successful</returns>
        Task<bool> UnarchiveCalculationAsync(string id);
    }
}