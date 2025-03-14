using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using VatFilingPricingTool.Domain.Rules.Expressions;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.UnitTests.Domain
{
    /// <summary>
    /// Test suite for the ExpressionEvaluator class
    /// </summary>
    public class ExpressionEvaluatorTests
    {
        [Fact]
        public void Evaluate_WithSimpleExpression_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "2 + 3";
            var parameters = new Dictionary<string, object>();

            // Act
            decimal result = ExpressionEvaluator.Evaluate(expression, parameters);

            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public void Evaluate_WithParameterSubstitution_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "basePrice * vatRate";
            var parameters = new Dictionary<string, object>
            {
                { "basePrice", 1000m },
                { "vatRate", 0.20m }
            };

            // Act
            decimal result = ExpressionEvaluator.Evaluate(expression, parameters);

            // Assert
            result.Should().Be(200);
        }

        [Fact]
        public void Evaluate_WithMissingParameter_ThrowsValidationException()
        {
            // Arrange
            string expression = "basePrice * vatRate";
            var parameters = new Dictionary<string, object>
            {
                { "basePrice", 1000m }
                // vatRate is missing
            };

            // Act & Assert
            Action act = () => ExpressionEvaluator.Evaluate(expression, parameters);
            act.Should().Throw<DomainException>()
                .Where(e => e.ErrorCode == ErrorCodes.Rule.RuleValidationFailed)
                .WithMessage("*Parameter not found: vatRate*");
        }

        [Fact]
        public void Evaluate_WithInvalidExpression_ThrowsValidationException()
        {
            // Arrange
            string expression = "2 + * 3";
            var parameters = new Dictionary<string, object>();

            // Act & Assert
            Action act = () => ExpressionEvaluator.Evaluate(expression, parameters);
            act.Should().Throw<DomainException>()
                .Where(e => e.ErrorCode == ErrorCodes.Rule.InvalidRuleExpression);
        }

        [Fact]
        public void Evaluate_WithDivisionByZero_ThrowsValidationException()
        {
            // Arrange
            string expression = "5 / 0";
            var parameters = new Dictionary<string, object>();

            // Act & Assert
            Action act = () => ExpressionEvaluator.Evaluate(expression, parameters);
            act.Should().Throw<DomainException>()
                .Where(e => e.ErrorCode == ErrorCodes.Rule.InvalidRuleExpression)
                .WithMessage("Division by zero is not allowed");
        }

        [Fact]
        public void Evaluate_WithComplexExpression_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "(2 + 3) * 4 / 2";
            var parameters = new Dictionary<string, object>();

            // Act
            decimal result = ExpressionEvaluator.Evaluate(expression, parameters);

            // Assert
            result.Should().Be(10);
        }

        [Fact]
        public void Evaluate_WithNestedFunctions_ReturnsCorrectResult()
        {
            // Arrange
            string expression = "max(2, min(5, 3))";
            var parameters = new Dictionary<string, object>();

            // Act
            decimal result = ExpressionEvaluator.Evaluate(expression, parameters);

            // Assert
            result.Should().Be(3);
        }

        [Fact]
        public void Evaluate_WithConditionalFunction_ReturnsCorrectResult()
        {
            // Arrange
            // The 'if' function takes three arguments: condition, trueValue, falseValue
            // It returns trueValue if condition > 0, otherwise falseValue
            string expression = "if(transactionVolume - 100, basePrice * 1.5, basePrice)";
            var parameters = new Dictionary<string, object>
            {
                { "transactionVolume", 150 },
                { "basePrice", 1000m }
            };

            // Act
            decimal result = ExpressionEvaluator.Evaluate(expression, parameters);

            // Assert
            result.Should().Be(1500); // Since 150 - 100 = 50 > 0, it returns basePrice * 1.5
        }

        [Fact]
        public void Evaluate_WithMathFunctions_ReturnsCorrectResult()
        {
            // Arrange & Act & Assert
            // Test abs function
            ExpressionEvaluator.Evaluate("abs(-5)", new Dictionary<string, object>())
                .Should().Be(5);

            // Test round function
            ExpressionEvaluator.Evaluate("round(5.7)", new Dictionary<string, object>())
                .Should().Be(6);

            // Test floor function
            ExpressionEvaluator.Evaluate("floor(5.7)", new Dictionary<string, object>())
                .Should().Be(5);

            // Test ceiling function
            ExpressionEvaluator.Evaluate("ceiling(5.2)", new Dictionary<string, object>())
                .Should().Be(6);

            // Test sqrt function
            ExpressionEvaluator.Evaluate("sqrt(9)", new Dictionary<string, object>())
                .Should().Be(3);
        }

        [Fact]
        public void Evaluate_WithInvalidFunction_ThrowsValidationException()
        {
            // Arrange
            string expression = "unknown(5)";
            var parameters = new Dictionary<string, object>();

            // Act & Assert
            Action act = () => ExpressionEvaluator.Evaluate(expression, parameters);
            act.Should().Throw<DomainException>()
                .Where(e => e.ErrorCode == ErrorCodes.Rule.InvalidRuleExpression)
                .WithMessage("*Unknown function: unknown*");
        }

        [Fact]
        public void Evaluate_WithInvalidOperator_ThrowsValidationException()
        {
            // Arrange
            string expression = "5 @ 3";
            var parameters = new Dictionary<string, object>();

            // Act & Assert
            Action act = () => ExpressionEvaluator.Evaluate(expression, parameters);
            act.Should().Throw<DomainException>()
                .Where(e => e.ErrorCode == ErrorCodes.Rule.InvalidRuleExpression);
        }

        [Fact]
        public void Evaluate_WithNullExpression_ThrowsValidationException()
        {
            // Arrange
            string expression = null;
            var parameters = new Dictionary<string, object>();

            // Act & Assert
            Action act = () => ExpressionEvaluator.Evaluate(expression, parameters);
            act.Should().Throw<ValidationException>()
                .WithMessage("*Expression cannot be null or empty*");
        }

        [Fact]
        public void Evaluate_WithEmptyExpression_ThrowsValidationException()
        {
            // Arrange
            string expression = "";
            var parameters = new Dictionary<string, object>();

            // Act & Assert
            Action act = () => ExpressionEvaluator.Evaluate(expression, parameters);
            act.Should().Throw<ValidationException>()
                .WithMessage("*Expression cannot be null or empty*");
        }

        [Fact]
        public void Evaluate_WithNullParameters_ThrowsValidationException()
        {
            // Arrange
            string expression = "2 + 3";
            Dictionary<string, object> parameters = null;

            // Act & Assert
            // The method handles null parameters by creating an empty dictionary
            // so it should not throw an exception for this case
            Action act = () => ExpressionEvaluator.Evaluate(expression, parameters);
            act.Should().NotThrow<ValidationException>();
            ExpressionEvaluator.Evaluate(expression, parameters).Should().Be(5);
        }

        [Fact]
        public void ValidateExpression_WithValidExpression_ReturnsTrue()
        {
            // Arrange
            string expression = "2 + 3 * 4";

            // Act
            bool isValid = ExpressionEvaluator.ValidateExpression(expression);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateExpression_WithInvalidExpression_ThrowsValidationException()
        {
            // Arrange
            string expression = "2 + * 3";

            // Act & Assert
            Action act = () => ExpressionEvaluator.ValidateExpression(expression);
            act.Should().Throw<DomainException>()
                .Where(e => e.ErrorCode == ErrorCodes.Rule.InvalidRuleExpression);
        }

        [Fact]
        public void Evaluate_WithRealWorldVatRuleExpressions_ReturnsCorrectResults()
        {
            // Arrange
            // Standard VAT rate application (e.g., 20% of base price)
            string vatRateExpression = "basePrice * 0.20";
            var vatRateParams = new Dictionary<string, object>
            {
                { "basePrice", 1000m }
            };

            // Volume-based pricing 
            // If transactionVolume > 100, apply 1.5x multiplier, otherwise use base price
            string volumeBasedExpression = "if(transactionVolume - 100, basePrice * 1.5, basePrice)";
            var volumeBasedParams = new Dictionary<string, object>
            {
                { "transactionVolume", 150 },
                { "basePrice", 1000m }
            };

            // Multi-country discount
            // If countriesCount > 2, apply 10% discount, otherwise use base price
            string discountExpression = "if(countriesCount - 2, basePrice * 0.9, basePrice)";
            var discountParams = new Dictionary<string, object>
            {
                { "countriesCount", 3 },
                { "basePrice", 1000m }
            };

            // Act
            decimal vatRateResult = ExpressionEvaluator.Evaluate(vatRateExpression, vatRateParams);
            decimal volumeBasedResult = ExpressionEvaluator.Evaluate(volumeBasedExpression, volumeBasedParams);
            decimal discountResult = ExpressionEvaluator.Evaluate(discountExpression, discountParams);

            // Assert
            vatRateResult.Should().Be(200);
            volumeBasedResult.Should().Be(1500);
            discountResult.Should().Be(900);
        }

        [Fact]
        public void Evaluate_WithParameterTypeConversion_HandlesTypesCorrectly()
        {
            // Arrange
            string expression = "intValue + decimalValue + stringNumber + boolValue";
            var parameters = new Dictionary<string, object>
            {
                { "intValue", 10 },
                { "decimalValue", 20.5m },
                { "stringNumber", "30.5" },
                { "boolValue", true }
            };

            // Act
            decimal result = ExpressionEvaluator.Evaluate(expression, parameters);

            // Assert
            // 10 + 20.5 + 30.5 + 1 = 62
            result.Should().Be(62);
        }
    }
}