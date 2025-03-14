using Microsoft.AspNetCore.Mvc; // Microsoft.AspNetCore.Mvc package version 6.0.0
using Microsoft.AspNetCore.Authorization; // Microsoft.AspNetCore.Authorization package version 6.0.0
using Microsoft.AspNetCore.Http; // Microsoft.AspNetCore.Http package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using System.Security.Claims; // System.Security.Claims package version 6.0.0
using VatFilingPricingTool.Service.Interfaces; // Service interface for integration operations
using VatFilingPricingTool.Common.Constants; // Constants for API route paths
using VatFilingPricingTool.Api.Models.Requests; // API request model for creating/updating integration
using VatFilingPricingTool.Api.Models.Responses; // API response model for integration data
using VatFilingPricingTool.Infrastructure.Integration.ERP; // Parameters for data import operations
using VatFilingPricingTool.Infrastructure.Integration.OCR; // Options for document processing operations

namespace VatFilingPricingTool.Api.Controllers
{
    /// <summary>
    /// API controller that handles integration-related operations including creating, updating, testing, and utilizing connections to external systems
    /// </summary>
    [ApiController]
    [Route(ApiRoutes.Integration.Base)]
    [Authorize]
    public class IntegrationController : ControllerBase
    {
        private readonly IIntegrationService _integrationService;
        private readonly ILogger<IntegrationController> _logger;

        /// <summary>
        /// Initializes a new instance of the IntegrationController with required dependencies
        /// </summary>
        /// <param name="integrationService">Service for managing integration operations</param>
        /// <param name="logger">Logger for logging controller operations</param>
        public IntegrationController(IIntegrationService integrationService, ILogger<IntegrationController> logger)
        {
            // Validate that integrationService is not null
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));

