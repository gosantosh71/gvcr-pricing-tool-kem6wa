# Pricing API Documentation

This document provides comprehensive information about the pricing calculation API endpoints for the VAT Filing Pricing Tool. These endpoints allow users to calculate, retrieve, save, and compare VAT filing costs across multiple jurisdictions.

## Overview

The pricing API endpoints provide core functionality for estimating VAT filing costs. They enable users to:

- Calculate VAT filing costs based on various parameters.
- Retrieve specific calculations by ID.
- Save calculation results for future reference.
- Retrieve calculation history with filtering options.
- Compare multiple pricing scenarios to identify the most cost-effective options.

These endpoints are designed to be flexible and scalable, supporting a wide range of use cases and integration scenarios.

## Authentication Requirements

All pricing API endpoints require authentication using JWT (JSON Web Token). The JWT must be included in the `Authorization` header of the request, following the `Bearer` scheme.

```
Authorization: Bearer <JWT_TOKEN>
```

For more information on JWT authentication, refer to [RFC 7519](https://tools.ietf.org/html/rfc7519).

## Calculate Pricing Endpoint

Calculates VAT filing costs based on the provided parameters.

- **HTTP Method:** `POST`
- **URL Path:** `/api/v1/pricing/calculate`

### Request Model

```json
{
  "serviceType": "StandardFiling",
  "transactionVolume": 500,
  "frequency": "Quarterly",
  "countryCodes": ["GB", "DE", "FR"],
  "additionalServices": ["TaxConsultancy"]
}
```

| Parameter | Type | Description |
|---|---|---|
| `serviceType` | enum (string) | The type of VAT filing service. Possible values: `StandardFiling`, `ComplexFiling`, `PriorityService`. |
| `transactionVolume` | integer | The number of transactions/invoices per period. |
| `frequency` | enum (string) | The filing frequency. Possible values: `Monthly`, `Quarterly`, `Annually`. |
| `countryCodes` | array (string) | An array of ISO 3166-1 alpha-2 country codes for which to calculate VAT filing costs. |
| `additionalServices` | array (string) | An array of service identifiers for additional services to include in the calculation. |

### Response Model

```json
{
  "calculationId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "serviceType": "StandardFiling",
  "transactionVolume": 500,
  "frequency": "Quarterly",
  "totalCost": 4250.00,
  "currencyCode": "EUR",
  "calculationDate": "2023-10-27T10:00:00Z",
  "countryBreakdowns": [
    {
      "countryCode": "GB",
      "countryName": "United Kingdom",
      "baseCost": 1200.00,
      "additionalCost": 300.00,
      "totalCost": 1500.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule"]
    },
    {
      "countryCode": "DE",
      "countryName": "Germany",
      "baseCost": 1000.00,
      "additionalCost": 250.00,
      "totalCost": 1250.00,
      "appliedRules": ["VATRateRule", "ComplexityRule"]
    },
    {
      "countryCode": "FR",
      "countryName": "France",
      "baseCost": 1200.00,
      "additionalCost": 300.00,
      "totalCost": 1500.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule"]
    }
  ],
  "additionalServices": ["TaxConsultancy"],
  "discounts": {
    "VolumeDiscount": 200.00,
    "MultiCountryDiscount": 150.00
  }
}
```

| Field | Type | Description |
|---|---|---|
| `calculationId` | string | Unique identifier for the calculation. |
| `serviceType` | enum (string) | The type of VAT filing service. |
| `transactionVolume` | integer | The number of transactions/invoices per period. |
| `frequency` | enum (string) | The filing frequency. |
| `totalCost` | decimal | The total estimated cost for VAT filing. |
| `currencyCode` | string | The currency code for the total cost. |
| `calculationDate` | datetime | The date and time when the calculation was performed. |
| `countryBreakdowns` | array (object) | An array of country-specific cost breakdowns. |
| `additionalServices` | array (string) | An array of service identifiers for additional services included in the calculation. |
| `discounts` | object | A dictionary of discounts applied to the calculation, with discount names as keys and discount amounts as values. |

### Example Request

```
POST /api/v1/pricing/calculate
Content-Type: application/json
Authorization: Bearer <JWT_TOKEN>

{
  "serviceType": "ComplexFiling",
  "transactionVolume": 1200,
  "frequency": "Monthly",
  "countryCodes": ["GB", "DE", "FR", "IT"],
  "additionalServices": ["TaxConsultancy", "ReconciliationServices"]
}
```

### Example Response

```
HTTP/1.1 200 OK
Content-Type: application/json

{
  "calculationId": "b9d87a65-4321-4fed-9876-543210abcdef",
  "serviceType": "ComplexFiling",
  "transactionVolume": 1200,
  "frequency": "Monthly",
  "totalCost": 7800.00,
  "currencyCode": "EUR",
  "calculationDate": "2023-10-27T10:00:00Z",
  "countryBreakdowns": [
    {
      "countryCode": "GB",
      "countryName": "United Kingdom",
      "baseCost": 1800.00,
      "additionalCost": 500.00,
      "totalCost": 2300.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule", "ComplexityRule"]
    },
    {
      "countryCode": "DE",
      "countryName": "Germany",
      "baseCost": 1500.00,
      "additionalCost": 400.00,
      "totalCost": 1900.00,
      "appliedRules": ["VATRateRule", "ComplexityRule"]
    },
    {
      "countryCode": "FR",
      "countryName": "France",
      "baseCost": 1700.00,
      "additionalCost": 450.00,
      "totalCost": 2150.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule", "ComplexityRule"]
    },
    {
      "countryCode": "IT",
      "countryName": "Italy",
      "baseCost": 1000.00,
      "additionalCost": 450.00,
      "totalCost": 1450.00,
      "appliedRules": ["VATRateRule", "ComplexityRule"]
    }
  ],
  "additionalServices": ["TaxConsultancy", "ReconciliationServices"],
  "discounts": {
    "VolumeDiscount": 300.00,
    "MultiCountryDiscount": 200.00
  }
}
```

### Error Responses

- `400 Bad Request`: Invalid request parameters. See [Common Error Responses](#common-error-responses) for details.
- `401 Unauthorized`: Missing or invalid JWT token. See [Common Error Responses](#common-error-responses) for details.
- `500 Internal Server Error`: An unexpected error occurred during the calculation. See [Common Error Responses](#common-error-responses) for details.

### Usage Notes

- Ensure that the `countryCodes` array contains valid ISO 3166-1 alpha-2 country codes.
- The `transactionVolume` should be a positive integer.
- The `serviceType` and `frequency` parameters must be valid enum values.

## Get Calculation Endpoint

Retrieves a specific calculation by its ID.

- **HTTP Method:** `GET`
- **URL Path:** `/api/v1/pricing/{id}`

### Request Parameters

| Parameter | Type | Description |
|---|---|---|
| `id` | string | The unique identifier of the calculation to retrieve. |

### Response Model

See [Calculate Pricing Endpoint](#calculate-pricing-endpoint) for the response model.

### Example Request

```
GET /api/v1/pricing/a1b2c3d4-e5f6-7890-1234-567890abcdef
Authorization: Bearer <JWT_TOKEN>
```

### Example Response

```
HTTP/1.1 200 OK
Content-Type: application/json

{
  "calculationId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "serviceType": "StandardFiling",
  "transactionVolume": 500,
  "frequency": "Quarterly",
  "totalCost": 4250.00,
  "currencyCode": "EUR",
  "calculationDate": "2023-10-27T10:00:00Z",
  "countryBreakdowns": [
    {
      "countryCode": "GB",
      "countryName": "United Kingdom",
      "baseCost": 1200.00,
      "additionalCost": 300.00,
      "totalCost": 1500.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule"]
    },
    {
      "countryCode": "DE",
      "countryName": "Germany",
      "baseCost": 1000.00,
      "additionalCost": 250.00,
      "totalCost": 1250.00,
      "appliedRules": ["VATRateRule", "ComplexityRule"]
    },
    {
      "countryCode": "FR",
      "countryName": "France",
      "baseCost": 1200.00,
      "additionalCost": 300.00,
      "totalCost": 1500.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule"]
    }
  ],
  "additionalServices": ["TaxConsultancy"],
  "discounts": {
    "VolumeDiscount": 200.00,
    "MultiCountryDiscount": 150.00
  }
}
```

### Error Responses

- `400 Bad Request`: Invalid request parameters. See [Common Error Responses](#common-error-responses) for details.
- `401 Unauthorized`: Missing or invalid JWT token. See [Common Error Responses](#common-error-responses) for details.
- `404 Not Found`: Calculation with the specified ID not found. See [Common Error Responses](#common-error-responses) for details.
- `500 Internal Server Error`: An unexpected error occurred during the retrieval. See [Common Error Responses](#common-error-responses) for details.

### Usage Notes

- The `id` parameter must be a valid UUID.

## Save Calculation Endpoint

Saves a calculation result for future reference.

- **HTTP Method:** `POST`
- **URL Path:** `/api/v1/pricing/save`

### Request Model

```json
{
  "serviceType": "StandardFiling",
  "transactionVolume": 500,
  "frequency": "Quarterly",
  "totalCost": 4250.00,
  "currencyCode": "EUR",
  "countryBreakdowns": [
    {
      "countryCode": "GB",
      "countryName": "United Kingdom",
      "baseCost": 1200.00,
      "additionalCost": 300.00,
      "totalCost": 1500.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule"]
    },
    {
      "countryCode": "DE",
      "countryName": "Germany",
      "baseCost": 1000.00,
      "additionalCost": 250.00,
      "totalCost": 1250.00,
      "appliedRules": ["VATRateRule", "ComplexityRule"]
    },
    {
      "countryCode": "FR",
      "countryName": "France",
      "baseCost": 1200.00,
      "additionalCost": 300.00,
      "totalCost": 1500.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule"]
    }
  ],
  "additionalServices": ["TaxConsultancy"],
  "discounts": {
    "VolumeDiscount": 200.00,
    "MultiCountryDiscount": 150.00
  }
}
```

| Parameter | Type | Description |
|---|---|---|
| `serviceType` | enum (string) | The type of VAT filing service. Possible values: `StandardFiling`, `ComplexFiling`, `PriorityService`. |
| `transactionVolume` | integer | The number of transactions/invoices per period. |
| `frequency` | enum (string) | The filing frequency. Possible values: `Monthly`, `Quarterly`, `Annually`. |
| `totalCost` | decimal | The total estimated cost for VAT filing. |
| `currencyCode` | string | The currency code for the total cost. |
| `countryBreakdowns` | array (object) | An array of country-specific cost breakdowns. |
| `additionalServices` | array (string) | An array of service identifiers for additional services included in the calculation. |
| `discounts` | object | A dictionary of discounts applied to the calculation, with discount names as keys and discount amounts as values. |

### Response Model

```json
{
  "calculationId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "calculationDate": "2023-10-27T10:00:00Z"
}
```

| Field | Type | Description |
|---|---|---|
| `calculationId` | string | Unique identifier for the saved calculation. |
| `calculationDate` | datetime | The date and time when the calculation was saved. |

### Example Request

```
POST /api/v1/pricing/save
Content-Type: application/json
Authorization: Bearer <JWT_TOKEN>

{
  "serviceType": "ComplexFiling",
  "transactionVolume": 1200,
  "frequency": "Monthly",
  "totalCost": 7800.00,
  "currencyCode": "EUR",
  "countryBreakdowns": [
    {
      "countryCode": "GB",
      "countryName": "United Kingdom",
      "baseCost": 1800.00,
      "additionalCost": 500.00,
      "totalCost": 2300.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule", "ComplexityRule"]
    },
    {
      "countryCode": "DE",
      "countryName": "Germany",
      "baseCost": 1500.00,
      "additionalCost": 400.00,
      "totalCost": 1900.00,
      "appliedRules": ["VATRateRule", "ComplexityRule"]
    },
    {
      "countryCode": "FR",
      "countryName": "France",
      "baseCost": 1700.00,
      "additionalCost": 450.00,
      "totalCost": 2150.00,
      "appliedRules": ["VATRateRule", "VolumeDiscountRule", "ComplexityRule"]
    },
    {
      "countryCode": "IT",
      "countryName": "Italy",
      "baseCost": 1000.00,
      "additionalCost": 450.00,
      "totalCost": 1450.00,
      "appliedRules": ["VATRateRule", "ComplexityRule"]
    }
  ],
  "additionalServices": ["TaxConsultancy", "ReconciliationServices"],
  "discounts": {
    "VolumeDiscount": 300.00,
    "MultiCountryDiscount": 200.00
  }
}
```

### Example Response

```
HTTP/1.1 201 Created
Content-Type: application/json
Location: /api/v1/pricing/b9d87a65-4321-4fed-9876-543210abcdef

{
  "calculationId": "b9d87a65-4321-4fed-9876-543210abcdef",
  "calculationDate": "2023-10-27T10:00:00Z"
}
```

### Error Responses

- `400 Bad Request`: Invalid request parameters. See [Common Error Responses](#common-error-responses) for details.
- `401 Unauthorized`: Missing or invalid JWT token. See [Common Error Responses](#common-error-responses) for details.
- `500 Internal Server Error`: An unexpected error occurred during the save operation. See [Common Error Responses](#common-error-responses) for details.

### Usage Notes

- The `totalCost` should be the sum of all `countryBreakdowns` costs, minus any `discounts`.
- The `currencyCode` should be a valid ISO 4217 currency code.

## Get Calculation History Endpoint

Retrieves calculation history for the current user with optional filtering.

- **HTTP Method:** `GET`
- **URL Path:** `/api/v1/pricing/history`

### Request Parameters

| Parameter | Type | Description |
|---|---|---|
| `startDate` | datetime (optional) | Filter calculations created after this date. |
| `endDate` | datetime (optional) | Filter calculations created before this date. |
| `countryCodes` | array (string, optional) | Filter calculations including any of these ISO 3166-1 alpha-2 country codes. |
| `serviceType` | enum (string, optional) | Filter calculations by service type. Possible values: `StandardFiling`, `ComplexFiling`, `PriorityService`. |
| `page` | integer (optional, default: 1) | The page number for pagination. |
| `pageSize` | integer (optional, default: 10) | The number of calculations per page. |

### Response Model

```json
{
  "items": [
    {
      "calculationId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "serviceType": "StandardFiling",
      "transactionVolume": 500,
      "frequency": "Quarterly",
      "totalCost": 4250.00,
      "currencyCode": "EUR",
      "calculationDate": "2023-10-27T10:00:00Z",
      "countries": ["United Kingdom", "Germany", "France"]
    },
    {
      "calculationId": "f1e2d3c4-b5a6-7890-1234-567890abcdef",
      "serviceType": "ComplexFiling",
      "transactionVolume": 1200,
      "frequency": "Monthly",
      "totalCost": 7800.00,
      "currencyCode": "EUR",
      "calculationDate": "2023-10-26T15:30:00Z",
      "countries": ["United Kingdom", "Germany", "France", "Italy"]
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

| Field | Type | Description |
|---|---|---|
| `items` | array (object) | An array of calculation summaries for the current page. |
| `pageNumber` | integer | The current page number. |
| `pageSize` | integer | The number of calculations per page. |
| `totalCount` | integer | The total number of calculations across all pages. |
| `totalPages` | integer | The total number of pages. |
| `hasPreviousPage` | boolean | Indicates whether there is a previous page. |
| `hasNextPage` | boolean | Indicates whether there is a next page. |

### Example Request

```
GET /api/v1/pricing/history?startDate=2023-01-01&endDate=2023-12-31&countryCodes=GB,DE&serviceType=ComplexFiling&page=2&pageSize=5
Authorization: Bearer <JWT_TOKEN>
```

### Example Response

```
HTTP/1.1 200 OK
Content-Type: application/json

{
  "items": [
    {
      "calculationId": "f1e2d3c4-b5a6-7890-1234-567890abcdef",
      "serviceType": "ComplexFiling",
      "transactionVolume": 1200,
      "frequency": "Monthly",
      "totalCost": 7800.00,
      "currencyCode": "EUR",
      "calculationDate": "2023-10-26T15:30:00Z",
      "countries": ["United Kingdom", "Germany", "France", "Italy"]
    },
    {
      "calculationId": "c4d3e2f1-a6b5-7890-1234-567890abcdef",
      "serviceType": "ComplexFiling",
      "transactionVolume": 800,
      "frequency": "Quarterly",
      "totalCost": 5500.00,
      "currencyCode": "EUR",
      "calculationDate": "2023-10-25T09:00:00Z",
      "countries": ["United Kingdom", "Germany"]
    },
    {
      "calculationId": "d4c3b2a1-f6e5-7890-1234-567890abcdef",
      "serviceType": "ComplexFiling",
      "transactionVolume": 2000,
      "frequency": "Monthly",
      "totalCost": 9200.00,
      "currencyCode": "EUR",
      "calculationDate": "2023-10-24T18:45:00Z",
      "countries": ["United Kingdom", "Germany", "France", "Italy", "Spain"]
    },
    {
      "calculationId": "e4f3d2c1-a5b6-7890-1234-567890abcdef",
      "serviceType": "ComplexFiling",
      "transactionVolume": 600,
      "frequency": "Monthly",
      "totalCost": 4800.00,
      "currencyCode": "EUR",
      "calculationDate": "2023-10-23T12:15:00Z",
      "countries": ["United Kingdom", "Germany"]
    },
    {
      "calculationId": "a5b6c7d8-e9f0-7890-1234-567890abcdef",
      "serviceType": "ComplexFiling",
      "transactionVolume": 1500,
      "frequency": "Quarterly",
      "totalCost": 8500.00,
      "currencyCode": "EUR",
      "calculationDate": "2023-10-22T21:00:00Z",
      "countries": ["United Kingdom", "Germany", "France", "Italy"]
    }
  ],
  "pageNumber": 2,
  "pageSize": 5,
  "totalCount": 25,
  "totalPages": 5,
  "hasPreviousPage": true,
  "hasNextPage": false
}
```

### Error Responses

- `400 Bad Request`: Invalid request parameters. See [Common Error Responses](#common-error-responses) for details.
- `401 Unauthorized`: Missing or invalid JWT token. See [Common Error Responses](#common-error-responses) for details.
- `500 Internal Server Error`: An unexpected error occurred during the retrieval. See [Common Error Responses](#common-error-responses) for details.

### Usage Notes

- The `startDate` and `endDate` parameters should be in ISO 8601 format (e.g., `2023-01-01`).
- The `countryCodes` parameter should be a comma-separated list of ISO 3166-1 alpha-2 country codes.
- The `page` and `pageSize` parameters should be positive integers.

## Compare Calculations Endpoint

Compares multiple pricing scenarios to help users identify the most cost-effective options.

- **HTTP Method:** `POST`
- **URL Path:** `/api/v1/pricing/compare`

### Request Model

```json
{
  "scenarios": [
    {
      "scenarioId": "scenario1",
      "scenarioName": "Scenario 1",
      "serviceType": "StandardFiling",
      "transactionVolume": 500,
      "frequency": "Quarterly",
      "countryCodes": ["GB", "DE", "FR"],
      "additionalServices": ["TaxConsultancy"]
    },
    {
      "scenarioId": "scenario2",
      "scenarioName": "Scenario 2",
      "serviceType": "ComplexFiling",
      "transactionVolume": 800,
      "frequency": "Monthly",
      "countryCodes": ["GB", "DE"],
      "additionalServices": ["TaxConsultancy", "ReconciliationServices"]
    }
  ]
}
```

| Parameter | Type | Description |
|---|---|---|
| `scenarios` | array (object) | An array of calculation scenarios to compare. |
| `scenarioId` | string | Unique identifier for the scenario. |
| `scenarioName` | string | Descriptive name for the scenario. |
| `serviceType` | enum (string) | The type of VAT filing service. Possible values: `StandardFiling`, `ComplexFiling`, `PriorityService`. |
| `transactionVolume` | integer | The number of transactions/invoices per period. |
| `frequency` | enum (string) | The filing frequency. Possible values: `Monthly`, `Quarterly`, `Annually`. |
| `countryCodes` | array (string) | An array of ISO 3166-1 alpha-2 country codes for which to calculate VAT filing costs. |
| `additionalServices` | array (string) | An array of service identifiers for additional services to include in the calculation. |

### Response Model

```json
{
  "scenarios": [
    {
      "scenarioId": "scenario1",
      "scenarioName": "Scenario 1",
      "serviceType": "StandardFiling",
      "transactionVolume": 500,
      "frequency": "Quarterly",
      "totalCost": 4250.00,
      "currencyCode": "EUR",
      "countryBreakdowns": [
        {
          "countryCode": "GB",
          "countryName": "United Kingdom",
          "baseCost": 1200.00,
          "additionalCost": 300.00,
          "totalCost": 1500.00,
          "appliedRules": ["VATRateRule", "VolumeDiscountRule"]
        },
        {
          "countryCode": "DE",
          "countryName": "Germany",
          "baseCost": 1000.00,
          "additionalCost": 250.00,
          "totalCost": 1250.00,
          "appliedRules": ["VATRateRule", "ComplexityRule"]
        },
        {
          "countryCode": "FR",
          "countryName": "France",
          "baseCost": 1200.00,
          "additionalCost": 300.00,
          "totalCost": 1500.00,
          "appliedRules": ["VATRateRule", "VolumeDiscountRule"]
        }
      ],
      "additionalServices": ["TaxConsultancy"]
    },
    {
      "scenarioId": "scenario2",
      "scenarioName": "Scenario 2",
      "serviceType": "ComplexFiling",
      "transactionVolume": 800,
      "frequency": "Monthly",
      "totalCost": 5500.00,
      "currencyCode": "EUR",
      "countryBreakdowns": [
        {
          "countryCode": "GB",
          "countryName": "United Kingdom",
          "baseCost": 1500.00,
          "additionalCost": 400.00,
          "totalCost": 1900.00,
          "appliedRules": ["VATRateRule", "VolumeDiscountRule", "ComplexityRule"]
        },
        {
          "countryCode": "DE",
          "countryName": "Germany",
          "baseCost": 1200.00,
          "additionalCost": 300.00,
          "totalCost": 1500.00,
          "appliedRules": ["VATRateRule", "ComplexityRule"]
        }
      ],
      "additionalServices": ["TaxConsultancy", "ReconciliationServices"]
    }
  ],
  "totalCostComparison": {
    "scenario1": 4250.00,
    "scenario2": 5500.00
  },
  "countryCostComparison": {
    "GB": {
      "scenario1": 1500.00,
      "scenario2": 1900.00
    },
    "DE": {
      "scenario1": 1250.00,
      "scenario2": 1500.00
    },
    "FR": {
      "scenario1": 1500.00
    }
  }
}
```

| Field | Type | Description |
|---|---|---|
| `scenarios` | array (object) | An array of calculation scenarios with detailed cost breakdowns. |
| `totalCostComparison` | object | A dictionary of scenario IDs to total costs. |
| `countryCostComparison` | object | A dictionary of country codes to dictionaries of scenario IDs to costs. |

### Example Request

```
POST /api/v1/pricing/compare
Content-Type: application/json
Authorization: Bearer <JWT_TOKEN>

{
  "scenarios": [
    {
      "scenarioId": "scenario1",
      "scenarioName": "Standard Filing",
      "serviceType": "StandardFiling",
      "transactionVolume": 800,
      "frequency": "Monthly",
      "countryCodes": ["GB", "DE", "FR"]
    },
    {
      "scenarioId": "scenario2",
      "scenarioName": "Complex Filing",
      "serviceType": "ComplexFiling",
      "transactionVolume": 800,
      "frequency": "Monthly",
      "countryCodes": ["GB", "DE", "FR"],
      "additionalServices": ["TaxConsultancy"]
    }
  ]
}
```

### Example Response