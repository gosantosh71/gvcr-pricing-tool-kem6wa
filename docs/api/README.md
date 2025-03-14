# VAT Filing Pricing Tool API Documentation

## Overview

The VAT Filing Pricing Tool API provides a comprehensive set of endpoints for calculating VAT filing costs, managing user accounts, configuring pricing rules, and generating reports. This documentation provides an overview of the API architecture, authentication mechanisms, available endpoints, and usage guidelines.

The API is designed to be RESTful, using standard HTTP methods and JSON for data exchange. It follows a consistent structure and naming convention to ensure ease of use and integration.

### API Architecture

The API is structured around the following key functional areas:

- **Authentication**: Manages user authentication and authorization.
- **Pricing**: Calculates VAT filing costs based on various parameters.
- **Reporting**: Generates detailed reports on VAT filing costs.
- **Admin**: Provides administrative functions for managing users, rules, and settings.
- **Country**: Provides endpoints for managing countries and their VAT requirements.
- **Rule**: Provides endpoints for managing VAT rules.

### API Versioning

The API uses URL path versioning to manage different versions of the API. The current version is `v1`. All API endpoints are prefixed with `/api/v1/`.

### API Gateway

The API is accessed through an API gateway, which provides a single entry point for all API requests. The API gateway handles authentication, authorization, rate limiting, and other cross-cutting concerns.

## Authentication

The API uses JWT (JSON Web Token) authentication to secure access to protected endpoints. For more information on authentication, refer to the [Authentication API Documentation](authentication.md).

## API Endpoints

The following table provides a high-level overview of all API endpoints:

| Category | Endpoint | Description |
|---|---|---|
| Authentication | `POST /api/v1/auth/login` | Authenticates a user and returns a JWT. |
|  | `POST /api/v1/auth/register` | Registers a new user account. |
|  | `POST /api/v1/auth/password/reset` | Initiates a password reset process. |
|  | `POST /api/v1/auth/password/change` | Changes a user's password using a reset token. |
|  | `POST /api/v1/auth/token/refresh` | Refreshes an authentication token using a refresh token. |
|  | `POST /api/v1/auth/logout` | Invalidates the current user's tokens. |
|  | `GET /api/v1/auth/user` | Retrieves the current authenticated user's profile. |
|  | `GET /api/v1/auth/azure-login` | Initiates the Azure AD authentication flow. |
|  | `POST /api/v1/auth/azure-callback` | Handles the callback from Azure AD after successful authentication. |
| Pricing | `POST /api/v1/pricing/calculate` | Calculates VAT filing costs based on the provided parameters. |
|  | `GET /api/v1/pricing/{id}` | Retrieves a specific calculation by its ID. |
|  | `POST /api/v1/pricing/save` | Saves a calculation result for future reference. |
|  | `GET /api/v1/pricing/history` | Retrieves calculation history for the current user with optional filtering. |
|  | `POST /api/v1/pricing/compare` | Compares multiple pricing scenarios to help users identify the most cost-effective options. |
|  | `DELETE /api/v1/pricing/{id}` | Deletes a specific calculation by its ID. |
| Reporting | `POST /api/v1/reports/generate` | Generates a new report based on calculation data with specified format and content options. |
|  | `GET /api/v1/reports/{id}` | Retrieves detailed information about a specific report. |
|  | `GET /api/v1/reports` | Retrieves a paginated list of reports for the current user with optional filtering. |
|  | `GET /api/v1/reports/{id}/download` | Downloads a specific report, optionally converting to a different format. |
|  | `POST /api/v1/reports/{id}/email` | Emails a specific report to the specified email address. |
|  | `PUT /api/v1/reports/{id}/archive` | Archives a specific report to hide it from regular report listings. |
|  | `PUT /api/v1/reports/{id}/unarchive` | Unarchives a previously archived report to make it visible in regular report listings. |
|  | `DELETE /api/v1/reports/{id}` | Permanently deletes a report and its associated file from storage. |
| Admin | `GET /api/v1/admin/users/{userId}` | Retrieves detailed information about a specific user. |
|  | `GET /api/v1/admin/users` | Retrieves a paginated list of users with optional filtering. |
|  | `GET /api/v1/admin/users/summaries` | Retrieves a simplified list of all users. |
|  | `PUT /api/v1/admin/users/{userId}/roles` | Updates the roles assigned to a specific user. |
|  | `PUT /api/v1/admin/users/{userId}/status` | Activates or deactivates a user account. |
|  | `GET /api/v1/admin/rules/{ruleId}` | Retrieves a specific rule by its ID. |
|  | `GET /api/v1/admin/rules/country/{countryCode}` | Retrieves rules for a specific country. |
|  | `GET /api/v1/admin/rules/summaries` | Retrieves a simplified list of all rules. |
|  | `POST /api/v1/admin/rules` | Creates a new pricing rule. |
|  | `PUT /api/v1/admin/rules/{ruleId}` | Updates an existing pricing rule. |
|  | `DELETE /api/v1/admin/rules/{ruleId}` | Deletes a pricing rule. |
|  | `POST /api/v1/admin/rules/validate` | Validates a rule expression without creating the rule. |
|  | `POST /api/v1/admin/rules/import` | Imports multiple rules from a file or bulk data. |
|  | `GET /api/v1/admin/rules/export` | Exports rules to a downloadable format. |
|  | `GET /api/v1/admin/countries/{countryCode}` | Retrieves detailed information about a specific country. |
|  | `GET /api/v1/admin/countries` | Retrieves a list of countries with optional filtering. |
|  | `GET /api/v1/admin/countries/active` | Retrieves a list of all active countries. |
|  | `GET /api/v1/admin/countries/filing-frequency/{frequency}` | Retrieves countries that support a specific filing frequency. |
|  | `GET /api/v1/admin/countries/summaries` | Retrieves a simplified list of all countries. |
|  | `POST /api/v1/admin/countries` | Creates a new country entry. |
|  | `PUT /api/v1/admin/countries/{countryCode}` | Updates an existing country entry. |
|  | `DELETE /api/v1/admin/countries/{countryCode}` | Deletes a country entry. |
|  | `GET /api/v1/admin/settings` | Retrieves system settings. |
|  | `PUT /api/v1/admin/settings` | Updates system settings. |
|  | `GET /api/v1/admin/audit-logs` | Retrieves audit logs with filtering and pagination. |
|  | `GET /api/v1/admin/dashboard` | Retrieves summary data for the administrative dashboard. |

