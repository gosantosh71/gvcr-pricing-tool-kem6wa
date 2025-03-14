# Administrative API Endpoints

This document provides comprehensive documentation for the administrative API endpoints of the VAT Filing Pricing Tool. These endpoints are used to manage users, configure pricing rules, manage countries, adjust system settings, and view audit logs.

## Table of Contents

- [Overview](#overview)
- [Authentication Requirements](#authentication-requirements)
- [User Management Endpoints](#user-management-endpoints)
- [Rule Management Endpoints](#rule-management-endpoints)
- [Country Management Endpoints](#country-management-endpoints)
- [System Settings Endpoints](#system-settings-endpoints)
- [Audit Logging Endpoints](#audit-logging-endpoints)
- [Admin Dashboard Endpoints](#admin-dashboard-endpoints)
- [Request and Response Models](#request-and-response-models)

## Overview

The administrative API endpoints provide secure access to system configuration and management features. These endpoints are only accessible to users with appropriate administrative roles.

### Common Response Format

All administrative endpoints follow a consistent response format:

**Success Response**:
```json
{
  "success": true,
  "data": { /* Response data varies by endpoint */ }
}
```

**Error Response**:
```json
{
  "success": false,
  "message": "Error message explaining what went wrong",
  "errors": {
    /* Field-specific errors if applicable */
    "fieldName": ["Error message for this field"]
  }
}
```

### Error Handling

| Status Code | Description |
|-------------|-------------|
| 400 | Bad Request - The request was invalid or cannot be processed |
| 401 | Unauthorized - Authentication is required or has failed |
| 403 | Forbidden - The authenticated user does not have the required permissions |
| 404 | Not Found - The requested resource was not found |
| 409 | Conflict - The request conflicts with the current state of the resource |
| 500 | Internal Server Error - An unexpected server error occurred |

## Authentication Requirements

All administrative endpoints require authentication using a JWT bearer token with appropriate administrative roles. For detailed authentication information, refer to the [Authentication Documentation](authentication.md).

Administrative endpoints have the following security requirements:

1. **Role Requirements**: Requires System Administrator, Pricing Administrator, or other specific administrator roles as specified for each endpoint.
2. **MFA Verification**: Recent MFA verification (within the last 15 minutes) is required for sensitive operations.
3. **Enhanced Logging**: All administrative actions are logged for audit purposes.
4. **Rate Limiting**: Administrative endpoints have stricter rate limits to prevent abuse.

## User Management Endpoints

Endpoints for managing user accounts and roles within the system.

### GET /api/admin/users/{userId}

Retrieves detailed information about a specific user.

**Authentication**: Bearer Token with System Administrator role

**Parameters**:
- `userId` (path, required) - The unique identifier of the user

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "user123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Smith",
    "roles": ["Customer"],
    "isActive": true,
    "createdDate": "2023-01-15T10:30:45Z",
    "lastLoginDate": "2023-05-20T14:25:30Z"
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "message": "User not found",
  "errors": null
}
```

### GET /api/admin/users

Retrieves a paginated list of users with optional filtering.

**Authentication**: Bearer Token with System Administrator role

**Query Parameters**:
- `page` (optional, default: 1) - Page number for pagination
- `pageSize` (optional, default: 20) - Number of items per page
- `searchTerm` (optional) - Search by name or email
- `roleFilter` (optional) - Filter users by role
- `activeOnly` (optional, default: true) - Filter by active status

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "userId": "user123",
        "email": "user@example.com",
        "firstName": "John",
        "lastName": "Smith",
        "roles": ["Customer"],
        "isActive": true,
        "createdDate": "2023-01-15T10:30:45Z",
        "lastLoginDate": "2023-05-20T14:25:30Z"
      },
      {
        /* Additional user records */
      }
    ],
    "pageNumber": 1,
    "totalCount": 150
  }
}
```

### GET /api/admin/users/summaries

Retrieves a simplified list of all users, useful for dropdowns and user selection interfaces.

**Authentication**: Bearer Token with System Administrator role

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "userId": "user123",
      "email": "user@example.com",
      "name": "John Smith"
    },
    {
      /* Additional user summaries */
    }
  ]
}
```

### PUT /api/admin/users/{userId}/roles

Updates the roles assigned to a specific user.

**Authentication**: Bearer Token with System Administrator role

**Parameters**:
- `userId` (path, required) - The unique identifier of the user

**Request Body**:
```json
{
  "roles": ["Customer", "Accountant"]
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "user123",
    "success": true,
    "roles": ["Customer", "Accountant"]
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Invalid roles specified",
  "errors": {
    "roles": ["One or more of the specified roles do not exist"]
  }
}
```

### PUT /api/admin/users/{userId}/status

Activates or deactivates a user account.

**Authentication**: Bearer Token with System Administrator role

**Parameters**:
- `userId` (path, required) - The unique identifier of the user

**Request Body**:
```json
{
  "isActive": false
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "user123",
    "success": true,
    "isActive": false
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "message": "User not found",
  "errors": null
}
```

### GET /api/admin/users/current

Retrieves the current authenticated user's information.

**Authentication**: Bearer Token with any Administrator role

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "admin123",
    "email": "admin@example.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["System Administrator"],
    "isActive": true,
    "createdDate": "2022-12-01T09:00:00Z",
    "lastLoginDate": "2023-05-25T08:15:30Z"
  }
}
```

## Rule Management Endpoints

Endpoints for managing pricing rules and tax jurisdiction configurations.

### GET /api/admin/rules/{ruleId}

Retrieves a specific rule by its ID.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Parameters**:
- `ruleId` (path, required) - The unique identifier of the rule

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "ruleId": "rule123",
    "countryCode": "GB",
    "ruleType": "VatRate",
    "name": "UK Standard VAT Rate",
    "description": "Standard VAT rate for UK filings",
    "expression": "basePrice * 0.20",
    "parameters": ["basePrice"],
    "conditions": [
      {
        "parameter": "serviceType",
        "operator": "equals",
        "value": "StandardFiling"
      }
    ],
    "effectiveFrom": "2023-01-01",
    "effectiveTo": null,
    "priority": 100,
    "isActive": true
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "message": "Rule not found",
  "errors": null
}
```

### GET /api/admin/rules/country/{countryCode}

Retrieves rules for a specific country.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Parameters**:
- `countryCode` (path, required) - The country code to retrieve rules for
- `activeOnly` (query, optional, default: true) - Filter for active rules only
- `ruleType` (query, optional) - Filter by rule type
- `pageNumber` (query, optional, default: 1) - Page number for pagination
- `pageSize` (query, optional, default: 20) - Number of items per page

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "ruleId": "rule123",
        "countryCode": "GB",
        "ruleType": "VatRate",
        "name": "UK Standard VAT Rate",
        "description": "Standard VAT rate for UK filings",
        "expression": "basePrice * 0.20",
        "parameters": ["basePrice"],
        "conditions": [
          {
            "parameter": "serviceType",
            "operator": "equals",
            "value": "StandardFiling"
          }
        ],
        "effectiveFrom": "2023-01-01",
        "effectiveTo": null,
        "priority": 100,
        "isActive": true
      },
      {
        /* Additional rule records */
      }
    ],
    "pageNumber": 1,
    "totalCount": 15
  }
}
```

### GET /api/admin/rules/summaries

Retrieves a simplified list of all rules.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "ruleId": "rule123",
      "name": "UK Standard VAT Rate",
      "ruleType": "VatRate"
    },
    {
      /* Additional rule summaries */
    }
  ]
}
```

### POST /api/admin/rules

Creates a new pricing rule.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Request Body**:
```json
{
  "countryCode": "DE",
  "ruleType": "VatRate",
  "name": "Germany Standard VAT Rate",
  "description": "Standard VAT rate for German filings",
  "expression": "basePrice * 0.19",
  "parameters": ["basePrice"],
  "conditions": [
    {
      "parameter": "serviceType",
      "operator": "equals",
      "value": "StandardFiling"
    }
  ]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "ruleId": "rule456",
    "success": true
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Invalid rule parameters",
  "errors": {
    "expression": ["Invalid rule expression format"]
  }
}
```

### PUT /api/admin/rules/{ruleId}

Updates an existing pricing rule.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Parameters**:
- `ruleId` (path, required) - The unique identifier of the rule

**Request Body**:
```json
{
  "name": "Germany Updated VAT Rate",
  "description": "Updated standard VAT rate for German filings",
  "expression": "basePrice * 0.19",
  "parameters": ["basePrice"],
  "conditions": [
    {
      "parameter": "serviceType",
      "operator": "equals",
      "value": "StandardFiling"
    }
  ]
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "ruleId": "rule456",
    "success": true
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "message": "Rule not found",
  "errors": null
}
```

### DELETE /api/admin/rules/{ruleId}

Deletes a pricing rule.

**Authentication**: Bearer Token with System Administrator role

**Parameters**:
- `ruleId` (path, required) - The unique identifier of the rule

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "ruleId": "rule456",
    "success": true
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "message": "Rule not found",
  "errors": null
}
```

### POST /api/admin/rules/validate

Validates a rule expression without creating the rule.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Request Body**:
```json
{
  "expression": "basePrice * vatRate + additionalFee",
  "parameters": ["basePrice", "vatRate", "additionalFee"],
  "sampleValues": {
    "basePrice": 100,
    "vatRate": 0.2,
    "additionalFee": 10
  }
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "isValid": true,
    "message": "Expression is valid",
    "evaluationResult": 30
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "data": {
    "isValid": false,
    "message": "Invalid parameter reference in expression",
    "evaluationResult": null
  }
}
```

### POST /api/admin/rules/import

Imports multiple rules from a file or bulk data.

**Authentication**: Bearer Token with System Administrator role

**Request Body**:
```json
{
  "rules": [
    {
      "countryCode": "FR",
      "ruleType": "VatRate",
      "name": "France Standard VAT Rate",
      "description": "Standard VAT rate for French filings",
      "expression": "basePrice * 0.20",
      "parameters": ["basePrice"],
      "conditions": []
    },
    {
      /* Additional rules */
    }
  ],
  "overwriteExisting": false
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "totalRules": 5,
    "importedRules": 4,
    "failedRules": 1,
    "success": true
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Rule import failed",
  "errors": {
    "rules[1]": ["Invalid country code"]
  }
}
```

### GET /api/admin/rules/export

Exports rules to a downloadable format.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Query Parameters**:
- `countryCode` (optional) - Filter by country code
- `ruleType` (optional) - Filter by rule type
- `format` (optional, default: "json") - Export format (json, csv, excel)

**Success Response** (200 OK):
File download with appropriate Content-Type header.

## Country Management Endpoints

Endpoints for managing countries and their VAT requirements.

### GET /api/admin/countries/{countryCode}

Retrieves detailed information about a specific country.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Parameters**:
- `countryCode` (path, required) - The country code to retrieve

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "countryCode": "GB",
    "name": "United Kingdom",
    "standardVatRate": 20.0,
    "currencyCode": "GBP",
    "availableFilingFrequencies": ["Monthly", "Quarterly", "Annually"],
    "isActive": true,
    "lastUpdated": "2023-04-01T00:00:00Z"
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "message": "Country not found",
  "errors": null
}
```

### GET /api/admin/countries

Retrieves a list of countries with optional filtering.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Query Parameters**:
- `activeOnly` (optional, default: true) - Filter by active status
- `countryCodes` (optional) - Comma-separated list of country codes to include
- `page` (optional, default: 1) - Page number for pagination
- `pageSize` (optional, default: 50) - Number of items per page

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "countryCode": "GB",
        "name": "United Kingdom",
        "standardVatRate": 20.0,
        "currencyCode": "GBP",
        "availableFilingFrequencies": ["Monthly", "Quarterly", "Annually"],
        "isActive": true,
        "lastUpdated": "2023-04-01T00:00:00Z"
      },
      {
        /* Additional country records */
      }
    ],
    "pageNumber": 1,
    "totalCount": 27
  }
}
```

