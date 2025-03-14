using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Domain.Rules.Expressions
{
    /// <summary>
    /// Enum representing the types of tokens in an expression
    /// </summary>
    public enum TokenType
    {
        Number,
        Variable,
        Operator,
        Function,
        LeftParenthesis,
        RightParenthesis,
        Comma
    }

    /// <summary>
    /// Represents a token in a mathematical expression
    /// </summary>
    public class ExpressionToken
    {
        /// <summary>
        /// Gets the value of the token
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the type of the token
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// Creates a new instance of ExpressionToken
        /// </summary>
        /// <param name="value">The token value</param>
        /// <param name="type">The token type</param>
        public ExpressionToken(string value, TokenType type)
        {
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Returns a string representation of the token
        /// </summary>
        /// <returns>String representation of the token</returns>
        public override string ToString()
        {
            return $"[{Type}: {Value}]";
        }
    }

    /// <summary>
    /// Parses mathematical expressions into tokens and converts from infix to postfix notation
    /// </summary>
    public static class ExpressionParser
    {
        /// <summary>
        /// Parses a string expression into tokens in postfix notation
        /// </summary>
        /// <param name="expression">The expression to parse</param>
        /// <returns>List of tokens in postfix notation (Reverse Polish Notation)</returns>
        public static List<ExpressionToken> Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ValidationException("Expression cannot be null or empty", 
                    new List<string> { "Expression is required" });
            }

            var tokens = Tokenize(expression);
            return ConvertToPostfix(tokens);
        }

        /// <summary>
        /// Converts a string expression into a list of tokens
        /// </summary>
        /// <param name="expression">The expression to tokenize</param>
        /// <returns>List of tokens in infix notation</returns>
        private static List<ExpressionToken> Tokenize(string expression)
        {
            var tokens = new List<ExpressionToken>();
            var currentToken = new StringBuilder();
            var currentState = TokenType.Number; // Default initial state

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                // Skip whitespace
                if (char.IsWhiteSpace(c))
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(new ExpressionToken(currentToken.ToString(), currentState));
                        currentToken.Clear();
                    }
                    continue;
                }

                // Process digits and decimal point
                if (IsDigit(c))
                {
                    if (currentState != TokenType.Number && currentToken.Length > 0)
                    {
                        tokens.Add(new ExpressionToken(currentToken.ToString(), currentState));
                        currentToken.Clear();
                    }
                    currentState = TokenType.Number;
                    currentToken.Append(c);
                    continue;
                }

                // Process operators
                if (IsOperator(c))
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(new ExpressionToken(currentToken.ToString(), currentState));
                        currentToken.Clear();
                    }
                    tokens.Add(new ExpressionToken(c.ToString(), TokenType.Operator));
                    continue;
                }

                // Process parentheses
                if (c == '(')
                {
                    if (currentToken.Length > 0)
                    {
                        // If we have a token before left parenthesis, it's a function
                        if (currentState == TokenType.Variable)
                        {
                            tokens.Add(new ExpressionToken(currentToken.ToString(), TokenType.Function));
                        }
                        else
                        {
                            tokens.Add(new ExpressionToken(currentToken.ToString(), currentState));
                        }
                        currentToken.Clear();
                    }
                    tokens.Add(new ExpressionToken("(", TokenType.LeftParenthesis));
                    continue;
                }

                if (c == ')')
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(new ExpressionToken(currentToken.ToString(), currentState));
                        currentToken.Clear();
                    }
                    tokens.Add(new ExpressionToken(")", TokenType.RightParenthesis));
                    continue;
                }

                // Process comma (for function arguments)
                if (c == ',')
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(new ExpressionToken(currentToken.ToString(), currentState));
                        currentToken.Clear();
                    }
                    tokens.Add(new ExpressionToken(",", TokenType.Comma));
                    continue;
                }

                // Process letters (variables or functions)
                if (IsLetter(c))
                {
                    if (currentState != TokenType.Variable && currentToken.Length > 0)
                    {
                        tokens.Add(new ExpressionToken(currentToken.ToString(), currentState));
                        currentToken.Clear();
                    }
                    currentState = TokenType.Variable;
                    currentToken.Append(c);
                }
                else
                {
                    throw new DomainException($"Unexpected character in expression: {c}", 
                        ErrorCodes.Rule.InvalidRuleExpression);
                }
            }

            // Add the last token if there is one
            if (currentToken.Length > 0)
            {
                tokens.Add(new ExpressionToken(currentToken.ToString(), currentState));
            }

            return tokens;
        }

        /// <summary>
        /// Converts tokens from infix to postfix notation using the Shunting Yard algorithm
        /// </summary>
        /// <param name="infixTokens">List of tokens in infix notation</param>
        /// <returns>List of tokens in postfix notation</returns>
        private static List<ExpressionToken> ConvertToPostfix(List<ExpressionToken> infixTokens)
        {
            var output = new List<ExpressionToken>();
            var operatorStack = new Stack<ExpressionToken>();

            foreach (var token in infixTokens)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                    case TokenType.Variable:
                        output.Add(token);
                        break;

                    case TokenType.Function:
                        operatorStack.Push(token);
                        break;

                    case TokenType.Comma:
                        // Pop operators until we find a left parenthesis
                        while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.LeftParenthesis)
                        {
                            output.Add(operatorStack.Pop());
                        }
                        
                        if (operatorStack.Count == 0 || operatorStack.Peek().Type != TokenType.LeftParenthesis)
                        {
                            throw new DomainException("Misplaced comma or mismatched parentheses", 
                                ErrorCodes.Rule.InvalidRuleExpression);
                        }
                        break;

                    case TokenType.Operator:
                        while (operatorStack.Count > 0 &&
                               (operatorStack.Peek().Type == TokenType.Operator || operatorStack.Peek().Type == TokenType.Function) &&
                               ((IsLeftAssociative(token.Value) && GetOperatorPrecedence(token.Value) <= GetOperatorPrecedence(operatorStack.Peek().Value)) ||
                                (!IsLeftAssociative(token.Value) && GetOperatorPrecedence(token.Value) < GetOperatorPrecedence(operatorStack.Peek().Value))))
                        {
                            output.Add(operatorStack.Pop());
                        }
                        operatorStack.Push(token);
                        break;

                    case TokenType.LeftParenthesis:
                        operatorStack.Push(token);
                        break;

                    case TokenType.RightParenthesis:
                        while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.LeftParenthesis)
                        {
                            output.Add(operatorStack.Pop());
                        }

                        if (operatorStack.Count == 0)
                        {
                            throw new DomainException("Mismatched parentheses", 
                                ErrorCodes.Rule.InvalidRuleExpression);
                        }

                        // Discard the left parenthesis
                        operatorStack.Pop();

                        // If the token at the top of the stack is a function token, pop it onto the output queue
                        if (operatorStack.Count > 0 && operatorStack.Peek().Type == TokenType.Function)
                        {
                            output.Add(operatorStack.Pop());
                        }
                        break;
                }
            }

            // Pop any remaining operators from the stack to the output
            while (operatorStack.Count > 0)
            {
                var op = operatorStack.Pop();
                if (op.Type == TokenType.LeftParenthesis)
                {
                    throw new DomainException("Mismatched parentheses", 
                        ErrorCodes.Rule.InvalidRuleExpression);
                }
                output.Add(op);
            }

            return output;
        }

        /// <summary>
        /// Gets the precedence level of an operator
        /// </summary>
        /// <param name="operator">The operator</param>
        /// <returns>Precedence level (higher number means higher precedence)</returns>
        private static int GetOperatorPrecedence(string @operator)
        {
            switch (@operator)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                    return 2;
                case "^":
                    return 3;
                default:
                    return 0; // Functions or unknown operators
            }
        }

        /// <summary>
        /// Determines if an operator is left associative
        /// </summary>
        /// <param name="operator">The operator</param>
        /// <returns>True if the operator is left associative, false otherwise</returns>
        private static bool IsLeftAssociative(string @operator)
        {
            // Most operators are left associative
            return @operator != "^"; // Power is right associative
        }

        /// <summary>
        /// Checks if a character is an operator
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>True if the character is an operator, false otherwise</returns>
        private static bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' || c == '^';
        }

        /// <summary>
        /// Checks if a character is a digit or decimal point
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>True if the character is a digit or decimal point, false otherwise</returns>
        private static bool IsDigit(char c)
        {
            return char.IsDigit(c) || c == '.';
        }

        /// <summary>
        /// Checks if a character is a letter or underscore
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>True if the character is a letter or underscore, false otherwise</returns>
        private static bool IsLetter(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        /// <summary>
        /// Validates that an expression can be parsed
        /// </summary>
        /// <param name="expression">The expression to validate</param>
        /// <returns>True if the expression is valid, otherwise throws an exception</returns>
        public static bool ValidateExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ValidationException("Expression cannot be null or empty", 
                    new List<string> { "Expression is required" });
            }

            try
            {
                var tokens = Tokenize(expression);
                ConvertToPostfix(tokens);
                return true;
            }
            catch (Exception ex)
            {
                // Use DomainException with specific error code for parsing errors
                throw new DomainException($"Invalid expression format: {ex.Message}", 
                    ErrorCodes.Rule.InvalidRuleExpression, ex);
            }
        }
    }
}