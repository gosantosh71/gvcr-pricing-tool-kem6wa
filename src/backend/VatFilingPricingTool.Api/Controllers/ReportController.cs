using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Service.Interfaces;

namespace VatFilingPricingTool.Api.Controllers
{
    /// <summary>
    /// API controller that handles report generation, retrieval, and management operations
    /// </summary>
    [ApiController]
    [Route(ApiRoutes.Report.Base)]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        /// <summary>
        /// Initializes a new instance of the ReportController with required dependencies
        /// </summary>
        /// <param name="reportService">Service for handling report operations</param>
        /// <param name="logger">Logger for recording controller activities</param>
        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a new report based on calculation data with specified format and content options
        /// </summary>
        /// <param name="request">The report generation request containing parameters and options</param>
        /// <returns>HTTP response with report generation result</returns>
        [HttpPost(ApiRoutes.Report.Generate)]
        [ProducesResponseType(typeof(GenerateReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateReportAsync([FromBody] GenerateReportRequest request)
        {
            try
            {
                _logger.LogInformation("Generating report for calculation: {CalculationId}", request.CalculationId);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for report generation request");
                    return BadRequest(CreateErrorResponse("Invalid request parameters", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var userId = GetUserIdFromClaims();
                var result = await _reportService.GenerateReportAsync(request, userId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Report generation failed: {ErrorMessage}, ErrorCode: {ErrorCode}", 
                        result.ErrorMessage, result.ErrorCode);
                    
                    if (result.ErrorCode == ErrorCodes.Report.ReportNotFound || 
                        result.ErrorCode == ErrorCodes.Pricing.CalculationNotFound)
                    {
                        return NotFound(CreateErrorResponse(result.ErrorMessage, result.ValidationErrors));
                    }
                    
                    return BadRequest(CreateErrorResponse(result.ErrorMessage, result.ValidationErrors));
                }

                _logger.LogInformation("Report generated successfully: {ReportId}", result.Value.ReportId);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating report");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An unexpected error occurred while generating the report. Please try again later."));
            }
        }

        /// <summary>
        /// Retrieves a specific report by ID with detailed information
        /// </summary>
        /// <param name="reportId">ID of the report to retrieve</param>
        /// <returns>HTTP response with report information</returns>
        [HttpGet(ApiRoutes.Report.GetById)]
        [ProducesResponseType(typeof(GetReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReportAsync(string reportId)
        {
            try
            {
                _logger.LogInformation("Retrieving report: {ReportId}", reportId);

                if (string.IsNullOrEmpty(reportId))
                {
                    _logger.LogWarning("Report ID is null or empty");
                    return BadRequest(CreateErrorResponse("Report ID is required"));
                }

                var userId = GetUserIdFromClaims();
                var request = new GetReportRequest { ReportId = reportId };
                var result = await _reportService.GetReportAsync(request, userId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Report retrieval failed: {ErrorMessage}, ErrorCode: {ErrorCode}", 
                        result.ErrorMessage, result.ErrorCode);
                    
                    if (result.ErrorCode == ErrorCodes.Report.ReportNotFound)
                    {
                        return NotFound(CreateErrorResponse(result.ErrorMessage));
                    }
                    
                    return BadRequest(CreateErrorResponse(result.ErrorMessage, result.ValidationErrors));
                }

                _logger.LogInformation("Report retrieved successfully: {ReportId}", result.Value.ReportId);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving report: {ReportId}", reportId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An unexpected error occurred while retrieving the report. Please try again later."));
            }
        }

        /// <summary>
        /// Retrieves a paginated list of reports for the current user with optional filtering
        /// </summary>
        /// <param name="request">Request parameters including pagination and filtering options</param>
        /// <returns>HTTP response with paginated report history</returns>
        [HttpGet(ApiRoutes.Report.GetAll)]
        [ProducesResponseType(typeof(GetReportHistoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReportHistoryAsync([FromQuery] GetReportHistoryRequest request)
        {
            try
            {
                _logger.LogInformation("Retrieving report history, Page: {Page}, PageSize: {PageSize}", 
                    request.Page, request.PageSize);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for report history request");
                    return BadRequest(CreateErrorResponse("Invalid request parameters", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var userId = GetUserIdFromClaims();
                var result = await _reportService.GetReportHistoryAsync(request, userId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Report history retrieval failed: {ErrorMessage}, ErrorCode: {ErrorCode}", 
                        result.ErrorMessage, result.ErrorCode);
                    return BadRequest(CreateErrorResponse(result.ErrorMessage, result.ValidationErrors));
                }

                _logger.LogInformation("Report history retrieved successfully, TotalCount: {TotalCount}", 
                    result.Value.TotalCount);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving report history");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An unexpected error occurred while retrieving report history. Please try again later."));
            }
        }

        /// <summary>
        /// Downloads a specific report, optionally converting to a different format
        /// </summary>
        /// <param name="reportId">ID of the report to download</param>
        /// <param name="format">Optional format to convert the report to</param>
        /// <returns>HTTP response with report file content</returns>
        [HttpGet(ApiRoutes.Report.Download)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadReportAsync(string reportId, string format = null)
        {
            try
            {
                _logger.LogInformation("Downloading report: {ReportId}, Format: {Format}", reportId, format ?? "Original");

                if (string.IsNullOrEmpty(reportId))
                {
                    _logger.LogWarning("Report ID is null or empty");
                    return BadRequest(CreateErrorResponse("Report ID is required"));
                }

                var userId = GetUserIdFromClaims();
                var request = new DownloadReportRequest 
                { 
                    ReportId = reportId 
                };

                // If a format was specified, try to parse it
                if (!string.IsNullOrEmpty(format))
                {
                    if (Enum.TryParse<ReportFormat>(format, true, out var reportFormat))
                    {
                        request.Format = reportFormat;
                    }
                    else
                    {
                        _logger.LogWarning("Invalid report format specified: {Format}", format);
                        return BadRequest(CreateErrorResponse($"Invalid report format: {format}"));
                    }
                }

                var result = await _reportService.DownloadReportAsync(request, userId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Report download failed: {ErrorMessage}, ErrorCode: {ErrorCode}", 
                        result.ErrorMessage, result.ErrorCode);
                    
                    if (result.ErrorCode == ErrorCodes.Report.ReportNotFound)
                    {
                        return NotFound(CreateErrorResponse(result.ErrorMessage));
                    }
                    
                    return BadRequest(CreateErrorResponse(result.ErrorMessage, result.ValidationErrors));
                }

                _logger.LogInformation("Report downloaded successfully: {ReportId}, FileName: {FileName}", 
                    result.Value.ReportId, result.Value.FileName);
                return File(result.Value.FileContent, result.Value.ContentType, result.Value.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while downloading report: {ReportId}", reportId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An unexpected error occurred while downloading the report. Please try again later."));
            }
        }

        /// <summary>
        /// Emails a specific report to the specified email address
        /// </summary>
        /// <param name="request">Email request containing recipient details and customization options</param>
        /// <returns>HTTP response with email result</returns>
        [HttpPost(ApiRoutes.Report.Email)]
        [ProducesResponseType(typeof(EmailReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmailReportAsync([FromBody] EmailReportRequest request)
        {
            try
            {
                _logger.LogInformation("Emailing report: {ReportId} to {EmailAddress}", 
                    request.ReportId, request.EmailAddress);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for email report request");
                    return BadRequest(CreateErrorResponse("Invalid request parameters", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var userId = GetUserIdFromClaims();
                var result = await _reportService.EmailReportAsync(request, userId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Report email failed: {ErrorMessage}, ErrorCode: {ErrorCode}", 
                        result.ErrorMessage, result.ErrorCode);
                    
                    if (result.ErrorCode == ErrorCodes.Report.ReportNotFound)
                    {
                        return NotFound(CreateErrorResponse(result.ErrorMessage));
                    }
                    
                    return BadRequest(CreateErrorResponse(result.ErrorMessage, result.ValidationErrors));
                }

                _logger.LogInformation("Report emailed successfully: {ReportId} to {EmailAddress}", 
                    request.ReportId, request.EmailAddress);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while emailing report: {ReportId}", request.ReportId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An unexpected error occurred while emailing the report. Please try again later."));
            }
        }

        /// <summary>
        /// Archives a specific report to hide it from regular report listings
        /// </summary>
        /// <param name="reportId">ID of the report to archive</param>
        /// <returns>HTTP response with archive result</returns>
        [HttpPut(ApiRoutes.Report.Archive)]
        [ProducesResponseType(typeof(ArchiveReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ArchiveReportAsync(string reportId)
        {
            try
            {
                _logger.LogInformation("Archiving report: {ReportId}", reportId);

                if (string.IsNullOrEmpty(reportId))
                {
                    _logger.LogWarning("Report ID is null or empty");
                    return BadRequest(CreateErrorResponse("Report ID is required"));
                }

                var userId = GetUserIdFromClaims();
                var request = new ArchiveReportRequest { ReportId = reportId };
                var result = await _reportService.ArchiveReportAsync(request, userId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Report archiving failed: {ErrorMessage}, ErrorCode: {ErrorCode}", 
                        result.ErrorMessage, result.ErrorCode);
                    
                    if (result.ErrorCode == ErrorCodes.Report.ReportNotFound)
                    {
                        return NotFound(CreateErrorResponse(result.ErrorMessage));
                    }
                    
                    return BadRequest(CreateErrorResponse(result.ErrorMessage, result.ValidationErrors));
                }

                _logger.LogInformation("Report archived successfully: {ReportId}", reportId);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while archiving report: {ReportId}", reportId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An unexpected error occurred while archiving the report. Please try again later."));
            }
        }

        /// <summary>
        /// Unarchives a previously archived report to make it visible in regular report listings
        /// </summary>
        /// <param name="reportId">ID of the report to unarchive</param>
        /// <returns>HTTP response with unarchive result</returns>
        [HttpPut(ApiRoutes.Report.Unarchive)]
        [ProducesResponseType(typeof(ArchiveReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnarchiveReportAsync(string reportId)
        {
            try
            {
                _logger.LogInformation("Unarchiving report: {ReportId}", reportId);

                if (string.IsNullOrEmpty(reportId))
                {
                    _logger.LogWarning("Report ID is null or empty");
                    return BadRequest(CreateErrorResponse("Report ID is required"));
                }

                var userId = GetUserIdFromClaims();
                var result = await _reportService.UnarchiveReportAsync(reportId, userId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Report unarchiving failed: {ErrorMessage}, ErrorCode: {ErrorCode}", 
                        result.ErrorMessage, result.ErrorCode);
                    
                    if (result.ErrorCode == ErrorCodes.Report.ReportNotFound)
                    {
                        return NotFound(CreateErrorResponse(result.ErrorMessage));
                    }
                    
                    return BadRequest(CreateErrorResponse(result.ErrorMessage, result.ValidationErrors));
                }

                _logger.LogInformation("Report unarchived successfully: {ReportId}", reportId);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while unarchiving report: {ReportId}", reportId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An unexpected error occurred while unarchiving the report. Please try again later."));
            }
        }

        /// <summary>
        /// Permanently deletes a report and its associated file from storage
        /// </summary>
        /// <param name="reportId">ID of the report to delete</param>
        /// <returns>HTTP response with delete result</returns>
        [HttpDelete(ApiRoutes.Report.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteReportAsync(string reportId)
        {
            try
            {
                _logger.LogInformation("Deleting report: {ReportId}", reportId);

                if (string.IsNullOrEmpty(reportId))
                {
                    _logger.LogWarning("Report ID is null or empty");
                    return BadRequest(CreateErrorResponse("Report ID is required"));
                }

                var userId = GetUserIdFromClaims();
                var result = await _reportService.DeleteReportAsync(reportId, userId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Report deletion failed: {ErrorMessage}, ErrorCode: {ErrorCode}", 
                        result.ErrorMessage, result.ErrorCode);
                    
                    if (result.ErrorCode == ErrorCodes.Report.ReportNotFound)
                    {
                        return NotFound(CreateErrorResponse(result.ErrorMessage));
                    }
                    
                    return BadRequest(CreateErrorResponse(result.ErrorMessage, result.ValidationErrors));
                }

                _logger.LogInformation("Report deleted successfully: {ReportId}", reportId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting report: {ReportId}", reportId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An unexpected error occurred while deleting the report. Please try again later."));
            }
        }

        /// <summary>
        /// Extracts the user ID from the HTTP context claims
        /// </summary>
        /// <returns>The user ID from claims</returns>
        private string GetUserIdFromClaims()
        {
            var claimsPrincipal = HttpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) ?? 
                              claimsPrincipal.FindFirst("sub");

            if (userIdClaim == null)
            {
                throw new InvalidOperationException("User ID claim not found");
            }

            return userIdClaim.Value;
        }

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errors">Optional list of validation errors</param>
        /// <returns>Standardized error response object</returns>
        private object CreateErrorResponse(string message, List<string> errors = null)
        {
            return new
            {
                success = false,
                message,
                errors = errors ?? new List<string>()
            };
        }
    }
}