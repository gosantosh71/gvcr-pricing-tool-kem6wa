using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Domain.Constants;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Rules.Expressions;

namespace VatFilingPricingTool.Domain.Entities
{
    /// <summary>
    /// Represents a country-specific VAT rule with an expression that is evaluated during pricing calculations
    /// </summary>
    public class Rule
    {
        /// <summary>
        /// Gets the unique identifier for the rule
        /// </summary>
        public string RuleId { get; private set; }

        /// <summary>
        /// Gets the country code for which this rule applies
        /// </summary>
        public CountryCode CountryCode { get; private set; }

        /// <summary>
        /// Gets the type of rule (VatRate, Threshold, Complexity, etc.)
        /// </summary>
        public RuleType Type { get; private set; }

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the rule
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the expression that calculates the rule's result
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        /// Gets the date from which this rule is effective
        /// </summary>
        public DateTime EffectiveFrom { get; private set; }

        /// <summary>
        /// Gets the date until which this rule is effective (null means indefinitely)
        /// </summary>
        public DateTime? EffectiveTo { get; private set; }

        /// <summary>
        /// Gets the priority of the rule (lower number = higher priority)
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the rule is active
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets the last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; private set; }

        /// <summary>
        /// Gets the collection of parameters used in the rule expression
        /// </summary>
        public ICollection<RuleParameter> Parameters { get; private set; }

        /// <summary>
        /// Gets the collection of conditions that determine when this rule applies
        /// </summary>
        public ICollection<RuleCondition> Conditions { get; private set; }

        /// <summary>
        /// Gets or sets the associated country entity (navigation property)
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        /// Private constructor for the Rule entity to enforce creation through factory methods
        /// </summary>
        private Rule()
        {
            Parameters = new List<RuleParameter>();
            Conditions = new List<RuleCondition>();
            IsActive = true;
            LastUpdated = DateTime.UtcNow;
            Priority = DomainConstants.Rules.DefaultRulePriority;
        }

        /// <summary>
        /// Factory method to create a new Rule instance with validation
        /// </summary>
        /// <param name="countryCode">The country code for which this rule applies</param>
        /// <param name="ruleType">The type of rule</param>
        /// <param name="name">The name of the rule</param>
        /// <param name="expression">The expression that calculates the rule's result</param>
        /// <param name="effectiveFrom">The date from which this rule is effective</param>
        /// <param name="description">Optional description of the rule</param>
        /// <returns>A new validated Rule instance</returns>
        public static Rule Create(string countryCode, RuleType ruleType, string name, string expression, DateTime effectiveFrom, string description = null)
        {
            Validate(countryCode, name, expression, effectiveFrom);

            var rule = new Rule
            {
                RuleId = Guid.NewGuid().ToString(),
                CountryCode = CountryCode.Create(countryCode),
                Type = ruleType,
                Name = name,
                Description = description ?? string.Empty,
                Expression = expression,
                EffectiveFrom = effectiveFrom
            };

            return rule;
        }

        /// <summary>
        /// Adds a parameter to the rule
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="dataType">Parameter data type</param>
        /// <param name="defaultValue">Default parameter value</param>
        public void AddParameter(string name, string dataType, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ValidationException("Parameter name cannot be null or empty",
                    new List<string> { "Parameter name is required" });
            }

            if (string.IsNullOrEmpty(dataType))
            {
                throw new ValidationException("Parameter data type cannot be null or empty",
                    new List<string> { "Parameter data type is required" });
            }

            // Check if parameter with the same name already exists
            foreach (var param in Parameters)
            {
                if (param.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ValidationException("Parameter name already exists",
                        new List<string> { $"A parameter with the name '{name}' already exists in this rule" },
                        ErrorCodes.Rule.DuplicateParameterName);
                }
            }

