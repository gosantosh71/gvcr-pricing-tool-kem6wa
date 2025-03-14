using System;
using System.Collections.Generic;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Domain.Rules
{
    /// <summary>
    /// Interface defining the contract for the Rule Engine which applies country-specific VAT rules to pricing calculations.
    /// The Rule Engine is responsible for retrieving, filtering, and evaluating rules based on various criteria
    /// to enable accurate calculation of VAT filing costs across different jurisdictions.
    /// </summary>
    public interface IRuleEngine
    {
        /// <summary>
        /// Retrieves rules applicable for a specific country and date.
        /// </summary>
        /// <param name="countryCode">The country code for which to retrieve rules.</param>
        /// <param name="effectiveDate">The date for which the rules should be effective.</param>
        /// <returns>Collection of applicable rules sorted by priority.</returns>
        IEnumerable<Rule> GetApplicableRules(CountryCode countryCode, DateTime effectiveDate);

        /// <summary>
        /// Retrieves rules of a specific type applicable for a country and date.
        /// </summary>
        /// <param name="countryCode">The country code for which to retrieve rules.</param>
        /// <param name="ruleType">The type of rules to retrieve.</param>
        /// <param name="effectiveDate">The date for which the rules should be effective.</param>
        /// <returns>Collection of applicable rules of the specified type sorted by priority.</returns>
        IEnumerable<Rule> GetApplicableRulesByType(CountryCode countryCode, RuleType ruleType, DateTime effectiveDate);

        /// <summary>
        /// Evaluates a single rule with the provided parameters.
        /// </summary>
        /// <param name="rule">The rule to evaluate.</param>
        /// <param name="parameters">Parameters to use in the rule evaluation.</param>
        /// <returns>Result of the rule evaluation.</returns>
        decimal EvaluateRule(Rule rule, Dictionary<string, object> parameters);

        /// <summary>
        /// Evaluates multiple rules with the provided parameters.
        /// </summary>
        /// <param name="rules">The rules to evaluate.</param>
        /// <param name="parameters">Parameters to use in the rule evaluation.</param>
        /// <returns>Results of rule evaluations with rule IDs as keys.</returns>
        Dictionary<string, decimal> EvaluateRules(IEnumerable<Rule> rules, Dictionary<string, object> parameters);

        /// <summary>
        /// Calculates the total cost for a specific country based on applicable rules.
        /// </summary>
        /// <param name="countryCode">The country code for which to calculate costs.</param>
        /// <param name="parameters">Parameters for the calculation.</param>
        /// <param name="effectiveDate">The date for which the rules should be effective.</param>
        /// <returns>Total calculated cost for the country.</returns>
        Money CalculateCountryCost(CountryCode countryCode, Dictionary<string, object> parameters, DateTime effectiveDate);

        /// <summary>
        /// Checks if a rule's conditions are satisfied with the provided parameters.
        /// </summary>
        /// <param name="rule">The rule to check.</param>
        /// <param name="parameters">Parameters to check against the rule conditions.</param>
        /// <returns>True if all conditions are satisfied, otherwise false.</returns>
        bool CheckRuleConditions(Rule rule, Dictionary<string, object> parameters);

        /// <summary>
        /// Validates if a rule expression is syntactically correct.
        /// </summary>
        /// <param name="expression">The rule expression to validate.</param>
        /// <returns>True if the expression is valid, otherwise false.</returns>
        bool ValidateRuleExpression(string expression);
    }
}