## Introduction

This document provides comprehensive guidance for disaster recovery procedures for the VAT Filing Pricing Tool. It covers recovery strategies, backup procedures, failover mechanisms, and testing methodologies to ensure business continuity in the event of various disaster scenarios. The disaster recovery plan is designed to meet the specific Recovery Time Objective (RTO) and Recovery Point Objective (RPO) requirements for each component of the system.

## Disaster Recovery Strategy

The VAT Filing Pricing Tool implements a multi-layered disaster recovery strategy to ensure business continuity in the event of various disaster scenarios. The strategy is based on the following principles:

### Recovery Objectives

| Component | Recovery Time Objective (RTO) | Recovery Point Objective (RPO) |
| --- | --- | --- |
| Web Application | < 30 minutes | 0 (no data loss) |
| API Services | < 30 minutes | 0 (no data loss) |
| SQL Database | < 1 hour | < 15 minutes |
| Cosmos DB | < 30 minutes | < 5 minutes |
| Blob Storage | < 2 hours | < 1 hour |
| Overall System | < 1 hour | < 15 minutes |

### Multi-Region Architecture

The system is deployed across two Azure regions:

- **Primary Region**: West Europe (Amsterdam)
- **Secondary Region**: North Europe (Dublin)

This multi-region architecture provides geographic redundancy and enables failover capabilities in the event of a regional outage.

### Data Replication

Critical data is replicated across regions to ensure availability in the event of a disaster:

- **SQL Database**: Geo-replication with auto-failover groups
- **Cosmos DB**: Multi-region writes with automatic failover
- **Blob Storage**: Geo-redundant storage (GRS) with read access (RA-GRS)
- **Redis Cache**: Geo-replication for Premium tier

### Backup Strategy

Regular backups are performed to enable point-in-time recovery:

- **SQL Database**:
  - Full backups: Daily
  - Differential backups: Every 12 hours
  - Transaction log backups: Every 15 minutes
  - Retention period: 35 days

- **Cosmos DB**:
  - Continuous backup with point-in-time restore
  - Retention period: 30 days

- **Blob Storage**:
  - Daily snapshots
  - Soft delete enabled (30-day retention)
  - Retention period: 90 days

### Environment Configuration

The VAT Filing Pricing Tool is deployed across multiple environments with different disaster recovery configurations. Each environment has its own set of resources and DR strategy:

- **Development Environment**:
  - Single-region deployment (West Europe)
  - Daily backups only
  - No automatic failover capabilities
  - RTO: 4 hours, RPO: 24 hours

- **Staging Environment**:
  - Multi-region deployment (West Europe primary, North Europe secondary)
  - Daily backups with manual failover
  - Semi-automated disaster recovery procedures
  - RTO: 2 hours, RPO: 1 hour

- **Production Environment**:
  - Multi-region deployment (West Europe primary, North Europe secondary)
  - Comprehensive backup strategy as outlined above
  - Fully automated failover capabilities
  - RTO: 1 hour, RPO: 15 minutes

Each environment uses appropriate Azure services tiered according to its requirements and budget constraints.

## Disaster Scenarios

The disaster recovery plan addresses the following key scenarios:

### Region Failure

A complete outage of the primary Azure region (West Europe) requiring failover to the secondary region (North Europe).

### Data Corruption

Corruption of data in one or more databases requiring restoration from backups or point-in-time recovery.

### Application Failure

Critical failure in the application code or configuration requiring rollback to a previous version.

### Infrastructure Failure

Failure of infrastructure components such as Kubernetes cluster, networking, or storage requiring recreation from Infrastructure as Code templates.

## Recovery Procedures

Detailed procedures for recovering from each disaster scenario:

### Region Failure Recovery

In the event of a primary region outage, follow these steps to failover to the secondary region:

1. **Assessment and Decision**
   - Confirm primary region outage through Azure Service Health
   - Assess impact on VAT Filing Pricing Tool services
   - Make failover decision based on expected outage duration

