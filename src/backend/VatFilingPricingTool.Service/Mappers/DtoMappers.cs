using System; // System version 6.0.0
using System.Collections.Generic; // System.Collections.Generic version 6.0.0
using System.Linq; // System.Linq version 6.0.0
using VatFilingPricingTool.Service.Models; // VatFilingPricingTool.Service.Models version 1.0.0
using VatFilingPricingTool.Contracts.V1.Models; // VatFilingPricingTool.Contracts version 1.0.0

namespace VatFilingPricingTool.Service.Mappers
{
    /// <summary>
    /// Provides extension methods for mapping between service models and contract DTOs
    /// in the VAT Filing Pricing Tool. This class facilitates the transformation of data
    /// between the service layer and API layer, ensuring proper data encapsulation and
    /// separation of concerns.
    /// </summary>
    public static class DtoMappers
    {
        #region Calculation Model Mappers

        /// <summary>
        /// Converts a service CalculationModel to a contract CalculationModel
        /// </summary>
        /// <param name="serviceModel">The service model to convert</param>
        /// <returns>The converted contract model</returns>
        public static Contracts.V1.Models.CalculationModel ToContractModel(
            this Service.Models.CalculationModel serviceModel)
        {
            if (serviceModel == null)
                return null;

            var contractModel = new Contracts.V1.Models.CalculationModel
            {
                CalculationId = serviceModel.CalculationId,
                UserId = serviceModel.UserId,
                ServiceType = serviceModel.ServiceType,
                TransactionVolume = serviceModel.TransactionVolume,
                FilingFrequency = serviceModel.FilingFrequency,
                TotalCost = serviceModel.TotalCost?.Amount ?? 0,
                CurrencyCode = serviceModel.TotalCost?.CurrencyCode ?? serviceModel.CurrencyCode,
                CalculationDate = serviceModel.CalculationDate,
                IsArchived = serviceModel.IsArchived,
                CountryBreakdowns = serviceModel.CountryBreakdowns?.Select(c => new Contracts.V1.Models.CountryCalculationModel
                {
                    CountryCode = c.CountryCode,
                    CountryCost = c.CountryCost?.Amount ?? 0,
                    AppliedRules = c.AppliedRules
                })?.ToList() ?? new List<Contracts.V1.Models.CountryCalculationModel>(),
                AdditionalServices = serviceModel.AdditionalServices?.ToList() ?? new List<string>(),
                Discounts = serviceModel.Discounts?.ToList() ?? new List<string>()
            };

            return contractModel;
        }

        /// <summary>
        /// Converts a contract CalculationModel to a service CalculationModel
        /// </summary>
        /// <param name="contractModel">The contract model to convert</param>
        /// <returns>The converted service model</returns>
        public static Service.Models.CalculationModel ToServiceModel(
            this Contracts.V1.Models.CalculationModel contractModel)
        {
            if (contractModel == null)
                return null;

            var serviceModel = new Service.Models.CalculationModel
            {
                CalculationId = contractModel.CalculationId,
                UserId = contractModel.UserId,
                ServiceType = contractModel.ServiceType,
                TransactionVolume = contractModel.TransactionVolume,
                FilingFrequency = contractModel.FilingFrequency,
                TotalCost = new Money { Amount = contractModel.TotalCost, CurrencyCode = contractModel.CurrencyCode },
                CurrencyCode = contractModel.CurrencyCode,
                CalculationDate = contractModel.CalculationDate,
                IsArchived = contractModel.IsArchived,
                CountryBreakdowns = contractModel.CountryBreakdowns?.Select(c => new Service.Models.CountryCalculationModel
                {
                    CountryCode = c.CountryCode,
                    CountryCost = new Money { Amount = c.CountryCost, CurrencyCode = contractModel.CurrencyCode },
                    AppliedRules = c.AppliedRules
                })?.ToList() ?? new List<Service.Models.CountryCalculationModel>(),
                AdditionalServices = contractModel.AdditionalServices?.ToList() ?? new List<string>(),
                Discounts = contractModel.Discounts?.ToList() ?? new List<string>()
            };

            return serviceModel;
        }

        #endregion

        #region Country Model Mappers

        /// <summary>
        /// Converts a service CountryModel to a contract CountryModel
        /// </summary>
        /// <param name="serviceModel">The service model to convert</param>
        /// <returns>The converted contract model</returns>
        public static Contracts.V1.Models.CountryModel ToContractModel(
            this Service.Models.CountryModel serviceModel)
        {
            if (serviceModel == null)
                return null;

            var contractModel = new Contracts.V1.Models.CountryModel
            {
                CountryCode = serviceModel.CountryCode,
                Name = serviceModel.Name,
                StandardVatRate = serviceModel.StandardVatRate,
                CurrencyCode = serviceModel.CurrencyCode,
                AvailableFilingFrequencies = serviceModel.AvailableFilingFrequencies?.ToList() ?? new List<string>(),
                IsActive = serviceModel.IsActive,
                LastUpdated = serviceModel.LastUpdated
            };

            return contractModel;
        }

