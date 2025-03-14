#nullable enable
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq;
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Moq; // Moq package version 4.18.2
using Xunit; // Xunit package version 2.4.2
using FluentAssertions; // FluentAssertions package version 6.7.0
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.Service.Implementations;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Infrastructure.Integration.ERP;
using VatFilingPricingTool.Infrastructure.Integration.OCR;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Infrastructure.Resilience;
using static VatFilingPricingTool.Infrastructure.Resilience.CircuitState;
using VatFilingPricingTool.UnitTests.Helpers;

namespace VatFilingPricingTool.UnitTests.Services
{
    [public: System.ComponentModel.DescriptionAttribute("Contains unit tests for the IntegrationService class")]
    public class IntegrationServiceTests
    {
        private readonly Mock<IIntegrationRepository> _mockRepository;
        private readonly Mock<ILogger<IntegrationService>> _mockLogger;
        private readonly Mock<DynamicsConnector> _mockDynamicsConnector;
        private readonly Mock<OcrProcessor> _mockOcrProcessor;
        private readonly Mock<IRetryPolicy> _mockRetryPolicy;
        private readonly Mock<ICircuitBreakerPolicy> _mockCircuitBreakerPolicy;
        private readonly IIntegrationService _integrationService;

        [public: System.ComponentModel.DescriptionAttribute("Initializes a new instance of the IntegrationServiceTests class")]
        public IntegrationServiceTests()
        {
            // Initialize mocks
            _mockRepository = new Mock<IIntegrationRepository>();
            _mockLogger = new Mock<ILogger<IntegrationService>>();
            _mockDynamicsConnector = new Mock<DynamicsConnector>(_mockLogger.Object, null!, null!, null!);
            _mockOcrProcessor = new Mock<OcrProcessor>(null!, null!, null!, null!, null!, null!);
            _mockRetryPolicy = new Mock<IRetryPolicy>();
            _mockCircuitBreakerPolicy = new Mock<ICircuitBreakerPolicy>();

            // Setup default mock behavior
            _mockCircuitBreakerPolicy.Setup(x => x.CurrentState).Returns(Closed);
            _mockRetryPolicy.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Success());
            _mockRetryPolicy.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result<Dictionary<string, string>>>>>(), It.IsAny<string>()))
                .ReturnsAsync(Result<Dictionary<string, string>>.Success(new Dictionary<string, string>()));

            // Initialize IntegrationService with mocked dependencies
            _integrationService = new IntegrationService(
                _mockRepository.Object,
                _mockLogger.Object,
                _mockDynamicsConnector.Object,
                _mockOcrProcessor.Object,
                _mockRetryPolicy.Object,
                _mockCircuitBreakerPolicy.Object);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that GetIntegrationsAsync returns all integrations from the repository")]
        [Fact]
        public async Task GetIntegrationsAsync_ShouldReturnAllIntegrations()
        {
            // Arrange
            var integrations = new List<Integration>
            {
                Integration.Create(TestHelpers.GetRandomUserId(), "Dynamics365", "conn1"),
                Integration.Create(TestHelpers.GetRandomUserId(), "AzureCognitiveServices", "conn2")
            };
            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(integrations);

            // Act
            var result = await _integrationService.GetIntegrationsAsync();

            // Assert
            result.Should().BeEquivalentTo(integrations);
            _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that GetIntegrationByIdAsync returns the correct integration when given a valid ID")]
        [Fact]
        public async Task GetIntegrationByIdAsync_WithValidId_ShouldReturnIntegration()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            var integration = Integration.Create(TestHelpers.GetRandomUserId(), "Dynamics365", "conn1");
            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync(integration);

            // Act
            var result = await _integrationService.GetIntegrationByIdAsync(integrationId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(integration);
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that GetIntegrationByIdAsync returns null when given an invalid ID")]
        [Fact]
        public async Task GetIntegrationByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync((Integration)null!);

            // Act
            var result = await _integrationService.GetIntegrationByIdAsync(integrationId);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that GetUserIntegrationsAsync returns integrations for a specific user")]
        [Fact]
        public async Task GetUserIntegrationsAsync_ShouldReturnUserIntegrations()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var integrations = new List<Integration>
            {
                Integration.Create(userId, "Dynamics365", "conn1"),
                Integration.Create(userId, "AzureCognitiveServices", "conn2")
            };
            _mockRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(integrations);

            // Act
            var result = await _integrationService.GetUserIntegrationsAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(integrations);
            _mockRepository.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that CreateIntegrationAsync creates and returns a new integration with valid data")]
        [Fact]
        public async Task CreateIntegrationAsync_WithValidData_ShouldCreateAndReturnIntegration()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var systemType = "Dynamics365";
            var connectionString = "testConnString";
            var apiKey = "testApiKey";
            var apiEndpoint = "https://test.api.endpoint";
            var additionalSettings = new Dictionary<string, string> { { "setting1", "value1" } };

            Integration? capturedIntegration = null;
            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Integration>()))
                .ReturnsAsync((Integration i) =>
                {
                    capturedIntegration = i;
                    return i;
                });

            // Act
            var result = await _integrationService.CreateIntegrationAsync(userId, systemType, connectionString, apiKey, apiEndpoint, additionalSettings);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.SystemType.Should().Be(systemType);
            result.ConnectionString.Should().Be(connectionString);
            result.ApiKey.Should().Be(apiKey);
            result.ApiEndpoint.Should().Be(apiEndpoint);
            result.AdditionalSettings.Should().BeEquivalentTo(additionalSettings);

            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Integration>()), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that UpdateIntegrationAsync updates and returns an existing integration with valid data")]
        [Fact]
        public async Task UpdateIntegrationAsync_WithValidData_ShouldUpdateAndReturnIntegration()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            var existingIntegration = Integration.Create(TestHelpers.GetRandomUserId(), "Dynamics365", "oldConnString");
            var newConnectionString = "newConnString";
            var newApiKey = "newApiKey";
            var newApiEndpoint = "https://new.api.endpoint";
            var newAdditionalSettings = new Dictionary<string, string> { { "setting1", "newValue1" } };

            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync(existingIntegration);
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Integration>())).ReturnsAsync(existingIntegration);

            // Act
            var result = await _integrationService.UpdateIntegrationAsync(integrationId, newConnectionString, newApiKey, newApiEndpoint, newAdditionalSettings);

            // Assert
            result.Should().NotBeNull();
            result.ConnectionString.Should().Be(newConnectionString);
            result.ApiKey.Should().Be(newApiKey);
            result.ApiEndpoint.Should().Be(newApiEndpoint);
            result.AdditionalSettings.Should().BeEquivalentTo(newAdditionalSettings);

            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Integration>()), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that UpdateIntegrationAsync returns null when given an invalid integration ID")]
        [Fact]
        public async Task UpdateIntegrationAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync((Integration)null!);

            // Act
            var result = await _integrationService.UpdateIntegrationAsync(integrationId, "newConnString", "newApiKey", "https://new.api.endpoint", new Dictionary<string, string>());

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Integration>()), Times.Never);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that DeleteIntegrationAsync returns true when deleting an existing integration")]
        [Fact]
        public async Task DeleteIntegrationAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            _mockRepository.Setup(repo => repo.DeleteAsync(integrationId)).ReturnsAsync(true);

            // Act
            var result = await _integrationService.DeleteIntegrationAsync(integrationId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(repo => repo.DeleteAsync(integrationId), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that DeleteIntegrationAsync returns false when trying to delete a non-existent integration")]
        [Fact]
        public async Task DeleteIntegrationAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            _mockRepository.Setup(repo => repo.DeleteAsync(integrationId)).ReturnsAsync(false);

            // Act
            var result = await _integrationService.DeleteIntegrationAsync(integrationId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(repo => repo.DeleteAsync(integrationId), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that TestConnectionAsync returns success for a valid Dynamics 365 integration")]
        [Fact]
        public async Task TestConnectionAsync_WithDynamics365_ShouldReturnSuccess()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            var integration = Integration.Create(TestHelpers.GetRandomUserId(), "Dynamics365", "conn1");

            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync(integration);
            _mockCircuitBreakerPolicy.Setup(x => x.CurrentState).Returns(Closed);
            _mockCircuitBreakerPolicy.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Success());
            _mockDynamicsConnector.Setup(conn => conn.TestConnectionAsync(integration)).ReturnsAsync(Result.Success());

            // Act
            var result = await _integrationService.TestConnectionAsync(integrationId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
            _mockCircuitBreakerPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<string>()), Times.Once);
            _mockDynamicsConnector.Verify(conn => conn.TestConnectionAsync(integration), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that TestConnectionAsync returns false when the circuit breaker is open")]
        [Fact]
        public async Task TestConnectionAsync_WithOpenCircuit_ShouldReturnFalse()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            var integration = Integration.Create(TestHelpers.GetRandomUserId(), "Dynamics365", "conn1");

            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync(integration);
            _mockCircuitBreakerPolicy.Setup(x => x.CurrentState).Returns(Open);

            // Act
            var result = await _integrationService.TestConnectionAsync(integrationId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
            _mockCircuitBreakerPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<string>()), Times.Never);
            _mockDynamicsConnector.Verify(conn => conn.TestConnectionAsync(integration), Times.Never);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that ImportDataAsync returns a successful result with valid parameters")]
        [Fact]
        public async Task ImportDataAsync_WithValidParameters_ShouldReturnSuccessResult()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            var integration = Integration.Create(TestHelpers.GetRandomUserId(), "Dynamics365", "conn1");
            var importParameters = new ImportParameters { StartDate = DateTime.Now.AddDays(-7), EndDate = DateTime.Now, EntityType = "invoice" };
            var transactionData = new TransactionData { EntityType = "invoice", RetrievalDate = DateTime.Now };

            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync(integration);
            _mockCircuitBreakerPolicy.Setup(x => x.CurrentState).Returns(Closed);
            _mockCircuitBreakerPolicy.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result<TransactionData>>>>(), It.IsAny<string>()))
                .ReturnsAsync(Result<TransactionData>.Success(transactionData));
            _mockDynamicsConnector.Setup(conn => conn.ImportDataAsync(integration, importParameters)).ReturnsAsync(Result<TransactionData>.Success(transactionData));
            _mockRepository.Setup(repo => repo.UpdateLastSyncDateAsync(integrationId)).ReturnsAsync(true);

            // Act
            var result = await _integrationService.ImportDataAsync(integrationId, importParameters);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
            _mockCircuitBreakerPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result<TransactionData>>>>(), It.IsAny<string>()), Times.Once);
            _mockDynamicsConnector.Verify(conn => conn.ImportDataAsync(integration, importParameters), Times.Once);
            _mockRepository.Verify(repo => repo.UpdateLastSyncDateAsync(integrationId), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that ImportDataAsync returns a failure result with an invalid integration ID")]
        [Fact]
        public async Task ImportDataAsync_WithInvalidIntegrationId_ShouldReturnFailureResult()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            var importParameters = new ImportParameters { StartDate = DateTime.Now.AddDays(-7), EndDate = DateTime.Now, EntityType = "invoice" };

            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync((Integration)null!);

            // Act
            var result = await _integrationService.ImportDataAsync(integrationId, importParameters);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
            _mockCircuitBreakerPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result<TransactionData>>>>(), It.IsAny<string>()), Times.Never);
            _mockDynamicsConnector.Verify(conn => conn.ImportDataAsync(It.IsAny<Integration>(), It.IsAny<ImportParameters>()), Times.Never);
            _mockRepository.Verify(repo => repo.UpdateLastSyncDateAsync(It.IsAny<string>()), Times.Never);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that ProcessDocumentAsync returns a successful result with valid parameters")]
        [Fact]
        public async Task ProcessDocumentAsync_WithValidParameters_ShouldReturnSuccessResult()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            var integration = Integration.Create(TestHelpers.GetRandomUserId(), "AzureCognitiveServices", "conn1");
            var documentUrl = "https://example.com/document.pdf";
            var options = new DocumentProcessingOptions { DocumentType = "invoice" };
            var extractedData = new Dictionary<string, string> { { "field1", "value1" } };

            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync(integration);
            _mockCircuitBreakerPolicy.Setup(x => x.CurrentState).Returns(Closed);
            _mockCircuitBreakerPolicy.Setup(x => x.ExecuteAsync(It.IsAny<Func<Task<Result<Dictionary<string, string>>>>>>(), It.IsAny<string>()))
                .ReturnsAsync(Result<Dictionary<string, string>>.Success(extractedData));
            _mockOcrProcessor.Setup(ocr => ocr.ProcessDocumentAsync(documentUrl, options))
                .ReturnsAsync(Result<Dictionary<string, string>>.Success(extractedData));
            _mockRepository.Setup(repo => repo.UpdateLastSyncDateAsync(integrationId)).ReturnsAsync(true);

            // Act
            var result = await _integrationService.ProcessDocumentAsync(integrationId, documentUrl, options);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.ExtractedData.Should().BeEquivalentTo(extractedData);
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
            _mockCircuitBreakerPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result<Dictionary<string, string>>>>>>(), It.IsAny<string>()), Times.Once);
            _mockOcrProcessor.Verify(ocr => ocr.ProcessDocumentAsync(documentUrl, options), Times.Never);
            _mockRepository.Verify(repo => repo.UpdateLastSyncDateAsync(integrationId), Times.Once);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that ProcessDocumentAsync returns a failure result with an invalid integration ID")]
        [Fact]
        public async Task ProcessDocumentAsync_WithInvalidIntegrationId_ShouldReturnFailureResult()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            var documentUrl = "https://example.com/document.pdf";
            var options = new DocumentProcessingOptions { DocumentType = "invoice" };

            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync((Integration)null!);

            // Act
            var result = await _integrationService.ProcessDocumentAsync(integrationId, documentUrl, options);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
            _mockCircuitBreakerPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result<Dictionary<string, string>>>>>>(), It.IsAny<string>()), Times.Never);
            _mockOcrProcessor.Verify(ocr => ocr.ProcessDocumentAsync(It.IsAny<string>(), It.IsAny<DocumentProcessingOptions>()), Times.Never);
            _mockRepository.Verify(repo => repo.UpdateLastSyncDateAsync(It.IsAny<string>>()), Times.Never);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that ProcessDocumentAsync returns a failure result with an integration that doesn't support OCR")]
        [Fact]
        public async Task ProcessDocumentAsync_WithUnsupportedSystemType_ShouldReturnFailureResult()
        {
            // Arrange
            var integrationId = Guid.NewGuid().ToString();
            var integration = Integration.Create(TestHelpers.GetRandomUserId(), "Dynamics365", "conn1");
            var documentUrl = "https://example.com/document.pdf";
            var options = new DocumentProcessingOptions { DocumentType = "invoice" };

            _mockRepository.Setup(repo => repo.GetByIdAsync(integrationId)).ReturnsAsync(integration);

            // Act
            var result = await _integrationService.ProcessDocumentAsync(integrationId, documentUrl, options);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            _mockRepository.Verify(repo => repo.GetByIdAsync(integrationId), Times.Once);
            _mockCircuitBreakerPolicy.Verify(x => x.ExecuteAsync(It.IsAny<Func<Task<Result<Dictionary<string, string>>>>>>(), It.IsAny<string>()), Times.Never);
            _mockOcrProcessor.Verify(ocr => ocr.ProcessDocumentAsync(It.IsAny<string>(), It.IsAny<DocumentProcessingOptions>()), Times.Never);
            _mockRepository.Verify(repo => repo.UpdateLastSyncDateAsync(It.IsAny<string>>()), Times.Never);
        }

        [public: System.ComponentModel.DescriptionAttribute("Tests that GetAvailableSystemTypesAsync returns all available system types")]
        [Fact]
        public async Task GetAvailableSystemTypesAsync_ShouldReturnAllSystemTypes()
        {
            // Act
            var result = await _integrationService.GetAvailableSystemTypesAsync();

            // Assert
            result.Should().Contain("Dynamics365");
            result.Should().Contain("AzureCognitiveServices");
            result.Should().NotBeEmpty();
        }
    }
}