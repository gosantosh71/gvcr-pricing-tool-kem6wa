# VAT Filing Pricing Tool - Web Frontend

This repository contains the web frontend for the VAT Filing Pricing Tool, a comprehensive web-based application designed to provide businesses with accurate cost estimates for VAT filing services across multiple jurisdictions. The frontend is built using Blazor WebAssembly, providing a responsive and interactive user experience.

## Architecture Overview

The web frontend is built as a Single Page Application (SPA) using Blazor WebAssembly with the following key components:

- **Authentication**: Integration with Azure Active Directory for secure user authentication
- **UI Components**: Responsive components following Microsoft's Fluent Design principles
- **API Integration**: HTTP clients for communication with backend services
- **State Management**: Client-side state management for user sessions and application data
- **Visualization**: Interactive charts and data visualizations for pricing breakdowns

## Project Structure

The web frontend is organized into the following structure:

- **VatFilingPricingTool.Web/**: Main Blazor WebAssembly application
  - **Authentication/**: Authentication services and handlers
  - **Components/**: Reusable UI components
  - **Pages/**: Application pages and views
  - **Services/**: Service interfaces and implementations
  - **Models/**: Data models and DTOs
  - **Helpers/**: Utility classes and helper functions
  - **wwwroot/**: Static assets and client-side resources
- **Tests/**: Test projects
  - **VatFilingPricingTool.Web.Tests/**: Unit and component tests
  - **VatFilingPricingTool.Web.E2E.Tests/**: End-to-end tests using Playwright

## Prerequisites

To develop and run the web frontend, you need the following tools installed:

- **.NET 6 SDK**: Required for building and running the Blazor WebAssembly application
- **Node.js 16+**: Required for frontend asset building and development tools
- **Docker**: Required for containerized development and deployment
- **Docker Compose**: Required for orchestrating multi-container development environment
- **Visual Studio 2022** or **Visual Studio Code**: Recommended IDEs for development

## Getting Started

### Local Development

1. Clone the repository
2. Navigate to the `src/web` directory
3. Restore dependencies:
   ```bash
   dotnet restore
   npm ci
   ```
4. Run the application:
   ```bash
   dotnet run --project VatFilingPricingTool.Web/VatFilingPricingTool.Web.csproj
   ```
5. Open your browser and navigate to `https://localhost:5001`

### Using Docker Compose

1. Navigate to the `src/web` directory
2. Build and start the containers:
   ```bash
   docker-compose up -d --build
   ```
3. Open your browser and navigate to `http://localhost:8080`

### Environment Configuration

The application uses different environment configurations:

- **Development**: Used during local development with debugging enabled
- **Staging**: Used for testing in a production-like environment
- **Production**: Used for the live production environment

Environment-specific settings are stored in `wwwroot/appsettings.{Environment}.json` files.

## Authentication Setup

The application uses Azure Active Directory for authentication. To configure authentication:

1. Create an Azure AD application registration in the Azure portal
2. Configure the redirect URIs for your environment
3. Update the authentication settings in `wwwroot/appsettings.json`:
   ```json
   {
     "AzureAd": {
       "Authority": "https://login.microsoftonline.com/{TENANT_ID}",
       "ClientId": "{CLIENT_ID}",
       "ValidateAuthority": true
     }
   }
   ```

## API Configuration

The web frontend communicates with backend APIs. Configure the API endpoints in `wwwroot/appsettings.json`:

```json
{
  "ApiEndpoints": {
    "BaseUrl": "https://api.example.com",
    "Pricing": "api/pricing",
    "Countries": "api/countries",
    "Rules": "api/rules",
    "Reports": "api/reports",
    "Users": "api/users"
  }
}
```

In development mode with Docker Compose, the API endpoints are automatically configured to connect to the containerized backend services.

## Building for Production

To build the application for production deployment:

1. Navigate to the `src/web` directory
2. Build the Docker image:
   ```bash
   docker build -t vatfilingpricingtool/web:latest --build-arg ASPNETCORE_ENVIRONMENT=Production .
   ```
3. Push the image to your container registry:
   ```bash
   docker push vatfilingpricingtool/web:latest
   ```

The production build uses multi-stage Docker builds to create an optimized Nginx-based container that serves the static Blazor WebAssembly files.

## Testing

### Unit and Component Tests

Run unit and component tests with:

```bash
dotnet test Tests/VatFilingPricingTool.Web.Tests/VatFilingPricingTool.Web.Tests.csproj
```

### End-to-End Tests

Run end-to-end tests with Playwright:

```bash
dotnet test Tests/VatFilingPricingTool.Web.E2E.Tests/VatFilingPricingTool.Web.E2E.Tests.csproj
```

Note: End-to-end tests require the application to be running, either locally or in containers.

## CI/CD Integration

The web frontend includes GitHub Actions workflows for continuous integration and deployment:

- **Build Workflow**: `.github/workflows/build.yml` - Builds and tests the application on every pull request and push to main branches
- **Deploy Workflow**: `.github/workflows/deploy.yml` - Deploys the application to Azure environments when changes are merged to deployment branches

## Key Features

The web frontend implements the following key features:

- **Dashboard**: Overview of recent calculations and activities
- **Pricing Calculator**: Interactive form for calculating VAT filing costs
- **Country Selection**: Multi-select interface for choosing VAT jurisdictions
- **Service Configuration**: Options for different service types and parameters
- **Results Display**: Detailed breakdown of pricing calculations
- **Report Generation**: Creation and export of detailed reports
- **Admin Interface**: Configuration tools for administrators

## Contributing

### Development Workflow

1. Create a feature branch from `develop`
2. Implement your changes with appropriate tests
3. Submit a pull request to `develop`
4. Ensure all CI checks pass
5. Request code review from team members

### Coding Standards

This project follows the standard C# coding conventions and best practices for Blazor WebAssembly development. Key guidelines include:

- Use C# 10 features where appropriate
- Follow the MVVM pattern for component design
- Use dependency injection for service access
- Write unit tests for all business logic
- Ensure accessibility compliance (WCAG 2.1 AA)

## Troubleshooting

### Common Issues

- **Authentication Errors**: Verify Azure AD configuration in appsettings.json
- **API Connection Issues**: Check API endpoint configuration and network connectivity
- **Build Failures**: Ensure .NET 6 SDK and Node.js are properly installed
- **Container Issues**: Verify Docker and Docker Compose installations

### Logging

The application uses structured logging with different levels:

- **Development**: Detailed logs in the browser console
- **Production**: Critical errors logged to Application Insights

Enable debug logging in the browser by setting `localStorage.setItem('VatFilingPricingTool:LogLevel', 'Debug')`

## License

Copyright © 2023 VAT Filing Pricing Tool. All rights reserved.