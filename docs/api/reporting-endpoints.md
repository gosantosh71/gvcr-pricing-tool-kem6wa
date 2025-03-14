# VAT Filing Pricing Tool - Reporting API

## Overview

The Reporting API provides comprehensive capabilities for generating, retrieving, downloading, and managing reports based on VAT filing cost calculations. These endpoints enable users to create detailed reports in various formats (PDF, Excel, CSV, HTML), view report history, download reports, email reports to specified recipients, and manage report lifecycle through archival operations.

Reports can include various components such as country breakdowns, service details, applied discounts, historical comparisons, and tax rate details, all based on previously performed VAT filing calculations.

## Authentication Requirements

All reporting API endpoints require authentication using JSON Web Tokens (JWT).

### Authentication Header

```
Authorization: Bearer {token}
```

Where `{token}` is a valid JWT obtained through the Authentication API. Tokens typically expire after 60 minutes and can be refreshed using the token refresh endpoint.

### Authentication Errors

- **401 Unauthorized** - Returned when no authentication token is provided or the token is invalid
- **403 Forbidden** - Returned when the authenticated user doesn't have permission to access the requested report

## Base URL

All API endpoints are relative to the base URL:

```
https://api.vatfilingpricingtool.com/api/v1
```

## Generate Report Endpoint

Creates a new report based on a previous calculation with specified format and content options.

### HTTP Request

```
POST /reports/generate
```

### Request Body

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| calculationId | string | Yes | - | ID of the calculation to base the report on |
| reportTitle | string | Yes | - | Title of the report (max 100 characters) |
| format | enum | No | PDF | Format of the report (PDF, Excel, CSV, HTML) |
| includeCountryBreakdown | boolean | No | true | Whether to include country breakdown |
| includeServiceDetails | boolean | No | true | Whether to include service details |
| includeAppliedDiscounts | boolean | No | true | Whether to include applied discounts |
| includeHistoricalComparison | boolean | No | false | Whether to include historical comparison |
| includeTaxRateDetails | boolean | No | false | Whether to include tax rate details |
| deliveryOptions | object | No | - | Options for report delivery |

#### Delivery Options Object

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| downloadImmediately | boolean | No | true | Whether to make report available for immediate download |
| sendEmail | boolean | No | false | Whether to email the report |
| emailAddress | string | Yes (if sendEmail=true) | - | Email address to send report to |
| emailSubject | string | No | - | Custom subject for the email |
| emailMessage | string | No | - | Custom message for the email |

### Example Request

```json
{
  "calculationId": "calc-2023-05-10-001",
  "reportTitle": "Q2 2023 VAT Filing Estimate",
  "format": "PDF",
  "includeCountryBreakdown": true,
  "includeServiceDetails": true,
  "includeAppliedDiscounts": true,
  "includeHistoricalComparison": false,
  "includeTaxRateDetails": true,
  "deliveryOptions": {
    "downloadImmediately": true,
    "sendEmail": true,
    "emailAddress": "finance@example.com",
    "emailSubject": "Q2 2023 VAT Filing Cost Report",
    "emailMessage": "Please find attached the VAT filing cost report for Q2 2023."
  }
}
```

### Response

| Parameter | Type | Description |
|-----------|------|-------------|
| reportId | string | Unique identifier for the generated report |
| reportTitle | string | Title of the report |
| format | enum | Format of the report (PDF, Excel, CSV, HTML) |
| downloadUrl | string | URL where the report can be downloaded |
| generationDate | datetime | Date and time when the report was generated |
| fileSize | long | Size of the report file in bytes |
| isReady | boolean | Whether the report is ready for download |

### Example Response

```json
{
  "reportId": "report-2023-05-10-001",
  "reportTitle": "Q2 2023 VAT Filing Estimate",
  "format": "PDF",
  "downloadUrl": "https://api.vatfilingpricingtool.com/api/v1/reports/report-2023-05-10-001/download",
  "generationDate": "2023-05-10T14:30:45Z",
  "fileSize": 1258437,
  "isReady": true
}
```

### Error Responses

