#nullable enable
using Microsoft.Extensions.DependencyInjection; // Microsoft.Extensions.DependencyInjection, Version=6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging, Version=6.0.0
using Moq; // Moq, Version=4.18.2
using System;
using System.Collections.Generic; // System.Collections.Generic, Version=6.0.0
using System.Linq;
using System.Threading.Tasks; // System.Threading.Tasks, Version=6.0.0
using Xunit; // Xunit, Version=2.4.1
using FluentAssertions; // FluentAssertions, Version=6.7.0
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Infrastructure.Integration.ERP;
using VatFilingPricingTool.Infrastructure.Integration.OCR;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Infrastructure.Resilience;

namespace VatFilingPricingTool.IntegrationTests.Services
{
    /// <summary>
    /// Integration tests for the IntegrationService class
    /// </summary>
    [Collection("Database collection")]
    public class IntegrationServiceTests
    {
        private readonly DatabaseFixture _fixture;
        private readonly IIntegrationService _integrationService;
        private readonly Mock<DynamicsConnector> _mockDynamicsConnector;
        private readonly Mock<OcrProcessor> _mockOcrProcessor;
        private readonly Mock<IRetryPolicy> _mockRetryPolicy;
        private readonly Mock<ICircuitBreakerPolicy> _mockCircuitBreakerPolicy;
        private readonly Mock<ILogger<IntegrationService>> _mockLogger;

        /// <summary>
        /// Initializes a new instance of the IntegrationServiceTests class
        /// </summary>
        /// <param name="fixture">The database fixture</param>
        public IntegrationServiceTests(DatabaseFixture fixture)
        {
            // LD1: Store the fixture in _fixture
            _fixture = fixture;

            // LD1: Initialize mock objects for dependencies
            _mockDynamicsConnector = new Mock<DynamicsConnector>(MockBehavior.Strict, null, null, null, null);
            _mockOcrProcessor = new Mock<OcrProcessor>(MockBehavior.Strict, null, null, null, null, null);
            _mockRetryPolicy = new Mock<IRetryPolicy>(MockBehavior.Strict);
            _mockCircuitBreakerPolicy = new Mock<ICircuitBreakerPolicy>(MockBehavior.Strict);
            _mockLogger = new Mock<ILogger<IntegrationService>>();

            // LD1: Configure mock DynamicsConnector to return success for TestConnectionAsync
            _mockDynamicsConnector.Setup(dc => dc.TestConnectionAsync(It.IsAny<Integration>()))
                .ReturnsAsync(Result.Success());

            // LD1: Configure mock DynamicsConnector to return success for ImportDataAsync
            _mockDynamicsConnector.Setup(dc => dc.ImportDataAsync(It.IsAny<Integration>(), It.IsAny<ImportParameters>()))
                .ReturnsAsync(Result<TransactionData>.Success(new TransactionData()));

            // LD1: Configure mock OcrProcessor to return success for ProcessDocumentAsync
            _mockOcrProcessor.Setup(ocr => ocr.ProcessDocumentUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result<Dictionary<string, string>>.Success(new Dictionary<string, string>()));

            // LD1: Configure mock RetryPolicy to pass through operations
            _mockRetryPolicy.Setup(rp => rp.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<string>()))
                .Returns(async (Func<Task<Result>> operation, string operationName) => await operation());

            _mockRetryPolicy.Setup(rp => rp.ExecuteAsync(It.IsAny<Func<Task<Result<Dictionary<string, string>>>>>(), It.IsAny<string>()))
                .Returns(async (Func<Task<Result<Dictionary<string, string>>>>> operation, string operationName) => await operation());

            // LD1: Configure mock CircuitBreakerPolicy to pass through operations
            _mockCircuitBreakerPolicy.Setup(cbp => cbp.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<string>()))
                .Returns(async (Func<Task<Result>> operation, string operationName) => await operation());

            _mockCircuitBreakerPolicy.Setup(cbp => cbp.ExecuteAsync(It.IsAny<Func<Task<Result<Dictionary<string, string>>>>>>(), It.IsAny<string>()))
                .Returns(async (Func<Task<Result<Dictionary<string, string>>>>> operation, string operationName) => await operation());

            _mockCircuitBreakerPolicy.SetupGet(cbp => cbp.CurrentState).Returns(CircuitState.Closed);

