using System; // System 6.0.0 - Core .NET functionality
using System.ComponentModel; // For DescriptionAttribute
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations 6.0.0 - For accessing DisplayAttribute and other annotation attributes
using System.Linq; // System.Linq 6.0.0 - For LINQ extension methods
using System.Reflection; // System.Reflection 6.0.0 - For accessing enum metadata and attributes
using System.Collections.Generic; // For IEnumerable and other collection types
using VatFilingPricingTool.Domain.Enums; // VatFilingPricingTool.Domain 1.0.0 - For accessing domain enum types

namespace VatFilingPricingTool.Web.Utils
{
    /// <summary>
    /// Provides extension methods for enum types used throughout the VAT Filing Pricing Tool web application.
    /// These extensions enhance enum usability in the UI by providing human-readable display names,
    /// descriptions, and utility methods for enum conversion and presentation.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the display name of an enum value using DisplayAttribute or falls back to formatted enum name.
        /// </summary>
        /// <param name="value">The enum value to get the display name for.</param>
        /// <returns>Human-readable display name for the enum value.</returns>
        public static string GetDisplayName(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            
            if (name == null)
                return string.Empty;
                
            MemberInfo memberInfo = type.GetMember(name).FirstOrDefault();
            
            if (memberInfo != null)
            {
                DisplayAttribute displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
                
                if (displayAttribute != null)
                    return displayAttribute.Name;
            }
            
            // If no DisplayAttribute was found, format the enum name by splitting camel case
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }
        
        /// <summary>
        /// Gets the description of an enum value using DescriptionAttribute or falls back to display name.
        /// </summary>
        /// <param name="value">The enum value to get the description for.</param>
        /// <returns>Description of the enum value.</returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            
            if (name == null)
                return string.Empty;
                
            MemberInfo memberInfo = type.GetMember(name).FirstOrDefault();
            
            if (memberInfo != null)
            {
                DescriptionAttribute descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
                
                if (descriptionAttribute != null)
                    return descriptionAttribute.Description;
            }
            
