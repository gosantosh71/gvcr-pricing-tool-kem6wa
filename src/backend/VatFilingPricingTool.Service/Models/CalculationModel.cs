using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Entities;

namespace VatFilingPricingTool.Service.Models
{
    /// <summary>
    /// Service layer model for VAT filing cost calculations with detailed breakdown by country and service type
    /// </summary>
    public class CalculationModel
    {
        /// <summary>
        /// Gets or sets the calculation ID
        /// </summary>
        public string CalculationId { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID who created the calculation
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the service type for this calculation
        /// </summary>
        public ServiceType ServiceType { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction volume
        /// </summary>
        public int TransactionVolume { get; set; }
        
        /// <summary>
        /// Gets or sets the filing frequency
        /// </summary>
        public FilingFrequency Frequency { get; set; }
        
        /// <summary>
        /// Gets or sets the total cost for VAT filing
        /// </summary>
        public Money TotalCost { get; set; }
        
        /// <summary>
        /// Gets or sets the calculation date
        /// </summary>
        public DateTime CalculationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the currency code used for the calculation
        /// </summary>
        public string CurrencyCode { get; set; }
        
        /// <summary>
        /// Gets or sets the country-specific calculation breakdowns
        /// </summary>
        public List<CountryCalculationModel> CountryBreakdowns { get; set; }
        
        /// <summary>
        /// Gets or sets the additional services included in the calculation
        /// </summary>
        public List<string> AdditionalServices { get; set; }
        
        /// <summary>
        /// Gets or sets the discounts applied to the calculation
        /// </summary>
        public Dictionary<string, decimal> Discounts { get; set; }
        
        /// <summary>
        /// Gets or sets whether the calculation is archived
        /// </summary>
        public bool IsArchived { get; set; }
        
        /// <summary>
        /// Default constructor for the CalculationModel
        /// </summary>
        public CalculationModel()
        {
            CountryBreakdowns = new List<CountryCalculationModel>();
            AdditionalServices = new List<string>();
            Discounts = new Dictionary<string, decimal>();
            CalculationDate = DateTime.UtcNow;
            IsArchived = false;
        }
        
        /// <summary>
        /// Creates a CalculationModel from a domain Calculation entity
        /// </summary>
        /// <param name="entity">The Calculation entity</param>
        /// <returns>A new CalculationModel populated from the entity</returns>
        public static CalculationModel FromEntity(Calculation entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
                
            var model = new CalculationModel
            {
                CalculationId = entity.CalculationId,
                UserId = entity.UserId,
                ServiceType = GetServiceTypeFromId(entity.ServiceId),
                TransactionVolume = entity.TransactionVolume,
                Frequency = entity.FilingFrequency,
                TotalCost = entity.TotalCost,
                CalculationDate = entity.CalculationDate,
                CurrencyCode = entity.CurrencyCode,
                IsArchived = entity.IsArchived
            };
            
            // Add country breakdowns
            if (entity.CalculationCountries != null)
            {
                foreach (var countryEntity in entity.CalculationCountries)
                {
                    string countryName = GetCountryName(countryEntity.CountryCode.Value);
                    var countryModel = CountryCalculationModel.FromEntity(countryEntity, countryName);
                    model.CountryBreakdowns.Add(countryModel);
                }
            }
            
            // In a real implementation, we would map additional services from the entity
            
            return model;
        }
        
        /// <summary>
        /// Converts the CalculationModel to a domain Calculation entity
        /// </summary>
        /// <returns>A new Calculation entity populated from this model</returns>
        public Calculation ToEntity()
        {
            // Use the entity's factory method to create a valid Calculation
            var entity = Calculation.Create(
                UserId,
                GetServiceIdFromType(ServiceType),
                TransactionVolume,
                Frequency,
                CurrencyCode
            );
            
            // Add countries
            foreach (var countryModel in CountryBreakdowns)
            {
                entity.AddCountry(countryModel.CountryCode, countryModel.TotalCost);
            }
            
            // In a real implementation, we would add additional services to the entity
            
            return entity;
        }
        
        /// <summary>
        /// Converts the service CalculationModel to a contract CalculationModel for API responses
        /// </summary>
        /// <returns>A contract model populated from this service model</returns>
        public Contracts.V1.Models.CalculationModel ToContractModel()
        {
            var contractModel = new Contracts.V1.Models.CalculationModel
            {
                CalculationId = this.CalculationId,
                UserId = this.UserId,
                ServiceType = (int)this.ServiceType,
                TransactionVolume = this.TransactionVolume,
                Frequency = (int)this.Frequency,
                TotalCost = this.TotalCost.Amount,
                CurrencyCode = this.CurrencyCode,
                CalculationDate = this.CalculationDate,
                IsArchived = this.IsArchived
            };
            
            // Map country breakdowns
            if (this.CountryBreakdowns != null)
            {
                contractModel.CountryBreakdowns = new List<Contracts.V1.Models.CountryCalculationModel>();
                foreach (var countryModel in this.CountryBreakdowns)
                {
                    contractModel.CountryBreakdowns.Add(new Contracts.V1.Models.CountryCalculationModel
                    {
                        CountryCode = countryModel.CountryCode,
                        CountryName = countryModel.CountryName,
                        BaseCost = countryModel.BaseCost.Amount,
                        AdditionalCost = countryModel.AdditionalCost.Amount,
                        TotalCost = countryModel.TotalCost.Amount,
                        AppliedRules = countryModel.AppliedRules
                    });
                }
            }
            
            contractModel.AdditionalServices = this.AdditionalServices;
            contractModel.Discounts = this.Discounts;
            
            return contractModel;
        }
        
        /// <summary>
        /// Creates a service CalculationModel from a contract CalculationModel
        /// </summary>
        /// <param name="contractModel">The contract model to convert</param>
        /// <returns>A service model populated from the contract model</returns>
        public static CalculationModel FromContractModel(Contracts.V1.Models.CalculationModel contractModel)
        {
            if (contractModel == null)
                throw new ArgumentNullException(nameof(contractModel));
                
            var serviceModel = new CalculationModel
            {
                CalculationId = contractModel.CalculationId,
                UserId = contractModel.UserId,
                ServiceType = (ServiceType)contractModel.ServiceType,
                TransactionVolume = contractModel.TransactionVolume,
                Frequency = (FilingFrequency)contractModel.Frequency,
                TotalCost = Money.Create(contractModel.TotalCost, contractModel.CurrencyCode),
                CalculationDate = contractModel.CalculationDate,
                CurrencyCode = contractModel.CurrencyCode,
                IsArchived = contractModel.IsArchived
            };
            
            // Map country breakdowns
            if (contractModel.CountryBreakdowns != null)
            {
                serviceModel.CountryBreakdowns = new List<CountryCalculationModel>();
                foreach (var countryDto in contractModel.CountryBreakdowns)
                {
                    var countryModel = new CountryCalculationModel
                    {
                        CountryCode = countryDto.CountryCode,
                        CountryName = countryDto.CountryName,
                        BaseCost = Money.Create(countryDto.BaseCost, contractModel.CurrencyCode),
                        AdditionalCost = Money.Create(countryDto.AdditionalCost, contractModel.CurrencyCode),
                        TotalCost = Money.Create(countryDto.TotalCost, contractModel.CurrencyCode),
                        AppliedRules = countryDto.AppliedRules != null ? new List<string>(countryDto.AppliedRules) : new List<string>()
                    };
                    serviceModel.CountryBreakdowns.Add(countryModel);
                }
            }
            
            serviceModel.AdditionalServices = contractModel.AdditionalServices != null 
                ? new List<string>(contractModel.AdditionalServices) 
                : new List<string>();
            
            serviceModel.Discounts = contractModel.Discounts != null 
                ? new Dictionary<string, decimal>(contractModel.Discounts) 
                : new Dictionary<string, decimal>();
            
            return serviceModel;
        }
        
        /// <summary>
        /// Adds a country calculation to the model
        /// </summary>
        /// <param name="countryCalculation">The country calculation to add</param>
        public void AddCountry(CountryCalculationModel countryCalculation)
        {
            if (countryCalculation == null)
                throw new ArgumentNullException(nameof(countryCalculation));
                
            // Check if country already exists
            if (CountryBreakdowns.Any(c => c.CountryCode == countryCalculation.CountryCode))
                throw new InvalidOperationException($"Country {countryCalculation.CountryCode} already exists in this calculation");
                
            CountryBreakdowns.Add(countryCalculation);
            
            // Update total cost
            if (TotalCost == null)
            {
                TotalCost = countryCalculation.TotalCost;
            }
            else
            {
                TotalCost = TotalCost.Add(countryCalculation.TotalCost);
            }
        }
        
        /// <summary>
        /// Removes a country calculation from the model
        /// </summary>
        /// <param name="countryCode">The country code to remove</param>
        /// <returns>True if the country was removed, false if not found</returns>
        public bool RemoveCountry(string countryCode)
        {
            var countryCalculation = CountryBreakdowns.FirstOrDefault(c => c.CountryCode == countryCode);
            if (countryCalculation == null)
                return false;
                
            // Update total cost before removing country
            TotalCost = TotalCost.Subtract(countryCalculation.TotalCost);
            
            CountryBreakdowns.Remove(countryCalculation);
            return true;
        }
        
        /// <summary>
        /// Adds a discount to the calculation
        /// </summary>
        /// <param name="discountName">The name of the discount</param>
        /// <param name="discountPercentage">The discount percentage (0-100)</param>
        /// <returns>The discount amount</returns>
        public Money AddDiscount(string discountName, decimal discountPercentage)
        {
            if (string.IsNullOrEmpty(discountName))
                throw new ArgumentException("Discount name cannot be null or empty", nameof(discountName));
                
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));
                
            // Calculate discount amount
            var discountAmount = TotalCost.Multiply(discountPercentage / 100m);
            
            // Add to discounts dictionary
            Discounts[discountName] = discountPercentage;
            
            // Apply discount to total cost
            TotalCost = TotalCost.Subtract(discountAmount);
            
            return discountAmount;
        }
        
