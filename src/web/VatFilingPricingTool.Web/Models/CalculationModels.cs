using System;  // System v6.0.0
using System.Collections.Generic;  // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations;  // System.ComponentModel.DataAnnotations v6.0.0
using System.Text.Json.Serialization;  // System.Text.Json v6.0.0

namespace VatFilingPricingTool.Web.Models
{
    /// <summary>
    /// Represents the input parameters for a VAT filing pricing calculation
    /// </summary>
    public class CalculationInputModel
    {
        /// <summary>
        /// The list of country codes to include in the calculation
        /// </summary>
        [Required(ErrorMessage = "At least one country must be selected")]
        public List<string> CountryCodes { get; set; }

        /// <summary>
        /// The type of VAT filing service (e.g., Standard, Complex, Priority)
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please select a valid service type")]
        public int ServiceType { get; set; }

        /// <summary>
        /// The number of transactions or invoices per period
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Transaction volume must be greater than zero")]
        public int TransactionVolume { get; set; }

        /// <summary>
        /// The frequency of VAT filings (e.g., Monthly, Quarterly, Annually)
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid filing frequency")]
        public int FilingFrequency { get; set; }

        /// <summary>
        /// The list of additional services to include in the calculation
        /// </summary>
        public List<string> AdditionalServices { get; set; }

        /// <summary>
        /// The currency code for the calculation (e.g., EUR, GBP, USD)
        /// </summary>
        [StringLength(3)]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Default constructor for the CalculationInputModel
        /// </summary>
        public CalculationInputModel()
        {
            CountryCodes = new List<string>();
            AdditionalServices = new List<string>();
            CurrencyCode = "EUR";
            TransactionVolume = 0;
            ServiceType = 0;  // StandardFiling
            FilingFrequency = 1;  // Monthly
        }
    }

    /// <summary>
    /// Represents the result of a VAT filing pricing calculation with detailed breakdown
    /// </summary>
    public class CalculationResultModel
    {
        /// <summary>
        /// The unique identifier for this calculation
        /// </summary>
        public string CalculationId { get; set; }

        /// <summary>
        /// The identifier of the user who performed this calculation
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The type of VAT filing service selected
        /// </summary>
        public int ServiceType { get; set; }

        /// <summary>
        /// The display name of the selected service type
        /// </summary>
        public string ServiceTypeName { get; set; }

        /// <summary>
        /// The number of transactions or invoices per period
        /// </summary>
        public int TransactionVolume { get; set; }

        /// <summary>
        /// The frequency of VAT filings
        /// </summary>
        public int FilingFrequency { get; set; }

        /// <summary>
        /// The display name of the selected filing frequency
        /// </summary>
        public string FilingFrequencyName { get; set; }

        /// <summary>
        /// The total cost of VAT filing services
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// The currency code for the calculation
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The total cost formatted with the currency symbol
        /// </summary>
        public string FormattedTotalCost { get; set; }

        /// <summary>
        /// The date and time when the calculation was performed
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// The cost breakdown by country
        /// </summary>
        public List<CountryCalculationResultModel> CountryBreakdowns { get; set; }

        /// <summary>
        /// The list of additional services included in the calculation
        /// </summary>
        public List<string> AdditionalServices { get; set; }

        /// <summary>
        /// The discounts applied to the calculation
        /// </summary>
        public Dictionary<string, decimal> Discounts { get; set; }

        /// <summary>
        /// The total amount of all discounts applied
        /// </summary>
        public decimal TotalDiscounts { get; set; }

        /// <summary>
        /// The total discounts formatted with the currency symbol
        /// </summary>
        public string FormattedTotalDiscounts { get; set; }

        /// <summary>
        /// Indicates if this calculation has been archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Default constructor for the CalculationResultModel
        /// </summary>
        public CalculationResultModel()
        {
            CountryBreakdowns = new List<CountryCalculationResultModel>();
            AdditionalServices = new List<string>();
            Discounts = new Dictionary<string, decimal>();
            CalculationDate = DateTime.UtcNow;
            IsArchived = false;
        }

        /// <summary>
        /// Returns the total cost formatted with currency symbol
        /// </summary>
        /// <returns>Formatted total cost with currency symbol</returns>
        public string GetFormattedTotalCost()
        {
            return $"{CurrencyCode} {TotalCost:N2}";
        }

        /// <summary>
        /// Returns the calculation date formatted for display
        /// </summary>
        /// <returns>Formatted calculation date</returns>
        public string GetFormattedDate()
        {
            return CalculationDate.ToShortDateString();
        }
    }

