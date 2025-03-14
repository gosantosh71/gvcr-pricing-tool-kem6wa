using System; // Version: 6.0.0
using System.Collections.Generic; // Version: 6.0.0
using System.Net; // Version: 6.0.0
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Service.Models;

namespace VatFilingPricingTool.Api.Models.Responses
{
    /// <summary>
    /// API response model for report generation operations
    /// </summary>
    public class GenerateReportApiResponse
    {
        /// <summary>
        /// The data payload for the response
        /// </summary>
        public GenerateReportResponse Data { get; private set; }

        /// <summary>
        /// Private constructor to enforce factory method usage
        /// </summary>
        private GenerateReportApiResponse()
        {
            Data = null;
        }

        /// <summary>
        /// Creates a successful API response for report generation
        /// </summary>
        /// <param name="data">The report generation data</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report generation data</returns>
        public static ApiResponse<GenerateReportResponse> Success(GenerateReportResponse data, string message = null)
        {
            return ApiResponse<GenerateReportResponse>.CreateSuccess(data, message ?? "Report generation initiated successfully");
        }

        /// <summary>
        /// Creates an error API response for report generation
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>An error API response with the provided details</returns>
        public static ApiResponse<GenerateReportResponse> Error(string message, string errorCode = null, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return ApiResponse<GenerateReportResponse>.CreateError(message, errorCode, statusCode);
        }

        /// <summary>
        /// Creates a successful API response from a service ReportModel
        /// </summary>
        /// <param name="model">The service report model</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report data</returns>
        public static ApiResponse<GenerateReportResponse> FromServiceModel(ReportModel model, string message = null)
        {
            var response = GenerateReportResponse.FromReportModel(model);
            return Success(response, message);
        }
    }

    /// <summary>
    /// API response model for retrieving a specific report
    /// </summary>
    public class GetReportApiResponse
    {
        /// <summary>
        /// The data payload for the response
        /// </summary>
        public GetReportResponse Data { get; private set; }

        /// <summary>
        /// Private constructor to enforce factory method usage
        /// </summary>
        private GetReportApiResponse()
        {
            Data = null;
        }

        /// <summary>
        /// Creates a successful API response for report retrieval
        /// </summary>
        /// <param name="data">The report data</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report data</returns>
        public static ApiResponse<GetReportResponse> Success(GetReportResponse data, string message = null)
        {
            return ApiResponse<GetReportResponse>.CreateSuccess(data, message ?? "Report retrieved successfully");
        }

        /// <summary>
        /// Creates an error API response for report retrieval
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>An error API response with the provided details</returns>
        public static ApiResponse<GetReportResponse> Error(string message, string errorCode = null, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return ApiResponse<GetReportResponse>.CreateError(message, errorCode, statusCode);
        }

        /// <summary>
        /// Creates a successful API response from a service ReportModel
        /// </summary>
        /// <param name="model">The service report model</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report data</returns>
        public static ApiResponse<GetReportResponse> FromServiceModel(ReportModel model, string message = null)
        {
            var response = GetReportResponse.FromReportModel(model);
            return Success(response, message);
        }
    }

    /// <summary>
    /// API response model for retrieving a paginated list of reports
    /// </summary>
    public class GetReportHistoryApiResponse
    {
        /// <summary>
        /// The data payload for the response
        /// </summary>
        public GetReportHistoryResponse Data { get; private set; }

        /// <summary>
        /// Private constructor to enforce factory method usage
        /// </summary>
        private GetReportHistoryApiResponse()
        {
            Data = null;
        }

        /// <summary>
        /// Creates a successful API response for report history retrieval
        /// </summary>
        /// <param name="data">The report history data</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report history data</returns>
        public static ApiResponse<GetReportHistoryResponse> Success(GetReportHistoryResponse data, string message = null)
        {
            return ApiResponse<GetReportHistoryResponse>.CreateSuccess(data, message ?? "Report history retrieved successfully");
        }

        /// <summary>
        /// Creates an error API response for report history retrieval
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>An error API response with the provided details</returns>
        public static ApiResponse<GetReportHistoryResponse> Error(string message, string errorCode = null, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return ApiResponse<GetReportHistoryResponse>.CreateError(message, errorCode, statusCode);
        }

