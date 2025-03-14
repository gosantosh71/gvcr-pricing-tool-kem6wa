using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Infrastructure.Storage
{
    /// <summary>
    /// Client for Azure Blob Storage operations in the VAT Filing Pricing Tool.
    /// Provides functionality for uploading, downloading, and managing files in blob storage,
    /// with support for reports, templates, and documents.
    /// </summary>
    public class BlobStorageClient
    {
        private readonly ILogger<BlobStorageClient> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly StorageOptions _options;

        /// <summary>
        /// Initializes a new instance of the BlobStorageClient class with dependencies.
        /// </summary>
        /// <param name="options">Configuration options for Azure Blob Storage.</param>
        /// <param name="logger">Logger for recording operations and errors.</param>
        /// <exception cref="ArgumentNullException">Thrown if options or logger is null.</exception>
        public BlobStorageClient(IOptions<StorageOptions> options, ILogger<BlobStorageClient> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
            
            if (_options.CreateContainersIfNotExist)
            {
                EnsureContainersExistAsync().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage.
        /// </summary>
        /// <param name="data">The file data as a byte array.</param>
        /// <param name="blobName">The name of the blob to upload.</param>
        /// <param name="containerName">The container to upload to.</param>
        /// <param name="contentType">Optional. The content type of the blob.</param>
        /// <returns>Result containing the URL of the uploaded blob or error details.</returns>
        public async Task<Result<string>> UploadBlobAsync(byte[] data, string blobName, string containerName, string contentType = null)
        {
            try
            {
                if (data == null || data.Length == 0)
                {
                    return Result<string>.Failure("File data cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(blobName))
                {
                    return Result<string>.Failure("Blob name cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(containerName))
                {
                    return Result<string>.Failure("Container name cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                _logger.LogInformation("Uploading blob {BlobName} to container {ContainerName}", blobName, containerName);

                var containerClient = GetContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var options = new BlobUploadOptions();
                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    options.HttpHeaders = new BlobHttpHeaders { ContentType = contentType };
                }
                else
                {
                    // Try to determine content type from file name
                    string detectedContentType = GetContentType(blobName);
                    if (!string.IsNullOrWhiteSpace(detectedContentType))
                    {
                        options.HttpHeaders = new BlobHttpHeaders { ContentType = detectedContentType };
                    }
                }

                await blobClient.UploadAsync(BinaryData.FromBytes(data), options);

                _logger.LogInformation("Successfully uploaded blob {BlobName} to container {ContainerName}", blobName, containerName);
                return Result<string>.Success(blobClient.Uri.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading blob {BlobName} to container {ContainerName}: {ErrorMessage}", 
                    blobName, containerName, ex.Message);
                return Result<string>.Failure($"Failed to upload file: {ex.Message}", ErrorCodes.Storage.UploadFailed);
            }
        }

        /// <summary>
        /// Downloads a file from Azure Blob Storage.
        /// </summary>
        /// <param name="blobName">The name of the blob to download.</param>
        /// <param name="containerName">The container to download from.</param>
        /// <returns>Result containing the downloaded blob data or error details.</returns>
        public async Task<Result<byte[]>> GetBlobAsync(string blobName, string containerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    return Result<byte[]>.Failure("Blob name cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(containerName))
                {
                    return Result<byte[]>.Failure("Container name cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                _logger.LogInformation("Downloading blob {BlobName} from container {ContainerName}", blobName, containerName);

                var containerClient = GetContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                // Check if blob exists
                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning("Blob {BlobName} not found in container {ContainerName}", blobName, containerName);
                    return Result<byte[]>.Failure($"Blob '{blobName}' not found", ErrorCodes.General.NotFound);
                }

                using var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                byte[] data = memoryStream.ToArray();

                _logger.LogInformation("Successfully downloaded blob {BlobName} from container {ContainerName}", blobName, containerName);
                return Result<byte[]>.Success(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading blob {BlobName} from container {ContainerName}: {ErrorMessage}", 
                    blobName, containerName, ex.Message);
                return Result<byte[]>.Failure($"Failed to download file: {ex.Message}", ErrorCodes.Storage.DownloadFailed);
            }
        }

        /// <summary>
        /// Deletes a file from Azure Blob Storage.
        /// </summary>
        /// <param name="blobName">The name of the blob to delete.</param>
        /// <param name="containerName">The container to delete from.</param>
        /// <returns>Result indicating success or failure of the delete operation.</returns>
        public async Task<Result> DeleteBlobAsync(string blobName, string containerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    return Result.Failure("Blob name cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(containerName))
                {
                    return Result.Failure("Container name cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                _logger.LogInformation("Deleting blob {BlobName} from container {ContainerName}", blobName, containerName);

                var containerClient = GetContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                await blobClient.DeleteIfExistsAsync();

                _logger.LogInformation("Successfully deleted blob {BlobName} from container {ContainerName}", blobName, containerName);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blob {BlobName} from container {ContainerName}: {ErrorMessage}", 
                    blobName, containerName, ex.Message);
                return Result.Failure($"Failed to delete file: {ex.Message}", ErrorCodes.Storage.DeleteFailed);
            }
        }

        /// <summary>
        /// Generates a SAS URL for a blob with read permissions.
        /// </summary>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="containerName">The container name.</param>
        /// <param name="expirationHours">Optional. The number of hours until the SAS token expires. If not specified, uses the default from configuration.</param>
        /// <returns>Result containing the SAS URL or error details.</returns>
        public Result<string> GenerateSasUrl(string blobName, string containerName, int? expirationHours = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    return Result<string>.Failure("Blob name cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(containerName))
                {
                    return Result<string>.Failure("Container name cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                _logger.LogInformation("Generating SAS URL for blob {BlobName} in container {ContainerName}", blobName, containerName);

                var containerClient = GetContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                // Set the expiry time to the specified hours or default from options
                var sasExpiryTime = DateTimeOffset.UtcNow.AddHours(expirationHours ?? _options.SasTokenExpirationHours);

                // Create a SAS token that's valid for the specified duration
                BlobSasBuilder sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerClient.Name,
                    BlobName = blobName,
                    Resource = "b", // b for blob
                    ExpiresOn = sasExpiryTime
                };

                // Set the permissions for the SAS token
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Generate the SAS token
                var sasToken = blobClient.GenerateSasUri(sasBuilder);

                _logger.LogInformation("Successfully generated SAS URL for blob {BlobName} in container {ContainerName}", blobName, containerName);
                return Result<string>.Success(sasToken.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS URL for blob {BlobName} in container {ContainerName}: {ErrorMessage}", 
                    blobName, containerName, ex.Message);
                return Result<string>.Failure($"Failed to generate SAS URL: {ex.Message}", ErrorCodes.Storage.SasGenerationFailed);
            }
        }

        /// <summary>
        /// Validates if a file type is allowed based on extension.
        /// </summary>
        /// <param name="fileName">The file name to validate.</param>
        /// <returns>Result indicating if the file type is valid.</returns>
        public async Task<Result<bool>> ValidateFileTypeAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Result<bool>.Failure("File name cannot be null or empty", ErrorCodes.General.BadRequest);
                }

                string extension = Path.GetExtension(fileName).ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(extension))
                {
                    return Result<bool>.Failure("File has no extension", ErrorCodes.Integration.InvalidDocumentFormat);
                }

                bool isValid = _options.AllowedFileExtensions.Contains(extension);

                if (!isValid)
                {
                    _logger.LogWarning("File type {Extension} is not allowed", extension);
                    return Result<bool>.Failure($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _options.AllowedFileExtensions)}", 
                        ErrorCodes.Integration.InvalidDocumentFormat);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file type for {FileName}: {ErrorMessage}", fileName, ex.Message);
                return Result<bool>.Failure($"Failed to validate file type: {ex.Message}", ErrorCodes.General.ServerError);
            }
        }

        /// <summary>
        /// Ensures that all required containers exist in blob storage.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task EnsureContainersExistAsync()
        {
            try
            {
                _logger.LogInformation("Ensuring blob containers exist");

                var reportsContainer = _blobServiceClient.GetBlobContainerClient(_options.ReportsContainerName);
                await reportsContainer.CreateIfNotExistsAsync();

                var templatesContainer = _blobServiceClient.GetBlobContainerClient(_options.TemplatesContainerName);
                await templatesContainer.CreateIfNotExistsAsync();

                var documentsContainer = _blobServiceClient.GetBlobContainerClient(_options.DocumentsContainerName);
                await documentsContainer.CreateIfNotExistsAsync();

                _logger.LogInformation("Successfully ensured blob containers exist");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring blob containers exist: {ErrorMessage}", ex.Message);
                throw; // Rethrow as this is a critical setup operation
            }
        }

        /// <summary>
        /// Gets a BlobContainerClient for the specified container.
        /// </summary>
        /// <param name="containerName">The container name or alias.</param>
        /// <returns>The container client for the specified container.</returns>
        private BlobContainerClient GetContainerClient(string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));
            }

            // Map container name to actual container based on predefined constants
            string actualContainerName = containerName.ToLowerInvariant() switch
            {
                "reports" => _options.ReportsContainerName,
                "templates" => _options.TemplatesContainerName,
                "documents" => _options.DocumentsContainerName,
                _ => containerName // Use as-is if not a known alias
            };

            return _blobServiceClient.GetBlobContainerClient(actualContainerName);
        }

        /// <summary>
        /// Gets the content type based on file extension.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The MIME content type for the file.</returns>
        private string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".csv" => "text/csv",
                ".txt" => "text/plain",
                ".xml" => "application/xml",
                ".json" => "application/json",
                ".html" => "text/html",
                ".htm" => "text/html",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                _ => "application/octet-stream" // Default content type
            };
        }
    }
}