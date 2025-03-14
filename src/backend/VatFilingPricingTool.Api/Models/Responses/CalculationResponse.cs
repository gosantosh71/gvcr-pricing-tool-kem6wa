using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.ComponentModel.DataAnnotations; // Version 6.0.0
using VatFilingPricingTool.Domain.Enums.ServiceType; // For ServiceType enum
using VatFilingPricingTool.Domain.Enums.FilingFrequency; // For FilingFrequency enum
using VatFilingPricingTool.Service.Models.CalculationModel; // For CalculationModel service layer model

namespace VatFilingPricingTool.Api.Models.Responses
{
    /// <summary>
    /// Response model for VAT filing cost calculation results
    /// </summary>
    public class CalculationResponse
    {
        /// <summary>
        /// Unique identifier for the calculation
        /// </summary>
        [Required]
        public string CalculationId { get; set; }

        /// <summary>
        /// The VAT filing service type selected for this calculation
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Number of transactions/invoices per period
        /// </summary>
        [Range(1, int.MaxValue)]
        public int TransactionVolume { get; set; }

        /// <summary>
        /// How frequently VAT filing is performed
        /// </summary>
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// Total cost of VAT filing across all countries and services
        /// </summary>
        [DataType(DataType.Currency)]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Currency code used for the calculation (ISO 4217)
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// When the calculation was created
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Countries included in this calculation with their specific costs
        /// </summary>
        public List<CountryBreakdownResponse> CountryBreakdowns { get; set; }

        /// <summary>
        /// Additional services included in this calculation
        /// </summary>
        public List<string> AdditionalServices { get; set; }

        /// <summary>
        /// Discounts applied to the calculation
        /// </summary>
        public Dictionary<string, decimal> Discounts { get; set; }

        /// <summary>
        /// Default constructor for the CalculationResponse
        /// </summary>
        public CalculationResponse()
        {
            CountryBreakdowns = new List<CountryBreakdownResponse>();
            AdditionalServices = new List<string>();
            Discounts = new Dictionary<string, decimal>();
            CalculationDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a CalculationResponse from a CalculationModel
        /// </summary>
        /// <param name="model">The calculation model from the service layer</param>
        /// <returns>A new CalculationResponse populated from the model</returns>
        public static CalculationResponse FromModel(CalculationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "Calculation model cannot be null");

            var response = new CalculationResponse
            {
                CalculationId = model.CalculationId,
                ServiceType = model.ServiceType,
                TransactionVolume = model.TransactionVolume,
                Frequency = model.Frequency,
                TotalCost = model.TotalCost?.Amount ?? 0,
                CurrencyCode = model.CurrencyCode,
                CalculationDate = model.CalculationDate
            };

            // Map country breakdowns
            if (model.CountryBreakdowns != null)
            {
                foreach (var country in model.CountryBreakdowns)
                {
                    response.CountryBreakdowns.Add(CountryBreakdownResponse.FromModel(country));
                }
            }

            // Map additional services
            if (model.AdditionalServices != null)
            {
                response.AdditionalServices = new List<string>(model.AdditionalServices);
            }

            // Map discounts
            if (model.Discounts != null)
            {
                response.Discounts = new Dictionary<string, decimal>(model.Discounts);
            }

            return response;
        }
    }

    /// <summary>
    /// Response model for country-specific calculation details
    /// </summary>
    public class CountryBreakdownResponse
    {
        /// <summary>
        /// ISO country code (ISO 3166-1 alpha-2)
        /// </summary>
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string CountryCode { get; set; }

        /// <summary>
        /// Name of the country
        /// </summary>
        [Required]
        public string CountryName { get; set; }

        /// <summary>
        /// Base cost for VAT filing in this country
        /// </summary>
        [DataType(DataType.Currency)]
        public decimal BaseCost { get; set; }

        /// <summary>
        /// Additional cost for VAT filing in this country
        /// </summary>
        [DataType(DataType.Currency)]
        public decimal AdditionalCost { get; set; }

        /// <summary>
        /// Total cost for VAT filing in this country
        /// </summary>
        [DataType(DataType.Currency)]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// List of rule IDs that were applied to calculate this country's cost
        /// </summary>
        public List<string> AppliedRules { get; set; }

        /// <summary>
        /// Default constructor for the CountryBreakdownResponse
        /// </summary>
        public CountryBreakdownResponse()
        {
            AppliedRules = new List<string>();
        }

        /// <summary>
        /// Creates a CountryBreakdownResponse from a country breakdown model
        /// </summary>
        /// <param name="model">The country calculation model from the service layer</param>
        /// <returns>A new CountryBreakdownResponse populated from the model</returns>
        public static CountryBreakdownResponse FromModel(CountryCalculationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "Country calculation model cannot be null");

            var response = new CountryBreakdownResponse
            {
                CountryCode = model.CountryCode,
                CountryName = model.CountryName,
                BaseCost = model.BaseCost?.Amount ?? 0,
                AdditionalCost = model.AdditionalCost?.Amount ?? 0,
                TotalCost = model.TotalCost?.Amount ?? 0
            };

            // Map applied rules
            if (model.AppliedRules != null)
            {
                response.AppliedRules = new List<string>(model.AppliedRules);
            }

