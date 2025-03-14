using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // version 6.0.0
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Data.Extensions
{
    /// <summary>
    /// Provides extension methods for IQueryable&lt;T&gt; to enhance Entity Framework Core queries with additional functionality
    /// such as pagination, filtering, and sorting. This class centralizes common query patterns used throughout the
    /// VAT Filing Pricing Tool application.
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Conditionally applies a filter to an IQueryable&lt;T&gt; based on a condition.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="predicate">The filter predicate to apply if the condition is true.</param>
        /// <returns>The filtered query if condition is true, otherwise the original query.</returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }

        /// <summary>
        /// Conditionally applies ordering to an IQueryable&lt;T&gt; based on a condition.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key to order by.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="keySelector">The expression to select the key for ordering.</param>
        /// <param name="ascending">If true, order ascending, otherwise order descending.</param>
        /// <returns>The ordered query if condition is true, otherwise the original query cast as IOrderedQueryable&lt;T&gt;.</returns>
        public static IOrderedQueryable<T> OrderByIf<T, TKey>(this IQueryable<T> query, bool condition, Expression<Func<T, TKey>> keySelector, bool ascending = true)
        {
            if (condition)
            {
                return ascending
                    ? query.OrderBy(keySelector)
                    : query.OrderByDescending(keySelector);
            }

            // If the condition is false, we need to return an IOrderedQueryable<T>
            // This is a bit of a hack, but it's the best we can do without modifying the query
            return (IOrderedQueryable<T>)query;
        }

        /// <summary>
        /// Conditionally applies a secondary ordering to an IOrderedQueryable&lt;T&gt; based on a condition.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key to order by.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="keySelector">The expression to select the key for ordering.</param>
        /// <param name="ascending">If true, order ascending, otherwise order descending.</param>
        /// <returns>The ordered query with additional ordering if condition is true, otherwise the original query.</returns>
        public static IOrderedQueryable<T> ThenByIf<T, TKey>(this IOrderedQueryable<T> query, bool condition, Expression<Func<T, TKey>> keySelector, bool ascending = true)
        {
            if (condition)
            {
                return ascending
                    ? query.ThenBy(keySelector)
                    : query.ThenByDescending(keySelector);
            }

            return query;
        }

        /// <summary>
        /// Conditionally includes a related entity in the query based on a condition.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <typeparam name="TProperty">The type of the related entity to include.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="navigationPropertyPath">A lambda expression representing the navigation property to include.</param>
        /// <returns>The query with included navigation property if condition is true, otherwise the original query.</returns>
        public static IQueryable<T> IncludeIf<T, TProperty>(this IQueryable<T> query, bool condition, Expression<Func<T, TProperty>> navigationPropertyPath) where T : class
        {
            return condition ? query.Include(navigationPropertyPath) : query;
        }

        /// <summary>
        /// Asynchronously converts an IQueryable&lt;T&gt; to a PagedList&lt;T&gt; with the specified page number and page size.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a PagedList&lt;T&gt; with the paginated items.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when pageNumber or pageSize is less than or equal to zero.</exception>
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            return await PagedList<T>.CreateAsync(query, pageNumber, pageSize);
        }

        /// <summary>
        /// Applies paging to an IQueryable&lt;T&gt; by skipping and taking the appropriate number of items.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>The query with paging applied.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when pageNumber or pageSize is less than or equal to zero.</exception>
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Applies a filter to an IQueryable&lt;T&gt; if the filter expression is not null.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="filter">The filter expression to apply.</param>
        /// <returns>The filtered query if filter is not null, otherwise the original query.</returns>
        public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, Expression<Func<T, bool>> filter)
        {
            return filter != null ? query.Where(filter) : query;
        }

        /// <summary>
        /// Applies sorting to an IQueryable&lt;T&gt; based on a property name and sort direction.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="propertyName">The name of the property to sort by.</param>
        /// <param name="ascending">If true, sort ascending, otherwise sort descending.</param>
        /// <returns>The sorted query.</returns>
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string propertyName, bool ascending = true)
        {
            if (string.IsNullOrEmpty(propertyName))
                return query;

            try
            {
                // Create parameter expression
                var parameter = Expression.Parameter(typeof(T), "x");
                
                // Create property access expression
                var property = Expression.Property(parameter, propertyName);
                
                // Create the appropriate delegate type: Func<T, TProperty>
                var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), property.Type);
                
                // Create lambda expression with the correct delegate type
                var lambda = Expression.Lambda(delegateType, property, parameter);
                
                // Get the method name based on sort direction
                string methodName = ascending ? "OrderBy" : "OrderByDescending";
                
                // Get the method info with correct generic type arguments
                var method = typeof(Queryable).GetMethods()
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.Type);
                
                // Apply the ordering
                return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda });
            }
            catch
            {
                // If anything goes wrong (property doesn't exist, etc.), return the original query
                return query;
            }
        }
    }
}