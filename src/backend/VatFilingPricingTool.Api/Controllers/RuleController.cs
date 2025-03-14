using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Controllers
{
    /// <summary>
    /// API controller for managing VAT filing pricing rules, providing endpoints for retrieving, creating, updating, and deleting rules,
    /// as well as validating rule expressions and importing/exporting rules.
    /// </summary>
    [ApiController]
    [Route(ApiRoutes.Rule.Base)]
    [Authorize]
    public class RuleController : ControllerBase
    {
        private readonly IRuleService _ruleService;

        /// <summary>
        /// Initializes a new instance of the RuleController with required dependencies
        /// </summary>
        /// <param name="ruleService">The rule service that handles all rule operations</param>
        public RuleController(IRuleService ruleService)
        {
            _ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
        }

        /// <summary>
        /// Retrieves a specific rule by its unique identifier
        /// </summary>
        /// <param name="request">The request containing the rule ID</param>
        /// <returns>The rule details if found</returns>
        [HttpGet(ApiRoutes.Rule.GetById)]
        [ProducesResponseType(typeof(ApiResponse<RuleResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<RuleResponse>>> GetRuleAsync([FromQuery] GetRuleRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null", ErrorCodes.General.BadRequest));

            var result = await _ruleService.GetRuleByIdAsync(request.RuleId);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<RuleResponse>.CreateSuccess(result.Value));
            }
            else
            {
                return NotFound(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
        }

        /// <summary>
        /// Retrieves a paginated list of rules for a specific country with optional filtering
        /// </summary>
        /// <param name="request">The request containing filter and pagination parameters</param>
        /// <returns>A paginated list of rules matching the criteria</returns>
        [HttpGet(ApiRoutes.Rule.GetByCountry)]
        [ProducesResponseType(typeof(ApiResponse<RulesResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<RulesResponse>>> GetRulesByCountryAsync([FromQuery] GetRulesByCountryRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null", ErrorCodes.General.BadRequest));

            var result = await _ruleService.GetRulesAsync(request);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<RulesResponse>.CreateSuccess(result.Value));
            }
            else
            {
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
        }

        /// <summary>
        /// Retrieves a simplified list of rules for dropdown menus and selection components
        /// </summary>
        /// <param name="request">The request containing filter parameters</param>
        /// <returns>A list of rule summaries</returns>
        [HttpGet(ApiRoutes.Rule.Summaries)]
        [ProducesResponseType(typeof(ApiResponse<List<RuleSummaryResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<List<RuleSummaryResponse>>>> GetRuleSummariesAsync([FromQuery] GetRuleSummariesRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null", ErrorCodes.General.BadRequest));

            var result = await _ruleService.GetRuleSummariesAsync(request.CountryCode, request.RuleType, request.ActiveOnly);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<List<RuleSummaryResponse>>.CreateSuccess(result.Value));
            }
            else
            {
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
        }

        /// <summary>
        /// Creates a new VAT filing pricing rule
        /// </summary>
        /// <param name="request">The request containing the rule details</param>
        /// <returns>The created rule information</returns>
        [HttpPost(ApiRoutes.Rule.Create)]
        [ProducesResponseType(typeof(ApiResponse<CreateRuleResponse>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<CreateRuleResponse>>> CreateRuleAsync([FromBody] CreateRuleRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null", ErrorCodes.General.BadRequest));

            var result = await _ruleService.CreateRuleAsync(request);

            if (result.IsSuccess)
            {
                return Created($"{ApiRoutes.Rule.Base}/{result.Value.RuleId}", ApiResponse<CreateRuleResponse>.CreateSuccess(result.Value));
            }
            else if (result.ErrorCode == ErrorCodes.Rule.DuplicateRuleId)
            {
                return Conflict(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
            else
            {
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
        }

        /// <summary>
        /// Updates an existing VAT filing pricing rule
        /// </summary>
        /// <param name="request">The request containing the updated rule details</param>
        /// <returns>The updated rule information</returns>
        [HttpPut(ApiRoutes.Rule.Update)]
        [ProducesResponseType(typeof(ApiResponse<UpdateRuleResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<UpdateRuleResponse>>> UpdateRuleAsync([FromBody] UpdateRuleRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null", ErrorCodes.General.BadRequest));

            var result = await _ruleService.UpdateRuleAsync(request);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<UpdateRuleResponse>.CreateSuccess(result.Value));
            }
            else if (result.ErrorCode == ErrorCodes.Rule.RuleNotFound)
            {
                return NotFound(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
            else
            {
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
        }

        /// <summary>
        /// Deletes a VAT filing pricing rule
        /// </summary>
        /// <param name="request">The request containing the rule ID to delete</param>
        /// <returns>The result of the deletion operation</returns>
        [HttpDelete(ApiRoutes.Rule.Delete)]
        [ProducesResponseType(typeof(ApiResponse<DeleteRuleResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<DeleteRuleResponse>>> DeleteRuleAsync([FromQuery] DeleteRuleRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null", ErrorCodes.General.BadRequest));

            var result = await _ruleService.DeleteRuleAsync(request.RuleId);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<DeleteRuleResponse>.CreateSuccess(result.Value));
            }
            else if (result.ErrorCode == ErrorCodes.Rule.RuleNotFound)
            {
                return NotFound(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
            else if (result.ErrorCode == ErrorCodes.Rule.RuleInUse)
            {
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
            else
            {
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
        }

        /// <summary>
        /// Validates a rule expression for syntax and evaluates it with sample data
        /// </summary>
        /// <param name="request">The request containing the expression to validate</param>
        /// <returns>The validation result with evaluation outcome</returns>
        [HttpPost(ApiRoutes.Rule.Validate)]
        [ProducesResponseType(typeof(ApiResponse<ValidateRuleExpressionResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<ValidateRuleExpressionResponse>>> ValidateRuleExpressionAsync([FromBody] ValidateRuleExpressionRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null", ErrorCodes.General.BadRequest));

            var result = await _ruleService.ValidateRuleExpressionAsync(request);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<ValidateRuleExpressionResponse>.CreateSuccess(result.Value));
            }
            else
            {
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
        }

        /// <summary>
        /// Imports multiple rules from an external source
        /// </summary>
        /// <param name="request">The request containing the rules to import</param>
        /// <returns>The result of the import operation</returns>
        [HttpPost(ApiRoutes.Rule.Import)]
        [ProducesResponseType(typeof(ApiResponse<ImportRulesResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse<ImportRulesResponse>>> ImportRulesAsync([FromBody] ImportRulesRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse.CreateError("Request cannot be null", ErrorCodes.General.BadRequest));

            var result = await _ruleService.ImportRulesAsync(request);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse<ImportRulesResponse>.CreateSuccess(result.Value));
            }
            else
            {
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
        }

        /// <summary>
        /// Exports rules to a file format (JSON, CSV, Excel) for external use
        /// </summary>
        /// <param name="countryCode">Country code to filter rules by</param>
        /// <param name="ruleType">Rule type to filter by</param>
        /// <param name="activeOnly">Whether to export only active rules</param>
        /// <param name="format">The file format to export as (json, csv, xlsx)</param>
        /// <returns>A file download containing the exported rules</returns>
        [HttpGet(ApiRoutes.Rule.Export)]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult> ExportRulesAsync(
            [FromQuery] string countryCode, 
            [FromQuery] RuleType? ruleType,
            [FromQuery] bool activeOnly = true,
            [FromQuery] string format = "json")
        {
            if (string.IsNullOrEmpty(countryCode))
                return BadRequest(ApiResponse.CreateError("Country code is required", ErrorCodes.Rule.RuleExportFailed));

            var result = await _ruleService.ExportRulesAsync(countryCode, ruleType, activeOnly, format);

            if (result.IsSuccess)
            {
                string contentType;
                string fileName = $"VAT_Rules_{countryCode}_{DateTime.UtcNow:yyyyMMdd}";

                switch (format.ToLower())
                {
                    case "json":
                        contentType = "application/json";
                        fileName += ".json";
                        break;
                    case "csv":
                        contentType = "text/csv";
                        fileName += ".csv";
                        break;
                    case "xlsx":
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileName += ".xlsx";
                        break;
                    default:
                        contentType = "application/json";
                        fileName += ".json";
                        break;
                }

                return File(result.Value, contentType, fileName);
            }
            else if (result.ErrorCode == ErrorCodes.Rule.RuleNotFound)
            {
                return NotFound(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
            else
            {
                return BadRequest(ApiResponse.CreateError(result.ErrorMessage, result.ErrorCode));
            }
        }
    }
}