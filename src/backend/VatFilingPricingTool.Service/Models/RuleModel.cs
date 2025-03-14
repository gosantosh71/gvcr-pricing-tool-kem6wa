using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Service.Helpers;

namespace VatFilingPricingTool.Service.Models
{
    /// <summary>
    /// Service layer model for VAT filing pricing rules with mapping functionality
    /// between domain entities and contract models.
    /// </summary>
    public class RuleModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the rule
        /// </summary>
        public string RuleId { get; set; }

        /// <summary>
        /// Gets or sets the country code for which this rule applies
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the type of rule (VatRate, Threshold, Complexity, etc.)
        /// </summary>
        public RuleType Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the rule
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the rule
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the expression that calculates the rule's result
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets the date from which this rule is effective
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Gets or sets the date until which this rule is effective (null means indefinitely)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Gets or sets the priority of the rule (lower number = higher priority)
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the rule is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the collection of parameters used in the rule expression
        /// </summary>
        public List<RuleParameterModel> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the collection of conditions that determine when this rule applies
        /// </summary>
        public List<RuleConditionModel> Conditions { get; set; }

        /// <summary>
        /// Default constructor for the RuleModel
        /// </summary>
        public RuleModel()
        {
            Parameters = new List<RuleParameterModel>();
            Conditions = new List<RuleConditionModel>();
            IsActive = true;
            EffectiveFrom = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a RuleModel from a domain Rule entity
        /// </summary>
        /// <param name="rule">The domain entity to map from</param>
        /// <returns>A new RuleModel populated with data from the domain entity</returns>
        public static RuleModel FromDomain(Rule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule), "Rule entity cannot be null");
            }

            var model = new RuleModel
            {
                RuleId = rule.RuleId,
                CountryCode = rule.CountryCode.Value,
                Type = rule.Type,
                Name = rule.Name,
                Description = rule.Description,
                Expression = rule.Expression,
                EffectiveFrom = rule.EffectiveFrom,
                EffectiveTo = rule.EffectiveTo,
                Priority = rule.Priority,
                IsActive = rule.IsActive,
                LastUpdated = rule.LastUpdated
            };

            // Map parameters
            foreach (var parameter in rule.Parameters)
            {
                model.Parameters.Add(RuleParameterModel.FromDomain(parameter));
            }

            // Map conditions
            foreach (var condition in rule.Conditions)
            {
                model.Conditions.Add(RuleConditionModel.FromDomain(condition));
            }

            return model;
        }

        /// <summary>
        /// Converts the RuleModel to a domain Rule entity
        /// </summary>
        /// <returns>A new domain Rule entity created from this model</returns>
        public Rule ToDomain()
        {
            // Create the base rule using the factory method
            var rule = Rule.Create(
                CountryCode,
                Type,
                Name,
                Expression,
                EffectiveFrom,
                Description
            );

            // Set the RuleId if it exists
            if (!string.IsNullOrEmpty(RuleId))
            {
                typeof(Rule).GetProperty("RuleId").SetValue(rule, RuleId);
            }

            // Set EffectiveTo if it has a value
            if (EffectiveTo.HasValue)
            {
                rule.UpdateEffectiveDates(EffectiveFrom, EffectiveTo);
            }

            // Set Priority if it's different from default
            if (Priority != 0)
            {
                rule.UpdatePriority(Priority);
            }

            // Set IsActive
            rule.SetActive(IsActive);

            // Add parameters
            foreach (var param in Parameters)
            {
                rule.AddParameter(param.Name, param.DataType, param.DefaultValue);
            }

            // Add conditions
            foreach (var condition in Conditions)
            {
                rule.AddCondition(condition.Parameter, condition.Operator, condition.Value);
            }

            return rule;
        }

        /// <summary>
        /// Creates a RuleModel from a contract RuleModel
        /// </summary>
        /// <param name="contractModel">The contract model to map from</param>
        /// <returns>A new RuleModel populated with data from the contract model</returns>
        public static RuleModel FromContract(Contracts.V1.Models.RuleModel contractModel)
        {
            if (contractModel == null)
            {
                throw new ArgumentNullException(nameof(contractModel), "Contract model cannot be null");
            }

            var model = new RuleModel
            {
                RuleId = contractModel.RuleId,
                CountryCode = contractModel.CountryCode,
                Type = contractModel.RuleType,
                Name = contractModel.Name,
                Description = contractModel.Description,
                Expression = contractModel.Expression,
                EffectiveFrom = contractModel.EffectiveFrom,
                EffectiveTo = contractModel.EffectiveTo,
                Priority = contractModel.Priority,
                IsActive = contractModel.IsActive,
                LastUpdated = contractModel.LastUpdated
            };

            // Map parameters
            if (contractModel.Parameters != null)
            {
                foreach (var parameter in contractModel.Parameters)
                {
                    model.Parameters.Add(RuleParameterModel.FromContract(parameter));
                }
            }

            // Map conditions
            if (contractModel.Conditions != null)
            {
                foreach (var condition in contractModel.Conditions)
                {
                    model.Conditions.Add(RuleConditionModel.FromContract(condition));
                }
            }

            return model;
        }

        /// <summary>
        /// Converts the RuleModel to a contract RuleModel
        /// </summary>
        /// <returns>A new contract RuleModel created from this model</returns>
        public Contracts.V1.Models.RuleModel ToContract()
        {
            var contractModel = new Contracts.V1.Models.RuleModel
            {
                RuleId = this.RuleId,
                CountryCode = this.CountryCode,
                RuleType = this.Type,
                Name = this.Name,
                Description = this.Description,
                Expression = this.Expression,
                EffectiveFrom = this.EffectiveFrom,
                EffectiveTo = this.EffectiveTo,
                Priority = this.Priority,
                IsActive = this.IsActive,
                LastUpdated = this.LastUpdated,
                Parameters = new List<Contracts.V1.Models.RuleParameterModel>(),
                Conditions = new List<Contracts.V1.Models.RuleConditionModel>()
            };

            // Map parameters
            foreach (var parameter in this.Parameters)
            {
                contractModel.Parameters.Add(parameter.ToContract());
            }

            // Map conditions
            foreach (var condition in this.Conditions)
            {
                contractModel.Conditions.Add(condition.ToContract());
            }

            return contractModel;
        }

        /// <summary>
        /// Validates the RuleModel according to business rules
        /// </summary>
        /// <returns>True if the model is valid, false otherwise</returns>
        public bool IsValid()
        {
            // Validate country code
            if (!ValidationHelper.IsValidCountryCode(CountryCode))
            {
                return false;
            }

            // Validate name
            if (string.IsNullOrEmpty(Name) || Name.Length > 100)
            {
                return false;
            }

            // Validate expression
            if (string.IsNullOrEmpty(Expression) || Expression.Length > 2000)
            {
                return false;
            }

            if (!ValidationHelper.IsValidExpression(Expression))
            {
                return false;
            }

            // Validate dates
            if (string.IsNullOrEmpty(RuleId) && EffectiveFrom < DateTime.UtcNow.Date)
            {
                // For new rules, effective date cannot be in the past
                return false;
            }

            if (EffectiveTo.HasValue && EffectiveTo.Value <= EffectiveFrom)
            {
                return false;
            }

            // Validate priority
            if (Priority < 1 || Priority > 1000)
            {
                return false;
            }

            // Validate parameters
            foreach (var parameter in Parameters)
            {
                if (string.IsNullOrEmpty(parameter.Name) || 
                    string.IsNullOrEmpty(parameter.DataType))
                {
                    return false;
                }
            }

            // Validate conditions
            foreach (var condition in Conditions)
            {
                if (string.IsNullOrEmpty(condition.Parameter) || 
                    string.IsNullOrEmpty(condition.Operator) || 
                    condition.Value == null)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Service layer model for parameters used in rule expressions
    /// </summary>
    public class RuleParameterModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the parameter
        /// </summary>
        public string ParameterId { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data type of the parameter
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the default value of the parameter
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Default constructor for the RuleParameterModel
        /// </summary>
        public RuleParameterModel()
        {
        }

        /// <summary>
        /// Creates a RuleParameterModel from a domain RuleParameter entity
        /// </summary>
        /// <param name="parameter">The domain entity to map from</param>
        /// <returns>A new RuleParameterModel populated with data from the domain entity</returns>
        public static RuleParameterModel FromDomain(RuleParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter), "Parameter entity cannot be null");
            }

            return new RuleParameterModel
            {
                ParameterId = parameter.ParameterId,
                Name = parameter.Name,
                DataType = parameter.DataType,
                DefaultValue = parameter.DefaultValue
            };
        }

        /// <summary>
        /// Converts the RuleParameterModel to a domain RuleParameter entity
        /// </summary>
        /// <returns>A new domain RuleParameter entity created from this model</returns>
        public RuleParameter ToDomain()
        {
            return RuleParameter.Create(Name, DataType, DefaultValue);
        }

        /// <summary>
        /// Creates a RuleParameterModel from a contract RuleParameterModel
        /// </summary>
        /// <param name="contractModel">The contract model to map from</param>
        /// <returns>A new RuleParameterModel populated with data from the contract model</returns>
        public static RuleParameterModel FromContract(Contracts.V1.Models.RuleParameterModel contractModel)
        {
            if (contractModel == null)
            {
                throw new ArgumentNullException(nameof(contractModel), "Contract model cannot be null");
            }

            return new RuleParameterModel
            {
                ParameterId = contractModel.ParameterId,
                Name = contractModel.Name,
                DataType = contractModel.DataType,
                DefaultValue = contractModel.DefaultValue
            };
        }

        /// <summary>
        /// Converts the RuleParameterModel to a contract RuleParameterModel
        /// </summary>
        /// <returns>A new contract RuleParameterModel created from this model</returns>
        public Contracts.V1.Models.RuleParameterModel ToContract()
        {
            return new Contracts.V1.Models.RuleParameterModel
            {
                ParameterId = this.ParameterId,
                Name = this.Name,
                DataType = this.DataType,
                DefaultValue = this.DefaultValue
            };
        }
    }

    /// <summary>
    /// Service layer model for conditions that determine when a rule should be applied
    /// </summary>
    public class RuleConditionModel
    {
        /// <summary>
        /// Gets or sets the parameter to check in the condition
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// Gets or sets the comparison operator
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the value to compare against
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Default constructor for the RuleConditionModel
        /// </summary>
        public RuleConditionModel()
        {
        }

        /// <summary>
        /// Creates a RuleConditionModel from a domain RuleCondition entity
        /// </summary>
        /// <param name="condition">The domain entity to map from</param>
        /// <returns>A new RuleConditionModel populated with data from the domain entity</returns>
        public static RuleConditionModel FromDomain(RuleCondition condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition), "Condition entity cannot be null");
            }

            return new RuleConditionModel
            {
                Parameter = condition.Parameter,
                Operator = condition.Operator,
                Value = condition.Value
            };
        }

        /// <summary>
        /// Converts the RuleConditionModel to a domain RuleCondition entity
        /// </summary>
        /// <returns>A new domain RuleCondition entity created from this model</returns>
        public RuleCondition ToDomain()
        {
            return new RuleCondition(Parameter, Operator, Value);
        }

        /// <summary>
        /// Creates a RuleConditionModel from a contract RuleConditionModel
        /// </summary>
        /// <param name="contractModel">The contract model to map from</param>
        /// <returns>A new RuleConditionModel populated with data from the contract model</returns>
        public static RuleConditionModel FromContract(Contracts.V1.Models.RuleConditionModel contractModel)
        {
            if (contractModel == null)
            {
                throw new ArgumentNullException(nameof(contractModel), "Contract model cannot be null");
            }

            return new RuleConditionModel
            {
                Parameter = contractModel.Parameter,
                Operator = contractModel.Operator,
                Value = contractModel.Value
            };
        }

        /// <summary>
        /// Converts the RuleConditionModel to a contract RuleConditionModel
        /// </summary>
        /// <returns>A new contract RuleConditionModel created from this model</returns>
        public Contracts.V1.Models.RuleConditionModel ToContract()
        {
            return new Contracts.V1.Models.RuleConditionModel
            {
                Parameter = this.Parameter,
                Operator = this.Operator,
                Value = this.Value
            };
        }
    }
}