        /// <summary>
        /// Recalculates the total cost based on all countries and discounts
        /// </summary>
        /// <returns>The updated total cost</returns>
        public Money RecalculateTotalCost()
        {
            // Start with zero
            var totalCost = Money.CreateZero(CurrencyCode);
            
            // Add up all country costs
            foreach (var country in CountryBreakdowns)
            {
                totalCost = totalCost.Add(country.TotalCost);
            }
            
            // Apply discounts
            foreach (var discount in Discounts)
            {
                var discountAmount = totalCost.Multiply(discount.Value / 100m);
                totalCost = totalCost.Subtract(discountAmount);
            }
            
            TotalCost = totalCost;
            return totalCost;
        }
        
        // Helper methods
        
        /// <summary>
        /// Helper method to map service ID to ServiceType enum
        /// </summary>
        /// <param name="serviceId">The service identifier string</param>
        /// <returns>The corresponding ServiceType enum value</returns>
        private static ServiceType GetServiceTypeFromId(string serviceId)
        {
            switch (serviceId)
            {
                case "StandardFiling": return ServiceType.StandardFiling;
                case "ComplexFiling": return ServiceType.ComplexFiling;
                case "PriorityService": return ServiceType.PriorityService;
                default: return ServiceType.StandardFiling;
            }
        }
        
