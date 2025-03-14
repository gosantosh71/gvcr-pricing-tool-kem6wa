using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Validation;
using VatFilingPricingTool.Common.Helpers;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Service.Helpers
{
    /// <summary>
    /// Provides specialized validation helper methods for service layer models in the VAT Filing Pricing Tool.
    /// This class extends the common validation functionality with service-specific validation logic,
    /// focusing on business rules relevant to the service layer.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates a transaction volume value against business rules.
        /// </summary>
        /// <param name="transactionVolume">The transaction volume to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateTransactionVolume(int transactionVolume, string fieldName)
        {
            var errors = new List<string>();

            if (transactionVolume < 1)
            {
                errors.Add(CommonValidationHelper.CreateMinValueError(fieldName, 1));
            }

            if (transactionVolume > 1000000)
            {
                errors.Add(CommonValidationHelper.CreateMaxValueError(fieldName, 1000000));
            }

            return errors;
        }

        /// <summary>
        /// Validates a filing frequency value against business rules.
        /// </summary>
        /// <param name="filingFrequency">The filing frequency to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateFilingFrequency(string filingFrequency, string fieldName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(filingFrequency))
            {
                errors.Add(CommonValidationHelper.CreateRequiredFieldError(fieldName));
                return errors;
            }

            if (!Enum.TryParse<FilingFrequency>(filingFrequency, true, out _))
            {
                errors.Add(CommonValidationHelper.CreateInvalidFormatError(fieldName, 
                    "valid filing frequency (Monthly, Quarterly, or Annually)"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a service type value against business rules.
        /// </summary>
        /// <param name="serviceType">The service type to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateServiceType(string serviceType, string fieldName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(serviceType))
            {
                errors.Add(CommonValidationHelper.CreateRequiredFieldError(fieldName));
                return errors;
            }

            if (!Enum.TryParse<ServiceType>(serviceType, true, out _))
            {
                errors.Add(CommonValidationHelper.CreateInvalidFormatError(fieldName, 
                    "valid service type (StandardFiling, ComplexFiling, or PriorityService)"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a VAT rate value against business rules.
        /// </summary>
        /// <param name="vatRate">The VAT rate to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateVatRate(decimal vatRate, string fieldName)
        {
            var errors = new List<string>();

            if (vatRate < 0)
            {
                errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, "VAT rate cannot be negative"));
            }

            if (vatRate > 30)
            {
                errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, "VAT rate cannot exceed 30%"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a Money value against business rules.
        /// </summary>
        /// <param name="money">The Money value to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="allowZero">Whether zero values are allowed.</param>
        /// <param name="allowNegative">Whether negative values are allowed.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateMoney(Money money, string fieldName, bool allowZero = true, bool allowNegative = false)
        {
            var errors = new List<string>();

            if (money == null)
            {
                errors.Add(CommonValidationHelper.CreateRequiredFieldError(fieldName));
                return errors;
            }

            if (!allowNegative && money.Amount < 0)
            {
                errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, "Value cannot be negative"));
            }

            if (!allowZero && money.Amount == 0)
            {
                errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, "Value cannot be zero"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a currency code against ISO 4217 format.
        /// </summary>
        /// <param name="currencyCode">The currency code to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateCurrencyCode(string currencyCode, string fieldName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                errors.Add(CommonValidationHelper.CreateRequiredFieldError(fieldName));
                return errors;
            }

            if (currencyCode.Length != 3)
            {
                errors.Add(CommonValidationHelper.CreateInvalidFormatError(fieldName, "3 characters"));
                return errors;
            }

            if (!Regex.IsMatch(currencyCode, "^[A-Z]{3}$"))
            {
                errors.Add(CommonValidationHelper.CreateInvalidFormatError(fieldName, "3 uppercase letters (ISO 4217 format)"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a discount percentage against business rules.
        /// </summary>
        /// <param name="discountPercentage">The discount percentage to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateDiscountPercentage(decimal discountPercentage, string fieldName)
        {
            var errors = new List<string>();

            if (discountPercentage < 0)
            {
                errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, "Discount percentage cannot be negative"));
            }

            if (discountPercentage > 100)
            {
                errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, "Discount percentage cannot exceed 100%"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a discount amount against a total amount.
        /// </summary>
        /// <param name="discountAmount">The discount amount to validate.</param>
        /// <param name="totalAmount">The total amount for comparison.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateDiscountAmount(decimal discountAmount, decimal totalAmount, string fieldName)
        {
            var errors = new List<string>();

            if (discountAmount < 0)
            {
                errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, "Discount amount cannot be negative"));
            }

            if (discountAmount > totalAmount)
            {
                errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, "Discount amount cannot exceed the total amount"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a rule expression syntax.
        /// </summary>
        /// <param name="expression">The rule expression to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateRuleExpression(string expression, string fieldName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(expression))
            {
                errors.Add(CommonValidationHelper.CreateRequiredFieldError(fieldName));
                return errors;
            }

            // Basic syntax validation
            try
            {
                // Check for balanced parentheses
                int openParenCount = 0;
                foreach (char c in expression)
                {
                    if (c == '(')
                        openParenCount++;
                    else if (c == ')')
                        openParenCount--;

                    if (openParenCount < 0)
                    {
                        errors.Add(CommonValidationHelper.CreateInvalidFormatError(fieldName, "Expression has unbalanced parentheses"));
                        break;
                    }
                }

                if (openParenCount != 0)
                {
                    errors.Add(CommonValidationHelper.CreateInvalidFormatError(fieldName, "Expression has unbalanced parentheses"));
                }

                // Check for invalid operators
                string[] validOperators = { "+", "-", "*", "/", "%", "==", "!=", "<", ">", "<=", ">=" };
                // This is a simplified check - a proper implementation would use a parser
                if (!validOperators.Any(op => expression.Contains(op)))
                {
                    errors.Add(CommonValidationHelper.CreateInvalidFormatError(fieldName, "Expression must contain valid operators"));
                }

                // Check for potentially dangerous expressions
                string[] dangerousPatterns = { "System.", "Process.", "File.", "Directory.", "Environment.", "Reflection", "Assembly", "Invoke" };
                if (dangerousPatterns.Any(pattern => expression.Contains(pattern)))
                {
                    errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, "Expression contains potentially unsafe code"));
                }
            }
            catch
            {
                errors.Add(CommonValidationHelper.CreateInvalidFormatError(fieldName, "Expression has invalid syntax"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a rule condition (parameter, operator, value).
        /// </summary>
        /// <param name="parameter">The rule parameter.</param>
        /// <param name="operator">The rule operator.</param>
        /// <param name="value">The rule value.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateRuleCondition(string parameter, string @operator, string value, string fieldName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(parameter))
            {
                errors.Add(CommonValidationHelper.CreateRequiredFieldError($"{fieldName} Parameter"));
            }

            if (string.IsNullOrWhiteSpace(@operator))
            {
                errors.Add(CommonValidationHelper.CreateRequiredFieldError($"{fieldName} Operator"));
            }
            else
            {
                string[] validOperators = { "equals", "notEquals", "greaterThan", "lessThan", "contains", "startsWith", "endsWith" };
                if (!validOperators.Contains(@operator.ToLower()))
                {
                    errors.Add(CommonValidationHelper.CreateInvalidValueError($"{fieldName} Operator", 
                        $"Operator must be one of: {string.Join(", ", validOperators)}"));
                }
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(CommonValidationHelper.CreateRequiredFieldError($"{fieldName} Value"));
            }

            return errors;
        }

        /// <summary>
        /// Validates calculation parameters for creating a new calculation.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="countryCodes">The list of country codes.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="transactionVolume">The transaction volume.</param>
        /// <param name="filingFrequency">The filing frequency.</param>
        /// <returns>Validation result with any validation errors.</returns>
        public static Result ValidateCalculationParameters(
            string userId,
            List<string> countryCodes,
            ServiceType serviceType,
            int transactionVolume,
            FilingFrequency filingFrequency)
        {
            var errors = new List<string>();

            // Validate user ID
            errors.AddRange(Validators.ValidateString(userId, "UserId"));

            // Validate country codes
            errors.AddRange(Validators.ValidateCollection(countryCodes, "Countries", true, 1));
            if (countryCodes != null && countryCodes.Any())
            {
                foreach (var countryCode in countryCodes)
                {
                    errors.AddRange(Validators.ValidateCountryCode(countryCode, "Country"));
                }
            }

            // Validate service type
            errors.AddRange(Validators.ValidateEnum(serviceType, "ServiceType"));

            // Validate transaction volume
            errors.AddRange(ValidateTransactionVolume(transactionVolume, "TransactionVolume"));

            // Validate filing frequency
            errors.AddRange(Validators.ValidateEnum(filingFrequency, "FilingFrequency"));

            return CommonValidationHelper.CreateValidationResult(errors);
        }

        /// <summary>
        /// Validates that multiple Money values have the same currency code.
        /// </summary>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="moneyValues">The money values to validate.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateMoneyValues(string fieldName, params Money[] moneyValues)
        {
            var errors = new List<string>();

            if (moneyValues == null || moneyValues.Length == 0)
            {
                return errors;
            }

            var firstCurrency = moneyValues[0]?.Currency;
            
            for (int i = 1; i < moneyValues.Length; i++)
            {
                if (moneyValues[i] != null && moneyValues[i].Currency != firstCurrency)
                {
                    errors.Add(CommonValidationHelper.CreateInvalidValueError(fieldName, 
                        $"All money values must have the same currency. Found {firstCurrency} and {moneyValues[i].Currency}"));
                    break;
                }
            }

            return errors;
        }
    }
}