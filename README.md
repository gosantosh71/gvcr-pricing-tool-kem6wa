# VAT Filing Pricing Tool

A comprehensive web-based application designed to provide businesses with accurate cost estimates for VAT filing services across multiple jurisdictions. This system addresses the complex challenge of determining VAT filing costs based on various factors including business size, transaction volume, filing frequency, and country-specific tax regulations.

## Key Features

- **Multi-country VAT Calculation**: Calculate VAT filing costs across multiple jurisdictions simultaneously
- **Dynamic Pricing Engine**: Accurate cost estimates based on transaction volume, filing frequency, and service complexity
- **Country-specific Rules**: Support for country-specific VAT regulations and requirements
- **Integration Capabilities**: Connect with Microsoft Dynamics 365 and other ERP systems
- **Document Processing**: OCR capabilities for automated data extraction from invoices and VAT forms
- **Comprehensive Reporting**: Generate detailed reports in multiple formats (PDF, Excel)
- **User Management**: Role-based access control with Azure AD integration
- **Admin Configuration**: Interface for administrators to configure pricing models, VAT rates, and system settings

## Technology Stack

- **Backend**: ASP.NET Core 6.0+, C# 10.0
- **Frontend**: Blazor WebAssembly / React with TypeScript
- **Database**: Azure SQL Database, Azure Cosmos DB
- **Authentication**: Azure Active Directory
- **Cloud Infrastructure**: Microsoft Azure (App Service, AKS, API Management, etc.)
- **DevOps**: Azure DevOps / GitHub Actions
- **Monitoring**: Azure Application Insights, Azure Monitor

## Project Structure

```
├── src/
│   ├── backend/             # Backend services and API
│   │   ├── VatFilingPricingTool.Api/            # API endpoints
│   │   ├── VatFilingPricingTool.Common/         # Shared utilities and helpers
│   │   ├── VatFilingPricingTool.Contracts/      # API contracts and models
│   │   ├── VatFilingPricingTool.Data/           # Data access layer
│   │   ├── VatFilingPricingTool.Domain/         # Domain entities and business logic
│   │   ├── VatFilingPricingTool.Infrastructure/ # External services integration
│   │   ├── VatFilingPricingTool.Service/        # Business services
│   │   └── Tests/                               # Unit and integration tests
│   └── web/                # Frontend Blazor WebAssembly application
│       ├── VatFilingPricingTool.Web/           # Main web application
│       └── Tests/                              # Frontend tests
├── infrastructure/         # Infrastructure as Code (ARM templates, Terraform)
├── docs/                   # Documentation
└── .github/                # GitHub workflows and templates
```

## Getting Started

### Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [Node.js](https://nodejs.org/) (LTS version)
- [Docker](https://www.docker.com/products/docker-desktop) (for containerized development)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for deployment)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

### Development Setup

1. Clone the repository
   ```bash
   git clone https://github.com/yourusername/vatfilingpricingtool.git
   cd vatfilingpricingtool
   ```

2. Backend setup
   ```bash
   cd src/backend
   dotnet restore
   dotnet build
   ```

3. Frontend setup
   ```bash
   cd src/web
   dotnet restore
   ```

4. Run the application using Docker Compose
   ```bash
   docker-compose up -d
   ```

5. Access the application
   - API: http://localhost:5000/swagger
   - Web: http://localhost:5001

## Deployment

### Azure Deployment

The application is designed to be deployed to Microsoft Azure using the provided infrastructure as code templates.

1. Deploy infrastructure using ARM templates
   ```bash
   cd infrastructure/azure/arm-templates
   az deployment group create --resource-group your-resource-group --template-file main.json --parameters parameters.json
   ```

2. Alternatively, use Terraform
   ```bash
   cd infrastructure/azure/terraform
   terraform init
   terraform plan -out=tfplan
   terraform apply tfplan
   ```

3. Deploy the application using Azure DevOps pipelines or GitHub Actions
   - Configure the deployment pipeline using the templates in `.azuredevops/pipelines` or `.github/workflows`

## Testing

### Running Backend Tests

```bash
cd src/backend
dotnet test
```

### Running Frontend Tests

```bash
cd src/web/Tests/VatFilingPricingTool.Web.Tests
dotnet test
```

### End-to-End Tests

```bash
cd src/web/Tests/VatFilingPricingTool.Web.E2E.Tests
dotnet test
```

## Documentation

Comprehensive documentation is available in the `docs` directory:

- Architecture: `docs/architecture/`
- API Documentation: `docs/api/`
- Development Guidelines: `docs/development/`
- Deployment Instructions: `docs/deployment/`

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please follow the coding standards and guidelines in `docs/development/coding-standards.md`.

## License

This project is licensed under the terms of the license included in the repository.

## Contact

For questions or support, please contact the project maintainers.