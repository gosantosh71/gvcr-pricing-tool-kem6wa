{% globals %}

## Introduction

This document provides comprehensive guidance for setting up and configuring the deployment environments for the VAT Filing Pricing Tool. It covers the infrastructure provisioning, configuration management, and environment-specific settings for development, staging, and production environments.

## Prerequisites

Before setting up the deployment environments, ensure you have the following prerequisites in place:

### Required Tools

- Azure CLI (latest version)
- Terraform (~> 1.3.0)
- PowerShell 7.2+ (for Windows) or Bash (for Linux/macOS)
- Kubernetes CLI (kubectl)
- Helm
- Git
- Docker

### Azure Subscription

- Active Azure subscription with appropriate permissions
- Service Principal with Contributor role for automated deployments
- Azure AD tenant for authentication and authorization

### Access Permissions

- Contributor access to Azure subscription
- Owner or User Access Administrator role for creating service principals
- Access to Azure Container Registry
- Access to Azure Key Vault

## Environment Architecture

The VAT Filing Pricing Tool uses a multi-environment architecture with separate environments for development, staging, and production. Each environment is deployed to Azure with appropriate isolation and security controls.

### Environment Types

- **Development**: Used for ongoing development and testing
- **Staging**: Used for pre-production validation and user acceptance testing
- **Production**: Used for the live application serving real users

### Multi-Region Strategy

- **Primary Region**: West Europe (Amsterdam)
- **Secondary Region**: North Europe (Dublin)
- Geo-replication for databases and storage
- Traffic Manager for global load balancing

### Resource Isolation

- Separate resource groups for each environment
- Separate virtual networks with appropriate peering
- Separate Kubernetes namespaces for application components
- Environment-specific configuration and secrets

## Infrastructure as Code

The VAT Filing Pricing Tool infrastructure is defined and managed using Infrastructure as Code (IaC) to ensure consistency, repeatability, and version control.

### Terraform Configuration

The primary IaC tool used is Terraform, with configurations stored in the `infrastructure/azure/terraform` directory. The main components include:

- `main.tf`: Main Terraform configuration file
- `variables.tf`: Variable definitions
- `outputs.tf`: Output definitions
- `environments/`: Environment-specific variable values
- `modules/`: Reusable Terraform modules

### ARM Templates

Azure Resource Manager (ARM) templates are available as an alternative deployment method, stored in the `infrastructure/azure/arm-templates` directory. These templates provide a declarative way to define Azure resources.

### Kubernetes Manifests

Kubernetes resources are defined using YAML manifests stored in the `infrastructure/kubernetes` directory, organized by component:

- `namespace.yaml`: Namespace definitions
- `backend/`: Backend service manifests
- `web/`: Web application manifests
- `monitoring/`: Monitoring component manifests

### Helm Charts

Helm charts are used for more complex Kubernetes deployments, stored in the `infrastructure/helm` directory. The main chart is `vatfilingpricingtool` with templates for all application components.

## Deployment Automation

Deployment of the VAT Filing Pricing Tool is automated using scripts and CI/CD pipelines to ensure consistent and reliable deployments.

### Deployment Scripts

PowerShell and Bash scripts are provided in the `infrastructure/scripts` directory to automate the deployment process:

- `deploy.ps1`/`deploy.sh`: Main deployment script
- `initialize-keyvault.ps1`/`initialize-keyvault.sh`: Script to initialize Key Vault with secrets
- `backup-databases.ps1`/`backup-databases.sh`: Script to backup databases
- `restore-databases.ps1`/`restore-databases.sh`: Script to restore databases

### CI/CD Integration

The deployment scripts are integrated with the CI/CD pipelines defined in Azure DevOps Pipelines and GitHub Actions. The pipeline configurations automate the testing, building, and deployment processes across different environments. For detailed information on pipelines, refer to the CI/CD pipeline documentation in the project repository.

### Deployment Parameters

The deployment scripts accept parameters to customize the deployment, including:

