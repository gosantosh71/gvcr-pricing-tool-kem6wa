using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.Linq; // Version 6.0.0
using Xunit; // Version 2.4.1
using FluentAssertions; // Version 6.7.0
using VatFilingPricingTool.Domain.Rules;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.UnitTests.Helpers;

namespace VatFilingPricingTool.UnitTests.Domain
{
    /// <summary>
    /// Test suite for the RuleEngine class
    /// </summary>
    public class RuleEngineTests
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RuleEngineTests()
        {
            // Initialization, if needed
        }

        /// <summary>
        /// Tests that GetApplicableRules correctly filters rules by country and effective date
        /// </summary>
        [Fact]
        public void GetApplicableRules_WithValidCountryAndDate_ReturnsFilteredRules()
        {
            // Arrange: Create a list of rules for different countries and dates
            var rules = new List<Rule>
            {
                Rule.Create("GB", RuleType.VatRate, "UK VAT Rate", "basePrice * 0.20", DateTime.UtcNow.AddDays(-1)),
                Rule.Create("DE", RuleType.VatRate, "DE VAT Rate", "basePrice * 0.19", DateTime.UtcNow.AddDays(1)),
                Rule.Create("GB", RuleType.Threshold, "UK Threshold", "transactionVolume > 1000 ? basePrice * 1.1 : basePrice", DateTime.UtcNow)
            };

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a CountryCode for 'GB'
            var gbCountryCode = CountryCode.Create("GB");

            // Arrange: Set a current date for testing
            var currentDate = DateTime.UtcNow;

            // Act: Call ruleEngine.GetApplicableRules(gbCountryCode, currentDate)
            var applicableRules = ruleEngine.GetApplicableRules(gbCountryCode, currentDate).ToList();

            // Assert: Verify that only rules for GB that are active and effective at the current date are returned
            applicableRules.Should().HaveCount(1);
            applicableRules.First().CountryCode.Should().Be(gbCountryCode);

            // Assert: Verify that rules are ordered by priority
            applicableRules.Should().BeInAscendingOrder(r => r.Priority);
        }

        /// <summary>
        /// Tests that GetApplicableRules filters out inactive rules
        /// </summary>
        [Fact]
        public void GetApplicableRules_WithInactiveRules_FiltersOutInactiveRules()
        {
            // Arrange: Create a list of rules including some inactive ones
            var rules = new List<Rule>
            {
                Rule.Create("GB", RuleType.VatRate, "Active Rule", "basePrice * 0.20", DateTime.UtcNow),
                Rule.Create("GB", RuleType.Threshold, "Inactive Rule", "transactionVolume > 1000 ? basePrice * 1.1 : basePrice", DateTime.UtcNow)
            };
            rules[1].SetActive(false);

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a CountryCode for 'GB'
            var gbCountryCode = CountryCode.Create("GB");

            // Arrange: Set a current date for testing
            var currentDate = DateTime.UtcNow;

            // Act: Call ruleEngine.GetApplicableRules(gbCountryCode, currentDate)
            var applicableRules = ruleEngine.GetApplicableRules(gbCountryCode, currentDate).ToList();

            // Assert: Verify that inactive rules are not included in the result
            applicableRules.Should().HaveCount(1);
            applicableRules.First().Name.Should().Be("Active Rule");
        }

        /// <summary>
        /// Tests that GetApplicableRules filters out rules that are not effective at the specified date
        /// </summary>
        [Fact]
        public void GetApplicableRules_WithExpiredRules_FiltersOutExpiredRules()
        {
            // Arrange: Create a list of rules with different effective date ranges
            var rules = new List<Rule>
            {
                Rule.Create("GB", RuleType.VatRate, "Future Rule", "basePrice * 0.20", DateTime.UtcNow.AddDays(1)),
                Rule.Create("GB", RuleType.Threshold, "Past Rule", "transactionVolume > 1000 ? basePrice * 1.1 : basePrice", DateTime.UtcNow.AddDays(-2))
            };

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a CountryCode for 'GB'
            var gbCountryCode = CountryCode.Create("GB");

            // Arrange: Set a future date for testing
            var futureDate = DateTime.UtcNow.AddDays(2);

            // Act: Call ruleEngine.GetApplicableRules(gbCountryCode, futureDate)
            var applicableRules = ruleEngine.GetApplicableRules(gbCountryCode, futureDate).ToList();

            // Assert: Verify that rules not effective at the future date are not included in the result
            applicableRules.Should().HaveCount(1);
            applicableRules.First().Name.Should().Be("Future Rule");
        }

