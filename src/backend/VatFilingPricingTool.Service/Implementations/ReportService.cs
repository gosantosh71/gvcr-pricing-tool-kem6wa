using System; // Version: 6.0.0
using System.Collections.Generic; // Version: 6.0.0
using System.IO; // Version: 6.0.0
using System.Linq; // Version: 6.0.0
using System.Threading.Tasks; // Version: 6.0.0
using Microsoft.Extensions.Logging; // Version: 6.0.0
using VatFilingPricingTool.Common.Constants; // Provides standardized error codes
using VatFilingPricingTool.Common.Models; // Provides standardized API response structure and pagination
using VatFilingPricingTool.Contracts.V1.Requests; // Provides the request models for report operations
using VatFilingPricingTool.Contracts.V1.Responses; // Provides the response models for report operations
using VatFilingPricingTool.Data.Repositories.Interfaces; // Provides the repository interfaces for data access
using VatFilingPricingTool.Domain.Enums; // Defines the available formats for report generation and export
using VatFilingPricingTool.Infrastructure.Integration.Email; // Provides the email sending interface
using VatFilingPricingTool.Infrastructure.Storage; // Provides the blob storage client
using VatFilingPricingTool.Service.Helpers; // Provides helper methods for report generation
using VatFilingPricingTool.Service.Interfaces; // Provides the report service interface
using VatFilingPricingTool.Service.Models; // Provides the service models for report data

