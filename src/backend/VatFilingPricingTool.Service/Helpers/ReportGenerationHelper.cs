using System; // Version: 6.0.0
using System.IO; // Version: 6.0.0
using System.Text; // Version: 6.0.0
using System.Collections.Generic; // Version: 6.0.0
using System.Linq; // Version: 6.0.0
using System.Threading.Tasks; // Version: 6.0.0
using System.Globalization;
using System.Text.RegularExpressions;
using PDFsharp; // Version: 1.50.5147
using PDFsharp.Drawing;
using PDFsharp.Pdf;
using PDFsharp.Pdf.IO;
using OfficeOpenXml; // Version: 5.8.7
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing.Chart;
using CsvHelper; // Version: 27.2.1
using CsvHelper.Configuration;
using VatFilingPricingTool.Service.Models;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Service.Helpers
{
    /// <summary>
    /// Helper class that provides functionality for generating VAT filing reports in various formats
    /// </summary>
    public static class ReportGenerationHelper
    {
        /// <summary>
        /// Private constructor to prevent instantiation as this is a static helper class
        /// </summary>
        private ReportGenerationHelper()
        {
            // Private constructor to enforce static usage pattern
        }

        /// <summary>
        /// Generates a report in the specified format based on the provided report model
        /// </summary>
        /// <param name="reportModel">The report model containing data and settings for the report</param>
        /// <returns>The generated report as a byte array</returns>
        public static byte[] GenerateReport(ReportModel reportModel)
        {
            ValidateReportModel(reportModel);
            
            switch (reportModel.Format)
            {
                case ReportFormat.PDF:
                    return GeneratePdfReport(reportModel);
                case ReportFormat.Excel:
                    return GenerateExcelReport(reportModel);
                case ReportFormat.CSV:
                    return GenerateCsvReport(reportModel);
                case ReportFormat.HTML:
                    return GenerateHtmlReport(reportModel);
                default:
                    throw new ValidationException(
                        $"Unsupported report format: {reportModel.Format}", 
                        new List<string> { $"The format {reportModel.Format} is not supported" },
                        ErrorCodes.Report.InvalidReportFormat);
            }
        }

        /// <summary>
        /// Generates a PDF report based on the provided report model
        /// </summary>
        /// <param name="reportModel">The report model containing data and settings for the report</param>
        /// <returns>The generated PDF report as a byte array</returns>
        private static byte[] GeneratePdfReport(ReportModel reportModel)
        {
            // Create a new PDF document
            using (var document = new PdfDocument())
            {
                // Add title page
                PdfPage titlePage = document.AddPage();
                using (XGraphics gfx = XGraphics.FromPdfPage(titlePage))
                {
                    // Add report title
                    XFont titleFont = new XFont("Arial", 24, XFontStyle.Bold);
                    gfx.DrawString(reportModel.ReportTitle, titleFont, XBrushes.Black, 
                        new XRect(0, 100, titlePage.Width, 50), XStringFormats.Center);
                    
                    // Add generation date
                    XFont dateFont = new XFont("Arial", 12, XFontStyle.Regular);
                    string dateText = $"Generated on: {reportModel.GenerationDate:yyyy-MM-dd HH:mm:ss}";
                    gfx.DrawString(dateText, dateFont, XBrushes.Black, 
                        new XRect(0, 150, titlePage.Width, 30), XStringFormats.Center);
                    
                    // Add calculation overview subtitle
                    XFont subtitleFont = new XFont("Arial", 18, XFontStyle.Bold);
                    gfx.DrawString("VAT Filing Cost Estimate", subtitleFont, XBrushes.Black, 
                        new XRect(0, 200, titlePage.Width, 40), XStringFormats.Center);
                }
                
                // Add summary page
                PdfPage summaryPage = document.AddPage();
                using (XGraphics gfx = XGraphics.FromPdfPage(summaryPage))
                {
                    XFont headerFont = new XFont("Arial", 16, XFontStyle.Bold);
                    XFont normalFont = new XFont("Arial", 12, XFontStyle.Regular);
                    XFont boldFont = new XFont("Arial", 12, XFontStyle.Bold);
                    
                    // Add page title
                    gfx.DrawString("Calculation Summary", headerFont, XBrushes.Black, 
                        new XRect(50, 50, summaryPage.Width - 100, 30), XStringFormats.TopLeft);
                    
                    int yPosition = 100;
                    int lineHeight = 25;
                    
                    // Service type
                    gfx.DrawString("Service Type:", boldFont, XBrushes.Black, 
                        new XRect(50, yPosition, 200, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(GetServiceTypeDescription(reportModel.CalculationData.ServiceType), 
                        normalFont, XBrushes.Black, 
                        new XRect(250, yPosition, summaryPage.Width - 300, lineHeight), XStringFormats.TopLeft);
                    yPosition += lineHeight;
                    
                    // Transaction volume
                    gfx.DrawString("Transaction Volume:", boldFont, XBrushes.Black, 
                        new XRect(50, yPosition, 200, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(reportModel.CalculationData.TransactionVolume.ToString("N0"), 
                        normalFont, XBrushes.Black, 
                        new XRect(250, yPosition, summaryPage.Width - 300, lineHeight), XStringFormats.TopLeft);
                    yPosition += lineHeight;
                    
                    // Filing frequency
                    gfx.DrawString("Filing Frequency:", boldFont, XBrushes.Black, 
                        new XRect(50, yPosition, 200, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(GetFilingFrequencyDescription(reportModel.CalculationData.Frequency), 
                        normalFont, XBrushes.Black, 
                        new XRect(250, yPosition, summaryPage.Width - 300, lineHeight), XStringFormats.TopLeft);
                    yPosition += lineHeight;
                    
                    // Total cost
                    gfx.DrawString("Total Cost:", boldFont, XBrushes.Black, 
                        new XRect(50, yPosition, 200, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(FormatCurrency(reportModel.CalculationData.TotalCost.Amount, 
                        reportModel.CalculationData.CurrencyCode), 
                        new XFont("Arial", 12, XFontStyle.Bold), XBrushes.Black, 
                        new XRect(250, yPosition, summaryPage.Width - 300, lineHeight), XStringFormats.TopLeft);
                    yPosition += lineHeight * 2;
                    
                    // Number of countries
                    gfx.DrawString("Number of Countries:", boldFont, XBrushes.Black, 
                        new XRect(50, yPosition, 200, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(reportModel.CalculationData.CountryBreakdowns.Count.ToString(), 
                        normalFont, XBrushes.Black, 
                        new XRect(250, yPosition, summaryPage.Width - 300, lineHeight), XStringFormats.TopLeft);
                    yPosition += lineHeight;
                    
                    // Calculation date
                    gfx.DrawString("Calculation Date:", boldFont, XBrushes.Black, 
                        new XRect(50, yPosition, 200, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(reportModel.CalculationData.CalculationDate.ToString("yyyy-MM-dd HH:mm:ss"), 
                        normalFont, XBrushes.Black, 
                        new XRect(250, yPosition, summaryPage.Width - 300, lineHeight), XStringFormats.TopLeft);
                    yPosition += lineHeight * 2;
                    
                    // Add additional information about the report
                    gfx.DrawString("This report provides an estimate of VAT filing costs based on the selected parameters.", 
                        normalFont, XBrushes.Black, 
                        new XRect(50, yPosition, summaryPage.Width - 100, lineHeight * 2), XStringFormats.TopLeft);
                }
                
                // Add country breakdown section if requested
                if (reportModel.IncludeCountryBreakdown && 
                    reportModel.CalculationData.CountryBreakdowns != null && 
                    reportModel.CalculationData.CountryBreakdowns.Any())
                {
                    PdfPage countryPage = document.AddPage();
                    using (XGraphics gfx = XGraphics.FromPdfPage(countryPage))
                    {
                        XFont headerFont = new XFont("Arial", 16, XFontStyle.Bold);
                        XFont normalFont = new XFont("Arial", 12, XFontStyle.Regular);
                        XFont boldFont = new XFont("Arial", 12, XFontStyle.Bold);
                        
                        // Add page title
                        gfx.DrawString("Country Breakdown", headerFont, XBrushes.Black, 
                            new XRect(50, 50, countryPage.Width - 100, 30), XStringFormats.TopLeft);
                        
                        int yPosition = 100;
                        int lineHeight = 25;
                        
                        // Draw table header
                        string[] columnHeaders = new[] { "Country", "Base Cost", "Additional Cost", "Total Cost" };
                        int[] columnWidths = new[] { 150, 100, 120, 100 };
                        
                        for (int i = 0; i < columnHeaders.Length; i++)
                        {
                            double colX = 50;
                            for (int j = 0; j < i; j++)
                            {
                                colX += columnWidths[j];
                            }
                            
                            gfx.DrawString(columnHeaders[i], boldFont, XBrushes.Black, 
                                new XRect(colX, yPosition, columnWidths[i], 25), XStringFormats.TopLeft);
                        }
                        yPosition += lineHeight;
                        
                        // Draw horizontal line
                        XPen pen = new XPen(XColors.Black, 1);
                        gfx.DrawLine(pen, 50, yPosition - 5, countryPage.Width - 100, yPosition - 5);
                        
                        // Draw table rows
                        foreach (var country in reportModel.CalculationData.CountryBreakdowns)
                        {
                            string[] rowData = new[] 
                            { 
                                country.CountryName, 
                                FormatCurrency(country.BaseCost.Amount, reportModel.CalculationData.CurrencyCode),
                                FormatCurrency(country.AdditionalCost.Amount, reportModel.CalculationData.CurrencyCode),
                                FormatCurrency(country.TotalCost.Amount, reportModel.CalculationData.CurrencyCode)
                            };
                            
                            for (int i = 0; i < rowData.Length; i++)
                            {
                                double colX = 50;
                                for (int j = 0; j < i; j++)
                                {
                                    colX += columnWidths[j];
                                }
                                
                                gfx.DrawString(rowData[i], normalFont, XBrushes.Black, 
                                    new XRect(colX, yPosition, columnWidths[i], 25), XStringFormats.TopLeft);
                            }
                            yPosition += lineHeight;
                            
                            // Check if we need a new page
                            if (yPosition > countryPage.Height - 100)
                            {
                                countryPage = document.AddPage();
                                gfx = XGraphics.FromPdfPage(countryPage);
                                yPosition = 100;
                                
                                // Add continued header
                                gfx.DrawString("Country Breakdown (Continued)", headerFont, XBrushes.Black, 
                                    new XRect(50, 50, countryPage.Width - 100, 30), XStringFormats.TopLeft);
                                
                                // Redraw table header
                                for (int i = 0; i < columnHeaders.Length; i++)
                                {
                                    double colX = 50;
                                    for (int j = 0; j < i; j++)
                                    {
                                        colX += columnWidths[j];
                                    }
                                    
                                    gfx.DrawString(columnHeaders[i], boldFont, XBrushes.Black, 
                                        new XRect(colX, yPosition, columnWidths[i], 25), XStringFormats.TopLeft);
                                }
                                yPosition += lineHeight;
                                
                                // Draw horizontal line
                                gfx.DrawLine(pen, 50, yPosition - 5, countryPage.Width - 100, yPosition - 5);
                            }
                        }
                    }
                }
                
                // Add service details section if requested
                if (reportModel.IncludeServiceDetails)
                {
                    PdfPage servicePage = document.AddPage();
                    using (XGraphics gfx = XGraphics.FromPdfPage(servicePage))
                    {
                        XFont headerFont = new XFont("Arial", 16, XFontStyle.Bold);
                        XFont normalFont = new XFont("Arial", 12, XFontStyle.Regular);
                        XFont boldFont = new XFont("Arial", 12, XFontStyle.Bold);
                        
                        // Add page title
                        gfx.DrawString("Service Details", headerFont, XBrushes.Black, 
                            new XRect(50, 50, servicePage.Width - 100, 30), XStringFormats.TopLeft);
                        
                        int yPosition = 100;
                        int lineHeight = 25;
                        
                        // Service description based on type
                        gfx.DrawString(GetServiceTypeDescription(reportModel.CalculationData.ServiceType) + " - Description:", 
                            boldFont, XBrushes.Black, 
                            new XRect(50, yPosition, servicePage.Width - 100, lineHeight), XStringFormats.TopLeft);
                        yPosition += lineHeight;
                        
                        // Add service description
                        string serviceDescription = GetServiceTypeDescription(reportModel.CalculationData.ServiceType);
                        switch (reportModel.CalculationData.ServiceType)
                        {
                            case ServiceType.StandardFiling:
                                serviceDescription = "Standard VAT Filing service includes basic processing of your VAT returns " +
                                    "with standard validation checks. This service is suitable for businesses with " +
                                    "straightforward VAT requirements and a moderate transaction volume.";
                                break;
                            case ServiceType.ComplexFiling:
                                serviceDescription = "Complex VAT Filing service is designed for businesses with sophisticated tax " +
                                    "situations. It includes enhanced validation, reconciliation services, and " +
                                    "handling of special VAT scenarios such as partial exemption, margin schemes, " +
                                    "and cross-border transactions.";
                                break;
                            case ServiceType.PriorityService:
                                serviceDescription = "Priority VAT Filing service offers all the features of Complex Filing with " +
                                    "expedited processing times and priority support. This premium service includes " +
                                    "dedicated account management, advanced validation, and accelerated filing to " +
                                    "ensure your VAT returns are processed with the highest priority.";
                                break;
                        }
                        
                        gfx.DrawString(serviceDescription, normalFont, XBrushes.Black, 
                            new XRect(50, yPosition, servicePage.Width - 100, lineHeight * 3), XStringFormats.TopLeft);
                        yPosition += lineHeight * 4;
                        
                        // Additional services
                        if (reportModel.CalculationData.AdditionalServices != null && 
                            reportModel.CalculationData.AdditionalServices.Any())
                        {
                            gfx.DrawString("Additional Services Included:", boldFont, XBrushes.Black, 
                                new XRect(50, yPosition, servicePage.Width - 100, lineHeight), XStringFormats.TopLeft);
                            yPosition += lineHeight;
                            
                            foreach (var service in reportModel.CalculationData.AdditionalServices)
                            {
                                gfx.DrawString("• " + service, normalFont, XBrushes.Black, 
                                    new XRect(70, yPosition, servicePage.Width - 120, lineHeight), XStringFormats.TopLeft);
                                yPosition += lineHeight;
                            }
                        }
                        else
                        {
                            gfx.DrawString("No additional services included.", normalFont, XBrushes.Black, 
                                new XRect(50, yPosition, servicePage.Width - 100, lineHeight), XStringFormats.TopLeft);
                            yPosition += lineHeight;
                        }
                    }
                }
                
                // Add discounts section if requested
                if (reportModel.IncludeAppliedDiscounts && 
                    reportModel.CalculationData.Discounts != null && 
                    reportModel.CalculationData.Discounts.Any())
                {
                    PdfPage discountPage = document.AddPage();
                    using (XGraphics gfx = XGraphics.FromPdfPage(discountPage))
                    {
                        XFont headerFont = new XFont("Arial", 16, XFontStyle.Bold);
                        XFont normalFont = new XFont("Arial", 12, XFontStyle.Regular);
                        XFont boldFont = new XFont("Arial", 12, XFontStyle.Bold);
                        
                        // Add page title
                        gfx.DrawString("Applied Discounts", headerFont, XBrushes.Black, 
                            new XRect(50, 50, discountPage.Width - 100, 30), XStringFormats.TopLeft);
                        
                        int yPosition = 100;
                        int lineHeight = 25;
                        
                        // Draw table header
                        string[] columnHeaders = new[] { "Discount Type", "Percentage", "Amount" };
                        int[] columnWidths = new[] { 200, 100, 120 };
                        
                        for (int i = 0; i < columnHeaders.Length; i++)
                        {
                            double colX = 50;
                            for (int j = 0; j < i; j++)
                            {
                                colX += columnWidths[j];
                            }
                            
                            gfx.DrawString(columnHeaders[i], boldFont, XBrushes.Black, 
                                new XRect(colX, yPosition, columnWidths[i], 25), XStringFormats.TopLeft);
                        }
                        yPosition += lineHeight;
                        
                        // Draw horizontal line
                        XPen pen = new XPen(XColors.Black, 1);
                        gfx.DrawLine(pen, 50, yPosition - 5, discountPage.Width - 100, yPosition - 5);
                        
                        // Calculate original cost (before discounts)
                        decimal originalCost = reportModel.CalculationData.TotalCost.Amount;
                        decimal totalDiscountAmount = 0;
                        
                        foreach (var discount in reportModel.CalculationData.Discounts)
                        {
                            // Calculate discount amount (this is simplified and would need to be adjusted based on actual discount logic)
                            decimal discountAmount = originalCost * (discount.Value / 100);
                            totalDiscountAmount += discountAmount;
                            
                            string[] rowData = new[] 
                            { 
                                discount.Key, 
                                discount.Value.ToString("0.##") + "%",
                                FormatCurrency(discountAmount, reportModel.CalculationData.CurrencyCode)
                            };
                            
                            for (int i = 0; i < rowData.Length; i++)
                            {
                                double colX = 50;
                                for (int j = 0; j < i; j++)
                                {
                                    colX += columnWidths[j];
                                }
                                
                                gfx.DrawString(rowData[i], normalFont, XBrushes.Black, 
                                    new XRect(colX, yPosition, columnWidths[i], 25), XStringFormats.TopLeft);
                            }
                            yPosition += lineHeight;
                        }
                        
                        // Draw horizontal line
                        gfx.DrawLine(pen, 50, yPosition - 5, discountPage.Width - 100, yPosition - 5);
                        
                        // Draw total discount
                        string[] totalRow = new[] 
                        { 
                            "Total Discounts", 
                            "",
                            FormatCurrency(totalDiscountAmount, reportModel.CalculationData.CurrencyCode)
                        };
                        
                        for (int i = 0; i < totalRow.Length; i++)
                        {
                            double colX = 50;
                            for (int j = 0; j < i; j++)
                            {
                                colX += columnWidths[j];
                            }
                            
                            gfx.DrawString(totalRow[i], boldFont, XBrushes.Black, 
                                new XRect(colX, yPosition, columnWidths[i], 25), XStringFormats.TopLeft);
                        }
                    }
                }
                
                // Add tax rate details if requested
                if (reportModel.IncludeTaxRateDetails)
                {
                    PdfPage taxRatesPage = document.AddPage();
                    using (XGraphics gfx = XGraphics.FromPdfPage(taxRatesPage))
                    {
                        XFont headerFont = new XFont("Arial", 16, XFontStyle.Bold);
                        XFont normalFont = new XFont("Arial", 12, XFontStyle.Regular);
                        XFont boldFont = new XFont("Arial", 12, XFontStyle.Bold);
                        
                        // Add page title
                        gfx.DrawString("Tax Rate Details", headerFont, XBrushes.Black, 
                            new XRect(50, 50, taxRatesPage.Width - 100, 30), XStringFormats.TopLeft);
                        
                        int yPosition = 100;
                        int lineHeight = 25;
                        
                        gfx.DrawString("Current VAT rates for selected countries:", normalFont, XBrushes.Black, 
                            new XRect(50, yPosition, taxRatesPage.Width - 100, lineHeight), XStringFormats.TopLeft);
                        yPosition += lineHeight * 2;
                        
                        // Draw table header
                        string[] columnHeaders = new[] { "Country", "Standard VAT Rate", "Filing Requirements" };
                        int[] columnWidths = new[] { 150, 120, 250 };
                        
                        for (int i = 0; i < columnHeaders.Length; i++)
                        {
                            double colX = 50;
                            for (int j = 0; j < i; j++)
                            {
                                colX += columnWidths[j];
                            }
                            
                            gfx.DrawString(columnHeaders[i], boldFont, XBrushes.Black, 
                                new XRect(colX, yPosition, columnWidths[i], 25), XStringFormats.TopLeft);
                        }
                        yPosition += lineHeight;
                        
                        // Draw horizontal line
                        XPen pen = new XPen(XColors.Black, 1);
                        gfx.DrawLine(pen, 50, yPosition - 5, taxRatesPage.Width - 100, yPosition - 5);
                        
                        // Add country tax rates (simplified with hard-coded data)
                        foreach (var country in reportModel.CalculationData.CountryBreakdowns)
                        {
                            string vatRate = GetVatRateForCountry(country.CountryCode);
                            string filingReqs = GetFilingRequirementsForCountry(country.CountryCode, reportModel.CalculationData.Frequency);
                            
                            string[] rowData = new[] { country.CountryName, vatRate, filingReqs };
                            
                            for (int i = 0; i < rowData.Length; i++)
                            {
                                double colX = 50;
                                for (int j = 0; j < i; j++)
                                {
                                    colX += columnWidths[j];
                                }
                                
                                gfx.DrawString(rowData[i], normalFont, XBrushes.Black, 
                                    new XRect(colX, yPosition, columnWidths[i], 25), XStringFormats.TopLeft);
                            }
                            yPosition += lineHeight;
                            
                            // Check if we need a new page
                            if (yPosition > taxRatesPage.Height - 100)
                            {
                                taxRatesPage = document.AddPage();
                                gfx = XGraphics.FromPdfPage(taxRatesPage);
                                yPosition = 100;
                                
                                // Add continued header
                                gfx.DrawString("Tax Rate Details (Continued)", headerFont, XBrushes.Black, 
                                    new XRect(50, 50, taxRatesPage.Width - 100, 30), XStringFormats.TopLeft);
                                
                                // Redraw table header
                                for (int i = 0; i < columnHeaders.Length; i++)
                                {
                                    double colX = 50;
                                    for (int j = 0; j < i; j++)
                                    {
                                        colX += columnWidths[j];
                                    }
                                    
                                    gfx.DrawString(columnHeaders[i], boldFont, XBrushes.Black, 
                                        new XRect(colX, yPosition, columnWidths[i], 25), XStringFormats.TopLeft);
                                }
                                yPosition += lineHeight;
                                
                                // Draw horizontal line
                                gfx.DrawLine(pen, 50, yPosition - 5, taxRatesPage.Width - 100, yPosition - 5);
                            }
                        }
                        
                        yPosition += lineHeight;
                        gfx.DrawString("Note: VAT rates and filing requirements may change. This information is current as of " + 
                            DateTime.UtcNow.ToString("yyyy-MM-dd") + ".", 
                            new XFont("Arial", 10, XFontStyle.Italic), XBrushes.Black, 
                            new XRect(50, yPosition, taxRatesPage.Width - 100, lineHeight * 2), XStringFormats.TopLeft);
                    }
                }
                
                // Add footer to all pages
                for (int i = 0; i < document.PageCount; i++)
                {
                    PdfPage page = document.Pages[i];
                    using (XGraphics gfx = XGraphics.FromPdfPage(page))
                    {
                        XFont footerFont = new XFont("Arial", 8, XFontStyle.Regular);
                        
                        // Add page number
                        string pageNumber = $"Page {i + 1} of {document.PageCount}";
                        gfx.DrawString(pageNumber, footerFont, XBrushes.Black, 
                            new XRect(0, page.Height - 20, page.Width, 20), XStringFormats.Center);
                        
                        // Add generation info
                        string generatedBy = $"Generated by VAT Filing Pricing Tool on {reportModel.GenerationDate:yyyy-MM-dd HH:mm:ss}";
                        gfx.DrawString(generatedBy, footerFont, XBrushes.Black, 
                            new XRect(50, page.Height - 20, page.Width - 100, 20), XStringFormats.Left);
                    }
                }
                
                // Save the document to a memory stream
                using (MemoryStream stream = new MemoryStream())
                {
                    document.Save(stream, false);
                    return stream.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Generates an Excel report based on the provided report model
        /// </summary>
        /// <param name="reportModel">The report model containing data and settings for the report</param>
        /// <returns>The generated Excel report as a byte array</returns>
        private static byte[] GenerateExcelReport(ReportModel reportModel)
        {
            // Enable LicenseContext for non-commercial use
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                // Create the Summary worksheet
                var summaryWorksheet = package.Workbook.Worksheets.Add("Summary");
                
                // Add title
                summaryWorksheet.Cells[1, 1].Value = reportModel.ReportTitle;
                summaryWorksheet.Cells[1, 1, 1, 5].Merge = true;
                summaryWorksheet.Cells[1, 1].Style.Font.Size = 16;
                summaryWorksheet.Cells[1, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Add generation date
                summaryWorksheet.Cells[2, 1].Value = $"Generated on: {reportModel.GenerationDate:yyyy-MM-dd HH:mm:ss}";
                summaryWorksheet.Cells[2, 1, 2, 5].Merge = true;
                summaryWorksheet.Cells[2, 1].Style.Font.Size = 12;
                summaryWorksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Add calculation summary header
                summaryWorksheet.Cells[4, 1].Value = "Calculation Summary";
                summaryWorksheet.Cells[4, 1, 4, 5].Merge = true;
                summaryWorksheet.Cells[4, 1].Style.Font.Size = 14;
                summaryWorksheet.Cells[4, 1].Style.Font.Bold = true;
                
                // Add calculation details
                int row = 6;
                
                // Service type
                summaryWorksheet.Cells[row, 1].Value = "Service Type:";
                summaryWorksheet.Cells[row, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[row, 2].Value = GetServiceTypeDescription(reportModel.CalculationData.ServiceType);
                row++;
                
                // Transaction volume
                summaryWorksheet.Cells[row, 1].Value = "Transaction Volume:";
                summaryWorksheet.Cells[row, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[row, 2].Value = reportModel.CalculationData.TransactionVolume;
                row++;
                
                // Filing frequency
                summaryWorksheet.Cells[row, 1].Value = "Filing Frequency:";
                summaryWorksheet.Cells[row, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[row, 2].Value = GetFilingFrequencyDescription(reportModel.CalculationData.Frequency);
                row++;
                
                // Total cost
                summaryWorksheet.Cells[row, 1].Value = "Total Cost:";
                summaryWorksheet.Cells[row, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[row, 2].Value = reportModel.CalculationData.TotalCost.Amount;
                summaryWorksheet.Cells[row, 2].Style.Numberformat.Format = GetCurrencyFormat(reportModel.CalculationData.CurrencyCode);
                summaryWorksheet.Cells[row, 2].Style.Font.Bold = true;
                row++;
                
                // Number of countries
                summaryWorksheet.Cells[row, 1].Value = "Number of Countries:";
                summaryWorksheet.Cells[row, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[row, 2].Value = reportModel.CalculationData.CountryBreakdowns.Count;
                row++;
                
                // Calculation date
                summaryWorksheet.Cells[row, 1].Value = "Calculation Date:";
                summaryWorksheet.Cells[row, 1].Style.Font.Bold = true;
                summaryWorksheet.Cells[row, 2].Value = reportModel.CalculationData.CalculationDate;
                summaryWorksheet.Cells[row, 2].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                row += 2;
                
                // Add note
                summaryWorksheet.Cells[row, 1].Value = "This report provides an estimate of VAT filing costs based on the selected parameters.";
                summaryWorksheet.Cells[row, 1, row, 5].Merge = true;
                row += 2;
                
                // Format columns
                summaryWorksheet.Column(1).Width = 25;
                summaryWorksheet.Column(2).Width = 30;
                summaryWorksheet.Column(3).Width = 20;
                summaryWorksheet.Column(4).Width = 20;
                summaryWorksheet.Column(5).Width = 20;
                
                // Add country breakdown worksheet if requested
                if (reportModel.IncludeCountryBreakdown && 
                    reportModel.CalculationData.CountryBreakdowns != null && 
                    reportModel.CalculationData.CountryBreakdowns.Any())
                {
                    var countryWorksheet = package.Workbook.Worksheets.Add("Country Breakdown");
                    
                    // Add header row
                    countryWorksheet.Cells[1, 1].Value = "Country";
                    countryWorksheet.Cells[1, 2].Value = "Base Cost";
                    countryWorksheet.Cells[1, 3].Value = "Additional Cost";
                    countryWorksheet.Cells[1, 4].Value = "Total Cost";
                    
                    // Style header row
                    var headerRange = countryWorksheet.Cells[1, 1, 1, 4];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    
                    // Add data rows
                    row = 2;
                    foreach (var country in reportModel.CalculationData.CountryBreakdowns)
                    {
                        countryWorksheet.Cells[row, 1].Value = country.CountryName;
                        countryWorksheet.Cells[row, 2].Value = country.BaseCost.Amount;
                        countryWorksheet.Cells[row, 3].Value = country.AdditionalCost.Amount;
                        countryWorksheet.Cells[row, 4].Value = country.TotalCost.Amount;
                        
                        // Format currency cells
                        string currencyFormat = GetCurrencyFormat(reportModel.CalculationData.CurrencyCode);
                        countryWorksheet.Cells[row, 2].Style.Numberformat.Format = currencyFormat;
                        countryWorksheet.Cells[row, 3].Style.Numberformat.Format = currencyFormat;
                        countryWorksheet.Cells[row, 4].Style.Numberformat.Format = currencyFormat;
                        
                        row++;
                    }
                    
                    // Add totals row
                    countryWorksheet.Cells[row, 1].Value = "Total";
                    countryWorksheet.Cells[row, 1].Style.Font.Bold = true;
                    
                    countryWorksheet.Cells[row, 2].Formula = $"SUM(B2:B{row-1})";
                    countryWorksheet.Cells[row, 3].Formula = $"SUM(C2:C{row-1})";
                    countryWorksheet.Cells[row, 4].Formula = $"SUM(D2:D{row-1})";
                    
                    // Format totals row
                    var totalsRange = countryWorksheet.Cells[row, 1, row, 4];
                    totalsRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    totalsRange.Style.Font.Bold = true;
                    
                    string currencyFormat = GetCurrencyFormat(reportModel.CalculationData.CurrencyCode);
                    countryWorksheet.Cells[row, 2].Style.Numberformat.Format = currencyFormat;
                    countryWorksheet.Cells[row, 3].Style.Numberformat.Format = currencyFormat;
                    countryWorksheet.Cells[row, 4].Style.Numberformat.Format = currencyFormat;
                    
                    // Format columns
                    countryWorksheet.Column(1).Width = 25;
                    countryWorksheet.Column(2).Width = 20;
                    countryWorksheet.Column(3).Width = 20;
                    countryWorksheet.Column(4).Width = 20;
                    
                    // Add a chart
                    var chart = countryWorksheet.Drawings.AddChart("CountryChart", eChartType.ColumnClustered);
                    chart.Title.Text = "VAT Filing Costs by Country";
                    chart.SetPosition(row + 2, 0, 0, 0);
                    chart.SetSize(800, 400);
                    var series = chart.Series.Add($"D2:D{row-1}", $"A2:A{row-1}");
                    series.Header = "Total Cost";
                }
                
                // Add service details worksheet if requested
                if (reportModel.IncludeServiceDetails)
                {
                    var serviceWorksheet = package.Workbook.Worksheets.Add("Service Details");
                    
                    // Add service details header
                    serviceWorksheet.Cells[1, 1].Value = "Service Details";
                    serviceWorksheet.Cells[1, 1].Style.Font.Size = 14;
                    serviceWorksheet.Cells[1, 1].Style.Font.Bold = true;
                    
                    row = 3;
                    
                    // Service type
                    serviceWorksheet.Cells[row, 1].Value = "Service Type:";
                    serviceWorksheet.Cells[row, 1].Style.Font.Bold = true;
                    serviceWorksheet.Cells[row, 2].Value = GetServiceTypeDescription(reportModel.CalculationData.ServiceType);
                    row++;
                    
                    // Service description
                    serviceWorksheet.Cells[row, 1].Value = "Description:";
                    serviceWorksheet.Cells[row, 1].Style.Font.Bold = true;
                    string serviceDescription = "";
                    switch (reportModel.CalculationData.ServiceType)
                    {
                        case ServiceType.StandardFiling:
                            serviceDescription = "Standard VAT Filing service includes basic processing of your VAT returns " +
                                "with standard validation checks. This service is suitable for businesses with " +
                                "straightforward VAT requirements and a moderate transaction volume.";
                            break;
                        case ServiceType.ComplexFiling:
                            serviceDescription = "Complex VAT Filing service is designed for businesses with sophisticated tax " +
                                "situations. It includes enhanced validation, reconciliation services, and " +
                                "handling of special VAT scenarios such as partial exemption, margin schemes, " +
                                "and cross-border transactions.";
                            break;
                        case ServiceType.PriorityService:
                            serviceDescription = "Priority VAT Filing service offers all the features of Complex Filing with " +
                                "expedited processing times and priority support. This premium service includes " +
                                "dedicated account management, advanced validation, and accelerated filing to " +
                                "ensure your VAT returns are processed with the highest priority.";
                            break;
                    }
                    serviceWorksheet.Cells[row, 2].Value = serviceDescription;
                    row += 2;
                    
                    // Additional services
                    serviceWorksheet.Cells[row, 1].Value = "Additional Services:";
                    serviceWorksheet.Cells[row, 1].Style.Font.Bold = true;
                    row++;
                    
                    if (reportModel.CalculationData.AdditionalServices != null && 
                        reportModel.CalculationData.AdditionalServices.Any())
                    {
                        foreach (var service in reportModel.CalculationData.AdditionalServices)
                        {
                            serviceWorksheet.Cells[row, 2].Value = service;
                            row++;
                        }
                    }
                    else
                    {
                        serviceWorksheet.Cells[row, 2].Value = "No additional services included";
                        row++;
                    }
                    
                    // Format columns
                    serviceWorksheet.Column(1).Width = 25;
                    serviceWorksheet.Column(2).Width = 50;
                }
                
                // Add discounts worksheet if requested
                if (reportModel.IncludeAppliedDiscounts && 
                    reportModel.CalculationData.Discounts != null && 
                    reportModel.CalculationData.Discounts.Any())
                {
                    var discountWorksheet = package.Workbook.Worksheets.Add("Discounts");
                    
                    // Add header row
                    discountWorksheet.Cells[1, 1].Value = "Discount Type";
                    discountWorksheet.Cells[1, 2].Value = "Percentage";
                    discountWorksheet.Cells[1, 3].Value = "Amount";
                    
                    // Style header row
                    var headerRange = discountWorksheet.Cells[1, 1, 1, 3];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    
                    // Add data rows
                    row = 2;
                    decimal originalCost = reportModel.CalculationData.TotalCost.Amount;
                    decimal totalDiscountAmount = 0;
                    
                    foreach (var discount in reportModel.CalculationData.Discounts)
                    {
                        // Calculate discount amount (this is simplified)
                        decimal discountAmount = originalCost * (discount.Value / 100);
                        totalDiscountAmount += discountAmount;
                        
                        discountWorksheet.Cells[row, 1].Value = discount.Key;
                        discountWorksheet.Cells[row, 2].Value = discount.Value / 100; // Store as decimal for formatting
                        discountWorksheet.Cells[row, 2].Style.Numberformat.Format = "0.00%";
                        discountWorksheet.Cells[row, 3].Value = discountAmount;
                        discountWorksheet.Cells[row, 3].Style.Numberformat.Format = GetCurrencyFormat(reportModel.CalculationData.CurrencyCode);
                        
                        row++;
                    }
                    
                    // Add totals row
                    discountWorksheet.Cells[row, 1].Value = "Total Discounts";
                    discountWorksheet.Cells[row, 1].Style.Font.Bold = true;
                    discountWorksheet.Cells[row, 3].Value = totalDiscountAmount;
                    discountWorksheet.Cells[row, 3].Style.Numberformat.Format = GetCurrencyFormat(reportModel.CalculationData.CurrencyCode);
                    discountWorksheet.Cells[row, 3].Style.Font.Bold = true;
                    
                    // Format totals row
                    var totalsRange = discountWorksheet.Cells[row, 1, row, 3];
                    totalsRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    
                    // Format columns
                    discountWorksheet.Column(1).Width = 30;
                    discountWorksheet.Column(2).Width = 15;
                    discountWorksheet.Column(3).Width = 20;
                    
                    // Add a chart
                    var chart = discountWorksheet.Drawings.AddChart("DiscountChart", eChartType.Pie);
                    chart.Title.Text = "Discount Breakdown";
                    chart.SetPosition(row + 2, 0, 0, 0);
                    chart.SetSize(500, 300);
                    var series = chart.Series.Add($"C2:C{row-1}", $"A2:A{row-1}");
                    series.Header = "Discount Amount";
                }
                
                // Add tax rate details worksheet if requested
                if (reportModel.IncludeTaxRateDetails)
                {
                    var taxRatesWorksheet = package.Workbook.Worksheets.Add("Tax Rates");
                    
                    // Add header row
                    taxRatesWorksheet.Cells[1, 1].Value = "Country";
                    taxRatesWorksheet.Cells[1, 2].Value = "Standard VAT Rate";
                    taxRatesWorksheet.Cells[1, 3].Value = "Filing Requirements";
                    
                    // Style header row
                    var headerRange = taxRatesWorksheet.Cells[1, 1, 1, 3];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    
                    // Add data rows
                    row = 2;
                    foreach (var country in reportModel.CalculationData.CountryBreakdowns)
                    {
                        string vatRate = GetVatRateForCountry(country.CountryCode);
                        string filingRequirements = GetFilingRequirementsForCountry(country.CountryCode, reportModel.CalculationData.Frequency);
                        
                        taxRatesWorksheet.Cells[row, 1].Value = country.CountryName;
                        taxRatesWorksheet.Cells[row, 2].Value = vatRate;
                        taxRatesWorksheet.Cells[row, 3].Value = filingRequirements;
                        
                        row++;
                    }
                    
                    // Add disclaimer note
                    row += 2;
                    taxRatesWorksheet.Cells[row, 1].Value = $"Note: VAT rates and filing requirements may change. This information is current as of {DateTime.UtcNow:yyyy-MM-dd}.";
                    taxRatesWorksheet.Cells[row, 1, row, 3].Merge = true;
                    taxRatesWorksheet.Cells[row, 1].Style.Font.Italic = true;
                    
                    // Format columns
                    taxRatesWorksheet.Column(1).Width = 25;
                    taxRatesWorksheet.Column(2).Width = 20;
                    taxRatesWorksheet.Column(3).Width = 50;
                }
                
                // Save the Excel package to a memory stream
                using (MemoryStream stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        /// <summary>
        /// Generates a CSV report based on the provided report model
        /// </summary>
        /// <param name="reportModel">The report model containing data and settings for the report</param>
        /// <returns>The generated CSV report as a byte array</returns>
        private static byte[] GenerateCsvReport(ReportModel reportModel)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // Write report metadata
                    csv.WriteField("Report Title");
                    csv.WriteField(reportModel.ReportTitle);
                    csv.NextRecord();
                    
                    csv.WriteField("Generated On");
                    csv.WriteField(reportModel.GenerationDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    csv.NextRecord();
                    
                    csv.WriteField("Calculation ID");
                    csv.WriteField(reportModel.CalculationId);
                    csv.NextRecord();
                    csv.NextRecord();
                    
                    // Write calculation summary
                    csv.WriteField("CALCULATION SUMMARY");
                    csv.NextRecord();
                    
                    csv.WriteField("Service Type");
                    csv.WriteField(GetServiceTypeDescription(reportModel.CalculationData.ServiceType));
                    csv.NextRecord();
                    
                    csv.WriteField("Transaction Volume");
                    csv.WriteField(reportModel.CalculationData.TransactionVolume.ToString());
                    csv.NextRecord();
                    
                    csv.WriteField("Filing Frequency");
                    csv.WriteField(GetFilingFrequencyDescription(reportModel.CalculationData.Frequency));
                    csv.NextRecord();
                    
                    csv.WriteField("Total Cost");
                    csv.WriteField(FormatCurrency(reportModel.CalculationData.TotalCost.Amount, reportModel.CalculationData.CurrencyCode));
                    csv.NextRecord();
                    
                    csv.WriteField("Calculation Date");
                    csv.WriteField(reportModel.CalculationData.CalculationDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    csv.NextRecord();
                    
                    csv.WriteField("Number of Countries");
                    csv.WriteField(reportModel.CalculationData.CountryBreakdowns.Count.ToString());
                    csv.NextRecord();
                    csv.NextRecord();
                    
                    // Write country breakdown if requested
                    if (reportModel.IncludeCountryBreakdown && 
                        reportModel.CalculationData.CountryBreakdowns != null && 
                        reportModel.CalculationData.CountryBreakdowns.Any())
                    {
                        csv.WriteField("COUNTRY BREAKDOWN");
                        csv.NextRecord();
                        
                        // Write header row
                        csv.WriteField("Country");
                        csv.WriteField("Base Cost");
                        csv.WriteField("Additional Cost");
                        csv.WriteField("Total Cost");
                        csv.NextRecord();
                        
                        // Write data rows
                        foreach (var country in reportModel.CalculationData.CountryBreakdowns)
                        {
                            csv.WriteField(country.CountryName);
                            csv.WriteField(FormatCurrency(country.BaseCost.Amount, reportModel.CalculationData.CurrencyCode));
                            csv.WriteField(FormatCurrency(country.AdditionalCost.Amount, reportModel.CalculationData.CurrencyCode));
                            csv.WriteField(FormatCurrency(country.TotalCost.Amount, reportModel.CalculationData.CurrencyCode));
                            csv.NextRecord();
                        }
                        
                        csv.NextRecord();
                    }
                    
                    // Write service details if requested
                    if (reportModel.IncludeServiceDetails)
                    {
                        csv.WriteField("SERVICE DETAILS");
                        csv.NextRecord();
                        
                        csv.WriteField("Service Description");
                        string serviceDescription = "";
                        switch (reportModel.CalculationData.ServiceType)
                        {
                            case ServiceType.StandardFiling:
                                serviceDescription = "Standard VAT Filing service includes basic processing of your VAT returns with standard validation checks.";
                                break;
                            case ServiceType.ComplexFiling:
                                serviceDescription = "Complex VAT Filing service includes enhanced validation, reconciliation services, and handling of special VAT scenarios.";
                                break;
                            case ServiceType.PriorityService:
                                serviceDescription = "Priority VAT Filing service offers expedited processing times and priority support with dedicated account management.";
                                break;
                        }
                        csv.WriteField(serviceDescription);
                        csv.NextRecord();
                        
                        // Write additional services
                        csv.WriteField("Additional Services");
                        csv.NextRecord();
                        
                        if (reportModel.CalculationData.AdditionalServices != null && 
                            reportModel.CalculationData.AdditionalServices.Any())
                        {
                            foreach (var service in reportModel.CalculationData.AdditionalServices)
                            {
                                csv.WriteField(service);
                                csv.NextRecord();
                            }
                        }
                        else
                        {
                            csv.WriteField("No additional services included");
                            csv.NextRecord();
                        }
                        
                        csv.NextRecord();
                    }
                    
                    // Write discounts if requested
                    if (reportModel.IncludeAppliedDiscounts && 
                        reportModel.CalculationData.Discounts != null && 
                        reportModel.CalculationData.Discounts.Any())
                    {
                        csv.WriteField("APPLIED DISCOUNTS");
                        csv.NextRecord();
                        
                        // Write header row
                        csv.WriteField("Discount Type");
                        csv.WriteField("Percentage");
                        csv.WriteField("Amount");
                        csv.NextRecord();
                        
                        // Calculate original cost (before discounts)
                        decimal originalCost = reportModel.CalculationData.TotalCost.Amount;
                        decimal totalDiscountAmount = 0;
                        
                        // Write data rows
                        foreach (var discount in reportModel.CalculationData.Discounts)
                        {
                            // Calculate discount amount (simplified)
                            decimal discountAmount = originalCost * (discount.Value / 100);
                            totalDiscountAmount += discountAmount;
                            
                            csv.WriteField(discount.Key);
                            csv.WriteField(discount.Value.ToString("0.##") + "%");
                            csv.WriteField(FormatCurrency(discountAmount, reportModel.CalculationData.CurrencyCode));
                            csv.NextRecord();
                        }
                        
                        // Write total
                        csv.WriteField("Total Discounts");
                        csv.WriteField("");
                        csv.WriteField(FormatCurrency(totalDiscountAmount, reportModel.CalculationData.CurrencyCode));
                        csv.NextRecord();
                        
                        csv.NextRecord();
                    }
                    
                    // Write tax rate details if requested
                    if (reportModel.IncludeTaxRateDetails)
                    {
                        csv.WriteField("TAX RATE DETAILS");
                        csv.NextRecord();
                        
                        // Write header row
                        csv.WriteField("Country");
                        csv.WriteField("Standard VAT Rate");
                        csv.WriteField("Filing Requirements");
                        csv.NextRecord();
                        
                        // Write data rows
                        foreach (var country in reportModel.CalculationData.CountryBreakdowns)
                        {
                            string vatRate = GetVatRateForCountry(country.CountryCode);
                            string filingRequirements = GetFilingRequirementsForCountry(country.CountryCode, reportModel.CalculationData.Frequency);
                            
                            csv.WriteField(country.CountryName);
                            csv.WriteField(vatRate);
                            csv.WriteField(filingRequirements);
                            csv.NextRecord();
                        }
                        
                        csv.NextRecord();
                        csv.WriteField($"Note: VAT rates and filing requirements may change. This information is current as of {DateTime.UtcNow:yyyy-MM-dd}.");
                        csv.NextRecord();
                    }
                    
                    // Ensure all data is written to the stream
                    writer.Flush();
                    return stream.ToArray();
                }
            }
        }

        /// <summary>
        /// Generates an HTML report based on the provided report model
        /// </summary>
        /// <param name="reportModel">The report model containing data and settings for the report</param>
        /// <returns>The generated HTML report as a byte array</returns>
        private static byte[] GenerateHtmlReport(ReportModel reportModel)
        {
            StringBuilder html = new StringBuilder();
            
            // Add HTML header
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine($"    <title>{reportModel.ReportTitle}</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; color: #333; line-height: 1.6; }");
            html.AppendLine("        h1, h2, h3 { color: #2c3e50; }");
            html.AppendLine("        h1 { text-align: center; margin-bottom: 10px; }");
            html.AppendLine("        h2 { margin-top: 30px; padding-bottom: 10px; border-bottom: 1px solid #eee; }");
            html.AppendLine("        .generation-info { text-align: center; color: #7f8c8d; margin-bottom: 30px; }");
            html.AppendLine("        .summary-item { margin-bottom: 10px; }");
            html.AppendLine("        .summary-item .label { font-weight: bold; display: inline-block; width: 200px; }");
            html.AppendLine("        .summary-item .value { display: inline-block; }");
            html.AppendLine("        .total-cost { font-weight: bold; color: #2980b9; }");
            html.AppendLine("        table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("        th { background-color: #f2f2f2; text-align: left; padding: 12px; }");
            html.AppendLine("        td { padding: 12px; border-bottom: 1px solid #ddd; }");
            html.AppendLine("        tr:hover { background-color: #f5f5f5; }");
            html.AppendLine("        .footer { margin-top: 50px; padding-top: 20px; border-top: 1px solid #eee; color: #7f8c8d; font-size: 0.9em; text-align: center; }");
            html.AppendLine("        .note { font-style: italic; color: #7f8c8d; margin: 15px 0; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Add header section
            html.AppendLine($"    <h1>{reportModel.ReportTitle}</h1>");
            html.AppendLine($"    <div class=\"generation-info\">Generated on: {reportModel.GenerationDate:yyyy-MM-dd HH:mm:ss}</div>");
            
            // Add summary section
            html.AppendLine("    <h2>Calculation Summary</h2>");
            html.AppendLine("    <div class=\"summary-section\">");
            html.AppendLine("        <div class=\"summary-item\">");
            html.AppendLine("            <span class=\"label\">Service Type:</span>");
            html.AppendLine($"            <span class=\"value\">{GetServiceTypeDescription(reportModel.CalculationData.ServiceType)}</span>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"summary-item\">");
            html.AppendLine("            <span class=\"label\">Transaction Volume:</span>");
            html.AppendLine($"            <span class=\"value\">{reportModel.CalculationData.TransactionVolume:N0}</span>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"summary-item\">");
            html.AppendLine("            <span class=\"label\">Filing Frequency:</span>");
            html.AppendLine($"            <span class=\"value\">{GetFilingFrequencyDescription(reportModel.CalculationData.Frequency)}</span>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"summary-item\">");
            html.AppendLine("            <span class=\"label\">Total Cost:</span>");
            html.AppendLine($"            <span class=\"value total-cost\">{FormatCurrency(reportModel.CalculationData.TotalCost.Amount, reportModel.CalculationData.CurrencyCode)}</span>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"summary-item\">");
            html.AppendLine("            <span class=\"label\">Number of Countries:</span>");
            html.AppendLine($"            <span class=\"value\">{reportModel.CalculationData.CountryBreakdowns.Count}</span>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"summary-item\">");
            html.AppendLine("            <span class=\"label\">Calculation Date:</span>");
            html.AppendLine($"            <span class=\"value\">{reportModel.CalculationData.CalculationDate:yyyy-MM-dd HH:mm:ss}</span>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("    <div class=\"note\">This report provides an estimate of VAT filing costs based on the selected parameters.</div>");
            
            // Add country breakdown section if requested
            if (reportModel.IncludeCountryBreakdown && 
                reportModel.CalculationData.CountryBreakdowns != null && 
                reportModel.CalculationData.CountryBreakdowns.Any())
            {
                html.AppendLine("    <h2>Country Breakdown</h2>");
                html.AppendLine("    <table>");
                html.AppendLine("        <thead>");
                html.AppendLine("            <tr>");
                html.AppendLine("                <th>Country</th>");
                html.AppendLine("                <th>Base Cost</th>");
                html.AppendLine("                <th>Additional Cost</th>");
                html.AppendLine("                <th>Total Cost</th>");
                html.AppendLine("            </tr>");
                html.AppendLine("        </thead>");
                html.AppendLine("        <tbody>");
                
                foreach (var country in reportModel.CalculationData.CountryBreakdowns)
                {
                    html.AppendLine("            <tr>");
                    html.AppendLine($"                <td>{country.CountryName}</td>");
                    html.AppendLine($"                <td>{FormatCurrency(country.BaseCost.Amount, reportModel.CalculationData.CurrencyCode)}</td>");
                    html.AppendLine($"                <td>{FormatCurrency(country.AdditionalCost.Amount, reportModel.CalculationData.CurrencyCode)}</td>");
                    html.AppendLine($"                <td class=\"total-cost\">{FormatCurrency(country.TotalCost.Amount, reportModel.CalculationData.CurrencyCode)}</td>");
                    html.AppendLine("            </tr>");
                }
                
                html.AppendLine("        </tbody>");
                html.AppendLine("    </table>");
            }
            
            // Add service details section if requested
            if (reportModel.IncludeServiceDetails)
            {
                html.AppendLine("    <h2>Service Details</h2>");
                html.AppendLine($"    <h3>{GetServiceTypeDescription(reportModel.CalculationData.ServiceType)}</h3>");
                
                // Add service description
                string serviceDescription = "";
                switch (reportModel.CalculationData.ServiceType)
                {
                    case ServiceType.StandardFiling:
                        serviceDescription = "Standard VAT Filing service includes basic processing of your VAT returns " +
                            "with standard validation checks. This service is suitable for businesses with " +
                            "straightforward VAT requirements and a moderate transaction volume.";
                        break;
                    case ServiceType.ComplexFiling:
                        serviceDescription = "Complex VAT Filing service is designed for businesses with sophisticated tax " +
                            "situations. It includes enhanced validation, reconciliation services, and " +
                            "handling of special VAT scenarios such as partial exemption, margin schemes, " +
                            "and cross-border transactions.";
                        break;
                    case ServiceType.PriorityService:
                        serviceDescription = "Priority VAT Filing service offers all the features of Complex Filing with " +
                            "expedited processing times and priority support. This premium service includes " +
                            "dedicated account management, advanced validation, and accelerated filing to " +
                            "ensure your VAT returns are processed with the highest priority.";
                        break;
                }
                html.AppendLine($"    <p>{serviceDescription}</p>");
                
                html.AppendLine("    <h3>Additional Services</h3>");
                if (reportModel.CalculationData.AdditionalServices != null && 
                    reportModel.CalculationData.AdditionalServices.Any())
                {
                    html.AppendLine("    <ul>");
                    foreach (var service in reportModel.CalculationData.AdditionalServices)
                    {
                        html.AppendLine($"        <li>{service}</li>");
                    }
                    html.AppendLine("    </ul>");
                }
                else
                {
                    html.AppendLine("    <p>No additional services included.</p>");
                }
            }
            
            // Add discounts section if requested
            if (reportModel.IncludeAppliedDiscounts && 
                reportModel.CalculationData.Discounts != null && 
                reportModel.CalculationData.Discounts.Any())
            {
                html.AppendLine("    <h2>Applied Discounts</h2>");
                html.AppendLine("    <table>");
                html.AppendLine("        <thead>");
                html.AppendLine("            <tr>");
                html.AppendLine("                <th>Discount Type</th>");
                html.AppendLine("                <th>Percentage</th>");
                html.AppendLine("                <th>Amount</th>");
                html.AppendLine("            </tr>");
                html.AppendLine("        </thead>");
                html.AppendLine("        <tbody>");
                
                // Calculate original cost (before discounts)
                decimal originalCost = reportModel.CalculationData.TotalCost.Amount;
                decimal totalDiscountAmount = 0;
                
                foreach (var discount in reportModel.CalculationData.Discounts)
                {
                    // Calculate discount amount (simplified)
                    decimal discountAmount = originalCost * (discount.Value / 100);
                    totalDiscountAmount += discountAmount;
                    
                    html.AppendLine("            <tr>");
                    html.AppendLine($"                <td>{discount.Key}</td>");
                    html.AppendLine($"                <td>{discount.Value:0.##}%</td>");
                    html.AppendLine($"                <td>{FormatCurrency(discountAmount, reportModel.CalculationData.CurrencyCode)}</td>");
                    html.AppendLine("            </tr>");
                }
                
                html.AppendLine("            <tr>");
                html.AppendLine("                <td><strong>Total Discounts</strong></td>");
                html.AppendLine("                <td></td>");
                html.AppendLine($"                <td><strong>{FormatCurrency(totalDiscountAmount, reportModel.CalculationData.CurrencyCode)}</strong></td>");
                html.AppendLine("            </tr>");
                
                html.AppendLine("        </tbody>");
                html.AppendLine("    </table>");
            }
            
            // Add tax rate details if requested
            if (reportModel.IncludeTaxRateDetails)
            {
                html.AppendLine("    <h2>Tax Rate Details</h2>");
                html.AppendLine("    <table>");
                html.AppendLine("        <thead>");
                html.AppendLine("            <tr>");
                html.AppendLine("                <th>Country</th>");
                html.AppendLine("                <th>Standard VAT Rate</th>");
                html.AppendLine("                <th>Filing Requirements</th>");
                html.AppendLine("            </tr>");
                html.AppendLine("        </thead>");
                html.AppendLine("        <tbody>");
                
                foreach (var country in reportModel.CalculationData.CountryBreakdowns)
                {
                    string vatRate = GetVatRateForCountry(country.CountryCode);
                    string filingRequirements = GetFilingRequirementsForCountry(country.CountryCode, reportModel.CalculationData.Frequency);
                    
                    html.AppendLine("            <tr>");
                    html.AppendLine($"                <td>{country.CountryName}</td>");
                    html.AppendLine($"                <td>{vatRate}</td>");
                    html.AppendLine($"                <td>{filingRequirements}</td>");
                    html.AppendLine("            </tr>");
                }
                
                html.AppendLine("        </tbody>");
                html.AppendLine("    </table>");
                html.AppendLine($"    <div class=\"note\">Note: VAT rates and filing requirements may change. This information is current as of {DateTime.UtcNow:yyyy-MM-dd}.</div>");
            }
            
            // Add footer
            html.AppendLine("    <div class=\"footer\">");
            html.AppendLine($"        Generated by VAT Filing Pricing Tool on {reportModel.GenerationDate:yyyy-MM-dd HH:mm:ss}");
            html.AppendLine("    </div>");
            
            // Add closing tags
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            // Convert to byte array
            return Encoding.UTF8.GetBytes(html.ToString());
        }

        /// <summary>
        /// Converts a report from one format to another
        /// </summary>
        /// <param name="reportContent">The original report content as a byte array</param>
        /// <param name="sourceFormat">The format of the original report</param>
        /// <param name="targetFormat">The desired output format</param>
        /// <returns>The converted report as a byte array</returns>
        public static byte[] ConvertReportFormat(byte[] reportContent, ReportFormat sourceFormat, ReportFormat targetFormat)
        {
            if (reportContent == null || reportContent.Length == 0)
            {
                throw new ArgumentException("Report content cannot be null or empty", nameof(reportContent));
            }
            
            // If formats are the same, return the original content
            if (sourceFormat == targetFormat)
            {
                return reportContent;
            }
            
            try
            {
                switch (sourceFormat)
                {
                    case ReportFormat.PDF:
                        return ConvertFromPdf(reportContent, targetFormat);
                        
                    case ReportFormat.Excel:
                        return ConvertFromExcel(reportContent, targetFormat);
                        
                    case ReportFormat.CSV:
                        return ConvertFromCsv(reportContent, targetFormat);
                        
                    case ReportFormat.HTML:
                        return ConvertFromHtml(reportContent, targetFormat);
                        
                    default:
                        throw new ValidationException(
                            $"Unsupported source format: {sourceFormat}", 
                            new List<string> { ErrorCodes.Report.InvalidReportFormat });
                }
            }
            catch (Exception ex) when (!(ex is ValidationException))
            {
                throw new ValidationException(
                    $"Failed to convert report from {sourceFormat} to {targetFormat}", 
                    new List<string> { "Report conversion failed with error: " + ex.Message }, 
                    ErrorCodes.Report.InvalidReportFormat, 
                    ex);
            }
        }

        /// <summary>
        /// Generates a standardized filename for a report based on title and format
        /// </summary>
        /// <param name="reportTitle">The title of the report</param>
        /// <param name="format">The format of the report</param>
        /// <returns>A formatted filename with appropriate extension</returns>
        public static string GetReportFileName(string reportTitle, ReportFormat format)
        {
            if (string.IsNullOrEmpty(reportTitle))
            {
                reportTitle = "Report";
            }
            
            string sanitizedTitle = SanitizeFileName(reportTitle);
            string extension = "";
            
            switch (format)
            {
                case ReportFormat.PDF:
                    extension = ".pdf";
                    break;
                case ReportFormat.Excel:
                    extension = ".xlsx";
                    break;
                case ReportFormat.CSV:
                    extension = ".csv";
                    break;
                case ReportFormat.HTML:
                    extension = ".html";
                    break;
                default:
                    extension = ".txt";
                    break;
            }
            
            return $"{sanitizedTitle}_{DateTime.Now:yyyyMMdd}{extension}";
        }

        /// <summary>
        /// Gets the MIME content type for a report format
        /// </summary>
        /// <param name="format">The format of the report</param>
        /// <returns>The MIME content type for the specified format</returns>
        public static string GetReportContentType(ReportFormat format)
        {
            switch (format)
            {
                case ReportFormat.PDF:
                    return "application/pdf";
                case ReportFormat.Excel:
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ReportFormat.CSV:
                    return "text/csv";
                case ReportFormat.HTML:
                    return "text/html";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// Validates that a report model contains all required data for report generation
        /// </summary>
        /// <param name="reportModel">The report model to validate</param>
        public static void ValidateReportModel(ReportModel reportModel)
        {
            if (reportModel == null)
            {
                throw new ArgumentNullException(nameof(reportModel), "Report model cannot be null");
            }
            
            try
            {
                // Use the model's built-in validation
                reportModel.Validate();
            }
            catch (ValidationException ex)
            {
                // Re-throw with more specific error code
                throw new ValidationException(
                    "Report model validation failed", 
                    ex.ValidationErrors, 
                    ErrorCodes.Report.InvalidReportParameters);
            }
            
            // Check calculation data
            if (reportModel.CalculationData == null)
            {
                throw new ValidationException(
                    "Calculation data is required", 
                    new List<string> { "CalculationData is missing in the report model" }, 
                    ErrorCodes.Report.InvalidReportParameters);
            }
            
            // Check country breakdowns
            if (reportModel.CalculationData.CountryBreakdowns == null || 
                !reportModel.CalculationData.CountryBreakdowns.Any())
            {
                throw new ValidationException(
                    "Country breakdown data is required", 
                    new List<string> { "The calculation must include at least one country" }, 
                    ErrorCodes.Report.InvalidReportParameters);
            }
            
            // Check total cost
            if (reportModel.CalculationData.TotalCost == null)
            {
                throw new ValidationException(
                    "Total cost is required", 
                    new List<string> { "TotalCost is missing in the calculation data" }, 
                    ErrorCodes.Report.InvalidReportParameters);
            }
        }

        #region Helper Methods

        private static byte[] ConvertFromPdf(byte[] pdfContent, ReportFormat targetFormat)
        {
            switch (targetFormat)
            {
                case ReportFormat.Excel:
                    // PDF to Excel conversion is complex and typically requires third-party libraries
                    throw new NotImplementedException("PDF to Excel conversion is not yet implemented");
                    
                case ReportFormat.CSV:
                    // PDF to CSV conversion is complex and typically requires third-party libraries
                    throw new NotImplementedException("PDF to CSV conversion is not yet implemented");
                    
                case ReportFormat.HTML:
                    // PDF to HTML conversion is complex and typically requires third-party libraries
                    throw new NotImplementedException("PDF to HTML conversion is not yet implemented");
                    
                default:
                    throw new ValidationException(
                        $"Unsupported target format: {targetFormat}", 
                        new List<string> { ErrorCodes.Report.InvalidReportFormat });
            }
        }

        private static byte[] ConvertFromExcel(byte[] excelContent, ReportFormat targetFormat)
        {
            switch (targetFormat)
            {
                case ReportFormat.PDF:
                    // Excel to PDF conversion is complex and typically requires third-party libraries
                    throw new NotImplementedException("Excel to PDF conversion is not yet implemented");
                    
                case ReportFormat.CSV:
                    // A simplified implementation of Excel to CSV conversion
                    using (MemoryStream ms = new MemoryStream(excelContent))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (var package = new ExcelPackage(ms))
                        {
                            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                            if (worksheet == null)
                            {
                                throw new ValidationException("Excel file contains no worksheets");
                            }
                            
                            using (var outputStream = new MemoryStream())
                            {
                                using (var writer = new StreamWriter(outputStream))
                                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                                {
                                    // Determine the dimensions of the worksheet
                                    int rowCount = worksheet.Dimension.Rows;
                                    int colCount = worksheet.Dimension.Columns;
                                    
                                    // Write each row to the CSV file
                                    for (int row = 1; row <= rowCount; row++)
                                    {
                                        for (int col = 1; col <= colCount; col++)
                                        {
                                            var cellValue = worksheet.Cells[row, col].Value;
                                            csv.WriteField(cellValue?.ToString());
                                        }
                                        
                                        csv.NextRecord();
                                    }
                                    
                                    writer.Flush();
                                    return outputStream.ToArray();
                                }
                            }
                        }
                    }
                    
                case ReportFormat.HTML:
                    // Excel to HTML conversion
                    using (MemoryStream ms = new MemoryStream(excelContent))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (var package = new ExcelPackage(ms))
                        {
                            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                            if (worksheet == null)
                            {
                                throw new ValidationException("Excel file contains no worksheets");
                            }
                            
                            StringBuilder html = new StringBuilder();
                            html.AppendLine("<!DOCTYPE html>");
                            html.AppendLine("<html lang=\"en\">");
                            html.AppendLine("<head>");
                            html.AppendLine("    <meta charset=\"UTF-8\">");
                            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                            html.AppendLine("    <title>Excel Report</title>");
                            html.AppendLine("    <style>");
                            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
                            html.AppendLine("        table { border-collapse: collapse; width: 100%; }");
                            html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; }");
                            html.AppendLine("        th { background-color: #f2f2f2; }");
                            html.AppendLine("    </style>");
                            html.AppendLine("</head>");
                            html.AppendLine("<body>");
                            
                            // Determine the dimensions of the worksheet
                            int rowCount = worksheet.Dimension.Rows;
                            int colCount = worksheet.Dimension.Columns;
                            
                            // Start table
                            html.AppendLine("<table>");
                            
                            // Create each row
                            for (int row = 1; row <= rowCount; row++)
                            {
                                html.AppendLine("    <tr>");
                                
                                for (int col = 1; col <= colCount; col++)
                                {
                                    var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? "";
                                    
                                    // Use th for header row
                                    if (row == 1)
                                    {
                                        html.AppendLine($"        <th>{cellValue}</th>");
                                    }
                                    else
                                    {
                                        html.AppendLine($"        <td>{cellValue}</td>");
                                    }
                                }
                                
                                html.AppendLine("    </tr>");
                            }
                            
                            html.AppendLine("</table>");
                            html.AppendLine("</body>");
                            html.AppendLine("</html>");
                            
                            return Encoding.UTF8.GetBytes(html.ToString());
                        }
                    }
                    
                default:
                    throw new ValidationException(
                        $"Unsupported target format: {targetFormat}", 
                        new List<string> { ErrorCodes.Report.InvalidReportFormat });
            }
        }

        private static byte[] ConvertFromCsv(byte[] csvContent, ReportFormat targetFormat)
        {
            switch (targetFormat)
            {
                case ReportFormat.PDF:
                    // CSV to PDF conversion is complex and typically requires third-party libraries
                    throw new NotImplementedException("CSV to PDF conversion is not yet implemented");
                    
                case ReportFormat.Excel:
                    // A simplified implementation of CSV to Excel conversion
                    using (var inputStream = new MemoryStream(csvContent))
                    using (var reader = new StreamReader(inputStream))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        // Read all records
                        var records = csv.GetRecords<dynamic>().ToList();
                        
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (var package = new ExcelPackage())
                        {
                            var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                            
                            // Check if records exist
                            if (records.Count > 0)
                            {
                                // Extract headers (property names)
                                var headers = ((IDictionary<string, object>)records[0]).Keys.ToList();
                                
                                // Write headers
                                for (int i = 0; i < headers.Count; i++)
                                {
                                    worksheet.Cells[1, i + 1].Value = headers[i];
                                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                                }
                                
                                // Write data
                                for (int row = 0; row < records.Count; row++)
                                {
                                    var record = (IDictionary<string, object>)records[row];
                                    
                                    for (int col = 0; col < headers.Count; col++)
                                    {
                                        worksheet.Cells[row + 2, col + 1].Value = record[headers[col]]?.ToString();
                                    }
                                }
                                
                                // Auto fit columns
                                for (int i = 1; i <= headers.Count; i++)
                                {
                                    worksheet.Column(i).AutoFit();
                                }
                            }
                            
                            // Save the Excel package to a memory stream
                            using (var outputStream = new MemoryStream())
                            {
                                package.SaveAs(outputStream);
                                return outputStream.ToArray();
                            }
                        }
                    }
                    
                case ReportFormat.HTML:
                    // A simplified implementation of CSV to HTML conversion
                    using (var inputStream = new MemoryStream(csvContent))
                    using (var reader = new StreamReader(inputStream))
                    {
                        StringBuilder html = new StringBuilder();
                        html.AppendLine("<!DOCTYPE html>");
                        html.AppendLine("<html lang=\"en\">");
                        html.AppendLine("<head>");
                        html.AppendLine("    <meta charset=\"UTF-8\">");
                        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                        html.AppendLine("    <title>CSV Report</title>");
                        html.AppendLine("    <style>");
                        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
                        html.AppendLine("        table { border-collapse: collapse; width: 100%; }");
                        html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; }");
                        html.AppendLine("        th { background-color: #f2f2f2; }");
                        html.AppendLine("    </style>");
                        html.AppendLine("</head>");
                        html.AppendLine("<body>");
                        html.AppendLine("<table>");
                        
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            // Read headers
                            csv.Read();
                            csv.ReadHeader();
                            var headers = csv.HeaderRecord;
                            
                            // Write table header
                            html.AppendLine("    <tr>");
                            foreach (var header in headers)
                            {
                                html.AppendLine($"        <th>{header}</th>");
                            }
                            html.AppendLine("    </tr>");
                            
                            // Write data rows
                            while (csv.Read())
                            {
                                html.AppendLine("    <tr>");
                                
                                foreach (var header in headers)
                                {
                                    string value = csv.GetField(header) ?? "";
                                    html.AppendLine($"        <td>{value}</td>");
                                }
                                
                                html.AppendLine("    </tr>");
                            }
                        }
                        
                        html.AppendLine("</table>");
                        html.AppendLine("</body>");
                        html.AppendLine("</html>");
                        
                        return Encoding.UTF8.GetBytes(html.ToString());
                    }
                    
                default:
                    throw new ValidationException(
                        $"Unsupported target format: {targetFormat}", 
                        new List<string> { ErrorCodes.Report.InvalidReportFormat });
            }
        }

        private static byte[] ConvertFromHtml(byte[] htmlContent, ReportFormat targetFormat)
        {
            switch (targetFormat)
            {
                case ReportFormat.PDF:
                    // HTML to PDF conversion is complex and typically requires third-party libraries
                    throw new NotImplementedException("HTML to PDF conversion is not yet implemented");
                    
                case ReportFormat.Excel:
                    // HTML to Excel conversion is complex and typically requires third-party libraries
                    throw new NotImplementedException("HTML to Excel conversion is not yet implemented");
                    
                case ReportFormat.CSV:
                    // HTML to CSV conversion is complex and typically requires third-party libraries
                    throw new NotImplementedException("HTML to CSV conversion is not yet implemented");
                    
                default:
                    throw new ValidationException(
                        $"Unsupported target format: {targetFormat}", 
                        new List<string> { ErrorCodes.Report.InvalidReportFormat });
            }
        }

        /// <summary>
        /// Removes invalid characters from a filename
        /// </summary>
        /// <param name="fileName">The filename to sanitize</param>
        /// <returns>The sanitized filename</returns>
        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "Report";
            }
            
            // Replace invalid filename characters with underscores
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"[{0}]", invalidChars);
            
            string sanitized = Regex.Replace(fileName, invalidRegStr, "_");
            
            // Replace spaces with underscores and trim
            sanitized = sanitized.Replace(' ', '_').Trim();
            
            // Limit length
            if (sanitized.Length > 100)
            {
                sanitized = sanitized.Substring(0, 100);
            }
            
            return sanitized;
        }

        /// <summary>
        /// Formats a monetary value with the appropriate currency symbol
        /// </summary>
        /// <param name="amount">The monetary amount</param>
        /// <param name="currencyCode">The ISO currency code</param>
        /// <returns>The formatted currency string</returns>
        private static string FormatCurrency(decimal amount, string currencyCode)
        {
            try
            {
                // Try to get a CultureInfo associated with the currency
                var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                foreach (var culture in cultures)
                {
                    try
                    {
                        var region = new RegionInfo(culture.Name);
                        if (region.ISOCurrencySymbol == currencyCode)
                        {
                            return amount.ToString("C", culture);
                        }
                    }
                    catch
                    {
                        // Ignore and continue checking other cultures
                    }
                }
            }
            catch
            {
                // Fall back to simple format if any error occurs
            }
            
            // Fallback to a simple format
            return $"{amount:0.00} {currencyCode}";
        }

        /// <summary>
        /// Gets a human-readable description of a service type
        /// </summary>
        /// <param name="serviceType">The service type</param>
        /// <returns>A description of the service type</returns>
        private static string GetServiceTypeDescription(ServiceType serviceType)
        {
            switch (serviceType)
            {
                case ServiceType.StandardFiling:
                    return "Standard VAT Filing";
                case ServiceType.ComplexFiling:
                    return "Complex VAT Filing";
                case ServiceType.PriorityService:
                    return "Priority VAT Filing Service";
                default:
                    return "Unknown Service Type";
            }
        }

        /// <summary>
        /// Gets a human-readable description of a filing frequency
        /// </summary>
        /// <param name="frequency">The filing frequency</param>
        /// <returns>A description of the filing frequency</returns>
        private static string GetFilingFrequencyDescription(FilingFrequency frequency)
        {
            switch (frequency)
            {
                case FilingFrequency.Monthly:
                    return "Monthly";
                case FilingFrequency.Quarterly:
                    return "Quarterly";
                case FilingFrequency.Annually:
                    return "Annually";
                default:
                    return "Unknown Frequency";
            }
        }

        /// <summary>
        /// Gets the VAT rate for a specific country
        /// </summary>
        /// <param name="countryCode">The country code</param>
        /// <returns>The VAT rate as a string</returns>
        private static string GetVatRateForCountry(string countryCode)
        {
            // This would typically come from a database or API
            // Using hardcoded values for illustration purposes
            switch (countryCode)
            {
                case "GB": return "20%";
                case "DE": return "19%";
                case "FR": return "20%";
                case "IT": return "22%";
                case "ES": return "21%";
                case "NL": return "21%";
                case "BE": return "21%";
                case "SE": return "25%";
                case "DK": return "25%";
                case "PL": return "23%";
                case "IE": return "23%";
                default: return "N/A";
            }
        }

        /// <summary>
        /// Gets the filing requirements for a specific country and frequency
        /// </summary>
        /// <param name="countryCode">The country code</param>
        /// <param name="frequency">The filing frequency</param>
        /// <returns>A description of the filing requirements</returns>
        private static string GetFilingRequirementsForCountry(string countryCode, FilingFrequency frequency)
        {
            // This would typically come from a database or API
            // Using simplified responses for illustration purposes
            string baseRequirements = "VAT registration required. ";
            string deadline;
            
            switch (frequency)
            {
                case FilingFrequency.Monthly:
                    deadline = "Filing deadline typically by the end of the following month.";
                    break;
                case FilingFrequency.Quarterly:
                    deadline = "Filing deadline typically within one month after the end of the quarter.";
                    break;
                case FilingFrequency.Annually:
                    deadline = "Annual filing deadline varies by country, typically within 3-6 months after the end of the fiscal year.";
                    break;
                default:
                    deadline = "Filing deadline information not available.";
                    break;
            }
            
            switch (countryCode)
            {
                case "GB":
                    return baseRequirements + "Making Tax Digital (MTD) compliant software required. " + deadline;
                case "DE":
                    return baseRequirements + "Advance VAT return (Umsatzsteuer-Voranmeldung) required. " + deadline;
                case "FR":
                    return baseRequirements + "Electronic filing mandatory for all businesses. " + deadline;
                case "IT":
                    return baseRequirements + "Electronic invoicing mandatory. " + deadline;
                default:
                    return baseRequirements + deadline;
            }
        }

        /// <summary>
        /// Helper method to get Excel currency format string
        /// </summary>
        /// <param name="currencyCode">The currency code</param>
        /// <returns>The Excel format string for the currency</returns>
        private static string GetCurrencyFormat(string currencyCode)
        {
            switch (currencyCode)
            {
                case "EUR": return "€#,##0.00";
                case "GBP": return "£#,##0.00";
                case "USD": return "$#,##0.00";
                default: return $"#,##0.00 {currencyCode}";
            }
        }
        
        #endregion
    }
}