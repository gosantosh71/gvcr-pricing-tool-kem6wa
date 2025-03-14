using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Helpers;
using VatFilingPricingTool.Web.Utils;

namespace VatFilingPricingTool.Web.Validators
{
    /// <summary>
    /// Provides validation logic for user profile data to ensure data integrity
    /// before submission to the backend API.
    /// </summary>
    public class UserProfileValidator
    {
        /// <summary>
        /// Validates that the email address is in a valid format and within the maximum length.
        /// </summary>
        /// <param name="email">Email address to validate</param>
        /// <returns>ValidationResult indicating success or failure with error message</returns>
        public static ValidationResult ValidateEmail(string email)
        {
            // Check if the email is required
            var requiredResult = ValidationHelper.ValidateRequired(email, "Email");
            if (requiredResult != ValidationResult.Success)
            {
                return requiredResult;
            }

            // Check maximum length
            var maxLengthResult = ValidationHelper.ValidateMaximumLength(email, ValidationConstants.MaxEmailLength, "Email");
            if (maxLengthResult != ValidationResult.Success)
            {
                return maxLengthResult;
            }

            // Check email format
            return ValidationHelper.ValidateEmail(email, "Email");
        }

        /// <summary>
        /// Validates that the first name is provided and within length constraints.
        /// </summary>
        /// <param name="firstName">First name to validate</param>
        /// <returns>ValidationResult indicating success or failure with error message</returns>
        public static ValidationResult ValidateFirstName(string firstName)
        {
            // Check if the first name is required
            var requiredResult = ValidationHelper.ValidateRequired(firstName, "First name");
            if (requiredResult != ValidationResult.Success)
            {
                return requiredResult;
            }

            // Check minimum length
            var minLengthResult = ValidationHelper.ValidateMinimumLength(firstName, ValidationConstants.MinUsernameLength, "First name");
            if (minLengthResult != ValidationResult.Success)
            {
                return minLengthResult;
            }

            // Check maximum length
            return ValidationHelper.ValidateMaximumLength(firstName, ValidationConstants.MaxUsernameLength, "First name");
        }

        /// <summary>
        /// Validates that the last name is provided and within length constraints.
        /// </summary>
        /// <param name="lastName">Last name to validate</param>
        /// <returns>ValidationResult indicating success or failure with error message</returns>
        public static ValidationResult ValidateLastName(string lastName)
        {
            // Check if the last name is required
            var requiredResult = ValidationHelper.ValidateRequired(lastName, "Last name");
            if (requiredResult != ValidationResult.Success)
            {
                return requiredResult;
            }

            // Check minimum length
            var minLengthResult = ValidationHelper.ValidateMinimumLength(lastName, ValidationConstants.MinUsernameLength, "Last name");
            if (minLengthResult != ValidationResult.Success)
            {
                return minLengthResult;
            }

            // Check maximum length
            return ValidationHelper.ValidateMaximumLength(lastName, ValidationConstants.MaxUsernameLength, "Last name");
        }

        /// <summary>
        /// Validates that the phone number is in a valid format if provided.
        /// </summary>
        /// <param name="phoneNumber">Phone number to validate</param>
        /// <returns>ValidationResult indicating success or failure with error message</returns>
        public static ValidationResult ValidatePhoneNumber(string phoneNumber)
        {
            // Phone number is optional, so if it's not provided, it's valid
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return ValidationResult.Success;
            }

            // Check phone number format
            return ValidationHelper.ValidatePhoneNumber(phoneNumber, "Phone number");
        }

        /// <summary>
        /// Validates that the company name is within length constraints if provided.
        /// </summary>
        /// <param name="companyName">Company name to validate</param>
        /// <returns>ValidationResult indicating success or failure with error message</returns>
        public static ValidationResult ValidateCompanyName(string companyName)
        {
            // Company name is optional, so if it's not provided, it's valid
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return ValidationResult.Success;
            }

            // Check maximum length (using 100 as a reasonable maximum based on the model)
            return ValidationHelper.ValidateMaximumLength(companyName, 100, "Company name");
        }

        /// <summary>
        /// Validates the entire user profile model.
        /// </summary>
        /// <param name="model">The user profile model to validate</param>
        /// <returns>Dictionary of field names and their validation errors</returns>
        public static Dictionary<string, List<string>> ValidateUserProfile(UserProfileModel model)
        {
            var validationErrors = new Dictionary<string, List<string>>();

            // Validate Email
            var emailResult = ValidateEmail(model.Email);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.Email), emailResult);

            // Validate FirstName
            var firstNameResult = ValidateFirstName(model.FirstName);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.FirstName), firstNameResult);

            // Validate LastName
            var lastNameResult = ValidateLastName(model.LastName);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.LastName), lastNameResult);

            // Validate PhoneNumber
            var phoneNumberResult = ValidatePhoneNumber(model.PhoneNumber);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.PhoneNumber), phoneNumberResult);

            // Validate CompanyName
            var companyNameResult = ValidateCompanyName(model.CompanyName);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.CompanyName), companyNameResult);

            return validationErrors;
        }

        /// <summary>
        /// Validates the user profile update model.
        /// </summary>
        /// <param name="model">The user profile update model to validate</param>
        /// <returns>Dictionary of field names and their validation errors</returns>
        public static Dictionary<string, List<string>> ValidateUserProfileUpdate(UserProfileUpdateModel model)
        {
            var validationErrors = new Dictionary<string, List<string>>();

            // Validate FirstName
            var firstNameResult = ValidateFirstName(model.FirstName);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.FirstName), firstNameResult);

            // Validate LastName
            var lastNameResult = ValidateLastName(model.LastName);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.LastName), lastNameResult);

            // Validate PhoneNumber
            var phoneNumberResult = ValidatePhoneNumber(model.PhoneNumber);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.PhoneNumber), phoneNumberResult);

            // Validate CompanyName
            var companyNameResult = ValidateCompanyName(model.CompanyName);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.CompanyName), companyNameResult);

            return validationErrors;
        }

        /// <summary>
        /// Validates the user profile model for use with Blazor's EditForm component.
        /// </summary>
        /// <param name="editContext">The EditContext from the EditForm component</param>
        /// <returns>True if the model is valid, otherwise false</returns>
        public static bool ValidateModel(EditContext editContext)
        {
            var model = editContext.Model;
            Dictionary<string, List<string>> validationErrors = null;

            // Check the model type and validate accordingly
            if (model is UserProfileModel userProfileModel)
            {
                validationErrors = ValidateUserProfile(userProfileModel);
            }
            else if (model is UserProfileUpdateModel userProfileUpdateModel)
            {
                validationErrors = ValidateUserProfileUpdate(userProfileUpdateModel);
            }
            else
            {
                // If the model type is not supported, return false
                return false;
            }

            // Create a ValidationMessageStore for the EditContext
            var messageStore = new ValidationMessageStore(editContext);
            
            // Clear any existing validation messages
            messageStore.Clear();
            
            // Add any validation errors to the ValidationMessageStore
            ValidationHelper.UpdateValidationMessagesStore(messageStore, validationErrors, model);
            
            // Notify the EditContext that validation has completed
            editContext.NotifyValidationStateChanged();
            
            // Return true if there are no validation errors, otherwise false
            return validationErrors.Count == 0;
        }
    }
}