2. **Database Failover**
   - **SQL Database**:
     - Verify failover group status using Azure Portal or PowerShell
     - If automatic failover hasn't triggered, initiate manual failover:
       ```powershell
       Switch-AzSqlDatabaseFailoverGroup -ResourceGroupName "<resource-group>" -ServerName "<sql-server-name>" -FailoverGroupName "<failover-group-name>"
       ```
     - Verify failover completion and database availability

   - **Cosmos DB**:
     - Verify multi-region write status
     - If necessary, manually trigger failover priority change:
       ```powershell
       Update-AzCosmosDBAccountFailoverPriority -ResourceGroupName "<resource-group>" -Name "<cosmos-account-name>" -FailoverPolicy <updated-failover-policy>
       ```
     - Verify failover completion and database availability

3. **Traffic Redirection**
   - Update Azure Front Door to route traffic to secondary region:
     ```powershell
     $frontDoor = Get-AzFrontDoor -ResourceGroupName "<resource-group>" -Name "<frontdoor-name>"
     $frontDoor.RoutingRules[0].RouteConfiguration.BackendPool = "secondaryBackendPool"
     Set-AzFrontDoor -ResourceGroupName "<resource-group>" -Name "<frontdoor-name>" -FrontDoorObject $frontDoor
     ```

4. **Verification**
   - Verify application health in secondary region
   - Run smoke tests to confirm functionality
   - Monitor application telemetry for errors

5. **Communication**
   - Notify stakeholders of the failover
   - Update status page
   - Provide estimated resolution time

6. **Post-Failover Actions**
   - Monitor application performance in secondary region
   - Assess primary region status for eventual failback
   - Document incident details for post-mortem analysis

```code_examples
```powershell
./Invoke-RegionFailover.ps1 -ResourceGroupName "vatfilingpricingtool-prod-rg" -Environment "prod" -ForcedFailover $false
```
### Data Corruption Recovery

In the event of data corruption, follow these steps to restore data integrity:

1. **Assessment and Containment**
   - Identify affected databases and extent of corruption
   - Isolate affected services to prevent further corruption
   - Determine corruption timeframe to identify recovery point

2. **SQL Database Recovery**
   - **Point-in-Time Restore**:
     - Identify the time before corruption occurred
     - Restore database to that point in time:
       ```powershell
       Restore-AzSqlDatabase -FromPointInTimeBackup -PointInTime "<time-before-corruption>" -ResourceGroupName "<resource-group>" -ServerName "<sql-server-name>" -TargetDatabaseName "<target-db-name>" -ResourceId "<source-database-resource-id>"
       ```

   - **Backup Restore** (if point-in-time is not suitable):
     - Identify appropriate backup file
     - Restore from backup using the restore-databases.ps1 script:
       ```powershell
       ./restore-databases.ps1 -ResourceGroupName "<resource-group>" -Environment "prod" -RestoreType "FromBackup" -SqlServerName "<sql-server-name>" -SqlDatabaseName "<database-name>" -BackupStorageAccountName "<storage-account>" -SqlBackupFileName "<backup-file-name>"
       ```

3. **Cosmos DB Recovery**
   - **Point-in-Time Restore**:
     - Identify the time before corruption occurred
     - Restore Cosmos DB account to that point in time:
       ```powershell
       Restore-AzCosmosDBAccount -ResourceGroupName "<resource-group>" -Name "<cosmos-account-name>" -RestoreTimestampInUtc "<time-before-corruption>" -TargetDatabaseAccountName "<target-account-name>"
       ```

   - **Backup Restore** (if point-in-time is not suitable):
     - Identify appropriate backup files
     - Restore from backup using the restore-databases.ps1 script:
       ```powershell
       ./restore-databases.ps1 -ResourceGroupName "<resource-group>" -Environment "prod" -RestoreType "FromBackup" -CosmosDBAccountName "<cosmos-account-name>" -CosmosDBDatabaseName "<database-name>" -BackupStorageAccountName "<storage-account>" -CosmosContainers "Rules,AuditLogs,Configurations"
       ```

4. **Verification**
   - Validate restored data integrity
   - Run data consistency checks
   - Verify application functionality with restored data

5. **Service Restoration**
   - Update connection strings if using new database names
   - Restart affected services
   - Monitor application for errors

6. **Post-Recovery Actions**
   - Document incident details
   - Analyze root cause of corruption
   - Implement preventive measures

```code_examples
```powershell
./restore-databases.ps1 -ResourceGroupName "vatfilingpricingtool-prod-rg" -Environment "prod" -RestoreType "PointInTime" -SqlServerName "vatfilingpricingtool-prod-sql" -SqlDatabaseName "VatFilingPricingTool" -PointInTimeUtc "2023-05-10T08:00:00Z" -TargetDatabaseName "VatFilingPricingTool-Restored"
```
### Application Failure Recovery

