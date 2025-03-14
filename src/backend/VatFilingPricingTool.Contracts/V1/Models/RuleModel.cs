using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Models
{
    /// <summary>
    /// Data transfer object for VAT filing pricing rules. Represents a rule that can be applied
    /// during pricing calculations based on country-specific VAT regulations.
    /// </summary>
    public class RuleModel
    {
        /// <summary>
        /// Unique identifier for the rule
        /// </summary>
        [Required(ErrorMessage = "Rule ID is required")]
        [StringLength(50, ErrorMessage = "Rule ID cannot exceed 50 characters")]
        public string RuleId { get; set; }

        /// <summary>
        /// Country code to which this rule applies (ISO 3166-1 alpha-2)
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
        /// Date and time when the rule was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Default constructor for the RuleModel
        /// </summary>
        public RuleModel()
        {
            Parameters = new List<RuleParameterModel>();
            Conditions = new List<RuleConditionModel>();
            EffectiveFrom = DateTime.UtcNow;
            IsActive = true;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Data transfer object for parameters used in rule expressions.
    /// These parameters provide values that can be substituted into the rule's expression.
    /// </summary>
    public class RuleParameterModel
    {
        /// <summary>
        /// Unique identifier for the parameter
        /// </summary>
        [StringLength(50, ErrorMessage = "Parameter ID cannot exceed 50 characters")]
        public string ParameterId { get; set; }

        /// <summary>
        /// Name of the parameter as referenced in the expression
        /// </summary>
        [Required(ErrorMessage = "Parameter name is required")]
        [StringLength(50, ErrorMessage = "Parameter name cannot exceed 50 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Data type of the parameter (e.g., "decimal", "int", "string", "date")
        /// </summary>
        [Required(ErrorMessage = "Data type is required")]
        [StringLength(20, ErrorMessage = "Data type cannot exceed 20 characters")]
        public string DataType { get; set; }

        /// <summary>
        /// Default value for the parameter if no value is provided during calculation
        /// </summary>
        [StringLength(100, ErrorMessage = "Default value cannot exceed 100 characters")]
        public string DefaultValue { get; set; }
    }

    /// <summary>
    /// Data transfer object for conditions that determine when a rule should be applied.
    /// These conditions act as filters to determine if a rule is applicable to a specific scenario.
    /// </summary>
    public class RuleConditionModel
    {
        /// <summary>
        /// The parameter to check in the condition (e.g., "serviceType", "transactionVolume")
        /// </summary>
        [Required(ErrorMessage = "Parameter is required")]
        [StringLength(50, ErrorMessage = "Parameter cannot exceed 50 characters")]
        public string Parameter { get; set; }

        /// <summary>
        /// The comparison operator (e.g., "equals", "greaterThan", "lessThan", "contains")
        /// </summary>
        [Required(ErrorMessage = "Operator is required")]
        [StringLength(20, ErrorMessage = "Operator cannot exceed 20 characters")]
        public string Operator { get; set; }

        /// <summary>
        /// The value to compare against
        /// </summary>
        [Required(ErrorMessage = "Value is required")]
        [StringLength(100, ErrorMessage = "Value cannot exceed 100 characters")]
        public string Value { get; set; }
    }
}