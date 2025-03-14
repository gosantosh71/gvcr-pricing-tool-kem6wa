using System; // Version 6.0.0 - Core .NET functionality
using System.Collections.Generic; // Version 6.0.0 - For collection types and generic operations
using System.Text.RegularExpressions;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Constants;
using VatFilingPricingTool.Domain.Exceptions;

namespace VatFilingPricingTool.Domain.Entities
{
    /// <summary>
    /// Represents a parameter used in a rule's expression for dynamic value substitution during rule evaluation.
    /// Parameters allow rules to be configurable and adaptable to different calculation scenarios.
    /// </summary>
    public class RuleParameter
    {
        /// <summary>
        /// Gets the unique identifier for the parameter.
        /// </summary>
        public string ParameterId { get; private set; }

        /// <summary>
        /// Gets the name of the parameter used in rule expressions.
        /// Must be alphanumeric and start with a letter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the data type of the parameter (e.g., string, number, boolean, date).
        /// </summary>
        public string DataType { get; private set; }

        /// <summary>
        /// Gets the default value for this parameter when no value is provided.
        /// </summary>
        public string DefaultValue { get; private set; }

        /// <summary>
        /// Gets or sets the rule that this parameter belongs to.
        /// </summary>
        public Rule Rule { get; internal set; }

        /// <summary>
        /// Supported data types for rule parameters
        /// </summary>
        private static readonly HashSet<string> SupportedDataTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "string",
            "number",
            "boolean",
            "date"
        };

        /// <summary>
        /// Private constructor to enforce creation through factory methods.
        /// </summary>
        private RuleParameter()
        {
            ParameterId = string.Empty;
            Name = string.Empty;
            DataType = string.Empty;
            DefaultValue = string.Empty;
        }

        /// <summary>
        /// Creates a new RuleParameter with the specified properties.
        /// </summary>
        /// <param name="name">The name of the parameter (must be alphanumeric and start with a letter).</param>
        /// <param name="dataType">The data type of the parameter (string, number, boolean, date).</param>
        /// <param name="defaultValue">The default value for this parameter.</param>
        /// <returns>A new validated RuleParameter instance.</returns>
        /// <exception cref="ValidationException">Thrown when the parameter data is invalid.</exception>
        public static RuleParameter Create(string name, string dataType, string defaultValue = "")
        {
            // Validate parameter data
            Validate(name, dataType);

            // Create and initialize a new parameter
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
        /// Updates the default value of the parameter.
        /// </summary>
        /// <param name="newDefaultValue">The new default value to set.</param>
        public void UpdateDefaultValue(string newDefaultValue)
        {
            DefaultValue = newDefaultValue ?? string.Empty;
        }

        /// <summary>
        /// Validates that the parameter name and data type meet the required format and constraints.
        /// </summary>
        /// <param name="name">The parameter name to validate.</param>
        /// <param name="dataType">The parameter data type to validate.</param>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
        private static void Validate(string name, string dataType)
        {
            List<string> validationErrors = new List<string>();

            // Validate name
            if (string.IsNullOrWhiteSpace(name))
            {
                validationErrors.Add("Parameter name cannot be null or empty.");
                throw new ValidationException("Invalid parameter name", validationErrors, ErrorCodes.Rule.InvalidParameterName);
            }

            // Check if name follows parameter naming rules (alphanumeric, starts with letter)
            if (!Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
            {
                validationErrors.Add("Parameter name must start with a letter and contain only letters, numbers, and underscores.");
                throw new ValidationException("Invalid parameter name format", validationErrors, ErrorCodes.Rule.InvalidParameterName);
            }

            // Validate data type
            if (string.IsNullOrWhiteSpace(dataType))
            {
                validationErrors.Add("Parameter data type cannot be null or empty.");
                throw new ValidationException("Invalid parameter data type", validationErrors, ErrorCodes.Rule.InvalidParameterDataType);
            }

            // Check if data type is supported
            if (!SupportedDataTypes.Contains(dataType))
            {
                validationErrors.Add($"Unsupported parameter data type. Supported types are: {string.Join(", ", SupportedDataTypes)}");
                throw new ValidationException("Unsupported parameter data type", validationErrors, ErrorCodes.Rule.InvalidParameterDataType);
            }
        }
    }
}