        /// <summary>
        /// Converts a contract CountryModel to a service CountryModel
        /// </summary>
        /// <param name="contractModel">The contract model to convert</param>
        /// <returns>The converted service model</returns>
        public static Service.Models.CountryModel ToServiceModel(
            this Contracts.V1.Models.CountryModel contractModel)
        {
            if (contractModel == null)
                return null;

            var serviceModel = new Service.Models.CountryModel
            {
                CountryCode = contractModel.CountryCode,
                Name = contractModel.Name,
                StandardVatRate = contractModel.StandardVatRate,
                CurrencyCode = contractModel.CurrencyCode,
                AvailableFilingFrequencies = contractModel.AvailableFilingFrequencies?.ToList() ?? new List<string>(),
                IsActive = contractModel.IsActive,
                LastUpdated = contractModel.LastUpdated
            };

            return serviceModel;
        }

        #endregion

        #region Report Model Mappers

        /// <summary>
        /// Converts a service ReportModel to a contract ReportModel
        /// </summary>
        /// <param name="serviceModel">The service model to convert</param>
        /// <returns>The converted contract model</returns>
        public static Contracts.V1.Models.ReportModel ToContractModel(
            this Service.Models.ReportModel serviceModel)
        {
            if (serviceModel == null)
                return null;

            var contractModel = new Contracts.V1.Models.ReportModel
            {
                ReportId = serviceModel.ReportId,
                UserId = serviceModel.UserId,
                CalculationId = serviceModel.CalculationId,
                ReportTitle = serviceModel.ReportTitle,
                ReportType = serviceModel.ReportType,
                Format = serviceModel.Format,
                StorageUrl = serviceModel.StorageUrl,
                GenerationDate = serviceModel.GenerationDate,
                FileSize = serviceModel.FileSize,
                IncludeCountryBreakdown = serviceModel.IncludeCountryBreakdown,
                IncludeServiceDetails = serviceModel.IncludeServiceDetails,
                IncludeAppliedDiscounts = serviceModel.IncludeAppliedDiscounts,
                IncludeHistoricalComparison = serviceModel.IncludeHistoricalComparison,
                IncludeTaxRateDetails = serviceModel.IncludeTaxRateDetails,
                IsArchived = serviceModel.IsArchived
            };

            return contractModel;
        }

        /// <summary>
        /// Converts a contract ReportModel to a service ReportModel
        /// </summary>
        /// <param name="contractModel">The contract model to convert</param>
        /// <returns>The converted service model</returns>
        public static Service.Models.ReportModel ToServiceModel(
            this Contracts.V1.Models.ReportModel contractModel)
        {
            if (contractModel == null)
                return null;

            var serviceModel = new Service.Models.ReportModel
            {
                ReportId = contractModel.ReportId,
                UserId = contractModel.UserId,
                CalculationId = contractModel.CalculationId,
                ReportTitle = contractModel.ReportTitle,
                ReportType = contractModel.ReportType,
                Format = contractModel.Format,
                StorageUrl = contractModel.StorageUrl,
                GenerationDate = contractModel.GenerationDate,
                FileSize = contractModel.FileSize,
                IncludeCountryBreakdown = contractModel.IncludeCountryBreakdown,
                IncludeServiceDetails = contractModel.IncludeServiceDetails,
                IncludeAppliedDiscounts = contractModel.IncludeAppliedDiscounts,
                IncludeHistoricalComparison = contractModel.IncludeHistoricalComparison,
                IncludeTaxRateDetails = contractModel.IncludeTaxRateDetails,
                IsArchived = contractModel.IsArchived
            };

            return serviceModel;
        }

        #endregion

        #region Rule Model Mappers

