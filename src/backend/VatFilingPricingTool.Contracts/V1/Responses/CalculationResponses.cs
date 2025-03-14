using System;
using System.Collections.Generic;
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Responses
{
    /// <summary>
    /// Response model for VAT filing cost calculation results
    /// </summary>
    public class CalculateResponse
    {
        /// <summary>
        /// Gets or sets the type of VAT filing service selected for the calculation.
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions/invoices per period.
        /// </summary>
        public int TransactionVolume { get; set; }

        /// <summary>
        /// Gets or sets the filing frequency (Monthly, Quarterly, Annually).
        /// </summary>
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the total cost of VAT filing services across all countries.
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the currency code for the calculation (e.g., EUR, USD, GBP).
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the detailed breakdown of costs by country.
        /// </summary>
        public List<CountryCalculationResponse> CountryBreakdowns { get; set; }

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
        /// Initializes a new instance of the <see cref="CalculateResponse"/> class.
        /// </summary>
        public CalculateResponse()
        {
            CountryBreakdowns = new List<CountryCalculationResponse>();
            AdditionalServices = new List<string>();
            Discounts = new Dictionary<string, decimal>();
        }

        /// <summary>
        /// Creates a CalculateResponse from a CalculationModel
        /// </summary>
        /// <param name="model">The calculation model</param>
        /// <returns>A new CalculateResponse populated from the model</returns>
        public static CalculateResponse FromModel(CalculationModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var response = new CalculateResponse
            {
                ServiceType = model.ServiceType,
                TransactionVolume = model.TransactionVolume,
                Frequency = model.Frequency,
                TotalCost = model.TotalCost,
                CurrencyCode = model.CurrencyCode
            };

            foreach (var country in model.CountryBreakdowns)
            {
                response.CountryBreakdowns.Add(CountryCalculationResponse.FromModel(country));
            }

            if (model.AdditionalServices != null)
            {
                response.AdditionalServices = new List<string>(model.AdditionalServices);
            }

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
    public class CountryCalculationResponse
    {
        /// <summary>
        /// Gets or sets the ISO country code (e.g., UK, DE, FR).
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the full name of the country.
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the base cost for VAT filing in this country.
        /// </summary>
        public decimal BaseCost { get; set; }

        /// <summary>
        /// Gets or sets the additional cost for services beyond the base filing.
        /// </summary>
        public decimal AdditionalCost { get; set; }

        /// <summary>
        /// Gets or sets the total cost for this country (BaseCost + AdditionalCost).
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the list of rules applied to calculate the cost for this country.
        /// </summary>
        public List<string> AppliedRules { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountryCalculationResponse"/> class.
        /// </summary>
        public CountryCalculationResponse()
        {
            AppliedRules = new List<string>();
        }

        /// <summary>
        /// Creates a CountryCalculationResponse from a CountryCalculationModel
        /// </summary>
        /// <param name="model">The country calculation model</param>
        /// <returns>A new CountryCalculationResponse populated from the model</returns>
        public static CountryCalculationResponse FromModel(CountryCalculationModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var response = new CountryCalculationResponse
            {
                CountryCode = model.CountryCode,
                CountryName = model.CountryName,
                BaseCost = model.BaseCost,
                AdditionalCost = model.AdditionalCost,
                TotalCost = model.TotalCost
            };

            if (model.AppliedRules != null)
            {
                response.AppliedRules = new List<string>(model.AppliedRules);
            }

            return response;
        }
    }

    /// <summary>
    /// Response model for retrieving a specific calculation by ID
    /// </summary>
    public class GetCalculationResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the calculation.
        /// </summary>
        public string CalculationId { get; set; }

        /// <summary>
        /// Gets or sets the type of VAT filing service selected for the calculation.
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions/invoices per period.
        /// </summary>
        public int TransactionVolume { get; set; }

        /// <summary>
        /// Gets or sets the filing frequency (Monthly, Quarterly, Annually).
        /// </summary>
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the total cost of VAT filing services across all countries.
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the currency code for the calculation (e.g., EUR, USD, GBP).
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the calculation was performed.
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Gets or sets the detailed breakdown of costs by country.
        /// </summary>
        public List<CountryCalculationResponse> CountryBreakdowns { get; set; }

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
        /// Initializes a new instance of the <see cref="GetCalculationResponse"/> class.
        /// </summary>
        public GetCalculationResponse()
        {
            CountryBreakdowns = new List<CountryCalculationResponse>();
            AdditionalServices = new List<string>();
            Discounts = new Dictionary<string, decimal>();
        }

        /// <summary>
        /// Creates a GetCalculationResponse from a CalculationModel
        /// </summary>
        /// <param name="model">The calculation model</param>
        /// <returns>A new GetCalculationResponse populated from the model</returns>
        public static GetCalculationResponse FromModel(CalculationModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var response = new GetCalculationResponse
            {
                CalculationId = model.CalculationId,
                ServiceType = model.ServiceType,
                TransactionVolume = model.TransactionVolume,
                Frequency = model.Frequency,
                TotalCost = model.TotalCost,
                CurrencyCode = model.CurrencyCode,
                CalculationDate = model.CalculationDate
            };

            foreach (var country in model.CountryBreakdowns)
            {
                response.CountryBreakdowns.Add(CountryCalculationResponse.FromModel(country));
            }

            if (model.AdditionalServices != null)
            {
                response.AdditionalServices = new List<string>(model.AdditionalServices);
            }

            if (model.Discounts != null)
            {
                response.Discounts = new Dictionary<string, decimal>(model.Discounts);
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
        /// Gets or sets the unique identifier for the saved calculation.
        /// </summary>
        public string CalculationId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the calculation was saved.
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCalculationResponse"/> class.
        /// </summary>
        public SaveCalculationResponse()
        {
            CalculationDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Response model for calculation history with pagination
    /// </summary>
    public class CalculationHistoryResponse
    {
        /// <summary>
        /// Gets or sets the list of calculation summaries in the current page.
        /// </summary>
        public List<CalculationSummaryResponse> Items { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of items across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a next page.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationHistoryResponse"/> class.
        /// </summary>
        public CalculationHistoryResponse()
        {
            Items = new List<CalculationSummaryResponse>();
        }

        /// <summary>
        /// Creates a CalculationHistoryResponse from a PagedList of CalculationModel
        /// </summary>
        /// <param name="pagedList">The paged list of calculation models</param>
        /// <returns>A new CalculationHistoryResponse populated from the paged list</returns>
        public static CalculationHistoryResponse FromPagedList(PagedList<CalculationModel> pagedList)
        {
            if (pagedList == null)
            {
                throw new ArgumentNullException(nameof(pagedList));
            }

            var response = new CalculationHistoryResponse
            {
                PageNumber = pagedList.PageNumber,
                PageSize = pagedList.PageSize,
                TotalCount = pagedList.TotalCount,
                TotalPages = pagedList.TotalPages,
                HasPreviousPage = pagedList.HasPreviousPage,
                HasNextPage = pagedList.HasNextPage
            };

            foreach (var calculation in pagedList.Items)
            {
                response.Items.Add(CalculationSummaryResponse.FromModel(calculation));
            }

            return response;
        }
    }

    /// <summary>
    /// Response model for a summary of a calculation in history
    /// </summary>
    public class CalculationSummaryResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the calculation.
        /// </summary>
        public string CalculationId { get; set; }

        /// <summary>
        /// Gets or sets the type of VAT filing service selected for the calculation.
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions/invoices per period.
        /// </summary>
        public int TransactionVolume { get; set; }

        /// <summary>
        /// Gets or sets the filing frequency (Monthly, Quarterly, Annually).
        /// </summary>
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the total cost of VAT filing services across all countries.
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the currency code for the calculation (e.g., EUR, USD, GBP).
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the calculation was performed.
        /// </summary>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Gets or sets the list of countries included in the calculation.
        /// </summary>
        public List<string> Countries { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationSummaryResponse"/> class.
        /// </summary>
        public CalculationSummaryResponse()
        {
            Countries = new List<string>();
        }

        /// <summary>
        /// Creates a CalculationSummaryResponse from a CalculationModel
        /// </summary>
        /// <param name="model">The calculation model</param>
        /// <returns>A new CalculationSummaryResponse populated from the model</returns>
        public static CalculationSummaryResponse FromModel(CalculationModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var response = new CalculationSummaryResponse
            {
                CalculationId = model.CalculationId,
                ServiceType = model.ServiceType,
                TransactionVolume = model.TransactionVolume,
                Frequency = model.Frequency,
                TotalCost = model.TotalCost,
                CurrencyCode = model.CurrencyCode,
                CalculationDate = model.CalculationDate
            };

            foreach (var country in model.CountryBreakdowns)
            {
                response.Countries.Add(country.CountryName);
            }

            return response;
        }
    }

    /// <summary>
    /// Response model for comparing multiple calculation scenarios
    /// </summary>
    public class CompareCalculationsResponse
    {
        /// <summary>
        /// Gets or sets the list of calculation scenarios being compared.
        /// </summary>
        public List<CalculationScenarioResponse> Scenarios { get; set; }

        /// <summary>
        /// Gets or sets the comparison of total costs between scenarios.
        /// The key is the scenario ID and the value is the total cost.
        /// </summary>
        public Dictionary<string, decimal> TotalCostComparison { get; set; }

        /// <summary>
        /// Gets or sets the comparison of costs by country between scenarios.
        /// The first key is the country code, the second key is the scenario ID,
        /// and the value is the cost for that country in that scenario.
        /// </summary>
        public Dictionary<string, Dictionary<string, decimal>> CountryCostComparison { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareCalculationsResponse"/> class.
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
        /// Gets or sets the unique identifier for the scenario.
        /// </summary>
        public string ScenarioId { get; set; }

        /// <summary>
        /// Gets or sets the name of the scenario.
        /// </summary>
        public string ScenarioName { get; set; }

        /// <summary>
        /// Gets or sets the type of VAT filing service selected for the calculation.
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the number of transactions/invoices per period.
        /// </summary>
        public int TransactionVolume { get; set; }

        /// <summary>
        /// Gets or sets the filing frequency (Monthly, Quarterly, Annually).
        /// </summary>
        public FilingFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the total cost of VAT filing services across all countries.
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the currency code for the calculation (e.g., EUR, USD, GBP).
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the detailed breakdown of costs by country.
        /// </summary>
        public List<CountryCalculationResponse> CountryBreakdowns { get; set; }

        /// <summary>
        /// Gets or sets the additional services included in the calculation.
        /// </summary>
        public List<string> AdditionalServices { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationScenarioResponse"/> class.
        /// </summary>
        public CalculationScenarioResponse()
        {
            CountryBreakdowns = new List<CountryCalculationResponse>();
            AdditionalServices = new List<string>();
        }
    }

    /// <summary>
    /// Response model for deleting a calculation
    /// </summary>
    public class DeleteCalculationResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the deleted calculation.
        /// </summary>
        public string CalculationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the calculation was successfully deleted.
        /// </summary>
        public bool Deleted { get; set; }
    }
}