        /// <summary>
        /// Tests that GetApplicableRulesByType correctly filters rules by country, type, and effective date
        /// </summary>
        [Fact]
        public void GetApplicableRulesByType_WithValidParameters_ReturnsFilteredRulesByType()
        {
            // Arrange: Create a list of rules with different types
            var rules = new List<Rule>
            {
                Rule.Create("GB", RuleType.VatRate, "UK VAT Rate", "basePrice * 0.20", DateTime.UtcNow),
                Rule.Create("GB", RuleType.Threshold, "UK Threshold", "transactionVolume > 1000 ? basePrice * 1.1 : basePrice", DateTime.UtcNow),
                Rule.Create("DE", RuleType.VatRate, "DE VAT Rate", "basePrice * 0.19", DateTime.UtcNow)
            };

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a CountryCode for 'GB'
            var gbCountryCode = CountryCode.Create("GB");

            // Arrange: Set a current date for testing
            var currentDate = DateTime.UtcNow;

            // Act: Call ruleEngine.GetApplicableRulesByType(gbCountryCode, RuleType.VatRate, currentDate)
            var applicableRules = ruleEngine.GetApplicableRulesByType(gbCountryCode, RuleType.VatRate, currentDate).ToList();

            // Assert: Verify that only VAT rate rules for GB that are active and effective at the current date are returned
            applicableRules.Should().HaveCount(1);
            applicableRules.First().Type.Should().Be(RuleType.VatRate);
            applicableRules.First().CountryCode.Should().Be(gbCountryCode);

            // Assert: Verify that rules are ordered by priority
            applicableRules.Should().BeInAscendingOrder(r => r.Priority);
        }

        /// <summary>
        /// Tests that EvaluateRule correctly evaluates a rule with the provided parameters
        /// </summary>
        [Fact]
        public void EvaluateRule_WithValidRuleAndParameters_ReturnsCorrectResult()
        {
            // Arrange: Create a rule with a simple expression like 'basePrice * 0.20'
            var rule = Rule.Create("GB", RuleType.VatRate, "UK VAT Rate", "basePrice * 0.20", DateTime.UtcNow);

            // Arrange: Create a RuleEngine instance with the rule
            var ruleEngine = new RuleEngine(new List<Rule> { rule });

            // Arrange: Create a parameters dictionary with basePrice = 1000
            var parameters = new Dictionary<string, object> { { "basePrice", 1000 } };

            // Act: Call ruleEngine.EvaluateRule(rule, parameters)
            var result = ruleEngine.EvaluateRule(rule, parameters);

            // Assert: Verify the result equals 200 (1000 * 0.20)
            result.Should().Be(200);
        }

        /// <summary>
        /// Tests that EvaluateRule throws a ValidationException when a required parameter is missing
        /// </summary>
        [Fact]
        public void EvaluateRule_WithMissingParameter_ThrowsValidationException()
        {
            // Arrange: Create a rule with an expression that uses a parameter like 'basePrice * vatRate'
            var rule = Rule.Create("GB", RuleType.VatRate, "UK VAT Rate", "basePrice * vatRate", DateTime.UtcNow);

            // Arrange: Create a RuleEngine instance with the rule
            var ruleEngine = new RuleEngine(new List<Rule> { rule });

            // Arrange: Create a parameters dictionary with only basePrice (missing vatRate)
            var parameters = new Dictionary<string, object> { { "basePrice", 1000 } };

            // Act/Assert: Verify that ruleEngine.EvaluateRule(rule, parameters) throws ValidationException with Rule.ParameterNotFound error code
            Assert.Throws<DomainException>(() => ruleEngine.EvaluateRule(rule, parameters))
                .ErrorCode.Should().Be(ErrorCodes.Rule.RuleValidationFailed);
        }

