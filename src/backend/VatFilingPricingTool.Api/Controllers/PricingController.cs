using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Api.Models.Responses;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Service.Interfaces;

namespace VatFilingPricingTool.Api.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Pricing.Base)]
    [Authorize]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        private readonly ILogger<PricingController> _logger;

        /// <summary>
        /// Initializes a new instance of the PricingController with required dependencies
        /// </summary>
        /// <param name="pricingService">Service for calculating and managing VAT filing costs</param>
        /// <param name="logger">Logger for recording controller operations</param>
        public PricingController(IPricingService pricingService, ILogger<PricingController> logger)
        {
            _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculates VAT filing costs based on the provided parameters
        /// </summary>
        /// <param name="request">The calculation request containing service type, transaction volume, filing frequency, countries, and additional services</param>
        /// <returns>HTTP response with detailed calculation result</returns>
        [HttpPost(ApiRoutes.Pricing.Calculate)]
        [ProducesResponseType(typeof(CalculationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalculatePricingAsync([FromBody] CalculationRequest request)
        {
            _logger.LogInformation("Calculating pricing for service type: {ServiceType}, transaction volume: {TransactionVolume}", 
                request.ServiceType, request.TransactionVolume);
            
            if (!ModelState.IsValid)
            {
                return CreateErrorResponse(
                    "Request validation failed", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }
            
            try
            {
                // Map API request to service contract request
                var calculateRequest = new VatFilingPricingTool.Contracts.V1.Requests.CalculateRequest
                {
                    ServiceType = request.ServiceType,
                    TransactionVolume = request.TransactionVolume,
                    Frequency = request.Frequency,
                    CountryCodes = request.CountryCodes,
                    AdditionalServices = request.AdditionalServices
                };
                
                // Call pricing service
                var result = await _pricingService.CalculatePricingAsync(calculateRequest);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Pricing calculation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Pricing calculation failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }
                
                // Map service response to API response
                var response = CalculationResponse.FromModel(result.Value);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating pricing");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "An error occurred while processing your request",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Retrieves a specific calculation by its ID
        /// </summary>
        /// <param name="request">The request containing the calculation ID to retrieve</param>
        /// <returns>HTTP response with calculation details</returns>
        [HttpGet(ApiRoutes.Pricing.GetById)]
        [ProducesResponseType(typeof(CalculationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCalculationAsync([FromRoute] GetCalculationRequest request)
        {
            _logger.LogInformation("Getting calculation with ID: {CalculationId}", request.CalculationId);
            
            if (!ModelState.IsValid)
            {
                return CreateErrorResponse(
                    "Request validation failed", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }
            
            try
            {
                // Create service request
                var getRequest = new VatFilingPricingTool.Contracts.V1.Requests.GetCalculationRequest
                {
                    CalculationId = request.CalculationId
                };
                
                // Call pricing service
                var result = await _pricingService.GetCalculationAsync(getRequest);
                
                if (!result.IsSuccess)
                {
                    if (result.ErrorCode == ErrorCodes.Pricing.CalculationNotFound)
                    {
                        return NotFound(new ProblemDetails
                        {
                            Title = "Calculation not found",
                            Detail = result.ErrorMessage,
                            Status = StatusCodes.Status404NotFound
                        });
                    }
                    
                    _logger.LogWarning("Get calculation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Get calculation failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }
                
                // Map service response to API response
                var response = CalculationResponse.FromModel(result.Value);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving calculation");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "An error occurred while processing your request",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Saves a calculation result for future reference
        /// </summary>
        /// <param name="request">The save calculation request containing calculation details</param>
        /// <returns>HTTP response with save operation result</returns>
        [HttpPost(ApiRoutes.Pricing.Save)]
        [ProducesResponseType(typeof(SaveCalculationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveCalculationAsync([FromBody] SaveCalculationRequest request)
        {
            _logger.LogInformation("Saving calculation for service type: {ServiceType}, transaction volume: {TransactionVolume}", 
                request.ServiceType, request.TransactionVolume);
            
            if (!ModelState.IsValid)
            {
                return CreateErrorResponse(
                    "Request validation failed", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }
            
            try
            {
                // Get user ID from claims
                string userId = GetUserIdFromClaims();
                
                // Map API request to service contract request
                var saveRequest = new VatFilingPricingTool.Contracts.V1.Requests.SaveCalculationRequest
                {
                    ServiceType = request.ServiceType,
                    TransactionVolume = request.TransactionVolume,
                    Frequency = request.Frequency,
                    TotalCost = request.TotalCost,
                    CurrencyCode = request.CurrencyCode,
                    CountryBreakdowns = request.CountryBreakdowns.Select(c => new VatFilingPricingTool.Contracts.V1.Requests.CountryBreakdownRequest
                    {
                        CountryCode = c.CountryCode,
                        CountryName = c.CountryName,
                        BaseCost = c.BaseCost,
                        AdditionalCost = c.AdditionalCost,
                        TotalCost = c.TotalCost,
                        AppliedRules = c.AppliedRules
                    }).ToList(),
                    AdditionalServices = request.AdditionalServices,
                    Discounts = request.Discounts
                };
                
                // Call pricing service
                var result = await _pricingService.SaveCalculationAsync(saveRequest, userId);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Save calculation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Save calculation failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }
                
                // Create response
                var response = new SaveCalculationResponse
                {
                    CalculationId = result.Value.CalculationId,
                    CalculationDate = result.Value.CalculationDate
                };
                
                return Created($"{ApiRoutes.Pricing.Base}/{response.CalculationId}", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving calculation");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "An error occurred while processing your request",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Retrieves calculation history for the current user with optional filtering
        /// </summary>
        /// <param name="request">The request containing filtering parameters for history retrieval</param>
        /// <returns>HTTP response with calculation history</returns>
        [HttpGet(ApiRoutes.Pricing.History)]
        [ProducesResponseType(typeof(CalculationHistoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCalculationHistoryAsync([FromQuery] GetCalculationHistoryRequest request)
        {
            _logger.LogInformation("Getting calculation history for page: {Page}, page size: {PageSize}", 
                request.Page, request.PageSize);
            
            if (!ModelState.IsValid)
            {
                return CreateErrorResponse(
                    "Request validation failed", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }
            
            try
            {
                // Get user ID from claims
                string userId = GetUserIdFromClaims();
                
                // Map API request to service contract request
                var historyRequest = new VatFilingPricingTool.Contracts.V1.Requests.GetCalculationHistoryRequest
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    CountryCodes = request.CountryCodes,
                    ServiceType = request.ServiceType,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
                
                // Call pricing service
                var result = await _pricingService.GetCalculationHistoryAsync(historyRequest, userId);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Get calculation history failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Get calculation history failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }
                
                // Map service response to API response using static method
                var response = CalculationHistoryResponse.FromPagedList(result.Value);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving calculation history");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "An error occurred while processing your request",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Compares multiple calculation scenarios to help users identify the most cost-effective options
        /// </summary>
        /// <param name="request">The request containing multiple calculation scenarios to compare</param>
        /// <returns>HTTP response with detailed comparison results</returns>
        [HttpPost(ApiRoutes.Pricing.Compare)]
        [ProducesResponseType(typeof(CompareCalculationsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompareCalculationsAsync([FromBody] CompareCalculationsRequest request)
        {
            _logger.LogInformation("Comparing {ScenarioCount} calculation scenarios", request.Scenarios.Count);
            
            if (!ModelState.IsValid)
            {
                return CreateErrorResponse(
                    "Request validation failed", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }
            
            // Ensure we have at least two scenarios to compare
            if (request.Scenarios.Count < 2)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid request",
                    Detail = "At least two scenarios are required for comparison",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            
            try
            {
                // Map API request to service contract request
                var compareRequest = new VatFilingPricingTool.Contracts.V1.Requests.CompareCalculationsRequest
                {
                    Scenarios = request.Scenarios.Select(s => new VatFilingPricingTool.Contracts.V1.Requests.CalculationScenario
                    {
                        ScenarioId = s.ScenarioId,
                        ScenarioName = s.ScenarioName,
                        ServiceType = s.ServiceType,
                        TransactionVolume = s.TransactionVolume,
                        Frequency = s.Frequency,
                        CountryCodes = s.CountryCodes,
                        AdditionalServices = s.AdditionalServices
                    }).ToList()
                };
                
                // Call pricing service
                var result = await _pricingService.CompareCalculationsAsync(compareRequest);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Compare calculations failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Compare calculations failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }
                
                // Create API response using the result
                var response = new CompareCalculationsResponse
                {
                    Scenarios = result.Value.Scenarios,
                    TotalCostComparison = result.Value.TotalCostComparison,
                    CountryCostComparison = result.Value.CountryCostComparison
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while comparing calculations");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "An error occurred while processing your request",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Deletes a specific calculation by its ID
        /// </summary>
        /// <param name="request">The request containing the calculation ID to delete</param>
        /// <returns>HTTP response with delete operation result</returns>
        [HttpDelete(ApiRoutes.Pricing.Delete)]
        [ProducesResponseType(typeof(DeleteCalculationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCalculationAsync([FromRoute] DeleteCalculationRequest request)
        {
            _logger.LogInformation("Deleting calculation with ID: {CalculationId}", request.CalculationId);
            
            if (!ModelState.IsValid)
            {
                return CreateErrorResponse(
                    "Request validation failed", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
            }
            
            try
            {
                // Get user ID from claims
                string userId = GetUserIdFromClaims();
                
                // Call pricing service
                var result = await _pricingService.DeleteCalculationAsync(request.CalculationId, userId);
                
                if (!result.IsSuccess)
                {
                    if (result.ErrorCode == ErrorCodes.Pricing.CalculationNotFound)
                    {
                        return NotFound(new ProblemDetails
                        {
                            Title = "Calculation not found",
                            Detail = result.ErrorMessage,
                            Status = StatusCodes.Status404NotFound
                        });
                    }
                    
                    _logger.LogWarning("Delete calculation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Delete calculation failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }
                
                // Create response
                var response = new DeleteCalculationResponse
                {
                    CalculationId = request.CalculationId,
                    Deleted = true
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting calculation");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "An error occurred while processing your request",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
        
        /// <summary>
        /// Extracts the user ID from the claims principal
        /// </summary>
        /// <returns>The user ID from claims</returns>
        private string GetUserIdFromClaims()
        {
            var claimsPrincipal = HttpContext.User;
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) 
                ?? claimsPrincipal.FindFirst("sub");
                
            if (userIdClaim == null)
            {
                throw new InvalidOperationException("Unable to get user ID from claims principal");
            }
            
            return userIdClaim.Value;
        }
        
        /// <summary>
        /// Creates a standardized error response for validation errors
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errors">List of validation errors</param>
        /// <returns>Standardized error response with appropriate status code</returns>
        private ObjectResult CreateErrorResponse(string message, List<string> errors)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "One or more validation errors occurred",
                Status = StatusCodes.Status400BadRequest,
                Detail = message
            };
            
            problemDetails.Extensions["errors"] = errors;
            
            return BadRequest(problemDetails);
        }
    }
}