### GET /api/admin/countries/active

Retrieves a list of all active countries.

**Authentication**: Bearer Token with any Administrator role

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "countryCode": "GB",
      "name": "United Kingdom",
      "standardVatRate": 20.0,
      "currencyCode": "GBP",
      "availableFilingFrequencies": ["Monthly", "Quarterly", "Annually"],
      "isActive": true,
      "lastUpdated": "2023-04-01T00:00:00Z"
    },
    {
      /* Additional country records */
    }
  ]
}
```

### GET /api/admin/countries/filing-frequency/{frequency}

Retrieves countries that support a specific filing frequency.

**Authentication**: Bearer Token with any Administrator role

**Parameters**:
- `frequency` (path, required) - The filing frequency to filter by (Monthly, Quarterly, Annually)

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "countryCode": "GB",
      "name": "United Kingdom",
      "standardVatRate": 20.0,
      "currencyCode": "GBP",
      "availableFilingFrequencies": ["Monthly", "Quarterly", "Annually"],
      "isActive": true,
      "lastUpdated": "2023-04-01T00:00:00Z"
    },
    {
      /* Additional country records */
    }
  ]
}
```

### GET /api/admin/countries/summaries

Retrieves a simplified list of all countries, useful for dropdowns and selection interfaces.

**Authentication**: Bearer Token with any Administrator role

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "countryCode": "GB",
      "name": "United Kingdom"
    },
    {
      /* Additional country summaries */
    }
  ]
}
```

### POST /api/admin/countries

Creates a new country entry.

**Authentication**: Bearer Token with System Administrator role

**Request Body**:
```json
{
  "countryCode": "ES",
  "name": "Spain",
  "standardVatRate": 21.0,
  "currencyCode": "EUR",
  "availableFilingFrequencies": ["Monthly", "Quarterly"]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "countryCode": "ES",
    "success": true
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Invalid country data",
  "errors": {
    "countryCode": ["Country code already exists"]
  }
}
```

### PUT /api/admin/countries/{countryCode}

Updates an existing country entry.

**Authentication**: Bearer Token with System Administrator or Pricing Administrator role

**Parameters**:
- `countryCode` (path, required) - The country code to update

**Request Body**:
```json
{
  "name": "Spain",
  "standardVatRate": 21.0,
  "currencyCode": "EUR",
  "availableFilingFrequencies": ["Monthly", "Quarterly", "Annually"],
  "isActive": true
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "countryCode": "ES",
    "success": true
  }
}
```

**Error Response** (404 Not Found):
```json
{
  "success": false,
  "message": "Country not found",
  "errors": null
}
```

### DELETE /api/admin/countries/{countryCode}

Deletes a country entry.

**Authentication**: Bearer Token with System Administrator role

**Parameters**:
- `countryCode` (path, required) - The country code to delete

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "countryCode": "ES",
    "success": true
  }
}
```

