using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Models.Requests
{
    /// <summary>
    /// Request model for creating a new VAT filing pricing rule
    /// </summary>
    public class CreateRuleRequest
    {
        /// <summary>
        /// The country code to which this rule applies (e.g., "GB" for United Kingdom)
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// The type of rule (e.g., VatRate, Threshold, Complexity, SpecialRequirement, Discount)
        /// </summary>
        [Required(ErrorMessage = "Rule type is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RuleType RuleType { get; set; }

        /// <summary>
        /// A descriptive name for the rule
        /// </summary>
        [Required(ErrorMessage = "Rule name is required")]
        [StringLength(100, ErrorMessage = "Rule name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// A detailed description of the rule's purpose and behavior
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// The expression used to calculate the rule's effect (e.g., "basePrice * 0.20")
        /// </summary>
        [Required(ErrorMessage = "Rule expression is required")]
        public string Expression { get; set; }

        /// <summary>
        /// The date from which this rule becomes effective
        /// </summary>
        [Required(ErrorMessage = "Effective from date is required")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// The date until which this rule is effective (null means indefinitely)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// The priority of the rule (lower numbers = higher priority)
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Priority must be between 1 and 1000")]
        public int Priority { get; set; }

        /// <summary>
        /// The parameters used in the rule expression
        /// </summary>
        public List<RuleParameterRequest> Parameters { get; set; }

        /// <summary>
        /// Conditions that determine when this rule should be applied
        /// </summary>
        public List<RuleConditionRequest> Conditions { get; set; }

        /// <summary>
        /// Indicates whether the rule is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Default constructor for the CreateRuleRequest
        /// </summary>
        public CreateRuleRequest()
        {
            Parameters = new List<RuleParameterRequest>();
            Conditions = new List<RuleConditionRequest>();
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
        /// The unique identifier for the rule
        /// </summary>
        [Required(ErrorMessage = "Rule ID is required")]
        public string RuleId { get; set; }

        /// <summary>
        /// A descriptive name for the rule
        /// </summary>
        [Required(ErrorMessage = "Rule name is required")]
        [StringLength(100, ErrorMessage = "Rule name cannot exceed 100 characters")]
        public string Name { get; set; }

        /// <summary>
        /// A detailed description of the rule's purpose and behavior
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// The expression used to calculate the rule's effect (e.g., "basePrice * 0.20")
        /// </summary>
        [Required(ErrorMessage = "Rule expression is required")]
        public string Expression { get; set; }

        /// <summary>
        /// The date from which this rule becomes effective
        /// </summary>
        [Required(ErrorMessage = "Effective from date is required")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// The date until which this rule is effective (null means indefinitely)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// The priority of the rule (lower numbers = higher priority)
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Priority must be between 1 and 1000")]
        public int Priority { get; set; }

        /// <summary>
        /// The parameters used in the rule expression
        /// </summary>
        public List<RuleParameterRequest> Parameters { get; set; }

        /// <summary>
        /// Conditions that determine when this rule should be applied
        /// </summary>
        public List<RuleConditionRequest> Conditions { get; set; }

        /// <summary>
        /// Indicates whether the rule is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Default constructor for the UpdateRuleRequest
        /// </summary>
        public UpdateRuleRequest()
        {
            Parameters = new List<RuleParameterRequest>();
            Conditions = new List<RuleConditionRequest>();
        }
    }

    /// <summary>
    /// Request model for deleting a VAT filing pricing rule
    /// </summary>
    public class DeleteRuleRequest
    {
        /// <summary>
        /// The unique identifier for the rule to be deleted
        /// </summary>
        [Required(ErrorMessage = "Rule ID is required")]
        public string RuleId { get; set; }
    }

    /// <summary>
    /// Request model for retrieving a specific rule by ID
    /// </summary>
    public class GetRuleRequest
    {
        /// <summary>
        /// The unique identifier for the rule to retrieve
        /// </summary>
        [Required(ErrorMessage = "Rule ID is required")]
        public string RuleId { get; set; }
    }

    /// <summary>
    /// Request model for retrieving rules for a specific country
    /// </summary>
    public class GetRulesByCountryRequest
    {
        /// <summary>
        /// The country code for which to retrieve rules (e.g., "GB" for United Kingdom)
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// If true, only active rules will be returned
        /// </summary>
        public bool ActiveOnly { get; set; }

        /// <summary>
        /// Optional filter for rule type
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RuleType? RuleType { get; set; }

        /// <summary>
        /// The page number for pagination (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of items per page
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
    /// Request model for validating a rule expression
    /// </summary>
    public class ValidateRuleExpressionRequest
    {
        /// <summary>
        /// The expression to validate (e.g., "basePrice * 0.20")
        /// </summary>
        [Required(ErrorMessage = "Expression is required")]
        public string Expression { get; set; }

        /// <summary>
        /// The parameters used in the rule expression
        /// </summary>
        public List<RuleParameterRequest> Parameters { get; set; }

        /// <summary>
        /// Sample values to use when evaluating the expression
        /// </summary>
        public Dictionary<string, object> SampleValues { get; set; }

        /// <summary>
        /// Default constructor for the ValidateRuleExpressionRequest
        /// </summary>
        public ValidateRuleExpressionRequest()
        {
            Parameters = new List<RuleParameterRequest>();
            SampleValues = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Request model for rule parameters used in rule expressions
    /// </summary>
    public class RuleParameterRequest
    {
        /// <summary>
        /// The unique identifier for the parameter (may be null for new parameters)
        /// </summary>
        public string ParameterId { get; set; }

        /// <summary>
        /// The name of the parameter as used in expressions
        /// </summary>
        [Required(ErrorMessage = "Parameter name is required")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_]*$", ErrorMessage = "Parameter name must start with a letter and contain only letters, numbers, and underscores")]
        public string Name { get; set; }

        /// <summary>
        /// The data type of the parameter (e.g., "decimal", "int", "bool")
        /// </summary>
        [Required(ErrorMessage = "Data type is required")]
        public string DataType { get; set; }

        /// <summary>
        /// The default value for the parameter
        /// </summary>
        public string DefaultValue { get; set; }
    }

    /// <summary>
    /// Request model for conditions that determine when a rule should be applied
    /// </summary>
    public class RuleConditionRequest
    {
        /// <summary>
        /// The parameter to check in the condition
        /// </summary>
        [Required(ErrorMessage = "Parameter is required")]
        public string Parameter { get; set; }

        /// <summary>
        /// The operator to use in the condition (e.g., "equals", "greaterThan", "lessThan")
        /// </summary>
        [Required(ErrorMessage = "Operator is required")]
        public string Operator { get; set; }

        /// <summary>
        /// The value to compare against
        /// </summary>
        [Required(ErrorMessage = "Value is required")]
        public string Value { get; set; }
    }
}