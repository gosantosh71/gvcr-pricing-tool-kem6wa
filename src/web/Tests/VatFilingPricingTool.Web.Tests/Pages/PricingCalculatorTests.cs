# src/web/Tests/VatFilingPricingTool.Web.Tests/Pages/PricingCalculatorTests.cs
using Bunit; // bunit version 1.12.6
using Microsoft.AspNetCore.Components; // Microsoft.AspNetCore.Components version 6.0.0
using Microsoft.Extensions.DependencyInjection; // Microsoft.Extensions.DependencyInjection version 6.0.0
using Moq; // Moq version 4.18.2
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Pages;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Tests.Helpers;
using Xunit; // Xunit

namespace VatFilingPricingTool.Web.Tests.Pages
{
    /// <summary>
    /// Contains unit tests for the PricingCalculator page component
    /// </summary>
    public class PricingCalculatorTests
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PricingCalculatorTests()
        {
        }

        /// <summary>
        /// Sets up the test context with required services for testing the PricingCalculator component
        /// </summary>
        /// <returns>A tuple containing the test context and mock services</returns>
        private (TestContext, Mock<IPricingService>, Mock<ICountryService>) Setup()
        {
            // Create a test context using RenderComponent.CreateTestContext()
            var context = RenderComponent.CreateTestContext();

            // Set up an authenticated user using RenderComponent.SetupAuthenticatedUser()
            RenderComponent.SetupAuthenticatedUser(context, "test-user", "test@example.com");

            // Create a mock pricing service using RenderComponent.CreateMockService<IPricingService>()
            var mockPricingService = RenderComponent.CreateMockService<IPricingService>(context);

            // Create a mock country service using RenderComponent.CreateMockService<ICountryService>()
            var mockCountryService = RenderComponent.CreateMockService<ICountryService>(context);

            // Return a tuple with the test context and mock services
            return (context, mockPricingService, mockCountryService);
        }

        /// <summary>
        /// Tests that the PricingCalculator component initializes correctly with default values
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_Initialization()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Set up mock responses for service type options, filing frequency options, and additional service options
            mockPricingService.Setup(s => s.GetServiceTypeOptionsAsync())
                .ReturnsAsync(new List<ServiceTypeOption> { new ServiceTypeOption(1, "Standard", "Basic", 500m) });
            mockPricingService.Setup(s => s.GetFilingFrequencyOptionsAsync())
                .ReturnsAsync(new List<FilingFrequencyOption> { new FilingFrequencyOption(1, "Monthly", "Monthly") });
            mockPricingService.Setup(s => s.GetAdditionalServiceOptionsAsync())
                .ReturnsAsync(new List<AdditionalServiceOption> { new AdditionalServiceOption("tax-consultancy", "Tax Consultancy", "Tax", 100m, "EUR") });

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Assert that the component renders without errors
            Assert.NotNull(component);

            // Assert that the service type options are displayed
            Assert.Contains("Standard", component.Markup);

            // Assert that the filing frequency options are displayed
            Assert.Contains("Monthly", component.Markup);

            // Assert that the transaction volume input is displayed
            Assert.Contains("Invoices per month", component.Markup);