            // Validate that logger is not null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all integrations configured in the system
        /// </summary>
        /// <returns>HTTP response with list of integrations</returns>
        [HttpGet(ApiRoutes.Integration.Get)]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(ApiIntegrationListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetIntegrationsAsync()
        {
            // Log the start of the operation
            _logger.LogInformation("Starting GetIntegrationsAsync operation");

            try
            {
                // Call _integrationService.GetIntegrationsAsync() to retrieve all integrations
                var integrations = await _integrationService.GetIntegrationsAsync();

                // Create ApiIntegrationListResponse using FromEntityList with the retrieved integrations
                var response = ApiIntegrationListResponse.FromEntityList(integrations).ToApiResponse();

                // Return Ok with the response converted to ApiResponse
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while retrieving integrations");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a specific integration by its ID
        /// </summary>
        /// <param name="id">The ID of the integration to retrieve</param>
        /// <returns>HTTP response with integration details</returns>
        [HttpGet(ApiRoutes.Integration.GetById)]
        [ProducesResponseType(typeof(ApiIntegrationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetIntegrationByIdAsync(string id)
        {
            // Log the start of the operation
            _logger.LogInformation("Starting GetIntegrationByIdAsync operation with ID: {IntegrationId}", id);

            try
            {
                // Validate that id is not null or empty
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogError("Integration ID is null or empty");
                    return BadRequest("Integration ID cannot be null or empty");
                }

                // Call _integrationService.GetIntegrationByIdAsync(id) to retrieve the integration
                var integration = await _integrationService.GetIntegrationByIdAsync(id);

                // If integration is null, return NotFound
                if (integration == null)
                {
                    _logger.LogWarning("Integration with ID {IntegrationId} not found", id);
                    return NotFound();
                }

                // Create ApiIntegrationResponse using FromEntity with the retrieved integration
                var response = ApiIntegrationResponse.FromEntity(integration).ToApiResponse();

                // Return Ok with the response converted to ApiResponse
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while retrieving integration with ID: {IntegrationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all integrations configured for the current user
        /// </summary>
        /// <returns>HTTP response with user's integrations</returns>
        [HttpGet("user")]
        [ProducesResponseType(typeof(ApiIntegrationListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserIntegrationsAsync()
        {
            // Log the start of the operation
            _logger.LogInformation("Starting GetUserIntegrationsAsync operation");

            try
            {
                // Get the current user ID from HttpContext.User claims
                var userId = GetCurrentUserId();

                // Call _integrationService.GetUserIntegrationsAsync(userId) to retrieve user's integrations
                var integrations = await _integrationService.GetUserIntegrationsAsync(userId);

                // Create ApiIntegrationListResponse using FromEntityList with the retrieved integrations
                var response = ApiIntegrationListResponse.FromEntityList(integrations).ToApiResponse();

                // Return Ok with the response converted to ApiResponse
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while retrieving user integrations");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Creates a new integration configuration for an external system
        /// </summary>
        /// <param name="request">The integration creation request</param>
        /// <returns>HTTP response with created integration</returns>
        [HttpPost(ApiRoutes.Integration.Create)]
        [ProducesResponseType(typeof(ApiIntegrationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateIntegrationAsync([FromBody] CreateIntegrationRequest request)
        {
            // Log the start of the operation
            _logger.LogInformation("Starting CreateIntegrationAsync operation");

            try
            {
                // Validate request model
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid create integration request");
                    return BadRequest(ModelState);
                }

                // Get the current user ID from HttpContext.User claims
                var userId = GetCurrentUserId();

                // Call _integrationService.CreateIntegrationAsync with userId, request.SystemType, request.ConnectionString, request.ApiKey, request.ApiEndpoint, and request.AdditionalSettings
                var integration = await _integrationService.CreateIntegrationAsync(
                    userId,
                    request.SystemType,
                    request.ConnectionString,
                    request.ApiKey,
                    request.ApiEndpoint,
                    request.AdditionalSettings);

                // Create ApiIntegrationResponse using FromEntity with the created integration
                var response = ApiIntegrationResponse.FromEntity(integration).ToApiResponse();

                // Return Created with the response converted to ApiResponse
                return CreatedAtAction(nameof(GetIntegrationByIdAsync), new { id = integration.IntegrationId }, response);
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while creating integration");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing integration configuration
        /// </summary>
        /// <param name="request">The integration update request</param>
        /// <returns>HTTP response with updated integration</returns>
        [HttpPut(ApiRoutes.Integration.Update)]
        [ProducesResponseType(typeof(ApiIntegrationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateIntegrationAsync([FromBody] UpdateIntegrationRequest request)
        {
            // Log the start of the operation
            _logger.LogInformation("Starting UpdateIntegrationAsync operation with ID: {IntegrationId}", request.IntegrationId);

            try
            {
                // Validate request model
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid update integration request");
                    return BadRequest(ModelState);
                }

                // Call _integrationService.UpdateIntegrationAsync with request.IntegrationId, request.ConnectionString, request.ApiKey, request.ApiEndpoint, and request.AdditionalSettings
                var integration = await _integrationService.UpdateIntegrationAsync(
                    request.IntegrationId,
                    request.ConnectionString,
                    request.ApiKey,
                    request.ApiEndpoint,
                    request.AdditionalSettings);

                // If result is null, return NotFound
                if (integration == null)
                {
                    _logger.LogWarning("Integration with ID {IntegrationId} not found", request.IntegrationId);
                    return NotFound();
                }

                // Create ApiIntegrationResponse using FromEntity with the updated integration
                var response = ApiIntegrationResponse.FromEntity(integration).ToApiResponse();

                // Return Ok with the response converted to ApiResponse
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while updating integration with ID: {IntegrationId}", request.IntegrationId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Deletes an integration configuration
        /// </summary>
        /// <param name="id">The ID of the integration to delete</param>
        /// <returns>HTTP response with deletion result</returns>
        [HttpDelete(ApiRoutes.Integration.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteIntegrationAsync(string id)
        {
            // Log the start of the operation
            _logger.LogInformation("Starting DeleteIntegrationAsync operation with ID: {IntegrationId}", id);

            try
            {
                // Validate that id is not null or empty
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogError("Integration ID is null or empty");
                    return BadRequest("Integration ID cannot be null or empty");
                }

                // Call _integrationService.DeleteIntegrationAsync(id) to delete the integration
                var result = await _integrationService.DeleteIntegrationAsync(id);

                // If result is false, return NotFound
                if (!result)
                {
                    _logger.LogWarning("Integration with ID {IntegrationId} not found", id);
                    return NotFound();
                }

                // Return NoContent for successful deletion
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while deleting integration with ID: {IntegrationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Tests the connection to an external system using the specified integration configuration
        /// </summary>
        /// <param name="request">The connection test request</param>
        /// <returns>HTTP response with connection test result</returns>
        [HttpPost(ApiRoutes.Integration.TestConnection)]
        [ProducesResponseType(typeof(ApiConnectionTestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> TestConnectionAsync([FromBody] TestConnectionRequest request)
        {
            // Log the start of the operation
            _logger.LogInformation("Starting TestConnectionAsync operation with ID: {IntegrationId}", request.IntegrationId);

            try
            {
                // Validate request model
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid test connection request");
                    return BadRequest(ModelState);
                }

                // Get the integration details using _integrationService.GetIntegrationByIdAsync(request.IntegrationId)
                var integration = await _integrationService.GetIntegrationByIdAsync(request.IntegrationId);

                // If integration is null, return NotFound
                if (integration == null)
                {
                    _logger.LogWarning("Integration with ID {IntegrationId} not found", request.IntegrationId);
                    return NotFound();
                }

                // Call _integrationService.TestConnectionAsync(request.IntegrationId) to test the connection
                var testResult = await _integrationService.TestConnectionAsync(request.IntegrationId);

                // Create ApiConnectionTestResponse based on the test result (success or failure)
                ApiConnectionTestResponse response;
                if (testResult)
                {
                    response = ApiConnectionTestResponse.CreateSuccess(request.IntegrationId, integration.SystemType);
                }
                else
                {
                    response = ApiConnectionTestResponse.CreateFailure(request.IntegrationId, integration.SystemType, "Connection test failed");
                }

                // Return Ok with the response converted to ApiResponse
                return Ok(response.ToApiResponse());
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while testing connection for integration with ID: {IntegrationId}", request.IntegrationId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Imports transaction data from an ERP or other external system using the specified integration
        /// </summary>
        /// <param name="request">The data import request</param>
        /// <returns>HTTP response with import result</returns>
        [HttpPost(ApiRoutes.Integration.Import)]
        [ProducesResponseType(typeof(ApiImportDataResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ImportDataAsync([FromBody] ImportDataRequest request)
        {
            // Log the start of the operation
            _logger.LogInformation("Starting ImportDataAsync operation with ID: {IntegrationId}", request.IntegrationId);

            try
            {
                // Validate request model
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid import data request");
                    return BadRequest(ModelState);
                }

                // Get the integration details using _integrationService.GetIntegrationByIdAsync(request.IntegrationId)
                var integration = await _integrationService.GetIntegrationByIdAsync(request.IntegrationId);

                // If integration is null, return NotFound
                if (integration == null)
                {
                    _logger.LogWarning("Integration with ID {IntegrationId} not found", request.IntegrationId);
                    return NotFound();
                }

                // Create ImportParameters object with request properties
                var parameters = new ImportParameters
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    EntityType = request.EntityType,
                    MaxRecords = request.MaxRecords,
                    AdditionalParameters = request.AdditionalParameters
                };

                // Call _integrationService.ImportDataAsync(request.IntegrationId, parameters) to import data
                var importResult = await _integrationService.ImportDataAsync(request.IntegrationId, parameters);

                // Create ApiImportDataResponse using FromImportResult with the import result
                var response = ApiImportDataResponse.FromImportResult(importResult).ToApiResponse();

                // Return Ok with the response converted to ApiResponse
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while importing data for integration with ID: {IntegrationId}", request.IntegrationId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Processes a document using OCR services to extract structured data from invoices and VAT forms
        /// </summary>
        /// <param name="request">The document processing request</param>
        /// <returns>HTTP response with document processing result</returns>
        [HttpPost(ApiRoutes.Integration.ProcessDocument)]
        [ProducesResponseType(typeof(ApiDocumentProcessingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ProcessDocumentAsync([FromBody] ProcessDocumentRequest request)
        {
            // Log the start of the operation
            _logger.LogInformation("Starting ProcessDocumentAsync operation with ID: {IntegrationId}", request.IntegrationId);

            try
            {
                // Validate request model
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid process document request");
                    return BadRequest(ModelState);
                }

                // Get the integration details using _integrationService.GetIntegrationByIdAsync(request.IntegrationId)
                var integration = await _integrationService.GetIntegrationByIdAsync(request.IntegrationId);

                // If integration is null, return NotFound
                if (integration == null)
                {
                    _logger.LogWarning("Integration with ID {IntegrationId} not found", request.IntegrationId);
                    return NotFound();
                }

                // Create DocumentProcessingOptions object with request properties
                var options = new DocumentProcessingOptions
                {
                    DocumentType = request.DocumentType,
                    Language = request.Language,
                    ExtractFields = request.ExtractFields,
                    MinimumConfidenceOverride = request.MinimumConfidence,
                    AdditionalOptions = request.AdditionalOptions
                };

                // Call _integrationService.ProcessDocumentAsync(request.IntegrationId, request.DocumentUrl, options) to process the document
                var processingResult = await _integrationService.ProcessDocumentAsync(request.IntegrationId, request.DocumentUrl, options);

                // Create ApiDocumentProcessingResponse using FromProcessingResult with the processing result
                var response = ApiDocumentProcessingResponse.FromProcessingResult(processingResult).ToApiResponse();

                // Return Ok with the response converted to ApiResponse
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while processing document for integration with ID: {IntegrationId}", request.IntegrationId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of available external system types that can be integrated
        /// </summary>
        /// <returns>HTTP response with available system types</returns>
        [HttpGet(ApiRoutes.Integration.SystemTypes)]
        [ProducesResponseType(typeof(ApiSystemTypesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSystemTypesAsync()
        {
            // Log the start of the operation
            _logger.LogInformation("Starting GetSystemTypesAsync operation");

            try
            {
                // Call _integrationService.GetAvailableSystemTypesAsync() to retrieve available system types
                var systemTypes = await _integrationService.GetAvailableSystemTypesAsync();

                // Create ApiSystemTypesResponse using FromSystemTypes with the retrieved system types
                var response = ApiSystemTypesResponse.FromSystemTypes(systemTypes).ToApiResponse();

                // Return Ok with the response converted to ApiResponse
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exceptions and return appropriate error responses
                _logger.LogError(ex, "Error while retrieving system types");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Helper method to extract the current user ID from claims
        /// </summary>
        /// <returns>The current user's ID</returns>
        [Private]
        private string GetCurrentUserId()
        {
            // Get the user claims principal from HttpContext.User
            var claimsPrincipal = HttpContext.User;

            // Find the claim with type ClaimTypes.NameIdentifier
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

            // Return the claim value as the user ID
            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }

            // If claim is not found, throw an exception
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
    }
}