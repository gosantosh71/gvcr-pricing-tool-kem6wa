using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Rules;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Service.Models;

namespace VatFilingPricingTool.Service.Helpers
{
    /// <summary>
    /// Static helper class that provides utility methods for VAT filing pricing calculations
    /// </summary>
    public static class CalculationHelper
    {
        /// <summary>
        /// Validates a calculation request to ensure all required parameters are provided and valid
        /// </summary>
        /// <param name="request">The calculation request to validate</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public static void ValidateCalculationRequest(CalculateRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                throw new ValidationException("Calculation request cannot be null", 
                    new List<string> { "Request is required" },
                    ErrorCodes.Pricing.InvalidParameters);
            }

            if (request.TransactionVolume <= 0)
            {
                errors.Add($"Transaction volume must be greater than zero. Provided value: {request.TransactionVolume}");
            }

            if (request.CountryCodes == null || !request.CountryCodes.Any())
            {
                errors.Add("At least one country must be selected");
            }

            if (errors.Count > 0)
            {
                throw new ValidationException("Invalid calculation parameters", errors, ErrorCodes.Pricing.InvalidParameters);
            }
        }

        /// <summary>
        /// Creates a Calculation entity from a calculation request
        /// </summary>
        /// <param name="request">The calculation request</param>
        /// <param name="userId">The ID of the user making the request</param>
        /// <param name="serviceId">The ID of the service type</param>
        /// <param name="currencyCode">The currency code to use for the calculation</param>
        /// <returns>A new Calculation entity populated from the request</returns>
        /// <exception cref="ValidationException">Thrown when parameters are invalid</exception>
        public static Calculation CreateCalculationFromRequest(CalculateRequest request, string userId, string serviceId, string currencyCode)
        {
            if (request == null)
            {
                throw new ValidationException("Calculation request cannot be null", 
                    new List<string> { "Request is required" },
                    ErrorCodes.Pricing.InvalidParameters);
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ValidationException("User ID cannot be null or empty",
                    new List<string> { "UserId is required" },
                    ErrorCodes.Pricing.InvalidParameters);
            }

            if (string.IsNullOrEmpty(serviceId))
            {
                throw new ValidationException("Service ID cannot be null or empty",
                    new List<string> { "ServiceId is required" },
                    ErrorCodes.Pricing.InvalidParameters);
            }

            if (string.IsNullOrEmpty(currencyCode))
            {
                throw new ValidationException("Currency code cannot be null or empty",
                    new List<string> { "CurrencyCode is required" },
                    ErrorCodes.Pricing.InvalidParameters);
            }

            var calculation = Calculation.Create(
                userId, 
                serviceId, 
                request.TransactionVolume, 
                request.Frequency,
                currencyCode
            );

            return calculation;
        }

        /// <summary>
        /// Calculates costs for each country in the calculation using the rule engine
        /// </summary>
        /// <param name="calculation">The calculation entity to update</param>
        /// <param name="countries">The list of countries to include in the calculation</param>
        /// <param name="ruleEngine">The rule engine to use for country-specific calculations</param>
        /// <param name="parameters">Parameters for rule evaluation</param>
        /// <exception cref="ValidationException">Thrown when parameters are invalid</exception>
        public static void CalculateCountryCosts(Calculation calculation, IEnumerable<Country> countries, IRuleEngine ruleEngine, Dictionary<string, object> parameters)
        {
            if (calculation == null)
            {
                throw new ValidationException("Calculation cannot be null",
                    new List<string> { "Calculation is required" },
                    ErrorCodes.Pricing.CalculationFailed);
            }

            if (countries == null || !countries.Any())
            {
                throw new ValidationException("Countries list cannot be null or empty",
                    new List<string> { "At least one country must be provided" },
                    ErrorCodes.Pricing.InvalidParameters);
            }

            if (ruleEngine == null)
            {
                throw new ValidationException("Rule engine cannot be null",
                    new List<string> { "RuleEngine is required" },
                    ErrorCodes.Pricing.CalculationFailed);
            }

            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            foreach (var country in countries)
            {
                // Get country-specific cost using the rule engine
                var countryCost = ruleEngine.CalculateCountryCost(
                    country.Code, 
                    parameters, 
                    DateTime.UtcNow
                );

                // Add the country to the calculation with its cost
                var calculationCountry = calculation.AddCountry(country.Code, countryCost);

                // Get applicable rules for this country
                var applicableRules = ruleEngine.GetApplicableRules(country.Code, DateTime.UtcNow);

                // Add each rule ID to the applied rules for this country
                foreach (var rule in applicableRules)
                {
                    calculationCountry.AddAppliedRule(rule.RuleId);
                }
            }

            // Recalculate the total cost to ensure consistency
            calculation.RecalculateTotalCost();
        }

        /// <summary>
        /// Applies transaction volume-based discounts to the calculation
        /// </summary>
        /// <param name="calculation">The calculation to apply discounts to</param>
        /// <exception cref="ValidationException">Thrown when calculation is null</exception>
        public static void ApplyVolumeDiscounts(Calculation calculation)
        {
            if (calculation == null)
            {
                throw new ValidationException("Calculation cannot be null",
                    new List<string> { "Calculation is required" },
                    ErrorCodes.Pricing.CalculationFailed);
            }

            decimal discountPercentage = 0;
            string discountReason = "";

            // Determine discount based on transaction volume tiers
            if (calculation.TransactionVolume > 1000)
            {
                discountPercentage = 15; // 15% discount for very high volume
                discountReason = "High Volume Discount (>1000 transactions)";
            }
            else if (calculation.TransactionVolume > 500)
            {
                discountPercentage = 10; // 10% discount for high volume
                discountReason = "Volume Discount (>500 transactions)";
            }
            else if (calculation.TransactionVolume > 100)
            {
                discountPercentage = 5; // 5% discount for medium volume
                discountReason = "Volume Discount (>100 transactions)";
            }

            // Apply discount if applicable
            if (discountPercentage > 0)
            {
                calculation.ApplyDiscount(discountPercentage, discountReason);
            }

            // Recalculate the total cost
            calculation.RecalculateTotalCost();
        }

        /// <summary>
        /// Applies discounts based on the number of countries in the calculation
        /// </summary>
        /// <param name="calculation">The calculation to apply discounts to</param>
        /// <exception cref="ValidationException">Thrown when calculation is null</exception>
        public static void ApplyMultiCountryDiscounts(Calculation calculation)
        {
            if (calculation == null)
            {
                throw new ValidationException("Calculation cannot be null",
                    new List<string> { "Calculation is required" },
                    ErrorCodes.Pricing.CalculationFailed);
            }

            int countryCount = calculation.GetCountriesCount();
            decimal discountPercentage = 0;
            string discountReason = "";

            // Determine discount based on number of countries
            if (countryCount >= 10)
            {
                discountPercentage = 20; // 20% discount for 10+ countries
                discountReason = "Multi-Country Discount (10+ countries)";
            }
            else if (countryCount >= 5)
            {
                discountPercentage = 15; // 15% discount for 5-9 countries
                discountReason = "Multi-Country Discount (5+ countries)";
            }
            else if (countryCount >= 3)
            {
                discountPercentage = 10; // 10% discount for 3-4 countries
                discountReason = "Multi-Country Discount (3+ countries)";
            }

            // Apply discount if applicable
            if (discountPercentage > 0)
            {
                calculation.ApplyDiscount(discountPercentage, discountReason);
            }

            // Recalculate the total cost
            calculation.RecalculateTotalCost();
        }

        /// <summary>
        /// Adds additional services to the calculation with their costs
        /// </summary>
        /// <param name="calculation">The calculation to add services to</param>
        /// <param name="additionalServices">Dictionary of available additional services with their costs</param>
        /// <param name="requestedServices">List of requested service IDs</param>
        /// <exception cref="ValidationException">Thrown when parameters are invalid</exception>
        public static void AddAdditionalServices(
            Calculation calculation, 
            Dictionary<string, (string Name, decimal Cost)> additionalServices,
            IEnumerable<string> requestedServices)
        {
            if (calculation == null)
            {
                throw new ValidationException("Calculation cannot be null",
                    new List<string> { "Calculation is required" },
                    ErrorCodes.Pricing.CalculationFailed);
            }

            if (additionalServices == null)
            {
                throw new ValidationException("Additional services dictionary cannot be null",
                    new List<string> { "AdditionalServices is required" },
                    ErrorCodes.Pricing.InvalidParameters);
            }

            if (requestedServices == null)
            {
                // No services requested, nothing to do
                return;
            }

            foreach (var serviceId in requestedServices)
            {
                if (additionalServices.TryGetValue(serviceId, out var service))
                {
                    // Create Money object for the service cost
                    var serviceCost = Money.Create(service.Cost, calculation.CurrencyCode);
                    
                    // Add the additional service to the calculation
                    calculation.AddAdditionalService(serviceId, serviceCost);
                }
            }

            // Recalculate the total cost
            calculation.RecalculateTotalCost();
        }

        /// <summary>
        /// Creates a service layer CalculationModel from a domain Calculation entity
        /// </summary>
        /// <param name="entity">The Calculation entity</param>
        /// <param name="countries">List of countries for name lookup</param>
        /// <returns>A service layer model populated from the entity</returns>
        /// <exception cref="ValidationException">Thrown when entity is null</exception>
        public static CalculationModel CreateCalculationModelFromEntity(Calculation entity, IEnumerable<Country> countries)
        {
            if (entity == null)
            {
                throw new ValidationException("Calculation entity cannot be null",
                    new List<string> { "Entity is required" },
                    ErrorCodes.Pricing.CalculationNotFound);
            }

            var model = CalculationModel.FromEntity(entity);

            // Add country calculation models with proper country names
            foreach (var calculationCountry in entity.CalculationCountries)
            {
                var country = countries.FirstOrDefault(c => c.Code.Value == calculationCountry.CountryCode.Value);
                if (country != null)
                {
                    var countryModel = CountryCalculationModel.FromEntity(calculationCountry, country.Name);
                    model.CountryBreakdowns.Add(countryModel);
                }
            }

            return model;
        }

        /// <summary>
        /// Creates a contract CalculationResponse from a service layer CalculationModel
        /// </summary>
        /// <param name="model">The service layer calculation model</param>
        /// <returns>A contract response model for API</returns>
        /// <exception cref="ValidationException">Thrown when model is null</exception>
        public static Contracts.V1.Responses.CalculationResponse CreateCalculationResponseFromModel(CalculationModel model)
        {
            if (model == null)
            {
                throw new ValidationException("Calculation model cannot be null",
                    new List<string> { "Model is required" },
                    ErrorCodes.Pricing.CalculationNotFound);
            }

            // Convert service model to contract model
            var contractModel = model.ToContractModel();

            // Create and return response object
            return new Contracts.V1.Responses.CalculationResponse
            {
                CalculationId = contractModel.CalculationId,
                ServiceType = contractModel.ServiceType,
                TransactionVolume = contractModel.TransactionVolume,
                Frequency = contractModel.Frequency,
                TotalCost = contractModel.TotalCost,
                CurrencyCode = contractModel.CurrencyCode,
                CalculationDate = contractModel.CalculationDate,
                CountryBreakdowns = contractModel.CountryBreakdowns,
                AdditionalServices = contractModel.AdditionalServices,
                Discounts = contractModel.Discounts,
                IsSuccess = true
            };
        }

        /// <summary>
        /// Creates a parameter dictionary for rule evaluation based on calculation properties
        /// </summary>
        /// <param name="calculation">The calculation entity</param>
        /// <param name="serviceType">The service type string</param>
        /// <returns>Parameters for rule evaluation</returns>
        /// <exception cref="ValidationException">Thrown when calculation is null</exception>
        public static Dictionary<string, object> GetCalculationParameters(Calculation calculation, string serviceType)
        {
            if (calculation == null)
            {
                throw new ValidationException("Calculation cannot be null",
                    new List<string> { "Calculation is required" },
                    ErrorCodes.Pricing.CalculationFailed);
            }

            var parameters = new Dictionary<string, object>
            {
                // Add relevant calculation parameters for rule evaluation
                ["transactionVolume"] = calculation.TransactionVolume,
                ["filingFrequency"] = calculation.FilingFrequency.ToString(),
                ["serviceType"] = serviceType,
                ["countriesCount"] = calculation.GetCountriesCount(),
                ["additionalServicesCount"] = calculation.GetAdditionalServicesCount(),
                ["currencyCode"] = calculation.CurrencyCode
            };

            // Add base price parameter - typically used in pricing calculations
            parameters["basePrice"] = 100.0m; // Default base price, should be configured per service type in a real implementation

            // Adjust base price based on service type
            if (serviceType == "ComplexFiling")
            {
                parameters["basePrice"] = 200.0m;
            }
            else if (serviceType == "PriorityService")
            {
                parameters["basePrice"] = 300.0m;
            }

            return parameters;
        }
    }
}