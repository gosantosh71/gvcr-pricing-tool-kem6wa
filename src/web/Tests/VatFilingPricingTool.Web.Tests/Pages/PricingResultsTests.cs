using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit; // bunit version 1.12.6
using FluentAssertions; // FluentAssertions version 6.7.0
using Microsoft.AspNetCore.Components; // Microsoft.AspNetCore.Components version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging version 6.0.0
using Microsoft.JSInterop; // Microsoft.AspNetCore.Components version 6.0.0
using Moq; // Moq version 4.18.2
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Pages;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Tests.Helpers;
using Xunit; // xunit version 2.4.2

namespace VatFilingPricingTool.Web.Tests.Pages
{
    /// <summary>
    /// Test class for the PricingResults page component
    /// </summary>
    public class PricingResultsTests
    {
        private readonly string TestCalculationId = "calc-123";
        private readonly string TestUserId = "user-123";
        private readonly string TestReportId = "report-123";

        /// <summary>
        /// Helper method to set up the test context with authenticated user and mocked services
        /// </summary>
        /// <returns>Configured test context and mocked services</returns>
        private Tuple<TestContext, Mock<IPricingService>, Mock<IReportService>, Mock<NavigationManager>, Mock<IJSRuntime>> Setup_TestContext()
        {
            // Create test context
            var testContext = RenderComponent.CreateTestContext();
            
            // Set up authenticated user
            RenderComponent.SetupAuthenticatedUser(testContext, TestUserId, "user@example.com");
            
            // Create mocks for required services
            var mockPricingService = RenderComponent.CreateMockService<IPricingService>(testContext);
            var mockReportService = RenderComponent.CreateMockService<IReportService>(testContext);
            
            // Create mock for NavigationManager
            var mockNavigationManager = new Mock<NavigationManager>();
            testContext.Services.AddSingleton(mockNavigationManager.Object);
            
            // Create mock for IJSRuntime
            var mockJSRuntime = new Mock<IJSRuntime>();
            testContext.Services.AddSingleton(mockJSRuntime.Object);
            
            return new Tuple<TestContext, Mock<IPricingService>, Mock<IReportService>, Mock<NavigationManager>, Mock<IJSRuntime>>(
                testContext, mockPricingService, mockReportService, mockNavigationManager, mockJSRuntime);
        }

        /// <summary>
        /// Tests that the PricingResults page loads a calculation successfully
        /// </summary>
        [Fact]
        public void Test_PricingResults_LoadsCalculation_Successfully()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, _) = Setup_TestContext();
            
            // Create test calculation result
            var testCalculation = TestData.CreateTestCalculationResult(TestCalculationId, TestUserId);
            
            // Set up mock pricing service to return the test calculation
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ReturnsAsync(testCalculation);
            
