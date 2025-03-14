## Introduction

This guide provides comprehensive instructions for setting up your development environment and getting started with the VAT Filing Pricing Tool project. It covers prerequisites, repository setup, configuration, and running the application locally.

## Project Overview

The VAT Filing Pricing Tool is a comprehensive web-based application designed to provide businesses with accurate cost estimates for VAT filing services across multiple jurisdictions. The system consists of:

- **Backend**: ASP.NET Core 6.0 API with a clean architecture approach
- **Frontend**: Blazor WebAssembly single-page application
- **Infrastructure**: Azure-based cloud infrastructure with Kubernetes orchestration

The application follows a microservices architecture and is deployed on Microsoft Azure.

## Prerequisites

Before you begin, ensure you have the following tools and software installed on your development machine:

### Required Software

- **Git**: Version control system ([download](https://git-scm.com/downloads))
- **.NET 6.0 SDK**: Required for backend and frontend development ([download](https://dotnet.microsoft.com/download/dotnet/6.0)) // .NET 6.0
- **Node.js 16.x or later**: Required for frontend development ([download](https://nodejs.org/)) // Node.js 16.x
- **Docker Desktop**: Required for containerized development ([download](https://www.docker.com/products/docker-desktop)) // Docker latest
- **Docker Compose**: Included with Docker Desktop
- **Visual Studio 2022** or **Visual Studio Code**: Recommended IDEs // Visual Studio 2022
  - For Visual Studio, ensure the following workloads are installed:
    - ASP.NET and web development
    - Azure development
  - For VS Code, recommended extensions:
    - C# Dev Kit
    - Blazor WASM Debugging
    - Docker
    - Azure Tools

### Optional Tools

- **Azure CLI**: For interacting with Azure resources ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)) // Azure CLI latest
- **SQL Server Management Studio** or **Azure Data Studio**: For database management
- **Postman** or **Insomnia**: For API testing
- **Azure Storage Explorer**: For managing blob storage

### Azure Resources (for full functionality)

- Azure subscription
- Azure AD tenant for authentication
- Azure SQL Database (or local SQL Server for development)
- Azure Redis Cache (or local Redis for development)

## Repository Setup

### Cloning the Repository

1. Clone the repository to your local machine:

   ```bash
   git clone https://github.com/your-organization/vat-filing-pricing-tool.git
   cd vat-filing-pricing-tool
   ```

2. Familiarize yourself with the repository structure:

   ```
   vat-filing-pricing-tool/
   ├── .github/                # GitHub workflows and templates
   ├── docs/                   # Project documentation
   ├── infrastructure/         # Infrastructure as Code definitions
   ├── src/                    # Source code
   │   ├── backend/            # Backend services (.NET Core)
   │   └── web/                # Frontend application (Blazor WebAssembly)
   ├── .editorconfig          # Editor configuration
   ├── .gitignore             # Git ignore file
   ├── docker-compose.yml     # Docker Compose configuration
   └── README.md              # Project overview
   ```

### Branching Strategy

The project follows a modified Git Flow branching model. For detailed information, see the [Branching Strategy](branching-strategy.md) document. Key points:

- `main`: Production code
- `develop`: Integration branch for development
- Feature branches: `feature/[issue-number]-descriptive-name`
- Bugfix branches: `bugfix/[issue-number]-descriptive-name`

To start working on a new feature:

```bash
git checkout develop
git pull
git checkout -b feature/123-add-country-selector
```

## Backend Setup

### Setting Up the Backend

1. Navigate to the backend directory:

   ```bash
   cd src/backend
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Configure local development settings:

   - Create a `src/backend/VatFilingPricingTool.Api/appsettings.Development.json` file if it doesn't exist
   - Update the connection strings for SQL Server and Redis Cache
   - Configure Azure AD settings for authentication (or use the development authentication mode)

   Example configuration:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\\\mssqllocaldb;Database=VatFilingPricingTool;Trusted_Connection=True;MultipleActiveResultSets=true",
       "Redis": "localhost:6379"
     },
     "Authentication": {
       "UseAzureAd": false,
       "AzureAd": {
         "Instance": "https://login.microsoftonline.com/",
         "TenantId": "your-tenant-id",
         "ClientId": "your-client-id"
       }
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning",
         "Microsoft.Hosting.Lifetime": "Information"
       }
     }
   }
   ```

4. Apply database migrations:

   ```bash
   dotnet ef database update --project VatFilingPricingTool.Data --startup-project VatFilingPricingTool.Api
   ```

5. Run the backend API:

   ```bash
   dotnet run --project VatFilingPricingTool.Api
   ```

6. The API will be available at `https://localhost:5001` with Swagger documentation at `https://localhost:5001/swagger`

### Backend Project Structure

```
src/backend/
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

For more detailed information about the backend, refer to the [Backend README](../backend/README.md).

## Frontend Setup

### Setting Up the Frontend

1. Navigate to the web directory:

   ```bash
   cd src/web
   ```

2. Restore .NET dependencies:

   ```bash
   dotnet restore
   ```

3. Install Node.js dependencies:

   ```bash
   npm ci
   ```

4. Configure local development settings:

   - Update `src/web/VatFilingPricingTool.Web/wwwroot/appsettings.Development.json` with appropriate settings
   - Configure API endpoints to point to your local backend
   - Configure Azure AD settings for authentication (if using)

   Example configuration:

   ```json
   {
     "ApiEndpoints": {
       "BaseUrl": "https://localhost:5001",
       "Pricing": "api/pricing",
       "Countries": "api/countries",
       "Rules": "api/rules",
       "Reports": "api/reports",
       "Users": "api/users"
     },
     "AzureAd": {
       "Authority": "https://login.microsoftonline.com/your-tenant-id",
       "ClientId": "your-client-id",
       "ValidateAuthority": true
     }
   }
   ```

5. Run the frontend application:

   ```bash
   dotnet run --project VatFilingPricingTool.Web/VatFilingPricingTool.Web.csproj
   ```

6. The application will be available at `https://localhost:5003`

### Frontend Project Structure

```
src/web/VatFilingPricingTool.Web/
├── Authentication/       # Authentication services and handlers
├── Components/           # Reusable UI components
├── Pages/                # Application pages and views
├── Services/             # Service interfaces and implementations
├── Models/               # Data models and DTOs
├── Helpers/              # Utility classes and helper functions
├── wwwroot/              # Static assets and client-side resources
└── Tests/                # Unit and end-to-end tests
```

For more detailed information about the frontend, refer to the [Web README](../web/README.md).

## Docker Development Environment

### Using Docker Compose

The project includes Docker Compose configuration for running the entire application stack locally:

1. From the root directory, start the Docker Compose environment:

   ```bash
   docker-compose up -d
   ```

2. This will start the following services:
   - SQL Server database
   - Redis Cache
   - Backend API
   - Frontend Web Application

3. Access the applications:
   - Frontend: http://localhost:8080
   - Backend API: http://localhost:8081
   - Swagger Documentation: http://localhost:8081/swagger

4. To stop the environment:

   ```bash
   docker-compose down
   ```

### Building Docker Images

To build the Docker images individually:

1. Build the backend image:

   ```bash
   cd src/backend
   docker build -t vatfilingpricingtool-api .
   ```

2. Build the frontend image:

   ```bash
   cd src/web
   docker build -t vatfilingpricingtool-web .
   ```

3. Run the containers:

   ```bash
   docker run -p 8081:80 -e "ASPNETCORE_ENVIRONMENT=Development" vatfilingpricingtool-api
   docker run -p 8080:80 -e "ASPNETCORE_ENVIRONMENT=Development" vatfilingpricingtool-web
   ```

## Database Setup

### Local Database Setup

#### Option 1: SQL Server LocalDB (Windows only)

1. Ensure SQL Server LocalDB is installed (included with Visual Studio)
2. Update the connection string in `appsettings.Development.json`:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\\\mssqllocaldb;Database=VatFilingPricingTool;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. Apply migrations:

   ```bash
   cd src/backend
   dotnet ef database update --project VatFilingPricingTool.Data --startup-project VatFilingPricingTool.Api
   ```

#### Option 2: SQL Server in Docker

1. Start SQL Server in Docker:

   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest
   ```

2. Update the connection string in `appsettings.Development.json`:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,1433;Database=VatFilingPricingTool;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
   }
   ```

3. Apply migrations as described above

#### Option 3: Azure SQL Database

1. Create an Azure SQL Database in the Azure portal
2. Update the connection string in `appsettings.Development.json` with your Azure SQL connection string
3. Apply migrations as described above

### Redis Cache Setup

#### Option 1: Local Redis (Windows)

1. Install Redis for Windows using [Redis-x64-3.0.504.msi](https://github.com/microsoftarchive/redis/releases)
2. Update the Redis connection string in `appsettings.Development.json`:

   ```json
   "ConnectionStrings": {
     "Redis": "localhost:6379"
   }
   ```

#### Option 2: Redis in Docker

1. Start Redis in Docker:

   ```bash
   docker run -p 6379:6379 -d redis
   ```

2. Update the Redis connection string as described above

#### Option 3: Azure Redis Cache

1. Create an Azure Redis Cache in the Azure portal
2. Update the Redis connection string in `appsettings.Development.json` with your Azure Redis connection string

## Authentication Setup

### Development Authentication

For local development, you can use the simplified development authentication mode:

1. In the backend `appsettings.Development.json`, set:

   ```json
   "Authentication": {
     "UseAzureAd": false
   }
   ```

2. In the frontend `wwwroot/appsettings.Development.json`, set:

   ```json
   "Authentication": {
     "UseDevelopmentAuth": true
   }
   ```

3. This will enable a simplified authentication flow with predefined test users

### Azure AD Authentication

To use Azure AD authentication (recommended for integration testing):

1. Register an application in the Azure portal:
   - Navigate to Azure Active Directory > App registrations > New registration
   - Name: VAT Filing Pricing Tool
   - Supported account types: Single tenant
   - Redirect URI: Web > https://localhost:5003/authentication/login-callback

2. Configure API permissions:
   - Add Microsoft Graph > User.Read permission

3. Create a client secret (optional, for backend authentication)

4. Update backend `appsettings.Development.json`:

   ```json
   "Authentication": {
     "UseAzureAd": true,
     "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "TenantId": "your-tenant-id",
       "ClientId": "your-client-id",
       "ClientSecret": "your-client-secret"
     }
   }
   ```

5. Update frontend `wwwroot/appsettings.Development.json`:

   ```json
   "Authentication": {
     "UseDevelopmentAuth": false
   },
   "AzureAd": {
     "Authority": "https://login.microsoftonline.com/your-tenant-id",
     "ClientId": "your-client-id",
     "ValidateAuthority": true
   }
   ```

## Running the Application

### Running Backend and Frontend Separately

1. Start the backend API:

   ```bash
   cd src/backend
   dotnet run --project VatFilingPricingTool.Api
   ```

2. In a separate terminal, start the frontend application:

   ```bash
   cd src/web
   dotnet run --project VatFilingPricingTool.Web/VatFilingPricingTool.Web.csproj
   ```

3. Access the applications:
   - Frontend: https://localhost:5003
   - Backend API: https://localhost:5001
   - Swagger Documentation: https://localhost:5001/swagger

### Running with Visual Studio

1. Open the solution file `src/backend/VatFilingPricingTool.sln` in Visual Studio
2. Configure the startup projects:
   - Right-click on the solution in Solution Explorer
   - Select "Set Startup Projects..."
   - Choose "Multiple startup projects"
   - Set "VatFilingPricingTool.Api" and "VatFilingPricingTool.Web" to "Start"
3. Press F5 to start debugging

### Running with Docker Compose

As described in the Docker Development Environment section:

```bash
docker-compose up -d
```

Access the applications:
- Frontend: http://localhost:8080
- Backend API: http://localhost:8081
- Swagger Documentation: http://localhost:8081/swagger

## Testing

### Running Backend Tests

1. Run unit tests:

   ```bash
   cd src/backend
   dotnet test Tests/VatFilingPricingTool.UnitTests
   ```

2. Run integration tests:

   ```bash
   cd src/backend
   dotnet test Tests/VatFilingPricingTool.IntegrationTests
   ```

3. Run all tests with coverage:

   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
   ```

### Running Frontend Tests

1. Run unit and component tests:

   ```bash
   cd src/web
   dotnet test Tests/VatFilingPricingTool.Web.Tests
   ```

2. Run end-to-end tests (requires the application to be running):

   ```bash
   dotnet test Tests/VatFilingPricingTool.Web.E2E.Tests
   ```

### Test Structure

- **Unit Tests**: Test individual components in isolation with mocked dependencies
- **Integration Tests**: Test interactions between components and external systems
- **End-to-End Tests**: Test complete user journeys through the application

Testing should follow the project's established standards, which include patterns for arranging tests, naming conventions, and code coverage requirements.

## Working with Azure Resources

### Local Development with Azure Services

For development with Azure services:

1. Install the Azure CLI:
   - [Azure CLI Installation Guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

2. Log in to Azure:

   ```bash
   az login
   ```

3. Set the active subscription:

   ```bash
   az account set --subscription "Your Subscription Name"
   ```

4. Create Azure resources for development:

   ```bash
   cd infrastructure/scripts
   ./deploy.ps1 -Environment Development
   ```

   Or manually create the required resources in the Azure portal

5. Update connection strings and configuration settings in your local development files

### Infrastructure as Code

The project uses Infrastructure as Code (IaC) for managing Azure resources:

- Azure Resource Manager (ARM) templates in `infrastructure/azure/arm-templates/`
- Terraform configurations in `infrastructure/azure/terraform/`
- Kubernetes manifests in `infrastructure/kubernetes/`
- Helm charts in `infrastructure/helm/`

For more information on infrastructure setup, refer to the [Infrastructure README](../../infrastructure/README.md).

## Development Workflow

### Standard Development Workflow

1. **Pick up a task**: Select an issue from the project board or create a new one

2. **Create a branch**: Create a feature or bugfix branch from the develop branch
   ```bash
   git checkout develop
   git pull
   git checkout -b feature/123-add-country-selector
   ```

3. **Implement changes**: Make your changes following the project's coding standards and best practices

4. **Write tests**: Add unit tests, integration tests, and/or end-to-end tests as appropriate

5. **Run tests locally**: Ensure all tests pass before committing
   ```bash
   dotnet test
   ```

6. **Commit changes**: Use descriptive commit messages
   ```bash
   git add .
   git commit -m "feat(calculator): add country selector component"
   ```

7. **Push branch**: Push your branch to the remote repository
   ```bash
   git push -u origin feature/123-add-country-selector
   ```

8. **Create a pull request**: Create a PR to merge your branch into develop

9. **Address review feedback**: Make any requested changes

10. **Merge**: Once approved and all checks pass, merge your PR

For more detailed information on the development workflow, refer to the [Branching Strategy](branching-strategy.md) document.

## Troubleshooting

### Common Issues and Solutions

#### Database Connection Issues

- **Error**: Cannot connect to the database
- **Solution**: 
  - Verify the connection string in `appsettings.Development.json`
  - Ensure SQL Server is running
  - Check firewall settings if using Azure SQL

#### Redis Connection Issues

- **Error**: Cannot connect to Redis
- **Solution**:
  - Verify Redis is running
  - Check the Redis connection string
  - Ensure Redis port (6379) is not blocked by firewall

#### Authentication Issues

- **Error**: Authentication failed or unauthorized access
- **Solution**:
  - Verify Azure AD configuration
  - Check client ID and tenant ID
  - Ensure redirect URIs are correctly configured
  - Try using development authentication mode for local testing

#### Docker Issues

- **Error**: Cannot start Docker containers
- **Solution**:
  - Ensure Docker Desktop is running
  - Check if ports are already in use
  - Verify Docker resource allocation (memory, CPU)
  - Try rebuilding the images: `docker-compose build --no-cache`

#### Build Errors

- **Error**: Build fails with package reference errors
- **Solution**:
  - Run `dotnet restore` to restore packages
  - Check NuGet package sources
  - Clear NuGet cache: `dotnet nuget locals all --clear`

### Getting Help

If you encounter issues not covered here:

1. Check the project documentation in the `docs` directory
2. Review existing issues in the issue tracker
3. Consult with team members
4. Create a new issue with detailed information about the problem

## Additional Resources

### Project Documentation

- [Project README](../../README.md): Overview of the project
- Project standards: Find information about coding conventions and best practices in the docs directory
- [Branching Strategy](branching-strategy.md): Git workflow and version control practices
- [Architecture Documentation](../architecture/README.md): System architecture and design
- [API Documentation](../api/README.md): API reference and integration guides
- [Deployment Documentation](../deployment/README.md): Deployment and operations

### External Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core)
- [Blazor WebAssembly Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor)
- [Azure Documentation](https://docs.microsoft.com/en-us/azure)
- [Docker Documentation](https://docs.docker.com/)
- [Kubernetes Documentation](https://kubernetes.io/docs/home/)

## Conclusion

This guide has provided the essential information to set up your development environment and start contributing to the VAT Filing Pricing Tool project. By following these instructions, you should have a working local development environment with all the necessary components.

Remember to follow the established project standards and branching strategy when making changes. If you have any questions or encounter issues not covered in this guide, please reach out to the development team.

Happy coding!