| Status Code | Error Code | Description |
|-------------|------------|-------------|
| 400 | REPORT-009 | Invalid report parameters |
| 400 | REPORT-002 | Report generation failed |
| 404 | PRICING-009 | Calculation not found |
| 500 | REPORT-002 | An unexpected error occurred during report generation |

## Get Report Endpoint

Retrieves detailed information about a specific report.

### HTTP Request

```
GET /reports/{id}
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | string | Yes | ID of the report to retrieve |

### Example Request

```
GET /reports/report-2023-05-10-001
```

### Response

| Parameter | Type | Description |
|-----------|------|-------------|
| reportId | string | Unique identifier for the report |
| reportTitle | string | Title of the report |
| calculationId | string | ID of the calculation the report is based on |
| format | enum | Format of the report (PDF, Excel, CSV, HTML) |
| downloadUrl | string | URL where the report can be downloaded |
| generationDate | datetime | Date and time when the report was generated |
| fileSize | long | Size of the report file in bytes |
| includeCountryBreakdown | boolean | Whether country breakdown is included |
| includeServiceDetails | boolean | Whether service details are included |
| includeAppliedDiscounts | boolean | Whether applied discounts are included |
| includeHistoricalComparison | boolean | Whether historical comparison is included |
| includeTaxRateDetails | boolean | Whether tax rate details are included |
| isArchived | boolean | Whether the report is archived |

### Example Response

```json
{
  "reportId": "report-2023-05-10-001",
  "reportTitle": "Q2 2023 VAT Filing Estimate",
  "calculationId": "calc-2023-05-10-001",
  "format": "PDF",
  "downloadUrl": "https://api.vatfilingpricingtool.com/api/v1/reports/report-2023-05-10-001/download",
  "generationDate": "2023-05-10T14:30:45Z",
  "fileSize": 1258437,
  "includeCountryBreakdown": true,
  "includeServiceDetails": true,
  "includeAppliedDiscounts": true,
  "includeHistoricalComparison": false,
  "includeTaxRateDetails": true,
  "isArchived": false
}
```

### Error Responses

| Status Code | Error Code | Description |
|-------------|------------|-------------|
| 400 | GENERAL-007 | Report ID is required |
| 404 | REPORT-001 | Report not found |
| 500 | GENERAL-001 | An unexpected error occurred while retrieving the report |

## Get Report History Endpoint

Retrieves a paginated list of reports with optional filtering.

### HTTP Request

```
GET /reports
```

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| startDate | datetime | No | - | Filter reports generated on or after this date |
| endDate | datetime | No | - | Filter reports generated on or before this date |
| reportType | string | No | - | Filter reports by type |
| format | enum | No | - | Filter reports by format (PDF, Excel, CSV, HTML) |
| page | integer | No | 1 | Page number (1-based) |
| pageSize | integer | No | 10 | Number of reports per page (1-100) |
| includeArchived | boolean | No | false | Whether to include archived reports |

### Example Request

```
GET /reports?startDate=2023-01-01T00:00:00Z&format=PDF&page=1&pageSize=10
```

### Response

| Parameter | Type | Description |
|-----------|------|-------------|
| reports | array | List of report items for the current page |
| pageNumber | integer | Current page number |
| pageSize | integer | Number of items per page |
| totalCount | integer | Total number of reports matching the filters |
| totalPages | integer | Total number of pages |
| hasPreviousPage | boolean | Whether there is a previous page |
| hasNextPage | boolean | Whether there is a next page |

#### Report List Item

| Parameter | Type | Description |
|-----------|------|-------------|
| reportId | string | Unique identifier for the report |
| reportTitle | string | Title of the report |
| format | enum | Format of the report (PDF, Excel, CSV, HTML) |
| generationDate | datetime | Date and time when the report was generated |
| fileSize | long | Size of the report file in bytes |
| isArchived | boolean | Whether the report is archived |

### Example Response

```json
{
  "reports": [
    {
      "reportId": "report-2023-05-10-001",
      "reportTitle": "Q2 2023 VAT Filing Estimate",
      "format": "PDF",
      "generationDate": "2023-05-10T14:30:45Z",
      "fileSize": 1258437,
      "isArchived": false
    },
    {
      "reportId": "report-2023-05-09-002",
      "reportTitle": "UK VAT Filing Analysis",
      "format": "PDF",
      "generationDate": "2023-05-09T10:15:22Z",
      "fileSize": 987624,
      "isArchived": false
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 28,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Error Responses

| Status Code | Error Code | Description |
|-------------|------------|-------------|
| 400 | GENERAL-003 | Validation error in request parameters |
| 500 | GENERAL-001 | An unexpected error occurred while retrieving report history |

## Download Report Endpoint

Downloads a report file in the specified format.

### HTTP Request

```
GET /reports/{id}/download
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | string | Yes | ID of the report to download |

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| format | enum | No | - | Convert the report to this format if different from original (PDF, Excel, CSV, HTML) |

### Example Request

```
GET /reports/report-2023-05-10-001/download
```

```
GET /reports/report-2023-05-10-001/download?format=Excel
```

### Response

The response is a file download with the appropriate Content-Type header based on the report format:

- PDF: `application/pdf`
- Excel: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- CSV: `text/csv`
- HTML: `text/html`

The response will include the following headers:

```
Content-Disposition: attachment; filename="Q2_2023_VAT_Filing_Estimate.pdf"
Content-Type: application/pdf
Content-Length: 1258437
```

### Error Responses

| Status Code | Error Code | Description |
|-------------|------------|-------------|
| 400 | GENERAL-007 | Report ID is required |
| 400 | REPORT-003 | Invalid report format |
| 404 | REPORT-001 | Report not found |
| 500 | REPORT-004 | An unexpected error occurred while downloading the report |

## Email Report Endpoint

Emails a report to the specified email address.

### HTTP Request

```
POST /reports/{id}/email
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | string | Yes | ID of the report to email |

### Request Body

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| emailAddress | string | Yes | Email address to send the report to |
| subject | string | Yes | Subject line for the email |
| message | string | No | Message body for the email |

### Example Request

```json
{
  "emailAddress": "finance@example.com",
  "subject": "Q2 2023 VAT Filing Cost Report",
  "message": "Please find attached the VAT filing cost report for Q2 2023 that provides a detailed breakdown of expected filing costs for the upcoming quarter."
}
```

### Response

| Parameter | Type | Description |
|-----------|------|-------------|
| reportId | string | Unique identifier for the report |
| emailAddress | string | Email address where the report was sent |
| emailSent | boolean | Whether the email was sent successfully |
| sentTime | datetime | Date and time when the email was sent |

### Example Response

```json
{
  "reportId": "report-2023-05-10-001",
  "emailAddress": "finance@example.com",
  "emailSent": true,
  "sentTime": "2023-05-10T14:35:12Z"
}
```

### Error Responses

| Status Code | Error Code | Description |
|-------------|------------|-------------|
| 400 | GENERAL-007 | Report ID is required |
| 400 | GENERAL-003 | Invalid email address format |
| 404 | REPORT-001 | Report not found |
| 500 | REPORT-005 | An unexpected error occurred while emailing the report |

## Archive Report Endpoint

Archives a report to hide it from regular report listings.

### HTTP Request

```
PUT /reports/{id}/archive
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | string | Yes | ID of the report to archive |

### Example Request

```
PUT /reports/report-2023-05-10-001/archive
```

### Response

| Parameter | Type | Description |
|-----------|------|-------------|
| reportId | string | Unique identifier for the report |
| isArchived | boolean | Always true when archive operation succeeds |

### Example Response

```json
{
  "reportId": "report-2023-05-10-001",
  "isArchived": true
}
```

### Error Responses

| Status Code | Error Code | Description |
|-------------|------------|-------------|
| 400 | GENERAL-007 | Report ID is required |
| 404 | REPORT-001 | Report not found |
| 500 | REPORT-006 | An unexpected error occurred while archiving the report |

## Unarchive Report Endpoint

Unarchives a previously archived report to make it visible in regular report listings.

### HTTP Request

```
PUT /reports/{id}/unarchive
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | string | Yes | ID of the report to unarchive |

### Example Request

```
PUT /reports/report-2023-05-10-001/unarchive
```

### Response

| Parameter | Type | Description |
|-----------|------|-------------|
| reportId | string | Unique identifier for the report |
| isArchived | boolean | Always false when unarchive operation succeeds |

### Example Response

```json
{
  "reportId": "report-2023-05-10-001",
  "isArchived": false
}
```

### Error Responses

| Status Code | Error Code | Description |
|-------------|------------|-------------|
| 400 | GENERAL-007 | Report ID is required |
| 404 | REPORT-001 | Report not found |
| 500 | REPORT-007 | An unexpected error occurred while unarchiving the report |

## Delete Report Endpoint

Permanently deletes a report and its associated file.

### HTTP Request

```
DELETE /reports/{id}
```

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | string | Yes | ID of the report to delete |

### Example Request

```
DELETE /reports/report-2023-05-10-001
```

### Response

Returns an HTTP 204 No Content response on successful deletion with no response body.

### Error Responses

| Status Code | Error Code | Description |
|-------------|------------|-------------|
| 400 | GENERAL-007 | Report ID is required |
| 404 | REPORT-001 | Report not found |
| 500 | REPORT-008 | An unexpected error occurred while deleting the report |

## Common Error Responses

### 400 Bad Request

Returned when the request contains invalid parameters or fails validation.

```json
{
  "success": false,
  "message": "Invalid request parameters",
  "errors": [
    "Report ID is required",
    "Email address format is invalid"
  ]
}
```

### 401 Unauthorized

Returned when authentication fails or is missing.

```json
{
  "success": false,
  "message": "Authentication required",
  "errors": [
    "Missing or invalid authentication token"
  ]
}
```

### 403 Forbidden

Returned when the authenticated user doesn't have permission to access the requested resource.

```json
{
  "success": false,
  "message": "Access denied",
  "errors": [
    "You do not have permission to access this report"
  ]
}
```

### 404 Not Found

Returned when the requested resource is not found.

```json
{
  "success": false,
  "message": "Report not found",
  "errors": [
    "The requested report does not exist or has been deleted"
  ]
}
```

### 500 Internal Server Error

Returned when an unexpected server error occurs.

```json
{
  "success": false,
  "message": "An unexpected error occurred while processing your request. Please try again later.",
  "errors": []
}
```

## Usage Examples

### Complete Report Generation Flow

The following example demonstrates a complete flow from generating a report to downloading it:

#### 1. Generate a Report Based on a Calculation

```javascript
// JavaScript / Fetch API
const generateReport = async (calculationId) => {
  const token = "your-auth-token";
  
  const response = await fetch("https://api.vatfilingpricingtool.com/api/v1/reports/generate", {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${token}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      calculationId: calculationId,
      reportTitle: "Q2 2023 VAT Filing Estimate",
      format: "PDF",
      includeCountryBreakdown: true,
      includeServiceDetails: true,
      includeAppliedDiscounts: true,
      deliveryOptions: {
        downloadImmediately: true
      }
    })
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(`Failed to generate report: ${error.message}`);
  }
  
  return await response.json();
};

// Usage
try {
  const report = await generateReport("calc-2023-05-10-001");
  console.log(`Report generated with ID: ${report.reportId}`);
  console.log(`Download URL: ${report.downloadUrl}`);
} catch (error) {
  console.error(error.message);
}
```

#### 2. Get Report Details

```csharp
// C#
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public async Task<Report> GetReportAsync(string reportId, string token)
{
    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await client.GetAsync($"https://api.vatfilingpricingtool.com/api/v1/reports/{reportId}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Report>(content);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var error = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            throw new Exception($"Failed to get report: {error.Message}");
        }
    }
}

// Usage
try
{
    var report = await GetReportAsync("report-2023-05-10-001", "your-auth-token");
    Console.WriteLine($"Report Title: {report.ReportTitle}");
    Console.WriteLine($"Format: {report.Format}");
    Console.WriteLine($"Size: {report.FileSize} bytes");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

#### 3. Download the Report

```bash
# Using curl
curl -X GET "https://api.vatfilingpricingtool.com/api/v1/reports/report-2023-05-10-001/download" \
  -H "Authorization: Bearer your-auth-token" \
  -o "Q2_2023_VAT_Filing_Estimate.pdf"
```

#### 4. Email the Report to a Colleague

```javascript
// JavaScript / Fetch API
const emailReport = async (reportId, emailAddress) => {
  const token = "your-auth-token";
  
  const response = await fetch(`https://api.vatfilingpricingtool.com/api/v1/reports/${reportId}/email`, {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${token}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      emailAddress: emailAddress,
      subject: "VAT Filing Cost Report",
      message: "Please review the attached VAT filing cost report."
    })
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(`Failed to email report: ${error.message}`);
  }
  
  return await response.json();
};

// Usage
try {
  const result = await emailReport("report-2023-05-10-001", "colleague@example.com");
  console.log(`Report emailed successfully to ${result.emailAddress}`);
} catch (error) {
  console.error(error.message);
}
```

#### 5. Archive an Old Report

```csharp
// C#
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public async Task<bool> ArchiveReportAsync(string reportId, string token)
{
    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await client.PutAsync($"https://api.vatfilingpricingtool.com/api/v1/reports/{reportId}/archive", null);
        
        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var error = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            throw new Exception($"Failed to archive report: {error.Message}");
        }
    }
}

