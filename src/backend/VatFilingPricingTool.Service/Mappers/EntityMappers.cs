using System; // System v6.0.0
using System.Collections.Generic;
using System.Linq; // System.Linq v6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Service.Models;

namespace VatFilingPricingTool.Service.Mappers
{
    /// <summary>
    /// Provides extension methods for mapping between domain entities and service models in the VAT Filing Pricing Tool.
    /// This class facilitates the transformation of data between the domain layer and service layer, ensuring proper domain encapsulation and separation of concerns.
    /// </summary>
    public static class EntityMappers
    {
        /// <summary>
        /// Converts a service CalculationModel to a domain Calculation entity
        /// </summary>
        /// <param name="model">The service model to convert</param>
        /// <returns>The converted domain entity</returns>
        public static Calculation ToEntity(this CalculationModel model)
        {
            // Validate that model is not null
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "CalculationModel cannot be null");
            }

            // Create a new Calculation entity using Calculation.Create factory method with model properties
            var calculation = Calculation.Create(
                model.UserId,
                model.ServiceType.ToString(),
                model.TransactionVolume,
                model.Frequency,
                model.CurrencyCode
            );

            // Set the CalculationId if it exists
            if (!string.IsNullOrEmpty(model.CalculationId))
            {
                typeof(Calculation).GetProperty("CalculationId").SetValue(calculation, model.CalculationId);
            }

            // Map TotalCost, CurrencyCode, CalculationDate, IsArchived
            typeof(Calculation).GetProperty("TotalCost").SetValue(calculation, model.TotalCost);
            typeof(Calculation).GetProperty("CalculationDate").SetValue(calculation, model.CalculationDate);
            typeof(Calculation).GetProperty("IsArchived").SetValue(calculation, model.IsArchived);

            // For each country in model.CountryBreakdowns, add a country to the calculation entity
            foreach (var countryModel in model.CountryBreakdowns)
            {
                // Create Money object for the country cost
                Money countryCost = countryModel.TotalCost;

                // Add country to the calculation
                var calculationCountry = calculation.AddCountry(countryModel.CountryCode, countryCost);

                // Apply applied rules to the calculation country
                if (countryModel.AppliedRules != null)
                {
                    foreach (var rule in countryModel.AppliedRules)
                    {
                        typeof(CalculationCountry).GetMethod("AddAppliedRule").Invoke(calculationCountry, new object[] { rule });
                    }
                }
            }

            // For each additional service, add to the calculation entity
            if (model.AdditionalServices != null)
            {
                foreach (var service in model.AdditionalServices)
                {
                    // Assuming a default cost for additional services
                    Money serviceCost = Money.Create(100, model.CurrencyCode);
                    calculation.AddAdditionalService(service, serviceCost);
                }
            }

            // Apply any discounts to the calculation entity
            if (model.Discounts != null)
            {
                foreach (var discount in model.Discounts)
                {
                    calculation.ApplyDiscount(discount.Value, discount.Key);
                }
            }

