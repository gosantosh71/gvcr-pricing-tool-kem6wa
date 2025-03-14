using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VatFilingPricingTool.Data.Context;
using VatFilingPricingTool.Data.Repositories.Implementations;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Service entities with specialized query operations
    /// </summary>
    public class ServiceRepository : Repository<Service>, IServiceRepository
    {
        /// <summary>
        /// Initializes a new instance of the ServiceRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Optional logger instance</param>
        public ServiceRepository(IVatFilingDbContext context, ILogger<ServiceRepository> logger = null)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves all active services
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of active services.</returns>
        public async Task<IEnumerable<Service>> GetActiveServicesAsync()
        {
            _logger?.LogInformation("Retrieving all active services");
            
            var activeServices = await _dbSet.Where(s => s.IsActive).ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} active services", activeServices.Count);
            
            return activeServices;
        }

        /// <summary>
        /// Retrieves services by their service type
        /// </summary>
        /// <param name="serviceType">The service type to filter by</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of services of the specified type.</returns>
        public async Task<IEnumerable<Service>> GetServicesByTypeAsync(ServiceType serviceType)
        {
            _logger?.LogInformation("Retrieving services with service type {ServiceType}", serviceType);
            
            var services = await _dbSet.Where(s => s.ServiceType == serviceType).ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} services with service type {ServiceType}", services.Count, serviceType);
            
            return services;
        }

        /// <summary>
        /// Retrieves services within a specified complexity level range
        /// </summary>
        /// <param name="minComplexityLevel">The minimum complexity level (inclusive)</param>
        /// <param name="maxComplexityLevel">The maximum complexity level (inclusive)</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of services within the specified complexity range.</returns>
        public async Task<IEnumerable<Service>> GetServicesByComplexityLevelAsync(int minComplexityLevel, int maxComplexityLevel)
        {
            _logger?.LogInformation("Retrieving services with complexity level between {MinLevel} and {MaxLevel}", 
                minComplexityLevel, maxComplexityLevel);
            
            if (minComplexityLevel <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minComplexityLevel), "Minimum complexity level must be greater than zero");
            }
            
            if (maxComplexityLevel < minComplexityLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(maxComplexityLevel), 
                    "Maximum complexity level must be greater than or equal to minimum complexity level");
            }
            
            var services = await _dbSet
                .Where(s => s.ComplexityLevel >= minComplexityLevel && s.ComplexityLevel <= maxComplexityLevel)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} services with complexity level between {MinLevel} and {MaxLevel}", 
                services.Count, minComplexityLevel, maxComplexityLevel);
            
            return services;
        }

        /// <summary>
        /// Retrieves a paginated list of services with optional filtering for active services only
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="activeOnly">Whether to include only active services</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the paginated services and the total count.</returns>
        public async Task<(IEnumerable<Service> Services, int TotalCount)> GetPagedServicesAsync(int pageNumber, int pageSize, bool activeOnly)
        {
            _logger?.LogInformation("Retrieving paged services - Page {PageNumber}, Size {PageSize}, ActiveOnly {ActiveOnly}", 
                pageNumber, pageSize, activeOnly);
            
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero");
            }
            
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero");
            }
            
            // Create base query
            var query = _dbSet.AsQueryable();
            
            // Apply filter for active services if requested
            if (activeOnly)
            {
                query = query.Where(s => s.IsActive);
            }
            
            // Get total count
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var services = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            _logger?.LogInformation("Retrieved page {PageNumber} of services with {Count} items (Total: {TotalCount})", 
                pageNumber, services.Count, totalCount);
            
            return (services, totalCount);
        }

        /// <summary>
        /// Checks if a service with the specified identifier exists
        /// </summary>
        /// <param name="serviceId">The service identifier to check</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the service exists.</returns>
        public async Task<bool> ExistsByIdAsync(string serviceId)
        {
            _logger?.LogInformation("Checking if service with ID {ServiceId} exists", serviceId);
            
            if (string.IsNullOrEmpty(serviceId))
            {
                throw new ArgumentNullException(nameof(serviceId), "Service ID cannot be null or empty");
            }
            
            var exists = await _dbSet.AnyAsync(s => s.ServiceId == serviceId);
            
            _logger?.LogInformation("Service with ID {ServiceId} {Exists}", serviceId, exists ? "exists" : "does not exist");
            
            return exists;
        }
    }
}