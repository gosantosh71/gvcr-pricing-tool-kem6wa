using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;

namespace VatFilingPricingTool.Service.Interfaces
{
    /// <summary>
    /// Interface for the pricing service that handles VAT filing cost calculations and related operations.
    /// This service is responsible for implementing the Dynamic Pricing Calculation Engine (F-202) and 
    /// Pricing Breakdown and Estimates (F-204) features, calculating accurate VAT filing costs based on
    /// multiple parameters including transaction volume, countries, service type, and filing frequency.
    /// </summary>
    public interface IPricingService
    {
        /// <summary>
        /// Calculates VAT filing costs based on the provided parameters.
        /// This method implements the core pricing calculation logic, applying country-specific VAT rules,
        /// volume-based pricing tiers, and additional service costs to generate a comprehensive cost estimate.
        /// </summary>
        /// <param name="request">
        /// The calculation request containing service type, transaction volume, filing frequency,
        /// selected countries, and additional services.
        /// </param>
        /// <returns>
        /// A result containing the detailed calculation response with total cost and country-specific
        /// breakdowns, or an error if the calculation could not be performed.
        /// </returns>
        Task<Result<CalculationResponse>> CalculatePricingAsync(CalculateRequest request);
        
        /// <summary>
        /// Retrieves a specific calculation by its ID.
        /// This allows users to access previously performed calculations for reference or comparison.
        /// </summary>
        /// <param name="request">
        /// The request containing the unique identifier of the calculation to retrieve.
        /// </param>
        /// <returns>
        /// A result containing the detailed calculation response with total cost and country-specific
        /// breakdowns, or an error if the calculation could not be found.
        /// </returns>
        Task<Result<CalculationResponse>> GetCalculationAsync(GetCalculationRequest request);
        
        /// <summary>
        /// Saves a calculation result for future reference.
        /// This enables users to store and retrieve calculation results over time, supporting
        /// historical analysis and reporting.
        /// </summary>
        /// <param name="request">
        /// The save calculation request containing the service type, transaction volume, filing frequency,
        /// total cost, and country-specific breakdowns to save.
        /// </param>
        /// <param name="userId">
        /// The ID of the user saving the calculation, used for access control and auditing.
        /// </param>
        /// <returns>
        /// A result containing the save operation response with the generated calculation ID and timestamp,
        /// or an error if the calculation could not be saved.
        /// </returns>
        Task<Result<SaveCalculationResponse>> SaveCalculationAsync(SaveCalculationRequest request, string userId);
        
        /// <summary>
        /// Retrieves calculation history for a specific user with optional filtering.
        /// This supports historical analysis of VAT filing costs over time, with filtering by date range,
        /// countries, and service type.
        /// </summary>
        /// <param name="request">
        /// The request containing filtering parameters (date range, countries, service type) and
        /// pagination settings (page number, page size).
        /// </param>
        /// <param name="userId">
        /// The ID of the user whose calculation history to retrieve, used for access control.
        /// </param>
        /// <returns>
        /// A result containing a paginated calculation history response with summary information for
        /// each calculation, or an error if the history could not be retrieved.
        /// </returns>
        Task<Result<CalculationHistoryResponse>> GetCalculationHistoryAsync(GetCalculationHistoryRequest request, string userId);
        
        /// <summary>
        /// Compares multiple calculation scenarios to help users identify the most cost-effective options.
        /// This allows users to evaluate different service configurations, transaction volumes, or country
        /// combinations to optimize their VAT filing costs.
        /// </summary>
        /// <param name="request">
        /// The request containing multiple calculation scenarios to compare, each with its own service type,
        /// transaction volume, filing frequency, countries, and additional services.
        /// </param>
        /// <returns>
        /// A result containing a detailed comparison of the scenarios with cost differences and potential
        /// savings highlighted, or an error if the comparison could not be performed.
        /// </returns>
        Task<Result<CalculationComparisonResponse>> CompareCalculationsAsync(CompareCalculationsRequest request);
        
        /// <summary>
        /// Deletes a specific calculation by its ID.
        /// This allows users to remove calculations that are no longer needed or were created in error.
        /// </summary>
        /// <param name="calculationId">
        /// The unique identifier of the calculation to delete.
        /// </param>
        /// <param name="userId">
        /// The ID of the user requesting the deletion, used for access control and verification.
        /// </param>
        /// <returns>
        /// A result indicating success or failure of the delete operation, with an error message and code
        /// if the operation failed.
        /// </returns>
        Task<Result> DeleteCalculationAsync(string calculationId, string userId);
    }
}