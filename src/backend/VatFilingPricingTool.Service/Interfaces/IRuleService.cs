using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Contracts.V1.Requests;

namespace VatFilingPricingTool.Service.Interfaces
{
    /// <summary>
    /// Service interface for managing VAT filing pricing rules
    /// </summary>
    public interface IRuleService
    {
        /// <summary>
        /// Retrieves a specific rule by its unique identifier
        /// </summary>
        /// <param name="ruleId">The unique identifier of the rule to retrieve</param>
        /// <returns>A result containing the rule if found</returns>
        Task<Result<RuleResponse>> GetRuleByIdAsync(string ruleId);

        /// <summary>
        /// Retrieves a paginated list of rules with optional filtering
        /// </summary>
        /// <param name="request">The request containing filter and pagination parameters</param>
        /// <returns>A result containing the paginated list of rules</returns>
        Task<Result<RulesResponse>> GetRulesAsync(GetRulesRequest request);

        /// <summary>
        /// Retrieves a simplified list of rules for dropdown selection components
        /// </summary>
        /// <param name="countryCode">Optional country code to filter rules by</param>
        /// <param name="ruleType">Optional rule type to filter by</param>
        /// <param name="activeOnly">Whether to return only active rules</param>
        /// <returns>A result containing a list of rule summaries</returns>
        Task<Result<List<RuleSummaryResponse>>> GetRuleSummariesAsync(string countryCode, RuleType? ruleType, bool activeOnly);

        /// <summary>
        /// Creates a new VAT filing pricing rule
        /// </summary>
        /// <param name="request">The request containing the rule details</param>
        /// <returns>A result containing the created rule's information</returns>
        Task<Result<CreateRuleResponse>> CreateRuleAsync(CreateRuleRequest request);

        /// <summary>
        /// Updates an existing VAT filing pricing rule
        /// </summary>
        /// <param name="request">The request containing the updated rule details</param>
        /// <returns>A result containing the updated rule's information</returns>
        Task<Result<UpdateRuleResponse>> UpdateRuleAsync(UpdateRuleRequest request);

        /// <summary>
        /// Deletes a VAT filing pricing rule
        /// </summary>
        /// <param name="ruleId">The unique identifier of the rule to delete</param>
        /// <returns>A result indicating the success of the deletion</returns>
        Task<Result<DeleteRuleResponse>> DeleteRuleAsync(string ruleId);

        /// <summary>
        /// Validates a rule expression for syntax and evaluates it with sample data
        /// </summary>
        /// <param name="request">The request containing the expression to validate</param>
        /// <returns>A result containing the validation outcome</returns>
        Task<Result<ValidateRuleExpressionResponse>> ValidateRuleExpressionAsync(ValidateRuleExpressionRequest request);

        /// <summary>
        /// Imports multiple rules from an external source
        /// </summary>
        /// <param name="request">The request containing the rules to import</param>
        /// <returns>A result containing the import operation outcome</returns>
        Task<Result<ImportRulesResponse>> ImportRulesAsync(ImportRulesRequest request);

        /// <summary>
        /// Exports rules to a file format (JSON, CSV, Excel) for external use
        /// </summary>
        /// <param name="countryCode">Optional country code to filter rules by</param>
        /// <param name="ruleType">Optional rule type to filter by</param>
        /// <param name="activeOnly">Whether to export only active rules</param>
        /// <param name="format">The file format to export as (json, csv, xlsx)</param>
        /// <returns>A result containing the exported file as a byte array</returns>
        Task<Result<byte[]>> ExportRulesAsync(string countryCode, RuleType? ruleType, bool activeOnly, string format);
    }
}