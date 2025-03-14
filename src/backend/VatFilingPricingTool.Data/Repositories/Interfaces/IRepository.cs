using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Data.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface that defines standard CRUD operations for entity types.
    /// Provides a consistent data access pattern across the application for the VAT Filing Pricing Tool.
    /// </summary>
    /// <typeparam name="T">The entity type for which the repository provides data access operations</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves an entity by its unique identifier
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found, null otherwise</returns>
        Task<T> GetByIdAsync(string id);

        /// <summary>
        /// Retrieves all entities of type T
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Finds entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of entities that match the predicate</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retrieves a paginated list of entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated collection of entities</returns>
        Task<PagedList<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);

        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity with the specified identifier
        /// </summary>
        /// <param name="id">The identifier of the entity to delete</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the deletion was successful</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Counts entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of matching entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Determines whether any entity matches the specified predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether any matching entity exists</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}