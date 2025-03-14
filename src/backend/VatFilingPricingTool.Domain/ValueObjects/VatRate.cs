using System; // Version 6.0.0 - Core .NET functionality
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Constants;

namespace VatFilingPricingTool.Domain.ValueObjects
{
    /// <summary>
    /// Represents a VAT (Value Added Tax) rate as an immutable value object.
    /// Provides validation to ensure the rate is within acceptable ranges and
    /// helper methods for common VAT rate scenarios.
    /// </summary>
    public class VatRate
    {
        /// <summary>
        /// Gets the numeric value of the VAT rate as a decimal percentage.
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VatRate"/> class.
        /// </summary>
        /// <param name="value">The decimal percentage value of the VAT rate.</param>
        /// <exception cref="ValidationException">Thrown when the VAT rate is outside the acceptable range.</exception>
        private VatRate(decimal value)
        {
            Validate(value);
            Value = value;
        }

        /// <summary>
        /// Creates a new VatRate instance with validation.
        /// </summary>
        /// <param name="value">The decimal percentage value of the VAT rate.</param>
        /// <returns>A new VatRate instance.</returns>
        /// <exception cref="ValidationException">Thrown when the VAT rate is outside the acceptable range.</exception>
        public static VatRate Create(decimal value)
        {
            return new VatRate(value);
        }

        /// <summary>
        /// Creates a new VatRate instance with zero rate (for zero-rated items).
        /// </summary>
        /// <returns>A new VatRate instance with zero rate.</returns>
        public static VatRate CreateZero()
        {
            return new VatRate(0);
        }

        /// <summary>
        /// Creates a new VatRate instance with a standard rate (typically 20%).
        /// </summary>
        /// <returns>A new VatRate instance with standard rate.</returns>
        public static VatRate CreateStandard()
        {
            return new VatRate(20.0m);
        }

        /// <summary>
        /// Validates that a VAT rate value is within acceptable range.
        /// </summary>
        /// <param name="value">The decimal percentage value of the VAT rate.</param>
        /// <exception cref="ValidationException">Thrown when the VAT rate is outside the acceptable range.</exception>
        private static void Validate(decimal value)
        {
            if (value < DomainConstants.Validation.MinVatRate)
            {
                throw new ValidationException($"VAT rate cannot be less than {DomainConstants.Validation.MinVatRate}%.");
            }

            if (value > DomainConstants.Validation.MaxVatRate)
            {
                throw new ValidationException($"VAT rate cannot be greater than {DomainConstants.Validation.MaxVatRate}%.");
            }
        }

        /// <summary>
        /// Returns a string representation of the VAT rate.
        /// </summary>
        /// <returns>The VAT rate formatted as a percentage string.</returns>
        public override string ToString()
        {
            return $"{Value:F2}%";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (!(obj is VatRate vatRate))
                return false;

            return Value == vatRate.Value;
        }

        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Equality operator for comparing two VatRate instances.
        /// </summary>
        /// <param name="left">The left VAT rate.</param>
        /// <param name="right">The right VAT rate.</param>
        /// <returns>true if the VAT rates are equal, otherwise false.</returns>
        public static bool operator ==(VatRate left, VatRate right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for comparing two VatRate instances.
        /// </summary>
        /// <param name="left">The left VAT rate.</param>
        /// <param name="right">The right VAT rate.</param>
        /// <returns>true if the VAT rates are not equal, otherwise false.</returns>
        public static bool operator !=(VatRate left, VatRate right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implicitly converts a VatRate to a decimal.
        /// </summary>
        /// <param name="vatRate">The VAT rate to convert.</param>
        /// <returns>The decimal value of the VAT rate.</returns>
        public static implicit operator decimal(VatRate vatRate)
        {
            return vatRate?.Value ?? 0;
        }

        /// <summary>
        /// Explicitly converts a decimal to a VatRate.
        /// </summary>
        /// <param name="value">The decimal value to convert.</param>
        /// <returns>A new VatRate instance.</returns>
        public static explicit operator VatRate(decimal value)
        {
            return new VatRate(value);
        }

        /// <summary>
        /// Checks if the VAT rate is zero (zero-rated).
        /// </summary>
        /// <returns>True if the VAT rate is zero, otherwise false.</returns>
        public bool IsZeroRated()
        {
            return Value == 0;
        }

        /// <summary>
        /// Checks if the VAT rate is the standard rate (typically 20%).
        /// </summary>
        /// <returns>True if the VAT rate is the standard rate, otherwise false.</returns>
        public bool IsStandardRate()
        {
            return Value == 20.0m;
        }

        /// <summary>
        /// Checks if the VAT rate is a reduced rate (typically between 5% and 15%).
        /// </summary>
        /// <returns>True if the VAT rate is a reduced rate, otherwise false.</returns>
        public bool IsReducedRate()
        {
            return Value > 0 && Value < 20.0m;
        }
    }
}