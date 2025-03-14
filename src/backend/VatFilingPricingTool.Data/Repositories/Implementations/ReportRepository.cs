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
    /// Repository implementation for Report entities with specialized query methods
    /// </summary>
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        /// <summary>
        /// Initializes a new instance of the ReportRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Optional logger instance</param>
        public ReportRepository(IVatFilingDbContext context, ILogger<ReportRepository> logger = null)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves a report by ID with all related details including user and calculation data
        /// </summary>
        /// <param name="id">The report identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the report with details if found, or null.</returns>
        public async Task<Report> GetByIdWithDetailsAsync(string id)
        {
            _logger?.LogInformation("Retrieving report with details for ID {ReportId}", id);

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "Report ID cannot be null or empty");
            }

            var report = await _context.Reports
                .Include(r => r.User)
                .Include(r => r.Calculation)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            _logger?.LogInformation("Report with ID {ReportId} {Result}", id, report != null ? "found" : "not found");

            return report;
        }

        /// <summary>
        /// Retrieves all reports for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of reports for the specified user.</returns>
        public async Task<IEnumerable<Report>> GetByUserIdAsync(string userId)
        {
            _logger?.LogInformation("Retrieving reports for user ID {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }

            var reports = await _context.Reports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.GenerationDate)
                .ToListAsync();

            _logger?.LogInformation("Retrieved {Count} reports for user ID {UserId}", reports.Count, userId);

            return reports;
        }

        /// <summary>
        /// Retrieves a paginated list of reports for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of reports for the specified user.</returns>
        public async Task<PagedList<Report>> GetPagedByUserIdAsync(string userId, int pageNumber, int pageSize)
        {
            _logger?.LogInformation("Retrieving paged reports for user ID {UserId} - Page {PageNumber}, Size {PageSize}", 
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

            var query = _context.Reports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.GenerationDate);

            var pagedList = await PagedList<Report>.CreateAsync(query, pageNumber, pageSize);

            _logger?.LogInformation("Retrieved page {PageNumber} of {TotalPages} with {Count} reports for user ID {UserId}",
                pagedList.PageNumber, pagedList.TotalPages, pagedList.Items.Count, userId);

            return pagedList;
        }

        /// <summary>
        /// Retrieves all reports associated with a specific calculation
        /// </summary>
        /// <param name="calculationId">The calculation identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of reports for the specified calculation.</returns>
        public async Task<IEnumerable<Report>> GetByCalculationIdAsync(string calculationId)
        {
            _logger?.LogInformation("Retrieving reports for calculation ID {CalculationId}", calculationId);

            if (string.IsNullOrEmpty(calculationId))
            {
                throw new ArgumentNullException(nameof(calculationId), "Calculation ID cannot be null or empty");
            }

            var reports = await _context.Reports
                .Where(r => r.CalculationId == calculationId)
                .OrderByDescending(r => r.GenerationDate)
                .ToListAsync();

            _logger?.LogInformation("Retrieved {Count} reports for calculation ID {CalculationId}", reports.Count, calculationId);

            return reports;
        }

        /// <summary>
        /// Retrieves the most recent reports for a specific user, limited by count
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="count">The maximum number of reports to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of the most recent reports for the specified user.</returns>
        public async Task<IEnumerable<Report>> GetRecentReportsAsync(string userId, int count)
        {
            _logger?.LogInformation("Retrieving {Count} recent reports for user ID {UserId}", count, userId);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");
            }

            var reports = await _context.Reports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.GenerationDate)
                .Take(count)
                .ToListAsync();

            _logger?.LogInformation("Retrieved {Count} recent reports for user ID {UserId}", reports.Count, userId);

            return reports;
        }

        /// <summary>
        /// Gets the total number of reports for a specific user
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of reports for the specified user.</returns>
        public async Task<int> GetReportCountByUserIdAsync(string userId)
        {
            _logger?.LogInformation("Counting reports for user ID {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }

            var count = await _context.Reports
                .CountAsync(r => r.UserId == userId);

            _logger?.LogInformation("User ID {UserId} has {Count} reports", userId, count);

            return count;
        }

        /// <summary>
        /// Archives a report by setting its IsArchived property to true
        /// </summary>
        /// <param name="id">The report identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the archiving was successful.</returns>
        public async Task<bool> ArchiveReportAsync(string id)
        {
            _logger?.LogInformation("Archiving report with ID {ReportId}", id);

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "Report ID cannot be null or empty");
            }

            var report = await _dbSet.FindAsync(id);
            if (report == null)
            {
                _logger?.LogWarning("Report with ID {ReportId} not found for archiving", id);
                return false;
            }

            report.Archive();
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Successfully archived report with ID {ReportId}", id);
            return true;
        }

        /// <summary>
        /// Unarchives a report by setting its IsArchived property to false
        /// </summary>
        /// <param name="id">The report identifier</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the unarchiving was successful.</returns>
        public async Task<bool> UnarchiveReportAsync(string id)
        {
            _logger?.LogInformation("Unarchiving report with ID {ReportId}", id);

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "Report ID cannot be null or empty");
            }

            var report = await _dbSet.FindAsync(id);
            if (report == null)
            {
                _logger?.LogWarning("Report with ID {ReportId} not found for unarchiving", id);
                return false;
            }

            report.Unarchive();
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Successfully unarchived report with ID {ReportId}", id);
            return true;
        }
    }
}