using System; // Version: 6.0.0
using System.Collections.Generic; // Version: 6.0.0
using System.Linq; // Version: 6.0.0
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Common.Validation;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Validators
{
    /// <summary>
    /// Validates report-related request models to ensure they contain valid data before processing
    /// </summary>
    public class ReportRequestValidator
    {
        /// <summary>
        /// Validates a GenerateReportRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateGenerateReportRequest(GenerateReportRequest request)
        {
            var errors = new List<string>();

            // Validate CalculationId
            var calculationIdErrors = Validators.ValidateGuid(request.CalculationId, "CalculationId", true);
            if (calculationIdErrors.Any())
            {
                errors.AddRange(calculationIdErrors);
            }

            // Validate ReportTitle
            var reportTitleErrors = Validators.ValidateString(request.ReportTitle, "ReportTitle", 3, 100, true);
            if (reportTitleErrors.Any())
            {
                errors.AddRange(reportTitleErrors);
            }

            // Validate Format
            var formatErrors = Validators.ValidateEnum(request.Format, "Format");
            if (formatErrors.Any())
            {
                errors.AddRange(formatErrors);
            }

            // Validate email options if email delivery is selected
            if (request.DeliveryOptions != null && request.DeliveryOptions.SendEmail)
            {
                // Validate EmailAddress
                var emailErrors = Validators.ValidateEmail(request.DeliveryOptions.EmailAddress, "EmailAddress", true);
                if (emailErrors.Any())
                {
                    errors.AddRange(emailErrors);
                }

                // Validate EmailSubject
                var subjectErrors = Validators.ValidateString(request.DeliveryOptions.EmailSubject, "EmailSubject", 3, 100, true);
                if (subjectErrors.Any())
                {
                    errors.AddRange(subjectErrors);
                }
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.Report.InvalidReportParameters);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a GetReportRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateGetReportRequest(GetReportRequest request)
        {
            var errors = new List<string>();

            // Validate ReportId
            var reportIdErrors = Validators.ValidateGuid(request.ReportId, "ReportId", true);
            if (reportIdErrors.Any())
            {
                errors.AddRange(reportIdErrors);
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.Report.ReportNotFound);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a GetReportHistoryRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateGetReportHistoryRequest(GetReportHistoryRequest request)
        {
            var errors = new List<string>();

            // Validate StartDate if provided
            if (request.StartDate.HasValue)
            {
                var startDateErrors = Validators.ValidateDate(request.StartDate.Value, "StartDate");
                if (startDateErrors.Any())
                {
                    errors.AddRange(startDateErrors);
                }
            }

            // Validate EndDate if provided
            if (request.EndDate.HasValue)
            {
                var endDateErrors = Validators.ValidateDate(request.EndDate.Value, "EndDate");
                if (endDateErrors.Any())
                {
                    errors.AddRange(endDateErrors);
                }
            }

            // Validate date range if both dates are provided
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            {
                errors.Add("StartDate must be before or equal to EndDate");
            }

            // Validate ReportType if provided
            if (!string.IsNullOrEmpty(request.ReportType))
            {
                var reportTypeErrors = Validators.ValidateString(request.ReportType, "ReportType", 0, 50, false);
                if (reportTypeErrors.Any())
                {
                    errors.AddRange(reportTypeErrors);
                }
            }

            // Validate Format if provided
            if (request.Format.HasValue)
            {
                var formatErrors = Validators.ValidateEnum(request.Format.Value, "Format");
                if (formatErrors.Any())
                {
                    errors.AddRange(formatErrors);
                }
            }

            // Validate pagination parameters
            var pageErrors = Validators.ValidateInteger(request.Page, "Page", 1, 10000);
            if (pageErrors.Any())
            {
                errors.AddRange(pageErrors);
            }

            var pageSizeErrors = Validators.ValidateInteger(request.PageSize, "PageSize", 1, 100);
            if (pageSizeErrors.Any())
            {
                errors.AddRange(pageSizeErrors);
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.Report.InvalidReportParameters);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a DownloadReportRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateDownloadReportRequest(DownloadReportRequest request)
        {
            var errors = new List<string>();

            // Validate ReportId
            var reportIdErrors = Validators.ValidateGuid(request.ReportId, "ReportId", true);
            if (reportIdErrors.Any())
            {
                errors.AddRange(reportIdErrors);
            }

            // Validate Format if provided
            if (request.Format.HasValue)
            {
                var formatErrors = Validators.ValidateEnum(request.Format.Value, "Format");
                if (formatErrors.Any())
                {
                    errors.AddRange(formatErrors);
                }
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.Report.ReportDownloadFailed);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates an EmailReportRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateEmailReportRequest(EmailReportRequest request)
        {
            var errors = new List<string>();

            // Validate ReportId
            var reportIdErrors = Validators.ValidateGuid(request.ReportId, "ReportId", true);
            if (reportIdErrors.Any())
            {
                errors.AddRange(reportIdErrors);
            }

            // Validate EmailAddress
            var emailErrors = Validators.ValidateEmail(request.EmailAddress, "EmailAddress", true);
            if (emailErrors.Any())
            {
                errors.AddRange(emailErrors);
            }

            // Validate Subject
            var subjectErrors = Validators.ValidateString(request.Subject, "Subject", 3, 100, true);
            if (subjectErrors.Any())
            {
                errors.AddRange(subjectErrors);
            }

            // Validate Message (optional)
            var messageErrors = Validators.ValidateString(request.Message, "Message", 0, 1000, false);
            if (messageErrors.Any())
            {
                errors.AddRange(messageErrors);
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.Report.ReportEmailFailed);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates an ArchiveReportRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateArchiveReportRequest(ArchiveReportRequest request)
        {
            var errors = new List<string>();

            // Validate ReportId
            var reportIdErrors = Validators.ValidateGuid(request.ReportId, "ReportId", true);
            if (reportIdErrors.Any())
            {
                errors.AddRange(reportIdErrors);
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.Report.ReportArchiveFailed);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates an UnarchiveReportRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateUnarchiveReportRequest(UnarchiveReportRequest request)
        {
            var errors = new List<string>();

            // Validate ReportId
            var reportIdErrors = Validators.ValidateGuid(request.ReportId, "ReportId", true);
            if (reportIdErrors.Any())
            {
                errors.AddRange(reportIdErrors);
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.Report.ReportUnarchiveFailed);
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates a DeleteReportRequest object against business rules
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result indicating success or failure with error details</returns>
        public Result ValidateDeleteReportRequest(DeleteReportRequest request)
        {
            var errors = new List<string>();

            // Validate ReportId
            var reportIdErrors = Validators.ValidateGuid(request.ReportId, "ReportId", true);
            if (reportIdErrors.Any())
            {
                errors.AddRange(reportIdErrors);
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors, ErrorCodes.Report.ReportDeletionFailed);
            }

            return Result.Success();
        }
    }
}