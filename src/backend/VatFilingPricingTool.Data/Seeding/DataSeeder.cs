using Microsoft.EntityFrameworkCore; // Version 6.0.0
using System;
using System.Collections.Generic;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Data.Seeding
{
    /// <summary>
    /// Static class responsible for orchestrating the seeding of all initial data into the database
    /// </summary>
    public static class DataSeeder
    {
        /// <summary>
        /// Coordinates the seeding of all initial data into the database in the correct order
        /// </summary>
        /// <param name="modelBuilder">The model builder to configure</param>
        public static void SeedAllData(ModelBuilder modelBuilder)
        {
            // Seed countries first since rules and other data depend on countries
            CountrySeeder.SeedCountries(modelBuilder);
            
            // Seed service types
            ServiceSeeder.SeedServices(modelBuilder);
            
            // Seed additional services
            ServiceSeeder.CreateAdditionalServices(modelBuilder);
            
            // Seed initial VAT rules
            SeedInitialRules(modelBuilder);
        }
        
        /// <summary>
        /// Seeds initial VAT rules for supported countries
        /// </summary>
        /// <param name="modelBuilder">The model builder to configure</param>
        private static void SeedInitialRules(ModelBuilder modelBuilder)
        {
            var rules = new List<Rule>();
            
            // Base VAT rate rules for each country
            rules.Add(CreateBaseRateRule("GB", "basePrice * 0.20"));
            rules.Add(CreateBaseRateRule("DE", "basePrice * 0.19"));
            rules.Add(CreateBaseRateRule("FR", "basePrice * 0.20"));
            rules.Add(CreateBaseRateRule("IT", "basePrice * 0.22"));
            rules.Add(CreateBaseRateRule("ES", "basePrice * 0.21"));
            
            // Transaction volume threshold rules
            rules.Add(CreateVolumeThresholdRule("GB", 100, 1.2m));
            rules.Add(CreateVolumeThresholdRule("GB", 500, 1.5m));
            rules.Add(CreateVolumeThresholdRule("GB", 1000, 2.0m));
            
            rules.Add(CreateVolumeThresholdRule("DE", 100, 1.2m));
            rules.Add(CreateVolumeThresholdRule("DE", 500, 1.5m));
            rules.Add(CreateVolumeThresholdRule("DE", 1000, 2.0m));
            
            rules.Add(CreateVolumeThresholdRule("FR", 100, 1.2m));
            rules.Add(CreateVolumeThresholdRule("FR", 500, 1.5m));
            rules.Add(CreateVolumeThresholdRule("FR", 1000, 2.0m));
            
            // Service complexity rules
            rules.Add(CreateComplexityRule("GB", ServiceType.ComplexFiling, 1.5m));
            rules.Add(CreateComplexityRule("GB", ServiceType.PriorityService, 2.0m));
            
            rules.Add(CreateComplexityRule("DE", ServiceType.ComplexFiling, 1.5m));
            rules.Add(CreateComplexityRule("DE", ServiceType.PriorityService, 2.0m));
            
            rules.Add(CreateComplexityRule("FR", ServiceType.ComplexFiling, 1.5m));
            rules.Add(CreateComplexityRule("FR", ServiceType.PriorityService, 2.0m));
            
            // Multi-country discount rules
            rules.Add(CreateMultiCountryDiscountRule(3, 5.0m));
            rules.Add(CreateMultiCountryDiscountRule(5, 10.0m));
            
            // Configure the model builder to seed the Rules DbSet with the created rule entities
            modelBuilder.Entity<Rule>().HasData(rules);
        }
        
        /// <summary>
        /// Helper method to create a base VAT rate rule for a country
        /// </summary>
        /// <param name="countryCode">Country code</param>
        /// <param name="expression">Rule expression</param>
        /// <returns>A configured Rule entity for base VAT rate</returns>
        private static Rule CreateBaseRateRule(string countryCode, string expression)
        {
            var rule = Rule.Create(
                countryCode, 
                RuleType.VatRate, 
                $"{countryCode} Base VAT Rate", 
                expression, 
                DateTime.UtcNow.Date);
            
            // Add base price parameter
            rule.AddParameter("basePrice", "number");
            
            // Set high priority to ensure base rate rules are applied first
            rule.UpdatePriority(10);
            
            return rule;
        }
        
        /// <summary>
        /// Helper method to create a transaction volume threshold rule
        /// </summary>
        /// <param name="countryCode">Country code</param>
        /// <param name="threshold">Transaction volume threshold</param>
        /// <param name="multiplier">Price multiplier to apply</param>
        /// <returns>A configured Rule entity for volume threshold</returns>
        private static Rule CreateVolumeThresholdRule(string countryCode, int threshold, decimal multiplier)
        {
            var rule = Rule.Create(
                countryCode, 
                RuleType.Threshold, 
                $"{countryCode} Volume Threshold {threshold}", 
                $"basePrice * {multiplier}", 
                DateTime.UtcNow.Date);
            
            // Add base price parameter
            rule.AddParameter("basePrice", "number");
            
            // Add transaction volume parameter
            rule.AddParameter("transactionVolume", "number");
            
            // Add condition to check transaction volume
            rule.AddCondition("transactionVolume", "greaterThan", threshold.ToString());
            
            return rule;
        }
        
        /// <summary>
        /// Helper method to create a service complexity rule
        /// </summary>
        /// <param name="countryCode">Country code</param>
        /// <param name="serviceType">Service type</param>
        /// <param name="multiplier">Price multiplier to apply</param>
        /// <returns>A configured Rule entity for service complexity</returns>
        private static Rule CreateComplexityRule(string countryCode, ServiceType serviceType, decimal multiplier)
        {
            var rule = Rule.Create(
                countryCode, 
                RuleType.Complexity, 
                $"{countryCode} {serviceType} Complexity", 
                $"basePrice * {multiplier}", 
                DateTime.UtcNow.Date);
            
            // Add base price parameter
            rule.AddParameter("basePrice", "number");
            
            // Add service type parameter
            rule.AddParameter("serviceType", "string");
            
            // Add condition to check service type
            rule.AddCondition("serviceType", "equals", serviceType.ToString());
            
            return rule;
        }
        
        /// <summary>
        /// Helper method to create a multi-country discount rule
        /// </summary>
        /// <param name="countryCount">Minimum number of countries for discount</param>
        /// <param name="discountPercentage">Discount percentage to apply</param>
        /// <returns>A configured Rule entity for multi-country discount</returns>
        private static Rule CreateMultiCountryDiscountRule(int countryCount, decimal discountPercentage)
        {
            // Using GB as the primary country for multi-country discount rules
            var rule = Rule.Create(
                "GB", 
                RuleType.Discount, 
                $"Multi-Country Discount ({countryCount}+ countries)", 
                $"basePrice * {discountPercentage / 100}", 
                DateTime.UtcNow.Date);
            
            // Add base price parameter
            rule.AddParameter("basePrice", "number");
            
            // Add country count parameter
            rule.AddParameter("countryCount", "number");
            
            // Add condition to check country count
            rule.AddCondition("countryCount", "greaterThanOrEqual", countryCount.ToString());
            
            return rule;
        }
    }
}