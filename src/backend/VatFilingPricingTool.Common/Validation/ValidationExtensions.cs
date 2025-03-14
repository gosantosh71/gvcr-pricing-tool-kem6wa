using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Common.Helpers;
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Common.Validation
{
    /// <summary>
    /// Provides extension methods for validation operations to enable a more fluent
    /// validation syntax throughout the VAT Filing Pricing Tool application.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Extension method to validate a string against common string validation rules.
        /// </summary>
        /// <param name="value">The string value to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="minLength">The minimum allowed length.</param>
        /// <param name="maxLength">The maximum allowed length.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> Validate(this string value, string fieldName, int minLength = 0, int maxLength = int.MaxValue, bool required = true)
        {
            return Validators.ValidateString(value, fieldName, minLength, maxLength, required);
        }

        /// <summary>
        /// Extension method to validate an email address format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateEmail(this string email, string fieldName, bool required = true)
        {
            return Validators.ValidateEmail(email, fieldName, required);
        }

        /// <summary>
        /// Extension method to validate a decimal value against minimum and maximum constraints.
        /// </summary>
        /// <param name="value">The decimal value to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="minValue">The minimum allowed value.</param>
        /// <param name="maxValue">The maximum allowed value.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateNumeric(this decimal value, string fieldName, decimal minValue = decimal.MinValue, decimal maxValue = decimal.MaxValue)
        {
            return Validators.ValidateNumeric(value, fieldName, minValue, maxValue);
        }

        /// <summary>
        /// Extension method to validate an integer value against minimum and maximum constraints.
        /// </summary>
        /// <param name="value">The integer value to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="minValue">The minimum allowed value.</param>
        /// <param name="maxValue">The maximum allowed value.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateInteger(this int value, string fieldName, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            return Validators.ValidateInteger(value, fieldName, minValue, maxValue);
        }

        /// <summary>
        /// Extension method to validate a country code format (ISO 3166-1 alpha-2).
        /// </summary>
        /// <param name="countryCode">The country code to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateCountryCode(this string countryCode, string fieldName)
        {
            return Validators.ValidateCountryCode(countryCode, fieldName);
        }

        /// <summary>
        /// Extension method to validate a collection against minimum and maximum count constraints.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The collection to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the collection is required.</param>
        /// <param name="minCount">The minimum allowed count.</param>
        /// <param name="maxCount">The maximum allowed count.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateCollection<T>(this IEnumerable<T> collection, string fieldName, bool required = true, int minCount = 0, int maxCount = int.MaxValue)
        {
            return Validators.ValidateCollection(collection, fieldName, required, minCount, maxCount);
        }

        /// <summary>
        /// Extension method to validate that a value is a defined value in an enum type.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateEnum<T>(this T value, string fieldName) where T : struct, Enum
        {
            return Validators.ValidateEnum(value, fieldName);
        }

        /// <summary>
        /// Extension method to validate a date against minimum and maximum date constraints.
        /// </summary>
        /// <param name="date">The date to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="minDate">The minimum allowed date.</param>
        /// <param name="maxDate">The maximum allowed date.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateDate(this DateTime date, string fieldName, DateTime? minDate = null, DateTime? maxDate = null)
        {
            return Validators.ValidateDate(date, fieldName, minDate, maxDate);
        }

        /// <summary>
        /// Extension method to validate that a password meets security requirements.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidatePassword(this string password, string fieldName, bool required = true)
        {
            return Validators.ValidatePassword(password, fieldName, required);
        }

        /// <summary>
        /// Extension method to validate that a string is a valid URL format.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateUrl(this string url, string fieldName, bool required = true)
        {
            return Validators.ValidateUrl(url, fieldName, required);
        }

        /// <summary>
        /// Extension method to convert a list of validation errors to a Result object.
        /// </summary>
        /// <param name="validationErrors">The list of validation errors.</param>
        /// <returns>Success result if no errors, validation failure result otherwise.</returns>
        public static Result ToValidationResult(this List<string> validationErrors)
        {
            return ValidationHelper.CreateValidationResult(validationErrors);
        }

        /// <summary>
        /// Extension method to convert a list of validation errors to a typed Result&lt;T&gt; object.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="validationErrors">The list of validation errors.</param>
        /// <param name="value">The value to include in the result if successful.</param>
        /// <returns>Success result with value if no errors, validation failure result otherwise.</returns>
        public static Result<T> ToValidationResult<T>(this List<string> validationErrors, T value)
        {
            return ValidationHelper.HasErrors(validationErrors) 
                ? Result<T>.ValidationFailure(validationErrors) 
                : Result<T>.Success(value);
        }

        /// <summary>
        /// Extension method to check if a list of validation errors is empty (validation passed).
        /// </summary>
        /// <param name="validationErrors">The list of validation errors.</param>
        /// <returns>True if validation passed (no errors), false otherwise.</returns>
        public static bool IsValid(this List<string> validationErrors)
        {
            return validationErrors == null || !validationErrors.Any();
        }
    }
}