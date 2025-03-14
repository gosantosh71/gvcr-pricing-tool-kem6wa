# VAT Filing Pricing Tool - Backend

Backend services for the VAT Filing Pricing Tool, a comprehensive web-based application designed to provide businesses with accurate cost estimates for VAT filing services across multiple jurisdictions.

## Architecture Overview

The backend is built using a clean architecture approach with the following layers:

- **API Layer**: ASP.NET Core 6.0 Web API with controllers handling HTTP requests
- **Service Layer**: Business logic implementation
- **Domain Layer**: Core business entities, rules, and interfaces
- **Data Layer**: Data access using Entity Framework Core with Azure SQL Database
- **Infrastructure Layer**: Cross-cutting concerns like authentication, caching, and external integrations

The system follows SOLID principles and implements a microservices-oriented architecture deployed on Azure Kubernetes Service.

## Project Structure

```
├── VatFilingPricingTool.Api            # API controllers and configuration
├── VatFilingPricingTool.Service        # Business logic services
├── VatFilingPricingTool.Domain         # Domain entities and business rules
├── VatFilingPricingTool.Data           # Data access and repositories
├── VatFilingPricingTool.Infrastructure # Cross-cutting concerns
├── VatFilingPricingTool.Common         # Shared utilities and helpers
├── VatFilingPricingTool.Contracts      # DTOs and API contracts
└── Tests                               # Unit and integration tests
    ├── VatFilingPricingTool.UnitTests
    └── VatFilingPricingTool.IntegrationTests
```

## Key Features

- **Authentication**: Azure AD integration with JWT token authentication
- **Dynamic Pricing Engine**: Calculates VAT filing costs based on multiple parameters
- **Rules Engine**: Applies country-specific VAT rules and regulations
- **Reporting Service**: Generates detailed reports in multiple formats
- **Integration Services**: Connects with ERP systems and OCR document processing
- **Caching**: Redis-based caching for improved performance
- **Resilience**: Circuit breaker and retry patterns for external dependencies

## Prerequisites

- .NET 6.0 SDK
- Visual Studio 2022 or VS Code
- Docker Desktop
- Azure CLI
- SQL Server (local or Azure SQL Database)
- Redis Cache (local or Azure Redis Cache)

## Getting Started

### Local Development Setup

1. Clone the repository
   ```bash
   git clone <repository-url>
   cd src/backend
   ```

2. Restore dependencies
   ```bash
   dotnet restore
   ```

3. Update the connection strings in `appsettings.Development.json`

4. Apply database migrations
   ```bash
   dotnet ef database update --project VatFilingPricingTool.Data --startup-project VatFilingPricingTool.Api
   ```

5. Run the application
   ```bash
   dotnet run --project VatFilingPricingTool.Api
   ```

6. Access the API at `https://localhost:5001` and Swagger documentation at `https://localhost:5001/swagger`

### Docker Setup

1. Build the Docker image
   ```bash
   docker build -t vatfilingpricingtool-api .
   ```

2. Run the container
   ```bash
   docker run -p 8080:80 -e "ASPNETCORE_ENVIRONMENT=Development" vatfilingpricingtool-api
   ```

3. Access the API at `http://localhost:8080`

## Configuration

The application uses the following configuration sources in order of precedence:

1. Azure Key Vault (production)
2. Environment variables
3. appsettings.{Environment}.json
4. appsettings.json

Key configuration sections:

- **ConnectionStrings**: Database and Redis connections
- **Authentication**: Azure AD and JWT settings
- **Logging**: Log levels and providers
- **Integration**: ERP and OCR service settings
- **Caching**: Redis cache configuration

## API Documentation

The API is documented using Swagger/OpenAPI. When running the application, navigate to `/swagger` to view the interactive documentation.

Main API endpoints:

- `/api/auth`: Authentication endpoints
- `/api/pricing`: Pricing calculation endpoints
- `/api/countries`: Country data and VAT rules
- `/api/reports`: Report generation and management
- `/api/rules`: VAT rule configuration
- `/api/integration`: External system integration
- `/api/admin`: Administrative functions

## Testing

### Running Tests

```bash
# Run unit tests
dotnet test Tests/VatFilingPricingTool.UnitTests

# Run integration tests
dotnet test Tests/VatFilingPricingTool.IntegrationTests

# Run all tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Structure

- **Unit Tests**: Test individual components in isolation with mocked dependencies
- **Integration Tests**: Test interactions between components and external systems
- **API Tests**: Test API endpoints using an in-memory test server

## Deployment

The application is deployed to Azure Kubernetes Service (AKS) using Azure DevOps Pipelines.

### CI/CD Pipeline

1. Build: Compile, test, and create Docker image
2. Push: Push image to Azure Container Registry
3. Deploy: Deploy to AKS using Helm charts

### Environment Configuration

- **Development**: Local development environment
- **Staging**: Pre-production environment for testing
- **Production**: Production environment with geo-redundancy

## Monitoring and Logging

- **Application Insights**: Performance monitoring and telemetry
- **Azure Monitor**: Resource monitoring and alerting
- **Log Analytics**: Centralized logging and analysis
- **Health Checks**: Endpoint at `/health` for monitoring service health

## Contributing

1. Create a feature branch from `develop`
2. Implement changes following the coding standards
3. Write unit and integration tests
4. Submit a pull request to `develop`
5. Ensure all checks pass before merging

### Coding Standards

- Follow Microsoft's C# coding conventions
- Use async/await for asynchronous operations
- Implement proper exception handling
- Document public APIs with XML comments
- Follow SOLID principles

## License

Copyright © 2023 VAT Filing Pricing Tool