        /// <summary>
        /// Creates a successful API response from a paged list of service ReportModels
        /// </summary>
        /// <param name="pagedList">The paged list of service report models</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report history data</returns>
        public static ApiResponse<GetReportHistoryResponse> FromPagedServiceModels(PagedList<ReportModel> pagedList, string message = null)
        {
            var response = GetReportHistoryResponse.FromPagedList(pagedList);
            return Success(response, message);
        }
    }

    /// <summary>
    /// API response model for downloading a report
    /// </summary>
    public class DownloadReportApiResponse
    {
        /// <summary>
        /// The data payload for the response
        /// </summary>
        public DownloadReportResponse Data { get; private set; }

        /// <summary>
        /// Private constructor to enforce factory method usage
        /// </summary>
        private DownloadReportApiResponse()
        {
            Data = null;
        }

        /// <summary>
        /// Creates a successful API response for report download
        /// </summary>
        /// <param name="data">The report download data</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report download data</returns>
        public static ApiResponse<DownloadReportResponse> Success(DownloadReportResponse data, string message = null)
        {
            return ApiResponse<DownloadReportResponse>.CreateSuccess(data, message ?? "Report download prepared successfully");
        }

        /// <summary>
        /// Creates an error API response for report download
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>An error API response with the provided details</returns>
        public static ApiResponse<DownloadReportResponse> Error(string message, string errorCode = null, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return ApiResponse<DownloadReportResponse>.CreateError(message, errorCode, statusCode);
        }

        /// <summary>
        /// Creates a successful API response from a service ReportModel
        /// </summary>
        /// <param name="model">The service report model</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report download data</returns>
        public static ApiResponse<DownloadReportResponse> FromServiceModel(ReportModel model, string message = null)
        {
            var response = DownloadReportResponse.FromReportModel(model);
            return Success(response, message);
        }
    }

    /// <summary>
    /// API response model for emailing a report
    /// </summary>
    public class EmailReportApiResponse
    {
        /// <summary>
        /// The data payload for the response
        /// </summary>
        public EmailReportResponse Data { get; private set; }

        /// <summary>
        /// Private constructor to enforce factory method usage
        /// </summary>
        private EmailReportApiResponse()
        {
            Data = null;
        }

        /// <summary>
        /// Creates a successful API response for report emailing
        /// </summary>
        /// <param name="data">The report email data</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report email data</returns>
        public static ApiResponse<EmailReportResponse> Success(EmailReportResponse data, string message = null)
        {
            return ApiResponse<EmailReportResponse>.CreateSuccess(data, message ?? "Report emailed successfully");
        }

        /// <summary>
        /// Creates an error API response for report emailing
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>An error API response with the provided details</returns>
        public static ApiResponse<EmailReportResponse> Error(string message, string errorCode = null, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return ApiResponse<EmailReportResponse>.CreateError(message, errorCode, statusCode);
        }

        /// <summary>
        /// Creates a successful API response for report emailing
        /// </summary>
        /// <param name="reportId">The ID of the report that was emailed</param>
        /// <param name="emailAddress">The email address the report was sent to</param>
        /// <param name="emailSent">Whether the email was successfully sent</param>
        /// <param name="sentTime">The time the email was sent</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>A successful API response with the report email data</returns>
        public static ApiResponse<EmailReportResponse> Create(string reportId, string emailAddress, bool emailSent, DateTime sentTime, string message = null)
        {
            var response = new EmailReportResponse
            {
                ReportId = reportId,
                EmailAddress = emailAddress,
                EmailSent = emailSent,
                SentTime = sentTime
            };

            return Success(response, message);
        }
    }

    /// <summary>
    /// API response model for report management operations (archive, unarchive, delete)
    /// </summary>
    public class ReportManagementApiResponse
    {
        /// <summary>
        /// Private constructor to enforce factory method usage
        /// </summary>
        private ReportManagementApiResponse()
        {
        }

        /// <summary>
        /// Creates a successful API response for report management operations
        /// </summary>
        /// <param name="message">Custom success message</param>
        /// <returns>A successful API response with the provided message</returns>
        public static ApiResponse Success(string message)
        {
            return ApiResponse.CreateSuccess(message);
        }

        /// <summary>
        /// Creates an error API response for report management operations
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>An error API response with the provided details</returns>
        public static ApiResponse Error(string message, string errorCode = null, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return ApiResponse.CreateError(message, errorCode, statusCode);
        }
    }
}