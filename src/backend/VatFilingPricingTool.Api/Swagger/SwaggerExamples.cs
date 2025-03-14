using System;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Filters; // Version 7.0.2
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Api.Models.Responses;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Swagger
{
    /// <summary>
    /// Provides example data for login request in Swagger documentation
    /// </summary>
    public class LoginRequestExample : IExamplesProvider<LoginRequest>
    {
        /// <summary>
        /// Returns an example login request with sample data
        /// </summary>
        /// <returns>A sample login request with example values</returns>
        public LoginRequest GetExamples()
        {
            return new LoginRequest
            {
                Email = "user@example.com",
                Password = "Password123!",
                RememberMe = true
            };
        }
    }

    /// <summary>
    /// Provides example data for registration request in Swagger documentation
    /// </summary>
    public class RegisterRequestExample : IExamplesProvider<RegisterRequest>
    {
        /// <summary>
        /// Returns an example registration request with sample data
        /// </summary>
        /// <returns>A sample registration request with example values</returns>
        public RegisterRequest GetExamples()
        {
            return new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "John",
                LastName = "Smith",
                CompanyName = "Example Company Ltd",
                PhoneNumber = "+44123456789",
                Roles = new List<UserRole> { UserRole.Customer }
            };
        }
    }

    /// <summary>
    /// Provides example data for calculation request in Swagger documentation
    /// </summary>
    public class CalculationRequestExample : IExamplesProvider<CalculationRequest>
    {
        /// <summary>
        /// Returns an example calculation request with sample data
        /// </summary>
        /// <returns>A sample calculation request with example values</returns>
        public CalculationRequest GetExamples()
        {
            return new CalculationRequest
            {
                ServiceType = ServiceType.ComplexFiling,
                TransactionVolume = 500,
                Frequency = FilingFrequency.Quarterly,
                CountryCodes = new List<string> { "GB", "DE", "FR" },
                AdditionalServices = new List<string> { "Tax consultancy", "Reconciliation services" }
            };
        }
    }

    /// <summary>
    /// Provides example data for report generation request in Swagger documentation
    /// </summary>
    public class ReportGenerationRequestExample : IExamplesProvider<GenerateReportRequest>
    {
        /// <summary>
        /// Returns an example report generation request with sample data
        /// </summary>
        /// <returns>A sample report generation request with example values</returns>
        public GenerateReportRequest GetExamples()
        {
            return new GenerateReportRequest
            {
                CalculationId = "c8f7e8d6-9a5b-4c3d-8e7f-1a2b3c4d5e6f",
                ReportTitle = "Q2 2023 VAT Filing Estimate",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true,
                IncludeHistoricalComparison = false,
                IncludeTaxRateDetails = false,
                DeliveryOptions = new ReportDeliveryOptions
                {
                    DownloadImmediately = true,
                    SendEmail = true,
                    EmailAddress = "user@example.com",
                    EmailSubject = "Your VAT Filing Cost Report",
                    EmailMessage = "Please find attached your VAT filing cost report."
                }
            };
        }
    }

    /// <summary>
    /// Provides example data for calculation response in Swagger documentation
    /// </summary>
    public class CalculationResponseExample : IExamplesProvider<CalculationResponse>
    {
        /// <summary>
        /// Returns an example calculation response with sample data
        /// </summary>
        /// <returns>A sample calculation response with example values</returns>
        public CalculationResponse GetExamples()
        {
            var response = new CalculationResponse
            {
                CalculationId = "c8f7e8d6-9a5b-4c3d-8e7f-1a2b3c4d5e6f",
                ServiceType = ServiceType.ComplexFiling,
                TransactionVolume = 500,
                Frequency = FilingFrequency.Quarterly,
                TotalCost = 4250.00m,
                CurrencyCode = "EUR",
                CalculationDate = DateTime.UtcNow,
                CountryBreakdowns = new List<CountryBreakdownResponse>
                {
                    new CountryBreakdownResponse
                    {
                        CountryCode = "GB",
                        CountryName = "United Kingdom",
                        BaseCost = 1200.00m,
                        AdditionalCost = 300.00m,
                        TotalCost = 1500.00m,
                        AppliedRules = new List<string> { "Base rate applied", "Volume discount" }
                    },
                    new CountryBreakdownResponse
                    {
                        CountryCode = "DE",
                        CountryName = "Germany",
                        BaseCost = 1000.00m,
                        AdditionalCost = 250.00m,
                        TotalCost = 1250.00m,
                        AppliedRules = new List<string> { "Base rate applied", "Volume discount" }
                    },
                    new CountryBreakdownResponse
                    {
                        CountryCode = "FR",
                        CountryName = "France",
                        BaseCost = 1200.00m,
                        AdditionalCost = 300.00m,
                        TotalCost = 1500.00m,
                        AppliedRules = new List<string> { "Base rate applied", "Volume discount" }
                    }
                },
                AdditionalServices = new List<string> { "Tax consultancy", "Reconciliation services" },
                Discounts = new Dictionary<string, decimal>
                {
                    { "Volume Discount", -200.00m },
                    { "Multi-country Discount", -150.00m }
                }
            };

            return response;
        }
    }
}