            return response;
        }
    }

    /// <summary>
    /// Response model for calculation history with pagination
    /// </summary>
    public class CalculationHistoryResponse
    {
        /// <summary>
        /// Collection of calculation summaries for the requested page
        /// </summary>
        public List<CalculationSummaryResponse> Items { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indicates if there is a previous page available
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Indicates if there is a next page available
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Default constructor for the CalculationHistoryResponse
        /// </summary>
        public CalculationHistoryResponse()
        {
            Items = new List<CalculationSummaryResponse>();
        }
    }

    /// <summary>
    /// Response model for a summary of a calculation in history
    /// </summary>
    public class CalculationSummaryResponse
    {
        /// <summary>
        /// Unique identifier for the calculation
        /// </summary>
        [Required]
        public string CalculationId { get; set; }

        /// <summary>
        /// The VAT filing service type selected for this calculation
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Number of transactions/invoices per period
        /// </summary>
        [Range(1, int.MaxValue)]
        public int TransactionVolume { get; set; }

        /// <summary>
        /// How frequently VAT filing is performed
        /// </summary>
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// Total cost of VAT filing across all countries and services
        /// </summary>
        [DataType(DataType.Currency)]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Currency code used for the calculation (ISO 4217)
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// When the calculation was created
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// List of country names included in this calculation
        /// </summary>
        public List<string> Countries { get; set; }

        /// <summary>
        /// Default constructor for the CalculationSummaryResponse
        /// </summary>
        public CalculationSummaryResponse()
        {
            Countries = new List<string>();
        }

        /// <summary>
        /// Creates a CalculationSummaryResponse from a CalculationModel
        /// </summary>
        /// <param name="model">The calculation model from the service layer</param>
        /// <returns>A new CalculationSummaryResponse populated from the model</returns>
        public static CalculationSummaryResponse FromModel(CalculationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "Calculation model cannot be null");

            var response = new CalculationSummaryResponse
            {
                CalculationId = model.CalculationId,
                ServiceType = model.ServiceType,
                TransactionVolume = model.TransactionVolume,
                Frequency = model.Frequency,
                TotalCost = model.TotalCost?.Amount ?? 0,
                CurrencyCode = model.CurrencyCode,
                CalculationDate = model.CalculationDate
            };

            // Extract country names
            if (model.CountryBreakdowns != null)
            {
                foreach (var country in model.CountryBreakdowns)
                {
                    response.Countries.Add(country.CountryName);
                }
            }

            return response;
        }
    }

    /// <summary>
    /// Response model for saving a calculation result
    /// </summary>
    public class SaveCalculationResponse
    {
        /// <summary>
        /// Unique identifier for the saved calculation
        /// </summary>
        [Required]
        public string CalculationId { get; set; }

        /// <summary>
        /// When the calculation was saved
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Default constructor for the SaveCalculationResponse
        /// </summary>
        public SaveCalculationResponse()
        {
            CalculationDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Response model for comparing multiple calculation scenarios
    /// </summary>
    public class CompareCalculationsResponse
    {
        /// <summary>
        /// Collection of calculation scenarios being compared
        /// </summary>
        public List<CalculationScenarioResponse> Scenarios { get; set; }

        /// <summary>
        /// Comparison of total costs between scenarios
        /// </summary>
        public Dictionary<string, decimal> TotalCostComparison { get; set; }

        /// <summary>
        /// Comparison of country-specific costs between scenarios
        /// </summary>
        public Dictionary<string, Dictionary<string, decimal>> CountryCostComparison { get; set; }

        /// <summary>
        /// Default constructor for the CompareCalculationsResponse
        /// </summary>
        public CompareCalculationsResponse()
        {
            Scenarios = new List<CalculationScenarioResponse>();
            TotalCostComparison = new Dictionary<string, decimal>();
            CountryCostComparison = new Dictionary<string, Dictionary<string, decimal>>();
        }
    }

    /// <summary>
    /// Response model for a calculation scenario in comparison
    /// </summary>
    public class CalculationScenarioResponse
    {
        /// <summary>
        /// Unique identifier for the scenario
        /// </summary>
        [Required]
        public string ScenarioId { get; set; }

        /// <summary>
        /// Name of the calculation scenario
        /// </summary>
        [Required]
        public string ScenarioName { get; set; }

        /// <summary>
        /// The VAT filing service type selected for this scenario
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Number of transactions/invoices per period
        /// </summary>
        [Range(1, int.MaxValue)]
        public int TransactionVolume { get; set; }

        /// <summary>
        /// How frequently VAT filing is performed
        /// </summary>
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// Total cost of VAT filing for this scenario
        /// </summary>
        [DataType(DataType.Currency)]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Currency code used for the calculation (ISO 4217)
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Countries included in this scenario with their specific costs
        /// </summary>
        public List<CountryBreakdownResponse> CountryBreakdowns { get; set; }

        /// <summary>
        /// Additional services included in this scenario
        /// </summary>
        public List<string> AdditionalServices { get; set; }

        /// <summary>
        /// Default constructor for the CalculationScenarioResponse
        /// </summary>
        public CalculationScenarioResponse()
        {
            CountryBreakdowns = new List<CountryBreakdownResponse>();
            AdditionalServices = new List<string>();
        }
    }

    /// <summary>
    /// Response model for deleting a calculation
    /// </summary>
    public class DeleteCalculationResponse
    {
        /// <summary>
        /// Unique identifier of the deleted calculation
        /// </summary>
        [Required]
        public string CalculationId { get; set; }

        /// <summary>
        /// Indicates if the deletion was successful
        /// </summary>
        public bool Deleted { get; set; }
    }
}