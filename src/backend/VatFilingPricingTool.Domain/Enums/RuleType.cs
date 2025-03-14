using System;

namespace VatFilingPricingTool.Domain.Enums
{
    /// <summary>
    /// Defines the types of VAT rules that can be applied during pricing calculations.
    /// These rule types determine how rules are applied during pricing calculations
    /// and help organize rules by their purpose and behavior.
    /// </summary>
    public enum RuleType
    {
        /// <summary>
        /// Represents rules related to standard VAT rates applied to calculations.
        /// These rules determine the base tax rate for a particular country or jurisdiction.
        /// </summary>
        VatRate = 1,
        
        /// <summary>
        /// Represents threshold-based rules that apply when certain volume or value conditions are met.
        /// These rules typically define pricing tiers based on transaction volumes or invoice counts.
        /// </summary>
        Threshold = 2,
        
        /// <summary>
        /// Represents rules that adjust pricing based on the complexity of filings.
        /// These rules factor in the complexity of VAT calculations for specific scenarios or jurisdictions.
        /// </summary>
        Complexity = 3,
        
        /// <summary>
        /// Represents special requirements or conditions specific to certain jurisdictions.
        /// These rules cover unique filing requirements that may incur additional costs.
        /// </summary>
        SpecialRequirement = 4,
        
        /// <summary>
        /// Represents discount rules that can reduce the final price under certain conditions.
        /// These rules may apply for volume discounts, multi-country filings, or loyalty benefits.
        /// </summary>
        Discount = 5
    }
}