In the event of a critical application failure, follow these steps to restore service:

1. **Assessment and Containment**
   - Identify affected application components
   - Assess impact on users and business operations
   - Isolate affected components if possible

2. **Rollback Deployment**
   - Identify the last known good version
   - Rollback to previous version using blue/green deployment:
     ```powershell
     # For AKS-hosted components
     kubectl rollout undo deployment/<deployment-name> --namespace=vatfilingpricingtool-prod
     
     # For App Service
     Swap-AzWebAppSlot -ResourceGroupName "<resource-group>" -Name "<app-service-name>" -SourceSlotName "production" -DestinationSlotName "staging"
     ```

3. **Configuration Rollback**
   - If the issue is configuration-related, restore previous configuration:
     ```powershell
     # Restore App Configuration from snapshot
     Restore-AzAppConfigurationStore -ResourceGroupName "<resource-group>" -Name "<appconfig-name>" -RestoreSourceId "<snapshot-id>"
     
     # Restore Key Vault secrets if needed
     foreach ($secret in $secretsToRestore) {
         Restore-AzKeyVaultSecret -VaultName "<keyvault-name>" -InputObject $secret
     }
     ```

4. **Verification**
   - Verify application health after rollback
   - Run smoke tests to confirm functionality
   - Monitor application telemetry for errors

5. **Service Restoration**
   - Update DNS/traffic routing if necessary
   - Verify all dependent services are functioning
   - Monitor application performance

6. **Post-Recovery Actions**
   - Document incident details
   - Analyze root cause of failure
   - Develop fix for the issue in development environment
   - Test fix thoroughly before redeploying

7. **Deployment Process for Fixed Version**
   - Once a fix is ready, follow these deployment steps:
     - Deploy to development environment and validate
     - Deploy to staging environment using blue/green methodology
     - Run full test suite on staging
     - Schedule production deployment during maintenance window
     - Deploy to production using blue/green deployment
     - Gradually shift traffic to new version
     - Monitor for issues during and after deployment

```code_examples
```powershell
./Rollback-Application.ps1 -ResourceGroupName "vatfilingpricingtool-prod-rg" -Environment "prod" -Version "1.2.5" -Component "all"
```
### Infrastructure Failure Recovery

In the event of infrastructure failure, follow these steps to restore the infrastructure:

1. **Assessment and Containment**
   - Identify failed infrastructure components
   - Assess impact on application services
   - Isolate affected components if possible

2. **Infrastructure Restoration**
   - Use Infrastructure as Code to recreate failed components:
     ```powershell
     # Using ARM templates
     New-AzResourceGroupDeployment -ResourceGroupName "<resource-group>" -TemplateFile "./infrastructure/azure/arm-templates/main.json" -TemplateParameterFile "./infrastructure/azure/arm-templates/parameters.json"
     
     # Using Terraform
     cd ./infrastructure/azure/terraform
     terraform init
     terraform apply -var-file="./environments/prod/terraform.tfvars"
     ```

3. **Kubernetes Cluster Recovery**
   - If AKS cluster is affected, restore using IaC or create new cluster and redeploy applications:
     ```powershell
     # Deploy Kubernetes resources
     kubectl apply -f ./infrastructure/kubernetes/namespace.yaml
     kubectl apply -f ./infrastructure/kubernetes/backend/
     kubectl apply -f ./infrastructure/kubernetes/web/
     kubectl apply -f ./infrastructure/kubernetes/monitoring/
     ```

4. **Data Restoration**
   - If data storage is affected, restore from backups or geo-replicated instances
   - Follow data corruption recovery procedure if needed

5. **Verification**
   - Verify infrastructure health
   - Verify application deployment and connectivity
   - Run smoke tests to confirm functionality

6. **Post-Recovery Actions**
   - Document incident details
   - Analyze root cause of infrastructure failure
   - Implement preventive measures
   - Update infrastructure templates if needed

```code_examples
```powershell
./deploy.ps1 -DeploymentType Terraform -Environment prod -Location westeurope -SecondaryLocation northeurope -ResourceGroupName "vatfilingpricingtool-prod-rg"
```