            var parameter = RuleParameter.Create(name, dataType, defaultValue);
            Parameters.Add(parameter);
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes a parameter from the rule
        /// </summary>
        /// <param name="parameterId">The ID of the parameter to remove</param>
        public void RemoveParameter(string parameterId)
        {
            var parameter = Parameters.FirstOrDefault(p => p.ParameterId == parameterId);
            if (parameter == null)
            {
                throw new ValidationException("Parameter not found",
                    new List<string> { $"Parameter with ID '{parameterId}' does not exist in this rule" },
                    ErrorCodes.Rule.ParameterNotFound);
            }

            Parameters.Remove(parameter);
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a condition to the rule
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="operator">Comparison operator</param>
        /// <param name="value">Comparison value</param>
        public void AddCondition(string parameter, string @operator, string value)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ValidationException("Condition parameter cannot be null or empty",
                    new List<string> { "Condition parameter is required" });
            }

            if (string.IsNullOrEmpty(@operator))
            {
                throw new ValidationException("Condition operator cannot be null or empty",
                    new List<string> { "Condition operator is required" });
            }

            if (!IsValidOperator(@operator))
            {
                throw new ValidationException("Invalid condition operator",
                    new List<string> { $"Operator '{@operator}' is not valid. Valid operators are: equals, notEquals, greaterThan, lessThan, contains" });
            }

            if (value == null)
            {
                throw new ValidationException("Condition value cannot be null",
                    new List<string> { "Condition value is required" });
            }

