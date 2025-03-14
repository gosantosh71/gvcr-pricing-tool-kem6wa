using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations v6.0.0
using VatFilingPricingTool.Domain.Enums; // For FilingFrequency and ServiceType enums
using VatFilingPricingTool.Contracts.V1.Models; // For CountryCalculationModel

namespace VatFilingPricingTool.Contracts.V1.Requests
{
    /// <summary>
    /// Request model for calculating VAT filing costs based on service type, transaction volume, filing frequency, and countries.
    /// </summary>
    public class CalculateRequest
    {
        /// <summary>
        /// Gets or sets the type of VAT filing service selected for the calculation.
        /// </summary>
        [Required]
        public ServiceType ServiceType { get; set; }
        
        /// <summary>
        /// Gets or sets the number of transactions/invoices per period.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Transaction volume must be greater than 0.")]
        public int TransactionVolume { get; set; }
        
        /// <summary>
        /// Gets or sets the filing frequency (Monthly, Quarterly, Annually).
        /// </summary>
        [Required]
        public FilingFrequency Frequency { get; set; }
        
        /// <summary>
        /// Gets or sets the list of country codes for which VAT filing costs should be calculated.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one country must be selected.")]
        public List<string> CountryCodes { get; set; }
        
        /// <summary>
        /// Gets or sets the list of additional services to include in the calculation.
        /// </summary>
        public List<string> AdditionalServices { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculateRequest"/> class.
        /// </summary>
        public CalculateRequest()
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
        /// Gets or sets the unique identifier of the calculation to retrieve.
        /// </summary>
        [Required]
        public string CalculationId { get; set; }
    }

    /// <summary>
    /// Request model for saving a calculation result for future reference.
    /// </summary>
    public class SaveCalculationRequest
    {
        /// <summary>
        /// Gets or sets the type of VAT filing service selected for the calculation.
        /// </summary>
        [Required]
        public ServiceType ServiceType { get; set; }
        
        /// <summary>
        /// Gets or sets the number of transactions/invoices per period.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Transaction volume must be greater than 0.")]
        public int TransactionVolume { get; set; }
        
        /// <summary>
        /// Gets or sets the filing frequency (Monthly, Quarterly, Annually).
        /// </summary>
        [Required]
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
        /// Gets or sets the detailed breakdown of costs by country.
        /// </summary>
        [Required]
        public List<CountryBreakdownRequest> CountryBreakdowns { get; set; }
        
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
        /// Initializes a new instance of the <see cref="SaveCalculationRequest"/> class.
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
        /// Initializes a new instance of the <see cref="CountryBreakdownRequest"/> class.
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
        /// Gets or sets the start date for filtering calculations.
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Gets or sets the end date for filtering calculations.
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Gets or sets the list of country codes to filter calculations by included countries.
        /// </summary>
        public List<string> CountryCodes { get; set; }
        
        /// <summary>
        /// Gets or sets the service type to filter calculations.
        /// </summary>
        public ServiceType? ServiceType { get; set; }
        
        /// <summary>
        /// Gets or sets the page number for pagination (1-based).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0.")]
        public int Page { get; set; }
        
        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
        public int PageSize { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GetCalculationHistoryRequest"/> class.
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
        /// Gets or sets the list of calculation scenarios to compare.
        /// </summary>
        [Required]
        [MinLength(2, ErrorMessage = "At least two scenarios are required for comparison.")]
        public List<CalculationScenario> Scenarios { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareCalculationsRequest"/> class.
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
        /// Gets or sets the unique identifier for the scenario.
        /// </summary>
        public string ScenarioId { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the scenario for display purposes.
        /// </summary>
        [Required]
        public string ScenarioName { get; set; }
        
        /// <summary>
        /// Gets or sets the type of VAT filing service selected for the scenario.
        /// </summary>
        [Required]
        public ServiceType ServiceType { get; set; }
        
        /// <summary>
        /// Gets or sets the number of transactions/invoices per period.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Transaction volume must be greater than 0.")]
        public int TransactionVolume { get; set; }
        
        /// <summary>
        /// Gets or sets the filing frequency (Monthly, Quarterly, Annually).
        /// </summary>
        [Required]
        public FilingFrequency Frequency { get; set; }
        
        /// <summary>
        /// Gets or sets the list of country codes for which VAT filing costs should be calculated.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one country must be selected.")]
        public List<string> CountryCodes { get; set; }
        
        /// <summary>
        /// Gets or sets the list of additional services to include in the calculation.
        /// </summary>
        public List<string> AdditionalServices { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationScenario"/> class.
        /// </summary>
        public CalculationScenario()
        {
            ScenarioId = Guid.NewGuid().ToString();
            CountryCodes = new List<string>();
            AdditionalServices = new List<string>();
        }
    }

    /// <summary>
    /// Request model for deleting a specific calculation.
    /// </summary>
    public class DeleteCalculationRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier of the calculation to delete.
        /// </summary>
        [Required]
        public string CalculationId { get; set; }
    }
}