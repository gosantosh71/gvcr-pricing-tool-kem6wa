using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.ComponentModel.DataAnnotations; // Version 6.0.0
using VatFilingPricingTool.Domain.Enums; // Contains ServiceType and FilingFrequency

namespace VatFilingPricingTool.Api.Models.Requests
{
    /// <summary>
    /// Request model for calculating VAT filing costs based on service type, transaction volume, filing frequency, and countries.
    /// This is the primary input for the pricing calculation engine.
    /// </summary>
    public class CalculationRequest
    {
        /// <summary>
        /// The type of VAT filing service requested.
        /// </summary>
        [Required(ErrorMessage = "Service type is required")]
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// The number of transactions (invoices) per filing period.
        /// </summary>
        [Required(ErrorMessage = "Transaction volume is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Transaction volume must be greater than zero")]
        public int TransactionVolume { get; set; }

        /// <summary>
        /// The frequency at which VAT returns are filed.
        /// </summary>
        [Required(ErrorMessage = "Filing frequency is required")]
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// The country codes for which VAT filing pricing is requested.
        /// Typically ISO 3166-1 alpha-2 country codes (e.g., "GB", "DE", "FR").
        /// </summary>
        [Required(ErrorMessage = "At least one country must be specified")]
        [MinLength(1, ErrorMessage = "At least one country must be specified")]
        public List<string> CountryCodes { get; set; }

        /// <summary>
        /// Additional services requested beyond the standard filing service.
        /// </summary>
        public List<string> AdditionalServices { get; set; }

        /// <summary>
        /// Default constructor for the CalculationRequest.
        /// Initializes empty collections for CountryCodes and AdditionalServices.
        /// </summary>
        public CalculationRequest()
        {
            CountryCodes = new List<string>();
            AdditionalServices = new List<string>();
        }
    }

    /// <summary>
    /// Request model for retrieving a specific calculation by its ID.
    /// </summary>
    public class GetCalculationRequest
    {
        /// <summary>
        /// The unique identifier of the calculation to retrieve.
        /// </summary>
        [Required(ErrorMessage = "Calculation ID is required")]
        public string CalculationId { get; set; }
    }

    /// <summary>
    /// Request model for saving a calculation result for future reference.
    /// </summary>
    public class SaveCalculationRequest
    {
        /// <summary>
        /// The type of VAT filing service.
        /// </summary>
        [Required(ErrorMessage = "Service type is required")]
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// The number of transactions (invoices) per filing period.
        /// </summary>
        [Required(ErrorMessage = "Transaction volume is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Transaction volume must be greater than zero")]
        public int TransactionVolume { get; set; }