            // Return the populated Calculation entity
            return calculation;
        }

        /// <summary>
        /// Converts a domain Calculation entity to a service CalculationModel
        /// </summary>
        /// <param name="entity">The domain entity to convert</param>
        /// <param name="countryNames">Dictionary of country codes and names</param>
        /// <returns>The converted service model</returns>
        public static CalculationModel ToModel(this Calculation entity, Dictionary<string, string> countryNames)
        {
            // Validate that entity is not null
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Calculation entity cannot be null");
            }

            // Create a new CalculationModel instance
            var model = new CalculationModel
            {
                CalculationId = entity.CalculationId,
                UserId = entity.UserId,
                ServiceType = Enum.Parse<ServiceType>(entity.ServiceId),
                TransactionVolume = entity.TransactionVolume,
                Frequency = entity.FilingFrequency,
                TotalCost = entity.TotalCost,
                CurrencyCode = entity.CurrencyCode,
                CalculationDate = entity.CalculationDate,
                IsArchived = entity.IsArchived,
                CountryBreakdowns = new List<CountryCalculationModel>(),
                AdditionalServices = entity.AdditionalServices.Select(s => s.AdditionalServiceId).ToList(),
                Discounts = new Dictionary<string, decimal>()
            };

            // For each CalculationCountry in entity.CalculationCountries, create a CountryCalculationModel
            foreach (var calculationCountry in entity.CalculationCountries)
            {
                // Use countryNames dictionary to get country names for each country code
                if (countryNames.TryGetValue(calculationCountry.CountryCode.Value, out string countryName))
                {
                    // Create a CountryCalculationModel
                    var countryModel = new CountryCalculationModel
                    {
                        CountryCode = calculationCountry.CountryCode.Value,
                        CountryName = countryName,
                        BaseCost = calculationCountry.CountryCost,
                        AdditionalCost = Money.CreateZero(entity.CurrencyCode), // Assuming no additional cost initially
                        TotalCost = calculationCountry.CountryCost,
                        AppliedRules = calculationCountry.GetAppliedRules().ToList()
                    };

                    // Add each CountryCalculationModel to model.CountryBreakdowns
                    model.CountryBreakdowns.Add(countryModel);
                }
                else
                {
                    // Handle case where country name is not found
                    Console.WriteLine($"Country name not found for code: {calculationCountry.CountryCode.Value}");
                }
            }

            // Map AdditionalServices and Discounts
            // (Implementation details depend on how these are stored in the domain entity)

            // Return the populated service model
            return model;
        }

        /// <summary>
        /// Converts a service CountryModel to a domain Country entity
        /// </summary>
        /// <param name="model">The service model to convert</param>
        /// <returns>The converted domain entity</returns>
        public static Country ToEntity(this CountryModel model)
        {
            // Validate that model is not null
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "CountryModel cannot be null");
            }

            // Create a new Country entity using Country.Create factory method with model properties
            var country = Country.Create(
                model.CountryCode,
                model.Name,
                model.StandardVatRate,
                model.CurrencyCode
            );

            // For each filing frequency in model.AvailableFilingFrequencies, add it to the country entity
            foreach (var frequency in model.AvailableFilingFrequencies)
            {
                country.AddFilingFrequency(frequency);
            }

            // If model.IsActive is false, call SetActive(false) on the country entity
            if (!model.IsActive)
            {
                typeof(Country).GetMethod("SetActive").Invoke(country, new object[] { false });
            }

            // Set LastUpdated
            typeof(Country).GetProperty("LastUpdated").SetValue(country, model.LastUpdated);

            // Return the populated Country entity
            return country;
        }

        /// <summary>
        /// Converts a domain Country entity to a service CountryModel
        /// </summary>
        /// <param name="entity">The domain entity to convert</param>
        /// <returns>The converted service model</returns>
        public static CountryModel ToModel(this Country entity)
        {
            // Validate that entity is not null
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Country entity cannot be null");
            }

            // Create a new CountryModel instance
            var model = new CountryModel
            {
                CountryCode = entity.Code.Value,
                Name = entity.Name,
                StandardVatRate = entity.StandardVatRate.Value,
                CurrencyCode = entity.CurrencyCode,
                AvailableFilingFrequencies = new List<FilingFrequency>(entity.AvailableFilingFrequencies),
                IsActive = entity.IsActive,
                LastUpdated = entity.LastUpdated
            };

            // Return the populated CountryModel
            return model;
        }

        /// <summary>
        /// Converts a service RuleModel to a domain Rule entity
        /// </summary>
        /// <param name="model">The service model to convert</param>
        /// <returns>The converted domain entity</returns>
        public static Rule ToEntity(this RuleModel model)
        {
            // Validate that model is not null
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "RuleModel cannot be null");
            }

            // Create a new Rule entity using Rule.Create factory method with model properties
            var rule = Rule.Create(
                model.CountryCode,
                model.Type,
                model.Name,
                model.Expression,
                model.EffectiveFrom,
                model.Description
            );

            // If model.RuleId is not null or empty, set it on the created rule
            if (!string.IsNullOrEmpty(model.RuleId))
            {
                typeof(Rule).GetProperty("RuleId").SetValue(rule, model.RuleId);
            }

            // If model.EffectiveTo has value, update effective dates on the rule
            if (model.EffectiveTo.HasValue)
            {
                typeof(Rule).GetMethod("UpdateEffectiveDates").Invoke(rule, new object[] { model.EffectiveFrom, model.EffectiveTo });
            }

            // If model.Priority is different from default, update priority on the rule
            if (model.Priority != 0)
            {
                typeof(Rule).GetMethod("UpdatePriority").Invoke(rule, new object[] { model.Priority });
            }

            // Set IsActive on the rule
            typeof(Rule).GetMethod("SetActive").Invoke(rule, new object[] { model.IsActive });

            // For each parameter in model.Parameters, add to the rule using AddParameter
            if (model.Parameters != null)
            {
                foreach (var parameter in model.Parameters)
                {
                    rule.AddParameter(parameter.Name, parameter.DataType, parameter.DefaultValue);
                }
            }

            // For each condition in model.Conditions, add to the rule using AddCondition
            if (model.Conditions != null)
            {
                foreach (var condition in model.Conditions)
                {
                    rule.AddCondition(condition.Parameter, condition.Operator, condition.Value);
                }
            }

            // Return the created and configured Rule entity
            return rule;
        }

        /// <summary>
        /// Converts a domain Rule entity to a service RuleModel
        /// </summary>
        /// <param name="entity">The domain entity to convert</param>
        /// <returns>The converted service model</returns>
        public static RuleModel ToModel(this Rule entity)
        {
            // Validate that entity is not null
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Rule entity cannot be null");
            }

            // Create a new RuleModel instance
            var model = new RuleModel
            {
                RuleId = entity.RuleId,
                CountryCode = entity.CountryCode.Value,
                Type = entity.Type,
                Name = entity.Name,
                Description = entity.Description,
                Expression = entity.Expression,
                EffectiveFrom = entity.EffectiveFrom,
                EffectiveTo = entity.EffectiveTo,
                Priority = entity.Priority,
                IsActive = entity.IsActive,
                LastUpdated = entity.LastUpdated,
                Parameters = new List<RuleParameterModel>(),
                Conditions = new List<RuleConditionModel>()
            };

            // Map each RuleParameter to a RuleParameterModel and add to Parameters
            foreach (var parameter in entity.Parameters)
            {
                model.Parameters.Add(new RuleParameterModel
                {
                    ParameterId = parameter.ParameterId,
                    Name = parameter.Name,
                    DataType = parameter.DataType,
                    DefaultValue = parameter.DefaultValue
                });
            }

            // Map each RuleCondition to a RuleConditionModel and add to Conditions
            foreach (var condition in entity.Conditions)
            {
                model.Conditions.Add(new RuleConditionModel
                {
                    Parameter = condition.Parameter,
                    Operator = condition.Operator,
                    Value = condition.Value
                });
            }

            // Return the populated RuleModel
            return model;
        }

        /// <summary>
        /// Converts a service ReportModel to a domain Report entity
        /// </summary>
        /// <param name="model">The service model to convert</param>
        /// <returns>The converted domain entity</returns>
        public static Report ToEntity(this ReportModel model)
        {
            // Validate that model is not null
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "ReportModel cannot be null");
            }

            // Create a new Report entity using Report.Create factory method with model properties
            var report = Report.Create(
                model.UserId,
                model.CalculationId,
                model.ReportTitle,
                model.ReportType,
                model.Format
            );

            // If model.StorageUrl and model.FileSize are set, update storage info on the entity
            if (!string.IsNullOrEmpty(model.StorageUrl) && model.FileSize > 0)
            {
                typeof(Report).GetMethod("UpdateStorageInfo").Invoke(report, new object[] { model.StorageUrl, model.FileSize });
            }

            // If model.IsArchived is true, archive the entity
            if (model.IsArchived)
            {
                typeof(Report).GetMethod("Archive").Invoke(report, null);
            }

            // Set ReportId
            if (!string.IsNullOrEmpty(model.ReportId))
            {
                typeof(Report).GetProperty("ReportId").SetValue(report, model.ReportId);
            }

            // Return the populated Report entity
            return report;
        }

        /// <summary>
        /// Converts a domain Report entity to a service ReportModel
        /// </summary>
        /// <param name="entity">The domain entity to convert</param>
        /// <param name="calculationData">The calculation data for the report</param>
        /// <returns>The converted service model</returns>
        public static ReportModel ToModel(this Report entity, CalculationModel calculationData)
        {
            // Validate that entity is not null
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Report entity cannot be null");
            }

            // Create a new ReportModel instance
            var model = new ReportModel
            {
                ReportId = entity.ReportId,
                UserId = entity.UserId,
                CalculationId = entity.CalculationId,
                ReportTitle = entity.ReportTitle,
                ReportType = entity.ReportType,
                Format = entity.Format,
                StorageUrl = entity.StorageUrl,
                GenerationDate = entity.GenerationDate,
                FileSize = entity.FileSize,
                IsArchived = entity.IsArchived,
                CalculationData = calculationData
            };

            // Parse report type to determine included sections
            if (!string.IsNullOrEmpty(entity.ReportType))
            {
                model.IncludeCountryBreakdown = entity.ReportType.Contains("Country", StringComparison.OrdinalIgnoreCase);
                model.IncludeServiceDetails = entity.ReportType.Contains("Service", StringComparison.OrdinalIgnoreCase);
                model.IncludeAppliedDiscounts = entity.ReportType.Contains("Discount", StringComparison.OrdinalIgnoreCase);
                model.IncludeHistoricalComparison = entity.ReportType.Contains("Historical", StringComparison.OrdinalIgnoreCase);
                model.IncludeTaxRateDetails = entity.ReportType.Contains("Tax", StringComparison.OrdinalIgnoreCase);
            }

            // Return the populated ReportModel
            return model;
        }

        /// <summary>
        /// Converts a service UserModel to a domain User entity
        /// </summary>
        /// <param name="model">The service model to convert</param>
        /// <returns>The converted domain entity</returns>
        public static User ToEntity(this UserModel model)
        {
            // Validate that model is not null
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "UserModel cannot be null");
            }

            // Create a new User entity with model properties
            var user = User.Create(
                model.Email,
                model.FirstName,
                model.LastName,
                model.Roles.FirstOrDefault() // Assuming only one role for now
            );

            // Set UserId
            typeof(User).GetProperty("UserId").SetValue(user, model.UserId);

            // Set CreatedDate
            typeof(User).GetProperty("CreatedDate").SetValue(user, model.CreatedDate);

            // Set LastLoginDate
            typeof(User).GetProperty("LastLoginDate").SetValue(user, model.LastLoginDate);

            // Set IsActive
            typeof(User).GetProperty("IsActive").SetValue(user, model.IsActive);

            // Set AzureAdObjectId
            typeof(User).GetProperty("AzureAdObjectId").SetValue(user, model.AzureAdObjectId);

            // Return the populated User entity
            return user;
        }

        /// <summary>
        /// Converts a domain User entity to a service UserModel
        /// </summary>
        /// <param name="entity">The domain entity to convert</param>
        /// <returns>The converted service model</returns>
        public static UserModel ToModel(this User entity)
        {
            // Validate that entity is not null
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "User entity cannot be null");
            }

            // Create a new UserModel instance
            var model = new UserModel
            {
                UserId = entity.UserId,
                Email = entity.Email,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                CreatedDate = entity.CreatedDate,
                LastLoginDate = entity.LastLoginDate,
                IsActive = entity.IsActive,
                AzureAdObjectId = entity.AzureAdObjectId,
                Roles = new List<UserRole> { entity.Role }
            };

            // Return the populated UserModel
            return model;
        }
    }
}