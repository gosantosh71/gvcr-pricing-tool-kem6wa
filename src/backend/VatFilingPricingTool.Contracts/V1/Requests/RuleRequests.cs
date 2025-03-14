using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Requests
{
    /// <summary>
    /// Request model for retrieving a specific rule by ID
    /// </summary>
    public class GetRuleRequest
    {
        /// <summary>
        /// Unique identifier of the rule to retrieve
        /// </summary>
        [Required(ErrorMessage = "Rule ID is required")]
        [StringLength(50, ErrorMessage = "Rule ID cannot exceed 50 characters")]
        public string RuleId { get; set; }
    }

    /// <summary>
    /// Request model for retrieving rules for a specific country
    /// </summary>
    public class GetRulesByCountryRequest
    {
        /// <summary>
        /// Country code (ISO 3166-1 alpha-2) to retrieve rules for
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Indicates whether to return only active rules (true) or all rules (false)
        /// </summary>
        public bool ActiveOnly { get; set; }

        /// <summary>
        /// Optional filter for rule type
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RuleType? RuleType { get; set; }

        /// <summary>
        /// Page number for pagination (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page for pagination
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; }

        /// <summary>
        /// Default constructor for the GetRulesByCountryRequest
        /// </summary>
        public GetRulesByCountryRequest()
        {
            ActiveOnly = true;
            PageNumber = 1;
            PageSize = 10;
        }
    }

    /// <summary>
    /// Request model for creating a new VAT filing pricing rule
    /// </summary>
    public class CreateRuleRequest
    {
        /// <summary>
        /// Country code (ISO 3166-1 alpha-2) to which this rule applies
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Type of the rule (VatRate, Threshold, Complexity, SpecialRequirement)
        /// </summary>
        [Required(ErrorMessage = "Rule type is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RuleType RuleType { get; set; }

        /// <summary>
        /// Descriptive name of the rule
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of the rule's purpose and behavior
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Mathematical or logical expression that defines how the rule is applied
        /// </summary>
        [Required(ErrorMessage = "Expression is required")]
        [StringLength(1000, ErrorMessage = "Expression cannot exceed 1000 characters")]
        public string Expression { get; set; }

        /// <summary>
        /// Date from which the rule becomes effective
        /// </summary>
        [Required(ErrorMessage = "Effective from date is required")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Optional date when the rule expires (null means no expiration)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Priority of the rule (higher numbers indicate higher priority when multiple rules apply)
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Priority must be between 1 and 1000")]
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
        /// Default constructor for the CreateRuleRequest
        /// </summary>
        public CreateRuleRequest()
        {
            Parameters = new List<RuleParameterModel>();
            Conditions = new List<RuleConditionModel>();
            EffectiveFrom = DateTime.UtcNow;
            IsActive = true;
            Priority = 100;
        }
    }

    /// <summary>
    /// Request model for updating an existing VAT filing pricing rule
    /// </summary>
    public class UpdateRuleRequest
    {
        /// <summary>
        /// Unique identifier of the rule to update
        /// </summary>
        [Required(ErrorMessage = "Rule ID is required")]
        [StringLength(50, ErrorMessage = "Rule ID cannot exceed 50 characters")]
        public string RuleId { get; set; }

        /// <summary>
        /// Updated descriptive name of the rule
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Updated description of the rule's purpose and behavior
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Updated mathematical or logical expression that defines how the rule is applied
        /// </summary>
        [Required(ErrorMessage = "Expression is required")]
        [StringLength(1000, ErrorMessage = "Expression cannot exceed 1000 characters")]
        public string Expression { get; set; }

        /// <summary>
        /// Updated date from which the rule becomes effective
        /// </summary>
        [Required(ErrorMessage = "Effective from date is required")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Updated optional date when the rule expires (null means no expiration)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Updated priority of the rule
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Priority must be between 1 and 1000")]
        public int Priority { get; set; }

        /// <summary>
        /// Updated list of parameters used in the rule's expression
        /// </summary>
        public List<RuleParameterModel> Parameters { get; set; }

        /// <summary>
        /// Updated list of conditions that determine when this rule should be applied
        /// </summary>
        public List<RuleConditionModel> Conditions { get; set; }

        /// <summary>
        /// Updated active status of the rule
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Default constructor for the UpdateRuleRequest
        /// </summary>
        public UpdateRuleRequest()
        {
            Parameters = new List<RuleParameterModel>();
            Conditions = new List<RuleConditionModel>();
        }
    }

    /// <summary>
    /// Request model for deleting a VAT filing pricing rule
    /// </summary>
    public class DeleteRuleRequest
    {
        /// <summary>
        /// Unique identifier of the rule to delete
        /// </summary>
        [Required(ErrorMessage = "Rule ID is required")]
        [StringLength(50, ErrorMessage = "Rule ID cannot exceed 50 characters")]
        public string RuleId { get; set; }
    }

    /// <summary>
    /// Request model for validating a rule expression
    /// </summary>
    public class ValidateRuleExpressionRequest
    {
        /// <summary>
        /// The expression to validate
        /// </summary>
        [Required(ErrorMessage = "Expression is required")]
        [StringLength(1000, ErrorMessage = "Expression cannot exceed 1000 characters")]
        public string Expression { get; set; }

        /// <summary>
        /// List of parameters referenced in the expression
        /// </summary>
        public List<RuleParameterModel> Parameters { get; set; }

        /// <summary>
        /// Sample values to use for testing the expression
        /// </summary>
        public Dictionary<string, object> SampleValues { get; set; }

        /// <summary>
        /// Default constructor for the ValidateRuleExpressionRequest
        /// </summary>
        public ValidateRuleExpressionRequest()
        {
            Parameters = new List<RuleParameterModel>();
            SampleValues = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Request model for importing multiple VAT filing pricing rules
    /// </summary>
    public class ImportRulesRequest
    {
        /// <summary>
        /// List of rules to import
        /// </summary>
        [Required(ErrorMessage = "Rules list is required")]
        [MinLength(1, ErrorMessage = "At least one rule must be provided")]
        public List<RuleModel> Rules { get; set; }

        /// <summary>
        /// Indicates whether to overwrite existing rules with the same ID
        /// </summary>
        public bool OverwriteExisting { get; set; }

        /// <summary>
        /// Default constructor for the ImportRulesRequest
        /// </summary>
        public ImportRulesRequest()
        {
            Rules = new List<RuleModel>();
            OverwriteExisting = false;
        }
    }

    /// <summary>
    /// Request model for exporting VAT filing pricing rules
    /// </summary>
    public class ExportRulesRequest
    {
        /// <summary>
        /// Country code to export rules for (null for all countries)
        /// </summary>
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Optional filter for rule type
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RuleType? RuleType { get; set; }

        /// <summary>
        /// Indicates whether to export only active rules (true) or all rules (false)
        /// </summary>
        public bool ActiveOnly { get; set; }

        /// <summary>
        /// Format for the exported rules (e.g., "json", "csv", "xml")
        /// </summary>
        [Required(ErrorMessage = "Export format is required")]
        [RegularExpression("^(json|csv|xml)$", ErrorMessage = "Export format must be json, csv, or xml")]
        public string ExportFormat { get; set; }

        /// <summary>
        /// Default constructor for the ExportRulesRequest
        /// </summary>
        public ExportRulesRequest()
        {
            ActiveOnly = true;
            ExportFormat = "json";
        }
    }

    /// <summary>
    /// Request model for retrieving summarized rule information for dropdowns and selection components
    /// </summary>
    public class GetRuleSummariesRequest
    {
        /// <summary>
        /// Country code to retrieve rule summaries for (null for all countries)
        /// </summary>
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Optional filter for rule type
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RuleType? RuleType { get; set; }

        /// <summary>
        /// Indicates whether to return only active rules (true) or all rules (false)
        /// </summary>
        public bool ActiveOnly { get; set; }

        /// <summary>
        /// Default constructor for the GetRuleSummariesRequest
        /// </summary>
        public GetRuleSummariesRequest()
        {
            ActiveOnly = true;
        }
    }
}