- `DeploymentType`: ARM or Terraform
- `Environment`: dev, staging, or prod
- `Location`: Primary Azure region
- `SecondaryLocation`: Secondary Azure region for disaster recovery
- `ResourceGroupName`: Name of the resource group
- Additional parameters for specific resources

## Development Environment Setup

The development environment is used for ongoing development and testing of the VAT Filing Pricing Tool.

### Resource Specifications

The development environment uses smaller, cost-effective resources:

- **Compute**: 2 vCPU, 8GB RAM
- **AKS**: 2 nodes, Standard_D2s_v3
- **SQL Database**: Standard S2 tier
- **App Service**: Standard S1 tier
- **API Management**: Developer tier
- **Storage**: Standard LRS

### Deployment Steps

1. Clone the repository
2. Navigate to the `infrastructure/scripts` directory
3. Run the deployment script with development parameters:
   ```powershell
   ./deploy.ps1 -DeploymentType Terraform -Environment dev -Location westeurope -SecondaryLocation northeurope
   ```
4. Verify the deployment by accessing the development endpoints
5. Configure monitoring as described in [Monitoring Setup](monitoring-setup.md)

### Configuration

Development environment configuration is stored in `infrastructure/azure/terraform/environments/dev/terraform.tfvars` with development-specific values for all variables.

### Access Control

- Development team members have Contributor access to the development resource group
- CI/CD pipelines use a service principal with Contributor access
- Azure AD authentication is configured with development tenant

### Monitoring

Development environment monitoring is configured with:

- Application Insights for application monitoring
- Azure Monitor for resource monitoring
- Prometheus and Grafana for Kubernetes monitoring
- Log Analytics for log aggregation

See [Monitoring Setup](monitoring-setup.md) for detailed monitoring configuration.

## Staging Environment Setup

The staging environment is used for pre-production validation and user acceptance testing of the VAT Filing Pricing Tool.

### Resource Specifications

The staging environment uses moderate resources to simulate production:

- **Compute**: 4 vCPU, 16GB RAM
- **AKS**: 3 nodes, Standard_D4s_v3
- **SQL Database**: Standard S3 tier
- **App Service**: Standard S2 tier
- **API Management**: Standard tier
- **Storage**: Standard GRS

### Deployment Steps

1. Ensure development environment is stable
2. Navigate to the `infrastructure/scripts` directory
3. Run the deployment script with staging parameters:
   ```powershell
   ./deploy.ps1 -DeploymentType Terraform -Environment staging -Location westeurope -SecondaryLocation northeurope
   ```
4. Verify the deployment by accessing the staging endpoints
5. Configure monitoring as described in [Monitoring Setup](monitoring-setup.md)
6. Perform user acceptance testing

### Configuration

Staging environment configuration is stored in `infrastructure/azure/terraform/environments/staging/terraform.tfvars` with staging-specific values for all variables.

### Access Control

- Limited team members have Contributor access to the staging resource group
- CI/CD pipelines use a service principal with Contributor access
- Azure AD authentication is configured with production tenant
- Additional security controls are in place

### Monitoring

Staging environment monitoring is configured with:

- Application Insights for application monitoring
- Azure Monitor for resource monitoring
- Prometheus and Grafana for Kubernetes monitoring
- Log Analytics for log aggregation
- Alert rules for critical issues

See [Monitoring Setup](monitoring-setup.md) for detailed monitoring configuration.

## Production Environment Setup

The production environment is used for the live application serving real users of the VAT Filing Pricing Tool.

### Resource Specifications

The production environment uses robust resources for optimal performance and reliability:

- **Compute**: 8 vCPU, 32GB RAM (auto-scaling)
- **AKS**: 3-10 nodes (auto-scaling), Standard_D4s_v3
- **SQL Database**: Business Critical, Gen5, 8 vCores
- **App Service**: Premium P2v3 tier
- **API Management**: Standard tier
- **Storage**: Premium GRS

### Deployment Steps

1. Ensure staging environment is validated
2. Obtain necessary approvals for production deployment
3. Navigate to the `infrastructure/scripts` directory
4. Run the deployment script with production parameters:
   ```powershell
   ./deploy.ps1 -DeploymentType Terraform -Environment prod -Location westeurope -SecondaryLocation northeurope
   ```
