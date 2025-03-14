using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Responses
{
    /// <summary>
    /// Response model for a VAT filing pricing rule
    /// </summary>
    public class RuleResponse
    {
        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        public string RuleId { get; set; }
        
        /// <summary>
        /// Country code to which this rule applies (ISO 3166-1 alpha-2)
        /// </summary>
        public string CountryCode { get; set; }
        
        /// <summary>
        /// Type of the rule (VatRate, Threshold, Complexity, SpecialRequirement)
        /// </summary>
        public RuleType RuleType { get; set; }
        
        /// <summary>
        /// Descriptive name of the rule
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Detailed description of the rule's purpose and behavior
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Mathematical or logical expression that defines how the rule is applied
        /// </summary>
        public string Expression { get; set; }
        
        /// <summary>
        /// Date from which the rule becomes effective
        /// </summary>
        public DateTime EffectiveFrom { get; set; }
        
        /// <summary>
        /// Optional date when the rule expires (null means no expiration)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }
        
        /// <summary>
        /// Priority of the rule (higher numbers indicate higher priority when multiple rules apply)
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// List of parameters used in the rule's expression
        /// </summary>
        public List<RuleParameterModel> Parameters { get; set; }
        
        /// <summary>
        /// List of conditions that determine when this rule should be applied
        /// </summary>
        public List<RuleConditionModel> Conditions { get; set; }
        
        /// <summary>
        /// Indicates whether the rule is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Date and time when the rule was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Default constructor for the RuleResponse
        /// </summary>
        public RuleResponse()
        {
            Parameters = new List<RuleParameterModel>();
            Conditions = new List<RuleConditionModel>();
            IsActive = true;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Response model for a paginated list of VAT filing pricing rules
    /// </summary>
    public class RulesResponse
    {
        /// <summary>
        /// Collection of rule items in the current page
        /// </summary>
        public List<RuleResponse> Items { get; set; }
        
        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// Indicates whether there is a previous page available
        /// </summary>
        public bool HasPreviousPage { get; set; }
        
        /// <summary>
        /// Indicates whether there is a next page available
        /// </summary>
        public bool HasNextPage { get; set; }
        
        /// <summary>
        /// Default constructor for the RulesResponse
        /// </summary>
        public RulesResponse()
        {
            Items = new List<RuleResponse>();
            PageNumber = 1;
            PageSize = 10;
            TotalCount = 0;
            TotalPages = 0;
            HasPreviousPage = false;
            HasNextPage = false;
        }
    }

    /// <summary>
    /// Response model for simplified rule information used in dropdowns and selection components
    /// </summary>
    public class RuleSummaryResponse
    {
        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        public string RuleId { get; set; }
        
        /// <summary>
        /// Descriptive name of the rule
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Type of the rule
        /// </summary>
        public RuleType RuleType { get; set; }
        
        /// <summary>
        /// Country code to which this rule applies
        /// </summary>
        public string CountryCode { get; set; }
        
        /// <summary>
        /// Indicates whether the rule is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Default constructor for the RuleSummaryResponse
        /// </summary>
        public RuleSummaryResponse()
        {
            IsActive = true;
        }
    }

    /// <summary>
    /// Response model for rule creation operations
    /// </summary>
    public class CreateRuleResponse
    {
        /// <summary>
        /// Unique identifier for the created rule
        /// </summary>
        public string RuleId { get; set; }
        
        /// <summary>
        /// Country code to which this rule applies
        /// </summary>
        public string CountryCode { get; set; }
        
        /// <summary>
        /// Descriptive name of the rule
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Indicates whether the rule creation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message providing additional information about the operation
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// List of errors that occurred during rule creation, if any
        /// </summary>
        public List<string> Errors { get; set; }
        
        /// <summary>
        /// Default constructor for the CreateRuleResponse
        /// </summary>
        public CreateRuleResponse()
        {
            Errors = new List<string>();
            Success = false;
        }
    }

    /// <summary>
    /// Response model for rule update operations
    /// </summary>
    public class UpdateRuleResponse
    {
        /// <summary>
        /// Unique identifier for the updated rule
        /// </summary>
        public string RuleId { get; set; }
        
        /// <summary>
        /// Descriptive name of the rule
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Date and time when the rule was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Indicates whether the rule update was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message providing additional information about the operation
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// List of errors that occurred during rule update, if any
        /// </summary>
        public List<string> Errors { get; set; }
        
        /// <summary>
        /// Default constructor for the UpdateRuleResponse
        /// </summary>
        public UpdateRuleResponse()
        {
            Errors = new List<string>();
            Success = false;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Response model for rule deletion operations
    /// </summary>
    public class DeleteRuleResponse
    {
        /// <summary>
        /// Unique identifier for the deleted rule
        /// </summary>
        public string RuleId { get; set; }
        
        /// <summary>
        /// Indicates whether the rule deletion was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message providing additional information about the operation
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Default constructor for the DeleteRuleResponse
        /// </summary>
        public DeleteRuleResponse()
        {
            Success = false;
        }
    }

    /// <summary>
    /// Response model for rule expression validation operations
    /// </summary>
    public class ValidateRuleExpressionResponse
    {
        /// <summary>
        /// Indicates whether the rule expression is valid
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Message providing additional information about the validation
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Result of the rule expression evaluation, if applicable
        /// </summary>
        public object EvaluationResult { get; set; }
        
        /// <summary>
        /// List of errors that occurred during validation, if any
        /// </summary>
        public List<string> Errors { get; set; }
        
        /// <summary>
        /// Default constructor for the ValidateRuleExpressionResponse
        /// </summary>
        public ValidateRuleExpressionResponse()
        {
            Errors = new List<string>();
            IsValid = false;
        }
    }

    /// <summary>
    /// Response model for rule import operations
    /// </summary>
    public class ImportRulesResponse
    {
        /// <summary>
        /// Total number of rules in the import operation
        /// </summary>
        public int TotalRules { get; set; }
        
        /// <summary>
        /// Number of rules successfully imported
        /// </summary>
        public int ImportedRules { get; set; }
        
        /// <summary>
        /// Number of rules that failed to import
        /// </summary>
        public int FailedRules { get; set; }
        
        /// <summary>
        /// Indicates whether the import operation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message providing additional information about the operation
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// List of errors that occurred during import, if any
        /// </summary>
        public List<string> Errors { get; set; }
        
        /// <summary>
        /// Default constructor for the ImportRulesResponse
        /// </summary>
        public ImportRulesResponse()
        {
            Errors = new List<string>();
            Success = false;
            TotalRules = 0;
            ImportedRules = 0;
            FailedRules = 0;
        }
    }

    /// <summary>
    /// Response model for rule export operations
    /// </summary>
    public class ExportRulesResponse
    {
        /// <summary>
        /// Total number of rules in the export operation
        /// </summary>
        public int TotalRules { get; set; }
        
        /// <summary>
        /// Format of the exported rules (e.g., "json", "xml", "csv")
        /// </summary>
        public string ExportFormat { get; set; }
        
        /// <summary>
        /// URL for downloading the exported rules
        /// </summary>
        public string DownloadUrl { get; set; }
        
        /// <summary>
        /// Indicates whether the export operation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message providing additional information about the operation
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Default constructor for the ExportRulesResponse
        /// </summary>
        public ExportRulesResponse()
        {
            Success = false;
            TotalRules = 0;
            ExportFormat = "json";
        }
    }
}