**Error Response** (409 Conflict):
```json
{
  "success": false,
  "message": "Cannot delete country with active rules",
  "errors": null
}
```

## System Settings Endpoints

Endpoints for managing system-wide settings and configurations.

### GET /api/admin/settings

Retrieves system settings.

**Authentication**: Bearer Token with System Administrator role

**Query Parameters**:
- `category` (optional) - Filter settings by category

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "settings": {
      "defaultCurrency": "EUR",
      "defaultVatRate": 20.0,
      "defaultFilingFrequency": "Quarterly",
      "allowUserRegistration": true,
      "maintenanceMode": false,
      "maintenanceMessage": "",
      "calculationCacheTimeMinutes": 15,
      "maxConcurrentCalculations": 100,
      "maxReportSize": 10485760,
      "apiRateLimitPerMinute": 100
    },
    "categories": ["General", "Pricing", "Security", "Performance"]
  }
}
```

### PUT /api/admin/settings

Updates system settings.

**Authentication**: Bearer Token with System Administrator role

**Request Body**:
```json
{
  "settings": {
    "defaultCurrency": "USD",
    "defaultVatRate": 20.0,
    "defaultFilingFrequency": "Quarterly",
    "allowUserRegistration": true,
    "maintenanceMode": false,
    "maintenanceMessage": "",
    "calculationCacheTimeMinutes": 30,
    "maxConcurrentCalculations": 100,
    "maxReportSize": 10485760,
    "apiRateLimitPerMinute": 100
  }
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "success": true,
    "updatedSettings": ["defaultCurrency", "calculationCacheTimeMinutes"]
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Invalid settings data",
  "errors": {
    "settings.apiRateLimitPerMinute": ["Value must be between 10 and 1000"]
  }
}
```

## Audit Logging Endpoints

Endpoints for retrieving and analyzing system audit logs.

### GET /api/admin/audit-logs

Retrieves audit logs with filtering and pagination.

**Authentication**: Bearer Token with System Administrator or Security Administrator role

**Query Parameters**:
- `startDate` (optional) - Filter logs after this date
- `endDate` (optional) - Filter logs before this date
- `userId` (optional) - Filter logs by user ID
- `eventType` (optional) - Filter logs by event type
- `page` (optional, default: 1) - Page number for pagination
- `pageSize` (optional, default: 50) - Number of items per page

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "logId": "log123",
        "timestamp": "2023-05-20T14:25:30Z",
        "userId": "admin123",
        "userName": "Admin User",
        "eventType": "UserCreated",
        "resourceType": "User",
        "resourceId": "user456",
        "description": "Created new user account",
        "details": {
          "email": "newuser@example.com",
          "roles": ["Customer"]
        }
      },
      {
        /* Additional audit log records */
      }
    ],
    "pageNumber": 1,
    "totalCount": 1250
  }
}
```

