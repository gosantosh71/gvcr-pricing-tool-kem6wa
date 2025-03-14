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
    /// Validator class for ReportRequestModel that implements validation rules for report generation requests
    /// </summary>
    public class ReportValidator
    {
        /// <summary>
        /// Validates that the report title is provided and not too long
        /// </summary>
        /// <param name="reportTitle">The report title to validate</param>
        /// <returns>Validation result with error message if invalid</returns>
        public static ValidationResult ValidateReportTitle(string reportTitle)
        {
            // Validate that report title is required
            var requiredValidation = ValidationHelper.ValidateRequired(reportTitle, "Report Title");
            if (requiredValidation != ValidationResult.Success)
                return requiredValidation;

            // Validate maximum length
            const int maxTitleLength = 100;
            var lengthValidation = ValidationHelper.ValidateMaximumLength(reportTitle, maxTitleLength, "Report Title");
            if (lengthValidation != ValidationResult.Success)
                return lengthValidation;

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that the calculation ID is provided
        /// </summary>
        /// <param name="calculationId">The calculation ID to validate</param>
        /// <returns>Validation result with error message if invalid</returns>
        public static ValidationResult ValidateCalculationId(string calculationId)
        {
            // Validate that calculation ID is required
            return ValidationHelper.ValidateRequired(calculationId, "Calculation ID");
        }

        /// <summary>
        /// Validates that the report format is a valid enum value
        /// </summary>
        /// <param name="format">The format to validate</param>
        /// <returns>Validation result with error message if invalid</returns>
        public static ValidationResult ValidateReportFormat(int format)
        {
            // Validate that format is within valid range (0-3)
            return ValidationHelper.ValidateIntegerRange(format, 0, 3, "Report Format");
        }

        /// <summary>
        /// Validates that an email address is provided when email delivery is selected
        /// </summary>
        /// <param name="deliveryOptions">The delivery options to validate</param>
        /// <returns>Validation result with error message if invalid</returns>
        public static ValidationResult ValidateEmailDelivery(ReportDeliveryOptions deliveryOptions)
        {
            if (deliveryOptions == null)
            {
                return ValidationResult.Success; // No validation needed if null
            }

            // If email delivery is selected, validate the email address
            if (deliveryOptions.SendEmail)
            {
                // Validate that email is required
                var requiredValidation = ValidationHelper.ValidateRequired(deliveryOptions.EmailAddress, "Email Address");
                if (requiredValidation != ValidationResult.Success)
                    return requiredValidation;

                // Validate email format
                var emailValidation = ValidationHelper.ValidateEmail(deliveryOptions.EmailAddress, "Email Address");
                if (emailValidation != ValidationResult.Success)
                    return emailValidation;

                // Validate email length
                var lengthValidation = ValidationHelper.ValidateMaximumLength(
                    deliveryOptions.EmailAddress, 
                    Constants.ValidationConstants.MaxEmailLength, 
                    "Email Address");
                if (lengthValidation != ValidationResult.Success)
                    return lengthValidation;
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates the entire report request model
        /// </summary>
        /// <param name="model">The report request model to validate</param>
        /// <returns>Dictionary of field names and their validation errors</returns>
        public static Dictionary<string, List<string>> ValidateReportRequest(ReportRequestModel model)
        {
            var validationErrors = new Dictionary<string, List<string>>();

            // Validate individual properties
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.ReportTitle), 
                ValidateReportTitle(model.ReportTitle));
            
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.CalculationId), 
                ValidateCalculationId(model.CalculationId));
            
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.Format), 
                ValidateReportFormat(model.Format));
            
            ValidationHelper.AddValidationResult(validationErrors, "DeliveryOptions.EmailAddress", 
                ValidateEmailDelivery(model.DeliveryOptions));

            return validationErrors;
        }

        /// <summary>
        /// Validates the report request model for use with Blazor's EditForm component
        /// </summary>
        /// <param name="editContext">The EditContext to validate</param>
        /// <returns>True if the model is valid, otherwise false</returns>
        public static bool ValidateModel(EditContext editContext)
        {
            var model = (ReportRequestModel)editContext.Model;
            var validationErrors = ValidateReportRequest(model);
            
            // Create ValidationMessageStore for the EditContext
            var messageStore = new ValidationMessageStore(editContext);
            messageStore.Clear();
            
            // Add validation errors to the message store
            ValidationHelper.UpdateValidationMessagesStore(messageStore, validationErrors, model);
            
            // Notify the EditContext that validation has completed
            editContext.NotifyValidationStateChanged();
            
            // Return true if no validation errors, otherwise false
            return validationErrors.Count == 0;
        }
    }
}