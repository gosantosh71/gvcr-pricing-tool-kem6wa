using System.Collections.Generic; // version 6.0.0
using System.Net; // version 6.0.0
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Common.Models
{
    /// <summary>
    /// Base class for API responses without data payload, providing a standardized
    /// response format across all API endpoints.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Indicates whether the API request was successful.
        /// </summary>
        public bool Success { get; protected set; }

        /// <summary>
        /// Human-readable message describing the result of the operation.
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// Error code identifier when an error occurs, used for client-side error handling.
        /// </summary>
        public string ErrorCode { get; protected set; }

        /// <summary>
        /// Additional metadata related to the response.
        /// </summary>
        public Dictionary<string, object> Metadata { get; protected set; }

        /// <summary>
        /// HTTP status code associated with the response.
        /// </summary>
        public int StatusCode { get; protected set; }

        /// <summary>
        /// Protected constructor for ApiResponse to enforce factory method usage.
        /// </summary>
        protected ApiResponse()
        {
            Success = false;
            Message = null;
            ErrorCode = null;
            Metadata = new Dictionary<string, object>();
            StatusCode = (int)HttpStatusCode.OK;
        }

        /// <summary>
        /// Creates a successful API response.
        /// </summary>
        /// <param name="message">Optional custom success message.</param>
        /// <returns>A successful API response with the provided message.</returns>
        public static ApiResponse CreateSuccess(string message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message ?? "Operation completed successfully",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Creates an error API response.
        /// </summary>
        /// <param name="message">Error message describing what went wrong.</param>
        /// <param name="errorCode">Specific error code for the issue.</param>
        /// <param name="statusCode">HTTP status code to return.</param>
        /// <returns>An error API response with the provided details.</returns>
        public static ApiResponse CreateError(
            string message = null,
            string errorCode = null,
            int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message ?? "An error occurred during the operation",
                ErrorCode = errorCode ?? ErrorCodes.General.UnknownError,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Adds metadata to the API response.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The current API response instance with added metadata.</returns>
        public ApiResponse AddMetadata(string key, object value)
        {
            Metadata[key] = value;
            return this;
        }

        /// <summary>
        /// Adds validation errors to the API response metadata.
        /// </summary>
        /// <param name="errors">Collection of validation error messages.</param>
        /// <returns>The current API response instance with added validation errors.</returns>
        public ApiResponse AddValidationErrors(IEnumerable<string> errors)
        {
            Metadata["ValidationErrors"] = errors;
            return this;
        }
    }

    /// <summary>
    /// Generic API response class that includes a data payload of type T.
    /// </summary>
    /// <typeparam name="T">Type parameter representing the data payload type.</typeparam>
    public class ApiResponse<T> 
    {
        /// <summary>
        /// Indicates whether the API request was successful.
        /// </summary>
        public bool Success { get; protected set; }

        /// <summary>
        /// Human-readable message describing the result of the operation.
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// Error code identifier when an error occurs, used for client-side error handling.
        /// </summary>
        public string ErrorCode { get; protected set; }

        /// <summary>
        /// Additional metadata related to the response.
        /// </summary>
        public Dictionary<string, object> Metadata { get; protected set; }

        /// <summary>
        /// HTTP status code associated with the response.
        /// </summary>
        public int StatusCode { get; protected set; }

        /// <summary>
        /// The data payload returned by the API.
        /// </summary>
        public T Data { get; protected set; }

        /// <summary>
        /// Protected constructor for ApiResponse to enforce factory method usage.
        /// </summary>
        protected ApiResponse()
        {
            Success = false;
            Message = null;
            ErrorCode = null;
            Metadata = new Dictionary<string, object>();
            StatusCode = (int)HttpStatusCode.OK;
            Data = default(T);
        }

        /// <summary>
        /// Creates a successful API response with data.
        /// </summary>
        /// <param name="data">The data payload to include in the response.</param>
        /// <param name="message">Optional custom success message.</param>
        /// <returns>A successful API response with the provided data and message.</returns>
        public static ApiResponse<T> CreateSuccess(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message ?? "Operation completed successfully",
                Data = data,
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Creates an error API response.
        /// </summary>
        /// <param name="message">Error message describing what went wrong.</param>
        /// <param name="errorCode">Specific error code for the issue.</param>
        /// <param name="statusCode">HTTP status code to return.</param>
        /// <returns>An error API response with the provided details.</returns>
        public static ApiResponse<T> CreateError(
            string message = null,
            string errorCode = null,
            int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message ?? "An error occurred during the operation",
                ErrorCode = errorCode ?? ErrorCodes.General.UnknownError,
                StatusCode = statusCode,
                Data = default(T)
            };
        }

        /// <summary>
        /// Adds metadata to the API response.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The current API response instance with added metadata.</returns>
        public ApiResponse<T> AddMetadata(string key, object value)
        {
            Metadata[key] = value;
            return this;
        }

        /// <summary>
        /// Adds validation errors to the API response metadata.
        /// </summary>
        /// <param name="errors">Collection of validation error messages.</param>
        /// <returns>The current API response instance with added validation errors.</returns>
        public ApiResponse<T> AddValidationErrors(IEnumerable<string> errors)
        {
            Metadata["ValidationErrors"] = errors;
            return this;
        }
    }

    /// <summary>
    /// Extension methods for working with ApiResponse objects and converting from internal Result objects.
    /// </summary>
    public static class ApiResponseExtensions
    {
        /// <summary>
        /// Converts a Result&lt;T&gt; to an ApiResponse&lt;T&gt;.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="result">The result to convert.</param>
        /// <returns>An API response based on the result.</returns>
        public static ApiResponse<T> ToApiResponse<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return ApiResponse<T>.CreateSuccess(result.Value);
            }
            else
            {
                var response = ApiResponse<T>.CreateError(
                    message: result.ErrorMessage, 
                    errorCode: result.ErrorCode,
                    statusCode: DetermineStatusCode(result.ErrorCode));
                
                if (result.ValidationErrors != null && result.ValidationErrors.Any())
                {
                    response.AddValidationErrors(result.ValidationErrors);
                }
                
                return response;
            }
        }

        /// <summary>
        /// Converts a Result to an ApiResponse.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        /// <returns>An API response based on the result.</returns>
        public static ApiResponse ToApiResponse(this Result result)
        {
            if (result.IsSuccess)
            {
                return ApiResponse.CreateSuccess();
            }
            else
            {
                var response = ApiResponse.CreateError(
                    message: result.ErrorMessage, 
                    errorCode: result.ErrorCode,
                    statusCode: DetermineStatusCode(result.ErrorCode));
                
                if (result.ValidationErrors != null && result.ValidationErrors.Any())
                {
                    response.AddValidationErrors(result.ValidationErrors);
                }
                
                return response;
            }
        }

        /// <summary>
        /// Determines the appropriate HTTP status code based on the error code.
        /// </summary>
        /// <param name="errorCode">The error code to evaluate.</param>
        /// <returns>The corresponding HTTP status code.</returns>
        private static int DetermineStatusCode(string errorCode)
        {
            if (errorCode == null)
                return (int)HttpStatusCode.BadRequest;

            if (errorCode == ErrorCodes.General.NotFound || 
                errorCode.EndsWith("NotFound"))
                return (int)HttpStatusCode.NotFound;

            if (errorCode == ErrorCodes.General.Unauthorized || 
                errorCode.StartsWith("AUTH"))
                return (int)HttpStatusCode.Unauthorized;

            if (errorCode == ErrorCodes.General.Forbidden)
                return (int)HttpStatusCode.Forbidden;

            if (errorCode == ErrorCodes.General.Conflict || 
                errorCode.Contains("Duplicate"))
                return (int)HttpStatusCode.Conflict;

            if (errorCode == ErrorCodes.General.ValidationError)
                return (int)HttpStatusCode.BadRequest;

            if (errorCode == ErrorCodes.General.ServiceUnavailable)
                return (int)HttpStatusCode.ServiceUnavailable;

            if (errorCode == ErrorCodes.General.ServerError)
                return (int)HttpStatusCode.InternalServerError;

            return (int)HttpStatusCode.BadRequest;
        }
    }
}