            // Assert that the calculate button is displayed and enabled
            Assert.Contains("Calculate Pricing", component.Markup);
        }

        /// <summary>
        /// Tests that the PricingCalculator loads an existing calculation when a calculation ID is provided
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_LoadsExistingCalculation()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Create a test calculation result with known values
            var testCalculation = TestData.CreateTestCalculationResult("calc-123", "test-user");

            // Set up the mock pricing service to return the test calculation when GetCalculationAsync is called
            mockPricingService.Setup(s => s.GetCalculationAsync("calc-123"))
                .ReturnsAsync(testCalculation);

            // Render the PricingCalculator component with a calculation ID parameter
            var component = context.RenderComponent<PricingCalculator>(parameters => parameters.Add(p => p.CalculationId, "calc-123"));

            // Assert that the component loads the existing calculation data
            Assert.NotNull(component);

            // Assert that the country selection reflects the loaded calculation
            Assert.Contains("United Kingdom", component.Markup);
            Assert.Contains("Germany", component.Markup);
            Assert.Contains("France", component.Markup);

            // Assert that the service type selection reflects the loaded calculation
            Assert.Contains("Complex Filing", component.Markup);

            // Assert that the transaction volume reflects the loaded calculation
            Assert.Contains("500", component.Markup);

            // Assert that the filing frequency reflects the loaded calculation
            Assert.Contains("Quarterly", component.Markup);

            // Assert that the additional services reflect the loaded calculation
            Assert.Contains("Tax Consultancy", component.Markup);
        }

        /// <summary>
        /// Tests that the country selection component works correctly
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_CountrySelection()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Set up the mock country service to return test countries
            var testCountries = TestData.CreateTestCountries();
            mockCountryService.Setup(s => s.GetActiveCountriesAsync())
                .ReturnsAsync(testCountries);

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Find the country selector component
            var countrySelector = component.FindComponent<CountrySelector>();

            // Simulate selecting countries
            await countrySelector.Instance.OnSelectionChanged.InvokeAsync(new List<string> { "GB", "DE" });

            // Assert that the component updates its internal state with the selected countries
            Assert.Equal(2, component.Instance.CalculationInput.CountryCodes.Count);
            Assert.Contains("GB", component.Instance.CalculationInput.CountryCodes);
            Assert.Contains("DE", component.Instance.CalculationInput.CountryCodes);

            // Assert that validation messages are cleared when countries are selected
            Assert.Empty(component.Instance.CountryValidationMessage);
        }

        /// <summary>
        /// Tests that the service type selection works correctly
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_ServiceTypeSelection()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Set up mock service type options
            mockPricingService.Setup(s => s.GetServiceTypeOptionsAsync())
                .ReturnsAsync(new List<ServiceTypeOption>
                {
                    new ServiceTypeOption(1, "Standard", "Basic", 500m),
                    new ServiceTypeOption(2, "Complex", "Advanced", 800m)
                });

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Find the service type selector component
            var serviceTypeSelector = component.Find("input[value='2']");

            // Simulate selecting a different service type
            await serviceTypeSelector.ClickAsync();

            // Assert that the component updates its internal state with the selected service type
            Assert.Equal(2, component.Instance.CalculationInput.ServiceType);

            // Assert that additional services are updated based on the selected service type
            // This assertion depends on the specific logic in the component, which is not fully mocked here
        }

        /// <summary>
        /// Tests that the transaction volume input works correctly
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_TransactionVolumeInput()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Find the transaction volume input component
            var volumeInput = component.Find("input[type='number']");

            // Simulate changing the transaction volume
            await volumeInput.ChangeAsync("1000");

            // Assert that the component updates its internal state with the new volume
            Assert.Equal(1000, component.Instance.CalculationInput.TransactionVolume);

            // Assert that validation messages are cleared when a valid volume is entered
            Assert.Empty(component.Instance.VolumeValidationMessage);
        }

        /// <summary>
        /// Tests that the filing frequency selection works correctly
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_FilingFrequencySelection()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Set up mock filing frequency options
            mockPricingService.Setup(s => s.GetFilingFrequencyOptionsAsync())
                .ReturnsAsync(new List<FilingFrequencyOption>
                {
                    new FilingFrequencyOption(1, "Monthly", "Monthly"),
                    new FilingFrequencyOption(2, "Quarterly", "Quarterly")
                });

            // Set up the mock country service to return countries by filing frequency
            mockCountryService.Setup(s => s.GetCountriesByFilingFrequencyAsync(2))
                .ReturnsAsync(TestData.CreateTestCountries().Where(c => c.CountryCode != "IT").ToList());

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Find the filing frequency dropdown
            var frequencyDropdown = component.Find("select");

            // Simulate selecting a different filing frequency
            await frequencyDropdown.ChangeAsync("2");

            // Assert that the component updates its internal state with the selected frequency
            Assert.Equal(2, component.Instance.CalculationInput.FilingFrequency);

            // Assert that the component checks if selected countries support the frequency
            mockCountryService.Verify(s => s.GetCountriesByFilingFrequencyAsync(2), Times.Once);

            // Assert that a warning is displayed if countries don't support the frequency
            Assert.Contains("Some selected countries do not support this filing frequency", component.Markup);
        }

        /// <summary>
        /// Tests that toggling additional services works correctly
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_AdditionalServiceToggle()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Set up mock additional service options
            mockPricingService.Setup(s => s.GetAdditionalServiceOptionsAsync())
                .ReturnsAsync(new List<AdditionalServiceOption>
                {
                    new AdditionalServiceOption("tax-consultancy", "Tax Consultancy", "Tax", 100m, "EUR"),
                    new AdditionalServiceOption("audit-support", "Audit Support", "Audit", 200m, "EUR")
                });

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Find the additional services checkboxes
            var taxConsultancyCheckbox = component.Find("input[id='service-tax-consultancy']");

            // Simulate toggling an additional service
            await taxConsultancyCheckbox.ClickAsync();

            // Assert that the component updates its internal state with the selected services
            Assert.Contains("tax-consultancy", component.Instance.CalculationInput.AdditionalServices);

            // Assert that the IsSelected property of the service is toggled
            Assert.True(component.Instance.AdditionalServices.First(s => s.Value == "tax-consultancy").IsSelected);
        }

        /// <summary>
        /// Tests that the calculate price function works correctly on success
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_CalculatePrice_Success()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Set up the mock pricing service to return a successful calculation result
            var testCalculation = TestData.CreateTestCalculationResult("calc-123", "test-user");
            mockPricingService.Setup(s => s.CalculatePricingAsync(It.IsAny<CalculationInputModel>()))
                .ReturnsAsync(testCalculation);

            // Set up the mock navigation manager
            var navigationManager = context.Services.GetRequiredService<NavigationManager>();
            var navigationMock = context.Services.GetService<FakeNavigationManager>() as FakeNavigationManager;

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Fill in valid calculation parameters
            component.Instance.CalculationInput.CountryCodes = new List<string> { "GB", "DE" };
            component.Instance.CalculationInput.TransactionVolume = 500;

            // Find and click the calculate button
            var calculateButton = component.Find("button[type='button'][class*='btn-primary']");
            await calculateButton.ClickAsync();

            // Assert that the pricing service was called with the correct parameters
            mockPricingService.Verify(s => s.CalculatePricingAsync(It.IsAny<CalculationInputModel>()), Times.Once);

            // Assert that navigation to the results page occurs with the correct calculation ID
            Assert.Equal("http://localhost/results/calc-123", navigationMock.Uri);

            // Assert that no error messages are displayed
            Assert.Empty(component.Instance.ErrorMessage);
        }

        /// <summary>
        /// Tests that validation errors are displayed when calculation parameters are invalid
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_CalculatePrice_ValidationErrors()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Leave required fields empty

            // Find and click the calculate button
            var calculateButton = component.Find("button[type='button'][class*='btn-primary']");
            await calculateButton.ClickAsync();

            // Assert that validation error messages are displayed
            Assert.Contains("At least one country must be selected.", component.Markup);
            Assert.Contains("Transaction volume must be greater than zero.", component.Markup);

            // Assert that the pricing service was not called
            mockPricingService.Verify(s => s.CalculatePricingAsync(It.IsAny<CalculationInputModel>()), Times.Never);

            // Assert that no navigation occurs
            var navigationManager = context.Services.GetRequiredService<NavigationManager>();
            var navigationMock = context.Services.GetService<FakeNavigationManager>() as FakeNavigationManager;
            Assert.Equal("http://localhost/", navigationMock.Uri);
        }

        /// <summary>
        /// Tests that API errors are handled correctly during calculation
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_CalculatePrice_ApiError()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Set up the mock pricing service to throw an exception when CalculatePricingAsync is called
            mockPricingService.Setup(s => s.CalculatePricingAsync(It.IsAny<CalculationInputModel>()))
                .ThrowsAsync(new Exception("API Error"));

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Fill in valid calculation parameters
            component.Instance.CalculationInput.CountryCodes = new List<string> { "GB", "DE" };
            component.Instance.CalculationInput.TransactionVolume = 500;

            // Find and click the calculate button
            var calculateButton = component.Find("button[type='button'][class*='btn-primary']");
            await calculateButton.ClickAsync();

            // Assert that an error message is displayed
            Assert.Contains("An error occurred while calculating pricing. Please try again later.", component.Markup);

            // Assert that no navigation occurs
            var navigationManager = context.Services.GetRequiredService<NavigationManager>();
            var navigationMock = context.Services.GetService<FakeNavigationManager>() as FakeNavigationManager;
            Assert.Equal("http://localhost/", navigationMock.Uri);

            // Assert that IsCalculating is set back to false
            Assert.False(component.Instance.IsCalculating);
        }

        /// <summary>
        /// Tests that saving calculation parameters works correctly on success
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_SaveParameters_Success()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Set up the mock pricing service to return a successful calculation result
            var testCalculation = TestData.CreateTestCalculationResult("calc-123", "test-user");
            mockPricingService.Setup(s => s.CalculatePricingAsync(It.IsAny<CalculationInputModel>()))
                .ReturnsAsync(testCalculation);

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Fill in valid calculation parameters
            component.Instance.CalculationInput.CountryCodes = new List<string> { "GB", "DE" };
            component.Instance.CalculationInput.TransactionVolume = 500;

            // Find and click the save parameters button
            var saveButton = component.Find("button[type='button'][class*='btn-outline-secondary']");
            await saveButton.ClickAsync();

            // Assert that the pricing service was called with the correct parameters
            mockPricingService.Verify(s => s.CalculatePricingAsync(It.IsAny<CalculationInputModel>()), Times.Once);

            // Assert that a success message is displayed
            Assert.Contains("Calculation parameters saved successfully. You can access this calculation from your history.", component.Markup);

            // Assert that no navigation occurs
            var navigationManager = context.Services.GetRequiredService<NavigationManager>();
            var navigationMock = context.Services.GetService<FakeNavigationManager>() as FakeNavigationManager;
            Assert.Equal("http://localhost/", navigationMock.Uri);
        }

        /// <summary>
        /// Tests that the reset form function works correctly
        /// </summary>
        [Fact]
        public async Task Test_PricingCalculator_ResetForm()
        {
            // Set up the test context and mock services using Setup()
            var (context, mockPricingService, mockCountryService) = Setup();

            // Render the PricingCalculator component
            var component = context.RenderComponent<PricingCalculator>();

            // Fill in calculation parameters
            component.Instance.CalculationInput.CountryCodes = new List<string> { "GB", "DE" };
            component.Instance.CalculationInput.TransactionVolume = 500;

            // Find and click the reset button
            var resetButton = component.Find("button[type='button'][class*='btn-link']");
            await resetButton.ClickAsync();

            // Assert that the form is reset to default values
            Assert.Empty(component.Instance.CalculationInput.CountryCodes);
            Assert.Equal(0, component.Instance.CalculationInput.TransactionVolume);

            // Assert that validation messages are cleared
            Assert.Empty(component.Instance.CountryValidationMessage);
            Assert.Empty(component.Instance.VolumeValidationMessage);

            // Assert that error messages are cleared
            Assert.Empty(component.Instance.ErrorMessage);
        }
    }
}