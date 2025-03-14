using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions; // version 6.7.0
using Moq; // version 4.18.2
using VatFilingPricingTool.Common.Models;
using Xunit; // version 2.4.1

namespace VatFilingPricingTool.UnitTests.Helpers
{
    /// <summary>
    /// Static class providing extension methods for unit testing
    /// </summary>
    public static class TestExtensions
    {
        /// <summary>
        /// Asserts that a Result object is successful
        /// </summary>
        /// <param name="result">The result to check</param>
        /// <returns>The original result for method chaining</returns>
        public static Result ShouldBeSuccessResult(this Result result)
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("the operation was expected to succeed");
            return result;
        }

        /// <summary>
        /// Asserts that a Result object is a failure with optional error code check
        /// </summary>
        /// <param name="result">The result to check</param>
        /// <param name="expectedErrorCode">The expected error code (optional)</param>
        /// <returns>The original result for method chaining</returns>
        public static Result ShouldBeFailureResult(this Result result, string expectedErrorCode = null)
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse("the operation was expected to fail");
            
            if (expectedErrorCode != null)
            {
                result.ErrorCode.Should().Be(expectedErrorCode, $"the error code should be '{expectedErrorCode}'");
            }
            
            return result;
        }

        /// <summary>
        /// Asserts that a Result{T} object is successful and optionally checks the value
        /// </summary>
        /// <typeparam name="T">The type of the result value</typeparam>
        /// <param name="result">The result to check</param>
        /// <param name="valueCheck">Optional predicate to validate the result value</param>
        /// <returns>The original result for method chaining</returns>
        public static Result<T> ShouldBeSuccessResult<T>(this Result<T> result, Func<T, bool> valueCheck = null)
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue("the operation was expected to succeed");
            
            if (valueCheck != null)
            {
                valueCheck(result.Value).Should().BeTrue("the result value should satisfy the provided condition");
            }
            
            return result;
        }

        /// <summary>
        /// Asserts that a Result{T} object is a failure with optional error code check
        /// </summary>
        /// <typeparam name="T">The type of the result value</typeparam>
        /// <param name="result">The result to check</param>
        /// <param name="expectedErrorCode">The expected error code (optional)</param>
        /// <returns>The original result for method chaining</returns>
        public static Result<T> ShouldBeFailureResult<T>(this Result<T> result, string expectedErrorCode = null)
        {
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse("the operation was expected to fail");
            
            if (expectedErrorCode != null)
            {
                result.ErrorCode.Should().Be(expectedErrorCode, $"the error code should be '{expectedErrorCode}'");
            }
            
            return result;
        }

        /// <summary>
        /// Asserts that an ApiResponse object is successful
        /// </summary>
        /// <param name="response">The response to check</param>
        /// <returns>The original response for method chaining</returns>
        public static ApiResponse ShouldBeSuccessResponse(this ApiResponse response)
        {
            response.Should().NotBeNull();
            response.Success.Should().BeTrue("the API operation was expected to succeed");
            return response;
        }

        /// <summary>
        /// Asserts that an ApiResponse object is an error with optional error code check
        /// </summary>
        /// <param name="response">The response to check</param>
        /// <param name="expectedErrorCode">The expected error code (optional)</param>
        /// <returns>The original response for method chaining</returns>
        public static ApiResponse ShouldBeErrorResponse(this ApiResponse response, string expectedErrorCode = null)
        {
            response.Should().NotBeNull();
            response.Success.Should().BeFalse("the API operation was expected to fail");
            
            if (expectedErrorCode != null)
            {
                response.ErrorCode.Should().Be(expectedErrorCode, $"the error code should be '{expectedErrorCode}'");
            }
            
            return response;
        }

        /// <summary>
        /// Asserts that an ApiResponse{T} object is successful and optionally checks the data
        /// </summary>
        /// <typeparam name="T">The type of the response data</typeparam>
        /// <param name="response">The response to check</param>
        /// <param name="dataCheck">Optional predicate to validate the response data</param>
        /// <returns>The original response for method chaining</returns>
        public static ApiResponse<T> ShouldBeSuccessResponse<T>(this ApiResponse<T> response, Func<T, bool> dataCheck = null)
        {
            response.Should().NotBeNull();
            response.Success.Should().BeTrue("the API operation was expected to succeed");
            
            if (dataCheck != null)
            {
                dataCheck(response.Data).Should().BeTrue("the response data should satisfy the provided condition");
            }
            
            return response;
        }

        /// <summary>
        /// Asserts that an ApiResponse{T} object is an error with optional error code check
        /// </summary>
        /// <typeparam name="T">The type of the response data</typeparam>
        /// <param name="response">The response to check</param>
        /// <param name="expectedErrorCode">The expected error code (optional)</param>
        /// <returns>The original response for method chaining</returns>
        public static ApiResponse<T> ShouldBeErrorResponse<T>(this ApiResponse<T> response, string expectedErrorCode = null)
        {
            response.Should().NotBeNull();
            response.Success.Should().BeFalse("the API operation was expected to fail");
            
            if (expectedErrorCode != null)
            {
                response.ErrorCode.Should().Be(expectedErrorCode, $"the error code should be '{expectedErrorCode}'");
            }
            
            return response;
        }

        /// <summary>
        /// Asserts that a collection contains an item matching the specified predicate
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="collection">The collection to check</param>
        /// <param name="predicate">The predicate to match items against</param>
        /// <returns>The original collection for method chaining</returns>
        public static IEnumerable<T> ShouldContainItem<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            collection.Should().NotBeNull();
            collection.Should().Contain(item => predicate(item), "the collection should contain an item matching the predicate");
            return collection;
        }

        /// <summary>
        /// Asserts that a collection does not contain any item matching the specified predicate
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="collection">The collection to check</param>
        /// <param name="predicate">The predicate to match items against</param>
        /// <returns>The original collection for method chaining</returns>
        public static IEnumerable<T> ShouldNotContainItem<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            collection.Should().NotBeNull();
            collection.Should().NotContain(item => predicate(item), "the collection should not contain any item matching the predicate");
            return collection;
        }

        /// <summary>
        /// Asserts that a collection has the expected count of items
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="collection">The collection to check</param>
        /// <param name="expectedCount">The expected count</param>
        /// <returns>The original collection for method chaining</returns>
        public static IEnumerable<T> ShouldHaveCount<T>(this IEnumerable<T> collection, int expectedCount)
        {
            collection.Should().NotBeNull();
            collection.Should().HaveCount(expectedCount, $"the collection should have {expectedCount} items");
            return collection;
        }

        /// <summary>
        /// Asserts that a PagedList{T} has the expected pagination properties
        /// </summary>
        /// <typeparam name="T">The type of items in the paged list</typeparam>
        /// <param name="pagedList">The paged list to check</param>
        /// <param name="expectedPageNumber">The expected page number</param>
        /// <param name="expectedPageSize">The expected page size</param>
        /// <param name="expectedTotalCount">The expected total count</param>
        /// <returns>The original paged list for method chaining</returns>
        public static PagedList<T> ShouldBePagedList<T>(this PagedList<T> pagedList, int expectedPageNumber, int expectedPageSize, int expectedTotalCount)
        {
            pagedList.Should().NotBeNull();
            pagedList.PageNumber.Should().Be(expectedPageNumber, $"the page number should be {expectedPageNumber}");
            pagedList.PageSize.Should().Be(expectedPageSize, $"the page size should be {expectedPageSize}");
            pagedList.TotalCount.Should().Be(expectedTotalCount, $"the total count should be {expectedTotalCount}");
            
            int expectedTotalPages = (int)Math.Ceiling((double)expectedTotalCount / expectedPageSize);
            pagedList.TotalPages.Should().Be(expectedTotalPages, $"the total pages should be {expectedTotalPages}");
            
            return pagedList;
        }

        /// <summary>
        /// Sets up a mock with common verification settings
        /// </summary>
        /// <typeparam name="T">The type being mocked</typeparam>
        /// <param name="mock">The mock to configure</param>
        /// <returns>The configured mock for method chaining</returns>
        public static Mock<T> SetupMockFor<T>(this Mock<T> mock) where T : class
        {
            // Configure the mock with default behavior and track all calls
            mock.DefaultValue = DefaultValue.Mock;
            mock.CallBase = false;
            mock.Invocations.Clear();
            return mock;
        }

        /// <summary>
        /// Converts a collection to an async enumerable for testing async methods
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <param name="source">The source collection</param>
        /// <returns>An async enumerable version of the source collection</returns>
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Converts a value to a completed Task{T}
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="value">The value to convert</param>
        /// <returns>A completed task containing the value</returns>
        public static Task<T> AsTask<T>(this T value)
        {
            return Task.FromResult(value);
        }
    }
}