    /// <summary>
    /// Represents the calculation details for a specific country in a VAT filing cost calculation
    /// </summary>
    public class CountryCalculationResultModel
    {
        /// <summary>
        /// The ISO country code for the country
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// The display name of the country
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// The country flag code used for displaying flag icons
        /// </summary>
        public string FlagCode { get; set; }

        /// <summary>
        /// The base cost for VAT filing in this country
        /// </summary>
        public decimal BaseCost { get; set; }

        /// <summary>
        /// The base cost formatted with currency symbol
        /// </summary>
        public string FormattedBaseCost { get; set; }

        /// <summary>
        /// The additional cost for special requirements or complexity
        /// </summary>
        public decimal AdditionalCost { get; set; }

        /// <summary>
        /// The additional cost formatted with currency symbol
        /// </summary>
        public string FormattedAdditionalCost { get; set; }

        /// <summary>
        /// The total cost for this country (BaseCost + AdditionalCost)
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// The total cost formatted with currency symbol
        /// </summary>
        public string FormattedTotalCost { get; set; }

        /// <summary>
        /// The list of rule names that were applied to calculate the cost
        /// </summary>
        public List<string> AppliedRules { get; set; }

        /// <summary>
        /// The currency code for the calculation
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Default constructor for the CountryCalculationResultModel
        /// </summary>
        public CountryCalculationResultModel()
        {
            AppliedRules = new List<string>();
            CurrencyCode = "EUR";
        }

        /// <summary>
        /// Creates a CountryCalculationResultModel from a CountrySummaryModel
        /// </summary>
        /// <param name="country">The country summary data</param>
        /// <returns>A new CountryCalculationResultModel with basic country information</returns>
        public static CountryCalculationResultModel FromCountrySummary(CountrySummaryModel country)
        {
            return new CountryCalculationResultModel
            {
                CountryCode = country.CountryCode,
                CountryName = country.Name,
                FlagCode = country.CountryCode.ToLower(),
                BaseCost = 0,
                AdditionalCost = 0,
                TotalCost = 0
            };
        }
    }

    /// <summary>
    /// Model for saving a calculation result for future reference
    /// </summary>
    public class SaveCalculationModel
    {
        /// <summary>
        /// The unique identifier of the calculation to save
        /// </summary>
        [Required]
        public string CalculationId { get; set; }

        /// <summary>
        /// A user-defined name for the saved calculation
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// An optional description of the saved calculation
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Indicates if the calculation should be archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Default constructor for the SaveCalculationModel
        /// </summary>
        public SaveCalculationModel()
        {
            IsArchived = false;
        }
    }

    /// <summary>
    /// Model for calculation history with pagination
    /// </summary>
    public class CalculationHistoryModel
    {
        /// <summary>
        /// The list of calculation results for the current page
        /// </summary>
        public List<CalculationResultModel> Items { get; set; }

        /// <summary>
        /// The total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// The current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Default constructor for the CalculationHistoryModel
        /// </summary>
        public CalculationHistoryModel()
        {
            Items = new List<CalculationResultModel>();
            TotalCount = 0;
            PageNumber = 1;
            PageSize = 10;
            TotalPages = 0;
        }

        /// <summary>
        /// Determines if there is a previous page of results
        /// </summary>
        /// <returns>True if there is a previous page, otherwise false</returns>
        public bool HasPreviousPage()
        {
            return PageNumber > 1;
        }

        /// <summary>
        /// Determines if there is a next page of results
        /// </summary>
        /// <returns>True if there is a next page, otherwise false</returns>
        public bool HasNextPage()
        {
            return PageNumber < TotalPages;
        }
    }

    /// <summary>
    /// Model for filtering calculation history
    /// </summary>
    public class CalculationFilterModel
    {
        /// <summary>
        /// The page number for pagination (1-based)
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of items per page
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; }

        /// <summary>
        /// The start date for filtering by calculation date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The end date for filtering by calculation date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Filter by specific country codes
        /// </summary>
        public List<string> CountryCodes { get; set; }

        /// <summary>
        /// Filter by service type
        /// </summary>
        public int? ServiceType { get; set; }

        /// <summary>
        /// Whether to include archived calculations
        /// </summary>
        public bool IncludeArchived { get; set; }

        /// <summary>
        /// The field to sort by
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Whether to sort in descending order
        /// </summary>
        public bool SortDescending { get; set; }

