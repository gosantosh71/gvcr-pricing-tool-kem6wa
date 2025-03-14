using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Constants;
using VatFilingPricingTool.Domain.Exceptions;

namespace VatFilingPricingTool.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a monetary amount with currency information.
    /// This immutable object ensures that money is correctly represented with proper validation.
    /// </summary>
    public class Money
    {
        /// <summary>
        /// Gets the monetary amount.
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// Gets the currency code (ISO 4217).
        /// </summary>
        public string Currency { get; }

        /// <summary>
        /// Creates a new Money instance with validation.
        /// </summary>
        /// <param name="amount">The monetary amount.</param>
        /// <param name="currency">The currency code (ISO 4217).</param>
        /// <exception cref="ValidationException">Thrown when amount is negative or currency is invalid.</exception>
        private Money(decimal amount, string currency)
        {
            Validate(amount, currency);
            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }

        /// <summary>
        /// Factory method to create a new Money instance.
        /// </summary>
        /// <param name="amount">The monetary amount.</param>
        /// <param name="currency">The currency code (ISO 4217).</param>
        /// <returns>A new Money instance.</returns>
        public static Money Create(decimal amount, string currency)
        {
            return new Money(amount, currency);
        }

        /// <summary>
        /// Factory method to create a new Money instance with zero amount.
        /// </summary>
        /// <param name="currency">The currency code (ISO 4217). If null or empty, the default currency will be used.</param>
        /// <returns>A new Money instance with zero amount.</returns>
        public static Money CreateZero(string currency = null)
        {
            if (string.IsNullOrEmpty(currency))
            {
                currency = DomainConstants.Defaults.DefaultCurrency;
            }

            return new Money(0, currency);
        }

        /// <summary>
        /// Factory method to create a new Money instance with the default currency.
        /// </summary>
        /// <param name="amount">The monetary amount.</param>
        /// <returns>A new Money instance with the default currency.</returns>
        public static Money CreateDefault(decimal amount)
        {
            return new Money(amount, DomainConstants.Defaults.DefaultCurrency);
        }

        /// <summary>
        /// Validates the money amount and currency.
        /// </summary>
        /// <param name="amount">The monetary amount to validate.</param>
        /// <param name="currency">The currency code to validate.</param>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
        private static void Validate(decimal amount, string currency)
        {
            var errors = new List<string>();

            if (amount < 0)
            {
                errors.Add("Money amount cannot be negative.");
            }

            if (string.IsNullOrEmpty(currency))
            {
                errors.Add("Currency code is required.");
            }
            else
            {
                if (currency.Length != DomainConstants.Validation.CurrencyCodeLength)
                {
                    errors.Add($"Currency code must be {DomainConstants.Validation.CurrencyCodeLength} characters.");
                }

                // Check if the currency code consists of 3 letters (ISO 4217 format)
                if (!Regex.IsMatch(currency, DomainConstants.Validation.CurrencyCodePattern, RegexOptions.IgnoreCase))
                {
                    errors.Add("Currency code must be in ISO 4217 format (three uppercase letters).");
                }
            }

            if (errors.Count > 0)
            {
                throw new ValidationException("Money validation failed.", errors);
            }
        }

        /// <summary>
        /// Adds another Money value to this one.
        /// </summary>
        /// <param name="other">The Money value to add.</param>
        /// <returns>A new Money instance with the sum of the amounts.</returns>
        /// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
        /// <exception cref="ValidationException">Thrown when currencies don't match.</exception>
        public Money Add(Money other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (Currency != other.Currency)
            {
                throw new ValidationException("Cannot add money with different currencies.", 
                    new List<string> { $"Currency mismatch: {Currency} and {other.Currency}" });
            }

            return new Money(Amount + other.Amount, Currency);
        }

        /// <summary>
        /// Subtracts another Money value from this one.
        /// </summary>
        /// <param name="other">The Money value to subtract.</param>
        /// <returns>A new Money instance with the difference of the amounts.</returns>
        /// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
        /// <exception cref="ValidationException">Thrown when currencies don't match or result would be negative.</exception>
        public Money Subtract(Money other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (Currency != other.Currency)
            {
                throw new ValidationException("Cannot subtract money with different currencies.", 
                    new List<string> { $"Currency mismatch: {Currency} and {other.Currency}" });
            }

            decimal result = Amount - other.Amount;
            if (result < 0)
            {
                throw new ValidationException("Money amount cannot be negative after subtraction.", 
                    new List<string> { $"Subtraction would result in negative amount: {Amount} - {other.Amount} = {result}" });
            }

            return new Money(result, Currency);
        }

        /// <summary>
        /// Multiplies the money amount by a factor.
        /// </summary>
        /// <param name="factor">The multiplication factor.</param>
        /// <returns>A new Money instance with the multiplied amount.</returns>
        /// <exception cref="ValidationException">Thrown when the result would be negative.</exception>
        public Money Multiply(decimal factor)
        {
            decimal result = Amount * factor;
            if (result < 0)
            {
                throw new ValidationException("Money amount cannot be negative after multiplication.", 
                    new List<string> { $"Multiplication would result in negative amount: {Amount} * {factor} = {result}" });
            }

            return new Money(result, Currency);
        }

        /// <summary>
        /// Divides the money amount by a divisor.
        /// </summary>
        /// <param name="divisor">The division factor.</param>
        /// <returns>A new Money instance with the divided amount.</returns>
        /// <exception cref="DivideByZeroException">Thrown when divisor is zero.</exception>
        /// <exception cref="ValidationException">Thrown when the result would be negative.</exception>
        public Money Divide(decimal divisor)
        {
            if (divisor == 0)
            {
                throw new DivideByZeroException("Cannot divide money by zero.");
            }

            decimal result = Amount / divisor;
            if (result < 0)
            {
                throw new ValidationException("Money amount cannot be negative after division.", 
                    new List<string> { $"Division would result in negative amount: {Amount} / {divisor} = {result}" });
            }

            return new Money(result, Currency);
        }

        /// <summary>
        /// Applies a percentage discount to the money amount.
        /// </summary>
        /// <param name="discountPercentage">The discount percentage (0-100).</param>
        /// <returns>A new Money instance with the discounted amount.</returns>
        /// <exception cref="ArgumentException">Thrown when discount percentage is invalid.</exception>
        public Money ApplyDiscount(decimal discountPercentage)
        {
            if (discountPercentage < 0)
            {
                throw new ArgumentException("Discount percentage cannot be negative.", nameof(discountPercentage));
            }

            if (discountPercentage > 100)
            {
                throw new ArgumentException("Discount percentage cannot exceed 100%.", nameof(discountPercentage));
            }

            decimal discountFactor = 1 - (discountPercentage / 100);
            return new Money(Amount * discountFactor, Currency);
        }

        /// <summary>
        /// Applies a VAT rate to the money amount.
        /// </summary>
        /// <param name="vatRate">The VAT rate percentage.</param>
        /// <returns>A new Money instance with the VAT amount.</returns>
        /// <exception cref="ArgumentException">Thrown when VAT rate is negative.</exception>
        public Money ApplyVatRate(decimal vatRate)
        {
            if (vatRate < 0)
            {
                throw new ArgumentException("VAT rate cannot be negative.", nameof(vatRate));
            }

            decimal vatAmount = Amount * (vatRate / 100);
            return new Money(vatAmount, Currency);
        }

        /// <summary>
        /// Adds VAT to the money amount.
        /// </summary>
        /// <param name="vatRate">The VAT rate percentage.</param>
        /// <returns>A new Money instance with the amount including VAT.</returns>
        public Money AddVat(decimal vatRate)
        {
            Money vatAmount = ApplyVatRate(vatRate);
            return Add(vatAmount);
        }

        /// <summary>
        /// Returns the string representation of the money value.
        /// </summary>
        /// <returns>The formatted money value with currency.</returns>
        public override string ToString()
        {
            // Try to get a CultureInfo associated with the currency
            try
            {
                var culture = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                    .FirstOrDefault(c => {
                        try {
                            return new RegionInfo(c.Name).ISOCurrencySymbol == Currency;
                        }
                        catch {
                            return false;
                        }
                    });

                if (culture != null)
                {
                    return Amount.ToString("C", culture);
                }
            }
            catch
            {
                // Ignore and use the fallback format
            }

            // Fallback to a simple format
            return $"{Amount} {Currency}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (obj is not Money money)
            {
                return false;
            }

            return Amount == money.Amount && Currency == money.Currency;
        }

        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        /// <summary>
        /// Equality operator for comparing two Money instances.
        /// </summary>
        /// <param name="left">The left Money instance.</param>
        /// <param name="right">The right Money instance.</param>
        /// <returns>true if the money values are equal, otherwise false.</returns>
        public static bool operator ==(Money left, Money right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for comparing two Money instances.
        /// </summary>
        /// <param name="left">The left Money instance.</param>
        /// <param name="right">The right Money instance.</param>
        /// <returns>true if the money values are not equal, otherwise false.</returns>
        public static bool operator !=(Money left, Money right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Addition operator for adding two Money instances.
        /// </summary>
        /// <param name="left">The left Money instance.</param>
        /// <param name="right">The right Money instance.</param>
        /// <returns>A new Money instance with the sum of the amounts.</returns>
        public static Money operator +(Money left, Money right)
        {
            if (left is null || right is null)
            {
                throw new ArgumentNullException(left is null ? nameof(left) : nameof(right));
            }

            return left.Add(right);
        }

        /// <summary>
        /// Subtraction operator for subtracting two Money instances.
        /// </summary>
        /// <param name="left">The left Money instance.</param>
        /// <param name="right">The right Money instance.</param>
        /// <returns>A new Money instance with the difference of the amounts.</returns>
        public static Money operator -(Money left, Money right)
        {
            if (left is null || right is null)
            {
                throw new ArgumentNullException(left is null ? nameof(left) : nameof(right));
            }

            return left.Subtract(right);
        }

        /// <summary>
        /// Multiplication operator for multiplying a Money instance by a factor.
        /// </summary>
        /// <param name="money">The Money instance.</param>
        /// <param name="factor">The multiplication factor.</param>
        /// <returns>A new Money instance with the multiplied amount.</returns>
        public static Money operator *(Money money, decimal factor)
        {
            if (money is null)
            {
                throw new ArgumentNullException(nameof(money));
            }

            return money.Multiply(factor);
        }

        /// <summary>
        /// Division operator for dividing a Money instance by a divisor.
        /// </summary>
        /// <param name="money">The Money instance.</param>
        /// <param name="divisor">The division factor.</param>
        /// <returns>A new Money instance with the divided amount.</returns>
        public static Money operator /(Money money, decimal divisor)
        {
            if (money is null)
            {
                throw new ArgumentNullException(nameof(money));
            }

            return money.Divide(divisor);
        }
    }
}