            var condition = new RuleCondition(parameter, @operator, value);
            Conditions.Add(condition);
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes a condition from the rule
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="operator">Comparison operator</param>
        /// <param name="value">Comparison value</param>
        public void RemoveCondition(string parameter, string @operator, string value)
        {
            var condition = Conditions.FirstOrDefault(c =>
                c.Parameter.Equals(parameter, StringComparison.OrdinalIgnoreCase) &&
                c.Operator.Equals(@operator, StringComparison.OrdinalIgnoreCase) &&
                c.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

            if (condition == null)
            {
                throw new ValidationException("Condition not found",
                    new List<string> { $"Condition with parameter '{parameter}', operator '{@operator}', and value '{value}' does not exist in this rule" },
                    ErrorCodes.Rule.ConditionNotFound);
            }

            Conditions.Remove(condition);
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the rule expression
        /// </summary>
        /// <param name="newExpression">The new expression</param>
        public void UpdateExpression(string newExpression)
        {
            if (string.IsNullOrEmpty(newExpression))
            {
                throw new ValidationException("Expression cannot be null or empty",
                    new List<string> { "Expression is required" });
            }

            if (newExpression.Length > DomainConstants.Validation.MaxRuleExpressionLength)
            {
                throw new ValidationException("Expression is too long",
                    new List<string> { $"Expression cannot exceed {DomainConstants.Validation.MaxRuleExpressionLength} characters" });
            }

            // Validate expression syntax
            ExpressionEvaluator.ValidateExpression(newExpression);

            Expression = newExpression;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the rule name
        /// </summary>
        /// <param name="newName">The new name</param>
        public void UpdateName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                throw new ValidationException("Name cannot be null or empty",
                    new List<string> { "Name is required" });
            }

            if (newName.Length > DomainConstants.Validation.MaxNameLength)
            {
                throw new ValidationException("Name is too long",
                    new List<string> { $"Name cannot exceed {DomainConstants.Validation.MaxNameLength} characters" });
            }

            Name = newName;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the rule description
        /// </summary>
        /// <param name="newDescription">The new description</param>
        public void UpdateDescription(string newDescription)
        {
            if (newDescription != null && newDescription.Length > DomainConstants.Validation.MaxDescriptionLength)
            {
                throw new ValidationException("Description is too long",
                    new List<string> { $"Description cannot exceed {DomainConstants.Validation.MaxDescriptionLength} characters" });
            }

            Description = newDescription ?? string.Empty;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the rule effective date range
        /// </summary>
        /// <param name="newEffectiveFrom">The new effective from date</param>
        /// <param name="newEffectiveTo">The new effective to date (optional)</param>
        public void UpdateEffectiveDates(DateTime newEffectiveFrom, DateTime? newEffectiveTo = null)
        {
            if (newEffectiveFrom < DateTime.UtcNow.Date && newEffectiveFrom != EffectiveFrom)
            {
                throw new ValidationException("Effective from date cannot be in the past",
                    new List<string> { "Effective from date must be today or later" });
            }

            if (newEffectiveTo.HasValue && newEffectiveTo < newEffectiveFrom)
            {
                throw new ValidationException("Effective to date must be after effective from date",
                    new List<string> { "Effective to date cannot be earlier than effective from date" });
            }

            EffectiveFrom = newEffectiveFrom;
            EffectiveTo = newEffectiveTo;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the rule priority
        /// </summary>
        /// <param name="newPriority">The new priority</param>
        public void UpdatePriority(int newPriority)
        {
            if (newPriority < DomainConstants.Rules.MinRulePriority || newPriority > DomainConstants.Rules.MaxRulePriority)
            {
                throw new ValidationException("Invalid priority value",
                    new List<string> { $"Priority must be between {DomainConstants.Rules.MinRulePriority} and {DomainConstants.Rules.MaxRulePriority}" });
            }

            Priority = newPriority;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the rule as active or inactive
        /// </summary>
        /// <param name="active">Whether the rule should be active</param>
        public void SetActive(bool active)
        {
            IsActive = active;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the rule is effective at a specific date
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <returns>True if the rule is effective at the specified date, false otherwise</returns>
        public bool IsEffectiveAt(DateTime date)
        {
            return date >= EffectiveFrom && (!EffectiveTo.HasValue || date < EffectiveTo.Value);
        }

        /// <summary>
        /// Validates rule data according to business rules
        /// </summary>
        /// <param name="countryCode">The country code</param>
        /// <param name="name">The rule name</param>
        /// <param name="expression">The rule expression</param>
        /// <param name="effectiveFrom">The effective from date</param>
        private static void Validate(string countryCode, string name, string expression, DateTime effectiveFrom)
        {
            List<string> validationErrors = new List<string>();

            if (string.IsNullOrEmpty(countryCode))
            {
                validationErrors.Add("Country code is required");
            }

            if (string.IsNullOrEmpty(name))
            {
                validationErrors.Add("Name is required");
            }
            else if (name.Length > DomainConstants.Validation.MaxNameLength)
            {
                validationErrors.Add($"Name cannot exceed {DomainConstants.Validation.MaxNameLength} characters");
            }

            if (string.IsNullOrEmpty(expression))
            {
                validationErrors.Add("Expression is required");
            }
            else if (expression.Length > DomainConstants.Validation.MaxRuleExpressionLength)
            {
                validationErrors.Add($"Expression cannot exceed {DomainConstants.Validation.MaxRuleExpressionLength} characters");
            }
            else
            {
                try
                {
                    // Validate expression syntax
                    ExpressionEvaluator.ValidateExpression(expression);
                }
                catch (Exception ex)
                {
                    validationErrors.Add($"Invalid expression: {ex.Message}");
                }
            }

            if (effectiveFrom < DateTime.UtcNow.Date)
            {
                validationErrors.Add("Effective from date cannot be in the past");
            }

            if (validationErrors.Any())
            {
                throw new ValidationException("Rule validation failed", validationErrors, ErrorCodes.Rule.RuleValidationFailed);
            }
        }

        /// <summary>
        /// Checks if an operator is valid
        /// </summary>
        /// <param name="operator">The operator to check</param>
        /// <returns>True if the operator is valid, false otherwise</returns>
        private bool IsValidOperator(string @operator)
        {
            return @operator == "equals" ||
                   @operator == "notEquals" ||
                   @operator == "greaterThan" ||
                   @operator == "lessThan" ||
                   @operator == "contains";
        }
    }

    /// <summary>
    /// Represents a parameter used in a rule's expression for dynamic value substitution during rule evaluation
    /// </summary>
    public class RuleParameter
    {
        /// <summary>
        /// Gets the unique identifier for the parameter
        /// </summary>
        public string ParameterId { get; private set; }

        /// <summary>
        /// Gets the name of the parameter
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the data type of the parameter
        /// </summary>
        public string DataType { get; private set; }

        /// <summary>
        /// Gets the default value of the parameter
        /// </summary>
        public string DefaultValue { get; private set; }

        /// <summary>
        /// Private constructor for the RuleParameter entity to enforce creation through factory methods
        /// </summary>
        private RuleParameter()
        {
        }

        /// <summary>
        /// Factory method to create a new RuleParameter instance with validation
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="dataType">The data type of the parameter</param>
        /// <param name="defaultValue">The default value of the parameter</param>
        /// <returns>A new validated RuleParameter instance</returns>
        public static RuleParameter Create(string name, string dataType, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ValidationException("Parameter name cannot be null or empty",
                    new List<string> { "Parameter name is required" });
            }

            if (!IsValidParameterName(name))
            {
                throw new ValidationException("Invalid parameter name",
                    new List<string> { "Parameter name must start with a letter and contain only letters, numbers, and underscores" });
            }

            if (string.IsNullOrEmpty(dataType))
            {
                throw new ValidationException("Parameter data type cannot be null or empty",
                    new List<string> { "Parameter data type is required" });
            }

            if (!IsValidDataType(dataType))
            {
                throw new ValidationException("Invalid parameter data type",
                    new List<string> { $"Data type '{dataType}' is not supported. Supported types are: string, number, boolean, date" });
            }

            var parameter = new RuleParameter
            {
                ParameterId = Guid.NewGuid().ToString(),
                Name = name,
                DataType = dataType,
                DefaultValue = defaultValue ?? string.Empty
            };

            return parameter;
        }

        /// <summary>
        /// Updates the default value of the parameter
        /// </summary>
        /// <param name="newDefaultValue">The new default value</param>
        public void UpdateDefaultValue(string newDefaultValue)
        {
            DefaultValue = newDefaultValue ?? string.Empty;
        }

        /// <summary>
        /// Checks if a parameter name is valid
        /// </summary>
        /// <param name="name">The parameter name to check</param>
        /// <returns>True if the parameter name is valid, false otherwise</returns>
        private static bool IsValidParameterName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (!char.IsLetter(name[0]))
            {
                return false;
            }

            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a data type is valid
        /// </summary>
        /// <param name="dataType">The data type to check</param>
        /// <returns>True if the data type is valid, false otherwise</returns>
        private static bool IsValidDataType(string dataType)
        {
            return dataType == "string" ||
                   dataType == "number" ||
                   dataType == "boolean" ||
                   dataType == "date";
        }
    }

    /// <summary>
    /// Represents a condition that determines when a rule should be applied during calculation
    /// </summary>
    public class RuleCondition
    {
        /// <summary>
        /// Gets the parameter name for the condition
        /// </summary>
        public string Parameter { get; private set; }

        /// <summary>
        /// Gets the operator for the condition
        /// </summary>
        public string Operator { get; private set; }

        /// <summary>
        /// Gets the value for the condition
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Creates a new RuleCondition with validation
        /// </summary>
        /// <param name="parameter">The parameter name</param>
        /// <param name="operator">The operator</param>
        /// <param name="value">The value</param>
        public RuleCondition(string parameter, string @operator, string value)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ValidationException("Parameter cannot be null or empty",
                    new List<string> { "Parameter is required" });
            }

            if (string.IsNullOrEmpty(@operator))
            {
                throw new ValidationException("Operator cannot be null or empty",
                    new List<string> { "Operator is required" });
            }

            if (!IsValidOperator(@operator))
            {
                throw new ValidationException("Invalid operator",
                    new List<string> { $"Operator '{@operator}' is not valid. Valid operators are: equals, notEquals, greaterThan, lessThan, contains" });
            }

            if (value == null)
            {
                throw new ValidationException("Value cannot be null",
                    new List<string> { "Value is required" });
            }

            Parameter = parameter;
            Operator = @operator;
            Value = value;
        }

        /// <summary>
        /// Checks if an operator is valid
        /// </summary>
        /// <param name="operator">The operator to check</param>
        /// <returns>True if the operator is valid, false otherwise</returns>
        private bool IsValidOperator(string @operator)
        {
            return @operator == "equals" ||
                   @operator == "notEquals" ||
                   @operator == "greaterThan" ||
                   @operator == "lessThan" ||
                   @operator == "contains";
        }
    }
}