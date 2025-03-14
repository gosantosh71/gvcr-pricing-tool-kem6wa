using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Data.Context;
using VatFilingPricingTool.Data.Repositories.Interfaces;

namespace VatFilingPricingTool.Data.Repositories.Implementations
{
    /// <summary>
    /// Generic repository implementation that provides standard data access operations for entity types
    /// </summary>
    /// <typeparam name="T">Type parameter representing the entity type, constrained to class</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly IVatFilingDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<Repository<T>> _logger;

        /// <summary>
        /// Initializes a new instance of the Repository<T> class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Optional logger instance</param>
        public Repository(IVatFilingDbContext context, ILogger<Repository<T>> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an entity by its unique identifier
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found, or null.</returns>
        public virtual async Task<T> GetByIdAsync(string id)
        {
            _logger?.LogInformation("Retrieving entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);
            
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "Entity ID cannot be null or empty");
            }
            
            var entity = await _dbSet.FindAsync(id);
            
            _logger?.LogInformation("Entity of type {EntityType} with ID {EntityId} {Result}", 
                typeof(T).Name, id, entity != null ? "found" : "not found");
            
            return entity;
        }

        /// <summary>
        /// Retrieves all entities of type T
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all entities.</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            _logger?.LogInformation("Retrieving all entities of type {EntityType}", typeof(T).Name);
            
            var entities = await _dbSet.ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} entities of type {EntityType}", entities.Count, typeof(T).Name);
            
            return entities;
        }

        /// <summary>
        /// Finds entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of matching entities.</returns>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            _logger?.LogInformation("Finding entities of type {EntityType} matching predicate", typeof(T).Name);
            
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "Predicate cannot be null");
            }
            
            var entities = await _dbSet.Where(predicate).ToListAsync();
            
            _logger?.LogInformation("Found {Count} entities of type {EntityType} matching predicate", entities.Count, typeof(T).Name);
            
            return entities;
        }

        /// <summary>
        /// Retrieves a paginated list of entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <param name="pageNumber">The page number to retrieve (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of entities.</returns>
        public virtual async Task<PagedList<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
        {
            _logger?.LogInformation("Retrieving paged entities of type {EntityType} - Page {PageNumber}, Size {PageSize}", 
                typeof(T).Name, pageNumber, pageSize);
            
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "Predicate cannot be null");
            }
            
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero");
            }
            
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero");
            }
            
            var query = _dbSet.Where(predicate);
            var pagedList = await PagedList<T>.CreateAsync(query, pageNumber, pageSize);
            
            _logger?.LogInformation("Retrieved page {PageNumber} of {TotalPages} with {Count} entities of type {EntityType}", 
                pagedList.PageNumber, pagedList.TotalPages, pagedList.Items.Count, typeof(T).Name);
            
            return pagedList;
        }

        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
        public virtual async Task<T> AddAsync(T entity)
        {
            _logger?.LogInformation("Adding new entity of type {EntityType}", typeof(T).Name);
            
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            _logger?.LogInformation("Successfully added new entity of type {EntityType}", typeof(T).Name);
            
            return entity;
        }

        /// <summary>
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity.</returns>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            _logger?.LogInformation("Updating entity of type {EntityType}", typeof(T).Name);
            
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            
            _logger?.LogInformation("Successfully updated entity of type {EntityType}", typeof(T).Name);
            
            return entity;
        }

        /// <summary>
        /// Deletes an entity with the specified identifier
        /// </summary>
        /// <param name="id">The identifier of the entity to delete</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the deletion was successful.</returns>
        public virtual async Task<bool> DeleteAsync(string id)
        {
            _logger?.LogInformation("Deleting entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);
            
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "Entity ID cannot be null or empty");
            }
            
            var entity = await _dbSet.FindAsync(id);
            
            if (entity == null)
            {
                _logger?.LogWarning("Entity of type {EntityType} with ID {EntityId} not found for deletion", typeof(T).Name, id);
                return false;
            }
            
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            
            _logger?.LogInformation("Successfully deleted entity of type {EntityType} with ID {EntityId}", typeof(T).Name, id);
            
            return true;
        }

        /// <summary>
        /// Counts entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of matching entities.</returns>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            _logger?.LogInformation("Counting entities of type {EntityType} matching predicate", typeof(T).Name);
            
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "Predicate cannot be null");
            }
            
            var count = await _dbSet.CountAsync(predicate);
            
            _logger?.LogInformation("Counted {Count} entities of type {EntityType} matching predicate", count, typeof(T).Name);
            
            return count;
        }

        /// <summary>
        /// Determines whether any entity matches the specified predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether any matching entity exists.</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            _logger?.LogInformation("Checking existence of entities of type {EntityType} matching predicate", typeof(T).Name);
            
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "Predicate cannot be null");
            }
            
            var exists = await _dbSet.AnyAsync(predicate);
            
            _logger?.LogInformation("Entities of type {EntityType} matching predicate {Exist}", 
                typeof(T).Name, exists ? "exist" : "do not exist");
            
            return exists;
        }
    }
}