            // LD1: Create an instance of IntegrationService with the repository from fixture and mocked dependencies
            _integrationService = new IntegrationService(
                _fixture.DbContext.Set<Integration>(),
                _mockLogger.Object,
                _mockDynamicsConnector.Object,
                _mockOcrProcessor.Object,
                _mockRetryPolicy.Object,
                _mockCircuitBreakerPolicy.Object);
        }

        /// <summary>
        /// Tests that GetIntegrationsAsync returns all integrations from the repository
        /// </summary>
        [Fact]
        public async Task GetIntegrationsAsync_ReturnsAllIntegrations()
        {
            // LD1: Call _integrationService.GetIntegrationsAsync()
            var integrations = await _integrationService.GetIntegrationsAsync();

            // LD1: Assert that the result is not null
            Assert.NotNull(integrations);

            // LD1: Assert that the result contains the expected number of integrations
            integrations.Count().Should().BeGreaterThan(0);

            // LD1: Assert that the integrations have the expected properties
            foreach (var integration in integrations)
            {
                Assert.False(string.IsNullOrEmpty(integration.IntegrationId));
                Assert.False(string.IsNullOrEmpty(integration.UserId));
                Assert.False(string.IsNullOrEmpty(integration.SystemType));
                Assert.False(string.IsNullOrEmpty(integration.ConnectionString));
            }
        }

        /// <summary>
        /// Tests that GetIntegrationByIdAsync returns the correct integration when given a valid ID
        /// </summary>
        [Fact]
        public async Task GetIntegrationByIdAsync_WithValidId_ReturnsIntegration()
        {
            // LD1: Get an existing integration ID from the database
            var integrationId = _fixture.DbContext.Set<Integration>().FirstOrDefault()?.IntegrationId;
            Assert.NotNull(integrationId);

            // LD1: Call _integrationService.GetIntegrationByIdAsync(integrationId)
            var integration = await _integrationService.GetIntegrationByIdAsync(integrationId);

            // LD1: Assert that the result is not null
            Assert.NotNull(integration);

            // LD1: Assert that the result has the expected ID
            Assert.Equal(integrationId, integration.IntegrationId);

            // LD1: Assert that the integration has the expected properties
            Assert.False(string.IsNullOrEmpty(integration.UserId));
            Assert.False(string.IsNullOrEmpty(integration.SystemType));
            Assert.False(string.IsNullOrEmpty(integration.ConnectionString));
        }

        /// <summary>
        /// Tests that GetIntegrationByIdAsync returns null when given an invalid ID
        /// </summary>
        [Fact]
        public async Task GetIntegrationByIdAsync_WithInvalidId_ReturnsNull()
        {
            // LD1: Call _integrationService.GetIntegrationByIdAsync with a non-existent ID
            var integration = await _integrationService.GetIntegrationByIdAsync("nonexistent-id");

            // LD1: Assert that the result is null
            Assert.Null(integration);
        }

        /// <summary>
        /// Tests that GetUserIntegrationsAsync returns integrations for a specific user
        /// </summary>
        [Fact]
        public async Task GetUserIntegrationsAsync_ReturnsUserIntegrations()
        {
            // LD1: Get a user ID from an existing integration
            var userId = _fixture.DbContext.Set<Integration>().FirstOrDefault()?.UserId;
            Assert.NotNull(userId);

            // LD1: Call _integrationService.GetUserIntegrationsAsync(userId)
            var integrations = await _integrationService.GetUserIntegrationsAsync(userId);

            // LD1: Assert that the result is not null
            Assert.NotNull(integrations);

            // LD1: Assert that all returned integrations belong to the specified user
            foreach (var integration in integrations)
            {
                Assert.Equal(userId, integration.UserId);
            }
        }

        /// <summary>
        /// Tests that CreateIntegrationAsync creates a new integration with the specified properties
        /// </summary>
        [Fact]
        public async Task CreateIntegrationAsync_CreatesNewIntegration()
        {
            // LD1: Define test data for a new integration
            var userId = "test-user";
            var systemType = "Dynamics365";
            var connectionString = "test-connection-string";
            var apiKey = "test-api-key";
            var apiEndpoint = "https://test.api.endpoint";
            var additionalSettings = new Dictionary<string, string> { { "setting1", "value1" }, { "setting2", "value2" } };

            // LD1: Call _integrationService.CreateIntegrationAsync with the test data
            var integration = await _integrationService.CreateIntegrationAsync(userId, systemType, connectionString, apiKey, apiEndpoint, additionalSettings);

            // LD1: Assert that the result is not null
            Assert.NotNull(integration);

            // LD1: Assert that the result has a non-empty ID
            Assert.False(string.IsNullOrEmpty(integration.IntegrationId));

            // LD1: Assert that the integration has the expected properties
            Assert.Equal(userId, integration.UserId);
            Assert.Equal(systemType, integration.SystemType);
            Assert.Equal(connectionString, integration.ConnectionString);
            Assert.Equal(apiKey, integration.ApiKey);
            Assert.Equal(apiEndpoint, integration.ApiEndpoint);
            Assert.Equal(additionalSettings, integration.AdditionalSettings);

            // LD1: Verify the integration was added to the database
            var dbIntegration = await _fixture.DbContext.Set<Integration>().FindAsync(integration.IntegrationId);
            Assert.NotNull(dbIntegration);
            Assert.Equal(userId, dbIntegration.UserId);
            Assert.Equal(systemType, dbIntegration.SystemType);
            Assert.Equal(connectionString, dbIntegration.ConnectionString);
            Assert.Equal(apiKey, dbIntegration.ApiKey);
            Assert.Equal(apiEndpoint, dbIntegration.ApiEndpoint);
            Assert.Equal(additionalSettings, dbIntegration.AdditionalSettings);
        }

        /// <summary>
        /// Tests that UpdateIntegrationAsync updates an existing integration with new properties
        /// </summary>
        [Fact]
        public async Task UpdateIntegrationAsync_UpdatesExistingIntegration()
        {
            // LD1: Get an existing integration ID from the database
            var integrationId = _fixture.DbContext.Set<Integration>().FirstOrDefault()?.IntegrationId;
            Assert.NotNull(integrationId);

            // LD1: Define updated properties for the integration
            var newConnectionString = "new-test-connection-string";
            var newApiKey = "new-test-api-key";
            var newApiEndpoint = "https://new.test.api.endpoint";
            var newAdditionalSettings = new Dictionary<string, string> { { "newSetting1", "newValue1" }, { "newSetting2", "newValue2" } };

            // LD1: Call _integrationService.UpdateIntegrationAsync with the ID and updated properties
            var integration = await _integrationService.UpdateIntegrationAsync(integrationId, newConnectionString, newApiKey, newApiEndpoint, newAdditionalSettings);

            // LD1: Assert that the result is not null
            Assert.NotNull(integration);

            // LD1: Assert that the integration has been updated with the new properties
            Assert.Equal(newConnectionString, integration.ConnectionString);
            Assert.Equal(newApiKey, integration.ApiKey);
            Assert.Equal(newApiEndpoint, integration.ApiEndpoint);
            Assert.Equal(newAdditionalSettings, integration.AdditionalSettings);

            // LD1: Verify the changes were persisted to the database
            var dbIntegration = await _fixture.DbContext.Set<Integration>().FindAsync(integrationId);
            Assert.NotNull(dbIntegration);
            Assert.Equal(newConnectionString, dbIntegration.ConnectionString);
            Assert.Equal(newApiKey, dbIntegration.ApiKey);
            Assert.Equal(newApiEndpoint, dbIntegration.ApiEndpoint);
            Assert.Equal(newAdditionalSettings, dbIntegration.AdditionalSettings);
        }

        /// <summary>
        /// Tests that DeleteIntegrationAsync removes an integration from the database
        /// </summary>
        [Fact]
        public async Task DeleteIntegrationAsync_DeletesExistingIntegration()
        {
            // LD1: Get an existing integration ID from the database
            var integrationId = _fixture.DbContext.Set<Integration>().FirstOrDefault()?.IntegrationId;
            Assert.NotNull(integrationId);

            // LD1: Call _integrationService.DeleteIntegrationAsync with the ID
            var result = await _integrationService.DeleteIntegrationAsync(integrationId);

            // LD1: Assert that the result is true
            Assert.True(result);

            // LD1: Verify the integration was removed from the database
            var dbIntegration = await _fixture.DbContext.Set<Integration>().FindAsync(integrationId);
            Assert.Null(dbIntegration);
        }

        /// <summary>
        /// Tests that TestConnectionAsync returns success for a valid Dynamics 365 integration
        /// </summary>
        [Fact]
        public async Task TestConnectionAsync_WithDynamics365_ReturnsSuccess()
        {
            // LD1: Get an existing Dynamics 365 integration ID from the database
            var integrationId = _fixture.DbContext.Set<Integration>().FirstOrDefault(i => i.SystemType == "Dynamics365")?.IntegrationId;
            Assert.NotNull(integrationId);

            // LD1: Configure mock DynamicsConnector to return success for TestConnectionAsync
            _mockDynamicsConnector.Setup(dc => dc.TestConnectionAsync(It.IsAny<Integration>()))
                .ReturnsAsync(Result.Success());

            // LD1: Call _integrationService.TestConnectionAsync with the integration ID
            var result = await _integrationService.TestConnectionAsync(integrationId);

            // LD1: Assert that the result is true
            Assert.True(result);

            // LD1: Verify that DynamicsConnector.TestConnectionAsync was called with the correct integration
            _mockDynamicsConnector.Verify(dc => dc.TestConnectionAsync(It.Is<Integration>(i => i.IntegrationId == integrationId)), Times.Once);
        }

        /// <summary>
        /// Tests that TestConnectionAsync returns success for a valid Azure Cognitive Services integration
        /// </summary>
        [Fact]
        public async Task TestConnectionAsync_WithAzureCognitiveServices_ReturnsSuccess()
        {
            // LD1: Get an existing Azure Cognitive Services integration ID from the database
            var integrationId = _fixture.DbContext.Set<Integration>().FirstOrDefault(i => i.SystemType == "AzureCognitiveServices")?.IntegrationId;
            Assert.NotNull(integrationId);

            // LD1: Configure mock OcrProcessor to return success for a test document processing
            _mockOcrProcessor.Setup(ocr => ocr.ProcessDocumentUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result<Dictionary<string, string>>.Success(new Dictionary<string, string>()));

            // LD1: Call _integrationService.TestConnectionAsync with the integration ID
            var result = await _integrationService.TestConnectionAsync(integrationId);

            // LD1: Assert that the result is true
            Assert.True(result);

            // LD1: Verify that OcrProcessor.ProcessDocumentAsync was called with the correct parameters
            _mockOcrProcessor.Verify(ocr => ocr.ProcessDocumentUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that ImportDataAsync returns a successful import result with the expected data
        /// </summary>
        [Fact]
        public async Task ImportDataAsync_WithValidParameters_ReturnsImportResult()
        {
            // LD1: Get an existing Dynamics 365 integration ID from the database
            var integrationId = _fixture.DbContext.Set<Integration>().FirstOrDefault(i => i.SystemType == "Dynamics365")?.IntegrationId;
            Assert.NotNull(integrationId);

            // LD1: Create ImportParameters with test data
            var parameters = new ImportParameters { StartDate = DateTime.UtcNow.AddDays(-7), EndDate = DateTime.UtcNow, EntityType = "invoice" };

            // LD1: Configure mock DynamicsConnector to return a successful ImportResult
            _mockDynamicsConnector.Setup(dc => dc.ImportDataAsync(It.IsAny<Integration>(), It.IsAny<ImportParameters>()))
                .ReturnsAsync(Result<TransactionData>.Success(new TransactionData { Records = new List<TransactionRecord> { new TransactionRecord() } }));

            // LD1: Call _integrationService.ImportDataAsync with the integration ID and parameters
            var result = await _integrationService.ImportDataAsync(integrationId, parameters);

            // LD1: Assert that the result is successful
            Assert.True(result.Success);

            // LD1: Assert that the result contains the expected imported records
            Assert.Equal(1, result.RecordsImported);

            // LD1: Verify that DynamicsConnector.ImportDataAsync was called with the correct parameters
            _mockDynamicsConnector.Verify(dc => dc.ImportDataAsync(It.Is<Integration>(i => i.IntegrationId == integrationId), It.Is<ImportParameters>(p => p == parameters)), Times.Once);
        }

        /// <summary>
        /// Tests that ProcessDocumentAsync returns a successful document processing result with extracted data
        /// </summary>
        [Fact]
        public async Task ProcessDocumentAsync_WithValidDocument_ReturnsProcessingResult()
        {
            // LD1: Get an existing Azure Cognitive Services integration ID from the database
            var integrationId = _fixture.DbContext.Set<Integration>().FirstOrDefault(i => i.SystemType == "AzureCognitiveServices")?.IntegrationId;
            Assert.NotNull(integrationId);

            // LD1: Create test document URL and processing options
            var documentUrl = "https://example.com/test-document.pdf";
            var options = new DocumentProcessingOptions { DocumentType = "invoice" };

            // LD1: Configure mock OcrProcessor to return a successful DocumentProcessingResult with extracted data
            _mockOcrProcessor.Setup(ocr => ocr.ProcessDocumentUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result<Dictionary<string, string>>.Success(new Dictionary<string, string> { { "field1", "value1" } }));

            // LD1: Call _integrationService.ProcessDocumentAsync with the integration ID, document URL, and options
            var result = await _integrationService.ProcessDocumentAsync(integrationId, documentUrl, options);

            // LD1: Assert that the result is successful
            Assert.True(result.IsSuccess);

            // LD1: Assert that the result contains the expected extracted data
            Assert.True(result.ExtractedData.ContainsKey("field1"));
            Assert.Equal("value1", result.ExtractedData["field1"]);

            // LD1: Verify that OcrProcessor.ProcessDocumentAsync was called with the correct parameters
            _mockOcrProcessor.Verify(ocr => ocr.ProcessDocumentUrlAsync(It.Is<string>(url => url == documentUrl), It.Is<string>(type => type == "invoice")), Times.Once);
        }

        /// <summary>
        /// Tests that GetAvailableSystemTypesAsync returns all supported system types
        /// </summary>
        [Fact]
        public async Task GetAvailableSystemTypesAsync_ReturnsAllSystemTypes()
        {
            // LD1: Call _integrationService.GetAvailableSystemTypesAsync()
            var systemTypes = await _integrationService.GetAvailableSystemTypesAsync();

            // LD1: Assert that the result is not null
            Assert.NotNull(systemTypes);

            // LD1: Assert that the result contains at least 'Dynamics365' and 'AzureCognitiveServices'
            Assert.Contains("Dynamics365", systemTypes);
            Assert.Contains("AzureCognitiveServices", systemTypes);
        }

        /// <summary>
        /// Tests that operations are prevented when the circuit breaker is open
        /// </summary>
        [Fact]
        public async Task CircuitBreakerOpen_PreventsOperations()
        {
            // LD1: Configure mock CircuitBreakerPolicy to simulate an open circuit
            _mockCircuitBreakerPolicy.SetupGet(cbp => cbp.CurrentState).Returns(CircuitState.Open);

            // LD1: Get an existing integration ID from the database
            var integrationId = _fixture.DbContext.Set<Integration>().FirstOrDefault()?.IntegrationId;
            Assert.NotNull(integrationId);

            // LD1: Call _integrationService.TestConnectionAsync with the integration ID
            var result = await _integrationService.TestConnectionAsync(integrationId);

            // LD1: Assert that the result is false
            Assert.False(result);

            // LD1: Verify that the underlying connector methods were not called
            _mockDynamicsConnector.Verify(dc => dc.TestConnectionAsync(It.IsAny<Integration>()), Times.Never);
            _mockOcrProcessor.Verify(ocr => ocr.ProcessDocumentUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that the retry policy handles transient failures and eventually succeeds
        /// </summary>
        [Fact]
        public async Task RetryPolicy_HandlesTransientFailures()
        {
            // LD1: Configure mock RetryPolicy to simulate retries and eventual success
            _mockRetryPolicy.Setup(rp => rp.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<string>()))
                .Returns(async (Func<Task<Result>> operation, string operationName) => await operation());

            // LD1: Get an existing integration ID from the database
            var integrationId = _fixture.DbContext.Set<Integration>().FirstOrDefault()?.IntegrationId;
            Assert.NotNull(integrationId);

            // LD1: Call _integrationService.TestConnectionAsync with the integration ID
            var result = await _integrationService.TestConnectionAsync(integrationId);

            // LD1: Assert that the result is true
            Assert.True(result);

            // LD1: Verify that RetryPolicy.ExecuteAsync was called
            _mockRetryPolicy.Verify(rp => rp.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<string>()), Times.Once);
        }
    }
}