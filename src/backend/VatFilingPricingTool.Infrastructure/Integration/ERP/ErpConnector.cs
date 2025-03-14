using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Microsoft.Extensions.Options; // Microsoft.Extensions.Options package version 6.0.0
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Common.Models.Result;
using VatFilingPricingTool.Domain.Entities;

namespace VatFilingPricingTool.Infrastructure.Integration.ERP
{
    /// <summary>
    /// Represents a single transaction record retrieved from an ERP system.
    /// </summary>
    public class TransactionRecord
    {
        /// <summary>
        /// Gets or sets the unique identifier for this transaction record.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the transaction number or reference code.
        /// </summary>
        public string TransactionNumber { get; set; }

        /// <summary>
        /// Gets or sets the date when the transaction occurred.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the transaction amount excluding tax.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the tax amount for this transaction.
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Gets or sets the currency code for this transaction (e.g., EUR, USD, GBP).
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the country code where the transaction occurred.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the type of transaction (e.g., Invoice, Credit Note, etc.).
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Gets or sets additional data related to this transaction that doesn't fit the standard schema.
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRecord"/> class.
        /// </summary>
        public TransactionRecord()
        {
            AdditionalData = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Contains a collection of transaction records retrieved from an ERP system.
    /// </summary>
    public class TransactionData
    {
        /// <summary>
        /// Gets or sets the list of transaction records.
        /// </summary>
        public List<TransactionRecord> Records { get; set; }

        /// <summary>
        /// Gets or sets the total count of records available in the source system.
        /// This may be greater than the Records count if pagination is used.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the entity type that was queried from the ERP system.
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the data was retrieved.
        /// </summary>
        public DateTime RetrievalDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionData"/> class.
        /// </summary>
        public TransactionData()
        {
            Records = new List<TransactionRecord>();
            RetrievalDate = DateTime.UtcNow;
            TotalCount = 0;
            EntityType = string.Empty;
        }
    }

    /// <summary>
    /// Parameters for importing data from an ERP system.
    /// </summary>
    public class ImportParameters
    {
        /// <summary>
        /// Gets or sets the start date for the data to be imported.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for the data to be imported.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the entity type to import (e.g., invoices, transactions, etc.).
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of records to import. If null, all available records will be imported.
        /// </summary>
        public int? MaxRecords { get; set; }

        /// <summary>
        /// Gets or sets additional parameters specific to the ERP system or entity type.
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportParameters"/> class.
        /// </summary>
        public ImportParameters()
        {
            AdditionalParameters = new Dictionary<string, object>();
            StartDate = DateTime.UtcNow.AddMonths(-1);
            EndDate = DateTime.UtcNow;
            EntityType = null;
            MaxRecords = null;
        }
    }

    /// <summary>
    /// Abstract base class for ERP system connectors providing common functionality and a standardized interface.
    /// Implemented by specific ERP system connectors like DynamicsConnector.
    /// </summary>
    public abstract class ErpConnector
    {
        /// <summary>
        /// Logger instance for logging operations.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// ERP configuration options.
        /// </summary>
        protected readonly ErpOptions _erpOptions;

        /// <summary>
        /// Gets a value indicating whether the connector is currently connected to the ERP system.
        /// </summary>
        public bool IsConnected { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErpConnector"/> class with required dependencies.
        /// </summary>
        /// <param name="logger">Logger for operation logging.</param>
        /// <param name="erpOptions">Configuration options for ERP connection.</param>
        /// <exception cref="ArgumentNullException">Thrown when logger or erpOptions is null.</exception>
        protected ErpConnector(ILogger logger, IOptions<ErpOptions> erpOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _erpOptions = erpOptions?.Value ?? throw new ArgumentNullException(nameof(erpOptions));
            IsConnected = false;
        }

        /// <summary>
        /// Establishes a connection to the ERP system.
        /// </summary>
        /// <returns>A result indicating success or failure of the connection attempt.</returns>
        public abstract Task<Result> ConnectAsync();

        /// <summary>
        /// Closes the connection to the ERP system.
        /// </summary>
        /// <returns>A result indicating success or failure of the disconnection attempt.</returns>
        public abstract Task<Result> DisconnectAsync();

        /// <summary>
        /// Tests the connection to the ERP system using the provided integration configuration.
        /// </summary>
        /// <param name="integration">Integration configuration to use for the test.</param>
        /// <returns>A result indicating success or failure of the connection test.</returns>
        public abstract Task<Result> TestConnectionAsync(Integration integration);

        /// <summary>
        /// Imports data from the ERP system based on the provided parameters.
        /// </summary>
        /// <param name="integration">Integration configuration to use for the import.</param>
        /// <param name="parameters">Parameters specifying what data to import.</param>
        /// <returns>A result containing the imported transaction data or error information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when integration or parameters is null.</exception>
        public virtual async Task<Result<TransactionData>> ImportDataAsync(Integration integration, ImportParameters parameters)
        {
            if (integration == null)
                throw new ArgumentNullException(nameof(integration));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            _logger.LogInformation("Starting import from ERP system. SystemType: {SystemType}, EntityType: {EntityType}, StartDate: {StartDate}, EndDate: {EndDate}",
                integration.SystemType, parameters.EntityType, parameters.StartDate, parameters.EndDate);

            return await GetTransactionDataAsync(
                parameters.StartDate,
                parameters.EndDate,
                parameters.EntityType,
                parameters.MaxRecords);
        }

        /// <summary>
        /// Retrieves transaction data from the ERP system based on specified parameters.
        /// </summary>
        /// <param name="startDate">The start date for the data to retrieve.</param>
        /// <param name="endDate">The end date for the data to retrieve.</param>
        /// <param name="entityType">The entity type to retrieve (e.g., invoices, transactions).</param>
        /// <param name="maxRecords">The maximum number of records to retrieve. If null, all available records will be retrieved.</param>
        /// <returns>A result containing the retrieved transaction data or error information.</returns>
        public abstract Task<Result<TransactionData>> GetTransactionDataAsync(
            DateTime startDate,
            DateTime endDate,
            string entityType,
            int? maxRecords);

        /// <summary>
        /// Validates that a connection to the ERP system is established.
        /// </summary>
        /// <returns>A result indicating whether the connection is valid.</returns>
        protected Result ValidateConnection()
        {
            if (!IsConnected)
            {
                return Result.Failure("Not connected to ERP system. Please connect before performing operations.");
            }

            return Result.Success();
        }

        /// <summary>
        /// Validates import parameters for data retrieval.
        /// </summary>
        /// <param name="startDate">The start date to validate.</param>
        /// <param name="endDate">The end date to validate.</param>
        /// <returns>A result indicating whether the parameters are valid.</returns>
        protected Result ValidateParameters(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return Result.Failure("Start date must be earlier than or equal to end date.");
            }

            // Check if date range is within reasonable limits (e.g., not more than 1 year)
            if ((endDate - startDate).TotalDays > 365)
            {
                return Result.Failure("Date range exceeds maximum allowed period (365 days).");
            }

            return Result.Success();
        }

        /// <summary>
        /// Logs an error with contextual information.
        /// </summary>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="operation">The operation that was being performed.</param>
        /// <param name="context">Additional context information about the operation.</param>
        protected void LogError(Exception ex, string operation, string context = null)
        {
            _logger.LogError(ex, "Error during ERP operation: {Operation}. Context: {Context}. Message: {Message}",
                operation, context ?? "None", ex.Message);
        }
    }
}