        /// <summary>
        /// Converts a service RuleModel to a contract RuleModel
        /// </summary>
        /// <param name="serviceModel">The service model to convert</param>
        /// <returns>The converted contract model</returns>
        public static Contracts.V1.Models.RuleModel ToContractModel(
            this Service.Models.RuleModel serviceModel)
        {
            if (serviceModel == null)
                return null;

            var contractModel = new Contracts.V1.Models.RuleModel
            {
                RuleId = serviceModel.RuleId,
                CountryCode = serviceModel.CountryCode,
                RuleType = serviceModel.RuleType,
                Name = serviceModel.Name,
                Description = serviceModel.Description,
                Expression = serviceModel.Expression,
                EffectiveFrom = serviceModel.EffectiveFrom,
                EffectiveTo = serviceModel.EffectiveTo,
                Priority = serviceModel.Priority,
                IsActive = serviceModel.IsActive,
                LastUpdated = serviceModel.LastUpdated,
                Parameters = serviceModel.Parameters?.Select(p => new Contracts.V1.Models.RuleParameterModel
                {
                    ParameterId = p.ParameterId,
                    Name = p.Name,
                    DataType = p.DataType,
                    DefaultValue = p.DefaultValue
                })?.ToList() ?? new List<Contracts.V1.Models.RuleParameterModel>(),
                Conditions = serviceModel.Conditions?.Select(c => new Contracts.V1.Models.RuleConditionModel
                {
                    Parameter = c.Parameter,
                    Operator = c.Operator,
                    Value = c.Value
                })?.ToList() ?? new List<Contracts.V1.Models.RuleConditionModel>()
            };

            return contractModel;
        }

        /// <summary>
        /// Converts a contract RuleModel to a service RuleModel
        /// </summary>
        /// <param name="contractModel">The contract model to convert</param>
        /// <returns>The converted service model</returns>
        public static Service.Models.RuleModel ToServiceModel(
            this Contracts.V1.Models.RuleModel contractModel)
        {
            if (contractModel == null)
                return null;

            var serviceModel = new Service.Models.RuleModel
            {
                RuleId = contractModel.RuleId,
                CountryCode = contractModel.CountryCode,
                RuleType = contractModel.RuleType,
                Name = contractModel.Name,
                Description = contractModel.Description,
                Expression = contractModel.Expression,
                EffectiveFrom = contractModel.EffectiveFrom,
                EffectiveTo = contractModel.EffectiveTo,
                Priority = contractModel.Priority,
                IsActive = contractModel.IsActive,
                LastUpdated = contractModel.LastUpdated,
                Parameters = contractModel.Parameters?.Select(p => new Service.Models.RuleParameterModel
                {
                    ParameterId = p.ParameterId,
                    Name = p.Name,
                    DataType = p.DataType,
                    DefaultValue = p.DefaultValue
                })?.ToList() ?? new List<Service.Models.RuleParameterModel>(),
                Conditions = contractModel.Conditions?.Select(c => new Service.Models.RuleConditionModel
                {
                    Parameter = c.Parameter,
                    Operator = c.Operator,
                    Value = c.Value
                })?.ToList() ?? new List<Service.Models.RuleConditionModel>()
            };

            return serviceModel;
        }

        #endregion

        #region User Model Mappers

        /// <summary>
        /// Converts a service UserModel to a contract UserModel
        /// </summary>
        /// <param name="serviceModel">The service model to convert</param>
        /// <returns>The converted contract model</returns>
        public static Contracts.V1.Models.UserModel ToContractModel(
            this Service.Models.UserModel serviceModel)
        {
            if (serviceModel == null)
                return null;

            var contractModel = new Contracts.V1.Models.UserModel
            {
                UserId = serviceModel.UserId,
                Email = serviceModel.Email,
                FirstName = serviceModel.FirstName,
                LastName = serviceModel.LastName,
                Roles = serviceModel.Roles?.ToList() ?? new List<string>(),
                CreatedDate = serviceModel.CreatedDate,
                LastLoginDate = serviceModel.LastLoginDate,
                IsActive = serviceModel.IsActive,
                CompanyName = serviceModel.CompanyName,
                PhoneNumber = serviceModel.PhoneNumber
            };

            return contractModel;
        }

        /// <summary>
        /// Converts a contract UserModel to a service UserModel
        /// </summary>
        /// <param name="contractModel">The contract model to convert</param>
        /// <returns>The converted service model</returns>
        public static Service.Models.UserModel ToServiceModel(
            this Contracts.V1.Models.UserModel contractModel)
        {
            if (contractModel == null)
                return null;

            var serviceModel = new Service.Models.UserModel
            {
                UserId = contractModel.UserId,
                Email = contractModel.Email,
                FirstName = contractModel.FirstName,
                LastName = contractModel.LastName,
                Roles = contractModel.Roles?.ToList() ?? new List<string>(),
                CreatedDate = contractModel.CreatedDate,
                LastLoginDate = contractModel.LastLoginDate,
                IsActive = contractModel.IsActive,
                CompanyName = contractModel.CompanyName,
                PhoneNumber = contractModel.PhoneNumber
            };

            return serviceModel;
        }

        #endregion
    }
}