## Failback Procedures

After a disaster has been mitigated and the primary region or system has been restored, follow these procedures to failback to the primary configuration:

### Region Failback

1. **Assessment and Planning**
   - Verify primary region is fully operational
   - Plan failback during low-traffic period
   - Notify stakeholders of planned failback

2. **Data Synchronization**
   - Ensure all data changes in secondary region are replicated back to primary
   - Verify data consistency between regions

3. **Failback Execution**
   - **SQL Database**:
     - Initiate failover group failback:
       ```powershell
       Switch-AzSqlDatabaseFailoverGroup -ResourceGroupName "<resource-group>" -ServerName "<secondary-sql-server-name>" -FailoverGroupName "<failover-group-name>"
       ```

   - **Cosmos DB**:
     - Update failover priorities to restore original configuration:
       ```powershell
       Update-AzCosmosDBAccountFailoverPriority -ResourceGroupName "<resource-group>" -Name "<cosmos-account-name>" -FailoverPolicy <original-failover-policy>
       ```

4. **Traffic Redirection**
   - Update Azure Front Door to route traffic back to primary region:
     ```powershell
     $frontDoor = Get-AzFrontDoor -ResourceGroupName "<resource-group>" -Name "<frontdoor-name>"
     $frontDoor.RoutingRules[0].RouteConfiguration.BackendPool = "primaryBackendPool"
     Set-AzFrontDoor -ResourceGroupName "<resource-group>" -Name "<frontdoor-name>" -FrontDoorObject $frontDoor
     ```

5. **Verification**
   - Verify application health in primary region
   - Run smoke tests to confirm functionality
   - Monitor application telemetry for errors

6. **Post-Failback Actions**
   - Document failback process
   - Update disaster recovery documentation with lessons learned
   - Review and optimize failover/failback procedures

### Application Failback

1. **Assessment and Planning**
   - Verify fixed application version is thoroughly tested
   - Plan deployment during low-traffic period
   - Notify stakeholders of planned update

2. **Deployment Preparation**
   - Prepare blue/green deployment of fixed version
   - Verify all dependencies and configurations

3. **Failback Execution**
   - Deploy fixed version using blue/green deployment strategy
   - Gradually shift traffic to new version
   - Monitor for errors during transition

4. **Verification**
   - Verify application health with new version
   - Run comprehensive tests to confirm functionality
   - Monitor application telemetry for errors

5. **Post-Failback Actions**
   - Document deployment process
   - Update application version documentation
   - Review and optimize deployment procedures

## Disaster Recovery Testing

Regular testing of disaster recovery procedures is essential to ensure their effectiveness. The following testing approach is implemented:

### Testing Schedule

| Test Type | Frequency | Environment | Participants |
| --- | --- | --- | --- |
| Tabletop Exercise | Quarterly | N/A | DR Team, Operations Team |
| Database Recovery Test | Quarterly | Development | Database Team, DR Team |
| Application Recovery Test | Quarterly | Staging | Development Team, DR Team |
| Full DR Drill | Annually | Production (off-hours) | All Teams |

### Testing Methodology

