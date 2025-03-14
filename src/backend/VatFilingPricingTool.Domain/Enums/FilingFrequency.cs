using System; // System v6.0.0

namespace VatFilingPricingTool.Domain.Enums
{
    /// <summary>
    /// Represents the frequency at which businesses are required to submit VAT returns to tax authorities.
    /// This is a core domain concept used in pricing calculations throughout the VAT Filing Pricing Tool.
    /// </summary>
    public enum FilingFrequency
    {
        /// <summary>
        /// VAT returns must be submitted every month.
        /// This is typically required for larger businesses or in countries with stricter reporting requirements.
        /// </summary>
        Monthly = 1,

        /// <summary>
        /// VAT returns must be submitted every three months.
        /// This is the most common filing frequency in many jurisdictions.
        /// </summary>
        Quarterly = 2,

        /// <summary>
        /// VAT returns must be submitted once per year.
        /// This is typically allowed for smaller businesses with lower transaction volumes.
        /// </summary>
        Annually = 3
    }
}