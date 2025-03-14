using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Common.Validation;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Validators
{
    /// <summary>
    /// Validates rule-related request models to ensure they contain valid data
    /// before processing
    /// </summary>
    public class RuleRequestValidator
    {
        /// <summary>
        /// Validates a CreateRuleRequest object against business rules
        /// </summary>
        /// <param name="request">The create rule request to validate</param>
        /// <returns>Result indicating success or validation failure with error details</returns>
        public Result ValidateCreateRuleRequest(CreateRuleRequest request)
        {
            var errors = new List<string>();
            
            // Validate CountryCode
            errors.AddRange(Validators.ValidateCountryCode(request.CountryCode, nameof(request.CountryCode)));
            
            // Validate RuleType
            errors.AddRange(Validators.ValidateEnum(request.RuleType, nameof(request.RuleType)));
            
            // Validate Name
            errors.AddRange(Validators.ValidateString(request.Name, nameof(request.Name), minLength: 3, maxLength: 100));
            
            // Validate Description
            errors.AddRange(Validators.ValidateString(request.Description, nameof(request.Description), maxLength: 500, required: false));
            
            // Validate Expression
            errors.AddRange(Validators.ValidateString(request.Expression, nameof(request.Expression), minLength: 1, maxLength: 1000));
            
            // Validate EffectiveFrom
            errors.AddRange(Validators.ValidateDate(request.EffectiveFrom, nameof(request.EffectiveFrom), 
                minDate: DateTime.UtcNow.AddYears(-1))); // Allow backdating up to 1 year
            
            // Validate EffectiveTo if provided
            if (request.EffectiveTo.HasValue)
            {
                // EffectiveTo must be after EffectiveFrom
                if (request.EffectiveTo.Value <= request.EffectiveFrom)
                {
                    errors.Add($"{nameof(request.EffectiveTo)}: Must be after {nameof(request.EffectiveFrom)}");
                }
            }
            
            // Validate Priority
            errors.AddRange(Validators.ValidateInteger(request.Priority, nameof(request.Priority), minValue: 1, maxValue: 1000));
            
            // Validate Parameters if provided
            if (request.Parameters != null && request.Parameters.Any())
            {
                errors.AddRange(ValidateRuleParameters(request.Parameters));
            }
            
            // Validate Conditions if provided
            if (request.Conditions != null && request.Conditions.Any())
            {
                errors.AddRange(ValidateRuleConditions(request.Conditions));
            }
            
            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }

        /// <summary>
        /// Validates an UpdateRuleRequest object against business rules
        /// </summary>
        /// <param name="request">The update rule request to validate</param>
        /// <returns>Result indicating success or validation failure with error details</returns>
        public Result ValidateUpdateRuleRequest(UpdateRuleRequest request)
        {
            var errors = new List<string>();
            
            // Validate RuleId
            errors.AddRange(Validators.ValidateGuid(request.RuleId, nameof(request.RuleId)));
            
            // Validate Name
            errors.AddRange(Validators.ValidateString(request.Name, nameof(request.Name), minLength: 3, maxLength: 100));
            
            // Validate Description
            errors.AddRange(Validators.ValidateString(request.Description, nameof(request.Description), maxLength: 500, required: false));
            
            // Validate Expression
            errors.AddRange(Validators.ValidateString(request.Expression, nameof(request.Expression), minLength: 1, maxLength: 1000));
            
            // Validate EffectiveFrom
            errors.AddRange(Validators.ValidateDate(request.EffectiveFrom, nameof(request.EffectiveFrom), 
                minDate: DateTime.UtcNow.AddYears(-1))); // Allow backdating up to 1 year
            
            // Validate EffectiveTo if provided
            if (request.EffectiveTo.HasValue)
            {
                // EffectiveTo must be after EffectiveFrom
                if (request.EffectiveTo.Value <= request.EffectiveFrom)
                {
                    errors.Add($"{nameof(request.EffectiveTo)}: Must be after {nameof(request.EffectiveFrom)}");
                }
            }
            
            // Validate Priority
            errors.AddRange(Validators.ValidateInteger(request.Priority, nameof(request.Priority), minValue: 1, maxValue: 1000));
            
            // Validate Parameters if provided
            if (request.Parameters != null && request.Parameters.Any())
            {
                errors.AddRange(ValidateRuleParameters(request.Parameters));
            }
            
            // Validate Conditions if provided
            if (request.Conditions != null && request.Conditions.Any())
            {
                errors.AddRange(ValidateRuleConditions(request.Conditions));
            }
            
            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }

        /// <summary>
        /// Validates a DeleteRuleRequest object against business rules
        /// </summary>
        /// <param name="request">The delete rule request to validate</param>
        /// <returns>Result indicating success or validation failure with error details</returns>
        public Result ValidateDeleteRuleRequest(DeleteRuleRequest request)
        {
            var errors = new List<string>();
            
            // Validate RuleId
            errors.AddRange(Validators.ValidateGuid(request.RuleId, nameof(request.RuleId)));
            
            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }

        /// <summary>
        /// Validates a GetRuleRequest object against business rules
        /// </summary>
        /// <param name="request">The get rule request to validate</param>
        /// <returns>Result indicating success or validation failure with error details</returns>
        public Result ValidateGetRuleRequest(GetRuleRequest request)
        {
            var errors = new List<string>();
            
            // Validate RuleId
            errors.AddRange(Validators.ValidateGuid(request.RuleId, nameof(request.RuleId)));
            
            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }

        /// <summary>
        /// Validates a GetRulesByCountryRequest object against business rules
        /// </summary>
        /// <param name="request">The get rules by country request to validate</param>
        /// <returns>Result indicating success or validation failure with error details</returns>
        public Result ValidateGetRulesByCountryRequest(GetRulesByCountryRequest request)
        {
            var errors = new List<string>();
            
            // Validate CountryCode
            errors.AddRange(Validators.ValidateCountryCode(request.CountryCode, nameof(request.CountryCode)));
            
            // Validate RuleType if provided
            if (request.RuleType.HasValue)
            {
                errors.AddRange(Validators.ValidateEnum(request.RuleType.Value, nameof(request.RuleType)));
            }
            
            // Validate PageNumber
            errors.AddRange(Validators.ValidateInteger(request.PageNumber, nameof(request.PageNumber), minValue: 1, maxValue: 10000));
            
            // Validate PageSize
            errors.AddRange(Validators.ValidateInteger(request.PageSize, nameof(request.PageSize), minValue: 1, maxValue: 100));
            
            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }

        /// <summary>
        /// Validates a ValidateRuleExpressionRequest object against business rules
        /// </summary>
        /// <param name="request">The validate rule expression request to validate</param>
        /// <returns>Result indicating success or validation failure with error details</returns>
        public Result ValidateRuleExpressionRequest(ValidateRuleExpressionRequest request)
        {
            var errors = new List<string>();
            
            // Validate Expression
            errors.AddRange(Validators.ValidateString(request.Expression, nameof(request.Expression), minLength: 1, maxLength: 1000));
            
            // Validate Parameters collection
            errors.AddRange(Validators.ValidateCollection(request.Parameters, nameof(request.Parameters), required: false));
            
            if (request.Parameters != null && request.Parameters.Any())
            {
                errors.AddRange(ValidateRuleParameters(request.Parameters));
            }
            
            // Validate SampleValues
            if (request.SampleValues == null || !request.SampleValues.Any())
            {
                errors.Add($"{nameof(request.SampleValues)}: Sample values are required for expression validation");
            }
            
            // Return validation result
            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }
            
            return Result.Success();
        }

        /// <summary>
        /// Validates a collection of rule parameters
        /// </summary>
        /// <param name="parameters">The rule parameters to validate</param>
        /// <returns>List of validation errors, empty if validation passes</returns>
        private List<string> ValidateRuleParameters(IEnumerable<RuleParameterRequest> parameters)
        {
            var errors = new List<string>();
            
            foreach (var parameter in parameters)
            {
                // Validate parameter Name
                errors.AddRange(Validators.ValidateString(parameter.Name, $"Parameter.{nameof(parameter.Name)}", 
                    minLength: 1, maxLength: 50));
                
                // Validate parameter DataType
                errors.AddRange(Validators.ValidateString(parameter.DataType, $"Parameter.{nameof(parameter.DataType)}", 
                    minLength: 1, maxLength: 20));
            }
            
            return errors;
        }

        /// <summary>
        /// Validates a collection of rule conditions
        /// </summary>
        /// <param name="conditions">The rule conditions to validate</param>
        /// <returns>List of validation errors, empty if validation passes</returns>
        private List<string> ValidateRuleConditions(IEnumerable<RuleConditionRequest> conditions)
        {
            var errors = new List<string>();
            
            foreach (var condition in conditions)
            {
                // Validate condition Parameter
                errors.AddRange(Validators.ValidateString(condition.Parameter, $"Condition.{nameof(condition.Parameter)}", 
                    minLength: 1, maxLength: 50));
                
                // Validate condition Operator
                errors.AddRange(Validators.ValidateString(condition.Operator, $"Condition.{nameof(condition.Operator)}", 
                    minLength: 1, maxLength: 20));
                
                // Validate condition Value
                errors.AddRange(Validators.ValidateString(condition.Value, $"Condition.{nameof(condition.Value)}", 
                    minLength: 1, maxLength: 100));
            }
            
            return errors;
        }
    }
}