1. **Tabletop Exercise**
   - Simulate disaster scenarios
   - Walk through recovery procedures
   - Identify gaps and improvements

2. **Database Recovery Test**
   - Restore databases from backups
   - Perform point-in-time recovery
   - Validate data integrity

3. **Application Recovery Test**
   - Deploy application to recovery environment
   - Verify functionality
   - Measure recovery time

4. **Full DR Drill**
   - Simulate complete disaster scenario
   - Execute full recovery procedures
   - Measure RTO and RPO
   - Validate business continuity

### Test Execution

1. **Preparation**
   - Define test objectives and scope
   - Prepare test environment
   - Notify stakeholders

2. **Execution**
   - Follow documented recovery procedures
   - Document actual steps taken
   - Measure recovery time and data loss

3. **Validation**
   - Verify system functionality
   - Validate data integrity
   - Confirm service level objectives

4. **Documentation**
   - Document test results
   - Identify gaps and issues
   - Update recovery procedures as needed

```code_examples
```powershell
./Test-DisasterRecovery.ps1 -ResourceGroupName "vatfilingpricingtool-test-rg" -TestType "RegionFailover" -Environment "test"
```

## Backup Management

Comprehensive backup management ensures data can be recovered in the event of a disaster:

### Backup Schedule

| Data Type | Backup Type | Frequency | Retention | Storage |
| --- | --- | --- | --- | --- |
| SQL Database | Full | Daily | 35 days | Geo-redundant storage |
| SQL Database | Differential | 12 hours | 35 days | Geo-redundant storage |
| SQL Database | Transaction Log | 15 minutes | 35 days | Geo-redundant storage |
| Cosmos DB | Continuous | Real-time | 30 days | Geo-redundant storage |
| Blob Storage | Snapshot | Daily | 90 days | Geo-redundant storage |
| Configuration | Full | Weekly | 90 days | Geo-redundant storage |

### Backup Procedures

1. **Automated Backups**
   - SQL Database automated backups are configured through Azure
   - Cosmos DB continuous backup is enabled
   - Blob Storage snapshots are scheduled

2. **Manual Backups**
   - Additional manual backups before major changes
   - Export of critical configuration data
   - Use backup-databases.ps1 script for manual backups:
     ```powershell
     ./backup-databases.ps1 -ResourceGroupName "<resource-group>" -Environment "prod" -BackupType "Full" -SqlServerName "<sql-server-name>" -SqlDatabaseName "<database-name>" -CosmosDBAccountName "<cosmos-account-name>" -BackupStorageAccountName "<storage-account>"
     ```

3. **Backup Validation**
   - Regular validation of backup integrity
   - Test restores to verify backup usability
   - Automated validation after backup completion

### Backup Storage

1. **Storage Configuration**
   - Geo-redundant storage (GRS) for all backups
   - Immutable storage for regulatory compliance
   - Encryption at rest for all backup data

2. **Access Control**
   - Restricted access to backup storage
   - Audit logging of all backup access
   - Separate credentials for backup operations

## Roles and Responsibilities

Clear definition of roles and responsibilities ensures effective disaster recovery execution:

### Disaster Recovery Team

| Role | Responsibilities | Contact |
| --- | --- | --- |
| DR Coordinator | Overall coordination of DR activities | dr-coordinator@example.com |
| Database Administrator | Database recovery operations | dba@example.com |
| Infrastructure Engineer | Infrastructure recovery operations | infra@example.com |
| Application Developer | Application recovery operations | dev@example.com |
| Security Officer | Security validation during recovery | security@example.com |
| Communications Lead | Stakeholder communications | communications@example.com |

### Escalation Path

1. **Level 1: DR Team**
   - Initial response and assessment
   - Implementation of recovery procedures
   - Regular status updates

2. **Level 2: Technical Management**
   - Escalation for complex issues
   - Resource allocation
   - Vendor engagement

3. **Level 3: Executive Management**
   - Escalation for critical business impact
   - Business continuity decisions
   - External communications approval

### Communication Plan

1. **Internal Communication**
   - Regular status updates to stakeholders
   - Technical updates to IT teams
   - Executive briefings

2. **External Communication**
   - Customer notifications
   - Vendor coordination
   - Regulatory reporting if required

## Documentation and Maintenance

Keeping disaster recovery documentation up-to-date is critical for effective response:

### Documentation Requirements

1. **Recovery Procedures**
   - Step-by-step instructions for each scenario
   - Command references and examples
   - Verification steps

2. **Contact Information**
   - DR team contact details
   - Vendor support contacts
   - Escalation contacts

3. **System Configuration**
   - Current architecture diagrams
   - Resource inventory
   - Dependency mapping

### Maintenance Schedule

| Activity | Frequency | Responsibility |
| --- | --- | --- |
| DR Plan Review | Quarterly | DR Coordinator |
| Contact Information Update | Monthly | DR Coordinator |
| Recovery Procedure Testing | Quarterly | DR Team |
| Full DR Documentation Review | Annually | All Teams |

### Change Management

1. **Infrastructure Changes**
   - Update DR documentation for all infrastructure changes
   - Validate recovery procedures after changes
   - Test recovery for critical changes

2. **Application Changes**
   - Update recovery procedures for major releases
   - Validate backup and restore procedures
   - Update dependency documentation

## Compliance and Reporting

Disaster recovery activities must comply with regulatory requirements and internal policies:

### Regulatory Requirements

1. **GDPR Compliance**
   - Ensure data protection during recovery
   - Document data processing activities
   - Maintain appropriate security measures

2. **SOC 2 Compliance**
   - Document recovery procedures
   - Maintain evidence of testing
   - Ensure appropriate access controls

3. **ISO 27001 Compliance**
   - Align with business continuity requirements
   - Regular testing and improvement
   - Documentation and training

### Reporting Requirements

1. **Test Reports**
   - Document test objectives and scope
   - Record test results and metrics
   - Identify issues and improvements

2. **Incident Reports**
   - Document incident timeline
   - Record recovery actions taken
   - Measure against RTO and RPO
   - Identify lessons learned

3. **Compliance Reports**
   - Regular reporting to compliance team
   - Evidence collection for audits
   - Documentation of control effectiveness

## Continuous Improvement

The disaster recovery plan is subject to continuous improvement based on testing, incidents, and changing requirements:

### Improvement Process

1. **Identify Improvements**
   - Test results analysis
   - Incident post-mortems
   - Technology advancements
   - Changing business requirements

2. **Prioritize Changes**
   - Impact on RTO and RPO
   - Implementation effort
   - Risk reduction
   - Cost considerations

3. **Implement Improvements**
   - Update procedures and documentation
   - Train team members
   - Test new procedures
   - Monitor effectiveness

### Metrics and KPIs

1. **Recovery Time**
   - Actual vs. target RTO
   - Time to detect incidents
   - Time to initiate recovery
   - Time to complete recovery

2. **Data Loss**
   - Actual vs. target RPO
   - Amount of data restored
   - Data validation success rate

3. **Operational Metrics**
   - Test completion rate
   - Documentation currency
   - Team readiness
   - Successful recovery rate

## Appendices

Additional reference materials and resources:

### Recovery Scripts

- backup-databases.ps1: Script for database backup operations
- restore-databases.ps1: Script for database restore operations
- Invoke-RegionFailover.ps1: Script for region failover operations
- Test-DisasterRecovery.ps1: Script for DR testing

### Checklists

- Region Failover Checklist
- Data Recovery Checklist
- Application Recovery Checklist
- DR Test Checklist

### Reference Architecture

- Multi-region deployment architecture
- Database replication configuration
- Network failover configuration
- Backup storage architecture

### Vendor Support

- Microsoft Azure Support: https://azure.microsoft.com/support/options/
- SQL Database Support: https://docs.microsoft.com/azure/sql-database/sql-database-get-support
- Cosmos DB Support: https://docs.microsoft.com/azure/cosmos-db/support