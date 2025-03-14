using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using VatFilingPricingTool.Common.Validation; // Import Validators class
using VatFilingPricingTool.Common.Helpers; // Import ValidationHelper class
using VatFilingPricingTool.Common.Models; // Import Result class
using VatFilingPricingTool.Service.Models; // Import CalculationModel, CountryModel, RuleModel, ReportModel, UserModel
// Provides namespace for the validators
namespace VatFilingPricingTool.Service.Validators
{
    /// <summary>
    /// Static class providing validation methods for service layer models
    /// </summary>
    public static class ModelValidators
    {
        /// <summary>
        /// Validates a CalculationModel according to business rules
        /// </summary>
        /// <param name="model">The CalculationModel to validate</param>
        /// <returns>Success result if validation passes, validation failure result otherwise</returns>
        public static Result ValidateCalculationModel(CalculationModel model)
        {
            // Initialize a list to store validation errors
            var validationErrors = new List<string>();

            // Validate UserId using Validators.ValidateString (required, min length 1)
            validationErrors.AddRange(Validators.ValidateString(model.UserId, "UserId", minLength: 1));

            // Validate ServiceType using Validators.ValidateEnum
            validationErrors.AddRange(Validators.ValidateEnum(model.ServiceType, "ServiceType"));

            // Validate TransactionVolume using Validators.ValidateInteger (min value 1, max value 1000000)
            validationErrors.AddRange(Validators.ValidateInteger(model.TransactionVolume, "TransactionVolume", minValue: 1, maxValue: 1000000));

            // Validate Frequency using Validators.ValidateEnum
            validationErrors.AddRange(Validators.ValidateEnum(model.Frequency, "Frequency"));

            // Validate CountryBreakdowns using Validators.ValidateCollection (required, min count 1)
            validationErrors.AddRange(Validators.ValidateCollection(model.CountryBreakdowns, "CountryBreakdowns", required: true, minCount: 1));

            // For each country in CountryBreakdowns, validate CountryCode using Validators.ValidateCountryCode
            if (model.CountryBreakdowns != null)
            {
                foreach (var country in model.CountryBreakdowns)
                {
                    validationErrors.AddRange(Validators.ValidateCountryCode(country.CountryCode, "CountryCode"));
                }
            }

            // Return ValidationHelper.CreateValidationResult with the collected validation errors
            return ValidationHelper.CreateValidationResult(validationErrors);
        }

        /// <summary>
        /// Validates a CountryModel according to business rules
        /// </summary>
        /// <param name="model">The CountryModel to validate</param>
        /// <returns>Success result if validation passes, validation failure result otherwise</returns>
        public static Result ValidateCountryModel(CountryModel model)
        {
            // Initialize a list to store validation errors
            var validationErrors = new List<string>();

            // Validate CountryCode using Validators.ValidateCountryCode
            validationErrors.AddRange(Validators.ValidateCountryCode(model.CountryCode, "CountryCode"));

            // Validate Name using Validators.ValidateString (required, min length 2, max length 100)
            validationErrors.AddRange(Validators.ValidateString(model.Name, "Name", required: true, minLength: 2, maxLength: 100));

            // Validate StandardVatRate using Validators.ValidateNumeric (min value 0, max value 100)
            validationErrors.AddRange(Validators.ValidateNumeric(model.StandardVatRate, "StandardVatRate", minValue: 0, maxValue: 100));

            // Validate CurrencyCode using Validators.ValidateString (required, min length 3, max length 3)
            validationErrors.AddRange(Validators.ValidateString(model.CurrencyCode, "CurrencyCode", required: true, minLength: 3, maxLength: 3));

            // Validate AvailableFilingFrequencies using Validators.ValidateCollection (required, min count 1)
            validationErrors.AddRange(Validators.ValidateCollection(model.AvailableFilingFrequencies, "AvailableFilingFrequencies", required: true, minCount: 1));

            // Return ValidationHelper.CreateValidationResult with the collected validation errors
            return ValidationHelper.CreateValidationResult(validationErrors);
        }

