using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Common.Helpers
{
    /// <summary>
    /// Provides helper methods for validation operations to ensure consistent
    /// error handling and validation processes across the application.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Creates a formatted validation error message for a field.
        /// </summary>
        /// <param name="fieldName">The name of the field with the error.</param>
        /// <param name="errorMessage">The error message describing the validation failure.</param>
        /// <returns>A formatted error message for the field.</returns>
        public static string CreateValidationError(string fieldName, string errorMessage)
        {
            return $"{fieldName}: {errorMessage}";
        }

        /// <summary>
        /// Creates a validation error message for a required field.
        /// </summary>
        /// <param name="fieldName">The name of the required field.</param>
        /// <returns>A validation error message indicating the field is required.</returns>
        public static string CreateRequiredFieldError(string fieldName)
        {
            return CreateValidationError(fieldName, "Field is required");
        }

        /// <summary>
        /// Creates a validation error message for minimum length requirement.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="minLength">The minimum required length.</param>
        /// <returns>A validation error message indicating the minimum length requirement.</returns>
        public static string CreateMinLengthError(string fieldName, int minLength)
        {
            return CreateValidationError(fieldName, $"Must be at least {minLength} characters long");
        }

        /// <summary>
        /// Creates a validation error message for maximum length requirement.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="maxLength">The maximum allowed length.</param>
        /// <returns>A validation error message indicating the maximum length requirement.</returns>
        public static string CreateMaxLengthError(string fieldName, int maxLength)
        {
            return CreateValidationError(fieldName, $"Must not exceed {maxLength} characters");
        }

        /// <summary>
        /// Creates a validation error message for minimum value requirement.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="minValue">The minimum allowed value.</param>
        /// <returns>A validation error message indicating the minimum value requirement.</returns>
        public static string CreateMinValueError(string fieldName, object minValue)
        {
            return CreateValidationError(fieldName, $"Must be greater than or equal to {minValue}");
        }

        /// <summary>
        /// Creates a validation error message for maximum value requirement.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="maxValue">The maximum allowed value.</param>
        /// <returns>A validation error message indicating the maximum value requirement.</returns>
        public static string CreateMaxValueError(string fieldName, object maxValue)
        {
            return CreateValidationError(fieldName, $"Must be less than or equal to {maxValue}");
        }

        /// <summary>
        /// Creates a validation error message for invalid format.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="expectedFormat">The expected format description.</param>
        /// <returns>A validation error message indicating the expected format.</returns>
        public static string CreateInvalidFormatError(string fieldName, string expectedFormat)
        {
            return CreateValidationError(fieldName, $"Invalid format. Expected: {expectedFormat}");
        }

        /// <summary>
        /// Creates a validation error message for an invalid value.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="reason">The reason why the value is invalid.</param>
        /// <returns>A validation error message indicating why the value is invalid.</returns>
        public static string CreateInvalidValueError(string fieldName, string reason)
        {
            return CreateValidationError(fieldName, $"Invalid value: {reason}");
        }

        /// <summary>
        /// Creates a validation error message for minimum collection count.
        /// </summary>
        /// <param name="fieldName">The name of the collection field.</param>
        /// <param name="minCount">The minimum required count.</param>
        /// <returns>A validation error message indicating the minimum count requirement.</returns>
        public static string CreateMinCountError(string fieldName, int minCount)
        {
            return CreateValidationError(fieldName, $"Must contain at least {minCount} item{(minCount != 1 ? "s" : "")}");
        }

        /// <summary>
        /// Creates a validation error message for maximum collection count.
        /// </summary>
        /// <param name="fieldName">The name of the collection field.</param>
        /// <param name="maxCount">The maximum allowed count.</param>
        /// <returns>A validation error message indicating the maximum count requirement.</returns>
        public static string CreateMaxCountError(string fieldName, int maxCount)
        {
            return CreateValidationError(fieldName, $"Must not contain more than {maxCount} item{(maxCount != 1 ? "s" : "")}");
        }

        /// <summary>
        /// Creates a validation result based on a list of validation errors.
        /// </summary>
        /// <param name="validationErrors">The list of validation errors.</param>
        /// <returns>A success result if no errors, or a validation failure result with the errors.</returns>
        public static Result CreateValidationResult(List<string> validationErrors)
        {
            return HasErrors(validationErrors) 
                ? Result.ValidationFailure(validationErrors) 
                : Result.Success();
        }

        /// <summary>
        /// Creates a typed validation result based on a list of validation errors.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="validationErrors">The list of validation errors.</param>
        /// <param name="value">The value to include in the result if successful.</param>
        /// <returns>A success result with the value if no errors, or a validation failure result with the errors.</returns>
        public static Result<T> CreateValidationResult<T>(List<string> validationErrors, T value)
        {
            return HasErrors(validationErrors) 
                ? Result<T>.ValidationFailure(validationErrors) 
                : Result<T>.Success(value);
        }

        /// <summary>
        /// Combines multiple lists of validation errors into a single list.
        /// </summary>
        /// <param name="errorLists">The error lists to combine.</param>
        /// <returns>A combined list of all validation errors.</returns>
        public static List<string> CombineValidationErrors(params List<string>[] errorLists)
        {
            var combinedErrors = new List<string>();
            
            foreach (var errorList in errorLists)
            {
                if (errorList != null && errorList.Any())
                {
                    combinedErrors.AddRange(errorList);
                }
            }
            
            return combinedErrors;
        }

        /// <summary>
        /// Checks if a list of validation errors contains any errors.
        /// </summary>
        /// <param name="validationErrors">The list of validation errors to check.</param>
        /// <returns>True if there are validation errors, false otherwise.</returns>
        public static bool HasErrors(List<string> validationErrors)
        {
            return validationErrors != null && validationErrors.Any();
        }
    }
}