using System; // System package version 6.0.0

namespace VatFilingPricingTool.Domain.Enums
{
    /// <summary>
    /// Represents the different roles that users can have in the VAT Filing Pricing Tool system,
    /// determining their access permissions and capabilities.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Full system administration with complete access to all features including user management,
        /// pricing rules, country configuration, and system settings.
        /// </summary>
        Administrator = 0,

        /// <summary>
        /// Manages pricing rules and models with access to pricing configuration and limited user management,
        /// but no access to system settings.
        /// </summary>
        PricingAdministrator = 1,

        /// <summary>
        /// Financial operations role with access to calculations, reporting, and view-only access to pricing configuration,
        /// but no administrative capabilities.
        /// </summary>
        Accountant = 2,

        /// <summary>
        /// Regular business user with access to their own data, pricing calculations, and reports,
        /// but no configuration or administrative access.
        /// </summary>
        Customer = 3,

        /// <summary>
        /// System integration role with API access only for automated operations,
        /// no UI access or administrative capabilities.
        /// </summary>
        ApiClient = 4
    }
}