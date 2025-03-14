using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Common.Validation;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Validators
{
    /// <summary>
    /// Validates calculation-related request models to ensure they contain valid data before processing
    /// </summary>
    public class CalculationRequestValidator
    {
        /// <summary>
        /// Default constructor for the CalculationRequestValidator
        /// </summary>
        public CalculationRequestValidator()
        {
        }

        /// <summary>
        /// Validates a CalculationRequest object against business rules
        /// </summary>
        /// <param name="request">The calculation request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateCalculationRequest(CalculationRequest request)
        {
            var errors = new List<string>();

            // Validate ServiceType
            errors.AddRange(Validators.ValidateEnum(request.ServiceType, nameof(request.ServiceType)));

            // Validate TransactionVolume
            errors.AddRange(Validators.ValidateInteger(
                request.TransactionVolume,
                nameof(request.TransactionVolume),
                minValue: 1,
                maxValue: 1000000));

            // Validate Frequency
            errors.AddRange(Validators.ValidateEnum(request.Frequency, nameof(request.Frequency)));

            // Validate CountryCodes collection
            errors.AddRange(Validators.ValidateCollection(
                request.CountryCodes,
                nameof(request.CountryCodes),
                required: true,
                minCount: 1,
                maxCount: 50));

            // Validate each country code
            if (request.CountryCodes != null)
            {
                for (int i = 0; i < request.CountryCodes.Count; i++)
                {
                    errors.AddRange(Validators.ValidateCountryCode(
                        request.CountryCodes[i],
                        $"{nameof(request.CountryCodes)}[{i}]"));
                }
            }

            // Validate AdditionalServices if provided
            if (request.AdditionalServices != null && request.AdditionalServices.Count > 0)
            {
                for (int i = 0; i < request.AdditionalServices.Count; i++)
                {
                    errors.AddRange(Validators.ValidateString(
                        request.AdditionalServices[i],
                        $"{nameof(request.AdditionalServices)}[{i}]",
                        minLength: 1,
                        maxLength: 100));
                }
            }

            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a GetCalculationRequest object against business rules
        /// </summary>
        /// <param name="request">The get calculation request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateGetCalculationRequest(GetCalculationRequest request)
        {
            var errors = new List<string>();

            // Validate CalculationId
            errors.AddRange(Validators.ValidateGuid(
                request.CalculationId,
                nameof(request.CalculationId),
                required: true));

            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a SaveCalculationRequest object against business rules
        /// </summary>
        /// <param name="request">The save calculation request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateSaveCalculationRequest(SaveCalculationRequest request)
        {
            var errors = new List<string>();

            // Validate ServiceType
            errors.AddRange(Validators.ValidateEnum(request.ServiceType, nameof(request.ServiceType)));

            // Validate TransactionVolume
            errors.AddRange(Validators.ValidateInteger(
                request.TransactionVolume,
                nameof(request.TransactionVolume),
                minValue: 1,
                maxValue: 1000000));

            // Validate Frequency
            errors.AddRange(Validators.ValidateEnum(request.Frequency, nameof(request.Frequency)));

            // Validate TotalCost
            errors.AddRange(Validators.ValidateNumeric(
                request.TotalCost,
                nameof(request.TotalCost),
                minValue: 0,
                maxValue: 1000000));

            // Validate CurrencyCode
            errors.AddRange(Validators.ValidateString(
                request.CurrencyCode,
                nameof(request.CurrencyCode),
                required: true,
                minLength: 3,
                maxLength: 3));

            // Validate CountryBreakdowns collection
            errors.AddRange(Validators.ValidateCollection(
                request.CountryBreakdowns,
                nameof(request.CountryBreakdowns),
                required: true,
                minCount: 1));

            // Validate each country breakdown
            if (request.CountryBreakdowns != null)
            {
                for (int i = 0; i < request.CountryBreakdowns.Count; i++)
                {
                    var breakdown = request.CountryBreakdowns[i];
                    string prefix = $"{nameof(request.CountryBreakdowns)}[{i}]";

                    // Validate CountryCode
                    errors.AddRange(Validators.ValidateCountryCode(
                        breakdown.CountryCode,
                        $"{prefix}.{nameof(breakdown.CountryCode)}"));

                    // Validate CountryName
                    errors.AddRange(Validators.ValidateString(
                        breakdown.CountryName,
                        $"{prefix}.{nameof(breakdown.CountryName)}",
                        minLength: 1,
                        maxLength: 100));

                    // Validate BaseCost
                    errors.AddRange(Validators.ValidateNumeric(
                        breakdown.BaseCost,
                        $"{prefix}.{nameof(breakdown.BaseCost)}",
                        minValue: 0));

                    // Validate AdditionalCost
                    errors.AddRange(Validators.ValidateNumeric(
                        breakdown.AdditionalCost,
                        $"{prefix}.{nameof(breakdown.AdditionalCost)}",
                        minValue: 0));

                    // Validate TotalCost
                    errors.AddRange(Validators.ValidateNumeric(
                        breakdown.TotalCost,
                        $"{prefix}.{nameof(breakdown.TotalCost)}",
                        minValue: 0));
                }
            }

            // If there are any validation errors, return validation failure
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a GetCalculationHistoryRequest object against business rules
        /// </summary>
        /// <param name="request">The calculation history request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateGetCalculationHistoryRequest(GetCalculationHistoryRequest request)
        {
            var errors = new List<string>();

            // Validate StartDate if provided
            if (request.StartDate.HasValue)
            {
                errors.AddRange(Validators.ValidateDate(
                    request.StartDate.Value,
                    nameof(request.StartDate)));
            }

            // Validate EndDate if provided
            if (request.EndDate.HasValue)
            {
                errors.AddRange(Validators.ValidateDate(
                    request.EndDate.Value,
                    nameof(request.EndDate)));
            }

            // Validate date range if both dates are provided
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            {
                errors.Add($"{nameof(request.StartDate)}: Start date must be before end date");
            }

            // Validate CountryCodes if provided
            if (request.CountryCodes != null && request.CountryCodes.Count > 0)
            {
                for (int i = 0; i < request.CountryCodes.Count; i++)
                {
                    errors.AddRange(Validators.ValidateCountryCode(
                        request.CountryCodes[i],
                        $"{nameof(request.CountryCodes)}[{i}]"));
                }
            }

            // Validate ServiceType if provided
            if (request.ServiceType.HasValue)
            {
                errors.AddRange(Validators.ValidateEnum(
                    request.ServiceType.Value,
                    nameof(request.ServiceType)));
            }

            // Validate Page
            errors.AddRange(Validators.ValidateInteger(
                request.Page,
                nameof(request.Page),
                minValue: 1,
                maxValue: 10000));

            // Validate PageSize
            errors.AddRange(Validators.ValidateInteger(
                request.PageSize,
                nameof(request.PageSize),
                minValue: 1,
                maxValue: 100));

            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a CompareCalculationsRequest object against business rules
        /// </summary>
        /// <param name="request">The compare calculations request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateCompareCalculationsRequest(CompareCalculationsRequest request)
        {
            var errors = new List<string>();

            // Validate Scenarios collection
            errors.AddRange(Validators.ValidateCollection(
                request.Scenarios,
                nameof(request.Scenarios),
                required: true,
                minCount: 2,
                maxCount: 5));

            // Validate each scenario
            if (request.Scenarios != null)
            {
                for (int i = 0; i < request.Scenarios.Count; i++)
                {
                    var scenario = request.Scenarios[i];
                    string prefix = $"{nameof(request.Scenarios)}[{i}]";

                    // Validate ScenarioId
                    errors.AddRange(Validators.ValidateString(
                        scenario.ScenarioId,
                        $"{prefix}.{nameof(scenario.ScenarioId)}",
                        required: true));

                    // Validate ScenarioName
                    errors.AddRange(Validators.ValidateString(
                        scenario.ScenarioName,
                        $"{prefix}.{nameof(scenario.ScenarioName)}",
                        required: true,
                        minLength: 1,
                        maxLength: 100));

                    // Validate ServiceType
                    errors.AddRange(Validators.ValidateEnum(
                        scenario.ServiceType,
                        $"{prefix}.{nameof(scenario.ServiceType)}"));

                    // Validate TransactionVolume
                    errors.AddRange(Validators.ValidateInteger(
                        scenario.TransactionVolume,
                        $"{prefix}.{nameof(scenario.TransactionVolume)}",
                        minValue: 1,
                        maxValue: 1000000));

                    // Validate Frequency
                    errors.AddRange(Validators.ValidateEnum(
                        scenario.Frequency,
                        $"{prefix}.{nameof(scenario.Frequency)}"));

                    // Validate CountryCodes collection
                    errors.AddRange(Validators.ValidateCollection(
                        scenario.CountryCodes,
                        $"{prefix}.{nameof(scenario.CountryCodes)}",
                        required: true,
                        minCount: 1,
                        maxCount: 50));

                    // Validate each country code
                    if (scenario.CountryCodes != null)
                    {
                        for (int j = 0; j < scenario.CountryCodes.Count; j++)
                        {
                            errors.AddRange(Validators.ValidateCountryCode(
                                scenario.CountryCodes[j],
                                $"{prefix}.{nameof(scenario.CountryCodes)}[{j}]"));
                        }
                    }

                    // Validate AdditionalServices if provided
                    if (scenario.AdditionalServices != null && scenario.AdditionalServices.Count > 0)
                    {
                        for (int j = 0; j < scenario.AdditionalServices.Count; j++)
                        {
                            errors.AddRange(Validators.ValidateString(
                                scenario.AdditionalServices[j],
                                $"{prefix}.{nameof(scenario.AdditionalServices)}[{j}]",
                                minLength: 1,
                                maxLength: 100));
                        }
                    }
                }
            }

            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a DeleteCalculationRequest object against business rules
        /// </summary>
        /// <param name="request">The delete calculation request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateDeleteCalculationRequest(DeleteCalculationRequest request)
        {
            var errors = new List<string>();

            // Validate CalculationId
            errors.AddRange(Validators.ValidateGuid(
                request.CalculationId,
                nameof(request.CalculationId),
                required: true));

            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }

            return Result.Success();
        }
    }
}