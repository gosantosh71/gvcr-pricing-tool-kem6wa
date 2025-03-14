using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations v6.0.0
using System.Text.Json.Serialization; // System.Text.Json v6.0.0
using VatFilingPricingTool.Domain.Enums; // Import FilingFrequency and ServiceType enums

namespace VatFilingPricingTool.Contracts.V1.Models
{
    /// <summary>
    /// Represents a VAT filing cost calculation with detailed breakdown by country and service type.
    /// This model serves as the data transfer object between the service layer and API controllers.
    /// </summary>
    public class CalculationModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the calculation.
        /// </summary>
        [Required]
        public string CalculationId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who performed the calculation.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the type of VAT filing service selected for the calculation.
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions/invoices per period.
        /// This affects the calculation based on volume-based pricing tiers.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Transaction volume must be greater than 0.")]
        public int TransactionVolume { get; set; }

        /// <summary>
        /// Gets or sets the filing frequency (Monthly, Quarterly, Annually).
        /// This affects the calculation based on frequency-specific pricing.
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the total cost of VAT filing services across all countries.
        /// </summary>
        [Required]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the currency code for the calculation (e.g., EUR, USD, GBP).
        /// </summary>
        [Required]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the calculation was performed.
        /// </summary>
        [Required]
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Gets or sets the detailed breakdown of costs by country.
        /// </summary>
        [Required]
        public List<CountryCalculationModel> CountryBreakdowns { get; set; }

        /// <summary>
        /// Gets or sets the additional services included in the calculation.
        /// </summary>
        public List<string> AdditionalServices { get; set; }

        /// <summary>
        /// Gets or sets the discounts applied to the calculation.
        /// The key is the discount name and the value is the discount amount.
        /// </summary>
        public Dictionary<string, decimal> Discounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this calculation has been archived.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationModel"/> class.
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
        /// Creates a CalculationModel from a domain Calculation entity.
        /// </summary>
        /// <param name="entity">The domain Calculation entity.</param>
        /// <returns>A new CalculationModel populated from the entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if entity is null.</exception>
        public static CalculationModel FromEntity(Calculation entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var model = new CalculationModel
            {
                CalculationId = entity.CalculationId,
                UserId = entity.UserId,
                ServiceType = entity.ServiceType,
                TransactionVolume = entity.TransactionVolume,
                Frequency = entity.Frequency,
                TotalCost = entity.TotalCost.Amount,
                CurrencyCode = entity.TotalCost.Currency,
                CalculationDate = entity.CalculationDate,
                IsArchived = entity.IsArchived
            };

            // Map country breakdowns
            if (entity.CalculationCountries != null)
            {
                foreach (var countryCalculation in entity.CalculationCountries)
                {
                    model.CountryBreakdowns.Add(
                        CountryCalculationModel.FromEntity(
                            countryCalculation, 
                            countryCalculation.Country
                        )
                    );
                }
            }

            // Map additional services
            if (entity.CalculationAdditionalServices != null)
            {
                foreach (var service in entity.CalculationAdditionalServices)
                {
                    model.AdditionalServices.Add(service.ServiceName);
                }
            }

            // Map discounts
            if (entity.Discounts != null)
            {
                foreach (var discount in entity.Discounts)
                {
                    model.Discounts.Add(discount.Name, discount.Amount);
                }
            }

            return model;
        }
    }

    /// <summary>
    /// Represents the calculation details for a specific country in a VAT filing cost calculation.
    /// </summary>
    public class CountryCalculationModel
    {
        /// <summary>
        /// Gets or sets the ISO country code (e.g., UK, DE, FR).
        /// </summary>
        [Required]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the full name of the country.
        /// </summary>
        [Required]
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the base cost for VAT filing in this country.
        /// </summary>
        [Required]
        public decimal BaseCost { get; set; }

        /// <summary>
        /// Gets or sets the additional cost for services beyond the base filing.
        /// </summary>
        public decimal AdditionalCost { get; set; }

        /// <summary>
        /// Gets or sets the total cost for this country (BaseCost + AdditionalCost).
        /// </summary>
        [Required]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the list of rules applied to calculate the cost for this country.
        /// </summary>
        public List<string> AppliedRules { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountryCalculationModel"/> class.
        /// </summary>
        public CountryCalculationModel()
        {
            AppliedRules = new List<string>();
        }

        /// <summary>
        /// Creates a CountryCalculationModel from a domain CalculationCountry entity.
        /// </summary>
        /// <param name="entity">The domain CalculationCountry entity.</param>
        /// <param name="country">The Country entity associated with the calculation.</param>
        /// <returns>A new CountryCalculationModel populated from the entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if entity or country is null.</exception>
        public static CountryCalculationModel FromEntity(CalculationCountry entity, Country country)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (country == null)
            {
                throw new ArgumentNullException(nameof(country));
            }

            var model = new CountryCalculationModel
            {
                CountryCode = entity.CountryCode,
                CountryName = country.Name,
                // Assuming BaseCost is 80% of total cost for this implementation
                BaseCost = entity.CountryCost.Amount * 0.8m,
                // Assuming AdditionalCost is 20% of total cost
                AdditionalCost = entity.CountryCost.Amount * 0.2m,
                TotalCost = entity.CountryCost.Amount
            };

            // Parse applied rules - assuming it's stored as a semicolon-separated string
            if (!string.IsNullOrEmpty(entity.AppliedRules))
            {
                foreach (var rule in entity.AppliedRules.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    model.AppliedRules.Add(rule.Trim());
                }
            }

            return model;
        }
    }
}