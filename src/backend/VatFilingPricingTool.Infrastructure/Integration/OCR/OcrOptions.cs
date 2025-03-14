using System;
using System.Collections.Generic;

namespace VatFilingPricingTool.Infrastructure.Integration.OCR
{
    /// <summary>
    /// Configuration options for OCR processing using Azure Cognitive Services
    /// </summary>
    public class OcrOptions
    {
        /// <summary>
        /// The endpoint URL for the Azure Cognitive Services API
        /// </summary>
        public string ApiEndpoint { get; set; }

        /// <summary>
        /// The authentication key for accessing the Azure Cognitive Services API
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The Azure region where the Cognitive Services resource is deployed
        /// </summary>
        public string ResourceRegion { get; set; }

        /// <summary>
        /// The version of the Azure Cognitive Services API being used
        /// </summary>
        public string ServiceVersion { get; set; }

        /// <summary>
        /// Timeout in seconds for API connections
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; }

        /// <summary>
        /// Maximum number of retry attempts for failed API calls
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// Minimum confidence threshold for accepting OCR results (0.0 to 1.0)
        /// </summary>
        public double MinimumConfidenceScore { get; set; }

        /// <summary>
        /// Flag to enable/disable Azure Form Recognizer service for document processing
        /// </summary>
        public bool EnableFormRecognizer { get; set; }

        /// <summary>
        /// Flag to enable/disable Azure Computer Vision OCR service for basic text extraction
        /// </summary>
        public bool EnableComputerVision { get; set; }

        /// <summary>
        /// Flag to indicate if OCR processing should be done asynchronously in the background
        /// </summary>
        public bool UseBackgroundProcessing { get; set; }

        /// <summary>
        /// Array of supported file extensions for OCR processing
        /// </summary>
        public string[] SupportedFileTypes { get; set; }

        /// <summary>
        /// Nested dictionary for mapping document fields 
        /// First key: Document type (e.g., "Invoice", "VATReturn")
        /// Second key: Source field name in OCR result
        /// Value: Target field name in application model
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> FieldMappings { get; set; }

        /// <summary>
        /// Dictionary mapping document types to their corresponding custom model IDs
        /// </summary>
        public Dictionary<string, string> ModelIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the OcrOptions class with default values
        /// </summary>
        public OcrOptions()
        {
            // Set default service version to latest stable
            ServiceVersion = "2023-07-31";
            
            // Set reasonable default timeouts and retry settings
            ConnectionTimeoutSeconds = 30;
            MaxRetryCount = 3;
            MinimumConfidenceScore = 0.6;
            
            // Enable both OCR services by default
            EnableFormRecognizer = true;
            EnableComputerVision = true;
            UseBackgroundProcessing = false;
            
            // Initialize supported file types with common formats
            SupportedFileTypes = new string[] 
            {
                ".pdf", 
                ".jpg", 
                ".jpeg", 
                ".png", 
                ".tiff", 
                ".tif", 
                ".bmp"
            };
            
            // Initialize field mappings dictionary
            FieldMappings = new Dictionary<string, Dictionary<string, string>>();
            
            // Add default field mappings for invoices
            var invoiceFieldMappings = new Dictionary<string, string>
            {
                { "InvoiceId", "InvoiceNumber" },
                { "InvoiceDate", "InvoiceDate" },
                { "DueDate", "DueDate" },
                { "VendorName", "SupplierName" },
                { "VendorAddress", "SupplierAddress" },
                { "VendorTaxId", "SupplierVatNumber" },
                { "CustomerName", "CustomerName" },
                { "CustomerAddress", "CustomerAddress" },
                { "CustomerTaxId", "CustomerVatNumber" },
                { "InvoiceTotal", "TotalAmount" },
                { "TaxAmount", "VatAmount" }
            };
            FieldMappings.Add("Invoice", invoiceFieldMappings);
            
            // Add default field mappings for VAT returns
            var vatReturnFieldMappings = new Dictionary<string, string>
            {
                { "ReturnPeriod", "FilingPeriod" },
                { "ReturnDueDate", "DueDate" },
                { "VatRegistrationNumber", "VatNumber" },
                { "Box1", "SalesVatAmount" },
                { "Box2", "PurchasesVatAmount" },
                { "Box3", "VatDueAmount" },
                { "Box4", "ReclaimableVatAmount" },
                { "Box5", "NetVatAmount" },
                { "Box6", "TotalSalesAmount" },
                { "Box7", "TotalPurchasesAmount" },
                { "Box8", "TotalEUSalesAmount" },
                { "Box9", "TotalEUPurchasesAmount" }
            };
            FieldMappings.Add("VATReturn", vatReturnFieldMappings);
            
            // Initialize model IDs dictionary
            ModelIds = new Dictionary<string, string>();
            
            // Add default model IDs (these would be replaced with actual custom model IDs in production)
            ModelIds.Add("Invoice", "prebuilt-invoice");
            ModelIds.Add("VATReturn", "custom-vatreturn-model");
        }
    }
}