using Microsoft.Playwright; // v1.30.0
using System.Threading.Tasks; // v6.0.0
using System.Collections.Generic; // v6.0.0
using VatFilingPricingTool.Web.E2E.Tests.Fixtures;

namespace VatFilingPricingTool.Web.E2E.Tests.PageObjects
{
    /// <summary>
    /// Page object representing the pricing results page of the VAT Filing Pricing Tool web application.
    /// </summary>
    public class ResultsPage : BasePage
    {
        // Selectors for pricing results elements
        private const string TotalCostSelector = "[data-testid='total-cost']";
        private const string CountryBreakdownSelector = "[data-testid='country-breakdown']";
        private const string ServiceDetailsSelector = "[data-testid='service-details']";
        private const string DiscountsSelector = "[data-testid='discounts']";
        private const string ViewDetailedBreakdownButtonSelector = "[data-testid='view-detailed-breakdown']";
        private const string EditInputsButtonSelector = "[data-testid='edit-inputs']";
        private const string SaveEstimateButtonSelector = "[data-testid='save-estimate']";
        private const string GenerateReportButtonSelector = "[data-testid='generate-report']";
        private const string DownloadPdfButtonSelector = "[data-testid='download-pdf']";
        
        // Selectors for report modal
        private const string ReportModalSelector = "[data-testid='report-modal']";
        private const string ReportTitleInputSelector = "[data-testid='report-title']";
        private const string ReportFormatDropdownSelector = "[data-testid='report-format']";
        private const string IncludeCountryBreakdownCheckboxSelector = "[data-testid='include-country-breakdown']";
        private const string IncludeServiceDetailsCheckboxSelector = "[data-testid='include-service-details']";
        private const string IncludeAppliedDiscountsCheckboxSelector = "[data-testid='include-applied-discounts']";
        private const string IncludeHistoricalComparisonCheckboxSelector = "[data-testid='include-historical-comparison']";
        private const string IncludeTaxRateDetailsCheckboxSelector = "[data-testid='include-tax-rate-details']";
        private const string DownloadImmediatelyRadioSelector = "[data-testid='download-immediately']";
        private const string EmailToRadioSelector = "[data-testid='email-to']";
        private const string EmailInputSelector = "[data-testid='email-input']";
        private const string GenerateReportSubmitButtonSelector = "[data-testid='generate-report-submit']";
        
        // Selectors for save estimate modal
        private const string SaveEstimateModalSelector = "[data-testid='save-estimate-modal']";
        private const string SaveNameInputSelector = "[data-testid='save-name']";
        private const string SaveDescriptionInputSelector = "[data-testid='save-description']";
        private const string SaveSubmitButtonSelector = "[data-testid='save-submit']";
        
        // Selectors for messages and loading
        private const string SuccessMessageSelector = ".alert-success";
        private const string ErrorMessageSelector = ".alert-danger";
        private const string LoadingSpinnerSelector = ".loading-spinner";

        /// <summary>
        /// Initializes a new instance of the ResultsPage class.
        /// </summary>
        /// <param name="fixture">The Playwright fixture that manages the browser instance.</param>
        public ResultsPage(PlaywrightFixture fixture) : base(fixture, "/results")
        {
            // Base constructor handles initialization
        }

        /// <summary>
        /// Checks if the results page is currently displayed.
        /// </summary>
        /// <returns>True if the results page is displayed, otherwise false.</returns>
        public async Task<bool> IsResultsPageDisplayedAsync()
        {
            bool totalCostVisible = await IsElementVisibleAsync(TotalCostSelector);
            bool countryBreakdownVisible = await IsElementVisibleAsync(CountryBreakdownSelector);
            bool serviceDetailsVisible = await IsElementVisibleAsync(ServiceDetailsSelector);
            
            return totalCostVisible && countryBreakdownVisible && serviceDetailsVisible;
        }

        /// <summary>
        /// Gets the total cost displayed on the results page.
        /// </summary>
        /// <returns>The total cost text including currency symbol.</returns>
        public async Task<string> GetTotalCostAsync()
        {
            await WaitForElementToBeVisibleAsync(TotalCostSelector);
            return await GetTextContentAsync(TotalCostSelector);
        }

