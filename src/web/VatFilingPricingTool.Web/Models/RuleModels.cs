using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VatFilingPricingTool.Web.Models
{
    /// <summary>
    /// Enumeration of VAT rule types that categorize different kinds of rules used in the pricing calculations
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RuleType
    {
        [Display(Name = "VAT Rate")]
        VatRate,
        
        [Display(Name = "Threshold")]
        Threshold,
        
        [Display(Name = "Complexity")]
        Complexity,
        
        [Display(Name = "Special Requirement")]
        SpecialRequirement,
        
        [Display(Name = "Discount")]
        Discount
    }

    /// <summary>
    /// Represents a VAT filing pricing rule with all its properties for display and editing in the UI
    /// </summary>
    public class RuleModel
    {
        [Required(ErrorMessage = "Rule ID is required")]
        [JsonPropertyName("ruleId")]
        [Display(Name = "Rule ID")]
        public string RuleId { get; set; }

        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        [JsonPropertyName("countryCode")]
        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }

        [Required(ErrorMessage = "Rule type is required")]
        [JsonPropertyName("ruleType")]
        [Display(Name = "Rule Type")]
        public RuleType RuleType { get; set; }

        [Required(ErrorMessage = "Rule name is required")]
        [StringLength(100, ErrorMessage = "Rule name cannot exceed 100 characters")]
        [JsonPropertyName("name")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [JsonPropertyName("description")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Expression is required")]
        [StringLength(1000, ErrorMessage = "Expression cannot exceed 1000 characters")]
        [JsonPropertyName("expression")]
        [Display(Name = "Expression")]
        public string Expression { get; set; }

        [Required(ErrorMessage = "Effective from date is required")]
        [JsonPropertyName("effectiveFrom")]
        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; }

        [JsonPropertyName("effectiveTo")]
        [Display(Name = "Effective To")]
        public DateTime? EffectiveTo { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Range(1, 1000, ErrorMessage = "Priority must be between 1 and 1000")]
        [JsonPropertyName("priority")]
        [Display(Name = "Priority")]
        public int Priority { get; set; }

        [JsonPropertyName("parameters")]
        [Display(Name = "Parameters")]
        public List<RuleParameterModel> Parameters { get; set; }

        [JsonPropertyName("conditions")]
        [Display(Name = "Conditions")]
        public List<RuleConditionModel> Conditions { get; set; }

        [JsonPropertyName("isActive")]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("lastUpdated")]
        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Default constructor for the RuleModel
        /// </summary>
        public RuleModel()
        {
            Parameters = new List<RuleParameterModel>();
            Conditions = new List<RuleConditionModel>();
            IsActive = true;
            EffectiveFrom = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Represents a parameter used in a rule's expression for dynamic value substitution during rule evaluation
    /// </summary>
    public class RuleParameterModel
    {
        [JsonPropertyName("parameterId")]
        [Display(Name = "Parameter ID")]
        public string ParameterId { get; set; }

        [Required(ErrorMessage = "Parameter name is required")]
        [StringLength(50, ErrorMessage = "Parameter name cannot exceed 50 characters")]
        [JsonPropertyName("name")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Data type is required")]
        [StringLength(50, ErrorMessage = "Data type cannot exceed 50 characters")]
        [JsonPropertyName("dataType")]
        [Display(Name = "Data Type")]
        public string DataType { get; set; }

        [JsonPropertyName("defaultValue")]
        [Display(Name = "Default Value")]
        public string DefaultValue { get; set; }
    }

    /// <summary>
    /// Represents a condition that determines when a rule should be applied during calculation
    /// </summary>
    public class RuleConditionModel
    {
        [Required(ErrorMessage = "Parameter is required")]
        [StringLength(50, ErrorMessage = "Parameter cannot exceed 50 characters")]
        [JsonPropertyName("parameter")]
        [Display(Name = "Parameter")]
        public string Parameter { get; set; }

        [Required(ErrorMessage = "Operator is required")]
        [StringLength(20, ErrorMessage = "Operator cannot exceed 20 characters")]
        [JsonPropertyName("operator")]
        [Display(Name = "Operator")]
        public string Operator { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [JsonPropertyName("value")]
        [Display(Name = "Value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Model for creating a new VAT filing pricing rule
    /// </summary>
    public class CreateRuleModel
    {
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        [JsonPropertyName("countryCode")]
        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }

        [Required(ErrorMessage = "Rule type is required")]
        [JsonPropertyName("ruleType")]
        [Display(Name = "Rule Type")]
        public RuleType RuleType { get; set; }

        [Required(ErrorMessage = "Rule name is required")]
        [StringLength(100, ErrorMessage = "Rule name cannot exceed 100 characters")]
        [JsonPropertyName("name")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [JsonPropertyName("description")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Expression is required")]
        [StringLength(1000, ErrorMessage = "Expression cannot exceed 1000 characters")]
        [JsonPropertyName("expression")]
        [Display(Name = "Expression")]
        public string Expression { get; set; }

        [Required(ErrorMessage = "Effective from date is required")]
        [JsonPropertyName("effectiveFrom")]
        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; }

        [JsonPropertyName("effectiveTo")]
        [Display(Name = "Effective To")]
        public DateTime? EffectiveTo { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Range(1, 1000, ErrorMessage = "Priority must be between 1 and 1000")]
        [JsonPropertyName("priority")]
        [Display(Name = "Priority")]
        public int Priority { get; set; }

        [JsonPropertyName("parameters")]
        [Display(Name = "Parameters")]
        public List<RuleParameterModel> Parameters { get; set; }

        [JsonPropertyName("conditions")]
        [Display(Name = "Conditions")]
        public List<RuleConditionModel> Conditions { get; set; }

        [JsonPropertyName("isActive")]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Default constructor for the CreateRuleModel
        /// </summary>
        public CreateRuleModel()
        {
            Parameters = new List<RuleParameterModel>();
            Conditions = new List<RuleConditionModel>();
            EffectiveFrom = DateTime.UtcNow;
            IsActive = true;
            Priority = 100;
        }
    }

    /// <summary>
    /// Model for updating an existing VAT filing pricing rule
    /// </summary>
    public class UpdateRuleModel
    {
        [Required(ErrorMessage = "Rule ID is required")]
        [JsonPropertyName("ruleId")]
        [Display(Name = "Rule ID")]
        public string RuleId { get; set; }

        [Required(ErrorMessage = "Rule name is required")]
        [StringLength(100, ErrorMessage = "Rule name cannot exceed 100 characters")]
        [JsonPropertyName("name")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [JsonPropertyName("description")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Expression is required")]
        [StringLength(1000, ErrorMessage = "Expression cannot exceed 1000 characters")]
        [JsonPropertyName("expression")]
        [Display(Name = "Expression")]
        public string Expression { get; set; }

        [Required(ErrorMessage = "Effective from date is required")]
        [JsonPropertyName("effectiveFrom")]
        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; }

        [JsonPropertyName("effectiveTo")]
        [Display(Name = "Effective To")]
        public DateTime? EffectiveTo { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Range(1, 1000, ErrorMessage = "Priority must be between 1 and 1000")]
        [JsonPropertyName("priority")]
        [Display(Name = "Priority")]
        public int Priority { get; set; }

        [JsonPropertyName("parameters")]
        [Display(Name = "Parameters")]
        public List<RuleParameterModel> Parameters { get; set; }

        [JsonPropertyName("conditions")]
        [Display(Name = "Conditions")]
        public List<RuleConditionModel> Conditions { get; set; }

        [JsonPropertyName("isActive")]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Default constructor for the UpdateRuleModel
        /// </summary>
        public UpdateRuleModel()
        {
            Parameters = new List<RuleParameterModel>();
            Conditions = new List<RuleConditionModel>();
        }
    }

    /// <summary>
    /// Model for filtering rules in list views
    /// </summary>
    public class RuleFilterModel
    {
        [StringLength(2, MinimumLength = 0, ErrorMessage = "Country code must be 2 characters if provided")]
        [JsonPropertyName("countryCode")]
        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("ruleType")]
        [Display(Name = "Rule Type")]
        public RuleType? RuleType { get; set; }

        [JsonPropertyName("activeOnly")]
        [Display(Name = "Active Only")]
        public bool ActiveOnly { get; set; }

        [JsonPropertyName("effectiveDate")]
        [Display(Name = "Effective Date")]
        public DateTime? EffectiveDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        [JsonPropertyName("pageNumber")]
        [Display(Name = "Page Number")]
        public int PageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        [JsonPropertyName("pageSize")]
        [Display(Name = "Page Size")]
        public int PageSize { get; set; }

        /// <summary>
        /// Default constructor for the RuleFilterModel
        /// </summary>
        public RuleFilterModel()
        {
            ActiveOnly = true;
            PageNumber = 1;
            PageSize = 10;
        }
    }

    /// <summary>
    /// Model for a paginated list of rules
    /// </summary>
    public class RuleListModel
    {
        [JsonPropertyName("items")]
        [Display(Name = "Rules")]
        public List<RuleModel> Items { get; set; }

        [JsonPropertyName("pageNumber")]
        [Display(Name = "Page Number")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        [Display(Name = "Page Size")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalCount")]
        [Display(Name = "Total Count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("totalPages")]
        [Display(Name = "Total Pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("hasPreviousPage")]
        [Display(Name = "Has Previous Page")]
        public bool HasPreviousPage { get; set; }

        [JsonPropertyName("hasNextPage")]
        [Display(Name = "Has Next Page")]
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Default constructor for the RuleListModel
        /// </summary>
        public RuleListModel()
        {
            Items = new List<RuleModel>();
            PageNumber = 1;
            PageSize = 10;
            TotalCount = 0;
            TotalPages = 0;
            HasPreviousPage = false;
            HasNextPage = false;
        }
    }

    /// <summary>
    /// Simplified rule model for dropdowns and selection components
    /// </summary>
    public class RuleSummaryModel
    {
        [Required(ErrorMessage = "Rule ID is required")]
        [JsonPropertyName("ruleId")]
        [Display(Name = "Rule ID")]
        public string RuleId { get; set; }

        [Required(ErrorMessage = "Rule name is required")]
        [JsonPropertyName("name")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [JsonPropertyName("ruleType")]
        [Display(Name = "Rule Type")]
        public RuleType RuleType { get; set; }

        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        [JsonPropertyName("countryCode")]
        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("isActive")]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Default constructor for the RuleSummaryModel
        /// </summary>
        public RuleSummaryModel()
        {
            IsActive = true;
        }
    }

    /// <summary>
    /// Model for validating a rule expression
    /// </summary>
    public class ValidateRuleExpressionModel
    {
        [Required(ErrorMessage = "Expression is required")]
        [StringLength(1000, ErrorMessage = "Expression cannot exceed 1000 characters")]
        [JsonPropertyName("expression")]
        [Display(Name = "Expression")]
        public string Expression { get; set; }

        [JsonPropertyName("parameters")]
        [Display(Name = "Parameters")]
        public List<RuleParameterModel> Parameters { get; set; }

        [JsonPropertyName("sampleValues")]
        [Display(Name = "Sample Values")]
        public Dictionary<string, object> SampleValues { get; set; }

        /// <summary>
        /// Default constructor for the ValidateRuleExpressionModel
        /// </summary>
        public ValidateRuleExpressionModel()
        {
            Parameters = new List<RuleParameterModel>();
            SampleValues = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Model for the result of a rule expression validation
    /// </summary>
    public class ValidateRuleExpressionResultModel
    {
        [JsonPropertyName("isValid")]
        [Display(Name = "Is Valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("message")]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [JsonPropertyName("evaluationResult")]
        [Display(Name = "Evaluation Result")]
        public object EvaluationResult { get; set; }

        [JsonPropertyName("errors")]
        [Display(Name = "Errors")]
        public List<string> Errors { get; set; }

        /// <summary>
        /// Default constructor for the ValidateRuleExpressionResultModel
        /// </summary>
        public ValidateRuleExpressionResultModel()
        {
            Errors = new List<string>();
            IsValid = false;
        }
    }

    /// <summary>
    /// Model for the result of a rule import operation
    /// </summary>
    public class ImportRulesResultModel
    {
        [JsonPropertyName("totalRules")]
        [Display(Name = "Total Rules")]
        public int TotalRules { get; set; }

        [JsonPropertyName("importedRules")]
        [Display(Name = "Imported Rules")]
        public int ImportedRules { get; set; }

        [JsonPropertyName("failedRules")]
        [Display(Name = "Failed Rules")]
        public int FailedRules { get; set; }

        [JsonPropertyName("success")]
        [Display(Name = "Success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [JsonPropertyName("errors")]
        [Display(Name = "Errors")]
        public List<string> Errors { get; set; }

        /// <summary>
        /// Default constructor for the ImportRulesResultModel
        /// </summary>
        public ImportRulesResultModel()
        {
            Errors = new List<string>();
            Success = false;
            TotalRules = 0;
            ImportedRules = 0;
            FailedRules = 0;
        }
    }

    /// <summary>
    /// Model for exporting rules to a file
    /// </summary>
    public class ExportRulesModel
    {
        [StringLength(2, MinimumLength = 0, ErrorMessage = "Country code must be 2 characters if provided")]
        [JsonPropertyName("countryCode")]
        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("ruleType")]
        [Display(Name = "Rule Type")]
        public RuleType? RuleType { get; set; }

        [JsonPropertyName("activeOnly")]
        [Display(Name = "Active Only")]
        public bool ActiveOnly { get; set; }

        [Required(ErrorMessage = "Export format is required")]
        [JsonPropertyName("exportFormat")]
        [Display(Name = "Export Format")]
        public string ExportFormat { get; set; }

        /// <summary>
        /// Default constructor for the ExportRulesModel
        /// </summary>
        public ExportRulesModel()
        {
            ActiveOnly = true;
            ExportFormat = "json";
        }
    }
}