        /// <summary>
        /// The frequency at which VAT returns are filed.
        /// </summary>
        [Required(ErrorMessage = "Filing frequency is required")]
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// The total calculated cost for the VAT filing service.
        /// </summary>
        [Required(ErrorMessage = "Total cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total cost must be a non-negative value")]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// The currency code for the calculated costs (e.g., EUR, USD, GBP).
        /// </summary>
        [Required(ErrorMessage = "Currency code is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Detailed breakdown of costs by country.
        /// </summary>
        [Required(ErrorMessage = "Country breakdowns are required")]
        public List<CountryBreakdownRequest> CountryBreakdowns { get; set; }

        /// <summary>
        /// Additional services included in the calculation.
        /// </summary>
        public List<string> AdditionalServices { get; set; }

        /// <summary>
        /// Applied discounts, with discount type as key and amount as value.
        /// </summary>
        public Dictionary<string, decimal> Discounts { get; set; }

        /// <summary>
        /// Default constructor for the SaveCalculationRequest.
        /// Initializes empty collections for CountryBreakdowns, AdditionalServices, and Discounts.
        /// </summary>
        public SaveCalculationRequest()
        {
            CountryBreakdowns = new List<CountryBreakdownRequest>();
            AdditionalServices = new List<string>();
            Discounts = new Dictionary<string, decimal>();
        }
    }

    /// <summary>
    /// Request model for country-specific calculation details when saving a calculation.
    /// </summary>
    public class CountryBreakdownRequest
    {
        /// <summary>
        /// The country code (ISO 3166-1 alpha-2).
        /// </summary>
        [Required(ErrorMessage = "Country code is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
        public string CountryCode { get; set; }

        /// <summary>
        /// The full name of the country.
        /// </summary>
        [Required(ErrorMessage = "Country name is required")]
        public string CountryName { get; set; }

        /// <summary>
        /// The base cost for VAT filing in this country.
        /// </summary>
        [Required(ErrorMessage = "Base cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Base cost must be a non-negative value")]
        public decimal BaseCost { get; set; }

        /// <summary>
        /// Additional costs for extra services in this country.
        /// </summary>
        [Required(ErrorMessage = "Additional cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Additional cost must be a non-negative value")]
        public decimal AdditionalCost { get; set; }

        /// <summary>
        /// The total cost for this country (Base + Additional).
        /// </summary>
        [Required(ErrorMessage = "Total cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total cost must be a non-negative value")]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// The list of rule IDs that were applied in the calculation for this country.
        /// </summary>
        public List<string> AppliedRules { get; set; }

        /// <summary>
        /// Default constructor for the CountryBreakdownRequest.
        /// Initializes an empty collection for AppliedRules.
        /// </summary>
        public CountryBreakdownRequest()
        {
            AppliedRules = new List<string>();
        }
    }

    /// <summary>
    /// Request model for retrieving calculation history with optional filtering.
    /// </summary>
    public class GetCalculationHistoryRequest
    {
        /// <summary>
        /// Optional start date for filtering calculations.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end date for filtering calculations.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Optional list of country codes to filter calculations.
        /// </summary>
        public List<string> CountryCodes { get; set; }

        /// <summary>
        /// Optional service type to filter calculations.
        /// </summary>
        public ServiceType? ServiceType { get; set; }

        /// <summary>
        /// The page number for pagination (1-based).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than zero")]
        public int Page { get; set; }

        /// <summary>
        /// The number of items per page for pagination.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; }

        /// <summary>
        /// Default constructor for the GetCalculationHistoryRequest.
        /// Initializes an empty collection for CountryCodes and sets default pagination values.
        /// </summary>
        public GetCalculationHistoryRequest()
        {
            CountryCodes = new List<string>();
            Page = 1;
            PageSize = 10;
        }
    }

    /// <summary>
    /// Request model for comparing multiple calculation scenarios.
    /// </summary>
    public class CompareCalculationsRequest
    {
        /// <summary>
        /// The list of calculation scenarios to compare.
        /// </summary>
        [Required(ErrorMessage = "Scenarios are required")]
        [MinLength(2, ErrorMessage = "At least two scenarios are required for comparison")]
        public List<CalculationScenario> Scenarios { get; set; }

        /// <summary>
        /// Default constructor for the CompareCalculationsRequest.
        /// Initializes an empty collection for Scenarios.
        /// </summary>
        public CompareCalculationsRequest()
        {
            Scenarios = new List<CalculationScenario>();
        }
    }

    /// <summary>
    /// Model for a calculation scenario used in comparison requests.
    /// </summary>
    public class CalculationScenario
    {
        /// <summary>
        /// Unique identifier for the scenario.
        /// </summary>
        [Required(ErrorMessage = "Scenario ID is required")]
        public string ScenarioId { get; set; }

        /// <summary>
        /// Descriptive name for the scenario.
        /// </summary>
        [Required(ErrorMessage = "Scenario name is required")]
        public string ScenarioName { get; set; }

        /// <summary>
        /// The type of VAT filing service for this scenario.
        /// </summary>
        [Required(ErrorMessage = "Service type is required")]
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// The number of transactions (invoices) per filing period for this scenario.
        /// </summary>
        [Required(ErrorMessage = "Transaction volume is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Transaction volume must be greater than zero")]
        public int TransactionVolume { get; set; }

        /// <summary>
        /// The frequency at which VAT returns are filed for this scenario.
        /// </summary>
        [Required(ErrorMessage = "Filing frequency is required")]
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// The country codes for which VAT filing pricing is requested in this scenario.
        /// </summary>
        [Required(ErrorMessage = "At least one country must be specified")]
        [MinLength(1, ErrorMessage = "At least one country must be specified")]
        public List<string> CountryCodes { get; set; }

        /// <summary>
        /// Additional services requested for this scenario.
        /// </summary>
        public List<string> AdditionalServices { get; set; }

        /// <summary>
        /// Default constructor for the CalculationScenario.
        /// Initializes empty collections and generates a default scenario ID.
        /// </summary>
        public CalculationScenario()
        {
            CountryCodes = new List<string>();
            AdditionalServices = new List<string>();
            ScenarioId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Request model for deleting a specific calculation.
    /// </summary>
    public class DeleteCalculationRequest
    {
        /// <summary>
        /// The unique identifier of the calculation to delete.
        /// </summary>
        [Required(ErrorMessage = "Calculation ID is required")]
        public string CalculationId { get; set; }
    }
}