        /// <summary>
        /// Tests that EvaluateRules correctly evaluates multiple rules and returns a dictionary of results
        /// </summary>
        [Fact]
        public void EvaluateRules_WithMultipleRules_ReturnsResultDictionary()
        {
            // Arrange: Create a list of rules with different expressions
            var rules = new List<Rule>
            {
                Rule.Create("GB", RuleType.VatRate, "UK VAT Rate", "basePrice * 0.20", DateTime.UtcNow),
                Rule.Create("GB", RuleType.Threshold, "UK Threshold", "transactionVolume > 1000 ? basePrice * 1.1 : basePrice", DateTime.UtcNow)
            };

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a parameters dictionary with necessary values
            var parameters = new Dictionary<string, object>
            {
                { "basePrice", 1000 },
                { "transactionVolume", 1200 }
            };

            // Act: Call ruleEngine.EvaluateRules(rules, parameters)
            var results = ruleEngine.EvaluateRules(rules, parameters);

            // Assert: Verify the result dictionary contains an entry for each rule
            results.Should().HaveCount(2);

            // Assert: Verify each result matches the expected calculation
            results[rules[0].RuleId].Should().Be(200);
            results[rules[1].RuleId].Should().Be(1100);
        }

        /// <summary>
        /// Tests that EvaluateRules only evaluates rules whose conditions are satisfied
        /// </summary>
        [Fact]
        public void EvaluateRules_WithConditions_EvaluatesOnlyRulesThatMeetConditions()
        {
            // Arrange: Create rules with different conditions
            var rules = new List<Rule>
            {
                Rule.Create("GB", RuleType.VatRate, "Standard Filing", "basePrice * 0.20", DateTime.UtcNow),
                Rule.Create("GB", RuleType.Threshold, "Complex Filing", "transactionVolume > 1000 ? basePrice * 1.1 : basePrice", DateTime.UtcNow)
            };
            rules[0].AddCondition("serviceType", "equals", "StandardFiling");
            rules[1].AddCondition("serviceType", "equals", "ComplexFiling");

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a parameters dictionary that satisfies some conditions but not others
            var parameters = new Dictionary<string, object>
            {
                { "basePrice", 1000 },
                { "transactionVolume", 1200 },
                { "serviceType", "StandardFiling" }
            };

            // Act: Call ruleEngine.EvaluateRules(rules, parameters)
            var results = ruleEngine.EvaluateRules(rules, parameters);

            // Assert: Verify the result dictionary only contains entries for rules whose conditions are satisfied
            results.Should().HaveCount(1);
            results.ContainsKey(rules[0].RuleId).Should().BeTrue();
        }