        /// <summary>
        /// Helper method to map ServiceType enum to service ID string
        /// </summary>
        /// <param name="serviceType">The ServiceType enum value</param>
        /// <returns>The corresponding service identifier string</returns>
        private static string GetServiceIdFromType(ServiceType serviceType)
        {
            switch (serviceType)
            {
                case ServiceType.StandardFiling: return "StandardFiling";
                case ServiceType.ComplexFiling: return "ComplexFiling";
                case ServiceType.PriorityService: return "PriorityService";
                default: return "StandardFiling";
            }
        }
        
        /// <summary>
        /// Helper method to get country name from country code
        /// </summary>
        /// <param name="countryCode">The ISO country code</param>
        /// <returns>The name of the country</returns>
        private static string GetCountryName(string countryCode)
        {
            // In a real implementation, this would look up the country name from a repository
            switch (countryCode)
            {
                case "GB": return "United Kingdom";
                case "DE": return "Germany";
                case "FR": return "France";
                case "IT": return "Italy";
                case "ES": return "Spain";
                case "NL": return "Netherlands";
                case "BE": return "Belgium";
                case "SE": return "Sweden";
                case "DK": return "Denmark";
                case "PL": return "Poland";
                default: return countryCode;
            }
        }
    }
    
    /// <summary>
    /// Represents the calculation details for a specific country in a VAT filing cost calculation
    /// </summary>
    public class CountryCalculationModel
    {
        /// <summary>
        /// Gets or sets the ISO country code
        /// </summary>
        public string CountryCode { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the country
        /// </summary>
        public string CountryName { get; set; }
        
        /// <summary>
        /// Gets or sets the base cost for VAT filing in this country
        /// </summary>
        public Money BaseCost { get; set; }
        
        /// <summary>
        /// Gets or sets the additional cost for VAT filing in this country
        /// </summary>
        public Money AdditionalCost { get; set; }
        
        /// <summary>
        /// Gets or sets the total cost for VAT filing in this country
        /// </summary>
        public Money TotalCost { get; set; }
        
        /// <summary>
        /// Gets or sets the list of rule IDs that were applied to calculate this country's cost
        /// </summary>
        public List<string> AppliedRules { get; set; }
        
        /// <summary>
        /// Default constructor for the CountryCalculationModel
        /// </summary>
        public CountryCalculationModel()
        {
            AppliedRules = new List<string>();
        }
        
        /// <summary>
        /// Creates a CountryCalculationModel from a domain CalculationCountry entity
        /// </summary>
        /// <param name="entity">The CalculationCountry entity</param>
        /// <param name="countryName">The name of the country</param>
        /// <returns>A new CountryCalculationModel populated from the entity</returns>
        public static CountryCalculationModel FromEntity(CalculationCountry entity, string countryName)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
                
            var model = new CountryCalculationModel
            {
                CountryCode = entity.CountryCode.Value,
                CountryName = countryName,
                TotalCost = entity.CountryCost,
                // We'll split the total cost into base (80%) and additional (20%) for presentation
                BaseCost = entity.CountryCost.Multiply(0.8m),
                AdditionalCost = entity.CountryCost.Multiply(0.2m)
            };
            
            // Parse the applied rules string and add each rule ID to the list
            if (!string.IsNullOrEmpty(entity.AppliedRules))
            {
                string[] ruleIds = entity.AppliedRules.Split(',');
                foreach (string ruleId in ruleIds)
                {
                    model.AppliedRules.Add(ruleId.Trim());
                }
            }
            
            return model;
        }
        