        /// <summary>
        /// Validates a RuleModel according to business rules
        /// </summary>
        /// <param name="model">The RuleModel to validate</param>
        /// <returns>Success result if validation passes, validation failure result otherwise</returns>
        public static Result ValidateRuleModel(RuleModel model)
        {
            // Initialize a list to store validation errors
            var validationErrors = new List<string>();

            // Validate CountryCode using Validators.ValidateCountryCode
            validationErrors.AddRange(Validators.ValidateCountryCode(model.CountryCode, "CountryCode"));

            // Validate Name using Validators.ValidateString (required, min length 3, max length 100)
            validationErrors.AddRange(Validators.ValidateString(model.Name, "Name", required: true, minLength: 3, maxLength: 100));

            // Validate Expression using Validators.ValidateString (required, min length 1, max length 500)
            validationErrors.AddRange(Validators.ValidateString(model.Expression, "Expression", required: true, minLength: 1, maxLength: 500));

            // Validate Type using Validators.ValidateEnum
            validationErrors.AddRange(Validators.ValidateEnum(model.Type, "Type"));

            // Validate EffectiveFrom using Validators.ValidateDate (min date is today for new rules)
            DateTime? minDate = string.IsNullOrEmpty(model.RuleId) ? DateTime.UtcNow.Date : (DateTime?)null;
            validationErrors.AddRange(Validators.ValidateDate(model.EffectiveFrom, "EffectiveFrom", minDate: minDate));

            // If EffectiveTo has value, validate it is after EffectiveFrom
            if (model.EffectiveTo.HasValue)
            {
                if (model.EffectiveTo < model.EffectiveFrom)
                {
                    validationErrors.Add(ValidationHelper.CreateInvalidValueError("EffectiveTo", "EffectiveTo must be after EffectiveFrom"));
                }
            }

            // Validate Parameters (if any) for valid names and data types
            if (model.Parameters != null)
            {
                foreach (var parameter in model.Parameters)
                {
                    var parameterErrors = ValidateRuleParameterModel(parameter);
                    validationErrors.AddRange(parameterErrors);
                }
            }

            // Validate Conditions (if any) for valid parameters, operators, and values
            if (model.Conditions != null)
            {
                foreach (var condition in model.Conditions)
                {
                    var conditionErrors = ValidateRuleConditionModel(condition);
                    validationErrors.AddRange(conditionErrors);
                }
            }

            // Return ValidationHelper.CreateValidationResult with the collected validation errors
            return ValidationHelper.CreateValidationResult(validationErrors);
        }

        /// <summary>
        /// Validates a ReportModel according to business rules
        /// </summary>
        /// <param name="model">The ReportModel to validate</param>
        /// <returns>Success result if validation passes, validation failure result otherwise</returns>
        public static Result ValidateReportModel(ReportModel model)
        {
            // Initialize a list to store validation errors
            var validationErrors = new List<string>();

            // Validate ReportTitle using Validators.ValidateString (required, min length 3, max length 100)
            validationErrors.AddRange(Validators.ValidateString(model.ReportTitle, "ReportTitle", required: true, minLength: 3, maxLength: 100));

            // Validate CalculationId using Validators.ValidateString (required, min length 1)
            validationErrors.AddRange(Validators.ValidateString(model.CalculationId, "CalculationId", required: true, minLength: 1));

            // Validate Format using Validators.ValidateEnum
            validationErrors.AddRange(Validators.ValidateEnum(model.Format, "Format"));

            // Check if CalculationData is null, add error if it is
            if (model.CalculationData == null)
            {
                validationErrors.Add(ValidationHelper.CreateRequiredFieldError("CalculationData"));
            }
            else
            {
                // If CalculationData is not null, validate it using ValidateCalculationModel
                var calculationResult = ValidateCalculationModel(model.CalculationData);
                if (!calculationResult.IsSuccess)
                {
                    validationErrors.AddRange(calculationResult.ValidationErrors);
                }
            }

            // Return ValidationHelper.CreateValidationResult with the collected validation errors
            return ValidationHelper.CreateValidationResult(validationErrors);
        }

        /// <summary>
        /// Validates a UserModel according to business rules
        /// </summary>
        /// <param name="model">The UserModel to validate</param>
        /// <returns>Success result if validation passes, validation failure result otherwise</returns>
        public static Result ValidateUserModel(UserModel model)
        {
            // Initialize a list to store validation errors
            var validationErrors = new List<string>();

            // Validate Email using Validators.ValidateEmail (required)
            validationErrors.AddRange(Validators.ValidateEmail(model.Email, "Email", required: true));

            // Validate FirstName using Validators.ValidateString (required, min length 1, max length 50)
            validationErrors.AddRange(Validators.ValidateString(model.FirstName, "FirstName", required: true, minLength: 1, maxLength: 50));

            // Validate LastName using Validators.ValidateString (required, min length 1, max length 50)
            validationErrors.AddRange(Validators.ValidateString(model.LastName, "LastName", required: true, minLength: 1, maxLength: 50));

            // Validate Roles using Validators.ValidateCollection (required, min count 1)
            validationErrors.AddRange(Validators.ValidateCollection(model.Roles, "Roles", required: true, minCount: 1));

            // For each role in Roles, validate it using Validators.ValidateEnum
            if (model.Roles != null)
            {
                foreach (var role in model.Roles)
                {
                    validationErrors.AddRange(Validators.ValidateEnum(role, "Role"));
                }
            }

            // If PhoneNumber is not null or empty, validate it using Validators.ValidatePhoneNumber
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                validationErrors.AddRange(Validators.ValidatePhoneNumber(model.PhoneNumber, "PhoneNumber", required: false));
            }