            // Set up mock report service to return test report formats
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>
                {
                    new ReportFormatOption(0, "PDF", "fas fa-file-pdf"),
                    new ReportFormatOption(1, "Excel", "fas fa-file-excel")
                });
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Assert
            component.Markup.Should().Contain(testCalculation.FormattedTotalCost);
            component.Markup.Should().Contain("United Kingdom");
            component.Markup.Should().Contain("Germany");
            component.Markup.Should().Contain("France");
            
            // Verify not in loading or error state
            component.FindAll(".spinner-border").Should().BeEmpty();
            component.FindAll("[role='alert']").Should().BeEmpty();
        }

        /// <summary>
        /// Tests that the PricingResults page shows loading state while fetching data
        /// </summary>
        [Fact]
        public void Test_PricingResults_ShowsLoading_State()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, _) = Setup_TestContext();
            
            // We won't complete the task to simulate loading state
            var tcs = new TaskCompletionSource<CalculationResultModel>();
            
            // Set up mock pricing service to return an incomplete task to show loading state
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .Returns(tcs.Task);
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>());
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Assert - should be in loading state
            component.Markup.Should().Contain("Loading calculation results");
            component.FindAll(".spinner-border").Should().NotBeEmpty();
            component.FindAll(".loading-container").Should().NotBeEmpty();
            
            // We're not completing the task in this test to keep the component in loading state
        }

        /// <summary>
        /// Tests that the PricingResults page handles errors when calculation is not found
        /// </summary>
        [Fact]
        public void Test_PricingResults_HandlesError_WhenCalculationNotFound()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, _) = Setup_TestContext();
            
            // Set up mock pricing service to return null for the calculation
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ReturnsAsync((CalculationResultModel)null);
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>());
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Assert
            component.Markup.Should().Contain("Calculation not found");
            component.FindAll("[role='alert']").Should().NotBeEmpty();
            component.FindAll(".spinner-border").Should().BeEmpty(); // Not loading anymore
        }

        /// <summary>
        /// Tests that the PricingResults page handles errors when service throws an exception
        /// </summary>
        [Fact]
        public void Test_PricingResults_HandlesError_WhenServiceThrowsException()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, _) = Setup_TestContext();
            
            // Set up mock pricing service to throw an exception
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ThrowsAsync(new Exception("Test exception"));
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>());
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Assert
            component.Markup.Should().Contain("An error occurred");
            component.FindAll("[role='alert']").Should().NotBeEmpty();
            component.FindAll(".spinner-border").Should().BeEmpty(); // Not loading anymore
        }

        /// <summary>
        /// Tests that the PricingResults page can save an estimate successfully
        /// </summary>
        [Fact]
        public async Task Test_PricingResults_SaveEstimate_Successfully()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, _) = Setup_TestContext();
            
            // Create test calculation result
            var testCalculation = TestData.CreateTestCalculationResult(TestCalculationId, TestUserId);
            
            // Set up mock pricing service
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ReturnsAsync(testCalculation);
            
            mockPricingService.Setup(s => s.SaveCalculationAsync(It.IsAny<SaveCalculationModel>()))
                .ReturnsAsync(true);
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>());
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Click the save options button
            var saveButton = component.Find("button:contains('Save Estimate')");
            saveButton.Click();
            
            // Fill in the save form
            var nameInput = component.Find("#calculationName");
            var descriptionInput = component.Find("#calculationDescription");
            nameInput.Change("Test Calculation");
            descriptionInput.Change("Test Description");
            
            // Click the save button
            var submitButton = component.Find("button:contains('Save'):not(:contains('Estimate'))");
            await submitButton.ClickAsync();
            
            // Assert
            mockPricingService.Verify(s => s.SaveCalculationAsync(It.Is<SaveCalculationModel>(m => 
                m.CalculationId == TestCalculationId && 
                m.Name == "Test Calculation" && 
                m.Description == "Test Description")), Times.Once);
            
            // Success message should be displayed
            component.Markup.Should().Contain("Calculation saved successfully");
        }

        /// <summary>
        /// Tests that the PricingResults page validates save estimate form
        /// </summary>
        [Fact]
        public async Task Test_PricingResults_SaveEstimate_ValidationError()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, _) = Setup_TestContext();
            
            // Create test calculation result
            var testCalculation = TestData.CreateTestCalculationResult(TestCalculationId, TestUserId);
            
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ReturnsAsync(testCalculation);
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>());
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Click the save options button
            var saveButton = component.Find("button:contains('Save Estimate')");
            saveButton.Click();
            
            // Leave the name empty (which is required)
            var descriptionInput = component.Find("#calculationDescription");
            descriptionInput.Change("Test Description");
            
            // Click the save button
            var submitButton = component.Find("button:contains('Save'):not(:contains('Estimate'))");
            await submitButton.ClickAsync();
            
            // Assert
            mockPricingService.Verify(s => s.SaveCalculationAsync(It.IsAny<SaveCalculationModel>()), Times.Never);
            
            // Validation error message should be displayed
            component.Markup.Should().Contain("Please enter a name");
        }

        /// <summary>
        /// Tests that the PricingResults page can generate a report successfully
        /// </summary>
        [Fact]
        public async Task Test_PricingResults_GenerateReport_Successfully()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, _) = Setup_TestContext();
            
            // Create test calculation result
            var testCalculation = TestData.CreateTestCalculationResult(TestCalculationId, TestUserId);
            
            // Create test report
            var testReport = TestData.CreateTestReport(TestReportId, TestCalculationId, TestUserId);
            
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ReturnsAsync(testCalculation);
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>
                {
                    new ReportFormatOption(0, "PDF", "fas fa-file-pdf"),
                    new ReportFormatOption(1, "Excel", "fas fa-file-excel")
                });
            
            mockReportService.Setup(s => s.GenerateReportAsync(It.IsAny<ReportRequestModel>()))
                .ReturnsAsync(testReport);
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Click the report options button
            var reportButton = component.Find("button:contains('Generate Report')");
            reportButton.Click();
            
            // Fill in the report form
            var titleInput = component.Find("#reportTitle");
            titleInput.Change("Test Report");
            
            // Select PDF format (should be selected by default, but making it explicit)
            var pdfFormat = component.Find("input[name='reportFormat'][value='0']");
            pdfFormat.Change(true);
            
            // Click the generate button
            var generateButton = component.Find("button:contains('Generate'):not(:contains('Report'))");
            await generateButton.ClickAsync();
            
            // Assert
            mockReportService.Verify(s => s.GenerateReportAsync(It.Is<ReportRequestModel>(r => 
                r.CalculationId == TestCalculationId &&
                r.ReportTitle == "Test Report" &&
                r.Format == 0)), Times.Once);
            
            // Success message should be displayed
            component.Markup.Should().Contain("Report generated successfully");
        }

        /// <summary>
        /// Tests that the PricingResults page validates report generation form
        /// </summary>
        [Fact]
        public async Task Test_PricingResults_GenerateReport_ValidationError()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, _) = Setup_TestContext();
            
            // Create test calculation result
            var testCalculation = TestData.CreateTestCalculationResult(TestCalculationId, TestUserId);
            
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ReturnsAsync(testCalculation);
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>
                {
                    new ReportFormatOption(0, "PDF", "fas fa-file-pdf"),
                    new ReportFormatOption(1, "Excel", "fas fa-file-excel")
                });
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Click the report options button
            var reportButton = component.Find("button:contains('Generate Report')");
            reportButton.Click();
            
            // Leave the title empty (which is required)
            // Clear the title field
            var titleInput = component.Find("#reportTitle");
            titleInput.Change("");
            
            // Click the generate button
            var generateButton = component.Find("button:contains('Generate'):not(:contains('Report'))");
            await generateButton.ClickAsync();
            
            // Assert
            mockReportService.Verify(s => s.GenerateReportAsync(It.IsAny<ReportRequestModel>()), Times.Never);
            
            // Validation error message should be displayed
            component.Markup.Should().Contain("Please enter a title");
        }

        /// <summary>
        /// Tests that the PricingResults page can download a PDF report successfully
        /// </summary>
        [Fact]
        public async Task Test_PricingResults_DownloadPdf_Successfully()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, mockJSRuntime) = Setup_TestContext();
            
            // Create test data
            var testCalculation = TestData.CreateTestCalculationResult(TestCalculationId, TestUserId);
            var testReport = TestData.CreateTestReport(TestReportId, TestCalculationId, TestUserId);
            
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ReturnsAsync(testCalculation);
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>());
            
            mockReportService.Setup(s => s.GenerateReportAsync(It.IsAny<ReportRequestModel>()))
                .ReturnsAsync(testReport);
            
            mockReportService.Setup(s => s.DownloadReportAsync(TestReportId, 0))
                .ReturnsAsync("https://example.com/reports/" + TestReportId + ".pdf");
            
            // Set up JS runtime for window.open call
            mockJSRuntime.Setup(j => j.InvokeVoidAsync("window.open", It.IsAny<string>(), "_blank"))
                .Returns(ValueTask.CompletedTask);
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Click the download PDF button
            var downloadButton = component.Find("button:contains('Download PDF')");
            await downloadButton.ClickAsync();
            
            // Assert
            mockReportService.Verify(s => s.GenerateReportAsync(It.Is<ReportRequestModel>(r => 
                r.CalculationId == TestCalculationId &&
                r.Format == 0)), Times.Once);
            
            mockReportService.Verify(s => s.DownloadReportAsync(TestReportId, 0), Times.Once);
            
            mockJSRuntime.Verify(j => j.InvokeVoidAsync("window.open", It.IsAny<string>(), "_blank"), Times.Once);
        }

        /// <summary>
        /// Tests that the PricingResults page navigates to calculator when edit inputs is clicked
        /// </summary>
        [Fact]
        public void Test_PricingResults_EditInputs_NavigatesToCalculator()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, mockNavigationManager, _) = Setup_TestContext();
            
            // Create test calculation result
            var testCalculation = TestData.CreateTestCalculationResult(TestCalculationId, TestUserId);
            
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ReturnsAsync(testCalculation);
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>());
            
            // Set up NavigationManager mock
            mockNavigationManager.Setup(n => n.NavigateTo(It.IsAny<string>(), It.IsAny<bool>()));
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Click the edit inputs button
            var editButton = component.Find("button:contains('Edit Inputs')");
            editButton.Click();
            
            // Assert
            mockNavigationManager.Verify(n => n.NavigateTo($"/calculator/{TestCalculationId}", false), Times.Once);
        }

        /// <summary>
        /// Tests that the PricingResults page can toggle between table and chart views
        /// </summary>
        [Fact]
        public void Test_PricingResults_ToggleChartView_SwitchesBetweenTableAndChart()
        {
            // Arrange
            var (testContext, mockPricingService, mockReportService, _, _) = Setup_TestContext();
            
            // Create test calculation result
            var testCalculation = TestData.CreateTestCalculationResult(TestCalculationId, TestUserId);
            
            mockPricingService.Setup(s => s.GetCalculationAsync(TestCalculationId))
                .ReturnsAsync(testCalculation);
            
            mockReportService.Setup(s => s.GetReportFormatsAsync())
                .ReturnsAsync(new List<ReportFormatOption>());
            
            // Act
            var component = testContext.RenderComponent<PricingResults>(parameters => 
                parameters.Add(p => p.CalculationId, TestCalculationId));
            
            // Assert - Table view should be shown by default
            component.FindAll("table").Should().NotBeEmpty();
            
            // Find and click the toggle chart view button
            var chartButton = component.Find("button:contains('Chart View')");
            chartButton.Click();
            
            // Assert - Chart view should be shown now (PricingChart component should be rendered)
            // The actual assertion depends on how PricingChart is rendered in the component
            component.FindAll("table").Should().BeEmpty(); // Tables should be hidden in chart view
            
            // Click the toggle chart view button again
            var tableButton = component.Find("button:contains('Table View')");
            tableButton.Click();
            
            // Assert - Table view should be shown again
            component.FindAll("table").Should().NotBeEmpty();
        }
    }
}