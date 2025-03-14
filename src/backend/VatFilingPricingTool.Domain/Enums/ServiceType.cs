using System; // Version 6.0.0

namespace VatFilingPricingTool.Domain.Enums
{
    /// <summary>
    /// Defines the available VAT filing service types that represent different levels of 
    /// service complexity and pricing tiers.
    /// This is a core domain concept used throughout the VAT Filing Pricing Tool 
    /// for service selection and pricing calculations.
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// Standard VAT filing service with basic features.
        /// Includes standard processing times and basic validation.
        /// This is the entry-level service option.
        /// </summary>
        StandardFiling = 1,
        
        /// <summary>
        /// Complex VAT filing service for businesses with more complicated tax situations.
        /// Includes additional verification steps, reconciliation services, and 
        /// handling of special VAT scenarios.
        /// </summary>
        ComplexFiling = 2,
        
        /// <summary>
        /// Premium service level with priority processing and expedited filing.
        /// Includes all features of Complex Filing plus dedicated support, 
        /// accelerated processing times, and enhanced validation.
        /// </summary>
        PriorityService = 3
    }
}