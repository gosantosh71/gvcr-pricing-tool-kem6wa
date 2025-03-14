using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;

namespace VatFilingPricingTool.UnitTests.Helpers
{
    /// <summary>
    /// Static class providing methods to generate mock data for unit testing
    /// </summary>
    public static class MockData
    {
        /// <summary>
        /// Creates a list of mock users for testing
        /// </summary>
        /// <returns>A list of mock User entities</returns>
        public static List<User> GetMockUsers()
        {
            var users = new List<User>();
            
            // Administrator
            var admin = User.Create("admin@example.com", "Admin", "User", UserRole.Administrator);
            users.Add(admin);
            
            // Pricing Administrator
            var pricingAdmin = User.Create("pricing@example.com", "Pricing", "Admin", UserRole.PricingAdministrator);
            users.Add(pricingAdmin);
            
            // Accountant
            var accountant = User.Create("accountant@example.com", "Accountant", "User", UserRole.Accountant);
            users.Add(accountant);
            
            // Customer
            var customer = User.Create("customer@example.com", "Customer", "User", UserRole.Customer);
            users.Add(customer);
            
            // Azure AD User
            var azureAdUser = User.CreateWithAzureAd("azuread@example.com", "Azure", "User", UserRole.Customer, "azure-ad-object-id");
            users.Add(azureAdUser);
            
            return users;
        }

        /// <summary>
        /// Creates a list of mock countries for testing
        /// </summary>
        /// <returns>A list of mock Country entities</returns>
        public static List<Country> GetMockCountries()
        {
            var countries = new List<Country>();
            
            // United Kingdom
            var uk = Country.Create("GB", "United Kingdom", 20.0m, "GBP");
            uk.AddFilingFrequency(FilingFrequency.Monthly);
            uk.AddFilingFrequency(FilingFrequency.Quarterly);
            countries.Add(uk);
            
            // Germany
            var germany = Country.Create("DE", "Germany", 19.0m, "EUR");
            germany.AddFilingFrequency(FilingFrequency.Monthly);
            germany.AddFilingFrequency(FilingFrequency.Quarterly);
            germany.AddFilingFrequency(FilingFrequency.Annually);
            countries.Add(germany);
            
            // France
            var france = Country.Create("FR", "France", 20.0m, "EUR");
            france.AddFilingFrequency(FilingFrequency.Monthly);
            france.AddFilingFrequency(FilingFrequency.Quarterly);
            countries.Add(france);
            
            // Italy
            var italy = Country.Create("IT", "Italy", 22.0m, "EUR");
            italy.AddFilingFrequency(FilingFrequency.Quarterly);
            countries.Add(italy);
            
            // Spain
            var spain = Country.Create("ES", "Spain", 21.0m, "EUR");
            spain.AddFilingFrequency(FilingFrequency.Quarterly);
            countries.Add(spain);
            
            return countries;
        }

        /// <summary>
        /// Creates a list of mock rules for testing
        /// </summary>
        /// <returns>A list of mock Rule entities</returns>
        public static List<Rule> GetMockRules()
        {
            var rules = new List<Rule>();
            
            // VAT rate rule for UK
            var ukVatRule = Rule.Create("GB", RuleType.VatRate, "UK Standard VAT Rate", "basePrice * 0.20", DateTime.UtcNow, "Standard VAT rate for UK filings");
            rules.Add(ukVatRule);
            
            // Threshold rule for UK
            var ukThresholdRule = Rule.Create("GB", RuleType.Threshold, "UK Transaction Threshold", "transactionVolume > 100 ? basePrice * 1.5 : basePrice", DateTime.UtcNow, "Volume-based pricing for UK");
            ukThresholdRule.AddParameter("transactionVolume", "number");
            ukThresholdRule.AddCondition("serviceType", "equals", "StandardFiling");
            rules.Add(ukThresholdRule);
            
            // VAT rate rule for Germany
            var deVatRule = Rule.Create("DE", RuleType.VatRate, "Germany Standard VAT Rate", "basePrice * 0.19", DateTime.UtcNow, "Standard VAT rate for German filings");
            rules.Add(deVatRule);
            
            // Complexity rule for Germany
            var deComplexityRule = Rule.Create("DE", RuleType.Complexity, "Germany Complexity Factor", "serviceType == 'ComplexFiling' ? basePrice * 1.25 : basePrice", DateTime.UtcNow, "Complexity-based pricing for Germany");
            deComplexityRule.AddParameter("serviceType", "string");
            rules.Add(deComplexityRule);
            
            // VAT rate rule for France
            var frVatRule = Rule.Create("FR", RuleType.VatRate, "France Standard VAT Rate", "basePrice * 0.20", DateTime.UtcNow, "Standard VAT rate for French filings");
            rules.Add(frVatRule);
            
            // Discount rule for multi-country
            var euDiscountRule = Rule.Create("EU", RuleType.Discount, "EU Multi-Country Discount", "countriesCount > 2 ? basePrice * 0.9 : basePrice", DateTime.UtcNow, "Discount for multi-country filings");
            euDiscountRule.AddParameter("countriesCount", "number");
            rules.Add(euDiscountRule);
            
            return rules;
        }

