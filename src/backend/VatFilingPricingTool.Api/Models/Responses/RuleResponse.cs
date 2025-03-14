using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Models.Responses
{
    /// <summary>
    /// Response model for a VAT filing pricing rule returned from the API.
    /// This model provides a standardized structure for rule data in API responses,
    /// serving as a bridge between domain entities and client-facing DTOs.
    /// </summary>
    public class RuleResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the rule.
        /// </summary>
        [JsonPropertyName("ruleId")]
        public string RuleId { get; set; }

        /// <summary>
        /// Gets or sets the country code to which this rule applies.
        /// </summary>
        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the type of rule (VatRate, Threshold, Complexity, SpecialRequirement, Discount).
        /// </summary>
        [JsonPropertyName("ruleType")]
        public RuleType RuleType { get; set; }

        /// <summary>
        /// Gets or sets the name of the rule.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the rule.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the expression used to evaluate the rule.
        /// This contains the formula or logic that will be executed when applying the rule.
        /// </summary>
        [JsonPropertyName("expression")]
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets the date from which this rule is effective.
        /// </summary>
        [JsonPropertyName("effectiveFrom")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Gets or sets the date until which this rule is effective (null if indefinite).
        /// </summary>
        [JsonPropertyName("effectiveTo")]
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Gets or sets the priority of the rule (higher priority rules are applied first).
        /// </summary>
        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the list of parameters used in the rule's expression.
        /// </summary>
        [JsonPropertyName("parameters")]
        public List<RuleParameterResponse> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the list of conditions that determine when this rule should be applied.
        /// </summary>
        [JsonPropertyName("conditions")]
        public List<RuleConditionResponse> Conditions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the rule is currently active.
        /// </summary>
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the date when the rule was last updated.
        /// </summary>
        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleResponse"/> class.
        /// </summary>
        public RuleResponse()
        {
            Parameters = new List<RuleParameterResponse>();
            Conditions = new List<RuleConditionResponse>();
            IsActive = true;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Response model for a parameter used in a rule's expression.
    /// These parameters define the inputs required for evaluating rule expressions.
    /// </summary>
    public class RuleParameterResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the parameter.
        /// </summary>
        [JsonPropertyName("parameterId")]
        public string ParameterId { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data type of the parameter (e.g., string, decimal, int).
        /// </summary>
        [JsonPropertyName("dataType")]
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the default value of the parameter.
        /// </summary>
        [JsonPropertyName("defaultValue")]
        public string DefaultValue { get; set; }
    }

    /// <summary>
    /// Response model for a condition that determines when a rule should be applied.
    /// Conditions are used to create complex rule activation logic based on multiple factors.
    /// </summary>
    public class RuleConditionResponse
    {
        /// <summary>
        /// Gets or sets the parameter to evaluate in the condition.
        /// </summary>
        [JsonPropertyName("parameter")]
        public string Parameter { get; set; }

        /// <summary>
        /// Gets or sets the operator to use in the condition (e.g., equals, greaterThan, lessThan).
        /// </summary>
        [JsonPropertyName("operator")]
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the value to compare against in the condition.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}