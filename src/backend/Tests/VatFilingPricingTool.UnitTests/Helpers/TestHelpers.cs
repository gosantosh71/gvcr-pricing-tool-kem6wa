using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.UnitTests.Helpers;

namespace VatFilingPricingTool.UnitTests.Helpers
{
    /// <summary>
    /// Static class providing helper methods for unit testing
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Private constructor to prevent instantiation of static class
        /// </summary>
        private TestHelpers()
        {
        }

        /// <summary>
        /// Creates a Result object for testing with specified success state
        /// </summary>
        /// <param name="isSuccess">Whether the Result should be successful</param>
        /// <param name="errorMessage">Error message for failure results</param>
        /// <param name="errorCode">Error code for failure results</param>
        /// <returns>A Result object with the specified properties</returns>
        public static Result CreateTestResult(bool isSuccess, string errorMessage = null, string errorCode = null)
        {
            return isSuccess
                ? Result.Success()
                : Result.Failure(errorMessage, errorCode);
        }

        /// <summary>
        /// Creates a Result<T> object for testing with specified success state and value
        /// </summary>
        /// <typeparam name="T">Type of the Result value</typeparam>
        /// <param name="isSuccess">Whether the Result should be successful</param>
        /// <param name="value">Value for success results</param>
        /// <param name="errorMessage">Error message for failure results</param>
        /// <param name="errorCode">Error code for failure results</param>
        /// <returns>A Result<T> object with the specified properties</returns>
        public static Result<T> CreateTestResult<T>(bool isSuccess, T value = default, string errorMessage = null, string errorCode = null)
        {
            return isSuccess
                ? Result<T>.Success(value)
                : Result<T>.Failure(errorMessage, errorCode);
        }

        /// <summary>
        /// Creates an ApiResponse object for testing with specified success state
        /// </summary>
        /// <param name="isSuccess">Whether the ApiResponse should be successful</param>
        /// <param name="message">Message for the response</param>
        /// <param name="errorCode">Error code for failure responses</param>
        /// <returns>An ApiResponse object with the specified properties</returns>
        public static ApiResponse CreateTestApiResponse(bool isSuccess, string message = null, string errorCode = null)
        {
            return isSuccess
                ? ApiResponse.CreateSuccess(message)
                : ApiResponse.CreateError(message, errorCode);
        }

        /// <summary>
        /// Creates an ApiResponse<T> object for testing with specified success state and data
        /// </summary>
        /// <typeparam name="T">Type of the ApiResponse data</typeparam>
        /// <param name="isSuccess">Whether the ApiResponse should be successful</param>
        /// <param name="data">Data for success responses</param>
        /// <param name="message">Message for the response</param>
        /// <param name="errorCode">Error code for failure responses</param>
        /// <returns>An ApiResponse<T> object with the specified properties</returns>
        public static ApiResponse<T> CreateTestApiResponse<T>(bool isSuccess, T data = default, string message = null, string errorCode = null)
        {
            return isSuccess
                ? ApiResponse<T>.CreateSuccess(data, message)
                : ApiResponse<T>.CreateError(message, errorCode);
        }

        /// <summary>
        /// Sets up a controller with a mock HTTP context and user identity
        /// </summary>
        /// <param name="controller">The controller to set up</param>
        /// <param name="userId">The user ID to use in the claims</param>
        public static void SetupControllerContext(ControllerBase controller, string userId)
        {
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            httpContext.User = claimsPrincipal;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Creates a mock HttpContext for controller testing
        /// </summary>
        /// <returns>A mock HttpContext</returns>
        public static HttpContext CreateMockHttpContext()
        {
            return new DefaultHttpContext();
        }

        /// <summary>
        /// Creates an async enumerable from a collection for testing async methods
        /// </summary>
        /// <typeparam name="T">Type of the items in the collection</typeparam>
        /// <param name="source">The source collection</param>
        /// <returns>An async enumerable version of the source collection</returns>
        public static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Sets up a mock repository with common configurations
        /// </summary>
        /// <typeparam name="T">Type of entity in the repository</typeparam>
        /// <param name="mockRepository">The mock repository to set up</param>
        /// <param name="entities">The entities to return from the repository</param>
        /// <returns>The configured mock repository</returns>
        public static Mock<IRepository<T>> SetupMockRepository<T>(Mock<IRepository<T>> mockRepository, IEnumerable<T> entities) where T : class
        {
            var entitiesList = entities.ToList();

            // Setup GetAll
            mockRepository.Setup(repo => repo.GetAll())
                .Returns(entitiesList);

            // Setup GetByIdAsync - assuming there's an ID property pattern
            mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => {
                    // Try to find an entity with a property ending in "Id" that matches
                    return entitiesList.FirstOrDefault(e => {
                        var idProperty = typeof(T).GetProperties()
                            .FirstOrDefault(p => p.Name.EndsWith("Id"));
                        
                        if (idProperty != null)
                        {
                            return idProperty.GetValue(e)?.ToString() == id;
                        }
                        return false;
                    });
                });

            // Setup AddAsync
            mockRepository.Setup(repo => repo.AddAsync(It.IsAny<T>()))
                .Returns(Task.CompletedTask);

            // Setup UpdateAsync
            mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<T>()))
                .Returns(Task.CompletedTask);

            // Setup DeleteAsync
            mockRepository.Setup(repo => repo.DeleteAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            return mockRepository;
        }

        /// <summary>
        /// Generates a random user ID for testing
        /// </summary>
        /// <returns>A random user ID</returns>
        public static string GetRandomUserId()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Generates a random calculation ID for testing
        /// </summary>
        /// <returns>A random calculation ID</returns>
        public static string GetRandomCalculationId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}