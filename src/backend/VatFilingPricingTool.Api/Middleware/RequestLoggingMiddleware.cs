using Microsoft.AspNetCore.Http; // v6.0.0
using Microsoft.Extensions.Options; // v6.0.0
using Microsoft.IO; // v2.2.0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VatFilingPricingTool.Infrastructure.Logging;

namespace VatFilingPricingTool.Api.Middleware
{
    /// <summary>
    /// Middleware that logs HTTP requests and responses with detailed information including
    /// request path, method, headers, response status code, and execution time.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _logger;
        private readonly RequestLoggingOptions _options;
        private readonly RecyclableMemoryStreamManager _streamManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logging service.</param>
        /// <param name="options">The request logging options.</param>
        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILoggingService logger,
            IOptions<RequestLoggingOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _streamManager = new RecyclableMemoryStreamManager();
        }

        /// <summary>
        /// Processes the HTTP request, logs request details, invokes the next middleware, and logs response details.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the middleware execution.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Check if the current path should be excluded from logging
            if (ShouldExcludePath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Start a stopwatch to measure request execution time
            var stopwatch = Stopwatch.StartNew();

            // Get or generate a correlation ID for request tracking
            var correlationId = GetCorrelationId(context);

            // Log request information
            var requestBody = await GetRequestBody(context.Request);
            _logger.LogInformation(
                $"HTTP Request: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}",
                new
                {
                    Protocol = context.Request.Protocol,
                    Scheme = context.Request.Scheme,
                    Host = context.Request.Host.ToString(),
                    Path = context.Request.Path.ToString(),
                    QueryString = context.Request.QueryString.ToString(),
                    Method = context.Request.Method,
                    ContentType = context.Request.ContentType,
                    ContentLength = context.Request.ContentLength,
                    Headers = context.Request.Headers
                        .Where(h => !h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(h => h.Key, h => h.Value.ToString()),
                    Body = requestBody,
                    RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString()
                },
                correlationId);

            // Store the original response body stream
            var originalBodyStream = context.Response.Body;

            // Create a new memory stream to capture the response
            using var responseBodyStream = _streamManager.GetStream();
            context.Response.Body = responseBodyStream;

            try
            {
                // Call the next middleware in the pipeline
                await _next(context);

                // Log response information
                var responseBody = await GetResponseBody(responseBodyStream);
                stopwatch.Stop();

                // Don't log binary response content types
                var contentType = context.Response.ContentType ?? string.Empty;
                var shouldLogResponseBody = _options.LogResponseBody && 
                    !_options.ExcludedContentTypes.Any(t => contentType.Contains(t, StringComparison.OrdinalIgnoreCase));

                _logger.LogInformation(
                    $"HTTP Response: {context.Response.StatusCode} ({stopwatch.ElapsedMilliseconds}ms)",
                    new
                    {
                        StatusCode = context.Response.StatusCode,
                        ContentType = context.Response.ContentType,
                        ContentLength = context.Response.ContentLength,
                        Headers = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                        Body = shouldLogResponseBody ? responseBody : "[Content not logged]",
                        ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
                    },
                    correlationId);

                // Copy the captured response body to the original stream
                responseBodyStream.Position = 0;
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            finally
            {
                // Restore the original response body stream
                context.Response.Body = originalBodyStream;
            }
        }

        /// <summary>
        /// Reads and returns the request body as a string while preserving the original stream.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The request body as a string.</returns>
        private async Task<string> GetRequestBody(HttpRequest request)
        {
            // Check if request body logging is enabled
            if (!_options.LogRequestBody)
            {
                return string.Empty;
            }

            // Check if the request body is empty or not readable
            if (!request.Body.CanRead || request.ContentLength == 0)
            {
                return string.Empty;
            }

            // Don't log binary content types
            if (request.ContentType != null && 
                _options.ExcludedContentTypes.Any(t => request.ContentType.Contains(t, StringComparison.OrdinalIgnoreCase)))
            {
                return "[Content type excluded from logging]";
            }

            // Create a memory stream using the stream manager
            using var requestBodyStream = _streamManager.GetStream();
            
            // Copy the request body to the memory stream
            await request.Body.CopyToAsync(requestBodyStream);
            
            // Reset the request body position
            request.Body.Position = 0;
            
            // Read the memory stream content as a string
            requestBodyStream.Position = 0;
            string body;
            using (var reader = new StreamReader(requestBodyStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                
                // Truncate the body if it exceeds the maximum size
                if (body.Length > _options.MaxBodyLogSize)
                {
                    body = body.Substring(0, _options.MaxBodyLogSize) + "... [truncated]";
                }
            }
            
            // Reset the memory stream position
            requestBodyStream.Position = 0;
            
            // Copy the memory stream back to the original request body stream
            await requestBodyStream.CopyToAsync(request.Body);
            
            // Reset the request body position again
            request.Body.Position = 0;
            
            return body;
        }

        /// <summary>
        /// Reads and returns the response body as a string from the memory stream.
        /// </summary>
        /// <param name="bodyStream">The response body stream.</param>
        /// <returns>The response body as a string.</returns>
        private async Task<string> GetResponseBody(Stream bodyStream)
        {
            // Check if response body logging is enabled
            if (!_options.LogResponseBody)
            {
                return string.Empty;
            }

            // Reset the body stream position
            bodyStream.Position = 0;
            
            // Read the stream content as a string
            using var reader = new StreamReader(bodyStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            
            // Truncate the body if it exceeds the maximum size
            if (body.Length > _options.MaxBodyLogSize)
            {
                body = body.Substring(0, _options.MaxBodyLogSize) + "... [truncated]";
            }
            
            // Reset the body stream position
            bodyStream.Position = 0;
            
            return body;
        }

        /// <summary>
        /// Extracts or generates a correlation ID for request tracking.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The correlation ID.</returns>
        private string GetCorrelationId(HttpContext context)
        {
            // Check if the correlation ID header exists in the request
            if (context.Request.Headers.TryGetValue(_options.CorrelationIdHeaderName, out var correlationIdValues))
            {
                var correlationId = correlationIdValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(correlationId))
                {
                    // Add the correlation ID to the response headers
                    context.Response.Headers[_options.CorrelationIdHeaderName] = correlationId;
                    return correlationId;
                }
            }

            // Generate a new correlation ID
            var newCorrelationId = Guid.NewGuid().ToString();
            
            // Add the generated correlation ID to the response headers
            context.Response.Headers[_options.CorrelationIdHeaderName] = newCorrelationId;
            
            return newCorrelationId;
        }

        /// <summary>
        /// Determines if the current request path should be excluded from logging.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <returns>True if the path should be excluded, otherwise false.</returns>
        private bool ShouldExcludePath(string path)
        {
            if (string.IsNullOrEmpty(path) || _options.ExcludedPaths == null || !_options.ExcludedPaths.Any())
            {
                return false;
            }

            return _options.ExcludedPaths.Any(excludedPath =>
            {
                if (excludedPath.EndsWith('*'))
                {
                    // Wildcard match
                    var prefix = excludedPath.TrimEnd('*');
                    return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                }
                
                // Exact match
                return path.Equals(excludedPath, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    /// <summary>
    /// Configuration options for the request logging middleware.
    /// </summary>
    public class RequestLoggingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to log request bodies.
        /// </summary>
        public bool LogRequestBody { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log response bodies.
        /// </summary>
        public bool LogResponseBody { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of body content to log in bytes.
        /// </summary>
        public int MaxBodyLogSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the header used for correlation ID.
        /// </summary>
        public string CorrelationIdHeaderName { get; set; }

        /// <summary>
        /// Gets the collection of paths that should be excluded from logging.
        /// </summary>
        public List<string> ExcludedPaths { get; set; }

        /// <summary>
        /// Gets the collection of content types that should be excluded from body logging.
        /// </summary>
        public List<string> ExcludedContentTypes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggingOptions"/> class with default values.
        /// </summary>
        public RequestLoggingOptions()
        {
            LogRequestBody = false;
            LogResponseBody = false;
            MaxBodyLogSize = 4096;
            CorrelationIdHeaderName = "X-Correlation-ID";
            ExcludedPaths = new List<string>
            {
                "/health",
                "/metrics",
                "/favicon.ico",
                "/swagger/*"
            };
            ExcludedContentTypes = new List<string>
            {
                "application/octet-stream",
                "application/pdf",
                "image/",
                "audio/",
                "video/"
            };
        }
    }

    /// <summary>
    /// Extension methods for registering the <see cref="RequestLoggingMiddleware"/> in the ASP.NET Core pipeline.
    /// </summary>
    public static class RequestLoggingMiddlewareExtensions
    {
        /// <summary>
        /// Adds the <see cref="RequestLoggingMiddleware"/> to the application's request pipeline.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        /// <summary>
        /// Adds the <see cref="RequestLoggingMiddleware"/> to the application's request pipeline with custom options.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="configureOptions">A delegate to configure the <see cref="RequestLoggingOptions"/>.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseRequestLogging(
            this IApplicationBuilder builder,
            Action<RequestLoggingOptions> configureOptions)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var options = new RequestLoggingOptions();
            configureOptions(options);

            return builder.UseMiddleware<RequestLoggingMiddleware>(Options.Create(options));
        }
    }
}