## Admin Dashboard Endpoints

Endpoints for retrieving administrative dashboard data.

### GET /api/admin/dashboard

Retrieves summary data for the administrative dashboard.

**Authentication**: Bearer Token with any Administrator role

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userStats": {
      "totalUsers": 1250,
      "activeUsers": 1180,
      "newUsersToday": 5,
      "adminUsers": 8
    },
    "ruleStats": {
      "totalRules": 150,
      "activeRules": 145,
      "countriesWithRules": 27
    },
    "countryStats": {
      "totalCountries": 30,
      "activeCountries": 27
    },
    "calculationStats": {
      "calculationsToday": 350,
      "calculationsThisWeek": 2450,
      "calculationsThisMonth": 10250,
      "averageCalculationTime": 1.5
    },
    "recentActivity": [
      {
        "timestamp": "2023-05-25T13:45:30Z",
        "eventType": "RuleModified",
        "description": "Updated VAT rate for Germany",
        "userId": "admin123",
        "userName": "Admin User"
      },
      {
        /* Additional activity records */
      }
    ]
  }
}
```

## Request and Response Models

### User Management Models

#### Request Models

##### GetUserRequest
- `userId` (string, required) - The unique identifier of the user

##### GetUsersRequest
- `page` (integer, optional, default: 1) - Page number for pagination
- `pageSize` (integer, optional, default: 20) - Number of items per page
- `searchTerm` (string, optional) - Search by name or email
- `roleFilter` (string, optional) - Filter users by role
- `activeOnly` (boolean, optional, default: true) - Filter by active status

##### UpdateUserRolesRequest
- `userId` (string, required) - The unique identifier of the user
- `roles` (array of strings, required) - The roles to assign to the user

##### ChangeUserStatusRequest
- `userId` (string, required) - The unique identifier of the user
- `isActive` (boolean, required) - The new active status for the user

#### Response Models

##### UserResponse
- `userId` (string) - The unique identifier of the user
- `email` (string) - The user's email address
- `firstName` (string) - The user's first name
- `lastName` (string) - The user's last name
- `roles` (array of strings) - The roles assigned to the user
- `isActive` (boolean) - Whether the user account is active
- `createdDate` (string, date-time) - When the user account was created
- `lastLoginDate` (string, date-time) - When the user last logged in

##### UserListResponse
- `items` (array of UserResponse) - The list of users
- `pageNumber` (integer) - The current page number
- `totalCount` (integer) - The total number of users matching the filters

##### UserSummaryResponse
- `userId` (string) - The unique identifier of the user
- `email` (string) - The user's email address
- `name` (string) - The user's full name

##### UpdateUserRolesResponse
- `userId` (string) - The unique identifier of the user
- `success` (boolean) - Whether the update was successful
- `roles` (array of strings) - The updated roles assigned to the user

##### ChangeUserStatusResponse
- `userId` (string) - The unique identifier of the user
- `success` (boolean) - Whether the update was successful
- `isActive` (boolean) - The updated active status of the user

### Rule Management Models

#### Request Models

##### GetRuleRequest
- `ruleId` (string, required) - The unique identifier of the rule

##### GetRulesByCountryRequest
- `countryCode` (string, required) - The country code to retrieve rules for
- `activeOnly` (boolean, optional, default: true) - Filter for active rules only
- `ruleType` (string, optional) - Filter by rule type
- `pageNumber` (integer, optional, default: 1) - Page number for pagination
- `pageSize` (integer, optional, default: 20) - Number of items per page

##### CreateRuleRequest
- `countryCode` (string, required) - The country code the rule applies to
- `ruleType` (string, required) - The type of rule
- `name` (string, required) - The name of the rule
- `description` (string, optional) - A description of the rule
- `expression` (string, required) - The rule calculation expression
- `parameters` (array of strings, required) - The parameters used in the expression
- `conditions` (array of objects, optional) - Conditions for when the rule applies
  - `parameter` (string, required) - The parameter name for the condition
  - `operator` (string, required) - The comparison operator
  - `value` (any, required) - The value to compare against

##### UpdateRuleRequest
- `ruleId` (string, required) - The unique identifier of the rule
- `name` (string, optional) - The updated name of the rule
- `description` (string, optional) - The updated description
- `expression` (string, optional) - The updated calculation expression
- `parameters` (array of strings, optional) - The updated parameters
- `conditions` (array of objects, optional) - The updated conditions

##### DeleteRuleRequest
- `ruleId` (string, required) - The unique identifier of the rule

##### ValidateRuleExpressionRequest
- `expression` (string, required) - The rule calculation expression to validate
- `parameters` (array of strings, required) - The parameters used in the expression
- `sampleValues` (object, optional) - Sample values for parameters to test evaluation

##### ImportRulesRequest
- `rules` (array of objects, required) - The rules to import
- `overwriteExisting` (boolean, optional, default: false) - Whether to overwrite existing rules

#### Response Models

##### RuleResponse
- `ruleId` (string) - The unique identifier of the rule
- `countryCode` (string) - The country code the rule applies to
- `ruleType` (string) - The type of rule
- `name` (string) - The name of the rule
- `description` (string) - A description of the rule
- `expression` (string) - The rule calculation expression
- `parameters` (array of strings) - The parameters used in the expression
- `conditions` (array of objects) - Conditions for when the rule applies
- `effectiveFrom` (string, date-time) - When the rule becomes effective
- `effectiveTo` (string, date-time, nullable) - When the rule expires (null for no expiration)
- `priority` (integer) - The rule's priority (higher priorities are applied first)
- `isActive` (boolean) - Whether the rule is active

##### RulesResponse
- `items` (array of RuleResponse) - The list of rules
- `pageNumber` (integer) - The current page number
- `totalCount` (integer) - The total number of rules matching the filters

##### RuleSummaryResponse
- `ruleId` (string) - The unique identifier of the rule
- `name` (string) - The name of the rule
- `ruleType` (string) - The type of rule

##### CreateRuleResponse
- `ruleId` (string) - The unique identifier of the created rule
- `success` (boolean) - Whether the creation was successful

##### UpdateRuleResponse
- `ruleId` (string) - The unique identifier of the updated rule
- `success` (boolean) - Whether the update was successful

##### DeleteRuleResponse
- `ruleId` (string) - The unique identifier of the deleted rule
- `success` (boolean) - Whether the deletion was successful

##### ValidateRuleExpressionResponse
- `isValid` (boolean) - Whether the expression is valid
- `message` (string) - Validation message or error details
- `evaluationResult` (any, nullable) - The result of evaluating the expression with sample values

##### ImportRulesResponse
- `totalRules` (integer) - The total number of rules attempted to import
- `importedRules` (integer) - The number of rules successfully imported
- `failedRules` (integer) - The number of rules that failed to import
- `success` (boolean) - Whether the overall import was successful

### Country Management Models

#### Request Models

##### GetCountryRequest
- `countryCode` (string, required) - The country code to retrieve

##### GetCountriesRequest
- `activeOnly` (boolean, optional, default: true) - Filter by active status
- `countryCodes` (array of strings, optional) - List of country codes to include
- `page` (integer, optional, default: 1) - Page number for pagination
- `pageSize` (integer, optional, default: 50) - Number of items per page

##### CreateCountryRequest
- `countryCode` (string, required) - The country code (ISO 3166-1 alpha-2)
- `name` (string, required) - The country name
- `standardVatRate` (number, required) - The standard VAT rate percentage
- `currencyCode` (string, required) - The currency code (ISO 4217)
- `availableFilingFrequencies` (array of strings, required) - Available filing frequencies

##### UpdateCountryRequest
- `countryCode` (string, required) - The country code to update
- `name` (string, optional) - The updated country name
- `standardVatRate` (number, optional) - The updated standard VAT rate
- `currencyCode` (string, optional) - The updated currency code
- `availableFilingFrequencies` (array of strings, optional) - Updated filing frequencies
- `isActive` (boolean, optional) - Updated active status

##### DeleteCountryRequest
- `countryCode` (string, required) - The country code to delete

#### Response Models

##### CountryResponse
- `countryCode` (string) - The country code (ISO 3166-1 alpha-2)
- `name` (string) - The country name
- `standardVatRate` (number) - The standard VAT rate percentage
- `currencyCode` (string) - The currency code (ISO 4217)
- `availableFilingFrequencies` (array of strings) - Available filing frequencies
- `isActive` (boolean) - Whether the country is active in the system
- `lastUpdated` (string, date-time) - When the country was last updated

##### CountriesResponse
- `items` (array of CountryResponse) - The list of countries
- `pageNumber` (integer) - The current page number
- `totalCount` (integer) - The total number of countries matching the filters

##### CountrySummaryResponse
- `countryCode` (string) - The country code
- `name` (string) - The country name

##### CreateCountryResponse
- `countryCode` (string) - The country code of the created country
- `success` (boolean) - Whether the creation was successful

##### UpdateCountryResponse
- `countryCode` (string) - The country code of the updated country
- `success` (boolean) - Whether the update was successful

##### DeleteCountryResponse
- `countryCode` (string) - The country code of the deleted country
- `success` (boolean) - Whether the deletion was successful

### System Settings Models

#### Request Models

##### GetSystemSettingsRequest
- `category` (string, optional) - Filter settings by category

##### UpdateSystemSettingsRequest
- `settings` (object, required) - The settings to update

#### Response Models

##### SystemSettingsResponse
- `settings` (object) - The system settings key-value pairs
- `categories` (array of strings) - Available setting categories

##### UpdateSystemSettingsResponse
- `success` (boolean) - Whether the update was successful
- `updatedSettings` (array of strings) - The names of settings that were updated

### Audit Logging Models

#### Request Models

##### GetAuditLogsRequest
- `startDate` (string, date-time, optional) - Filter logs after this date
- `endDate` (string, date-time, optional) - Filter logs before this date
- `userId` (string, optional) - Filter logs by user ID
- `eventType` (string, optional) - Filter logs by event type
- `page` (integer, optional, default: 1) - Page number for pagination
- `pageSize` (integer, optional, default: 50) - Number of items per page

#### Response Models

##### AuditLogResponse
- `logId` (string) - The unique identifier of the log entry
- `timestamp` (string, date-time) - When the event occurred
- `userId` (string) - The ID of the user who performed the action
- `userName` (string) - The name of the user who performed the action
- `eventType` (string) - The type of event
- `resourceType` (string) - The type of resource affected
- `resourceId` (string) - The ID of the resource affected
- `description` (string) - A human-readable description of the event
- `details` (object) - Additional details about the event

##### AuditLogsResponse
- `items` (array of AuditLogResponse) - The list of audit logs
- `pageNumber` (integer) - The current page number
- `totalCount` (integer) - The total number of logs matching the filters

### Admin Dashboard Models

#### Response Models

##### AdminDashboardResponse
- `userStats` (object) - Statistics about users
  - `totalUsers` (integer) - Total number of users
  - `activeUsers` (integer) - Number of active users
  - `newUsersToday` (integer) - New users created today
  - `adminUsers` (integer) - Number of administrator users
- `ruleStats` (object) - Statistics about rules
  - `totalRules` (integer) - Total number of rules
  - `activeRules` (integer) - Number of active rules
  - `countriesWithRules` (integer) - Number of countries with rules
- `countryStats` (object) - Statistics about countries
  - `totalCountries` (integer) - Total number of countries
  - `activeCountries` (integer) - Number of active countries
- `calculationStats` (object) - Statistics about calculations
  - `calculationsToday` (integer) - Number of calculations today
  - `calculationsThisWeek` (integer) - Number of calculations this week
  - `calculationsThisMonth` (integer) - Number of calculations this month
  - `averageCalculationTime` (number) - Average calculation time in seconds
- `recentActivity` (array of objects) - Recent system activity
  - `timestamp` (string, date-time) - When the activity occurred
  - `eventType` (string) - The type of event
  - `description` (string) - A description of the activity
  - `userId` (string) - The ID of the user who performed the activity
  - `userName` (string) - The name of the user who performed the activity