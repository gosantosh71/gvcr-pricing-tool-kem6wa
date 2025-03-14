using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Tests.Helpers
{
    /// <summary>
    /// Static class providing test data for unit tests in the VAT Filing Pricing Tool web application
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// Private constructor to prevent instantiation of static class
        /// </summary>
        private TestData() { }

        /// <summary>
        /// Creates a test user with specified ID and email
        /// </summary>
        /// <param name="userId">User ID to assign</param>
        /// <param name="email">Email to assign</param>
        /// <returns>A test user with the specified ID and email</returns>
        public static UserModel CreateTestUser(string userId, string email)
        {
            return new UserModel
            {
                UserId = userId,
                Email = email,
                FirstName = "Test",
                LastName = "User",
                Roles = new List<UserRole> { UserRole.Customer },
                CreatedDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                IsActive = true
            };
        }

        /// <summary>
        /// Creates a test administrator user
        /// </summary>
        /// <returns>A test user with administrator role</returns>
        public static UserModel CreateTestAdminUser()
        {
            return new UserModel
            {
                UserId = "admin-123",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                Roles = new List<UserRole> { UserRole.Administrator },
                CreatedDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                IsActive = true
            };
        }

        /// <summary>
        /// Creates a list of test countries for testing
        /// </summary>
        /// <returns>A list of test countries</returns>
        public static List<CountryModel> CreateTestCountries()
        {
            return new List<CountryModel>
            {
                new CountryModel 
                { 
                    CountryCode = "GB", 
                    Name = "United Kingdom", 
                    StandardVatRate = 20.0m,
                    CurrencyCode = "GBP",
                    IsActive = true
                },
                new CountryModel 
                {
                    CountryCode = "DE", 
                    Name = "Germany", 
                    StandardVatRate = 19.0m,
                    CurrencyCode = "EUR",
                    IsActive = true
                },
                new CountryModel 
                {
                    CountryCode = "FR", 
                    Name = "France", 
                    StandardVatRate = 20.0m,
                    CurrencyCode = "EUR",
                    IsActive = true
                },
                new CountryModel 
                {
                    CountryCode = "ES", 
                    Name = "Spain", 
                    StandardVatRate = 21.0m,
                    CurrencyCode = "EUR",
                    IsActive = true
                },
                new CountryModel 
                {
                    CountryCode = "IT", 
                    Name = "Italy", 
                    StandardVatRate = 22.0m,
                    CurrencyCode = "EUR",
                    IsActive = true
                }
            };
        }

        /// <summary>
        /// Creates a list of test country summaries for dropdown testing
        /// </summary>
        /// <returns>A list of test country summaries</returns>
        public static List<CountrySummaryModel> CreateTestCountrySummaries()
        {
            return new List<CountrySummaryModel>
            {
                new CountrySummaryModel 
                { 
                    CountryCode = "GB", 
                    Name = "United Kingdom", 
                    StandardVatRate = 20.0m,
                    IsActive = true
                },
                new CountrySummaryModel 
                {
                    CountryCode = "DE", 
                    Name = "Germany", 
                    StandardVatRate = 19.0m,
                    IsActive = true
                },
                new CountrySummaryModel 
                {
                    CountryCode = "FR", 
                    Name = "France", 
                    StandardVatRate = 20.0m,
                    IsActive = true
                },
                new CountrySummaryModel 
                {
                    CountryCode = "ES", 
                    Name = "Spain", 
                    StandardVatRate = 21.0m,
                    IsActive = true
                },
                new CountrySummaryModel 
                {
                    CountryCode = "IT", 
                    Name = "Italy", 
                    StandardVatRate = 22.0m,
                    IsActive = true
                }
            };
        }

        /// <summary>
        /// Creates a test calculation input model with default values
        /// </summary>
        /// <returns>A test calculation input model</returns>
        public static CalculationInputModel CreateTestCalculationInput()
        {
            return new CalculationInputModel
            {
                CountryCodes = new List<string> { "GB", "DE", "FR" },
                ServiceType = 1, // Complex Filing
                TransactionVolume = 500,
                FilingFrequency = 2, // Quarterly
                AdditionalServices = new List<string> { "tax-consultancy" },
                CurrencyCode = "EUR"
            };
        }

        /// <summary>
        /// Creates a test calculation result model with realistic values
        /// </summary>
        /// <param name="calculationId">Calculation ID to assign</param>
        /// <param name="userId">User ID to assign</param>
        /// <returns>A test calculation result model</returns>
        public static CalculationResultModel CreateTestCalculationResult(string calculationId, string userId)
        {
            var result = new CalculationResultModel
            {
                CalculationId = calculationId,
                UserId = userId,
                ServiceType = 1, // Complex Filing
                ServiceTypeName = "Complex Filing",
                TransactionVolume = 500,
                FilingFrequency = 2, // Quarterly
                FilingFrequencyName = "Quarterly",
                TotalCost = 4250.00m,
                CurrencyCode = "EUR",
                FormattedTotalCost = "€4,250.00",
                CalculationDate = DateTime.UtcNow,
                AdditionalServices = new List<string> { "tax-consultancy" },
                CountryBreakdowns = CreateTestCountryBreakdowns(),
                Discounts = new Dictionary<string, decimal>
                {
                    { "Volume Discount", 200.00m },
                    { "Multi-country Discount", 150.00m }
                },
                TotalDiscounts = 350.00m,
                FormattedTotalDiscounts = "€350.00",
                IsArchived = false
            };
            
            return result;
        }

        /// <summary>
        /// Creates test country breakdown models for calculation results
        /// </summary>
        /// <returns>A list of test country calculation results</returns>
        public static List<CountryCalculationResultModel> CreateTestCountryBreakdowns()
        {
            return new List<CountryCalculationResultModel>
            {
                new CountryCalculationResultModel
                {
                    CountryCode = "GB",
                    CountryName = "United Kingdom",
                    FlagCode = "gb",
                    BaseCost = 1200.00m,
                    FormattedBaseCost = "€1,200.00",
                    AdditionalCost = 300.00m,
                    FormattedAdditionalCost = "€300.00",
                    TotalCost = 1500.00m,
                    FormattedTotalCost = "€1,500.00",
                    CurrencyCode = "EUR",
                    AppliedRules = new List<string> { "UK-VAT-001", "UK-VAT-002" }
                },
                new CountryCalculationResultModel
                {
                    CountryCode = "DE",
                    CountryName = "Germany",
                    FlagCode = "de",
                    BaseCost = 1000.00m,
                    FormattedBaseCost = "€1,000.00",
                    AdditionalCost = 250.00m,
                    FormattedAdditionalCost = "€250.00",
                    TotalCost = 1250.00m,
                    FormattedTotalCost = "€1,250.00",
                    CurrencyCode = "EUR",
                    AppliedRules = new List<string> { "DE-VAT-001", "DE-VAT-002" }
                },
                new CountryCalculationResultModel
                {
                    CountryCode = "FR",
                    CountryName = "France",
                    FlagCode = "fr",
                    BaseCost = 1200.00m,
                    FormattedBaseCost = "€1,200.00",
                    AdditionalCost = 300.00m,
                    FormattedAdditionalCost = "€300.00",
                    TotalCost = 1500.00m,
                    FormattedTotalCost = "€1,500.00",
                    CurrencyCode = "EUR",
                    AppliedRules = new List<string> { "FR-VAT-001", "FR-VAT-002" }
                }
            };
        }

        /// <summary>
        /// Creates a test report request model
        /// </summary>
        /// <param name="calculationId">Calculation ID to use for the report</param>
        /// <returns>A test report request model</returns>
        public static ReportRequestModel CreateTestReportRequest(string calculationId)
        {
            return new ReportRequestModel
            {
                ReportTitle = "Test VAT Filing Report",
                CalculationId = calculationId,
                Format = 0, // PDF
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true,
                IncludeHistoricalComparison = false,
                IncludeTaxRateDetails = false,
                DeliveryOptions = new ReportDeliveryOptions
                {
                    DownloadImmediately = true,
                    SendEmail = false,
                    StoreForLater = true
                }
            };
        }

        /// <summary>
        /// Creates a test report model
        /// </summary>
        /// <param name="reportId">Report ID to assign</param>
        /// <param name="calculationId">Calculation ID the report is based on</param>
        /// <param name="userId">User ID to assign</param>
        /// <returns>A test report model</returns>
        public static ReportModel CreateTestReport(string reportId, string calculationId, string userId)
        {
            return new ReportModel
            {
                ReportId = reportId,
                UserId = userId,
                CalculationId = calculationId,
                ReportTitle = "VAT Filing Report Q2 2023",
                ReportType = "Calculation Report",
                Format = 0, // PDF
                StorageUrl = $"https://storage.example.com/reports/{reportId}.pdf",
                GenerationDate = DateTime.UtcNow,
                FileSize = 256000L, // 250 KB
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true,
                IncludeHistoricalComparison = false,
                IncludeTaxRateDetails = false,
                IsArchived = false
            };
        }
    }
}