        /// <summary>
        /// Tests that CheckRuleConditions returns true when all conditions are satisfied
        /// </summary>
        [Fact]
        public void CheckRuleConditions_WithSatisfiedConditions_ReturnsTrue()
        {
            // Arrange: Create a rule with conditions
            var rule = Rule.Create("GB", RuleType.VatRate, "Standard Filing", "basePrice * 0.20", DateTime.UtcNow);
            rule.AddCondition("serviceType", "equals", "StandardFiling");
            rule.AddCondition("transactionVolume", "greaterThan", "500");

            // Arrange: Create a RuleEngine instance
            var ruleEngine = new RuleEngine(new List<Rule> { rule });

            // Arrange: Create a parameters dictionary that satisfies all conditions
            var parameters = new Dictionary<string, object>
            {
                { "serviceType", "StandardFiling" },
                { "transactionVolume", 1000 }
            };

            // Act: Call ruleEngine.CheckRuleConditions(rule, parameters)
            var result = ruleEngine.CheckRuleConditions(rule, parameters);

            // Assert: Verify the result is true
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that CheckRuleConditions returns false when any condition is not satisfied
        /// </summary>
        [Fact]
        public void CheckRuleConditions_WithUnsatisfiedConditions_ReturnsFalse()
        {
            // Arrange: Create a rule with multiple conditions
            var rule = Rule.Create("GB", RuleType.VatRate, "Standard Filing", "basePrice * 0.20", DateTime.UtcNow);
            rule.AddCondition("serviceType", "equals", "StandardFiling");
            rule.AddCondition("transactionVolume", "greaterThan", "500");

            // Arrange: Create a RuleEngine instance
            var ruleEngine = new RuleEngine(new List<Rule> { rule });

            // Arrange: Create a parameters dictionary that satisfies some conditions but not all
            var parameters = new Dictionary<string, object>
            {
                { "serviceType", "ComplexFiling" },
                { "transactionVolume", 1000 }
            };

            // Act: Call ruleEngine.CheckRuleConditions(rule, parameters)
            var result = ruleEngine.CheckRuleConditions(rule, parameters);

            // Assert: Verify the result is false
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that ValidateRuleExpression returns true for valid expressions
        /// </summary>
        [Fact]
        public void ValidateRuleExpression_WithValidExpression_ReturnsTrue()
        {
            // Arrange: Create a valid expression string
            string expression = "basePrice * 0.20";

            // Arrange: Create a RuleEngine instance
            var ruleEngine = new RuleEngine(new List<Rule>());

            // Act: Call ruleEngine.ValidateRuleExpression(expression)
            var result = ruleEngine.ValidateRuleExpression(expression);

            // Assert: Verify the result is true
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ValidateRuleExpression throws ValidationException for invalid expressions
        /// </summary>
        [Fact]
        public void ValidateRuleExpression_WithInvalidExpression_ThrowsValidationException()
        {
            // Arrange: Create an invalid expression string
            string expression = "basePrice ** 0.20";

            // Arrange: Create a RuleEngine instance
            var ruleEngine = new RuleEngine(new List<Rule>());

            // Act/Assert: Verify that ruleEngine.ValidateRuleExpression(expression) throws ValidationException with Rule.InvalidRuleExpression error code
            Assert.Throws<DomainException>(() => ruleEngine.ValidateRuleExpression(expression))
                .ErrorCode.Should().Be(ErrorCodes.Rule.InvalidRuleExpression);
        }

        /// <summary>
        /// Tests that CalculateCountryCost correctly calculates the total cost for a country based on applicable rules
        /// </summary>
        [Fact]
        public void CalculateCountryCost_WithValidParameters_ReturnsCorrectTotalCost()
        {
            // Arrange: Create rules of different types for a country
            var rules = new List<Rule>
            {
                Rule.Create("GB", RuleType.VatRate, "UK VAT Rate", "basePrice * 0.20", DateTime.UtcNow),
                Rule.Create("GB", RuleType.Threshold, "UK Threshold", "transactionVolume > 1000 ? basePrice * 1.1 : basePrice", DateTime.UtcNow)
            };

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a CountryCode for 'GB'
            var gbCountryCode = CountryCode.Create("GB");

            // Arrange: Create a parameters dictionary with necessary values
            var parameters = new Dictionary<string, object>
            {
                { "basePrice", 1000 },
                { "transactionVolume", 1200 }
            };

            // Arrange: Set a current date for testing
            var currentDate = DateTime.UtcNow;

            // Act: Call ruleEngine.CalculateCountryCost(gbCountryCode, parameters, currentDate)
            var result = ruleEngine.CalculateCountryCost(gbCountryCode, parameters, currentDate);

            // Assert: Verify the result is a Money object with the correct amount and currency
            result.Should().BeOfType<Money>();
            result.Currency.Should().Be("GBP");

            // Assert: Verify the amount matches the expected calculation based on the rules
            result.Amount.Should().Be(1300); // 1000 * 0.20 + 1000 * 1.1 = 200 + 1100 = 1300
        }

        /// <summary>
        /// Tests that CalculateCountryCost correctly applies discount rules by subtracting from the total
        /// </summary>
        [Fact]
        public void CalculateCountryCost_WithDiscountRules_AppliesDiscountsCorrectly()
        {
            // Arrange: Create rules including discount rules for a country
            var rules = new List<Rule>
            {
                Rule.Create("GB", RuleType.VatRate, "UK VAT Rate", "basePrice * 0.20", DateTime.UtcNow),
                Rule.Create("GB", RuleType.Discount, "Volume Discount", "basePrice * 0.10", DateTime.UtcNow)
            };

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a CountryCode for 'GB'
            var gbCountryCode = CountryCode.Create("GB");

            // Arrange: Create a parameters dictionary with necessary values
            var parameters = new Dictionary<string, object> { { "basePrice", 1000 } };

            // Arrange: Set a current date for testing
            var currentDate = DateTime.UtcNow;

            // Act: Call ruleEngine.CalculateCountryCost(gbCountryCode, parameters, currentDate)
            var result = ruleEngine.CalculateCountryCost(gbCountryCode, parameters, currentDate);

            // Assert: Verify the result amount reflects the application of discount rules (subtracted from total)
            result.Amount.Should().Be(100); // 1000 * 0.20 - 1000 * 0.10 = 200 - 100 = 100
        }

        /// <summary>
        /// Tests that CalculateCountryCost returns zero when no applicable rules are found
        /// </summary>
        [Fact]
        public void CalculateCountryCost_WithNoApplicableRules_ReturnsZero()
        {
            // Arrange: Create rules for countries other than the test country
            var rules = new List<Rule> { Rule.Create("DE", RuleType.VatRate, "DE VAT Rate", "basePrice * 0.19", DateTime.UtcNow) };

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a CountryCode for a country with no rules
            var countryCode = CountryCode.Create("GB");

            // Arrange: Create a parameters dictionary
            var parameters = new Dictionary<string, object> { { "basePrice", 1000 } };

            // Arrange: Set a current date for testing
            var currentDate = DateTime.UtcNow;

            // Act: Call ruleEngine.CalculateCountryCost(countryCode, parameters, currentDate)
            var result = ruleEngine.CalculateCountryCost(countryCode, parameters, currentDate);

            // Assert: Verify the result is a Money object with zero amount
            result.Amount.Should().Be(0);
        }

        /// <summary>
        /// Tests a realistic scenario with multiple rule types and conditions
        /// </summary>
        [Fact]
        public void CalculateCountryCost_WithRealWorldScenario_CalculatesCorrectly()
        {
            // Arrange: Create a comprehensive set of rules mimicking a real-world VAT calculation scenario
            var rules = new List<Rule>
            {
                Rule.Create("GB", RuleType.VatRate, "UK Standard VAT Rate", "basePrice * 0.20", DateTime.UtcNow),
                Rule.Create("GB", RuleType.Threshold, "High Volume Discount", "transactionVolume > 1000 ? -basePrice * 0.05 : 0", DateTime.UtcNow),
                Rule.Create("GB", RuleType.Complexity, "Complex Filing Surcharge", "serviceType == 'ComplexFiling' ? basePrice * 0.10 : 0", DateTime.UtcNow)
            };

            // Arrange: Create a RuleEngine instance with the rules
            var ruleEngine = new RuleEngine(rules);

            // Arrange: Create a CountryCode for 'GB'
            var gbCountryCode = CountryCode.Create("GB");

            // Arrange: Create a detailed parameters dictionary with realistic values
            var parameters = new Dictionary<string, object>
            {
                { "basePrice", 1000 },
                { "transactionVolume", 1200 },
                { "serviceType", "ComplexFiling" }
            };

            // Arrange: Set a current date for testing
            var currentDate = DateTime.UtcNow;

            // Act: Call ruleEngine.CalculateCountryCost(gbCountryCode, parameters, currentDate)
            var result = ruleEngine.CalculateCountryCost(gbCountryCode, parameters, currentDate);

            // Assert: Verify the result matches the expected calculation for the complex scenario
            result.Amount.Should().Be(250); // 1000 * 0.20 - 1000 * 0.05 + 1000 * 0.10 = 200 - 50 + 100 = 250
        }
    }
}