5. Verify the deployment by accessing the production endpoints
6. Configure monitoring as described in [Monitoring Setup](monitoring-setup.md)
7. Perform smoke tests and validation

### Configuration

Production environment configuration is stored in `infrastructure/azure/terraform/environments/prod/terraform.tfvars` with production-specific values for all variables.

### Access Control

- Strict access control with limited team members having Contributor access
- CI/CD pipelines use a service principal with restricted permissions
- Azure AD authentication is configured with production tenant
- Just-in-time access for administrative functions
- Privileged Identity Management for role elevation

### Monitoring

Production environment monitoring is configured with:

- Application Insights for application monitoring
- Azure Monitor for resource monitoring
- Prometheus and Grafana for Kubernetes monitoring
- Log Analytics for log aggregation
- Comprehensive alert rules with appropriate notification channels
- SLA monitoring and reporting

See [Monitoring Setup](monitoring-setup.md) for detailed monitoring configuration.

### Disaster Recovery

Production environment includes disaster recovery configuration:

- Geo-replicated databases and storage
- Secondary region deployment for failover
- Regular backups with appropriate retention
- Documented recovery procedures
- Regular DR testing

See [Disaster Recovery](disaster-recovery.md) for detailed disaster recovery procedures.

## Environment-Specific Configuration

Each environment has specific configuration settings to customize the behavior of the VAT Filing Pricing Tool.

### Configuration Sources

Environment-specific configuration comes from multiple sources:

- Terraform variables in `environments/<env>/terraform.tfvars`
- Kubernetes ConfigMaps in `kubernetes/<component>/config.yaml`
- Application settings in `appsettings.<Environment>.json`
- Environment variables in container definitions

### Secret Management

Secrets are managed using Azure Key Vault:

- Each environment has its own Key Vault instance
- Secrets are initialized during deployment using `initialize-keyvault.ps1`
- Applications access secrets using managed identities
- Kubernetes secrets are synced from Key Vault

### Feature Flags

Feature flags are used to enable or disable features in specific environments:

- Configured in Azure App Configuration
- Environment-specific feature flag settings
- Feature flags for gradual rollout of new features

### Connection Strings

Database and service connection strings are environment-specific:

- Stored as secrets in Key Vault
- Referenced in application configuration
- Injected as environment variables in containers

## Kubernetes Configuration

The VAT Filing Pricing Tool uses Kubernetes (AKS) for container orchestration, with environment-specific configurations.

### Namespace Structure

Each environment uses a dedicated namespace:

- `vatfilingpricingtool-dev`
- `vatfilingpricingtool-staging`
- `vatfilingpricingtool-prod`

### Resource Quotas

Resource quotas are defined for each namespace to limit resource usage:

- CPU and memory limits
- Storage quotas
- Object count limits

### Network Policies

Network policies control traffic between pods:

- Default deny all ingress/egress
- Explicit allow rules for required communication
- Environment-specific network policies

### Service Accounts

Service accounts with appropriate permissions:

- Component-specific service accounts
- RBAC roles and bindings
- Integration with Azure AD for pod identity

### Deployment Configuration

Deployment configurations vary by environment:

- Development: Single replica, rolling updates
- Staging: Multiple replicas, canary deployments
- Production: Multiple replicas, blue/green deployments

## Database Configuration

The VAT Filing Pricing Tool uses Azure SQL Database and Cosmos DB, with environment-specific configurations.

### Azure SQL Database

SQL Database configuration varies by environment:

- Development: Standard S2 tier, local redundancy
- Staging: Standard S3 tier, geo-redundancy
- Production: Business Critical, Gen5, 8 vCores, geo-redundancy with auto-failover

### Cosmos DB

Cosmos DB configuration varies by environment:

- Development: Session consistency, single region
- Staging: Bounded staleness consistency, two regions
- Production: Bounded staleness consistency, multi-region writes

### Database Initialization

Databases are initialized during deployment:

- Entity Framework Core migrations for SQL Database
- Container and index creation for Cosmos DB
- Seeding of reference data

### Backup and Restore