## Common Patterns

The following patterns and conventions are used across all API endpoints:

### Request/Response Format

All API requests and responses use JSON (JavaScript Object Notation) as the data format.

### Pagination

Endpoints that return lists of resources support pagination using the following query parameters:

- `page`: The page number to retrieve (1-based).
- `pageSize`: The number of items to return per page.

The response includes metadata about the pagination:

- `pageNumber`: The current page number.
- `pageSize`: The number of items per page.
- `totalCount`: The total number of items across all pages.
- `totalPages`: The total number of pages.
- `hasPreviousPage`: Indicates whether there is a previous page.
- `hasNextPage`: Indicates whether there is a next page.

### Filtering and Sorting

Endpoints that return lists of resources support filtering and sorting using query parameters. The specific filtering and sorting options vary by endpoint.

### Error Handling

API endpoints return standard HTTP status codes to indicate the success or failure of a request. Error responses include a JSON payload with a `message` and an optional `errors` field containing field-specific validation errors.

### Rate Limiting and Throttling

The API implements rate limiting and throttling to prevent abuse and ensure the stability of the system. The specific rate limits vary by endpoint and user role.

## Getting Started

To start using the API, you will need to obtain API credentials and authenticate your requests.

### Obtaining API Credentials

To obtain API credentials, you will need to register an application in Azure Active Directory (AAD) and configure the appropriate permissions.

### Authentication Process

To authenticate your requests, you will need to obtain a JWT (JSON Web Token) from the authentication endpoint and include it in the `Authorization` header of your requests.

### Making an API Request

Once you have obtained a JWT, you can make an API request by including the token in the `Authorization` header:

```
Authorization: Bearer <JWT_TOKEN>
```

### Handling API Responses

API responses are returned in JSON format. Successful responses include a `success` field set to `true` and a `data` field containing the response data. Error responses include a `success` field set to `false` and a `message` field containing an error message.

## API Security

The following security guidelines should be followed when using the API:

- Always use HTTPS to encrypt all API communication.
- Store tokens securely and never expose them to unauthorized users.
- Implement token refresh to obtain new tokens when access tokens expire.
- Validate all input data to prevent injection attacks.
- Implement rate limiting and throttling to prevent abuse.

For more detailed information on specific API endpoints, refer to the following documentation:

- [Authentication API Documentation](authentication.md)
- [Pricing API Documentation](pricing-endpoints.md)
- [Reporting API Documentation](reporting-endpoints.md)
- [Admin API Documentation](admin-endpoints.md)