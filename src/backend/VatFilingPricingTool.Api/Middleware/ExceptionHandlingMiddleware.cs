using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // version 6.0.0
using Microsoft.Extensions.Hosting; // version 6.0.0
using Microsoft.Extensions.Options; // version 6.0.0
using VatFilingPricingTool.Common.Models.ApiResponse;
using VatFilingPricingTool.Common.Extensions;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Infrastructure.Logging;

namespace VatFilingPricingTool.Api.Middleware
{
    /// <summary>
    /// ASP.NET Core middleware that catches and handles all unhandled exceptions in the application pipeline,
    /// converting them to standardized API responses with appropriate status codes and error details.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _logger;
        private readonly IHostEnvironment _environment;
        private readonly ExceptionHandlingOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class with required dependencies.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">The logging service</param>
        /// <param name="environment">The hosting environment</param>
        /// <param name="options">The exception handling options</param>
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILoggingService logger,
            IHostEnvironment environment,
            IOptions<ExceptionHandlingOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Processes the HTTP request and catches any unhandled exceptions,
        /// converting them to standardized API responses.
        /// </summary>
        /// <param name="context">The HTTP context for the request</param>
        /// <returns>A task representing the middleware execution</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Skip exception handling for excluded paths
            string path = context.Request.Path.Value?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(path) && 
                _options.ExcludedPaths.Any(p => path.StartsWith(p.ToLowerInvariant())))
            {
                await _next(context);
                return;
            }

            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception exception)
            {
                // Handle the exception
                await HandleExceptionAsync(context, exception);
            }
        }

        /// <summary>
        /// Handles an exception by creating an appropriate error response and determining the HTTP status code.
        /// </summary>
        /// <param name="context">The HTTP context for the request</param>
        /// <param name="exception">The exception to handle</param>
        /// <returns>A task representing the exception handling</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Determine the appropriate HTTP status code
            int statusCode = GetStatusCode(exception);

            // Extract error details from the exception
            string errorMessage = ShouldIncludeDetails() 
                ? exception.GetDetailedMessage() 
                : exception.Message;
                
            string errorCode = exception.GetErrorCode();
            
            // In production, use more generic error messages for 500-level errors
            // unless specifically configured to include details
            if (statusCode >= 500 && !ShouldIncludeDetails())
            {
                errorMessage = "An unexpected error occurred. Please try again later.";
            }
            
            // Create an error response
            var response = ApiResponse.CreateError(
                message: errorMessage,
                errorCode: errorCode,
                statusCode: statusCode);

            // Add validation errors for ValidationException
            if (exception is ValidationException validationException && validationException.ValidationErrors != null)
            {
                response.AddValidationErrors(validationException.ValidationErrors);
            }

            // Add request ID for tracking
            if (context.TraceIdentifier != null)
            {
                response.AddMetadata("requestId", context.TraceIdentifier);
            }

            // Add additional context for specific error types if detailed info is allowed
            if (ShouldIncludeDetails())
            {
                response.AddMetadata("exceptionType", exception.GetType().Name);
                
                // Add stack trace if configured
                if (_options.LogStackTrace && !string.IsNullOrEmpty(exception.StackTrace))
                {
                    response.AddMetadata("stackTrace", exception.StackTrace);
                }

                // Add inner exception details if present
                if (exception.InnerException != null)
                {
                    response.AddMetadata("innerException", exception.InnerException.Message);
                }
            }

            // Log the exception with appropriate severity and context
            var logContext = new
            {
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = statusCode,
                ErrorCode = errorCode,
                RequestId = context.TraceIdentifier
            };

            if (statusCode >= 500)
            {
                _logger.LogError("An unhandled exception occurred", exception, logContext);
            }
            else
            {
                _logger.LogWarning($"Request failed with status code {statusCode}", exception, logContext);
            }

            // Set the response status code and content type
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // Serialize the error response to JSON and write to the response
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        /// <summary>
        /// Determines the appropriate HTTP status code based on the exception type.
        /// </summary>
        /// <param name="exception">The exception to map to a status code</param>
        /// <returns>The HTTP status code</returns>
        private int GetStatusCode(Exception exception)
        {
            if (exception == null)
            {
                return (int)HttpStatusCode.InternalServerError;
            }

            // Domain exceptions can carry their own status code mappings
            if (exception is DomainException domainException)
            {
                // Map domain error codes to HTTP status codes
                if (domainException.ErrorCode.EndsWith("NotFound", StringComparison.OrdinalIgnoreCase) || 
                    domainException.ErrorCode == ErrorCodes.General.NotFound)
                {
                    return (int)HttpStatusCode.NotFound;
                }
                
                if (domainException.ErrorCode == ErrorCodes.General.ValidationError ||
                    domainException.ErrorCode == ErrorCodes.General.BadRequest ||
                    domainException.ErrorCode.Contains("Invalid", StringComparison.OrdinalIgnoreCase))
                {
                    return (int)HttpStatusCode.BadRequest;
                }
                
                if (domainException.ErrorCode == ErrorCodes.General.Unauthorized ||
                    domainException.ErrorCode.StartsWith("AUTH", StringComparison.OrdinalIgnoreCase))
                {
                    return (int)HttpStatusCode.Unauthorized;
                }
                
                if (domainException.ErrorCode == ErrorCodes.General.Forbidden)
                {
                    return (int)HttpStatusCode.Forbidden;
                }
                
                if (domainException.ErrorCode == ErrorCodes.General.Conflict ||
                    domainException.ErrorCode.Contains("Duplicate", StringComparison.OrdinalIgnoreCase))
                {
                    return (int)HttpStatusCode.Conflict;
                }
                
                if (domainException.ErrorCode == ErrorCodes.General.Timeout)
                {
                    return (int)HttpStatusCode.RequestTimeout;
                }
                
                if (domainException.ErrorCode == ErrorCodes.General.ServiceUnavailable)
                {
                    return (int)HttpStatusCode.ServiceUnavailable;
                }
                
                // For other domain exceptions, use BadRequest by default
                return (int)HttpStatusCode.BadRequest;
            }

            // Map common exception types to appropriate status codes
            if (exception is ValidationException)
            {
                return (int)HttpStatusCode.BadRequest;
            }
            
            if (exception is UnauthorizedAccessException)
            {
                return (int)HttpStatusCode.Unauthorized;
            }
            
            if (exception is KeyNotFoundException)
            {
                return (int)HttpStatusCode.NotFound;
            }
            
            if (exception is TimeoutException)
            {
                return (int)HttpStatusCode.RequestTimeout;
            }
            
            if (exception is ArgumentException || 
                exception is FormatException ||
                exception is InvalidOperationException)
            {
                return (int)HttpStatusCode.BadRequest;
            }

            // Default to Internal Server Error for unhandled exceptions
            return (int)HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Determines if detailed exception information should be included in the response.
        /// </summary>
        /// <returns>True if details should be included, otherwise false</returns>
        private bool ShouldIncludeDetails()
        {
            // Include details in development environment or if explicitly enabled in options
            return _environment.IsDevelopment() || _options.IncludeDetails;
        }
    }

    /// <summary>
    /// Configuration options for the exception handling middleware.
    /// </summary>
    public class ExceptionHandlingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include detailed exception information in responses.
        /// This should typically be disabled in production environments.
        /// </summary>
        public bool IncludeDetails { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to log stack traces for exceptions.
        /// </summary>
        public bool LogStackTrace { get; set; } = true;

        /// <summary>
        /// Gets or sets a list of request paths to exclude from exception handling.
        /// </summary>
        public List<string> ExcludedPaths { get; set; } = new List<string>();
    }

    /// <summary>
    /// Extension methods for registering the ExceptionHandlingMiddleware in the ASP.NET Core pipeline.
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Adds the ExceptionHandlingMiddleware to the application's request pipeline.
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        /// <summary>
        /// Adds the ExceptionHandlingMiddleware to the application's request pipeline with custom options.
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <param name="configureOptions">Action to configure the exception handling options</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseExceptionHandling(
            this IApplicationBuilder builder, 
            Action<ExceptionHandlingOptions> configureOptions)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var options = new ExceptionHandlingOptions();
            configureOptions(options);

            return builder.UseMiddleware<ExceptionHandlingMiddleware>(Options.Create(options));
        }
    }
}