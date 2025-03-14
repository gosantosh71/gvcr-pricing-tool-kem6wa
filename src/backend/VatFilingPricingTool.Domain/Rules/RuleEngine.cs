using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.Linq; // Version 6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Domain.Rules
{
    /// <summary>
    /// Implements the Rule Engine which applies country-specific VAT rules to pricing calculations.
    /// This class is responsible for retrieving, filtering, and evaluating rules based on country,
    /// rule type, and effective dates to calculate accurate VAT filing costs.
    /// </summary>
    public class RuleEngine : IRuleEngine
    {
        /// <summary>
        /// Gets the collection of rules used by the engine.
        /// </summary>
        public IEnumerable<Rule> Rules { get; }

        /// <summary>
        /// Creates a new instance of the RuleEngine with a collection of rules.
        /// </summary>
        /// <param name="rules">The collection of rules to use.</param>
        /// <exception cref="ValidationException">Thrown when rules is null.</exception>
        public RuleEngine(IEnumerable<Rule> rules)
        {
            if (rules == null)
            {
                throw new ValidationException("Rules collection cannot be null",
                    new List<string> { "A valid collection of rules must be provided" });
            }

            Rules = rules;
        }

        /// <summary>
        /// Retrieves rules applicable for a specific country and date.
        /// </summary>
        /// <param name="countryCode">The country code for which to retrieve rules.</param>
        /// <param name="effectiveDate">The date for which the rules should be effective.</param>
        /// <returns>Collection of applicable rules sorted by priority.</returns>
        /// <exception cref="ValidationException">Thrown when countryCode is null.</exception>
        public IEnumerable<Rule> GetApplicableRules(CountryCode countryCode, DateTime effectiveDate)
        {
            if (countryCode == null)
            {
                throw new ValidationException("Country code cannot be null",
                    new List<string> { "A valid country code must be provided" });
            }

            return Rules
                .Where(r => r.CountryCode == countryCode)
                .Where(r => r.IsEffectiveAt(effectiveDate))
                .Where(r => r.IsActive)
                .OrderBy(r => r.Priority)
                .ToList();
        }

        /// <summary>
        /// Retrieves rules of a specific type applicable for a country and date.
        /// </summary>
        /// <param name="countryCode">The country code for which to retrieve rules.</param>
        /// <param name="ruleType">The type of rules to retrieve.</param>
        /// <param name="effectiveDate">The date for which the rules should be effective.</param>
        /// <returns>Collection of applicable rules of the specified type sorted by priority.</returns>
        public IEnumerable<Rule> GetApplicableRulesByType(CountryCode countryCode, RuleType ruleType, DateTime effectiveDate)
        {
            return GetApplicableRules(countryCode, effectiveDate)
                .Where(r => r.Type == ruleType)
                .ToList();
        }

        /// <summary>
        /// Evaluates a single rule with the provided parameters.
        /// </summary>
        /// <param name="rule">The rule to evaluate.</param>
        /// <param name="parameters">Parameters to use in the rule evaluation.</param>
        /// <returns>Result of the rule evaluation.</returns>
        /// <exception cref="ValidationException">Thrown when rule or parameters is null.</exception>
        public decimal EvaluateRule(Rule rule, Dictionary<string, object> parameters)
        {
            if (rule == null)
            {
                throw new ValidationException("Rule cannot be null",
                    new List<string> { "A valid rule must be provided for evaluation" });
            }

            if (parameters == null)
            {
                throw new ValidationException("Parameters cannot be null",
                    new List<string> { "Parameter dictionary must be provided for rule evaluation" });
            }

            return RuleEvaluator.EvaluateRule(rule, parameters);
        }

        /// <summary>
        /// Evaluates multiple rules with the provided parameters.
        /// </summary>
        /// <param name="rules">The rules to evaluate.</param>
        /// <param name="parameters">Parameters to use in the rule evaluation.</param>
        /// <returns>Results of rule evaluations with rule IDs as keys.</returns>
        /// <exception cref="ValidationException">Thrown when rules or parameters is null.</exception>
        public Dictionary<string, decimal> EvaluateRules(IEnumerable<Rule> rules, Dictionary<string, object> parameters)
        {
            if (rules == null)
            {
                throw new ValidationException("Rules collection cannot be null",
                    new List<string> { "A valid collection of rules must be provided for evaluation" });
            }

            if (parameters == null)
            {
                throw new ValidationException("Parameters cannot be null",
                    new List<string> { "Parameter dictionary must be provided for rule evaluation" });
            }

            var results = new Dictionary<string, decimal>();

            foreach (var rule in rules)
            {
                if (CheckRuleConditions(rule, parameters))
                {
                    results[rule.RuleId] = EvaluateRule(rule, parameters);
                }
            }

            return results;
        }

        /// <summary>
        /// Calculates the total cost for a specific country based on applicable rules.
        /// </summary>
        /// <param name="countryCode">The country code for which to calculate costs.</param>
        /// <param name="parameters">Parameters for the calculation.</param>
        /// <param name="effectiveDate">The date for which the rules should be effective.</param>
        /// <returns>Total calculated cost for the country.</returns>
        /// <exception cref="ValidationException">Thrown when countryCode or parameters is null.</exception>
        public Money CalculateCountryCost(CountryCode countryCode, Dictionary<string, object> parameters, DateTime effectiveDate)
        {
            if (countryCode == null)
            {
                throw new ValidationException("Country code cannot be null",
                    new List<string> { "A valid country code must be provided" });
            }

            if (parameters == null)
            {
                throw new ValidationException("Parameters cannot be null",
                    new List<string> { "Parameter dictionary must be provided for calculation" });
            }

            decimal totalCost = 0;

            // Process rules by type in a specific order
            // 1. VatRate - Base VAT rates
            ProcessRuleType(RuleType.VatRate, ref totalCost);
            
            // 2. Threshold - Volume-based pricing tiers
            ProcessRuleType(RuleType.Threshold, ref totalCost);
            
            // 3. Complexity - Filing complexity adjustments
            ProcessRuleType(RuleType.Complexity, ref totalCost);
            
            // 4. SpecialRequirement - Country-specific special requirements
            ProcessRuleType(RuleType.SpecialRequirement, ref totalCost);
            
            // 5. Discount - Apply discounts last
            ProcessRuleType(RuleType.Discount, ref totalCost, true); // Discounts reduce the cost

            // Ensure total cost is never negative
            if (totalCost < 0)
            {
                totalCost = 0;
            }

            // Create a Money object with the total cost and the country's currency
            return Money.Create(totalCost, countryCode.GetCurrencyCode());

            // Local function to process rules of a specific type
            void ProcessRuleType(RuleType ruleType, ref decimal cost, bool isDiscount = false)
            {
                var typeRules = GetApplicableRulesByType(countryCode, ruleType, effectiveDate);
                foreach (var rule in typeRules)
                {
                    if (CheckRuleConditions(rule, parameters))
                    {
                        var ruleResult = EvaluateRule(rule, parameters);
                        if (isDiscount)
                        {
                            cost -= ruleResult; // Discounts reduce the cost
                        }
                        else
                        {
                            cost += ruleResult; // Other rule types add to the cost
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a rule's conditions are satisfied with the provided parameters.
        /// </summary>
        /// <param name="rule">The rule to check.</param>
        /// <param name="parameters">Parameters to check against the rule conditions.</param>
        /// <returns>True if all conditions are satisfied, otherwise false.</returns>
        /// <exception cref="ValidationException">Thrown when rule or parameters is null.</exception>
        public bool CheckRuleConditions(Rule rule, Dictionary<string, object> parameters)
        {
            if (rule == null)
            {
                throw new ValidationException("Rule cannot be null",
                    new List<string> { "A valid rule must be provided for condition checking" });
            }

            if (parameters == null)
            {
                throw new ValidationException("Parameters cannot be null",
                    new List<string> { "Parameter dictionary must be provided for condition checking" });
            }

            return RuleEvaluator.CheckConditions(rule, parameters);
        }

        /// <summary>
        /// Validates if a rule expression is syntactically correct.
        /// </summary>
        /// <param name="expression">The rule expression to validate.</param>
        /// <returns>True if the expression is valid, otherwise false.</returns>
        /// <exception cref="ValidationException">Thrown when expression is null or empty.</exception>
        public bool ValidateRuleExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ValidationException("Expression cannot be null or empty",
                    new List<string> { "A valid expression must be provided" },
                    ErrorCodes.Rule.InvalidRuleExpression);
            }

            return RuleEvaluator.ValidateRuleExpression(expression);
        }
    }
}