namespace VatFilingPricingTool.Service.Implementations
{
    /// <summary>
    /// Implementation of <see cref="IReportService"/> that handles report generation, retrieval, and management
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly ILogger<ReportService> _logger;
        private readonly IReportRepository _reportRepository;
        private readonly IPricingService _pricingService;
        private readonly BlobStorageClient _blobStorageClient;
        private readonly IEmailSender _emailSender;
        private readonly string _reportsContainerName = "reports";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class with dependencies
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="reportRepository">The report repository</param>
        /// <param name="pricingService">The pricing service</param>
        /// <param name="blobStorageClient">The blob storage client</param>
        /// <param name="emailSender">The email sender</param>
        public ReportService(
            ILogger<ReportService> logger,
            IReportRepository reportRepository,
            IPricingService pricingService,
            BlobStorageClient blobStorageClient,
            IEmailSender emailSender)
        {
            // Validate that all dependencies are not null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
            _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
            _blobStorageClient = blobStorageClient ?? throw new ArgumentNullException(nameof(blobStorageClient));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));

            // Set the reports container name
            _reportsContainerName = "reports";
        }

        /// <summary>
        /// Generates a new report based on calculation data with specified format and content options
        /// </summary>
        /// <param name="request">The report generation request containing calculation ID, title, format, and content options</param>
        /// <param name="userId">ID of the user generating the report</param>
        /// <returns>A result containing the generated report information or error</returns>
        public async Task<Result<GenerateReportResponse>> GenerateReportAsync(GenerateReportRequest request, string userId)
        {
            // Validate request parameters (calculationId, reportTitle)
            if (string.IsNullOrEmpty(request?.CalculationId))
            {
                return Result<GenerateReportResponse>.Failure("CalculationId is required", ErrorCodes.Report.InvalidReportParameters);
            }

            if (string.IsNullOrEmpty(request?.ReportTitle))
            {
                return Result<GenerateReportResponse>.Failure("ReportTitle is required", ErrorCodes.Report.InvalidReportParameters);
            }

            _logger.LogInformation($"Starting report generation for CalculationId: {request.CalculationId}, UserId: {userId}");

            try
            {
                // Get calculation data using _pricingService.GetCalculationAsync
                var calculationResult = await _pricingService.GetCalculationAsync(new GetCalculationRequest { CalculationId = request.CalculationId });

                // If calculation not found, return failure result with appropriate error code
                if (!calculationResult.IsSuccess)
                {
                    _logger.LogWarning($"Calculation not found for CalculationId: {request.CalculationId}");
                    return Result<GenerateReportResponse>.Failure($"Calculation not found: {request.CalculationId}", ErrorCodes.Pricing.CalculationNotFound);
                }

                // Create a new ReportModel with data from request and calculation
                var reportModel = new ReportModel
                {
                    ReportTitle = request.ReportTitle,
                    ReportType = $"{(request.IncludeCountryBreakdown ? "Country" : "")}" +
                                 $"{(request.IncludeServiceDetails ? "Service" : "")}" +
                                 $"{(request.IncludeAppliedDiscounts ? "Discount" : "")}" +
                                 $"{(request.IncludeHistoricalComparison ? "Historical" : "")}" +
                                 $"{(request.IncludeTaxRateDetails ? "Tax" : "")}",
                    Format = request.Format,
                    CalculationId = request.CalculationId,
                    CalculationData = CalculationModel.FromContractModel(calculationResult.Value),
                    UserId = userId,
                    IncludeCountryBreakdown = request.IncludeCountryBreakdown,
                    IncludeServiceDetails = request.IncludeServiceDetails,
                    IncludeAppliedDiscounts = request.IncludeAppliedDiscounts,
                    IncludeHistoricalComparison = request.IncludeHistoricalComparison,
                    IncludeTaxRateDetails = request.IncludeTaxRateDetails
                };

                // Set UserId to the provided userId
                reportModel.UserId = userId;

                // Generate report content using ReportGenerationHelper.GenerateReport
                byte[] reportContent = ReportGenerationHelper.GenerateReport(reportModel);

                // Create a unique blob name using a GUID and file extension
                string blobName = $"{Guid.NewGuid()}{reportModel.GetFileExtension()}";

                // Upload the report to blob storage using _blobStorageClient.UploadBlobAsync
                var uploadResult = await _blobStorageClient.UploadBlobAsync(reportContent, blobName, _reportsContainerName, reportModel.GetContentType());

                // Check if upload was successful
                if (!uploadResult.IsSuccess)
                {
                    _logger.LogError($"Failed to upload report to blob storage: {uploadResult.ErrorMessage}");
                    return Result<GenerateReportResponse>.Failure($"Failed to upload report: {uploadResult.ErrorMessage}", ErrorCodes.Report.ReportGenerationFailed);
                }

                // Update the report model with storage details (URL and file size)
                reportModel.SetStorageDetails(uploadResult.Value, reportContent.Length);

                // Create a domain entity from the model using ToEntity()
                var reportEntity = reportModel.ToEntity();

                // Save the report entity to the repository using _reportRepository.AddAsync
                await _reportRepository.AddAsync(reportEntity);

                // Generate a SAS URL for the report using _blobStorageClient.GenerateSasUrl
                var sasResult = _blobStorageClient.GenerateSasUrl(blobName, _reportsContainerName);

                // Check if SAS URL generation was successful
                if (!sasResult.IsSuccess)
                {
                    _logger.LogError($"Failed to generate SAS URL: {sasResult.ErrorMessage}");
                    return Result<GenerateReportResponse>.Failure($"Failed to generate SAS URL: {sasResult.ErrorMessage}", ErrorCodes.Report.ReportGenerationFailed);
                }

                // Create and return a success result with GenerateReportResponse containing reportId and downloadUrl
                var response = new GenerateReportResponse
                {
                    ReportId = reportEntity.ReportId,
                    ReportTitle = reportEntity.ReportTitle,
                    Format = reportEntity.Format,
                    DownloadUrl = sasResult.Value,
                    GenerationDate = reportEntity.GenerationDate,
                    FileSize = reportEntity.FileSize,
                    IsReady = true
                };

                _logger.LogInformation($"Successfully generated report with ReportId: {reportEntity.ReportId}");
                return Result<GenerateReportResponse>.Success(response);
            }
            catch (Exception ex)
            {
                // If any exceptions occur, log error and return failure result with appropriate error code
                _logger.LogError(ex, $"Error generating report for CalculationId: {request.CalculationId}, UserId: {userId}: {ex.Message}");
                return Result<GenerateReportResponse>.Failure($"Error generating report: {ex.Message}", ErrorCodes.Report.ReportGenerationFailed);
            }
        }

        /// <summary>
        /// Retrieves a specific report by ID with detailed information and download URL
        /// </summary>
        /// <param name="request">The report retrieval request containing the report ID</param>
        /// <param name="userId">ID of the user requesting the report</param>
        /// <returns>A result containing the report information or error</returns>
        public async Task<Result<GetReportResponse>> GetReportAsync(GetReportRequest request, string userId)
        {
            // Validate request parameters (reportId)
            if (string.IsNullOrEmpty(request?.ReportId))
            {
                return Result<GetReportResponse>.Failure("ReportId is required", ErrorCodes.Report.InvalidReportParameters);
            }

            _logger.LogInformation($"Attempting to retrieve report with ReportId: {request.ReportId}, UserId: {userId}");

            try
            {
                // Get report entity from repository using _reportRepository.GetByIdWithDetailsAsync
                var reportEntity = await _reportRepository.GetByIdWithDetailsAsync(request.ReportId);

                // If report not found, return failure result with appropriate error code
                if (reportEntity == null)
                {
                    _logger.LogWarning($"Report not found with ReportId: {request.ReportId}");
                    return Result<GetReportResponse>.Failure($"Report not found with ReportId: {request.ReportId}", ErrorCodes.Report.ReportNotFound);
                }

                // Verify that the report belongs to the requesting user
                if (!ValidateReportOwnership(reportEntity.UserId, userId))
                {
                    _logger.LogWarning($"User {userId} is not authorized to access report {request.ReportId}");
                    return Result<GetReportResponse>.Failure("Unauthorized access", ErrorCodes.General.Unauthorized);
                }

                // Generate a SAS URL for the report using _blobStorageClient.GenerateSasUrl
                var sasResult = _blobStorageClient.GenerateSasUrl(reportEntity.StorageUrl, _reportsContainerName);

                // Check if SAS URL generation was successful
                if (!sasResult.IsSuccess)
                {
                    _logger.LogError($"Failed to generate SAS URL: {sasResult.ErrorMessage}");
                    return Result<GetReportResponse>.Failure($"Failed to generate SAS URL: {sasResult.ErrorMessage}", ErrorCodes.Report.ReportDownloadFailed);
                }

                // Create and return a success result with GetReportResponse containing report details and downloadUrl
                var response = new GetReportResponse
                {
                    ReportId = reportEntity.ReportId,
                    ReportTitle = reportEntity.ReportTitle,
                    CalculationId = reportEntity.CalculationId,
                    Format = reportEntity.Format,
                    DownloadUrl = sasResult.Value,
                    GenerationDate = reportEntity.GenerationDate,
                    FileSize = reportEntity.FileSize,
                    IncludeCountryBreakdown = reportEntity.ReportType.Contains("Country"),
                    IncludeServiceDetails = reportEntity.ReportType.Contains("Service"),
                    IncludeAppliedDiscounts = reportEntity.ReportType.Contains("Discount"),
                    IncludeHistoricalComparison = reportEntity.ReportType.Contains("Historical"),
                    IncludeTaxRateDetails = reportEntity.ReportType.Contains("Tax"),
                    IsArchived = reportEntity.IsArchived
                };

                _logger.LogInformation($"Successfully retrieved report with ReportId: {request.ReportId}");
                return Result<GetReportResponse>.Success(response);
            }
            catch (Exception ex)
            {
                // If any exceptions occur, log error and return failure result with appropriate error code
                _logger.LogError(ex, $"Error retrieving report with ReportId: {request.ReportId}, UserId: {userId}: {ex.Message}");
                return Result<GetReportResponse>.Failure($"Error retrieving report: {ex.Message}", ErrorCodes.Report.ReportDownloadFailed);
            }
        }

        /// <summary>
        /// Retrieves a paginated list of reports for a user with optional filtering
        /// </summary>
        /// <param name="request">The report history request containing pagination, filtering and sorting parameters</param>
        /// <param name="userId">ID of the user requesting report history</param>
        /// <returns>A result containing the paginated list of reports</returns>
        public async Task<Result<GetReportHistoryResponse>> GetReportHistoryAsync(GetReportHistoryRequest request, string userId)
        {
            // Validate request parameters (page, pageSize)
            if (request == null)
            {
                return Result<GetReportHistoryResponse>.Failure("Request cannot be null", ErrorCodes.General.BadRequest);
            }

            // Set default values if not provided (page=1, pageSize=10)
            int page = request.Page > 0 ? request.Page : 1;
            int pageSize = request.PageSize > 0 ? request.PageSize : 10;

            _logger.LogInformation($"Attempting to retrieve report history for UserId: {userId}, Page: {page}, PageSize: {pageSize}");

            try
            {
                // Get paginated report entities from repository using _reportRepository.GetPagedByUserIdAsync
                var pagedReports = await _reportRepository.GetPagedByUserIdAsync(userId, page, pageSize);

                // Create report response models for each report entity
                var reportListItems = pagedReports.Items.Select(reportEntity => new ReportListItem
                {
                    ReportId = reportEntity.ReportId,
                    ReportTitle = reportEntity.ReportTitle,
                    Format = reportEntity.Format,
                    GenerationDate = reportEntity.GenerationDate,
                    FileSize = reportEntity.FileSize,
                    IsArchived = reportEntity.IsArchived
                }).ToList();

                // Create and return a success result with GetReportHistoryResponse containing paginated reports
                var response = new GetReportHistoryResponse
                {
                    Reports = reportListItems,
                    PageNumber = pagedReports.PageNumber,
                    PageSize = pagedReports.PageSize,
                    TotalCount = pagedReports.TotalCount,
                    TotalPages = pagedReports.TotalPages,
                    HasPreviousPage = pagedReports.HasPreviousPage,
                    HasNextPage = pagedReports.HasNextPage
                };

                _logger.LogInformation($"Successfully retrieved report history for UserId: {userId}, Total reports: {pagedReports.TotalCount}");
                return Result<GetReportHistoryResponse>.Success(response);
            }
            catch (Exception ex)
            {
                // If any exceptions occur, log error and return failure result with appropriate error code
                _logger.LogError(ex, $"Error retrieving report history for UserId: {userId}, Page: {page}, PageSize: {pageSize}: {ex.Message}");
                return Result<GetReportHistoryResponse>.Failure($"Error retrieving report history: {ex.Message}", ErrorCodes.Report.ReportDownloadFailed);
            }
        }

        /// <summary>
        /// Downloads a specific report, optionally converting to a different format
        /// </summary>
        /// <param name="request">The download request containing report ID and optional format conversion</param>
        /// <param name="userId">ID of the user downloading the report</param>
        /// <returns>A result containing the report file content or error</returns>
        public async Task<Result<DownloadReportResponse>> DownloadReportAsync(DownloadReportRequest request, string userId)
        {
            // Validate request parameters (reportId)
            if (string.IsNullOrEmpty(request?.ReportId))
            {
                return Result<DownloadReportResponse>.Failure("ReportId is required", ErrorCodes.Report.InvalidReportParameters);
            }

            _logger.LogInformation($"Attempting to download report with ReportId: {request.ReportId}, UserId: {userId}");

            try
            {
                // Get report entity from repository using _reportRepository.GetByIdWithDetailsAsync
                var reportEntity = await _reportRepository.GetByIdWithDetailsAsync(request.ReportId);

                // If report not found, return failure result with appropriate error code
                if (reportEntity == null)
                {
                    _logger.LogWarning($"Report not found with ReportId: {request.ReportId}");
                    return Result<DownloadReportResponse>.Failure($"Report not found with ReportId: {request.ReportId}", ErrorCodes.Report.ReportNotFound);
                }

                // Verify that the report belongs to the requesting user
                if (!ValidateReportOwnership(reportEntity.UserId, userId))
                {
                    _logger.LogWarning($"User {userId} is not authorized to download report {request.ReportId}");
                    return Result<DownloadReportResponse>.Failure("Unauthorized access", ErrorCodes.General.Unauthorized);
                }

                // Get the report content from blob storage using _blobStorageClient.GetBlobAsync
                var blobResult = await _blobStorageClient.GetBlobAsync(reportEntity.StorageUrl, _reportsContainerName);

                // Check if blob retrieval was successful
                if (!blobResult.IsSuccess)
                {
                    _logger.LogError($"Failed to retrieve blob from storage: {blobResult.ErrorMessage}");
                    return Result<DownloadReportResponse>.Failure($"Failed to retrieve blob from storage: {blobResult.ErrorMessage}", ErrorCodes.Report.ReportDownloadFailed);
                }

                byte[] reportContent = blobResult.Value;

                // If requested format is different from original format, convert using ReportGenerationHelper.ConvertReportFormat
                if (request.Format.HasValue && request.Format.Value != reportEntity.Format)
                {
                    try
                    {
                        reportContent = ReportGenerationHelper.ConvertReportFormat(reportContent, reportEntity.Format, request.Format.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to convert report format: {ex.Message}");
                        return Result<DownloadReportResponse>.Failure($"Failed to convert report format: {ex.Message}", ErrorCodes.Report.InvalidReportFormat);
                    }
                }

                // Create a filename based on report title and format
                string fileName = ReportGenerationHelper.GetReportFileName(reportEntity.ReportTitle, request.Format ?? reportEntity.Format);

                // Get content type based on format
                string contentType = ReportGenerationHelper.GetReportContentType(request.Format ?? reportEntity.Format);

                // Create and return a success result with DownloadReportResponse containing file content and metadata
                var response = new DownloadReportResponse
                {
                    ReportId = reportEntity.ReportId,
                    FileName = fileName,
                    ContentType = contentType,
                    FileContent = reportContent,
                    FileSize = reportContent.Length,
                    Format = request.Format ?? reportEntity.Format
                };

                _logger.LogInformation($"Successfully downloaded report with ReportId: {request.ReportId}");
                return Result<DownloadReportResponse>.Success(response);
            }
            catch (Exception ex)
            {
                // If any exceptions occur, log error and return failure result with appropriate error code
                _logger.LogError(ex, $"Error downloading report with ReportId: {request.ReportId}, UserId: {userId}: {ex.Message}");
                return Result<DownloadReportResponse>.Failure($"Error downloading report: {ex.Message}", ErrorCodes.Report.ReportDownloadFailed);
            }
        }

        /// <summary>
        /// Emails a specific report to the specified email address
        /// </summary>
        /// <param name="request">The email request containing report ID, recipient, subject and message</param>
        /// <param name="userId">ID of the user requesting the email</param>
        /// <returns>A result indicating whether the email was sent successfully</returns>
        public async Task<Result<EmailReportResponse>> EmailReportAsync(EmailReportRequest request, string userId)
        {
            // Validate request parameters (reportId, emailAddress)
            if (string.IsNullOrEmpty(request?.ReportId))
            {
                return Result<EmailReportResponse>.Failure("ReportId is required", ErrorCodes.Report.InvalidReportParameters);
            }

            if (string.IsNullOrEmpty(request?.EmailAddress))
            {
                return Result<EmailReportResponse>.Failure("EmailAddress is required", ErrorCodes.Report.InvalidReportParameters);
            }

            _logger.LogInformation($"Attempting to email report with ReportId: {request.ReportId} to EmailAddress: {request.EmailAddress}, UserId: {userId}");

            try
            {
                // Get report entity from repository using _reportRepository.GetByIdWithDetailsAsync
                var reportEntity = await _reportRepository.GetByIdWithDetailsAsync(request.ReportId);

                // If report not found, return failure result with appropriate error code
                if (reportEntity == null)
                {
                    _logger.LogWarning($"Report not found with ReportId: {request.ReportId}");
                    return Result<EmailReportResponse>.Failure($"Report not found with ReportId: {request.ReportId}", ErrorCodes.Report.ReportNotFound);
                }

                // Verify that the report belongs to the requesting user
                if (!ValidateReportOwnership(reportEntity.UserId, userId))
                {
                    _logger.LogWarning($"User {userId} is not authorized to email report {request.ReportId}");
                    return Result<EmailReportResponse>.Failure("Unauthorized access", ErrorCodes.General.Unauthorized);
                }

                // Get the report content from blob storage using _blobStorageClient.GetBlobAsync
                var blobResult = await _blobStorageClient.GetBlobAsync(reportEntity.StorageUrl, _reportsContainerName);

                // Check if blob retrieval was successful
                if (!blobResult.IsSuccess)
                {
                    _logger.LogError($"Failed to retrieve blob from storage: {blobResult.ErrorMessage}");
                    return Result<EmailReportResponse>.Failure($"Failed to retrieve blob from storage: {blobResult.ErrorMessage}", ErrorCodes.Report.ReportDownloadFailed);
                }

                byte[] reportContent = blobResult.Value;

                // Create a temporary file path for the report
                string tempFilePath = Path.GetTempFileName();

                // Write the report content to the temporary file
                await File.WriteAllBytesAsync(tempFilePath, reportContent);

                // Create email subject with report title
                string subject = request.Subject ?? $"VAT Filing Report: {reportEntity.ReportTitle}";

                // Create email template data with user and report information
                var templateData = new Dictionary<string, string>
                {
                    { "UserName", userId }, // Replace with actual user name if available
                    { "ReportName", reportEntity.ReportTitle },
                    { "ReportUrl", reportEntity.StorageUrl } // Consider generating a temporary SAS URL
                };

                // Send email with attachment using _emailSender.SendTemplatedEmailAsync
                var emailResult = await _emailSender.SendEmailAsync(request.EmailAddress, subject, "ReportReady", templateData);

                // Delete the temporary file after sending
                File.Delete(tempFilePath);

                // Check if email sending was successful
                if (!emailResult.IsSuccess)
                {
                    _logger.LogError($"Failed to send email: {emailResult.ErrorMessage}");
                    return Result<EmailReportResponse>.Failure($"Failed to send email: {emailResult.ErrorMessage}", ErrorCodes.Report.ReportEmailFailed);
                }

                // Create and return a success result with EmailReportResponse indicating email was sent
                var response = new EmailReportResponse
                {
                    ReportId = reportEntity.ReportId,
                    EmailAddress = request.EmailAddress,
                    EmailSent = true,
                    SentTime = DateTime.UtcNow
                };

                _logger.LogInformation($"Successfully emailed report with ReportId: {reportEntity.ReportId} to EmailAddress: {request.EmailAddress}");
                return Result<EmailReportResponse>.Success(response);
            }
            catch (Exception ex)
            {
                // If any exceptions occur, log error and return failure result with appropriate error code
                _logger.LogError(ex, $"Error emailing report with ReportId: {request.ReportId} to EmailAddress: {request.EmailAddress}, UserId: {userId}: {ex.Message}");
                return Result<EmailReportResponse>.Failure($"Error emailing report: {ex.Message}", ErrorCodes.Report.ReportEmailFailed);
            }
        }

        /// <summary>
        /// Archives a specific report to hide it from regular report listings
        /// </summary>
        /// <param name="request">The archive request containing report ID</param>
        /// <param name="userId">ID of the user archiving the report</param>
        /// <returns>A result indicating success or failure of the archive operation</returns>
        public async Task<Result<ArchiveReportResponse>> ArchiveReportAsync(ArchiveReportRequest request, string userId)
        {
            // Validate request parameters (reportId)
            if (string.IsNullOrEmpty(request?.ReportId))
            {
                return Result<ArchiveReportResponse>.Failure("ReportId is required", ErrorCodes.Report.InvalidReportParameters);
            }

            _logger.LogInformation($"Attempting to archive report with ReportId: {request.ReportId}, UserId: {userId}");

            try
            {
                // Get report entity from repository using _reportRepository.GetByIdWithDetailsAsync
                var reportEntity = await _reportRepository.GetByIdWithDetailsAsync(request.ReportId);

                // If report not found, return failure result with appropriate error code
                if (reportEntity == null)
                {
                    _logger.LogWarning($"Report not found with ReportId: {request.ReportId}");
                    return Result<ArchiveReportResponse>.Failure($"Report not found with ReportId: {request.ReportId}", ErrorCodes.Report.ReportNotFound);
                }

                // Verify that the report belongs to the requesting user
                if (!ValidateReportOwnership(reportEntity.UserId, userId))
                {
                    _logger.LogWarning($"User {userId} is not authorized to archive report {request.ReportId}");
                    return Result<ArchiveReportResponse>.Failure("Unauthorized access", ErrorCodes.General.Unauthorized);
                }

                // Archive the report using _reportRepository.ArchiveReportAsync
                bool archiveResult = await _reportRepository.ArchiveReportAsync(request.ReportId);

                // Create and return a success result with ArchiveReportResponse indicating report was archived
                var response = new ArchiveReportResponse { ReportId = request.ReportId, IsArchived = archiveResult };

                _logger.LogInformation($"Successfully archived report with ReportId: {request.ReportId}");
                return Result<ArchiveReportResponse>.Success(response);
            }
            catch (Exception ex)
            {
                // If any exceptions occur, log error and return failure result with appropriate error code
                _logger.LogError(ex, $"Error archiving report with ReportId: {request.ReportId}, UserId: {userId}: {ex.Message}");
                return Result<ArchiveReportResponse>.Failure($"Error archiving report: {ex.Message}", ErrorCodes.Report.ReportArchiveFailed);
            }
        }

        /// <summary>
        /// Unarchives a previously archived report to make it visible in regular report listings again
        /// </summary>
        /// <param name="reportId">ID of the report to unarchive</param>
        /// <param name="userId">ID of the user unarchiving the report</param>
        /// <returns>A result indicating success or failure of the unarchive operation</returns>
        public async Task<Result<ArchiveReportResponse>> UnarchiveReportAsync(string reportId, string userId)
        {
            // Validate parameters (reportId)
            if (string.IsNullOrEmpty(reportId))
            {
                return Result<ArchiveReportResponse>.Failure("ReportId is required", ErrorCodes.Report.InvalidReportParameters);
            }

            _logger.LogInformation($"Attempting to unarchive report with ReportId: {reportId}, UserId: {userId}");

            try
            {
                // Get report entity from repository using _reportRepository.GetByIdWithDetailsAsync
                var reportEntity = await _reportRepository.GetByIdWithDetailsAsync(reportId);

                // If report not found, return failure result with appropriate error code
                if (reportEntity == null)
                {
                    _logger.LogWarning($"Report not found with ReportId: {reportId}");
                    return Result<ArchiveReportResponse>.Failure($"Report not found with ReportId: {reportId}", ErrorCodes.Report.ReportNotFound);
                }

                // Verify that the report belongs to the requesting user
                if (!ValidateReportOwnership(reportEntity.UserId, userId))
                {
                    _logger.LogWarning($"User {userId} is not authorized to unarchive report {reportId}");
                    return Result<ArchiveReportResponse>.Failure("Unauthorized access", ErrorCodes.General.Unauthorized);
                }

                // Unarchive the report using _reportRepository.UnarchiveReportAsync
                bool unarchiveResult = await _reportRepository.UnarchiveReportAsync(reportId);

                // Create and return a success result with ArchiveReportResponse indicating report was unarchived
                var response = new ArchiveReportResponse { ReportId = reportId, IsArchived = !unarchiveResult };

                _logger.LogInformation($"Successfully unarchived report with ReportId: {reportId}");
                return Result<ArchiveReportResponse>.Success(response);
            }
            catch (Exception ex)
            {
                // If any exceptions occur, log error and return failure result with appropriate error code
                _logger.LogError(ex, $"Error unarchiving report with ReportId: {reportId}, UserId: {userId}: {ex.Message}");
                return Result<ArchiveReportResponse>.Failure($"Error unarchiving report: {ex.Message}", ErrorCodes.Report.ReportUnarchiveFailed);
            }
        }

        /// <summary>
        /// Permanently deletes a report and its associated file from storage
        /// </summary>
        /// <param name="reportId">ID of the report to delete</param>
        /// <param name="userId">ID of the user deleting the report</param>
        /// <returns>A result indicating success or failure of the delete operation</returns>
        public async Task<Result> DeleteReportAsync(string reportId, string userId)
        {
            // Validate parameters (reportId)
            if (string.IsNullOrEmpty(reportId))
            {
                return Result.Failure("ReportId is required", ErrorCodes.Report.InvalidReportParameters);
            }

            _logger.LogInformation($"Attempting to delete report with ReportId: {reportId}, UserId: {userId}");

            try
            {
                // Get report entity from repository using _reportRepository.GetByIdWithDetailsAsync
                var reportEntity = await _reportRepository.GetByIdWithDetailsAsync(reportId);

                // If report not found, return failure result with appropriate error code
                if (reportEntity == null)
                {
                    _logger.LogWarning($"Report not found with ReportId: {reportId}");
                    return Result.Failure($"Report not found with ReportId: {reportId}", ErrorCodes.Report.ReportNotFound);
                }

                // Verify that the report belongs to the requesting user
                if (!ValidateReportOwnership(reportEntity.UserId, userId))
                {
                    _logger.LogWarning($"User {userId} is not authorized to delete report {reportId}");
                    return Result.Failure("Unauthorized access", ErrorCodes.General.Unauthorized);
                }

                // Extract blob name from the report's storage URL
                string blobName = GetBlobNameFromUrl(reportEntity.StorageUrl);

                // Delete the report file from blob storage using _blobStorageClient.DeleteBlobAsync
                var deleteBlobResult = await _blobStorageClient.DeleteBlobAsync(blobName, _reportsContainerName);

                // Check if blob deletion was successful
                if (!deleteBlobResult.IsSuccess)
                {
                    _logger.LogError($"Failed to delete blob from storage: {deleteBlobResult.ErrorMessage}");
                    return Result.Failure($"Failed to delete blob from storage: {deleteBlobResult.ErrorMessage}", ErrorCodes.Report.ReportDeletionFailed);
                }

                // Delete the report entity from repository using _reportRepository.DeleteAsync
                bool deleteResult = await _reportRepository.DeleteAsync(reportId);

                _logger.LogInformation($"Successfully deleted report with ReportId: {reportId}");
                return Result.Success();
            }
            catch (Exception ex)
            {
                // If any exceptions occur, log error and return failure result with appropriate error code
                _logger.LogError(ex, $"Error deleting report with ReportId: {reportId}, UserId: {userId}: {ex.Message}");
                return Result.Failure($"Error deleting report: {ex.Message}", ErrorCodes.Report.ReportDeletionFailed);
            }
        }

        /// <summary>
        /// Validates that a report belongs to the specified user
        /// </summary>
        /// <param name="reportUserId">The user ID of the report owner</param>
        /// <param name="requestingUserId">The user ID of the user requesting access</param>
        /// <returns>True if the report belongs to the user, false otherwise</returns>
        private bool ValidateReportOwnership(string reportUserId, string requestingUserId)
        {
            // Compare reportUserId with requestingUserId
            return string.Equals(reportUserId, requestingUserId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Extracts the blob name from a storage URL
        /// </summary>
        /// <param name="storageUrl">The storage URL</param>
        /// <returns>The extracted blob name</returns>
        private string GetBlobNameFromUrl(string storageUrl)
        {
            // Parse the URL to extract the blob name
            Uri uri = new Uri(storageUrl);
            return uri.Segments.Last();
        }
    }
}