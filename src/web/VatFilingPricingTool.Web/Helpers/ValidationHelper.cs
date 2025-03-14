using System; // System 6.0.0 - Core .NET functionality
using System.Collections.Generic; // System.Collections.Generic 6.0.0 - For collection types like List and Dictionary
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations 6.0.0 - For validation attributes and validation context
using System.Text.RegularExpressions; // System.Text.RegularExpressions 6.0.0 - For regex-based validation
using Microsoft.AspNetCore.Components.Forms; // Microsoft.AspNetCore.Components.Forms 6.0.0 - For Blazor form validation integration
using VatFilingPricingTool.Web.Utils; // Internal imports for constants and extensions

namespace VatFilingPricingTool.Web.Helpers
{
    /// <summary>
    /// Static helper class providing common validation methods for form inputs
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates that a string value is not null or empty
        /// </summary>
        /// <param name="value">The string value to validate</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateRequired(string value, string fieldName)
        {
            if (Extensions.IsNullOrEmpty(value))
            {
                return new ValidationResult($"{fieldName} is required.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that a string value meets the minimum length requirement
        /// </summary>
        /// <param name="value">The string value to validate</param>
        /// <param name="minLength">The minimum required length</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateMinimumLength(string value, int minLength, string fieldName)
        {
            if (Extensions.IsNullOrEmpty(value))
            {
                return ValidationResult.Success; // This is handled by required validation
            }

            if (value.Length < minLength)
            {
                return new ValidationResult($"{fieldName} must be at least {minLength} characters long.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that a string value does not exceed the maximum length
        /// </summary>
        /// <param name="value">The string value to validate</param>
        /// <param name="maxLength">The maximum allowed length</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateMaximumLength(string value, int maxLength, string fieldName)
        {
            if (Extensions.IsNullOrEmpty(value))
            {
                return ValidationResult.Success;
            }

            if (value.Length > maxLength)
            {
                return new ValidationResult($"{fieldName} must not exceed {maxLength} characters.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that a string is a valid email address format
        /// </summary>
        /// <param name="email">The email address to validate</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateEmail(string email, string fieldName)
        {
            if (Extensions.IsNullOrEmpty(email))
            {
                return ValidationResult.Success; // This is handled by required validation
            }

            if (!Extensions.IsValidEmail(email))
            {
                return new ValidationResult($"Please enter a valid {fieldName} address.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that a password meets complexity requirements
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidatePassword(string password, string fieldName)
        {
            if (Extensions.IsNullOrEmpty(password))
            {
                return ValidationResult.Success; // This is handled by required validation
            }

            if (password.Length < Constants.ValidationConstants.MinPasswordLength)
            {
                return new ValidationResult($"{fieldName} must be at least {Constants.ValidationConstants.MinPasswordLength} characters long.");
            }

            if (password.Length > Constants.ValidationConstants.MaxPasswordLength)
            {
                return new ValidationResult($"{fieldName} must not exceed {Constants.ValidationConstants.MaxPasswordLength} characters.");
            }

            if (!Regex.IsMatch(password, Constants.ValidationConstants.PasswordRegexPattern))
            {
                return new ValidationResult($"{fieldName} must include at least one uppercase letter, one lowercase letter, one digit, and one special character.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that an integer value is within a specified range
        /// </summary>
        /// <param name="value">The integer value to validate</param>
        /// <param name="minValue">The minimum allowed value</param>
        /// <param name="maxValue">The maximum allowed value</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateIntegerRange(int value, int minValue, int maxValue, string fieldName)
        {
            if (value < minValue)
            {
                return new ValidationResult($"{fieldName} must be at least {minValue}.");
            }

            if (value > maxValue)
            {
                return new ValidationResult($"{fieldName} must not exceed {maxValue}.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that a decimal value is within a specified range
        /// </summary>
        /// <param name="value">The decimal value to validate</param>
        /// <param name="minValue">The minimum allowed value</param>
        /// <param name="maxValue">The maximum allowed value</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateDecimalRange(decimal value, decimal minValue, decimal maxValue, string fieldName)
        {
            if (value < minValue)
            {
                return new ValidationResult($"{fieldName} must be at least {minValue}.");
            }

            if (value > maxValue)
            {
                return new ValidationResult($"{fieldName} must not exceed {maxValue}.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that a collection contains at least one item
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="collection">The collection to validate</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateCollection<T>(IEnumerable<T> collection, string fieldName)
        {
            if (Extensions.IsNullOrEmpty(collection))
            {
                return new ValidationResult($"At least one {fieldName} must be selected.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that an integer value is a valid enum value
        /// </summary>
        /// <param name="value">The integer value to validate</param>
        /// <param name="enumType">The type of the enum</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateEnum(int value, Type enumType, string fieldName)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException($"{nameof(enumType)} must be an enum type.");
            }

            if (!Enum.IsDefined(enumType, value))
            {
                return new ValidationResult($"The selected {fieldName} is not valid.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that a string is a valid phone number format
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidatePhoneNumber(string phoneNumber, string fieldName)
        {
            if (Extensions.IsNullOrEmpty(phoneNumber))
            {
                return ValidationResult.Success; // This is handled by required validation
            }

            // Basic validation for phone numbers allowing digits, spaces, dashes, parentheses, and plus sign
            var phonePattern = @"^[\d\s\-\(\)\+]+$";
            if (!Regex.IsMatch(phoneNumber, phonePattern))
            {
                return new ValidationResult($"Please enter a valid {fieldName}.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that a date is within a valid range
        /// </summary>
        /// <param name="date">The date to validate</param>
        /// <param name="minDate">The earliest allowed date (optional)</param>
        /// <param name="maxDate">The latest allowed date (optional)</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateDate(DateTime date, DateTime? minDate, DateTime? maxDate, string fieldName)
        {
            if (minDate.HasValue && date < minDate.Value)
            {
                return new ValidationResult($"{fieldName} must not be earlier than {minDate.Value.ToShortDateString()}.");
            }

            if (maxDate.HasValue && date > maxDate.Value)
            {
                return new ValidationResult($"{fieldName} must not be later than {maxDate.Value.ToShortDateString()}.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that a string is a valid URL format
        /// </summary>
        /// <param name="url">The URL to validate</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateUrl(string url, string fieldName)
        {
            if (Extensions.IsNullOrEmpty(url))
            {
                return ValidationResult.Success; // This is handled by required validation
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"Please enter a valid {fieldName} (e.g., https://example.com).");
        }

        /// <summary>
        /// Validates that a string is a valid ISO country code
        /// </summary>
        /// <param name="countryCode">The country code to validate</param>
        /// <param name="fieldName">The name of the field being validated</param>
        /// <returns>ValidationResult.Success if valid, otherwise ValidationResult with error message</returns>
        public static ValidationResult ValidateCountryCode(string countryCode, string fieldName)
        {
            if (Extensions.IsNullOrEmpty(countryCode))
            {
                return ValidationResult.Success; // This is handled by required validation
            }

            // ISO 3166-1 alpha-2 country codes are 2 uppercase letters
            if (countryCode.Length != 2 || !Regex.IsMatch(countryCode, "^[A-Z]{2}$"))
            {
                return new ValidationResult($"Please enter a valid {fieldName} (2-letter ISO country code).");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Adds a validation error to a dictionary of validation errors
        /// </summary>
        /// <param name="validationErrors">The dictionary of validation errors</param>
        /// <param name="key">The field key for the error</param>
        /// <param name="errorMessage">The error message</param>
        public static void AddValidationError(Dictionary<string, List<string>> validationErrors, string key, string errorMessage)
        {
            if (validationErrors.ContainsKey(key))
            {
                validationErrors[key].Add(errorMessage);
            }
            else
            {
                validationErrors[key] = new List<string> { errorMessage };
            }
        }

        /// <summary>
        /// Adds a validation result to a dictionary of validation errors if the result is not successful
        /// </summary>
        /// <param name="validationErrors">The dictionary of validation errors</param>
        /// <param name="key">The field key for the error</param>
        /// <param name="validationResult">The validation result to add</param>
        public static void AddValidationResult(Dictionary<string, List<string>> validationErrors, string key, ValidationResult validationResult)
        {
            if (validationResult != ValidationResult.Success)
            {
                AddValidationError(validationErrors, key, validationResult.ErrorMessage);
            }
        }

        /// <summary>
        /// Updates a ValidationMessageStore with validation errors from a dictionary
        /// </summary>
        /// <param name="messageStore">The ValidationMessageStore to update</param>
        /// <param name="validationErrors">The dictionary of validation errors</param>
        /// <param name="model">The model being validated</param>
        public static void UpdateValidationMessagesStore(ValidationMessageStore messageStore, Dictionary<string, List<string>> validationErrors, object model)
        {
            messageStore.Clear();
            foreach (var error in validationErrors)
            {
                foreach (var message in error.Value)
                {
                    messageStore.Add(error.Key, message);
                }
            }
        }
    }
}