using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; // version 6.0.0
using Microsoft.AspNetCore.Http; // version 6.0.0
using Microsoft.Extensions.Hosting; // version 6.0.0
using Microsoft.Extensions.Logging; // version 6.0.0
using System;
using System.Net;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Infrastructure.Logging;

namespace VatFilingPricingTool.Api.Filters
{
    /// <summary>
    /// ASP.NET Core exception filter that catches and handles exceptions thrown during controller action execution.
    /// Ensures consistent error responses across all API endpoints by transforming exceptions into standardized ApiResponse objects
    /// with appropriate HTTP status codes.
    /// </summary>
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILoggingService _logger;
        private readonly IHostEnvironment _environment;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        /// <summary>
        /// Initializes a new instance of the ApiExceptionFilter with required dependencies.
        /// </summary>
        /// <param name="logger">Logging service for exception logging</param>
        /// <param name="environment">Hosting environment to determine if in development mode</param>
        public ApiExceptionFilter(ILoggingService logger, IHostEnvironment environment)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Executes when an unhandled exception occurs during controller action execution.
        /// </summary>
        /// <param name="context">The exception context containing the exception and HTTP context</param>
        public override void OnException(ExceptionContext context)
        {
            // Extract correlation ID from HttpContext if available
            string correlationId = GetCorrelationId(context.HttpContext);

            // Create additional context information for logging
            var logContext = new
            {
                Path = context.HttpContext?.Request?.Path.Value,
                Method = context.HttpContext?.Request?.Method,
                Controller = context.ActionDescriptor?.DisplayName
            };

            // Log the exception with appropriate severity level
            if (context.Exception is ValidationException)
            {
                _logger.LogWarning($"Validation error: {context.Exception.Message}", logContext, correlationId);
            }
            else if (context.Exception is DomainException)
            {
                _logger.LogError("Domain exception occurred", context.Exception, logContext, correlationId);
            }
            else if (context.Exception is OperationCanceledException)
            {
                _logger.LogWarning($"Request cancelled: {context.Exception.Message}", logContext, correlationId);
            }
            else
            {
                _logger.LogCritical("Unhandled exception occurred", context.Exception, logContext, correlationId);
            }

            // Determine the appropriate HTTP status code
            HttpStatusCode statusCode = GetStatusCode(context.Exception);

            // Create an error response
            ApiResponse errorResponse = GetErrorResponse(context.Exception, statusCode, correlationId);

            // Set the HTTP response status code
            context.HttpContext.Response.StatusCode = (int)statusCode;

            // Set the result
            context.Result = new ObjectResult(errorResponse);

            // Mark the exception as handled
            context.ExceptionHandled = true;
        }

        /// <summary>
        /// Determines the appropriate HTTP status code based on the exception type.
        /// </summary>
        /// <param name="exception">The exception to evaluate</param>
        /// <returns>The appropriate HTTP status code</returns>
        private HttpStatusCode GetStatusCode(Exception exception)
        {
            // Handle different exception types with appropriate status codes
            if (exception is ValidationException)
            {
                return HttpStatusCode.BadRequest;
            }
            
            if (exception is KeyNotFoundException)
            {
                return HttpStatusCode.NotFound;
            }
            
            if (exception is UnauthorizedAccessException)
            {
                return HttpStatusCode.Unauthorized;
            }

            if (exception is OperationCanceledException)
            {
                return HttpStatusCode.BadRequest;
            }
            
            if (exception is DomainException domainException)
            {
                // Map specific domain exception error codes to HTTP status codes
                switch (domainException.ErrorCode)
                {
                    case ErrorCodes.General.NotFound:
                    case var code when code?.EndsWith("NotFound") == true:
                        return HttpStatusCode.NotFound;
                        
                    case ErrorCodes.General.Unauthorized:
                    case var code when code?.StartsWith("AUTH") == true:
                        return HttpStatusCode.Unauthorized;
                        
                    case ErrorCodes.General.Forbidden:
                        return HttpStatusCode.Forbidden;
                        
                    case ErrorCodes.General.Conflict:
                    case var code when code?.Contains("Duplicate") == true:
                        return HttpStatusCode.Conflict;
                        
                    case ErrorCodes.General.ValidationError:
                        return HttpStatusCode.BadRequest;
                        
                    case ErrorCodes.General.ServiceUnavailable:
                        return HttpStatusCode.ServiceUnavailable;
                        
                    case ErrorCodes.General.Timeout:
                        return HttpStatusCode.RequestTimeout;
                        
                    default:
                        return HttpStatusCode.BadRequest;
                }
            }
            
            // Default to 500 Internal Server Error for unhandled exception types
            return HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Creates an appropriate error response based on the exception.
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="statusCode">The HTTP status code to return</param>
        /// <param name="correlationId">Correlation ID for tracking the request</param>
        /// <returns>A standardized API error response</returns>
        private ApiResponse GetErrorResponse(Exception exception, HttpStatusCode statusCode, string correlationId)
        {
            string errorCode;
            
            // Determine error code based on exception type
            if (exception is DomainException domainException)
            {
                errorCode = domainException.ErrorCode;
            }
            else if (exception is ValidationException)
            {
                errorCode = ErrorCodes.General.ValidationError;
            }
            else if (exception is KeyNotFoundException)
            {
                errorCode = ErrorCodes.General.NotFound;
            }
            else if (exception is UnauthorizedAccessException)
            {
                errorCode = ErrorCodes.General.Unauthorized;
            }
            else if (exception is OperationCanceledException)
            {
                errorCode = ErrorCodes.General.BadRequest;
            }
            else
            {
                errorCode = ErrorCodes.General.ServerError;
            }
            
            // Create the error response
            var response = ApiResponse.CreateError(
                message: exception.Message,
                errorCode: errorCode,
                statusCode: (int)statusCode);
            
            // Add validation errors if present
            if (exception is ValidationException validationException && validationException.ValidationErrors != null)
            {
                response.AddValidationErrors(validationException.ValidationErrors);
            }
            
            // In development environment, include stack trace for debugging
            if (_environment.IsDevelopment())
            {
                response.AddMetadata("StackTrace", exception.StackTrace);
                
                if (exception.InnerException != null)
                {
                    response.AddMetadata("InnerException", exception.InnerException.Message);
                }
            }
            
            // Add correlation ID if available
            if (!string.IsNullOrEmpty(correlationId))
            {
                response.AddMetadata("CorrelationId", correlationId);
            }
            
            return response;
        }
        
        /// <summary>
        /// Extracts the correlation ID from the HTTP context.
        /// </summary>
        /// <param name="httpContext">The current HTTP context</param>
        /// <returns>The correlation ID if found, otherwise null</returns>
        private string GetCorrelationId(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                return null;
            }
            
            // Check for correlation ID in request headers (standardized header name)
            if (httpContext.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) &&
                !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId;
            }
            
            // Also check for other common correlation ID header names
            if (httpContext.Request.Headers.TryGetValue("Request-Id", out correlationId) ||
                httpContext.Request.Headers.TryGetValue("X-Request-ID", out correlationId) ||
                httpContext.Request.Headers.TryGetValue("Correlation-ID", out correlationId))
            {
                return correlationId;
            }
            
            return null;
        }
    }
}