        /// <summary>
        /// Converts the CountryCalculationModel to a domain CalculationCountry entity
        /// </summary>
        /// <param name="calculationId">The ID of the parent calculation</param>
        /// <returns>A new CalculationCountry entity populated from this model</returns>
        public CalculationCountry ToEntity(string calculationId)
        {
            // Use the entity's factory method to create a valid CalculationCountry
            var entity = CalculationCountry.Create(
                calculationId,
                CountryCode, // CountryCode.Create() will be called inside the entity factory method
                TotalCost
            );
            
            // Add applied rules
            foreach (string ruleId in AppliedRules)
            {
                entity.AddAppliedRule(ruleId);
            }
            
            return entity;
        }
        
        /// <summary>
        /// Adds a rule ID to the list of applied rules
        /// </summary>
        /// <param name="ruleId">The rule ID to add</param>
        public void AddAppliedRule(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
                throw new ArgumentException("Rule ID cannot be null or empty", nameof(ruleId));
                
            if (!AppliedRules.Contains(ruleId))
                AppliedRules.Add(ruleId);
        }
        
        /// <summary>
        /// Updates the costs for this country calculation
        /// </summary>
        /// <param name="baseCost">The new base cost</param>
        /// <param name="additionalCost">The new additional cost</param>
        public void UpdateCost(Money baseCost, Money additionalCost)
        {
            if (baseCost == null)
                throw new ArgumentNullException(nameof(baseCost));
                
            if (additionalCost == null)
                throw new ArgumentNullException(nameof(additionalCost));
                
            if (baseCost.Currency != additionalCost.Currency)
                throw new ArgumentException("Base cost and additional cost must use the same currency");
                
            BaseCost = baseCost;
            AdditionalCost = additionalCost;
            TotalCost = baseCost.Add(additionalCost);
        }
    }
}