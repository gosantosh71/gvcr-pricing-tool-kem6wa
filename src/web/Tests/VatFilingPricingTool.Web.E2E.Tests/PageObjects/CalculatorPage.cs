using Microsoft.Playwright; // v1.30.0
using System.Threading.Tasks; // v6.0.0
using System.Collections.Generic; // v6.0.0
using VatFilingPricingTool.Web.E2E.Tests.Fixtures;

namespace VatFilingPricingTool.Web.E2E.Tests.PageObjects
{
    /// <summary>
    /// Page object representing the pricing calculator page of the VAT Filing Pricing Tool web application.
    /// </summary>
    public class CalculatorPage : BasePage
    {
        // Selectors for country selection
        private const string CountrySelectorSelector = "[data-testid='country-selector']";
        private const string AddCountryButtonSelector = "[data-testid='add-country']";
        private const string CountryDropdownSelector = "[data-testid='country-dropdown']";
        private const string SelectedCountriesSelector = "[data-testid='selected-countries']";
        
        // Selectors for service type
        private const string ServiceTypeStandardSelector = "[data-testid='service-type-standard']";
        private const string ServiceTypeComplexSelector = "[data-testid='service-type-complex']";
        private const string ServiceTypePrioritySelector = "[data-testid='service-type-priority']";
        
        // Selectors for transaction volume and filing frequency
        private const string TransactionVolumeInputSelector = "[data-testid='transaction-volume']";
        private const string FilingFrequencyDropdownSelector = "[data-testid='filing-frequency']";
        
        // Selectors for additional services
        private const string AdditionalServicesSelector = "[data-testid='additional-services']";
        private const string TaxConsultancyCheckboxSelector = "[data-testid='tax-consultancy']";
        private const string HistoricalDataCheckboxSelector = "[data-testid='historical-data']";
        private const string ReconciliationCheckboxSelector = "[data-testid='reconciliation']";
        private const string AuditSupportCheckboxSelector = "[data-testid='audit-support']";
        
        // Selectors for buttons
        private const string CalculatePricingButtonSelector = "[data-testid='calculate-pricing']";
        private const string SaveParametersButtonSelector = "[data-testid='save-parameters']";
        
        // Selectors for messages and validation
        private const string ValidationErrorSelector = ".validation-error";
        private const string SuccessMessageSelector = ".alert-success";
        private const string ErrorMessageSelector = ".alert-danger";
        private const string LoadingSpinnerSelector = ".loading-spinner";

        /// <summary>
        /// Initializes a new instance of the CalculatorPage class.
        /// </summary>
        /// <param name="fixture">The Playwright fixture that manages the browser instance.</param>
        public CalculatorPage(PlaywrightFixture fixture) : base(fixture, "/calculator")
        {
            // Base constructor handles initialization
        }

        /// <summary>
        /// Checks if the calculator page is currently displayed.
        /// </summary>
        /// <returns>True if the calculator page is displayed, otherwise false.</returns>
        public async Task<bool> IsCalculatorPageDisplayedAsync()
        {
            bool countrySelectorVisible = await IsElementVisibleAsync(CountrySelectorSelector);
            bool serviceTypeVisible = await IsElementVisibleAsync(ServiceTypeStandardSelector);
            bool transactionVolumeVisible = await IsElementVisibleAsync(TransactionVolumeInputSelector);
            
            return countrySelectorVisible && serviceTypeVisible && transactionVolumeVisible;
        }

        /// <summary>
        /// Selects the specified countries in the country selector.
        /// </summary>
        /// <param name="countries">The list of countries to select.</param>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task SelectCountriesAsync(List<string> countries)
        {
            await WaitForElementToBeVisibleAsync(CountrySelectorSelector);
            
            foreach (var country in countries)
            {
                await ClickAsync(AddCountryButtonSelector);
                await WaitForElementToBeVisibleAsync(CountryDropdownSelector);
                await SelectOptionAsync(CountryDropdownSelector, country);
                
                // Wait for the selected country to appear in the list
                await WaitForElementToBeVisibleAsync(SelectedCountriesSelector);
            }
        }

        /// <summary>
        /// Selects the specified service type.
        /// </summary>
        /// <param name="serviceType">The service type to select (standard, complex, or priority).</param>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task SelectServiceTypeAsync(string serviceType)
        {
            await WaitForElementToBeVisibleAsync(ServiceTypeStandardSelector);
            
            switch (serviceType.ToLower())
            {
                case "standard":
                    await ClickAsync(ServiceTypeStandardSelector);
                    break;
                case "complex":
                    await ClickAsync(ServiceTypeComplexSelector);
                    break;
                case "priority":
                    await ClickAsync(ServiceTypePrioritySelector);
                    break;
                default:
                    throw new System.ArgumentException($"Invalid service type: {serviceType}. Valid values are: standard, complex, priority.");
            }
        }