            // Return ValidationHelper.CreateValidationResult with the collected validation errors
            return ValidationHelper.CreateValidationResult(validationErrors);
        }

        /// <summary>
        /// Validates a RuleParameterModel according to business rules
        /// </summary>
        /// <param name="model">The RuleParameterModel to validate</param>
        /// <returns>List of validation errors, empty if validation passes</returns>
        public static List<string> ValidateRuleParameterModel(RuleParameterModel model)
        {
            // Initialize a list to store validation errors
            var validationErrors = new List<string>();

            // Validate Name using Validators.ValidateString (required, min length 1, max length 50)
            validationErrors.AddRange(Validators.ValidateString(model.Name, "Name", required: true, minLength: 1, maxLength: 50));

            // Validate DataType using Validators.ValidateString (required, min length 1, max length 50)
            validationErrors.AddRange(Validators.ValidateString(model.DataType, "DataType", required: true, minLength: 1, maxLength: 50));

            // Check if DataType is one of the supported types (string, int, decimal, bool, datetime)
            string[] supportedDataTypes = { "string", "int", "decimal", "bool", "datetime" };
            if (!supportedDataTypes.Contains(model.DataType))
            {
                validationErrors.Add(ValidationHelper.CreateInvalidValueError("DataType", $"DataType must be one of: {string.Join(", ", supportedDataTypes)}"));
            }

            // If DefaultValue is provided, check if it can be parsed as the specified DataType
            if (!string.IsNullOrEmpty(model.DefaultValue))
            {
                try
                {
                    switch (model.DataType.ToLower())
                    {
                        case "int":
                            Convert.ToInt32(model.DefaultValue);
                            break;
                        case "decimal":
                            Convert.ToDecimal(model.DefaultValue);
                            break;
                        case "bool":
                            Convert.ToBoolean(model.DefaultValue);
                            break;
                        case "datetime":
                            Convert.ToDateTime(model.DefaultValue);
                            break;
                        // string type does not need parsing
                    }
                }
                catch (Exception)
                {
                    validationErrors.Add(ValidationHelper.CreateInvalidValueError("DefaultValue", $"Cannot parse DefaultValue as {model.DataType}"));
                }
            }

            // Return the list of validation errors
            return validationErrors;
        }

        /// <summary>
        /// Validates a RuleConditionModel according to business rules
        /// </summary>
        /// <param name="model">The RuleConditionModel to validate</param>
        /// <returns>List of validation errors, empty if validation passes</returns>
        public static List<string> ValidateRuleConditionModel(RuleConditionModel model)
        {
            // Initialize a list to store validation errors
            var validationErrors = new List<string>();

            // Validate Parameter using Validators.ValidateString (required, min length 1, max length 50)
            validationErrors.AddRange(Validators.ValidateString(model.Parameter, "Parameter", required: true, minLength: 1, maxLength: 50));

            // Validate Operator using Validators.ValidateString (required, min length 1, max length 20)
            validationErrors.AddRange(Validators.ValidateString(model.Operator, "Operator", required: true, minLength: 1, maxLength: 20));

            // Check if Operator is one of the supported operators (equals, notEquals, greaterThan, lessThan, contains, startsWith, endsWith)
            string[] supportedOperators = { "equals", "notequals", "greaterthan", "lessthan", "contains", "startswith", "endswith" };
            if (!supportedOperators.Contains(model.Operator.ToLower()))
            {
                validationErrors.Add(ValidationHelper.CreateInvalidValueError("Operator", $"Operator must be one of: {string.Join(", ", supportedOperators)}"));
            }

            // Validate Value using Validators.ValidateString (required, min length 1)
            validationErrors.AddRange(Validators.ValidateString(model.Value, "Value", required: true, minLength: 1));

            // Return the list of validation errors
            return validationErrors;
        }
    }
}