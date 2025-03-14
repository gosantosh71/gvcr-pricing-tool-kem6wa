using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Helpers;

namespace VatFilingPricingTool.Common.Validation
{
    /// <summary>
    /// Static class providing core validation functions for various data types 
    /// used throughout the VAT Filing Pricing Tool application.
    /// </summary>
    public static class Validators
    {
        /// <summary>
        /// Validates a string against common string validation rules.
        /// </summary>
        /// <param name="value">The string value to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="minLength">The minimum allowed length.</param>
        /// <param name="maxLength">The maximum allowed length.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateString(string value, string fieldName, int minLength = 0, int maxLength = int.MaxValue, bool required = true)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                {
                    errors.Add(ValidationHelper.CreateRequiredFieldError(fieldName));
                }
                return errors;
            }

            if (value.Length < minLength)
            {
                errors.Add(ValidationHelper.CreateMinLengthError(fieldName, minLength));
            }

            if (value.Length > maxLength)
            {
                errors.Add(ValidationHelper.CreateMaxLengthError(fieldName, maxLength));
            }

            return errors;
        }

        /// <summary>
        /// Validates an email address format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateEmail(string email, string fieldName, bool required = true)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(email))
            {
                if (required)
                {
                    errors.Add(ValidationHelper.CreateRequiredFieldError(fieldName));
                }
                return errors;
            }

            // Email regex pattern based on RFC 5322 standard
            string emailPattern = @"^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$";
            
            if (!Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "valid email address"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a decimal value against minimum and maximum constraints.
        /// </summary>
        /// <param name="value">The decimal value to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="minValue">The minimum allowed value.</param>
        /// <param name="maxValue">The maximum allowed value.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateNumeric(decimal value, string fieldName, decimal minValue = decimal.MinValue, decimal maxValue = decimal.MaxValue)
        {
            var errors = new List<string>();

            if (value < minValue)
            {
                errors.Add(ValidationHelper.CreateMinValueError(fieldName, minValue));
            }

            if (value > maxValue)
            {
                errors.Add(ValidationHelper.CreateMaxValueError(fieldName, maxValue));
            }

            return errors;
        }

        /// <summary>
        /// Validates an integer value against minimum and maximum constraints.
        /// </summary>
        /// <param name="value">The integer value to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="minValue">The minimum allowed value.</param>
        /// <param name="maxValue">The maximum allowed value.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateInteger(int value, string fieldName, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            var errors = new List<string>();

            if (value < minValue)
            {
                errors.Add(ValidationHelper.CreateMinValueError(fieldName, minValue));
            }

            if (value > maxValue)
            {
                errors.Add(ValidationHelper.CreateMaxValueError(fieldName, maxValue));
            }

            return errors;
        }

        /// <summary>
        /// Validates a country code format (ISO 3166-1 alpha-2).
        /// </summary>
        /// <param name="countryCode">The country code to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateCountryCode(string countryCode, string fieldName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(countryCode))
            {
                errors.Add(ValidationHelper.CreateRequiredFieldError(fieldName));
                return errors;
            }

            if (countryCode.Length != 2)
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "ISO 3166-1 alpha-2 country code (2 uppercase letters)"));
                return errors;
            }

            if (!Regex.IsMatch(countryCode, "^[A-Z]{2}$"))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "ISO 3166-1 alpha-2 country code (2 uppercase letters)"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a collection against minimum and maximum count constraints.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The collection to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the collection is required.</param>
        /// <param name="minCount">The minimum allowed count.</param>
        /// <param name="maxCount">The maximum allowed count.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateCollection<T>(IEnumerable<T> collection, string fieldName, bool required = true, int minCount = 0, int maxCount = int.MaxValue)
        {
            var errors = new List<string>();

            if (collection == null)
            {
                if (required)
                {
                    errors.Add(ValidationHelper.CreateRequiredFieldError(fieldName));
                }
                return errors;
            }

            int count = collection.Count();

            if (count < minCount)
            {
                errors.Add(ValidationHelper.CreateMinCountError(fieldName, minCount));
            }

            if (count > maxCount)
            {
                errors.Add(ValidationHelper.CreateMaxCountError(fieldName, maxCount));
            }

            return errors;
        }

        /// <summary>
        /// Validates that a value is a defined value in an enum type.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateEnum<T>(T value, string fieldName) where T : struct, Enum
        {
            var errors = new List<string>();

            if (!Enum.IsDefined(typeof(T), value))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, $"a valid {typeof(T).Name} value"));
            }

            return errors;
        }

        /// <summary>
        /// Validates a date against minimum and maximum date constraints.
        /// </summary>
        /// <param name="date">The date to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="minDate">The minimum allowed date.</param>
        /// <param name="maxDate">The maximum allowed date.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateDate(DateTime date, string fieldName, DateTime? minDate = null, DateTime? maxDate = null)
        {
            var errors = new List<string>();

            if (minDate.HasValue && date < minDate.Value)
            {
                errors.Add(ValidationHelper.CreateMinValueError(fieldName, minDate.Value.ToString("yyyy-MM-dd")));
            }

            if (maxDate.HasValue && date > maxDate.Value)
            {
                errors.Add(ValidationHelper.CreateMaxValueError(fieldName, maxDate.Value.ToString("yyyy-MM-dd")));
            }

            return errors;
        }

        /// <summary>
        /// Validates that a string is a valid GUID format.
        /// </summary>
        /// <param name="value">The string to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateGuid(string value, string fieldName, bool required = true)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                {
                    errors.Add(ValidationHelper.CreateRequiredFieldError(fieldName));
                }
                return errors;
            }

            if (!Guid.TryParse(value, out _))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "valid GUID format"));
            }

            return errors;
        }

        /// <summary>
        /// Validates that a string is a valid phone number format.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidatePhoneNumber(string phoneNumber, string fieldName, bool required = true)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                if (required)
                {
                    errors.Add(ValidationHelper.CreateRequiredFieldError(fieldName));
                }
                return errors;
            }

            // International phone number format
            // Allows +, country code, and digits with optional spaces, parentheses, and hyphens
            string phonePattern = @"^\+?[0-9]{1,4}?[-.\s]?\(?\d{1,4}\)?[-.\s]?\d{1,4}[-.\s]?\d{1,9}$";
            
            if (!Regex.IsMatch(phoneNumber, phonePattern))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "valid phone number"));
            }

            return errors;
        }

        /// <summary>
        /// Validates that a password meets security requirements.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidatePassword(string password, string fieldName, bool required = true)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                if (required)
                {
                    errors.Add(ValidationHelper.CreateRequiredFieldError(fieldName));
                }
                return errors;
            }

            if (password.Length < 8)
            {
                errors.Add(ValidationHelper.CreateMinLengthError(fieldName, 8));
            }

            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "at least one uppercase letter"));
            }

            if (!Regex.IsMatch(password, "[a-z]"))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "at least one lowercase letter"));
            }

            if (!Regex.IsMatch(password, "[0-9]"))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "at least one digit"));
            }

            if (!Regex.IsMatch(password, "[^A-Za-z0-9]"))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "at least one special character"));
            }

            return errors;
        }

        /// <summary>
        /// Validates that a string is a valid URL format.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="required">Whether the field is required.</param>
        /// <returns>List of validation errors, empty if validation passes.</returns>
        public static List<string> ValidateUrl(string url, string fieldName, bool required = true)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(url))
            {
                if (required)
                {
                    errors.Add(ValidationHelper.CreateRequiredFieldError(fieldName));
                }
                return errors;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                errors.Add(ValidationHelper.CreateInvalidFormatError(fieldName, "valid URL format"));
            }

            return errors;
        }
    }
}