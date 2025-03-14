using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VatFilingPricingTool.Common.Models
{
    /// <summary>
    /// Generic class that provides pagination functionality for collections of any type.
    /// Implements a standardized approach to handle large datasets throughout the application.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    public class PagedList<T>
    {
        /// <summary>
        /// The list of items in the current page
        /// </summary>
        public List<T> Items { get; private set; }

        /// <summary>
        /// The current page number (1-based)
        /// </summary>
        public int PageNumber { get; private set; }

        /// <summary>
        /// The number of items per page
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// The total number of items across all pages
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// The total number of pages
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Indicates whether there is a previous page available
        /// </summary>
        public bool HasPreviousPage { get; private set; }

        /// <summary>
        /// Indicates whether there is a next page available
        /// </summary>
        public bool HasNextPage { get; private set; }

        /// <summary>
        /// Private constructor for PagedList to enforce factory method usage
        /// </summary>
        /// <param name="items">The items for the current page</param>
        /// <param name="count">The total count of items across all pages</param>
        /// <param name="pageNumber">The current page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        private PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            HasPreviousPage = pageNumber > 1;
            HasNextPage = pageNumber < TotalPages;
        }

        /// <summary>
        /// Creates a new PagedList from a source collection with pagination parameters
        /// </summary>
        /// <param name="source">The source collection</param>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>A new PagedList containing the paginated items</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when pageNumber or pageSize is less than or equal to zero</exception>
        public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        /// <summary>
        /// Asynchronously creates a new PagedList from a queryable source with pagination parameters
        /// </summary>
        /// <param name="source">The source queryable</param>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a new PagedList with the paginated items</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when pageNumber or pageSize is less than or equal to zero</exception>
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            var count = await Task.Run(() => source.Count());
            var items = await Task.Run(() => source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        /// <summary>
        /// Creates a new PagedList from a source collection and total count with pagination parameters.
        /// This is useful when the total count is already known or calculated separately.
        /// </summary>
        /// <param name="source">The source collection (already paginated)</param>
        /// <param name="totalCount">The total count of items across all pages</param>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>A new PagedList containing the paginated items</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when pageNumber or pageSize is less than or equal to zero</exception>
        public static PagedList<T> Create(IEnumerable<T> source, int totalCount, int pageNumber, int pageSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            var items = source.ToList();

            return new PagedList<T>(items, totalCount, pageNumber, pageSize);
        }
    }

    /// <summary>
    /// Extension methods for working with collections to easily convert them to PagedList
    /// </summary>
    public static class PagedListExtensions
    {
        /// <summary>
        /// Extension method to convert an IEnumerable to a PagedList
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="source">The source collection</param>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>A new PagedList containing the paginated items</returns>
        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            return PagedList<T>.Create(source, pageNumber, pageSize);
        }

        /// <summary>
        /// Extension method to asynchronously convert an IQueryable to a PagedList
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="source">The source queryable</param>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a new PagedList with the paginated items</returns>
        public static Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            return PagedList<T>.CreateAsync(source, pageNumber, pageSize);
        }
    }
}