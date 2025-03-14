using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Infrastructure.Storage;
using Xunit;

namespace VatFilingPricingTool.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Integration tests for the BlobStorageClient class to verify Azure Blob Storage operations
    /// </summary>
    public class BlobStorageIntegrationTests : IDisposable
    {
        private readonly BlobStorageClient _blobStorageClient;
        private readonly ILogger<BlobStorageClient> _logger;
        private readonly StorageOptions _options;
        private readonly string _testContainerName;
        private readonly BlobServiceClient _blobServiceClient;

        /// <summary>
        /// Initializes a new instance of the BlobStorageIntegrationTests class
        /// </summary>
        public BlobStorageIntegrationTests()
        {
            // Create a mock logger for testing
            var loggerMock = new Mock<ILogger<BlobStorageClient>>();
            _logger = loggerMock.Object;

            // Initialize StorageOptions with test values
            _options = new StorageOptions
            {
                // Azurite local emulator connection string for isolated testing
                ConnectionString = "UseDevelopmentStorage=true",
                ReportsContainerName = "test-reports",
                TemplatesContainerName = "test-templates",
                DocumentsContainerName = "test-documents",
                CreateContainersIfNotExist = true,
                SasTokenExpirationHours = 1,
                AllowedFileExtensions = new System.Collections.Generic.List<string> 
                { 
                    ".pdf", ".xlsx", ".docx", ".csv", ".txt" 
                }
            };

            // Create options wrapper for injection
            var optionsWrapper = Options.Create(_options);

            // Create BlobServiceClient for test cleanup operations
            _blobServiceClient = new BlobServiceClient(_options.ConnectionString);

            // Create the BlobStorageClient with test configuration
            _blobStorageClient = new BlobStorageClient(optionsWrapper, _logger);

            // Store test container name for reuse in tests
            _testContainerName = _options.ReportsContainerName;
        }

        /// <summary>
        /// Cleans up resources after tests
        /// </summary>
        public void Dispose()
        {
            // Delete test containers if they exist
            try
            {
                var reportsContainer = _blobServiceClient.GetBlobContainerClient(_options.ReportsContainerName);
                reportsContainer.DeleteIfExistsAsync().GetAwaiter().GetResult();

                var templatesContainer = _blobServiceClient.GetBlobContainerClient(_options.TemplatesContainerName);
                templatesContainer.DeleteIfExistsAsync().GetAwaiter().GetResult();

                var documentsContainer = _blobServiceClient.GetBlobContainerClient(_options.DocumentsContainerName);
                documentsContainer.DeleteIfExistsAsync().GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // Log but don't fail tests if cleanup fails
            }
        }

        /// <summary>
        /// Tests that uploading a valid blob succeeds
        /// </summary>
        [Fact]
        public async Task UploadBlobAsync_ValidData_ShouldSucceed()
        {
            // Arrange
            var testData = CreateTestData("Hello, world!");
            var blobName = GenerateRandomBlobName(".txt");

            // Act
            var result = await _blobStorageClient.UploadBlobAsync(testData, blobName, _testContainerName);

            // Assert
            result.IsSuccess.Should().BeTrue("because uploading a valid blob should succeed");
            result.Value.Should().NotBeNullOrEmpty("because the blob URL should be returned");
            
            // Verify the blob exists in the container
            var exists = await BlobExists(blobName, _testContainerName);
            exists.Should().BeTrue("because the blob should have been created");
        }

        /// <summary>
        /// Tests that retrieving an existing blob returns the correct data
        /// </summary>
        [Fact]
        public async Task GetBlobAsync_ExistingBlob_ShouldReturnData()
        {
            // Arrange
            var testContent = "Test content for download";
            var testData = CreateTestData(testContent);
            var blobName = GenerateRandomBlobName(".txt");
            
            // Upload a test blob first
            await _blobStorageClient.UploadBlobAsync(testData, blobName, _testContainerName);

            // Act
            var result = await _blobStorageClient.GetBlobAsync(blobName, _testContainerName);

            // Assert
            result.IsSuccess.Should().BeTrue("because retrieving an existing blob should succeed");
            result.Value.Should().NotBeNull("because the blob data should be returned");
            Encoding.UTF8.GetString(result.Value).Should().Be(testContent, "because the content should match what was uploaded");
        }

        /// <summary>
        /// Tests that attempting to retrieve a non-existent blob returns a failure result
        /// </summary>
        [Fact]
        public async Task GetBlobAsync_NonExistentBlob_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentBlobName = GenerateRandomBlobName(".txt");

            // Act
            var result = await _blobStorageClient.GetBlobAsync(nonExistentBlobName, _testContainerName);

            // Assert
            result.IsSuccess.Should().BeFalse("because retrieving a non-existent blob should fail");
            result.ErrorCode.Should().Be(ErrorCodes.General.NotFound, "because a not found error should be returned");
        }

        /// <summary>
        /// Tests that deleting an existing blob succeeds
        /// </summary>
        [Fact]
        public async Task DeleteBlobAsync_ExistingBlob_ShouldSucceed()
        {
            // Arrange
            var testData = CreateTestData("Delete me");
            var blobName = GenerateRandomBlobName(".txt");
            
            // Upload a test blob first
            await _blobStorageClient.UploadBlobAsync(testData, blobName, _testContainerName);

            // Act
            var result = await _blobStorageClient.DeleteBlobAsync(blobName, _testContainerName);

            // Assert
            result.IsSuccess.Should().BeTrue("because deleting an existing blob should succeed");
            
            // Verify the blob no longer exists
            var exists = await BlobExists(blobName, _testContainerName);
            exists.Should().BeFalse("because the blob should have been deleted");
        }

        /// <summary>
        /// Tests that attempting to delete a non-existent blob returns a failure result
        /// </summary>
        [Fact]
        public async Task DeleteBlobAsync_NonExistentBlob_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentBlobName = GenerateRandomBlobName(".txt");

            // Act
            var result = await _blobStorageClient.DeleteBlobAsync(nonExistentBlobName, _testContainerName);

            // Assert
            result.IsSuccess.Should().BeFalse("because deleting a non-existent blob should return failure");
            result.ErrorCode.Should().NotBeNull("because an error code should be provided");
        }

        /// <summary>
        /// Tests that generating a SAS URL for an existing blob returns a valid URL
        /// </summary>
        [Fact]
        public async Task GenerateSasUrl_ExistingBlob_ShouldReturnValidUrl()
        {
            // Arrange
            var testData = CreateTestData("SAS test content");
            var blobName = GenerateRandomBlobName(".txt");
            
            // Upload a test blob first
            await _blobStorageClient.UploadBlobAsync(testData, blobName, _testContainerName);

            // Act
            var result = _blobStorageClient.GenerateSasUrl(blobName, _testContainerName);

            // Assert
            result.IsSuccess.Should().BeTrue("because generating a SAS URL for an existing blob should succeed");
            result.Value.Should().NotBeNullOrEmpty("because a valid URL should be returned");
            result.Value.Should().Contain("sig=", "because the URL should contain SAS token signature");
        }

        /// <summary>
        /// Tests that validating a file with an allowed extension returns success
        /// </summary>
        [Fact]
        public async Task ValidateFileTypeAsync_AllowedExtension_ShouldReturnSuccess()
        {
            // Arrange
            var validFileName = "test-document.pdf";

            // Act
            var result = await _blobStorageClient.ValidateFileTypeAsync(validFileName);

            // Assert
            result.IsSuccess.Should().BeTrue("because validating an allowed file type should succeed");
            result.Value.Should().BeTrue("because the file extension is in the allowed list");
        }

        /// <summary>
        /// Tests that validating a file with a disallowed extension returns failure
        /// </summary>
        [Fact]
        public async Task ValidateFileTypeAsync_DisallowedExtension_ShouldReturnFailure()
        {
            // Arrange
            var invalidFileName = "malicious-file.exe";

            // Act
            var result = await _blobStorageClient.ValidateFileTypeAsync(invalidFileName);

            // Assert
            result.IsSuccess.Should().BeFalse("because validating a disallowed file type should fail");
            result.ErrorCode.Should().Be(ErrorCodes.Integration.InvalidDocumentFormat, 
                "because an invalid document format error should be returned");
        }

        /// <summary>
        /// Tests that uploading blobs to different containers stores them in the correct locations
        /// </summary>
        [Fact]
        public async Task UploadBlobAsync_DifferentContainers_ShouldStoreInCorrectContainer()
        {
            // Arrange
            var reportData = CreateTestData("Report content");
            var reportBlobName = GenerateRandomBlobName(".pdf");
            
            var templateData = CreateTestData("Template content");
            var templateBlobName = GenerateRandomBlobName(".docx");
            
            var documentData = CreateTestData("Document content");
            var documentBlobName = GenerateRandomBlobName(".xlsx");

            // Act
            var reportResult = await _blobStorageClient.UploadBlobAsync(reportData, reportBlobName, "reports");
            var templateResult = await _blobStorageClient.UploadBlobAsync(templateData, templateBlobName, "templates");
            var documentResult = await _blobStorageClient.UploadBlobAsync(documentData, documentBlobName, "documents");

            // Assert
            reportResult.IsSuccess.Should().BeTrue("because uploading to reports container should succeed");
            templateResult.IsSuccess.Should().BeTrue("because uploading to templates container should succeed");
            documentResult.IsSuccess.Should().BeTrue("because uploading to documents container should succeed");
            
            // Verify each blob exists in its respective container
            var reportExists = await BlobExists(reportBlobName, _options.ReportsContainerName);
            var templateExists = await BlobExists(templateBlobName, _options.TemplatesContainerName);
            var documentExists = await BlobExists(documentBlobName, _options.DocumentsContainerName);
            
            reportExists.Should().BeTrue("because the report blob should exist in the reports container");
            templateExists.Should().BeTrue("because the template blob should exist in the templates container");
            documentExists.Should().BeTrue("because the document blob should exist in the documents container");
            
            // Verify blobs don't exist in other containers
            var reportInTemplates = await BlobExists(reportBlobName, _options.TemplatesContainerName);
            var reportInDocuments = await BlobExists(reportBlobName, _options.DocumentsContainerName);
            
            reportInTemplates.Should().BeFalse("because the report blob should not exist in the templates container");
            reportInDocuments.Should().BeFalse("because the report blob should not exist in the documents container");
        }

        /// <summary>
        /// Creates test data for blob operations
        /// </summary>
        /// <param name="content">The content to convert to binary data</param>
        /// <returns>Byte array containing the test data</returns>
        private byte[] CreateTestData(string content)
        {
            return Encoding.UTF8.GetBytes(content);
        }

        /// <summary>
        /// Generates a random blob name for testing
        /// </summary>
        /// <param name="extension">The file extension to append</param>
        /// <returns>A random blob name with the specified extension</returns>
        private string GenerateRandomBlobName(string extension)
        {
            return Guid.NewGuid().ToString() + extension;
        }

        /// <summary>
        /// Checks if a blob exists in the specified container
        /// </summary>
        /// <param name="blobName">The name of the blob to check</param>
        /// <param name="containerName">The container to check in</param>
        /// <returns>True if the blob exists, false otherwise</returns>
        private async Task<bool> BlobExists(string blobName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            return await blobClient.ExistsAsync();
        }
    }
}