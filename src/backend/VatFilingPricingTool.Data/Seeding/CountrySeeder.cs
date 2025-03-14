using System.Collections.Generic;
using Microsoft.EntityFrameworkCore; // Version 6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Constants;

namespace VatFilingPricingTool.Data.Seeding
{
    /// <summary>
    /// Static class responsible for seeding country data into the database
    /// </summary>
    public static class CountrySeeder
    {
        /// <summary>
        /// Seeds predefined country data into the database
        /// </summary>
        /// <param name="modelBuilder">Entity Framework ModelBuilder</param>
        public static void SeedCountries(ModelBuilder modelBuilder)
        {
            var countries = new List<Country>
            {
                // United Kingdom
                CreateCountry(
                    "GB", 
                    "United Kingdom", 
                    20.0m, 
                    "GBP", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly }),
                
                // Germany
                CreateCountry(
                    "DE", 
                    "Germany", 
                    19.0m, 
                    "EUR", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly }),
                
                // France
                CreateCountry(
                    "FR", 
                    "France", 
                    20.0m, 
                    "EUR", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly }),
                
                // Italy
                CreateCountry(
                    "IT", 
                    "Italy", 
                    22.0m, 
                    "EUR", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly, FilingFrequency.Annually }),
                
                // Spain
                CreateCountry(
                    "ES", 
                    "Spain", 
                    21.0m, 
                    "EUR", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly }),
                
                // Netherlands
                CreateCountry(
                    "NL", 
                    "Netherlands", 
                    21.0m, 
                    "EUR", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly }),
                
                // Belgium
                CreateCountry(
                    "BE", 
                    "Belgium", 
                    21.0m, 
                    "EUR", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly }),
                
                // Ireland
                CreateCountry(
                    "IE", 
                    "Ireland", 
                    23.0m, 
                    "EUR", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly, FilingFrequency.Annually }),
                
                // Sweden
                CreateCountry(
                    "SE", 
                    "Sweden", 
                    25.0m, 
                    "SEK", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly }),
                
                // Denmark
                CreateCountry(
                    "DK", 
                    "Denmark", 
                    25.0m, 
                    "DKK", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly }),
                
                // United States (included for international comparison)
                CreateCountry(
                    "US", 
                    "United States", 
                    0.0m, 
                    "USD", 
                    new List<FilingFrequency> { FilingFrequency.Monthly, FilingFrequency.Quarterly })
            };

            // Configure the model builder to seed the Countries DbSet with the created country entities
            modelBuilder.Entity<Country>().HasData(countries);
        }

        /// <summary>
        /// Helper method to create a Country entity with the specified parameters
        /// </summary>
        /// <param name="countryCode">ISO country code</param>
        /// <param name="name">Country name</param>
        /// <param name="vatRate">VAT rate percentage</param>
        /// <param name="currencyCode">ISO currency code</param>
        /// <param name="filingFrequencies">List of available filing frequencies</param>
        /// <returns>A configured Country entity</returns>
        private static Country CreateCountry(string countryCode, string name, decimal vatRate, string currencyCode, List<FilingFrequency> filingFrequencies)
        {
            // Create a new Country entity using Country.Create factory method
            var country = Country.Create(countryCode, name, vatRate, currencyCode);
            
            // For each filing frequency in the filingFrequencies list, add it to the country
            foreach (var frequency in filingFrequencies)
            {
                country.AddFilingFrequency(frequency);
            }
            
            return country;
        }
    }
}