        /// <summary>
        /// Gets the country breakdown details from the results page.
        /// </summary>
        /// <returns>Dictionary mapping country names to their costs.</returns>
        public async Task<Dictionary<string, string>> GetCountryBreakdownAsync()
        {
            await WaitForElementToBeVisibleAsync(CountryBreakdownSelector);
            
            var result = new Dictionary<string, string>();
            var countryRows = await Page.QuerySelectorAllAsync($"{CountryBreakdownSelector} tr");
            
            // Skip header row if present
            for (int i = 1; i < countryRows.Count; i++)
            {
                var row = countryRows[i];
                var cells = await row.QuerySelectorAllAsync("td");
                
                if (cells.Count >= 2)
                {
                    string countryName = await cells[0].TextContentAsync() ?? string.Empty;
                    string cost = await cells[cells.Count - 1].TextContentAsync() ?? string.Empty;
                    
                    countryName = countryName.Trim();
                    cost = cost.Trim();
                    
                    if (!string.IsNullOrEmpty(countryName) && !string.IsNullOrEmpty(cost))
                    {
                        result[countryName] = cost;
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// Gets the service details from the results page.
        /// </summary>
        /// <returns>Dictionary mapping service detail names to their values.</returns>
        public async Task<Dictionary<string, string>> GetServiceDetailsAsync()
        {
            await WaitForElementToBeVisibleAsync(ServiceDetailsSelector);
            
            var result = new Dictionary<string, string>();
            var detailRows = await Page.QuerySelectorAllAsync($"{ServiceDetailsSelector} .detail-row");
            
            foreach (var row in detailRows)
            {
                var label = await row.QuerySelectorAsync(".detail-label");
                var value = await row.QuerySelectorAsync(".detail-value");
                
                if (label != null && value != null)
                {
                    string labelText = await label.TextContentAsync() ?? string.Empty;
                    string valueText = await value.TextContentAsync() ?? string.Empty;
                    
                    labelText = labelText.Trim();
                    valueText = valueText.Trim();
                    
                    if (!string.IsNullOrEmpty(labelText))
                    {
                        result[labelText] = valueText;
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// Gets the discounts information from the results page.
        /// </summary>
        /// <returns>Dictionary mapping discount types to their values.</returns>
        public async Task<Dictionary<string, string>> GetDiscountsAsync()
        {
            var result = new Dictionary<string, string>();
            
            if (!await IsElementVisibleAsync(DiscountsSelector))
            {
                return result; // No discounts section visible
            }
            
            var discountRows = await Page.QuerySelectorAllAsync($"{DiscountsSelector} .discount-row");
            
            foreach (var row in discountRows)
            {
                var label = await row.QuerySelectorAsync(".discount-label");
                var value = await row.QuerySelectorAsync(".discount-value");
                
                if (label != null && value != null)
                {
                    string labelText = await label.TextContentAsync() ?? string.Empty;
                    string valueText = await value.TextContentAsync() ?? string.Empty;
                    
                    labelText = labelText.Trim();
                    valueText = valueText.Trim();
                    
                    if (!string.IsNullOrEmpty(labelText))
                    {
                        result[labelText] = valueText;
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// Clicks the View Detailed Breakdown button.
        /// </summary>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task ClickViewDetailedBreakdownAsync()
        {
            await WaitForElementToBeVisibleAsync(ViewDetailedBreakdownButtonSelector);
            await ClickAsync(ViewDetailedBreakdownButtonSelector);
            await WaitForLoadingToFinishAsync();
        }

        /// <summary>
        /// Clicks the Edit Inputs button to return to the calculator.
        /// </summary>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task ClickEditInputsAsync()
        {
            await WaitForElementToBeVisibleAsync(EditInputsButtonSelector);
            await ClickAsync(EditInputsButtonSelector);
            await WaitForNavigationAsync();
        }

        /// <summary>
        /// Clicks the Save Estimate button to open the save modal.
        /// </summary>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task ClickSaveEstimateAsync()
        {
            await WaitForElementToBeVisibleAsync(SaveEstimateButtonSelector);
            await ClickAsync(SaveEstimateButtonSelector);
            await WaitForElementToBeVisibleAsync(SaveEstimateModalSelector);
        }

        /// <summary>
        /// Clicks the Generate Report button to open the report modal.
        /// </summary>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task ClickGenerateReportAsync()
        {
            await WaitForElementToBeVisibleAsync(GenerateReportButtonSelector);
            await ClickAsync(GenerateReportButtonSelector);
            await WaitForElementToBeVisibleAsync(ReportModalSelector);
        }

        /// <summary>
        /// Clicks the Download PDF button to download the results as a PDF.
        /// </summary>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task ClickDownloadPdfAsync()
        {
            await WaitForElementToBeVisibleAsync(DownloadPdfButtonSelector);
            await ClickAsync(DownloadPdfButtonSelector);
        }

        /// <summary>
        /// Checks if the report generation modal is visible.
        /// </summary>
        /// <returns>True if the report modal is visible, otherwise false.</returns>
        public async Task<bool> IsReportModalVisibleAsync()
        {
            return await IsElementVisibleAsync(ReportModalSelector);
        }

        /// <summary>
        /// Checks if the save estimate modal is visible.
        /// </summary>
        /// <returns>True if the save estimate modal is visible, otherwise false.</returns>
        public async Task<bool> IsSaveEstimateModalVisibleAsync()
        {
            return await IsElementVisibleAsync(SaveEstimateModalSelector);
        }

        /// <summary>
        /// Fills the report generation form with the specified parameters.
        /// </summary>
        /// <param name="title">The report title.</param>
        /// <param name="format">The report format (PDF, Excel, Both).</param>
        /// <param name="includeCountryBreakdown">Whether to include country breakdown in the report.</param>
        /// <param name="includeServiceDetails">Whether to include service details in the report.</param>
        /// <param name="includeAppliedDiscounts">Whether to include applied discounts in the report.</param>
        /// <param name="includeHistoricalComparison">Whether to include historical comparison in the report.</param>
        /// <param name="includeTaxRateDetails">Whether to include tax rate details in the report.</param>
        /// <param name="downloadImmediately">Whether to download the report immediately.</param>
        /// <param name="email">The email address to send the report to if not downloading immediately.</param>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task FillReportFormAsync(
            string title, 
            string format, 
            bool includeCountryBreakdown = true, 
            bool includeServiceDetails = true, 
            bool includeAppliedDiscounts = true, 
            bool includeHistoricalComparison = false, 
            bool includeTaxRateDetails = false,
            bool downloadImmediately = true,
            string email = null)
        {
            await WaitForElementToBeVisibleAsync(ReportModalSelector);
            
            // Fill report title
            await FillAsync(ReportTitleInputSelector, title);
            
            // Select report format
            await SelectOptionAsync(ReportFormatDropdownSelector, format);
            
            // Set include checkboxes
            await CheckAsync(IncludeCountryBreakdownCheckboxSelector, includeCountryBreakdown);
            await CheckAsync(IncludeServiceDetailsCheckboxSelector, includeServiceDetails);
            await CheckAsync(IncludeAppliedDiscountsCheckboxSelector, includeAppliedDiscounts);
            await CheckAsync(IncludeHistoricalComparisonCheckboxSelector, includeHistoricalComparison);
            await CheckAsync(IncludeTaxRateDetailsCheckboxSelector, includeTaxRateDetails);
            
            // Set delivery options
            if (downloadImmediately)
            {
                await ClickAsync(DownloadImmediatelyRadioSelector);
            }
            else if (email != null)
            {
                await ClickAsync(EmailToRadioSelector);
                await FillAsync(EmailInputSelector, email);
            }
        }

        /// <summary>
        /// Fills the save estimate form with the specified parameters.
        /// </summary>
        /// <param name="name">The name to save the estimate as.</param>
        /// <param name="description">Optional description for the saved estimate.</param>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task FillSaveEstimateFormAsync(string name, string description = null)
        {
            await WaitForElementToBeVisibleAsync(SaveEstimateModalSelector);
            
            // Fill save name
            await FillAsync(SaveNameInputSelector, name);
            
            // Fill description if provided
            if (description != null)
            {
                await FillAsync(SaveDescriptionInputSelector, description);
            }
        }

        /// <summary>
        /// Submits the report generation form.
        /// </summary>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task SubmitReportFormAsync()
        {
            await WaitForElementToBeVisibleAsync(GenerateReportSubmitButtonSelector);
            await ClickAsync(GenerateReportSubmitButtonSelector);
            await WaitForLoadingToFinishAsync();
        }

        /// <summary>
        /// Submits the save estimate form.
        /// </summary>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task SubmitSaveEstimateFormAsync()
        {
            await WaitForElementToBeVisibleAsync(SaveSubmitButtonSelector);
            await ClickAsync(SaveSubmitButtonSelector);
            await WaitForLoadingToFinishAsync();
        }

        /// <summary>
        /// Generates a report with the specified parameters.
        /// </summary>
        /// <param name="title">The report title.</param>
        /// <param name="format">The report format (PDF, Excel, Both).</param>
        /// <param name="includeCountryBreakdown">Whether to include country breakdown in the report.</param>
        /// <param name="includeServiceDetails">Whether to include service details in the report.</param>
        /// <param name="includeAppliedDiscounts">Whether to include applied discounts in the report.</param>
        /// <param name="includeHistoricalComparison">Whether to include historical comparison in the report.</param>
        /// <param name="includeTaxRateDetails">Whether to include tax rate details in the report.</param>
        /// <param name="downloadImmediately">Whether to download the report immediately.</param>
        /// <param name="email">The email address to send the report to if not downloading immediately.</param>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task GenerateReportAsync(
            string title, 
            string format, 
            bool includeCountryBreakdown = true, 
            bool includeServiceDetails = true, 
            bool includeAppliedDiscounts = true, 
            bool includeHistoricalComparison = false, 
            bool includeTaxRateDetails = false,
            bool downloadImmediately = true,
            string email = null)
        {
            await ClickGenerateReportAsync();
            await FillReportFormAsync(
                title, 
                format, 
                includeCountryBreakdown, 
                includeServiceDetails, 
                includeAppliedDiscounts, 
                includeHistoricalComparison, 
                includeTaxRateDetails,
                downloadImmediately,
                email);
            await SubmitReportFormAsync();
        }

        /// <summary>
        /// Saves the estimate with the specified name and description.
        /// </summary>
        /// <param name="name">The name to save the estimate as.</param>
        /// <param name="description">Optional description for the saved estimate.</param>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task SaveEstimateAsync(string name, string description = null)
        {
            await ClickSaveEstimateAsync();
            await FillSaveEstimateFormAsync(name, description);
            await SubmitSaveEstimateFormAsync();
        }

        /// <summary>
        /// Gets the success message displayed on the page, if any.
        /// </summary>
        /// <returns>The success message text, or empty string if no success message is displayed.</returns>
        public async Task<string> GetSuccessMessageAsync()
        {
            if (await IsElementVisibleAsync(SuccessMessageSelector))
            {
                return await GetTextContentAsync(SuccessMessageSelector);
            }
            
            return string.Empty;
        }

        /// <summary>
        /// Gets the error message displayed on the page, if any.
        /// </summary>
        /// <returns>The error message text, or empty string if no error is displayed.</returns>
        public async Task<string> GetErrorMessageAsync()
        {
            if (await IsElementVisibleAsync(ErrorMessageSelector))
            {
                return await GetTextContentAsync(ErrorMessageSelector);
            }
            
            return string.Empty;
        }

        /// <summary>
        /// Checks if the loading spinner is currently visible.
        /// </summary>
        /// <returns>True if the loading spinner is visible, otherwise false.</returns>
        public async Task<bool> IsLoadingSpinnerVisibleAsync()
        {
            return await IsElementVisibleAsync(LoadingSpinnerSelector);
        }

        /// <summary>
        /// Waits for the loading spinner to disappear, indicating that the operation has completed.
        /// </summary>
        /// <returns>Asynchronous task representing the wait operation.</returns>
        public async Task WaitForLoadingToFinishAsync()
        {
            if (await IsLoadingSpinnerVisibleAsync())
            {
                await Page.WaitForSelectorAsync(LoadingSpinnerSelector, new { state = WaitForSelectorState.Hidden });
            }
        }

        /// <summary>
        /// Navigates directly to the results page for a specific calculation.
        /// </summary>
        /// <param name="calculationId">The ID of the calculation to view results for.</param>
        /// <returns>Asynchronous task representing the navigation operation.</returns>
        public async Task NavigateToResultsPageAsync(string calculationId)
        {
            await Fixture.NavigateToAsync($"/results/{calculationId}");
            await WaitForLoadingToFinishAsync();
        }
    }
}