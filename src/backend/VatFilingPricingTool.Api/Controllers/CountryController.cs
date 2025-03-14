using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Controllers
{
    /// <summary>
    /// API controller for managing country data with VAT filing requirements,
    /// providing endpoints for retrieving, creating, updating, and deleting countries.
    /// </summary>
    [ApiController]
    [Route(ApiRoutes.Country.Base)]
    [Authorize]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;

        /// <summary>
        /// Initializes a new instance of the CountryController with required dependencies.
        /// </summary>
        /// <param name="countryService">The country service for country-related operations.</param>
        public CountryController(ICountryService countryService)
        {
            _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
        }

        /// <summary>
        /// Retrieves a specific country by its country code.
        /// </summary>
        /// <param name="id">The country code to retrieve.</param>
        /// <returns>The country details if found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CountryResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<CountryResponse>>> GetCountryAsync(string id)
        {
            var request = new GetCountryRequest { CountryCode = id };
            var result = await _countryService.GetCountryAsync(request);
            
            if (result.IsSuccess)
                return Ok(result.ToApiResponse());
                
            return NotFound(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
        }

        /// <summary>
        /// Retrieves a paginated list of countries with optional filtering.
        /// </summary>
        /// <param name="request">The request containing pagination and filtering parameters.</param>
        /// <returns>A paginated list of countries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<CountriesResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<CountriesResponse>>> GetCountriesAsync([FromQuery] GetCountriesRequest request)
        {
            if (request == null)
                request = new GetCountriesRequest();

            var result = await _countryService.GetCountriesAsync(request);
            
            if (result.IsSuccess)
                return Ok(result.ToApiResponse());
                
            return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
        }

        /// <summary>
        /// Retrieves all active countries.
        /// </summary>
        /// <returns>A list of all active countries.</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<List<CountryResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<List<CountryResponse>>>> GetActiveCountriesAsync()
        {
            var result = await _countryService.GetActiveCountriesAsync();
            
            if (result.IsSuccess)
                return Ok(result.ToApiResponse());
                
            return StatusCode(500, ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
        }

        /// <summary>
        /// Retrieves countries that support a specific filing frequency.
        /// </summary>
        /// <param name="frequency">The filing frequency to filter by.</param>
        /// <returns>A list of countries that support the specified filing frequency.</returns>
        [HttpGet("frequency/{frequency}")]
        [ProducesResponseType(typeof(ApiResponse<List<CountryResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<List<CountryResponse>>>> GetCountriesByFilingFrequencyAsync(FilingFrequency frequency)
        {
            var result = await _countryService.GetCountriesByFilingFrequencyAsync(frequency);
            
            if (result.IsSuccess)
                return Ok(result.ToApiResponse());
                
            return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
        }

        /// <summary>
        /// Retrieves a simplified list of countries for dropdown menus and selection components.
        /// </summary>
        /// <returns>A list of simplified country information.</returns>
        [HttpGet("summaries")]
        [ProducesResponseType(typeof(ApiResponse<List<CountrySummaryResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<List<CountrySummaryResponse>>>> GetCountrySummariesAsync()
        {
            var result = await _countryService.GetCountrySummariesAsync();
            
            if (result.IsSuccess)
                return Ok(result.ToApiResponse());
                
            return StatusCode(500, ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
        }

        /// <summary>
        /// Creates a new country with the specified details.
        /// </summary>
        /// <param name="request">The request containing the country details to create.</param>
        /// <returns>The created country details.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CreateCountryResponse>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<CreateCountryResponse>>> CreateCountryAsync([FromBody] CreateCountryRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null"));

            var result = await _countryService.CreateCountryAsync(request);
            
            if (result.IsSuccess)
                return Created($"{ApiRoutes.Country.Base}/{request.CountryCode}", result.ToApiResponse());

            if (result.ErrorCode == ErrorCodes.Country.DuplicateCountryCode)
                return Conflict(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
                
            return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
        }

        /// <summary>
        /// Updates an existing country with the specified details.
        /// </summary>
        /// <param name="id">The country code to update.</param>
        /// <param name="request">The request containing the updated country details.</param>
        /// <returns>The updated country details.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UpdateCountryResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<UpdateCountryResponse>>> UpdateCountryAsync(string id, [FromBody] UpdateCountryRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null"));

            if (id != request.CountryCode)
                return BadRequest(ApiResponse.CreateError("Country code in route must match request body"));

            var result = await _countryService.UpdateCountryAsync(request);
            
            if (result.IsSuccess)
                return Ok(result.ToApiResponse());

            if (result.ErrorCode == ErrorCodes.Country.CountryNotFound)
                return NotFound(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
                
            return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
        }

        /// <summary>
        /// Deletes a country with the specified country code.
        /// </summary>
        /// <param name="id">The country code to delete.</param>
        /// <returns>The result of the deletion operation.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DeleteCountryResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<DeleteCountryResponse>>> DeleteCountryAsync(string id)
        {
            var request = new DeleteCountryRequest { CountryCode = id };
            var result = await _countryService.DeleteCountryAsync(request);
            
            if (result.IsSuccess)
                return Ok(result.ToApiResponse());

            if (result.ErrorCode == ErrorCodes.Country.CountryNotFound)
                return NotFound(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
                
            if (result.ErrorCode == ErrorCodes.Country.CountryInUse)
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
                
            return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
        }
    }
}