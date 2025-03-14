using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Domain.Rules.Expressions
{
    /// <summary>
    /// Evaluates mathematical expressions in postfix notation with parameter substitution.
    /// Used by the VAT rule calculation engine to apply dynamic pricing rules.
    /// </summary>
    public static class ExpressionEvaluator
    {
        /// <summary>
        /// Evaluates a mathematical expression with the provided parameters.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>Result of the expression evaluation.</returns>
        public static decimal Evaluate(string expression, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ValidationException("Expression cannot be null or empty",
                    new List<string> { "Expression is required" });
            }

            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            var tokens = ExpressionParser.Parse(expression);
            return EvaluatePostfix(tokens, parameters);
        }

        /// <summary>
        /// Evaluates a postfix expression represented as a list of tokens.
        /// </summary>
        /// <param name="tokens">The tokens in postfix notation.</param>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>Result of the expression evaluation.</returns>
        private static decimal EvaluatePostfix(List<ExpressionToken> tokens, Dictionary<string, object> parameters)
        {
            var stack = new Stack<decimal>();

            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                        if (decimal.TryParse(token.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
                        {
                            stack.Push(value);
                        }
                        else
                        {
                            throw new ValidationException($"Invalid number format: {token.Value}",
                                new List<string> { $"'{token.Value}' is not a valid numeric value" });
                        }
                        break;

                    case TokenType.Variable:
                        stack.Push(GetParameterValue(token.Value, parameters));
                        break;

                    case TokenType.Operator:
                        if (stack.Count < 2)
                        {
                            throw new DomainException("Insufficient operands for operator",
                                ErrorCodes.Rule.InvalidRuleExpression);
                        }

                        var operand2 = stack.Pop();
                        var operand1 = stack.Pop();
                        stack.Push(ApplyOperator(token.Value, operand1, operand2));
                        break;

                    case TokenType.Function:
                        var functionName = token.Value.ToLowerInvariant();
                        var argCount = GetFunctionArgumentCount(functionName);
                        
                        if (stack.Count < argCount)
                        {
                            throw new DomainException($"Insufficient arguments for function '{token.Value}'",
                                ErrorCodes.Rule.InvalidRuleExpression);
                        }

                        var args = new List<decimal>();
                        for (int i = 0; i < argCount; i++)
                        {
                            args.Insert(0, stack.Pop()); // Insert at beginning to maintain order
                        }

                        stack.Push(ApplyFunction(functionName, args));
                        break;

                    default:
                        throw new DomainException($"Unexpected token type in evaluation: {token.Type}",
                            ErrorCodes.Rule.InvalidRuleExpression);
                }
            }

            if (stack.Count != 1)
            {
                throw new DomainException("Invalid expression: does not evaluate to a single result",
                    ErrorCodes.Rule.InvalidRuleExpression);
            }

            return stack.Pop();
        }

        /// <summary>
        /// Applies an arithmetic operator to operands.
        /// </summary>
        /// <param name="operator">The operator.</param>
        /// <param name="operand1">First operand.</param>
        /// <param name="operand2">Second operand.</param>
        /// <returns>Result of the operation.</returns>
        private static decimal ApplyOperator(string @operator, decimal operand1, decimal operand2)
        {
            switch (@operator)
            {
                case "+":
                    return operand1 + operand2;
                case "-":
                    return operand1 - operand2;
                case "*":
                    return operand1 * operand2;
                case "/":
                    if (operand2 == 0)
                    {
                        throw new DomainException("Division by zero is not allowed",
                            ErrorCodes.Rule.InvalidRuleExpression);
                    }
                    return operand1 / operand2;
                case "^":
                    return (decimal)Math.Pow((double)operand1, (double)operand2);
                default:
                    throw new DomainException($"Unknown operator: {@operator}",
                        ErrorCodes.Rule.InvalidRuleExpression);
            }
        }

        /// <summary>
        /// Applies a mathematical function to arguments.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="arguments">List of arguments.</param>
        /// <returns>Result of the function application.</returns>
        private static decimal ApplyFunction(string functionName, List<decimal> arguments)
        {
            switch (functionName)
            {
                case "min":
                    return arguments.Min();
                
                case "max":
                    return arguments.Max();
                
                case "abs":
                    return Math.Abs(arguments[0]);
                
                case "round":
                    return Math.Round(arguments[0]);
                
                case "floor":
                    return Math.Floor(arguments[0]);
                
                case "ceiling":
                    return Math.Ceiling(arguments[0]);
                
                case "sqrt":
                    if (arguments[0] < 0)
                    {
                        throw new DomainException("Cannot calculate square root of a negative number",
                            ErrorCodes.Rule.InvalidRuleExpression);
                    }
                    return (decimal)Math.Sqrt((double)arguments[0]);
                
                case "if":
                    // If condition (arg[0]) is greater than 0, return trueValue (arg[1]), else return falseValue (arg[2])
                    return arguments[0] > 0 ? arguments[1] : arguments[2];
                
                default:
                    throw new DomainException($"Unknown function: {functionName}",
                        ErrorCodes.Rule.InvalidRuleExpression);
            }
        }

        /// <summary>
        /// Gets a parameter value from the parameters dictionary and converts it to decimal.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameters">Dictionary of parameter names and values.</param>
        /// <returns>Parameter value converted to decimal.</returns>
        private static decimal GetParameterValue(string parameterName, Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(parameterName, out object value))
            {
                throw new DomainException($"Parameter not found: {parameterName}",
                    ErrorCodes.Rule.RuleValidationFailed);
            }

            return ConvertToDecimal(value);
        }

        /// <summary>
        /// Converts a value to decimal for calculation.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value converted to decimal.</returns>
        private static decimal ConvertToDecimal(object value)
        {
            if (value == null)
            {
                return 0;
            }

            if (value is decimal decimalValue)
            {
                return decimalValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return longValue;
            }

            if (value is double doubleValue)
            {
                return (decimal)doubleValue;
            }

            if (value is float floatValue)
            {
                return (decimal)floatValue;
            }

            if (value is string stringValue)
            {
                if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }
            }

            if (value is bool boolValue)
            {
                return boolValue ? 1 : 0;
            }

            throw new DomainException($"Cannot convert value to decimal: {value}",
                ErrorCodes.Pricing.InvalidNumericConversion);
        }

        /// <summary>
        /// Determines the number of arguments a function expects.
        /// </summary>
        /// <param name="functionName">Name of the function (lowercase).</param>
        /// <returns>Number of arguments the function expects.</returns>
        private static int GetFunctionArgumentCount(string functionName)
        {
            switch (functionName)
            {
                case "min":
                case "max":
                    return 2;
                case "abs":
                case "round":
                case "floor":
                case "ceiling":
                case "sqrt":
                    return 1;
                case "if":
                    return 3; // condition, trueValue, falseValue
                default:
                    throw new DomainException($"Unknown function: {functionName}",
                        ErrorCodes.Rule.InvalidRuleExpression);
            }
        }

        /// <summary>
        /// Validates that an expression can be evaluated.
        /// </summary>
        /// <param name="expression">The expression to validate.</param>
        /// <returns>True if the expression is valid, otherwise throws an exception.</returns>
        public static bool ValidateExpression(string expression)
        {
            return ExpressionParser.ValidateExpression(expression);
        }
    }
}