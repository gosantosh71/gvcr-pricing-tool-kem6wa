using System;
using System.Collections.Generic;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Domain.Constants;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.Domain.Entities
{
    /// <summary>
    /// Represents an additional service that can be added to a VAT filing calculation
    /// </summary>
    public class AdditionalService
    {
        /// <summary>
        /// Gets or sets the unique identifier for the service
        /// </summary>
        public string ServiceId { get; private set; }

        /// <summary>
        /// Gets or sets the name of the service
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description of the service
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the cost of the service
        /// </summary>
        public Money Cost { get; private set; }

        /// <summary>
        /// Gets or sets whether the service is active and available for selection
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets the collection of calculation-additional service relationships
        /// </summary>
        public ICollection<CalculationAdditionalService> CalculationAdditionalServices { get; private set; }

        /// <summary>
        /// Default constructor for the AdditionalService entity
        /// </summary>
        public AdditionalService()
        {
            CalculationAdditionalServices = new HashSet<CalculationAdditionalService>();
        }

        /// <summary>
        /// Factory method to create a new AdditionalService instance
        /// </summary>
        /// <param name="name">The name of the service</param>
        /// <param name="description">The description of the service</param>
        /// <param name="cost">The cost of the service</param>
        /// <returns>A new AdditionalService instance</returns>
        public static AdditionalService Create(string name, string description, Money cost)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ValidationException("Service name is required", 
                    new List<string> { "Name is required" });
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ValidationException("Service description is required", 
                    new List<string> { "Description is required" });
            }

            if (cost == null || cost.Amount <= 0)
            {
                throw new ValidationException("Service cost must be greater than zero", 
                    new List<string> { "Cost must be greater than zero" });
            }

            var service = new AdditionalService
            {
                ServiceId = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                Cost = cost,
                IsActive = true
            };

            service.Validate();
            return service;
        }

        /// <summary>
        /// Factory method to create a tax consultancy service
        /// </summary>
        /// <returns>A new tax consultancy service instance</returns>
        public static AdditionalService CreateTaxConsultancy()
        {
            return new AdditionalService
            {
                ServiceId = Guid.NewGuid().ToString(),
                Name = "Tax Consultancy",
                Description = "Professional tax advice and consultation services",
                Cost = Money.Create(300, "EUR"),
                IsActive = true
            };
        }

        /// <summary>
        /// Factory method to create a historical data processing service
        /// </summary>
        /// <returns>A new historical data processing service instance</returns>
        public static AdditionalService CreateHistoricalDataProcessing()
        {
            return new AdditionalService
            {
                ServiceId = Guid.NewGuid().ToString(),
                Name = "Historical Data Processing",
                Description = "Processing and analysis of historical VAT filing data",
                Cost = Money.Create(250, "EUR"),
                IsActive = true
            };
        }

        /// <summary>
        /// Factory method to create a reconciliation service
        /// </summary>
        /// <returns>A new reconciliation service instance</returns>
        public static AdditionalService CreateReconciliationService()
        {
            return new AdditionalService
            {
                ServiceId = Guid.NewGuid().ToString(),
                Name = "Reconciliation Services",
                Description = "Reconciliation of VAT accounts and transactions",
                Cost = Money.Create(350, "EUR"),
                IsActive = true
            };
        }

        /// <summary>
        /// Factory method to create an audit support service
        /// </summary>
        /// <returns>A new audit support service instance</returns>
        public static AdditionalService CreateAuditSupport()
        {
            return new AdditionalService
            {
                ServiceId = Guid.NewGuid().ToString(),
                Name = "Audit Support",
                Description = "Support during tax authority audits and inquiries",
                Cost = Money.Create(400, "EUR"),
                IsActive = true
            };
        }

        /// <summary>
        /// Updates the service details
        /// </summary>
        /// <param name="name">The new service name</param>
        /// <param name="description">The new service description</param>
        public void UpdateDetails(string name, string description)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ValidationException("Service name is required", 
                    new List<string> { "Name is required" });
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ValidationException("Service description is required", 
                    new List<string> { "Description is required" });
            }

            Name = name;
            Description = description;
            Validate();
        }

        /// <summary>
        /// Updates the cost of the service
        /// </summary>
        /// <param name="newCost">The new cost of the service</param>
        public void UpdateCost(Money newCost)
        {
            if (newCost == null)
            {
                throw new ValidationException("Service cost is required", 
                    new List<string> { "Cost is required" });
            }

            if (newCost.Amount <= 0)
            {
                throw new ValidationException("Service cost must be greater than zero", 
                    new List<string> { "Cost must be greater than zero" });
            }

            Cost = newCost;
        }

        /// <summary>
        /// Activates the service
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Deactivates the service
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Validates the service data
        /// </summary>
        private void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(Name))
            {
                errors.Add("Name is required");
            }
            else if (Name.Length < DomainConstants.Validation.MinNameLength || 
                     Name.Length > DomainConstants.Validation.MaxNameLength)
            {
                errors.Add($"Name length must be between {DomainConstants.Validation.MinNameLength} and {DomainConstants.Validation.MaxNameLength} characters");
            }

            if (string.IsNullOrEmpty(Description))
            {
                errors.Add("Description is required");
            }
            else if (Description.Length < DomainConstants.Validation.MinDescriptionLength || 
                     Description.Length > DomainConstants.Validation.MaxDescriptionLength)
            {
                errors.Add($"Description length must be between {DomainConstants.Validation.MinDescriptionLength} and {DomainConstants.Validation.MaxDescriptionLength} characters");
            }

            if (Cost == null || Cost.Amount <= 0)
            {
                errors.Add("Cost must be greater than zero");
            }

            if (errors.Count > 0)
            {
                throw new ValidationException("Additional service validation failed", errors);
            }
        }
    }

    /// <summary>
    /// Represents the relationship between a calculation and an additional service
    /// </summary>
    public class CalculationAdditionalService
    {
        /// <summary>
        /// Gets or sets the calculation identifier
        /// </summary>
        public string CalculationId { get; private set; }

        /// <summary>
        /// Gets or sets the additional service identifier
        /// </summary>
        public string AdditionalServiceId { get; private set; }

        /// <summary>
        /// Gets or sets the calculation
        /// </summary>
        public Calculation Calculation { get; private set; }

        /// <summary>
        /// Gets or sets the additional service
        /// </summary>
        public AdditionalService AdditionalService { get; private set; }

        /// <summary>
        /// Gets or sets the cost of the additional service for this calculation
        /// </summary>
        public Money Cost { get; private set; }

        /// <summary>
        /// Default constructor for the CalculationAdditionalService entity
        /// </summary>
        public CalculationAdditionalService()
        {
        }

        /// <summary>
        /// Factory method to create a new CalculationAdditionalService instance
        /// </summary>
        /// <param name="calculationId">The calculation identifier</param>
        /// <param name="additionalServiceId">The additional service identifier</param>
        /// <param name="cost">The cost of the additional service for this calculation</param>
        /// <returns>A new CalculationAdditionalService instance</returns>
        public static CalculationAdditionalService Create(string calculationId, string additionalServiceId, Money cost)
        {
            if (string.IsNullOrEmpty(calculationId))
            {
                throw new ValidationException("Calculation ID is required", 
                    new List<string> { "Calculation ID is required" });
            }

            if (string.IsNullOrEmpty(additionalServiceId))
            {
                throw new ValidationException("Additional service ID is required", 
                    new List<string> { "Additional service ID is required" });
            }

            if (cost == null || cost.Amount <= 0)
            {
                throw new ValidationException("Cost must be greater than zero", 
                    new List<string> { "Cost must be greater than zero" });
            }

            return new CalculationAdditionalService
            {
                CalculationId = calculationId,
                AdditionalServiceId = additionalServiceId,
                Cost = cost
            };
        }
    }
}