Backup and restore procedures:

- Automated backups with environment-specific retention
- Point-in-time restore capability
- Geo-restore for disaster recovery
- Scripts for manual backup and restore

## Networking Configuration

The VAT Filing Pricing Tool uses Azure networking services with environment-specific configurations.

### Virtual Network

Each environment has its own virtual network:

- Development: 10.0.0.0/16
- Staging: 10.1.0.0/16
- Production: 10.2.0.0/16

### Subnets

Subnets for different components:

- App subnet: x.x.1.0/24
- AKS subnet: x.x.2.0/24
- Database subnet: x.x.3.0/24

### Network Security

Network security controls:

- Network Security Groups (NSGs)
- Azure Firewall
- Private Endpoints for PaaS services
- Service Endpoints for secure access

### Load Balancing

Load balancing configuration:

- Azure Front Door for global load balancing
- Application Gateway for web traffic
- Internal load balancers for service-to-service communication

## Monitoring and Logging

The VAT Filing Pricing Tool uses comprehensive monitoring and logging, with environment-specific configurations.

### Application Insights

Application performance monitoring:

- Separate Application Insights instance per environment
- Environment-specific instrumentation keys
- Sampling rates adjusted by environment

### Log Analytics

Centralized logging:

- Separate Log Analytics workspace per environment
- Log retention policies by environment
- Query packs for common analysis

### Prometheus and Grafana

Kubernetes monitoring:

- Deployed to each AKS cluster
- Environment-specific dashboards
- Alert rules adjusted by environment

### Alerting

Alert configuration:

- Development: Minimal alerts, email only
- Staging: Moderate alerts, email and Teams
- Production: Comprehensive alerts, email, Teams, and SMS

## Security Configuration

The VAT Filing Pricing Tool implements comprehensive security measures, with environment-specific configurations.

### Identity and Access Management

Azure AD integration:

- Development: Development tenant
- Staging and Production: Production tenant
- Role-based access control
- Conditional Access policies

### Key Vault

Secret management:

- Separate Key Vault per environment
- Access policies based on environment
- Soft delete and purge protection
- Managed identities for access

### Network Security

Network protection:

- Development: Basic security
- Staging: Enhanced security
- Production: Maximum security with WAF, private endpoints, and network isolation

### Compliance

Compliance controls:

- GDPR compliance in all environments
- SOC 2 controls in staging and production
- ISO 27001 controls in production
- Regular compliance scanning

## Maintenance Procedures

Regular maintenance procedures for the VAT Filing Pricing Tool environments.

### Patching

Regular patching schedule:

- OS updates for AKS nodes
- Kubernetes version upgrades
- Database patching
- Application dependencies

### Backup Verification

Regular backup verification:

- Test restores from backups
- Validation of backup integrity
- Disaster recovery testing

### Performance Tuning

Regular performance optimization:

- Database index maintenance
- Query optimization
- Resource scaling adjustments
- Cache configuration tuning

### Security Updates

Security maintenance:

- Vulnerability scanning
- Security patch application
- Credential rotation
- Security policy updates

## Troubleshooting

Guidance for troubleshooting common issues with the VAT Filing Pricing Tool environments.

### Deployment Issues

Troubleshooting deployment failures:

- Check deployment logs
- Verify permissions
- Validate templates and configurations
- Common error resolutions

### Application Issues

Troubleshooting application problems:

- Check application logs
- Verify configuration
- Test connectivity
- Common application error resolutions

### Infrastructure Issues

Troubleshooting infrastructure problems:

- Check resource health
- Verify network connectivity
- Validate security settings
- Common infrastructure error resolutions

### Performance Issues

Troubleshooting performance problems:

- Identify bottlenecks
- Check resource utilization
- Analyze database performance
- Common performance optimization techniques

## References

Additional resources and references related to environment setup.

### Azure Documentation

Links to relevant Azure documentation.

### Terraform Documentation

Links to Terraform documentation and best practices.

### Kubernetes Documentation

Links to Kubernetes documentation and AKS-specific guidance.

### Project Documentation

Links to other relevant documentation within the project.