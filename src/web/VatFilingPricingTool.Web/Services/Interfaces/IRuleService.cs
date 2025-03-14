using System.Collections.Generic;
using System.Threading.Tasks;
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing VAT filing pricing rules in the web application.
    /// Provides methods for retrieving, creating, updating, and deleting rules,
    /// as well as validating rule expressions and importing/exporting rules.
    /// </summary>
    public interface IRuleService
    {
        /// <summary>
        /// Retrieves a specific rule by its unique identifier
        /// </summary>
        /// <param name="ruleId">The unique identifier of the rule to retrieve</param>
        /// <returns>The rule if found, otherwise null</returns>
        Task<RuleModel> GetRuleByIdAsync(string ruleId);

        /// <summary>
        /// Retrieves a paginated list of rules with optional filtering
        /// </summary>
        /// <param name="filter">Filter criteria for the rules</param>
        /// <returns>A paginated list of rules matching the filter criteria</returns>
        Task<RuleListModel> GetRulesAsync(RuleFilterModel filter);

        /// <summary>
        /// Retrieves a simplified list of rules for dropdown selection components
        /// </summary>
        /// <param name="countryCode">Optional country code to filter rules by country</param>
        /// <param name="ruleType">Optional rule type to filter rules by type</param>
        /// <param name="activeOnly">When true, returns only active rules</param>
        /// <returns>A list of rule summaries for selection purposes</returns>
        Task<List<RuleSummaryModel>> GetRuleSummariesAsync(string countryCode, RuleType? ruleType, bool activeOnly);

        /// <summary>
        /// Creates a new VAT filing pricing rule
        /// </summary>
        /// <param name="rule">The rule to create</param>
        /// <returns>The created rule with assigned ID</returns>
        Task<RuleModel> CreateRuleAsync(CreateRuleModel rule);

        /// <summary>
        /// Updates an existing VAT filing pricing rule
        /// </summary>
        /// <param name="rule">The rule with updated values</param>
        /// <returns>The updated rule</returns>
        Task<RuleModel> UpdateRuleAsync(UpdateRuleModel rule);

        /// <summary>
        /// Deletes a VAT filing pricing rule
        /// </summary>
        /// <param name="ruleId">The unique identifier of the rule to delete</param>
        /// <returns>True if the rule was successfully deleted, otherwise false</returns>
        Task<bool> DeleteRuleAsync(string ruleId);

        /// <summary>
        /// Validates a rule expression for syntax and evaluates it with sample data
        /// </summary>
        /// <param name="model">The validation model containing the expression, parameters, and sample values</param>
        /// <returns>A validation result indicating if the expression is valid and its evaluation result</returns>
        Task<ValidateRuleExpressionResultModel> ValidateRuleExpressionAsync(ValidateRuleExpressionModel model);

        /// <summary>
        /// Imports multiple rules from an uploaded file (JSON, CSV, Excel)
        /// </summary>
        /// <param name="countryCode">The country code for the imported rules</param>
        /// <param name="fileContent">The binary content of the uploaded file</param>
        /// <param name="fileName">The name of the uploaded file</param>
        /// <returns>The number of successfully imported rules</returns>
        Task<int> ImportRulesAsync(string countryCode, byte[] fileContent, string fileName);

        /// <summary>
        /// Exports rules to a file format (JSON, CSV, Excel) for download
        /// </summary>
        /// <param name="countryCode">Optional country code to filter rules by country</param>
        /// <param name="ruleType">Optional rule type to filter rules by type</param>
        /// <param name="activeOnly">When true, exports only active rules</param>
        /// <param name="format">The export file format (json, csv, excel)</param>
        /// <returns>The binary content of the exported file</returns>
        Task<byte[]> ExportRulesAsync(string countryCode, RuleType? ruleType, bool activeOnly, string format);
    }
}