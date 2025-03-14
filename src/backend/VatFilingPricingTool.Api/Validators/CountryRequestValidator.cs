using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.Linq; // System.Linq v6.0.0
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Common.Validation;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Validators
{
    /// <summary>
    /// Validates country-related request models to ensure they contain valid data before processing
    /// </summary>
    public class CountryRequestValidator
    {
        /// <summary>
        /// Validates a CreateCountryRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateCreateCountryRequest(CreateCountryRequest request)
        {
            var errors = new List<string>();
            
            // Validate CountryCode
            var countryCodeErrors = Validators.ValidateCountryCode(request.CountryCode, nameof(request.CountryCode));
            errors.AddRange(countryCodeErrors);
            
            // Validate Name
            var nameErrors = Validators.ValidateString(request.Name, nameof(request.Name), minLength: 2, maxLength: 100, required: true);
            errors.AddRange(nameErrors);
            
            // Validate StandardVatRate
            var vatRateErrors = Validators.ValidateNumeric(request.StandardVatRate, nameof(request.StandardVatRate), minValue: 0, maxValue: 100);
            errors.AddRange(vatRateErrors);
            
            // Validate CurrencyCode
            var currencyCodeErrors = Validators.ValidateString(request.CurrencyCode, nameof(request.CurrencyCode), minLength: 3, maxLength: 3, required: true);
            errors.AddRange(currencyCodeErrors);
            
            // Validate AvailableFilingFrequencies
            var frequenciesErrors = Validators.ValidateCollection(request.AvailableFilingFrequencies, nameof(request.AvailableFilingFrequencies), required: true, minCount: 1);
            errors.AddRange(frequenciesErrors);
            
            // Validate each filing frequency
            if (request.AvailableFilingFrequencies != null)
            {
                foreach (var frequency in request.AvailableFilingFrequencies)
                {
                    var frequencyErrors = Validators.ValidateEnum<FilingFrequency>(frequency, nameof(frequency));
                    errors.AddRange(frequencyErrors);
                }
            }
            
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }
        
        /// <summary>
        /// Validates an UpdateCountryRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateUpdateCountryRequest(UpdateCountryRequest request)
        {
            var errors = new List<string>();
            
            // Validate CountryCode
            var countryCodeErrors = Validators.ValidateCountryCode(request.CountryCode, nameof(request.CountryCode));
            errors.AddRange(countryCodeErrors);
            
            // Validate Name
            var nameErrors = Validators.ValidateString(request.Name, nameof(request.Name), minLength: 2, maxLength: 100, required: true);
            errors.AddRange(nameErrors);
            
            // Validate StandardVatRate
            var vatRateErrors = Validators.ValidateNumeric(request.StandardVatRate, nameof(request.StandardVatRate), minValue: 0, maxValue: 100);
            errors.AddRange(vatRateErrors);
            
            // Validate CurrencyCode
            var currencyCodeErrors = Validators.ValidateString(request.CurrencyCode, nameof(request.CurrencyCode), minLength: 3, maxLength: 3, required: true);
            errors.AddRange(currencyCodeErrors);
            
            // Validate AvailableFilingFrequencies
            var frequenciesErrors = Validators.ValidateCollection(request.AvailableFilingFrequencies, nameof(request.AvailableFilingFrequencies), required: true, minCount: 1);
            errors.AddRange(frequenciesErrors);
            
            // Validate each filing frequency
            if (request.AvailableFilingFrequencies != null)
            {
                foreach (var frequency in request.AvailableFilingFrequencies)
                {
                    var frequencyErrors = Validators.ValidateEnum<FilingFrequency>(frequency, nameof(frequency));
                    errors.AddRange(frequencyErrors);
                }
            }
            
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }
        
        /// <summary>
        /// Validates a GetCountryRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateGetCountryRequest(GetCountryRequest request)
        {
            var errors = new List<string>();
            
            // Validate CountryCode
            var countryCodeErrors = Validators.ValidateCountryCode(request.CountryCode, nameof(request.CountryCode));
            errors.AddRange(countryCodeErrors);
            
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }
        
        /// <summary>
        /// Validates a GetCountriesRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateGetCountriesRequest(GetCountriesRequest request)
        {
            var errors = new List<string>();
            
            // Validate Page
            var pageErrors = Validators.ValidateInteger(request.Page, nameof(request.Page), minValue: 1, maxValue: 10000);
            errors.AddRange(pageErrors);
            
            // Validate PageSize
            var pageSizeErrors = Validators.ValidateInteger(request.PageSize, nameof(request.PageSize), minValue: 1, maxValue: 100);
            errors.AddRange(pageSizeErrors);
            
            // Validate CountryCodes if any are provided
            if (request.CountryCodes != null && request.CountryCodes.Any())
            {
                foreach (var countryCode in request.CountryCodes)
                {
                    var countryCodeErrors = Validators.ValidateCountryCode(countryCode, nameof(request.CountryCodes));
                    errors.AddRange(countryCodeErrors);
                }
            }
            
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }
        
        /// <summary>
        /// Validates a DeleteCountryRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateDeleteCountryRequest(DeleteCountryRequest request)
        {
            var errors = new List<string>();
            
            // Validate CountryCode
            var countryCodeErrors = Validators.ValidateCountryCode(request.CountryCode, nameof(request.CountryCode));
            errors.AddRange(countryCodeErrors);
            
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }
    }
}