using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Rules.Expressions;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Domain.Rules
{
    /// <summary>
    /// Evaluates VAT rule expressions and conditions with parameter substitution.
    /// Serves as a bridge between the Rule Engine and Expression Evaluator.
    /// </summary>
    public static class RuleEvaluator
    {
        /// <summary>
        /// Evaluates a rule's expression with the provided parameters.
        /// </summary>
        /// <param name="rule">The rule to evaluate.</param>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>Result of the rule evaluation as decimal.</returns>
        public static decimal EvaluateRule(Rule rule, Dictionary<string, object> parameters)
        {
            if (rule == null)
            {
                throw new ValidationException("Rule cannot be null",
                    new List<string> { "A valid rule must be provided for evaluation" });
            }

            if (parameters == null)
            {
                throw new ValidationException("Parameters cannot be null",
                    new List<string> { "Parameter dictionary must be provided for rule evaluation" });
            }

            if (string.IsNullOrEmpty(rule.Expression))
            {
                throw new ValidationException("Rule expression cannot be null or empty",
                    new List<string> { "Rule must have a valid expression to evaluate" },
                    ErrorCodes.Rule.InvalidRuleExpression);
            }

            return ExpressionEvaluator.Evaluate(rule.Expression, parameters);
        }

        /// <summary>
        /// Checks if all conditions of a rule are satisfied with the provided parameters.
        /// </summary>
        /// <param name="rule">The rule to check.</param>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>True if all conditions are satisfied, otherwise false.</returns>
        public static bool CheckConditions(Rule rule, Dictionary<string, object> parameters)
        {
            if (rule == null)
            {
                throw new ValidationException("Rule cannot be null",
                    new List<string> { "A valid rule must be provided for condition checking" });
            }

            if (parameters == null)
            {
                throw new ValidationException("Parameters cannot be null",
                    new List<string> { "Parameter dictionary must be provided for condition checking" });
            }

            // If there are no conditions, the rule applies unconditionally
            if (rule.Conditions == null || !rule.Conditions.Any())
            {
                return true;
            }

            // Check each condition
            foreach (var condition in rule.Conditions)
            {
                // Check if the parameter exists
                if (!parameters.TryGetValue(condition.Parameter, out object parameterValue))
                {
                    // If parameter doesn't exist, the condition can't be evaluated so the rule doesn't apply
                    return false;
                }

                // Evaluate the condition
                if (!EvaluateCondition(condition, parameterValue))
                {
                    // If any condition is not satisfied, the rule doesn't apply
                    return false;
                }
            }

            // All conditions satisfied
            return true;
        }

        /// <summary>
        /// Evaluates a single condition with the provided parameter value.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="parameterValue">The parameter value to check against.</param>
        /// <returns>True if the condition is satisfied, otherwise false.</returns>
        private static bool EvaluateCondition(RuleCondition condition, object parameterValue)
        {
            // Convert values to comparable types
            var conditionValue = ConvertToComparableValue(condition.Value);
            var paramValue = ConvertToComparableValue(parameterValue);

            // Compare based on operator (using case-insensitive comparison)
            switch (condition.Operator.ToLowerInvariant())
            {
                case "equals":
                    return CompareValues(paramValue, conditionValue, "equals");
                case "notequals":
                    return CompareValues(paramValue, conditionValue, "notequals");
                case "greaterthan":
                    return CompareValues(paramValue, conditionValue, "greaterthan");
                case "lessthan":
                    return CompareValues(paramValue, conditionValue, "lessthan");
                case "greaterthanorequal":
                    return CompareValues(paramValue, conditionValue, "greaterthanorequal");
                case "lessthanorequal":
                    return CompareValues(paramValue, conditionValue, "lessthanorequal");
                case "contains":
                    return CompareValues(paramValue, conditionValue, "contains");
                case "startswith":
                    return CompareValues(paramValue, conditionValue, "startswith");
                case "endswith":
                    return CompareValues(paramValue, conditionValue, "endswith");
                default:
                    throw new ValidationException($"Invalid operator: {condition.Operator}",
                        new List<string> { $"Operator '{condition.Operator}' is not supported" },
                        ErrorCodes.Rule.InvalidOperator);
            }
        }

        /// <summary>
        /// Compares two values of potentially different types.
        /// </summary>
        /// <param name="value1">First value.</param>
        /// <param name="value2">Second value.</param>
        /// <param name="operator">Comparison operator.</param>
        /// <returns>Result of the comparison.</returns>
        private static bool CompareValues(object value1, object value2, string @operator)
        {
            // Handle null values
            if (value1 == null && value2 == null)
            {
                return @operator == "equals";
            }

            if (value1 == null || value2 == null)
            {
                return @operator == "notequals";
            }

            // If both values are numeric, compare as decimal
            if (value1 is decimal num1 && value2 is decimal num2)
            {
                switch (@operator)
                {
                    case "equals": return num1 == num2;
                    case "notequals": return num1 != num2;
                    case "greaterthan": return num1 > num2;
                    case "lessthan": return num1 < num2;
                    case "greaterthanorequal": return num1 >= num2;
                    case "lessthanorequal": return num1 <= num2;
                    default: return false; // Other operators don't make sense for numeric comparison
                }
            }

            // If both values are strings, compare as strings
            if (value1 is string str1 && value2 is string str2)
            {
                switch (@operator)
                {
                    case "equals": return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
                    case "notequals": return !string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
                    case "contains": return str1.IndexOf(str2, StringComparison.OrdinalIgnoreCase) >= 0;
                    case "startswith": return str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);
                    case "endswith": return str1.EndsWith(str2, StringComparison.OrdinalIgnoreCase);
                    default: return false; // Other operators don't make sense for string comparison
                }
            }

            // If both values are boolean, compare as boolean
            if (value1 is bool bool1 && value2 is bool bool2)
            {
                switch (@operator)
                {
                    case "equals": return bool1 == bool2;
                    case "notequals": return bool1 != bool2;
                    default: return false; // Other operators don't make sense for boolean comparison
                }
            }

            // If both values are DateTime, compare as DateTime
            if (value1 is DateTime date1 && value2 is DateTime date2)
            {
                switch (@operator)
                {
                    case "equals": return date1 == date2;
                    case "notequals": return date1 != date2;
                    case "greaterthan": return date1 > date2;
                    case "lessthan": return date1 < date2;
                    case "greaterthanorequal": return date1 >= date2;
                    case "lessthanorequal": return date1 <= date2;
                    default: return false; // Other operators don't make sense for date comparison
                }
            }

            // If types are different, attempt to convert to common type for comparison
            try
            {
                // Try to convert to string and compare
                string str1 = value1.ToString();
                string str2 = value2.ToString();

                switch (@operator)
                {
                    case "equals": return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
                    case "notequals": return !string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
                    case "contains": return str1.IndexOf(str2, StringComparison.OrdinalIgnoreCase) >= 0;
                    case "startswith": return str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);
                    case "endswith": return str1.EndsWith(str2, StringComparison.OrdinalIgnoreCase);
                    default: return false;
                }
            }
            catch
            {
                // If comparison fails, the values are not compatible
                return false;
            }
        }

        /// <summary>
        /// Converts a value to a type that can be compared.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value converted to a comparable type.</returns>
        private static object ConvertToComparableValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            // If already a numeric type, convert to decimal for consistency
            if (value is int || value is long || value is double || value is float || value is decimal)
            {
                return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }

            // Handle string values
            string stringValue = value.ToString();

            // Try to parse as decimal if the string represents a number
            if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal numericValue))
            {
                return numericValue;
            }

            // Try to parse as DateTime if the string represents a date
            if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
            {
                return dateValue;
            }

            // Try to parse as boolean if the string represents a boolean
            if (bool.TryParse(stringValue, out bool boolValue))
            {
                return boolValue;
            }

            // If no conversion was successful, return the original value
            return stringValue;
        }

        /// <summary>
        /// Validates that a rule expression can be evaluated.
        /// </summary>
        /// <param name="expression">The expression to validate.</param>
        /// <returns>True if the expression is valid, otherwise throws an exception.</returns>
        public static bool ValidateRuleExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ValidationException("Rule expression cannot be null or empty",
                    new List<string> { "A valid expression must be provided" },
                    ErrorCodes.Rule.InvalidRuleExpression);
            }

            return ExpressionEvaluator.ValidateExpression(expression);
        }
    }
}