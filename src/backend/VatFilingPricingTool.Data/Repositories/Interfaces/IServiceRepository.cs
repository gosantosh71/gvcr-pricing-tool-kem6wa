using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Service entities with specialized query operations.
    /// Extends the generic repository pattern to provide Service-specific data access operations
    /// for retrieving, filtering, and managing VAT filing service types.
    /// </summary>
    public interface IServiceRepository : IRepository<Service>
    {
        /// <summary>
        /// Retrieves all active services
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of active services</returns>
        Task<IEnumerable<Service>> GetActiveServicesAsync();

        /// <summary>
        /// Retrieves services by their service type
        /// </summary>
        /// <param name="serviceType">The service type to filter by</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of services matching the specified service type</returns>
        Task<IEnumerable<Service>> GetServicesByTypeAsync(ServiceType serviceType);

        /// <summary>
        /// Retrieves services within a specified complexity level range
        /// </summary>
        /// <param name="minComplexityLevel">The minimum complexity level (inclusive)</param>
        /// <param name="maxComplexityLevel">The maximum complexity level (inclusive)</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of services within the specified complexity range</returns>
        Task<IEnumerable<Service>> GetServicesByComplexityLevelAsync(int minComplexityLevel, int maxComplexityLevel);

        /// <summary>
        /// Retrieves a paginated list of services with optional filtering for active services only
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="activeOnly">Whether to include only active services</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the paginated collection of services and the total count</returns>
        Task<(IEnumerable<Service> Services, int TotalCount)> GetPagedServicesAsync(int pageNumber, int pageSize, bool activeOnly);

        /// <summary>
        /// Checks if a service with the specified identifier exists
        /// </summary>
        /// <param name="serviceId">The service identifier to check</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the service exists</returns>
        Task<bool> ExistsByIdAsync(string serviceId);
    }
}