        /// <summary>
        /// Default constructor for the CalculationFilterModel
        /// </summary>
        public CalculationFilterModel()
        {
            CountryCodes = new List<string>();
            PageNumber = 1;
            PageSize = 10;
            IncludeArchived = false;
            SortBy = "CalculationDate";
            SortDescending = true;
        }
    }

    /// <summary>
    /// Represents a service type option for selection in the UI
    /// </summary>
    public class ServiceTypeOption
    {
        /// <summary>
        /// The value of the service type option
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The display text for the service type option
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// A description of the service type
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates if this service type is currently selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Constructor for creating a service type option
        /// </summary>
        /// <param name="value">The value of the service type</param>
        /// <param name="text">The display text for the service type</param>
        /// <param name="description">A description of the service type</param>
        public ServiceTypeOption(int value, string text, string description)
        {
            Value = value;
            Text = text;
            Description = description;
            IsSelected = false;
        }
    }

    /// <summary>
    /// Represents a filing frequency option for selection in the UI
    /// </summary>
    public class FilingFrequencyOption
    {
        /// <summary>
        /// The value of the filing frequency option
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The display text for the filing frequency option
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// A description of the filing frequency
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates if this filing frequency is currently selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Constructor for creating a filing frequency option
        /// </summary>
        /// <param name="value">The value of the filing frequency</param>
        /// <param name="text">The display text for the filing frequency</param>
        /// <param name="description">A description of the filing frequency</param>
        public FilingFrequencyOption(int value, string text, string description)
        {
            Value = value;
            Text = text;
            Description = description;
            IsSelected = false;
        }
    }

    /// <summary>
    /// Represents an additional service option for selection in the UI
    /// </summary>
    public class AdditionalServiceOption
    {
        /// <summary>
        /// The value of the additional service option
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The display text for the additional service option
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// A description of the additional service
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The cost of the additional service
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// The cost formatted with currency symbol
        /// </summary>
        public string FormattedCost { get; set; }

        /// <summary>
        /// Indicates if this additional service is currently selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Constructor for creating an additional service option
        /// </summary>
        /// <param name="value">The value of the additional service</param>
        /// <param name="text">The display text for the additional service</param>
        /// <param name="description">A description of the additional service</param>
        /// <param name="cost">The cost of the additional service</param>
        /// <param name="currencyCode">The currency code for formatting the cost</param>
        public AdditionalServiceOption(string value, string text, string description, decimal cost, string currencyCode)
        {
            Value = value;
            Text = text;
            Description = description;
            Cost = cost;
            FormattedCost = $"{currencyCode} {cost:N2}";
            IsSelected = false;
        }
    }

    /// <summary>
    /// Model for comparing multiple calculation scenarios
    /// </summary>
    public class CalculationComparisonModel
    {
        /// <summary>
        /// The list of calculations to compare
        /// </summary>
        public List<CalculationResultModel> Calculations { get; set; }

        /// <summary>
        /// The calculation with the lowest total cost
        /// </summary>
        public CalculationResultModel LowestCostCalculation { get; set; }

        /// <summary>
        /// A dictionary mapping country codes to lists of country calculation results across all scenarios
        /// </summary>
        public Dictionary<string, List<CountryCalculationResultModel>> CountryComparisons { get; set; }

        /// <summary>
        /// Default constructor for the CalculationComparisonModel
        /// </summary>
        public CalculationComparisonModel()
        {
            Calculations = new List<CalculationResultModel>();
            CountryComparisons = new Dictionary<string, List<CountryCalculationResultModel>>();
        }

        /// <summary>
        /// Identifies the calculation with the lowest total cost
        /// </summary>
        public void FindLowestCostCalculation()
        {
            if (Calculations == null || Calculations.Count == 0)
            {
                LowestCostCalculation = null;
                return;
            }

            LowestCostCalculation = Calculations.OrderBy(c => c.TotalCost).First();
        }

        /// <summary>
        /// Builds a dictionary of country-specific cost comparisons across calculations
        /// </summary>
        public void BuildCountryComparisons()
        {
            CountryComparisons.Clear();

            if (Calculations == null || Calculations.Count == 0)
                return;

            // Get all unique country codes across all calculations
            var allCountryCodes = new HashSet<string>();
            foreach (var calc in Calculations)
            {
                foreach (var country in calc.CountryBreakdowns)
                {
                    allCountryCodes.Add(country.CountryCode);
                }
            }

            // Build comparison lists for each country
            foreach (var countryCode in allCountryCodes)
            {
                var countryResults = new List<CountryCalculationResultModel>();
                
                foreach (var calc in Calculations)
                {
                    var countryResult = calc.CountryBreakdowns.FirstOrDefault(c => c.CountryCode == countryCode);
                    if (countryResult != null)
                    {
                        countryResults.Add(countryResult);
                    }
                }

                CountryComparisons[countryCode] = countryResults;
            }
        }
    }
}