        /// <summary>
        /// Enters the specified transaction volume.
        /// </summary>
        /// <param name="volume">The transaction volume to enter.</param>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task EnterTransactionVolumeAsync(int volume)
        {
            await WaitForElementToBeVisibleAsync(TransactionVolumeInputSelector);
            await FillAsync(TransactionVolumeInputSelector, volume.ToString());
        }

        /// <summary>
        /// Selects the specified filing frequency.
        /// </summary>
        /// <param name="frequency">The filing frequency to select.</param>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task SelectFilingFrequencyAsync(string frequency)
        {
            await WaitForElementToBeVisibleAsync(FilingFrequencyDropdownSelector);
            await SelectOptionAsync(FilingFrequencyDropdownSelector, frequency);
        }

        /// <summary>
        /// Selects the specified additional services.
        /// </summary>
        /// <param name="services">The list of additional services to select.</param>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task SelectAdditionalServicesAsync(List<string> services)
        {
            if (services == null || services.Count == 0)
            {
                return;
            }
            
            await WaitForElementToBeVisibleAsync(AdditionalServicesSelector);
            
            foreach (var service in services)
            {
                switch (service.ToLower())
                {
                    case "tax-consultancy":
                        await CheckAsync(TaxConsultancyCheckboxSelector, true);
                        break;
                    case "historical-data":
                        await CheckAsync(HistoricalDataCheckboxSelector, true);
                        break;
                    case "reconciliation":
                        await CheckAsync(ReconciliationCheckboxSelector, true);
                        break;
                    case "audit-support":
                        await CheckAsync(AuditSupportCheckboxSelector, true);
                        break;
                    default:
                        throw new System.ArgumentException($"Invalid additional service: {service}.");
                }
            }
        }

        /// <summary>
        /// Clicks the Calculate Pricing button to submit the form.
        /// </summary>
        /// <returns>The results page that is displayed after calculation.</returns>
        public async Task<ResultsPage> ClickCalculatePricingAsync()
        {
            await WaitForElementToBeVisibleAsync(CalculatePricingButtonSelector);
            await ClickAsync(CalculatePricingButtonSelector);
            await WaitForNavigationAsync();
            
            return new ResultsPage(Fixture);
        }

        /// <summary>
        /// Clicks the Save Parameters button to save the current calculation parameters.
        /// </summary>
        /// <returns>Asynchronous task representing the operation.</returns>
        public async Task ClickSaveParametersAsync()
        {
            await WaitForElementToBeVisibleAsync(SaveParametersButtonSelector);
            await ClickAsync(SaveParametersButtonSelector);
            await WaitForElementToBeVisibleAsync(SuccessMessageSelector);
        }

        /// <summary>
        /// Gets all validation error messages displayed on the page.
        /// </summary>
        /// <returns>List of validation error messages.</returns>
        public async Task<List<string>> GetValidationErrorsAsync()
        {
            var errors = new List<string>();
            
            if (!await IsElementVisibleAsync(ValidationErrorSelector))
            {
                return errors;
            }
            
            var errorElements = await Page.QuerySelectorAllAsync(ValidationErrorSelector);
            foreach (var element in errorElements)
            {
                string errorText = await element.TextContentAsync();
                if (!string.IsNullOrEmpty(errorText))
                {
                    errors.Add(errorText.Trim());
                }
            }
            
            return errors;
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
        /// Fills the calculator form with the specified parameters and calculates pricing.
        /// </summary>
        /// <param name="countries">The list of countries to select.</param>
        /// <param name="serviceType">The service type to select.</param>
        /// <param name="transactionVolume">The transaction volume to enter.</param>
        /// <param name="filingFrequency">The filing frequency to select.</param>
        /// <param name="additionalServices">The list of additional services to select (optional).</param>
        /// <returns>The results page that is displayed after calculation.</returns>
        public async Task<ResultsPage> FillCalculatorFormAsync(
            List<string> countries,
            string serviceType,
            int transactionVolume,
            string filingFrequency,
            List<string> additionalServices = null)
        {
            await SelectCountriesAsync(countries);
            await SelectServiceTypeAsync(serviceType);
            await EnterTransactionVolumeAsync(transactionVolume);
            await SelectFilingFrequencyAsync(filingFrequency);
            
            if (additionalServices != null && additionalServices.Count > 0)
            {
                await SelectAdditionalServicesAsync(additionalServices);
            }
            
            return await ClickCalculatePricingAsync();
        }

        /// <summary>
        /// Navigates directly to the calculator page.
        /// </summary>
        /// <returns>Asynchronous task representing the navigation operation.</returns>
        public async Task NavigateToCalculatorPageAsync()
        {
            await NavigateToAsync();
            await WaitForLoadingToFinishAsync();
        }

        /// <summary>
        /// Navigates to the calculator page with an existing calculation ID.
        /// </summary>
        /// <param name="calculationId">The ID of the calculation to edit.</param>
        /// <returns>Asynchronous task representing the navigation operation.</returns>
        public async Task NavigateToExistingCalculationAsync(string calculationId)
        {
            await Fixture.NavigateToAsync($"/calculator/{calculationId}");
            await WaitForLoadingToFinishAsync();
        }
    }
}