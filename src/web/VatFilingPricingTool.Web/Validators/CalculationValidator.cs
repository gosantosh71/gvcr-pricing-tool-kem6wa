using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Helpers;
using VatFilingPricingTool.Web.Utils;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Web.Validators
{
    /// <summary>
    /// Validator class for CalculationInputModel that implements validation rules for VAT filing pricing calculations
    /// </summary>
    public class CalculationValidator
    {
        /// <summary>
        /// Validates that at least one country is selected for the calculation
        /// </summary>
        /// <param name="countryCodes">The list of country codes</param>
        /// <returns>Validation result with error message if invalid</returns>
        public static ValidationResult ValidateCountryCodes(List<string> countryCodes)
        {
            return ValidationHelper.ValidateCollection(countryCodes, "Country");
        }

        /// <summary>
        /// Validates that the service type is a valid enum value
        /// </summary>
        /// <param name="serviceType">The service type value</param>
        /// <returns>Validation result with error message if invalid</returns>
        public static ValidationResult ValidateServiceType(int serviceType)
        {
            // Since ServiceType is represented as an integer in the model
            // We validate that it's a non-negative value
            if (serviceType < 0)
            {
                return new ValidationResult("Please select a valid service type.");
            }
            return ValidationResult.Success;
        }

        /// <summary>
        /// Validates that the transaction volume is within the allowed range
        /// </summary>
        /// <param name="transactionVolume">The transaction volume value</param>
        /// <returns>Validation result with error message if invalid</returns>
        public static ValidationResult ValidateTransactionVolume(int transactionVolume)
        {
            return ValidationHelper.ValidateIntegerRange(
                transactionVolume, 
                ValidationConstants.MinTransactionVolume, 
                ValidationConstants.MaxTransactionVolume, 
                "Transaction Volume");
        }

        /// <summary>
        /// Validates that the filing frequency is a valid enum value
        /// </summary>
        /// <param name="filingFrequency">The filing frequency value</param>
        /// <returns>Validation result with error message if invalid</returns>
        public static ValidationResult ValidateFilingFrequency(int filingFrequency)
        {
            return ValidationHelper.ValidateEnum(filingFrequency, typeof(FilingFrequency), "Filing Frequency");
        }

        /// <summary>
        /// Validates the entire calculation input model
        /// </summary>
        /// <param name="model">The calculation input model to validate</param>
        /// <returns>Dictionary of field names and their validation errors</returns>
        public static Dictionary<string, List<string>> ValidateCalculationInput(CalculationInputModel model)
        {
            var validationErrors = new Dictionary<string, List<string>>();

            // Validate CountryCodes
            var countryCodesResult = ValidateCountryCodes(model.CountryCodes);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.CountryCodes), countryCodesResult);

            // Validate ServiceType
            var serviceTypeResult = ValidateServiceType(model.ServiceType);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.ServiceType), serviceTypeResult);

            // Validate TransactionVolume
            var transactionVolumeResult = ValidateTransactionVolume(model.TransactionVolume);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.TransactionVolume), transactionVolumeResult);

            // Validate FilingFrequency
            var filingFrequencyResult = ValidateFilingFrequency(model.FilingFrequency);
            ValidationHelper.AddValidationResult(validationErrors, nameof(model.FilingFrequency), filingFrequencyResult);

            // Additional services don't require specific validation as they are optional

            return validationErrors;
        }

        /// <summary>
        /// Validates the calculation input model for use with Blazor's EditForm component
        /// </summary>
        /// <param name="editContext">The EditContext for the form</param>
        /// <returns>True if the model is valid, otherwise false</returns>
        public static bool ValidateModel(EditContext editContext)
        {
            // Get the model from the EditContext
            var model = editContext.Model as CalculationInputModel;
            if (model == null)
                return false;

            // Validate the model
            var validationErrors = ValidateCalculationInput(model);

            // Create a ValidationMessageStore for the EditContext
            var messageStore = new ValidationMessageStore(editContext);
            
            // Clear any existing validation messages
            messageStore.Clear();

            // Add validation errors to the ValidationMessageStore
            ValidationHelper.UpdateValidationMessagesStore(messageStore, validationErrors, model);

            // Notify the EditContext that validation has completed
            editContext.NotifyValidationStateChanged();

            // Return true if there are no validation errors
            return validationErrors.Count == 0;
        }
    }
}