// Usage
try
{
    bool success = await ArchiveReportAsync("report-2023-01-15-003", "your-auth-token");
    Console.WriteLine("Report archived successfully");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

#### 6. Retrieve Report History with Filtering

```javascript
// JavaScript / Fetch API
const getReportHistory = async (startDate, format, page = 1, pageSize = 10) => {
  const token = "your-auth-token";
  
  const queryParams = new URLSearchParams({
    startDate: startDate.toISOString(),
    format: format,
    page: page,
    pageSize: pageSize
  });
  
  const response = await fetch(`https://api.vatfilingpricingtool.com/api/v1/reports?${queryParams}`, {
    method: "GET",
    headers: {
      "Authorization": `Bearer ${token}`
    }
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(`Failed to get report history: ${error.message}`);
  }
  
  return await response.json();
};

// Usage
try {
  const startDate = new Date();
  startDate.setMonth(startDate.getMonth() - 3); // Last 3 months
  
  const history = await getReportHistory(startDate, "PDF", 1, 20);
  console.log(`Found ${history.totalCount} reports`);
  
  history.reports.forEach(report => {
    console.log(`${report.reportTitle} (${report.generationDate})`);
  });
  
  if (history.hasNextPage) {
    console.log("More reports available on next page");
  }
} catch (error) {
  console.error(error.message);
}
```

#### 7. Error Handling Example

```javascript
// JavaScript / Fetch API
const handleReportOperation = async (operation, url, options = {}) => {
  try {
    const response = await fetch(url, options);
    
    if (!response.ok) {
      const errorData = await response.json();
      
      // Handle specific error cases
      switch (response.status) {
        case 400:
          console.error("Invalid request:", errorData.errors.join(", "));
          break;
        case 401:
          console.error("Authentication required. Please log in again.");
          // Redirect to login page
          break;
        case 403:
          console.error("You don't have permission to access this report.");
          break;
        case 404:
          console.error("Report not found. It may have been deleted.");
          break;
        case 500:
          console.error("Server error. Please try again later.");
          break;
        default:
          console.error("Operation failed:", errorData.message);
      }
      
      throw new Error(`${operation} failed: ${errorData.message}`);
    }
    
    return response.status === 204 ? true : await response.json();
  } catch (error) {
    console.error(`Error during ${operation}:`, error);
    throw error;
  }
};

// Usage example
const deleteReport = (reportId) => {
  const token = "your-auth-token";
  
  return handleReportOperation(
    "Delete report",
    `https://api.vatfilingpricingtool.com/api/v1/reports/${reportId}`,
    {
      method: "DELETE",
      headers: {
        "Authorization": `Bearer ${token}`
      }
    }
  );
};
```