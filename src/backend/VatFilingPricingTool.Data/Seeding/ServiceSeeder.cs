using Microsoft.EntityFrameworkCore; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Data.Seeding
{
    /// <summary>
    /// Static class responsible for seeding service data into the database
    /// </summary>
    public static class ServiceSeeder
    {
        /// <summary>
        /// Seeds predefined service types into the database
        /// </summary>
        /// <param name="modelBuilder">The model builder to configure</param>
        public static void SeedServices(ModelBuilder modelBuilder)
        {
            var services = new List<Service>
            {
                CreateService(
                    "Standard Filing",
                    "Basic VAT filing service with standard processing times and validation",
                    800,
                    "EUR",
                    ServiceType.StandardFiling,
                    1),

                CreateService(
                    "Complex Filing",
                    "Enhanced filing service for businesses with complex tax situations, including additional verification and reconciliation",
                    1200,
                    "EUR",
                    ServiceType.ComplexFiling,
                    3),

                CreateService(
                    "Priority Service",
                    "Premium service with expedited processing, enhanced validation, and dedicated support",
                    1500,
                    "EUR",
                    ServiceType.PriorityService,
                    5)
            };

            modelBuilder.Entity<Service>().HasData(services);
        }

        /// <summary>
        /// Seeds predefined additional services into the database
        /// </summary>
        /// <param name="modelBuilder">The model builder to configure</param>
        public static void CreateAdditionalServices(ModelBuilder modelBuilder)
        {
            var additionalServices = new List<AdditionalService>
            {
                AdditionalService.CreateTaxConsultancy(),
                AdditionalService.CreateHistoricalDataProcessing(),
                AdditionalService.CreateReconciliationService(),
                AdditionalService.CreateAuditSupport()
            };

            modelBuilder.Entity<AdditionalService>().HasData(additionalServices);
        }

        /// <summary>
        /// Helper method to create a Service entity with the specified parameters
        /// </summary>
        /// <param name="name">Service name</param>
        /// <param name="description">Service description</param>
        /// <param name="basePrice">Base price of the service</param>
        /// <param name="currency">Currency code</param>
        /// <param name="serviceType">Type of service</param>
        /// <param name="complexityLevel">Complexity level (1-10)</param>
        /// <returns>A configured Service entity</returns>
        private static Service CreateService(string name, string description, decimal basePrice, string currency, ServiceType serviceType, int complexityLevel)
        {
            return Service.Create(name, description, basePrice, currency, serviceType, complexityLevel);
        }
    }
}