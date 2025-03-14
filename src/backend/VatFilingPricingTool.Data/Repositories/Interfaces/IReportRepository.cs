using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Domain.Entities;

namespace VatFilingPricingTool.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Report entities with specialized query methods
    /// </summary>
    public interface IReportRepository : IRepository<Report>
    {
        /// <summary>
        /// Retrieves a report by ID with all related details including user and calculation data
        /// </summary>
        /// <param name="id">The report identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the report with details if found, null otherwise</returns>
        Task<Report> GetByIdWithDetailsAsync(string id);
        
        /// <summary>
        /// Retrieves all reports for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of reports for the specified user</returns>
        Task<IEnumerable<Report>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Retrieves a paginated list of reports for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated collection of reports for the specified user</returns>
        Task<PagedList<Report>> GetPagedByUserIdAsync(string userId, int pageNumber, int pageSize);
        
        /// <summary>
        /// Retrieves all reports associated with a specific calculation
        /// </summary>
        /// <param name="calculationId">The calculation identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of reports for the specified calculation</returns>
        Task<IEnumerable<Report>> GetByCalculationIdAsync(string calculationId);
        
        /// <summary>
        /// Retrieves the most recent reports for a specific user, limited by count
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="count">The maximum number of reports to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of recent reports for the specified user</returns>
        Task<IEnumerable<Report>> GetRecentReportsAsync(string userId, int count);
        
        /// <summary>
        /// Gets the total number of reports for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of reports for the specified user</returns>
        Task<int> GetReportCountByUserIdAsync(string userId);
        
        /// <summary>
        /// Archives a report by setting its IsArchived property to true
        /// </summary>
        /// <param name="id">The report identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the archiving was successful</returns>
        Task<bool> ArchiveReportAsync(string id);
        
        /// <summary>
        /// Unarchives a report by setting its IsArchived property to false
        /// </summary>
        /// <param name="id">The report identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the unarchiving was successful</returns>
        Task<bool> UnarchiveReportAsync(string id);
    }
}