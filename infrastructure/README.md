# VAT Filing Pricing Tool Infrastructure

This repository contains the infrastructure as code (IaC) definitions and deployment scripts for the VAT Filing Pricing Tool. The infrastructure is designed to be deployed on Microsoft Azure with a focus on scalability, security, and high availability.

## Architecture Overview

The VAT Filing Pricing Tool uses a cloud-native architecture deployed on Microsoft Azure with multi-region configuration for high availability and disaster recovery. The primary region is Western Europe with a secondary region in Northern Europe.

## Directory Structure

- `azure/arm-templates/`: Azure Resource Manager templates for resource provisioning
- `azure/terraform/`: Terraform configurations for infrastructure management
- `scripts/`: Deployment and management scripts
- `kubernetes/`: Kubernetes manifests for container orchestration
- `helm/`: Helm charts for Kubernetes application deployment

## Azure Resources

The application uses the following Azure services:

- Azure App Service: Host web application (Premium v3)
- Azure Kubernetes Service: Host microservices (Standard with 3 nodes)
- Azure SQL Database: Primary data storage (Business Critical)
- Azure Cosmos DB: Store country-specific rules (Multi-region writes)
- Azure Blob Storage: Store reports and documents (GRS)
- Azure Redis Cache: Caching layer (Premium)
- Azure API Management: API gateway
- Azure Front Door: Global load balancing
- Azure Key Vault: Secret management
- Azure Monitor: Monitoring and alerting

## Deployment Instructions

### Prerequisites

- Azure CLI
- Terraform CLI (v1.0+)
- Kubernetes CLI (kubectl)
- Helm CLI
- PowerShell or Bash

### Deployment Steps

1. **Initialize Azure Resources**:
   ```bash
   cd infrastructure/azure/terraform
   terraform init
   terraform plan -var-file=environments/dev/terraform.tfvars
   terraform apply -var-file=environments/dev/terraform.tfvars
   ```

2. **Configure Kubernetes**:
   ```bash
   az aks get-credentials --resource-group vat-pricing-tool-rg --name vat-pricing-tool-aks
   kubectl apply -f infrastructure/kubernetes/namespace.yaml
   ```

3. **Deploy Application**:
   ```bash
   cd infrastructure/helm
   helm install vatfilingpricingtool ./vatfilingpricingtool -f values.yaml
   ```

## High Availability and Disaster Recovery

The infrastructure is designed with high availability and disaster recovery in mind:

- Multi-region deployment with Azure Front Door for global load balancing
- Azure SQL Database with geo-replication
- Cosmos DB with multi-region writes
- Geo-redundant storage for blob storage
- Kubernetes pods distributed across availability zones

Refer to the disaster recovery documentation for detailed recovery procedures.

## Security Considerations

The infrastructure implements several security measures:

- Azure Virtual Network with NSGs
- Private Endpoints for PaaS services
- TDE for SQL, encryption at rest for all storage
- HTTPS/TLS 1.2+ for all communications
- Azure AD with MFA for authentication
- Azure Key Vault for secret management

## Monitoring and Observability

The infrastructure includes comprehensive monitoring:

- Azure Monitor for resource monitoring
- Application Insights for application performance monitoring
- Azure Log Analytics for centralized logging
- Custom dashboards for different stakeholders
- Alerting based on predefined thresholds

## Cost Optimization

Cost optimization strategies include:

- Auto-scaling to reduce costs during off-hours (30-40% savings)
- Reserved Instances for base capacity (20-30% savings)
- Storage tiering for older data (40-50% savings)
- Dev/Test subscriptions for non-production environments (40-60% savings)

## Maintenance Procedures

Regular maintenance procedures include:

- OS Patching (AKS): Monthly, rolling updates
- Database Maintenance: Weekly, using replicas
- Security Updates: As needed, using blue/green deployment
- Performance Tuning: Quarterly

Maintenance windows:
- Development: Anytime
- Staging: Weekdays 8 PM - 6 AM
- Production: Sundays 2 AM - 6 AM (monthly)

## CI/CD Pipeline

The infrastructure is deployed and updated using Azure DevOps Pipelines:

- Source control in Azure DevOps Repos
- Infrastructure changes validated through pull requests
- Terraform plan output reviewed before apply
- Automated testing of infrastructure changes
- Staged deployment through environments (dev → staging → production)

## Contributing

When contributing to the infrastructure code:

1. Create a feature branch from main
2. Make your changes following the coding standards
3. Test your changes locally
4. Submit a pull request for review
5. Address any feedback from reviewers

All infrastructure changes must be reviewed by at least one infrastructure team member.

This infrastructure is designed to provide a robust, secure, and scalable foundation for the VAT Filing Pricing Tool. For questions or issues, please contact the infrastructure team.