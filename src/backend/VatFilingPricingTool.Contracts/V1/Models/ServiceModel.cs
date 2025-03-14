using System; // Version 6.0.0
using System.ComponentModel.DataAnnotations; // Version 6.0.0
using System.Text.Json.Serialization; // Version 6.0.0
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Models
{
    /// <summary>
    /// Represents a VAT filing service type with pricing and complexity information.
    /// This model serves as a contract between the API and clients, providing a standardized
    /// representation of service data for pricing calculations.
    /// </summary>
    public class ServiceModel
    {
        /// <summary>
        /// Unique identifier for the service.
        /// </summary>
        [Required]
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Name of the VAT filing service.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the service, including its features and benefits.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Base price of the service before any adjustments.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Currency code for the base price (e.g., USD, EUR, GBP).
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CurrencyCode { get; set; } = "EUR";

        /// <summary>
        /// Type of VAT filing service (StandardFiling, ComplexFiling, PriorityService).
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceType ServiceType { get; set; } = ServiceType.StandardFiling;

        /// <summary>
        /// Complexity level of the service on a scale (e.g., 1-10).
        /// Higher values indicate more complex service offerings.
        /// </summary>
        [Range(1, 10)]
        public int ComplexityLevel { get; set; } = 1;

        /// <summary>
        /// Indicates if the service is currently active and available for selection.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Creates a ServiceModel from a domain Service entity.
        /// </summary>
        /// <param name="entity">The domain entity to convert</param>
        /// <returns>A new ServiceModel populated from the entity</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        public static ServiceModel FromEntity(Service entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            return new ServiceModel
            {
                ServiceId = entity.ServiceId,
                Name = entity.Name,
                Description = entity.Description,
                BasePrice = entity.BasePrice.Amount,
                CurrencyCode = entity.BasePrice.Currency,
                ServiceType = entity.ServiceType,
                ComplexityLevel = entity.ComplexityLevel,
                IsActive = entity.IsActive
            };
        }
    }
}