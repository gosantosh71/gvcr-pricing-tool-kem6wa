using System;
using System.Collections.Generic;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Domain.Entities
{
    /// <summary>
    /// Represents a VAT filing service type with pricing and complexity information.
    /// This entity is a core domain model for the VAT Filing Pricing Tool.
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Gets or sets the unique identifier for the service.
        /// </summary>
        public string ServiceId { get; private set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description of the service.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the base price of the service.
        /// </summary>
        public Money BasePrice { get; private set; }

        /// <summary>
        /// Gets or sets the service type.
        /// </summary>
        public ServiceType ServiceType { get; private set; }

        /// <summary>
        /// Gets or sets the complexity level of the service (1-10).
        /// </summary>
        public int ComplexityLevel { get; private set; }

        /// <summary>
        /// Gets or sets whether the service is active.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets the collection of calculations using this service.
        /// </summary>
        public ICollection<Calculation> Calculations { get; private set; }

        /// <summary>
        /// Default constructor for the Service entity.
        /// </summary>
        public Service()
        {
            Calculations = new HashSet<Calculation>();
            IsActive = true;
        }

        /// <summary>
        /// Factory method to create a new Service instance.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="basePrice">The base price of the service.</param>
        /// <param name="currency">The currency code for the base price.</param>
        /// <param name="serviceType">The type of service.</param>
        /// <param name="complexityLevel">The complexity level of the service (1-10).</param>
        /// <returns>A new Service instance.</returns>
        public static Service Create(string name, string description, decimal basePrice, string currency, ServiceType serviceType, int complexityLevel)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ValidationException("Service name cannot be null or empty.", 
                    new List<string> { "Service name is required." });
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ValidationException("Service description cannot be null or empty.", 
                    new List<string> { "Service description is required." });
            }

            if (basePrice <= 0)
            {
                throw new ValidationException("Service base price must be greater than zero.", 
                    new List<string> { "Base price must be a positive value." });
            }

            if (string.IsNullOrEmpty(currency))
            {
                throw new ValidationException("Currency code cannot be null or empty.", 
                    new List<string> { "Currency code is required." });
            }

            if (complexityLevel < 1 || complexityLevel > 10)
            {
                throw new ValidationException("Complexity level must be between 1 and 10.", 
                    new List<string> { "Valid complexity levels range from 1 (simplest) to 10 (most complex)." });
            }

            var service = new Service
            {
                ServiceId = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                BasePrice = Money.Create(basePrice, currency),
                ServiceType = serviceType,
                ComplexityLevel = complexityLevel,
                IsActive = true
            };

            return service;
        }

        /// <summary>
        /// Updates the service properties.
        /// </summary>
        /// <param name="name">The updated name.</param>
        /// <param name="description">The updated description.</param>
        /// <param name="basePrice">The updated base price.</param>
        /// <param name="serviceType">The updated service type.</param>
        /// <param name="complexityLevel">The updated complexity level.</param>
        public void Update(string name, string description, Money basePrice, ServiceType serviceType, int complexityLevel)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ValidationException("Service name cannot be null or empty.",
                    new List<string> { "Service name is required." });
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ValidationException("Service description cannot be null or empty.",
                    new List<string> { "Service description is required." });
            }

            if (basePrice == null)
            {
                throw new ValidationException("Base price cannot be null.",
                    new List<string> { "Base price is required." });
            }

            if (complexityLevel < 1 || complexityLevel > 10)
            {
                throw new ValidationException("Complexity level must be between 1 and 10.",
                    new List<string> { "Valid complexity levels range from 1 (simplest) to 10 (most complex)." });
            }

            Name = name;
            Description = description;
            BasePrice = basePrice;
            ServiceType = serviceType;
            ComplexityLevel = complexityLevel;
        }

        /// <summary>
        /// Updates the base price of the service.
        /// </summary>
        /// <param name="newBasePrice">The new base price.</param>
        public void UpdateBasePrice(Money newBasePrice)
        {
            if (newBasePrice == null)
            {
                throw new ValidationException("Base price cannot be null.",
                    new List<string> { "Base price is required." });
            }

            BasePrice = newBasePrice;
        }

        /// <summary>
        /// Updates the complexity level of the service.
        /// </summary>
        /// <param name="newComplexityLevel">The new complexity level (1-10).</param>
        public void UpdateComplexityLevel(int newComplexityLevel)
        {
            if (newComplexityLevel < 1 || newComplexityLevel > 10)
            {
                throw new ValidationException("Complexity level must be between 1 and 10.",
                    new List<string> { "Valid complexity levels range from 1 (simplest) to 10 (most complex)." });
            }

            ComplexityLevel = newComplexityLevel;
        }

        /// <summary>
        /// Activates the service.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Deactivates the service.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Calculates the price for a specific transaction volume.
        /// </summary>
        /// <param name="transactionVolume">The number of transactions.</param>
        /// <returns>The calculated price.</returns>
        public Money CalculatePriceForVolume(int transactionVolume)
        {
            if (transactionVolume <= 0)
            {
                throw new ValidationException("Transaction volume must be greater than zero.",
                    new List<string> { "Please provide a positive transaction volume." });
            }

            // Apply volume-based scaling
            decimal volumeFactor = 1.0m;
            
            // Volume-based scaling logic
            if (transactionVolume <= 100)
            {
                volumeFactor = 1.0m;
            }
            else if (transactionVolume <= 500)
            {
                volumeFactor = 1.2m;
            }
            else if (transactionVolume <= 1000)
            {
                volumeFactor = 1.5m;
            }
            else
            {
                volumeFactor = 2.0m;
            }

            // Apply complexity factor
            decimal complexityFactor = 1.0m + ((ComplexityLevel - 1) * 0.1m);

            // Calculate final price
            return BasePrice.Multiply(volumeFactor * complexityFactor);
        }

        /// <summary>
        /// Validates the service data.
        /// </summary>
        private void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(ServiceId))
            {
                errors.Add("Service ID is required.");
            }

            if (string.IsNullOrEmpty(Name))
            {
                errors.Add("Service name is required.");
            }

            if (string.IsNullOrEmpty(Description))
            {
                errors.Add("Service description is required.");
            }

            if (BasePrice == null)
            {
                errors.Add("Base price is required.");
            }

            if (ComplexityLevel < 1 || ComplexityLevel > 10)
            {
                errors.Add("Complexity level must be between 1 and 10.");
            }

            if (errors.Count > 0)
            {
                throw new ValidationException("Service validation failed.", errors);
            }
        }
    }
}