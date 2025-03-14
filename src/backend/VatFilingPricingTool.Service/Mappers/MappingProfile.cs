using AutoMapper; // AutoMapper - version 11.0.0
using System; // System - version 6.0.0
using System.Collections.Generic; // System.Collections.Generic - version 6.0.0
using System.Linq; // System.Linq - version 6.0.0
using VatFilingPricingTool.Service.Models; // VatFilingPricingTool.Service.Models - version 1.0.0
using VatFilingPricingTool.Contracts.V1.Models; // VatFilingPricingTool.Contracts - version 1.0.0

namespace VatFilingPricingTool.Service.Mappers
{
    /// <summary>
    /// AutoMapper profile that defines bidirectional mappings between service models and contract DTOs.
    /// This class facilitates seamless data transformation across application layers while maintaining
    /// proper separation of concerns between the internal service implementation and external API contracts.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the MappingProfile class and configures all mappings
        /// between service models and contract DTOs in both directions.
        /// </summary>
        public MappingProfile()
        {
            // Configure mappings from service models to contract DTOs
            ConfigureServiceToContractMappings();
            
            // Configure mappings from contract DTOs to service models
            ConfigureContractToServiceMappings();
        }

        /// <summary>
        /// Configures mappings from service models to contract DTOs.
        /// These mappings transform the internal service model representation to the external contract DTOs
        /// that are exposed through the API.
        /// </summary>
        private void ConfigureServiceToContractMappings()
        {
            // Calculation model mapping
            CreateMap<Service.Models.CalculationModel, Contracts.V1.Models.CalculationModel>()
                // Map most properties directly as they have the same name and type
                .ForMember(dest => dest.CalculationId, opt => opt.MapFrom(src => src.CalculationId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => src.ServiceType))
                .ForMember(dest => dest.TransactionVolume, opt => opt.MapFrom(src => src.TransactionVolume))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency))
                .ForMember(dest => dest.CalculationDate, opt => opt.MapFrom(src => src.CalculationDate))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(dest => dest.IsArchived, opt => opt.MapFrom(src => src.IsArchived))
                // Convert Money object to decimal for the API contract
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.TotalCost.Amount))
                // Map collections with potential custom resolvers
                .ForMember(dest => dest.CountryBreakdowns, opt => opt.MapFrom(src => src.CountryBreakdowns))
                .ForMember(dest => dest.AdditionalServices, opt => opt.MapFrom(src => src.AdditionalServices))
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src => src.Discounts));

            // Country calculation model mapping
            CreateMap<Service.Models.CountryCalculationModel, Contracts.V1.Models.CountryCalculationModel>()
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.CountryName))
                // Convert Money objects to decimal for the API contract
                .ForMember(dest => dest.BaseCost, opt => opt.MapFrom(src => src.BaseCost.Amount))
                .ForMember(dest => dest.AdditionalCost, opt => opt.MapFrom(src => src.AdditionalCost.Amount))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.TotalCost.Amount))
                .ForMember(dest => dest.AppliedRules, opt => opt.MapFrom(src => src.AppliedRules));

            // Country model mapping
            CreateMap<Service.Models.CountryModel, Contracts.V1.Models.CountryModel>()
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StandardVatRate, opt => opt.MapFrom(src => src.StandardVatRate))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(dest => dest.AvailableFilingFrequencies, opt => opt.MapFrom(src => src.AvailableFilingFrequencies))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated));

            // Rule model mapping
            CreateMap<Service.Models.RuleModel, Contracts.V1.Models.RuleModel>()
                .ForMember(dest => dest.RuleId, opt => opt.MapFrom(src => src.RuleId))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Expression, opt => opt.MapFrom(src => src.Expression))
                .ForMember(dest => dest.EffectiveFrom, opt => opt.MapFrom(src => src.EffectiveFrom))
                .ForMember(dest => dest.EffectiveTo, opt => opt.MapFrom(src => src.EffectiveTo))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated))
                // Map collections with custom resolvers if necessary
                .ForMember(dest => dest.Parameters, opt => opt.MapFrom(src => src.Parameters))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions));

            // Report model mapping
            CreateMap<Service.Models.ReportModel, Contracts.V1.Models.ReportModel>()
                .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.ReportId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CalculationId, opt => opt.MapFrom(src => src.CalculationId))
                .ForMember(dest => dest.ReportTitle, opt => opt.MapFrom(src => src.ReportTitle))
                .ForMember(dest => dest.ReportType, opt => opt.MapFrom(src => src.ReportType))
                .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format))
                .ForMember(dest => dest.StorageUrl, opt => opt.MapFrom(src => src.StorageUrl))
                .ForMember(dest => dest.GenerationDate, opt => opt.MapFrom(src => src.GenerationDate))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.IsArchived, opt => opt.MapFrom(src => src.IsArchived))
                // Map include flags for report configuration
                .ForMember(dest => dest.IncludeCountryBreakdown, opt => opt.MapFrom(src => src.IncludeCountryBreakdown))
                .ForMember(dest => dest.IncludeServiceDetails, opt => opt.MapFrom(src => src.IncludeServiceDetails))
                .ForMember(dest => dest.IncludeAppliedDiscounts, opt => opt.MapFrom(src => src.IncludeAppliedDiscounts))
                .ForMember(dest => dest.IncludeHistoricalComparison, opt => opt.MapFrom(src => src.IncludeHistoricalComparison))
                .ForMember(dest => dest.IncludeTaxRateDetails, opt => opt.MapFrom(src => src.IncludeTaxRateDetails));

            // User model mapping
            CreateMap<Service.Models.UserModel, Contracts.V1.Models.UserModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.LastLoginDate))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
        }

        /// <summary>
        /// Configures mappings from contract DTOs to service models.
        /// These mappings transform the external contract DTOs that are received through the API
        /// to the internal service model representation.
        /// </summary>
        private void ConfigureContractToServiceMappings()
        {
            // Calculation model mapping
            CreateMap<Contracts.V1.Models.CalculationModel, Service.Models.CalculationModel>()
                // Map most properties directly as they have the same name and type
                .ForMember(dest => dest.CalculationId, opt => opt.MapFrom(src => src.CalculationId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => src.ServiceType))
                .ForMember(dest => dest.TransactionVolume, opt => opt.MapFrom(src => src.TransactionVolume))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency))
                .ForMember(dest => dest.CalculationDate, opt => opt.MapFrom(src => src.CalculationDate))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(dest => dest.IsArchived, opt => opt.MapFrom(src => src.IsArchived))
                // Convert decimal to Money object for internal service use
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => Money.Create(src.TotalCost, src.CurrencyCode)))
                // Map collections with potential custom resolvers
                .ForMember(dest => dest.CountryBreakdowns, opt => opt.MapFrom(src => src.CountryBreakdowns))
                .ForMember(dest => dest.AdditionalServices, opt => opt.MapFrom(src => src.AdditionalServices))
                .ForMember(dest => dest.Discounts, opt => opt.MapFrom(src => src.Discounts));

            // Country calculation model mapping
            CreateMap<Contracts.V1.Models.CountryCalculationModel, Service.Models.CountryCalculationModel>()
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.CountryName))
                // Convert decimal to Money objects for internal service use
                // Use context.Items to get currency code from parent mapping context when available
                .ForMember(dest => dest.BaseCost, opt => opt.MapFrom((src, dest, _, context) => 
                    Money.Create(src.BaseCost, context.Items.ContainsKey("CurrencyCode") ? context.Items["CurrencyCode"].ToString() : "EUR")))
                .ForMember(dest => dest.AdditionalCost, opt => opt.MapFrom((src, dest, _, context) => 
                    Money.Create(src.AdditionalCost, context.Items.ContainsKey("CurrencyCode") ? context.Items["CurrencyCode"].ToString() : "EUR")))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom((src, dest, _, context) => 
                    Money.Create(src.TotalCost, context.Items.ContainsKey("CurrencyCode") ? context.Items["CurrencyCode"].ToString() : "EUR")))
                .ForMember(dest => dest.AppliedRules, opt => opt.MapFrom(src => src.AppliedRules));

            // Country model mapping
            CreateMap<Contracts.V1.Models.CountryModel, Service.Models.CountryModel>()
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StandardVatRate, opt => opt.MapFrom(src => src.StandardVatRate))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(dest => dest.AvailableFilingFrequencies, opt => opt.MapFrom(src => src.AvailableFilingFrequencies))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated));

            // Rule model mapping
            CreateMap<Contracts.V1.Models.RuleModel, Service.Models.RuleModel>()
                .ForMember(dest => dest.RuleId, opt => opt.MapFrom(src => src.RuleId))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Expression, opt => opt.MapFrom(src => src.Expression))
                .ForMember(dest => dest.EffectiveFrom, opt => opt.MapFrom(src => src.EffectiveFrom))
                .ForMember(dest => dest.EffectiveTo, opt => opt.MapFrom(src => src.EffectiveTo))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.LastUpdated))
                // Map collections with custom resolvers if necessary
                .ForMember(dest => dest.Parameters, opt => opt.MapFrom(src => src.Parameters))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions));

            // Report model mapping
            CreateMap<Contracts.V1.Models.ReportModel, Service.Models.ReportModel>()
                .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.ReportId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CalculationId, opt => opt.MapFrom(src => src.CalculationId))
                .ForMember(dest => dest.ReportTitle, opt => opt.MapFrom(src => src.ReportTitle))
                .ForMember(dest => dest.ReportType, opt => opt.MapFrom(src => src.ReportType))
                .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format))
                .ForMember(dest => dest.StorageUrl, opt => opt.MapFrom(src => src.StorageUrl))
                .ForMember(dest => dest.GenerationDate, opt => opt.MapFrom(src => src.GenerationDate))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.IsArchived, opt => opt.MapFrom(src => src.IsArchived))
                // Map include flags for report configuration
                .ForMember(dest => dest.IncludeCountryBreakdown, opt => opt.MapFrom(src => src.IncludeCountryBreakdown))
                .ForMember(dest => dest.IncludeServiceDetails, opt => opt.MapFrom(src => src.IncludeServiceDetails))
                .ForMember(dest => dest.IncludeAppliedDiscounts, opt => opt.MapFrom(src => src.IncludeAppliedDiscounts))
                .ForMember(dest => dest.IncludeHistoricalComparison, opt => opt.MapFrom(src => src.IncludeHistoricalComparison))
                .ForMember(dest => dest.IncludeTaxRateDetails, opt => opt.MapFrom(src => src.IncludeTaxRateDetails));

            // User model mapping
            CreateMap<Contracts.V1.Models.UserModel, Service.Models.UserModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.LastLoginDate))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
        }
    }
}