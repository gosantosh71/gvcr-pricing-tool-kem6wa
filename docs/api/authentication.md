# Authentication API

This document provides a comprehensive overview of the authentication mechanisms and API endpoints for the VAT Filing Pricing Tool. It covers standard authentication, Azure AD integration, token management, and security considerations.

## Table of Contents

- [Authentication Framework Overview](#authentication-framework-overview)
- [Authentication Endpoints](#authentication-endpoints)
- [Authentication Flows](#authentication-flows)
- [Request and Response Models](#request-and-response-models)
- [Token Management](#token-management)
- [Azure AD Integration](#azure-ad-integration)
- [Identity Management](#identity-management)
- [Administrative Security](#administrative-security)
- [Protected Endpoint Authentication](#protected-endpoint-authentication)
- [Security Considerations](#security-considerations)

## Authentication Framework Overview

The VAT Filing Pricing Tool implements a robust authentication framework leveraging Azure Active Directory (AAD) as the primary identity provider, with support for standard username/password authentication as a fallback.

### Identity Management Approach

The system uses Azure AD for centralized identity management, providing:

- Enterprise-grade authentication with single sign-on (SSO) capabilities
- Integration with existing Microsoft 365 accounts
- Comprehensive user lifecycle management
- Flexible identity federation options

### Multi-factor Authentication

MFA is enforced for:
- All administrative accounts
- Access to sensitive data operations
- Unusual login patterns or locations
- High-risk operations (e.g., pricing rule changes)

Supported MFA methods:
- Microsoft Authenticator
- SMS verification
- FIDO2 security keys
- Time-based one-time passwords (TOTP)

### Session Management

| Session Management Policy | Value | Notes |
|---------------------------|-------|-------|
| Session timeout (regular users) | 30 minutes | Idle timeout |
| Session timeout (administrators) | 15 minutes | Idle timeout |
| Access token lifetime | 60 minutes | |
| Refresh token lifetime | 8 hours | |
| Concurrent session limit | 3 | Per user |
| Session validation | Every request | Token validation |

### Password Policies

| Policy | Requirement |
|--------|-------------|
| Minimum length | 12 characters |
| Complexity | 3 of 4 character types (uppercase, lowercase, numbers, symbols) |
| Password history | 24 passwords |
| Maximum age | 90 days |
| Account lockout | 5 failed attempts (30-minute lockout) |

## Authentication Endpoints

### Standard Authentication

#### POST /api/auth/login

Authenticates a user with email and password.

**Authentication**: None

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "rememberMe": true
}
```

**Success Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "6fd8d272-375a-4d8f-b2ba-8849831ba59a",
  "expiresAt": "2023-07-01T15:30:45Z",
  "user": {
    "id": "user123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Smith",
    "roles": ["Customer"]
  }
}
```

**Error Response** (401 Unauthorized):
```json
{
  "success": false,
  "message": "Invalid email or password",
  "errors": null
}
```

#### POST /api/auth/register

Registers a new user account.

**Authentication**: None

**Request Body**:
```json
{
  "email": "newuser@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "firstName": "Jane",
  "lastName": "Doe",
  "roles": ["Customer"]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "userId": "user456",
  "email": "newuser@example.com"
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Registration failed",
  "errors": {
    "email": ["Email is already in use"],
    "password": ["Password does not meet complexity requirements"]
  }
}
```

#### POST /api/auth/password/reset

Initiates a password reset process.

**Authentication**: None

**Request Body**:
```json
{
  "email": "user@example.com"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "email": "user@example.com"
}
```

**Note**: Always returns success to prevent email enumeration, even if the email does not exist.

#### POST /api/auth/password/change

Changes a user's password using a reset token.

**Authentication**: None

**Request Body**:
```json
{
  "email": "user@example.com",
  "resetToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "email": "user@example.com"
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Password change failed",
  "errors": {
    "resetToken": ["Invalid or expired reset token"],
    "newPassword": ["Password does not meet complexity requirements"]
  }
}
```

#### POST /api/auth/token/refresh

Refreshes an authentication token using a refresh token.

**Authentication**: None

**Request Body**:
```json
{
  "refreshToken": "6fd8d272-375a-4d8f-b2ba-8849831ba59a"
}
```

**Success Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "8ac9d372-485b-5e9f-c3cb-9950942cb70b",
  "expiresAt": "2023-07-01T16:30:45Z"
}
```

**Error Response** (401 Unauthorized):
```json
{
  "success": false,
  "message": "Invalid or expired refresh token",
  "errors": null
}
```

#### POST /api/auth/logout

Invalidates the current user's tokens.

**Authentication**: Bearer Token

**Request Body**: None

**Success Response** (200 OK):
```json
{
  "success": true
}
```

#### GET /api/auth/user

Retrieves the current authenticated user's profile.

**Authentication**: Bearer Token

**Success Response** (200 OK):
```json
{
  "id": "user123",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Smith",
  "roles": ["Customer"],
  "lastLogin": "2023-06-30T10:15:30Z"
}
```

**Error Response** (401 Unauthorized): If the token is missing or invalid.

### Azure AD Authentication

#### GET /api/auth/azure-login

Initiates the Azure AD authentication flow.

**Authentication**: None

**Query Parameters**:
- `redirect_uri` (optional): The URI to redirect to after authentication

**Response**: Redirects to Azure AD login page

#### POST /api/auth/azure-callback

Handles the callback from Azure AD after successful authentication.

**Authentication**: None

**Request Body**:
```json
{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjFLME..."
}
```

**Success Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "6fd8d272-375a-4d8f-b2ba-8849831ba59a",
  "expiresAt": "2023-07-01T15:30:45Z",
  "user": {
    "id": "user123",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Smith",
    "roles": ["Customer"]
  },
  "isNewUser": false
}
```

**Error Response** (401 Unauthorized):
```json
{
  "success": false,
  "message": "Invalid or expired Azure AD token",
  "errors": null
}
```

## Authentication Flows

### Standard Username/Password Authentication Flow

1. User navigates to the login page
2. User enters email and password
3. Application sends credentials to `/api/auth/login`
4. Server validates credentials against the database
5. If valid, server generates JWT access token and refresh token
6. Tokens are returned to the client
7. Client stores tokens securely (HTTP-only cookies or secure local storage)
8. Client includes access token in subsequent API requests
9. When access token expires, client uses refresh token to obtain a new access token

### Azure AD Authentication Flow

1. User clicks "Sign in with Microsoft" button
2. Application redirects to `/api/auth/azure-login`
3. Server redirects to Azure AD login page with appropriate parameters
4. User authenticates with Azure AD (including MFA if required)
5. Azure AD redirects back to the application with an authorization code
6. Application exchanges the code for ID and access tokens
7. Application validates the tokens and extracts user information
8. If the user doesn't exist in the system, a new user record is created
9. Application generates its own JWT access token and refresh token
10. Tokens are returned to the client
11. Client follows the same token usage pattern as standard authentication

### Token Refresh Flow

1. Client detects that the access token is expired or about to expire
2. Client sends refresh token to `/api/auth/token/refresh`
3. Server validates the refresh token
4. If valid, server generates new access token and refresh token
5. New tokens are returned to the client
6. Client updates stored tokens

### Password Reset Flow

1. User clicks "Forgot password" link
2. User enters email address
3. Application sends request to `/api/auth/password/reset`
4. Server generates a password reset token and sends it via email
5. User clicks the link in the email and is directed to password reset page
6. User enters new password
7. Application sends request to `/api/auth/password/change`
8. Server validates the reset token and updates the password
9. User is redirected to login page to authenticate with new password

## Request and Response Models

### Authentication Requests

#### LoginRequest

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| email | string | Yes | User's email address |
| password | string | Yes | User's password |
| rememberMe | boolean | No | Whether to issue a longer-lived refresh token |

#### RegisterRequest

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| email | string | Yes | User's email address |
| password | string | Yes | User's password |
| confirmPassword | string | Yes | Confirmation of password (must match password) |
| firstName | string | Yes | User's first name |
| lastName | string | Yes | User's last name |
| roles | string[] | No | Requested roles (subject to authorization) |

#### PasswordResetRequest

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| email | string | Yes | User's email address |

#### PasswordChangeRequest

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| email | string | Yes | User's email address |
| resetToken | string | Yes | Token received via email |
| newPassword | string | Yes | New password |
| confirmPassword | string | Yes | Confirmation of new password |

#### RefreshTokenRequest

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| refreshToken | string | Yes | Refresh token previously issued |

#### AzureAdAuthRequest

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| idToken | string | Yes | ID token from Azure AD |

### Authentication Responses

#### AuthSuccessResponse

| Property | Type | Description |
|----------|------|-------------|
| token | string | JWT access token |
| refreshToken | string | Refresh token for obtaining new access tokens |
| expiresAt | string | ISO 8601 timestamp when the access token expires |
| user | object | User profile information |
| user.id | string | Unique user identifier |
| user.email | string | User's email address |
| user.firstName | string | User's first name |
| user.lastName | string | User's last name |
| user.roles | string[] | User's assigned roles |

#### RegisterResponse

| Property | Type | Description |
|----------|------|-------------|
| success | boolean | Whether the registration was successful |
| userId | string | The ID of the newly registered user |
| email | string | The email address of the registered user |

#### PasswordResetResponse

| Property | Type | Description |
|----------|------|-------------|
| success | boolean | Whether the password reset request was successful |
| email | string | The email address for the reset request |

#### PasswordChangeResponse

| Property | Type | Description |
|----------|------|-------------|
| success | boolean | Whether the password change was successful |
| email | string | The email address of the user |

#### RefreshTokenResponse

| Property | Type | Description |
|----------|------|-------------|
| token | string | New JWT access token |
| refreshToken | string | New refresh token |
| expiresAt | string | ISO 8601 timestamp when the new access token expires |

#### AzureAdAuthResponse

| Property | Type | Description |
|----------|------|-------------|
| token | string | JWT access token |
| refreshToken | string | Refresh token for obtaining new access tokens |
| expiresAt | string | ISO 8601 timestamp when the access token expires |
| user | object | User profile information (same as AuthSuccessResponse) |
| isNewUser | boolean | Whether this is a newly created user account |

#### AuthFailureResponse

| Property | Type | Description |
|----------|------|-------------|
| success | boolean | Always false for failure responses |
| message | string | Human-readable error message |
| errors | object | Optional field-specific error messages |

## Token Management

### Token Generation

The VAT Filing Pricing Tool uses JSON Web Tokens (JWT) for secure authentication and authorization.

#### JWT Token Structure

**Header**:
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload**:
```json
{
  "sub": "user123",                // Subject (user ID)
  "email": "user@example.com",     // User's email
  "roles": ["Customer"],           // User's roles
  "iat": 1625144445,               // Issued at (Unix timestamp)
  "exp": 1625148045,               // Expiration (Unix timestamp)
  "iss": "vat-filing-pricing-tool", // Issuer
  "aud": "api"                     // Audience
}
```

#### Claims Included in Tokens

| Claim | Description | Purpose |
|-------|-------------|---------|
| sub | Subject identifier | Unique user ID |
| email | User's email address | User identification |
| roles | User's assigned roles | Authorization |
| iat | Issued at timestamp | Token tracking |
| exp | Expiration timestamp | Token expiry |
| iss | Issuer identifier | Token validation |
| aud | Audience identifier | Token validation |
| jti | JWT ID | Unique token identifier |

#### Token Expiration Policy

- Access tokens expire after 60 minutes
- Refresh tokens expire after 8 hours
- Password reset tokens expire after 24 hours

#### Signing Mechanism

Tokens are signed using HMAC-SHA256 (HS256) with a secure, environment-specific secret key.

### Token Validation

All incoming JWT tokens are validated against the following criteria:

1. **Signature Verification**: Ensures the token has not been tampered with
2. **Expiration Check**: Verifies the token has not expired
3. **Issuer Validation**: Confirms the token was issued by our application
4. **Audience Validation**: Ensures the token is intended for our API

#### Validation Parameters

| Parameter | Value | Purpose |
|-----------|-------|---------|
| ValidateIssuerSigningKey | true | Verifies token signature |
| ValidateIssuer | true | Verifies token issuer |
| ValidateAudience | true | Verifies token audience |
| ValidateLifetime | true | Verifies token expiration |
| ClockSkew | 5 minutes | Allows for minor clock differences |

### Token Refresh

The system implements a refresh token rotation strategy for enhanced security:

1. When a refresh token is used, it is immediately invalidated
2. A new refresh token is issued along with a new access token
3. Refresh tokens are single-use only
4. If a refresh token is reused, all tokens for the user are invalidated (possible token theft)

#### Refresh Token Security Measures

- Refresh tokens are stored as secure hashes in the database
- Each refresh token is linked to a specific user
- Refresh tokens are invalidated when a user changes their password or logs out
- Refresh tokens can be revoked by administrators

### Token Storage

#### Client-Side Storage Recommendations

**Web Applications**:
- Store access tokens in memory (JavaScript variable)
- Store refresh tokens in HttpOnly, Secure, SameSite=Strict cookies
- Never store tokens in localStorage or sessionStorage

**Mobile Applications**:
- Store tokens in secure, encrypted storage (Keychain for iOS, Keystore for Android)
- Implement certificate pinning to prevent MITM attacks

## Azure AD Integration

### Azure AD Configuration Requirements

#### Required Azure AD Application Registration

1. Register a new application in Azure AD:
   - Navigate to [Azure Portal](https://portal.azure.com) > Azure Active Directory > App registrations
   - Click "New registration"
   - Enter a name (e.g., "VAT Filing Pricing Tool")
   - Select supported account types (single tenant or multi-tenant)
   - Set the redirect URI (e.g., `https://yourapp.com/api/auth/azure-callback`)

2. Configure authentication:
   - Enable implicit grant flow for access tokens and ID tokens
   - Add web platform and configure redirect URIs

3. Add API permissions:
   - Microsoft Graph > User.Read (delegated)
   - Additional permissions as needed

#### Redirect URI Configuration

The following redirect URIs must be configured in your Azure AD application:

- Production: `https://yourapp.com/api/auth/azure-callback`
- Staging: `https://staging.yourapp.com/api/auth/azure-callback`
- Development: `https://localhost:5001/api/auth/azure-callback`

#### Application Settings

Add the following settings to your application configuration:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "CallbackPath": "/api/auth/azure-callback"
  }
}
```

### Azure AD Authentication Flow

The VAT Filing Pricing Tool implements the OAuth 2.0 authorization code flow with PKCE (Proof Key for Code Exchange) for Azure AD authentication:

1. User initiates Azure AD login
2. Application generates a code verifier and code challenge
3. Application redirects to Azure AD login page with the following parameters:
   - `client_id`: Application's client ID
   - `response_type`: "code"
   - `redirect_uri`: Callback URL
   - `scope`: "openid profile email"
   - `response_mode`: "form_post"
   - `state`: Random state value for CSRF protection
   - `code_challenge`: PKCE code challenge
   - `code_challenge_method`: "S256"

4. User authenticates with Azure AD (including MFA if required)
5. Azure AD redirects to callback URL with authorization code
6. Application exchanges code for tokens using:
   - `client_id`: Application's client ID
   - `grant_type`: "authorization_code"
   - `code`: Authorization code from Azure AD
   - `redirect_uri`: Same callback URL
   - `code_verifier`: Original PKCE code verifier

7. Azure AD returns ID token, access token, and refresh token
8. Application validates the ID token and extracts claims
9. Application creates or updates user in local database
10. Application issues its own JWT token for API access

### Azure AD Token Validation

ID tokens from Azure AD are validated according to OpenID Connect specifications:

1. **Signature Verification**: Verify the token signature using Azure AD's public keys from the OpenID Configuration endpoint
2. **Claims Validation**:
   - `iss` (Issuer): Must match Azure AD tenant
   - `aud` (Audience): Must match application client ID
   - `exp` (Expiration): Must not be expired
   - `nbf` (Not Before): Must be in the past
   - `nonce`: Must match the nonce sent in the request (if used)

## Identity Management

### User Identity Management

The VAT Filing Pricing Tool implements a centralized identity management approach using Azure AD:

#### User Provisioning and Deprovisioning

- **Just-in-time Provisioning**: New users are automatically created in the application database upon first Azure AD login
- **Attribute Mapping**: User attributes from Azure AD are mapped to application user properties
- **Role Assignment**: Default roles are assigned based on Azure AD group membership
- **Deprovisioning**: Users can be disabled in Azure AD to revoke access to the application

#### User Profile Management

- Basic profile information is synced from Azure AD (name, email, etc.)
- Additional application-specific profile data is stored in the application database
- Users can update their application-specific profile information

#### Identity Lifecycle Management

- User accounts follow the lifecycle defined in Azure AD
- Account status changes in Azure AD (disabled, deleted) are reflected in application access
- Regular access reviews are conducted to ensure proper access levels

### Multi-factor Authentication

The VAT Filing Pricing Tool supports and recommends multi-factor authentication for all users:

#### MFA Methods Supported

- **Microsoft Authenticator app** (push notifications or one-time codes)
- **SMS verification codes** (as a fallback option)
- **FIDO2 security keys** (for high-security accounts)
- **Time-based one-time passwords (TOTP)** with any compatible authenticator app

#### MFA Enforcement Policies

MFA is enforced under the following conditions:

1. **Role-based enforcement**: Always required for administrative roles
2. **Conditional enforcement**:
   - New locations or devices
   - High-risk sign-in activities
   - Accessing sensitive operations (bulk operations, rule changes)
3. **Azure AD Conditional Access**: For enterprise deployments

#### Risk-based Adaptive Authentication

For organizations using Azure AD Premium:

- **Sign-in risk policies**: Challenge or block high-risk sign-ins
- **User risk policies**: Require password change for compromised accounts
- **Location-based policies**: Additional verification for unfamiliar locations

### Session Management

The VAT Filing Pricing Tool implements robust session management to secure user sessions:

#### Session Timeout Configurations

- **Regular user sessions**: Timeout after 30 minutes of inactivity
- **Administrative sessions**: Timeout after 15 minutes of inactivity
- **Remember me option**: Extends refresh token validity to 7 days

#### Concurrent Session Limitations

- Maximum of 3 concurrent active sessions per user
- New sessions beyond this limit will invalidate the oldest session
- Administrators can view and terminate user sessions

#### Session Validation Process

- Access tokens are validated on every API request
- Refresh tokens are validated when renewing access tokens
- Azure AD sessions are managed separately from application sessions

#### Session Termination Procedures

Sessions can be terminated in the following ways:

1. **User-initiated logout**: Invalidates current session tokens
2. **Administrative termination**: Administrators can terminate user sessions
3. **Password change**: All sessions are terminated when a user changes their password
4. **Inactivity timeout**: Sessions expire automatically after inactivity
5. **Global sign-out**: Users can sign out of all sessions across devices

## Administrative Security

### Administrator Role Management

The VAT Filing Pricing Tool implements a comprehensive approach to administrator role management:

#### Administrator Role Definitions

| Role | Description | Capabilities |
|------|-------------|--------------|
| System Administrator | Complete system administration | Full system access, user management, configuration |
| Pricing Administrator | Manage pricing rules and models | Pricing configuration, limited user management |
| Security Administrator | Manage security settings | User security, audit logs, access control |
| Read-only Administrator | View-only administrative access | View configuration and users without modification |

#### Role Assignment and Revocation

- Roles are assigned based on job function and responsibility
- Role assignments require approval from existing administrators
- Role revocations take effect immediately
- Regular access reviews ensure appropriate role assignments

#### Separation of Duties

The system enforces separation of duties through:

- Requiring multiple approvers for sensitive changes
- Preventing conflicting role assignments
- Audit logging of all administrative actions
- Restricting certain action combinations

### Privileged Access Workflows

The VAT Filing Pricing Tool implements secure workflows for privileged access:

#### Just-in-Time Access

- Administrative access is provided on a time-limited basis
- Users request elevated privileges when needed
- Approvals are required for sensitive administrative functions
- Access is automatically revoked after a defined period

#### Approval Workflows

1. **User requests access**: Specifies the role, reason, and duration
2. **Approval notification**: Sent to authorized approvers
3. **Approval decision**: Grant or deny with comments
4. **Access provisioning**: Temporary role assignment if approved
5. **Access expiration**: Automatic revocation at the end of the approved period

#### Emergency Access Procedures

For urgent situations where normal approval processes would cause unacceptable delays:

1. **Emergency access request**: With justification and manager notification
2. **Expedited approval**: By designated emergency approvers
3. **Enhanced monitoring**: Increased logging of all actions
4. **Post-incident review**: Mandatory review of all emergency access

### Administrative Security Controls

#### Enhanced MFA Requirements

- Administrative actions always require recent MFA
- Higher-privileged roles may require stronger MFA methods (FIDO2 keys)
- Step-up authentication for sensitive operations

#### Administrative Action Logging

All administrative actions are logged with detailed information:

- Administrator identity
- Action performed
- Affected resources
- Timestamp
- Source IP and device information
- Before/after state for configuration changes

#### Administrative Access Review

- Quarterly review of all administrative role assignments
- Validation of continuing business need for access
- Verification of appropriate privilege levels
- Documentation of access review decisions

#### Secure Administrator Workstation Requirements

- Dedicated, hardened workstations for administrative access
- Enhanced security controls (disk encryption, endpoint protection)
- Restricted network access
- Regular security updates and vulnerability scanning

## Protected Endpoint Authentication

### Authentication for API Endpoints

The VAT Filing Pricing Tool API uses JWT Bearer token authentication for protected endpoints.

#### Including Authentication in API Requests

**HTTP Header**:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Swagger UI**:
The Swagger UI includes an Authorize button to input your JWT token for testing endpoints.

### Role-Based Access to Endpoints

API endpoints are protected based on user roles:

| Endpoint Category | Required Roles | Notes |
|-------------------|----------------|-------|
| Public Endpoints | None | Authentication endpoints, public information |
| User Endpoints | Authenticated User | User's own data, calculations, reports |
| Pricing Endpoints | Customer, Accountant, Administrator | Pricing calculations and estimates |
| Reporting Endpoints | Customer, Accountant, Administrator | Report generation and export |
| Rule Configuration | Pricing Administrator, System Administrator | Manage pricing rules and models |
| User Administration | System Administrator | User management and role assignment |
| System Configuration | System Administrator | System settings and configuration |

### Authentication Requirements by Endpoint Group

#### Pricing Calculation Endpoints

- **Authentication**: Bearer Token required
- **Authorization**: Any authenticated user can access
- **Rate Limiting**: 100 requests per minute per user

#### Reporting Endpoints

- **Authentication**: Bearer Token required
- **Authorization**: Users can only access their own reports
- **Special Requirements**: Report export may require re-authentication for sensitive reports

#### Administrative Endpoints

- **Authentication**: Bearer Token required with administrator role
- **Authorization**: Role-based access to specific administrative functions
- **Additional Security**: Recent MFA verification required (within 15 minutes)
- **IP Restrictions**: May be limited to specific networks
- **Rate Limiting**: Strict limits to prevent abuse

#### Country Configuration Endpoints

- **Authentication**: Bearer Token required with Pricing Administrator or System Administrator role
- **Authorization**: Role-based permissions for viewing vs. modifying
- **Audit Requirements**: All changes are logged with before/after state

## Security Considerations

### Token Security Measures

- **Short Token Lifetimes**: Access tokens expire after 60 minutes
- **Secure Storage**: Tokens must be stored securely (HttpOnly cookies, secure storage)
- **Token Revocation**: Available for emergencies or suspected compromise
- **Signature Verification**: All tokens are cryptographically verified

### Password Security Requirements

- **Complexity Requirements**: 12+ characters with 3 of 4 character types
- **Password History**: Prevents reuse of previous 24 passwords
- **Maximum Age**: Passwords expire after 90 days
- **Breach Detection**: Passwords are checked against known breached password databases

### MFA Implementation

- **Risk-Based Approach**: MFA triggered based on risk factors
- **Multiple Methods**: Support for app, SMS, security key
- **Enrollment Requirement**: MFA enrollment required for administrative users
- **Bypass Monitoring**: MFA bypasses are logged and monitored

### Secure Communication Requirements

- **TLS 1.2+ Required**: All API communication requires TLS 1.2 or higher
- **Strong Cipher Suites**: Only secure cipher suites are allowed
- **HSTS Implemented**: HTTP Strict Transport Security
- **Certificate Pinning**: Recommended for mobile applications

### Administrative Security Considerations

- **Privileged Access Management**: Just-in-time, time-limited administrative access
- **Enhanced Monitoring**: All administrative actions are logged and monitored
- **Separation of Duties**: Multiple approvers for sensitive changes
- **Least Privilege**: Administrators are granted minimum necessary permissions