            // If no DescriptionAttribute was found, fall back to display name
            return value.GetDisplayName();
        }
        
        /// <summary>
        /// Converts an enum type to a list of key-value pairs for use in dropdown lists.
        /// </summary>
        /// <param name="enumType">The enum type to convert.</param>
        /// <param name="includeEmpty">Whether to include an empty option at the beginning.</param>
        /// <param name="emptyText">The text to display for the empty option (if included).</param>
        /// <returns>Collection of enum values and their display names.</returns>
        public static IEnumerable<KeyValuePair<string, string>> ToSelectList(
            Type enumType, 
            bool includeEmpty = false, 
            string emptyText = "-- Select --")
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Type must be an enum", nameof(enumType));
            
            var items = new List<KeyValuePair<string, string>>();
            
            if (includeEmpty)
                items.Add(new KeyValuePair<string, string>("", emptyText));
            
            foreach (Enum value in Enum.GetValues(enumType))
            {
                items.Add(new KeyValuePair<string, string>(
                    value.ToString(), 
                    value.GetDisplayName()));
            }
            
            return items;
        }
        
        /// <summary>
        /// Gets a user-friendly display name for a ServiceType enum value.
        /// </summary>
        /// <param name="serviceType">The ServiceType enum value.</param>
        /// <returns>User-friendly display name.</returns>
        public static string GetServiceTypeDisplayName(this ServiceType serviceType)
        {
            switch (serviceType)
            {
                case ServiceType.StandardFiling:
                    return "Standard Filing";
                case ServiceType.ComplexFiling:
                    return "Complex Filing";
                case ServiceType.PriorityService:
                    return "Priority Service";
                default:
                    return serviceType.GetDisplayName();
            }
        }
        
        /// <summary>
        /// Gets a detailed description for a ServiceType enum value.
        /// </summary>
        /// <param name="serviceType">The ServiceType enum value.</param>
        /// <returns>Detailed description of the service type.</returns>
        public static string GetServiceTypeDescription(this ServiceType serviceType)
        {
            switch (serviceType)
            {
                case ServiceType.StandardFiling:
                    return "Basic VAT filing service covering standard transactions with regular processing times.";
                case ServiceType.ComplexFiling:
                    return "Comprehensive VAT filing service for complex transactions including reconciliation and verification steps.";
                case ServiceType.PriorityService:
                    return "Expedited VAT filing service with priority processing and enhanced support.";
                default:
                    return serviceType.GetDescription();
            }
        }
        
        /// <summary>
        /// Gets a user-friendly display name for a FilingFrequency enum value.
        /// </summary>
        /// <param name="filingFrequency">The FilingFrequency enum value.</param>
        /// <returns>User-friendly display name.</returns>
        public static string GetFilingFrequencyDisplayName(this FilingFrequency filingFrequency)
        {
            switch (filingFrequency)
            {
                case FilingFrequency.Monthly:
                    return "Monthly";
                case FilingFrequency.Quarterly:
                    return "Quarterly";
                case FilingFrequency.Annually:
                    return "Annually";
                default:
                    return filingFrequency.GetDisplayName();
            }
        }
        
        /// <summary>
        /// Gets a detailed description for a FilingFrequency enum value.
        /// </summary>
        /// <param name="filingFrequency">The FilingFrequency enum value.</param>
        /// <returns>Detailed description of the filing frequency.</returns>
        public static string GetFilingFrequencyDescription(this FilingFrequency filingFrequency)
        {
            switch (filingFrequency)
            {
                case FilingFrequency.Monthly:
                    return "VAT returns submitted every month, typically required for businesses with high transaction volumes.";
                case FilingFrequency.Quarterly:
                    return "VAT returns submitted every three months, the standard filing frequency for most businesses.";
                case FilingFrequency.Annually:
                    return "VAT returns submitted once per year, typically available for businesses with low transaction volumes.";
                default:
                    return filingFrequency.GetDescription();
            }
        }
        
        /// <summary>
        /// Gets a user-friendly display name for a ReportFormat enum value.
        /// </summary>
        /// <param name="reportFormat">The ReportFormat enum value.</param>
        /// <returns>User-friendly display name.</returns>
        public static string GetReportFormatDisplayName(this ReportFormat reportFormat)
        {
            switch (reportFormat)
            {
                case ReportFormat.PDF:
                    return "PDF Document";
                case ReportFormat.Excel:
                    return "Excel Spreadsheet";
                case ReportFormat.CSV:
                    return "CSV File";
                case ReportFormat.HTML:
                    return "HTML Document";
                default:
                    return reportFormat.GetDisplayName();
            }
        }
        
        /// <summary>
        /// Gets the file extension for a ReportFormat enum value.
        /// </summary>
        /// <param name="reportFormat">The ReportFormat enum value.</param>
        /// <returns>File extension including the dot.</returns>
        public static string GetReportFormatFileExtension(this ReportFormat reportFormat)
        {
            switch (reportFormat)
            {
                case ReportFormat.PDF:
                    return ".pdf";
                case ReportFormat.Excel:
                    return ".xlsx";
                case ReportFormat.CSV:
                    return ".csv";
                case ReportFormat.HTML:
                    return ".html";
                default:
                    return ".pdf";
            }
        }
        
        /// <summary>
        /// Gets the MIME type for a ReportFormat enum value.
        /// </summary>
        /// <param name="reportFormat">The ReportFormat enum value.</param>
        /// <returns>MIME type string.</returns>
        public static string GetReportFormatMimeType(this ReportFormat reportFormat)
        {
            switch (reportFormat)
            {
                case ReportFormat.PDF:
                    return "application/pdf";
                case ReportFormat.Excel:
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ReportFormat.CSV:
                    return "text/csv";
                case ReportFormat.HTML:
                    return "text/html";
                default:
                    return "application/octet-stream";
            }
        }
        
        /// <summary>
        /// Gets a user-friendly display name for a RuleType enum value.
        /// </summary>
        /// <param name="ruleType">The RuleType enum value.</param>
        /// <returns>User-friendly display name.</returns>
        public static string GetRuleTypeDisplayName(this RuleType ruleType)
        {
            switch (ruleType)
            {
                case RuleType.VatRate:
                    return "VAT Rate";
                case RuleType.Threshold:
                    return "Threshold";
                case RuleType.Complexity:
                    return "Complexity Factor";
                case RuleType.SpecialRequirement:
                    return "Special Requirement";
                case RuleType.Discount:
                    return "Discount";
                default:
                    return ruleType.GetDisplayName();
            }
        }
        
        /// <summary>
        /// Gets a user-friendly display name for a UserRole enum value.
        /// </summary>
        /// <param name="userRole">The UserRole enum value.</param>
        /// <returns>User-friendly display name.</returns>
        public static string GetUserRoleDisplayName(this UserRole userRole)
        {
            switch (userRole)
            {
                case UserRole.Administrator:
                    return "System Administrator";
                case UserRole.PricingAdministrator:
                    return "Pricing Administrator";
                case UserRole.Accountant:
                    return "Accountant";
                case UserRole.Customer:
                    return "Customer";
                case UserRole.ApiClient:
                    return "API Client";
                default:
                    return userRole.GetDisplayName();
            }
        }
        
        /// <summary>
        /// Safely parses a string to an enum value with a default value if parsing fails.
        /// </summary>
        /// <typeparam name="TEnum">The enum type to parse to.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="defaultValue">The default value to return if parsing fails.</param>
        /// <returns>Parsed enum value or default if parsing fails.</returns>
        public static TEnum ParseEnum<TEnum>(string value, TEnum defaultValue) where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
                
            if (Enum.TryParse<TEnum>(value, true, out TEnum result))
                return result;
                
            return defaultValue;
        }
    }
}