        /// <summary>
        /// Creates a list of mock calculations for testing
        /// </summary>
        /// <returns>A list of mock Calculation entities</returns>
        public static List<Calculation> GetMockCalculations()
        {
            var calculations = new List<Calculation>();
            
            // Standard calculation
            var standardCalculation = Calculation.Create("customer-id", "standard-service-id", 500, FilingFrequency.Quarterly, "EUR");
            standardCalculation.AddCountry("GB", Money.Create(1500m, "EUR"));
            standardCalculation.AddCountry("DE", Money.Create(1250m, "EUR"));
            calculations.Add(standardCalculation);
            
            // Complex calculation
            var complexCalculation = Calculation.Create("customer-id", "complex-service-id", 1000, FilingFrequency.Monthly, "EUR");
            complexCalculation.AddCountry("GB", Money.Create(2000m, "EUR"));
            complexCalculation.AddCountry("DE", Money.Create(1800m, "EUR"));
            complexCalculation.AddCountry("FR", Money.Create(1900m, "EUR"));
            calculations.Add(complexCalculation);
            
            // Priority calculation
            var priorityCalculation = Calculation.Create("customer-id", "priority-service-id", 250, FilingFrequency.Quarterly, "GBP");
            priorityCalculation.AddCountry("GB", Money.Create(2500m, "GBP"));
            calculations.Add(priorityCalculation);
            
            return calculations;
        }

        /// <summary>
        /// Creates a list of mock services for testing
        /// </summary>
        /// <returns>A list of mock Service entities</returns>
        public static List<Service> GetMockServices()
        {
            var services = new List<Service>();
            
            // Standard filing service
            var standardService = Service.Create("Standard VAT Filing", "Basic VAT filing service", 800m, "EUR", ServiceType.StandardFiling, 3);
            services.Add(standardService);
            
            // Complex filing service
            var complexService = Service.Create("Complex VAT Filing", "Advanced VAT filing with reconciliation", 1200m, "EUR", ServiceType.ComplexFiling, 7);
            services.Add(complexService);
            
            // Priority service
            var priorityService = Service.Create("Priority VAT Filing", "Expedited VAT filing service", 1500m, "EUR", ServiceType.PriorityService, 5);
            services.Add(priorityService);
            
            return services;
        }

        /// <summary>
        /// Creates a list of mock reports for testing
        /// </summary>
        /// <returns>A list of mock Report entities</returns>
        public static List<Report> GetMockReports()
        {
            var reports = new List<Report>();
            
            // PDF report
            var pdfReport = Report.Create("customer-id", "calculation-id", "Q2 2023 VAT Filing Estimate", "Cost Breakdown", ReportFormat.PDF);
            pdfReport.UpdateStorageInfo("https://storage.example.com/reports/report1.pdf", 256000L);
            reports.Add(pdfReport);
            
            // Excel report
            var excelReport = Report.Create("customer-id", "calculation-id", "Annual VAT Summary 2023", "Annual Summary", ReportFormat.Excel);
            excelReport.UpdateStorageInfo("https://storage.example.com/reports/report2.xlsx", 512000L);
            reports.Add(excelReport);
            
            // CSV report
            var csvReport = Report.Create("accountant-id", "calculation-id", "VAT Transactions Q3 2023", "Transaction List", ReportFormat.CSV);
            csvReport.UpdateStorageInfo("https://storage.example.com/reports/report3.csv", 128000L);
            reports.Add(csvReport);
            
            return reports;
        }

        /// <summary>
        /// Creates a list of mock additional services for testing
        /// </summary>
        /// <returns>A list of mock AdditionalService entities</returns>
        public static List<AdditionalService> GetMockAdditionalServices()
        {
            var additionalServices = new List<AdditionalService>();
            
            // Tax consultancy
            var taxConsultancy = AdditionalService.Create("Tax Consultancy", "Expert tax advice and consultation", Money.Create(300m, "EUR"));
            additionalServices.Add(taxConsultancy);
            
            // Historical data processing
            var historicalData = AdditionalService.Create("Historical Data Processing", "Processing and analysis of historical VAT data", Money.Create(250m, "EUR"));
            additionalServices.Add(historicalData);
            
            // Reconciliation services
            var reconciliation = AdditionalService.Create("Reconciliation Services", "Detailed reconciliation of VAT transactions", Money.Create(350m, "EUR"));
            additionalServices.Add(reconciliation);
            
            // Audit support
            var auditSupport = AdditionalService.Create("Audit Support", "Assistance during tax authority audits", Money.Create(500m, "EUR"));
            additionalServices.Add(auditSupport);
            
            return additionalServices;
        }
    }
}