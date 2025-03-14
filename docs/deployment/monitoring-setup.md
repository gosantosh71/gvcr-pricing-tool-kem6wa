## Introduction

This document provides detailed instructions for setting up and configuring the monitoring infrastructure for the VAT Filing Pricing Tool. The monitoring solution combines Azure's native monitoring services with open-source tools deployed in Kubernetes to provide comprehensive observability across all application components and infrastructure resources.

## Monitoring Architecture

The VAT Filing Pricing Tool implements a multi-layered monitoring architecture that provides comprehensive visibility into system health, performance, and security.

### Architecture Overview

The monitoring architecture consists of the following key components:

- **Azure Monitor**: Native Azure monitoring for infrastructure resources
- **Application Insights**: Application performance monitoring and user telemetry
- **Log Analytics**: Centralized log storage and analysis
- **Prometheus**: Metrics collection in Kubernetes environments // Prometheus v2.42.0
- **Grafana**: Visualization and dashboarding // Grafana 9.3.2
- **Loki**: Log aggregation for Kubernetes workloads // Loki 2.7.3
- **Azure Alerts**: Notification system for critical events

### Data Flow

1. Application components emit telemetry via Application Insights SDK
2. Kubernetes resources emit metrics collected by Prometheus
3. Container logs are collected by Loki
4. Azure resources emit metrics and logs to Azure Monitor
5. Log Analytics centralizes logs from multiple sources
6. Grafana visualizes metrics from Prometheus and Azure Monitor
7. Alert rules in Prometheus and Azure Monitor trigger notifications

### Component Interaction Diagram

```
+---------------------+     +---------------------+
|                     |     |                     |
|  Azure Resources    |     |  AKS Cluster        |
|  - App Service      |     |  - API Services     |
|  - SQL Database     |     |  - Background Jobs  |
|  - Cosmos DB        |     |  - Web Frontend     |
|  - Redis Cache      |     |                     |
|                     |     |                     |
+----------+----------+     +----------+----------+
           |                            |
           v                            v
+----------+----------------------------+----------+
|                                                  |
|                 Azure Monitor                    |
|                                                  |
+--+-------------------+---------------------------+
   |                   |                           |
   v                   v                           v
+--+-------+     +-----+------+           +-------+------+
|          |     |            |           |              |
| Metrics  |     | Logs       |           | Application  |
| Database |     | Analytics  |           | Insights     |
|          |     |            |           |              |
+----------+     +-----+------+           +-------+------+
                       ^                          |
                       |                          |
                       |                          v
+----------------------+--+            +----------+----------+
|                         |            |                     |
|  Kubernetes Monitoring  |            |  Alerting System    |
|  - Prometheus           |            |  - Azure Alerts     |
|  - Grafana              +----------->+  - Action Groups    |
|  - Loki                 |            |  - Notification     |
|                         |            |                     |
+-------------------------+            +---------------------+
```

## Azure Monitor Setup

Azure Monitor provides the foundation for monitoring Azure resources and collecting application telemetry.

### Resource Provisioning

Deploy the Azure Monitor resources using the ARM template:

```bash
az deployment group create \
  --resource-group vatfilingpricingtool-monitoring \
  --template-file infrastructure/azure/arm-templates/monitor.json \
  --parameters @infrastructure/azure/arm-templates/parameters.json
```

This will create:
- Log Analytics Workspace
- Application Insights instance
- Alert rules and action groups

### Application Insights Configuration

1. Retrieve the Application Insights instrumentation key:

```bash
az monitor app-insights component show \
  --resource-group vatfilingpricingtool-monitoring \
  --query instrumentationKey \
  --output tsv \
  --name vatfilingpricingtool-appinsights
```

2. Configure the application to use Application Insights:
   - For backend services, add the instrumentation key to appsettings.json
   - For frontend applications, add the instrumentation key to the web configuration

### Log Analytics Configuration

1. Configure data collection rules to gather logs from various sources
2. Set up diagnostic settings for Azure resources to send logs to Log Analytics
3. Configure retention policies based on data type and compliance requirements

```bash
# Configure diagnostic settings for Azure SQL Database
az monitor diagnostic-settings create \
  --resource $(az sql server show -g vatfilingpricingtool-data -n vatfilingpricingtool-sql --query id -o tsv) \
  --name "SQLDiagnostics" \
  --workspace $(az monitor log-analytics workspace show -g vatfilingpricingtool-monitoring -n vatfilingpricingtool-logs --query id -o tsv) \
  --logs "[{\"category\":\"SQLSecurityAuditEvents\",\"enabled\":true}, {\"category\":\"SQLInsights\",\"enabled\":true}]" \
  --metrics "[{\"category\":\"Basic\",\"enabled\":true}]"
```

### Azure Monitor Alerts

Configure Azure Monitor alerts for critical infrastructure components:

1. Create action groups for different notification channels:

```bash
az monitor action-group create \
  --resource-group vatfilingpricingtool-monitoring \
  --name critical-alerts \
  --short-name CritAlert \
  --email-receiver name=oncall email=oncall@vatfilingpricingtool.com \
  --sms-receiver name=oncall countrycode=1 phoneNumber=5551234567
```

2. Create alert rules for key metrics:

```bash
az monitor metrics alert create \
  --resource-group vatfilingpricingtool-monitoring \
  --name HighCpuAlert \
  --scopes $(az webapp show -g vatfilingpricingtool-app -n vatfilingpricingtool-api --query id -o tsv) \
  --condition "avg Percentage CPU > 80" \
  --window-size 5m \
  --evaluation-frequency 1m \
  --action $(az monitor action-group show -g vatfilingpricingtool-monitoring -n critical-alerts --query id -o tsv)
```

## Kubernetes Monitoring Setup

For the Kubernetes-hosted components of the VAT Filing Pricing Tool, we deploy a monitoring stack consisting of Prometheus, Grafana, and Loki.

### Namespace Setup

Create a dedicated namespace for monitoring components:

```bash
kubectl apply -f infrastructure/kubernetes/namespace.yaml
```

### Prometheus Deployment

Deploy Prometheus for metrics collection:

```bash
kubectl apply -f infrastructure/kubernetes/monitoring/prometheus.yaml
```

This deploys:
- Prometheus server with persistent storage
- ConfigMap with scrape configuration
- ServiceAccount with appropriate RBAC permissions
- Service for accessing Prometheus API

The Prometheus configuration includes scrape configurations for:
- Kubernetes API server
- Kubernetes nodes
- Kubernetes pods with Prometheus annotations
- Service endpoints with Prometheus annotations
- Custom VAT Filing Pricing Tool metrics

### Grafana Deployment

Deploy Grafana for visualization:

```bash
kubectl apply -f infrastructure/kubernetes/monitoring/grafana.yaml
```

This deploys:
- Grafana server with persistent storage
- ConfigMaps for datasources and dashboards
- Service for accessing Grafana UI
- Ingress for external access

Grafana is configured with:
- Prometheus and Loki as data sources
- Pre-configured dashboards for system, application, and business metrics
- Azure AD integration for authentication
- Appropriate RBAC permissions

### Loki Deployment

Deploy Loki for log aggregation:

```bash
kubectl apply -f infrastructure/kubernetes/monitoring/loki.yaml
```

This deploys:
- Loki server with persistent storage
- Promtail DaemonSet for log collection
- ConfigMaps for Loki and Promtail configuration
- Services for accessing Loki API

Loki is configured to:
- Collect logs from all pods in the vatfilingpricingtool and monitoring namespaces
- Parse structured logs with timestamp and severity information
- Extract trace IDs for distributed tracing correlation

### Alert Rules Configuration

Deploy Prometheus alert rules:

```bash
kubectl apply -f infrastructure/kubernetes/monitoring/alert-rules.yaml
```

This ConfigMap contains alert rules for:
- System alerts (CPU, memory, disk usage)
- Kubernetes alerts (pod status, deployment status)
- Application alerts (API response time, error rates)
- Business alerts (calculation failures, integration failures)
- SLA compliance alerts

## Application Instrumentation

To ensure comprehensive monitoring, the VAT Filing Pricing Tool application components must be properly instrumented.

### Backend Services Instrumentation

1. Add Application Insights SDK to backend services:

```csharp
// In Program.cs or Startup.cs
services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:InstrumentationKey"]);

// Add custom telemetry
services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
```

2. Configure logging to send to Application Insights:

```json
{
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

3. Add custom metrics for business operations:

```csharp
private readonly TelemetryClient _telemetryClient;

public PricingService(TelemetryClient telemetryClient)
{
    _telemetryClient = telemetryClient;
}

public async Task<CalculationResult> CalculatePrice(CalculationRequest request)
{
    var stopwatch = Stopwatch.StartNew();
    var result = await PerformCalculation(request);
    stopwatch.Stop();
    
    // Track calculation metrics
    _telemetryClient.TrackMetric("CalculationDuration", stopwatch.ElapsedMilliseconds);
    _telemetryClient.TrackEvent("PriceCalculation", new Dictionary<string, string>
    {
        { "ServiceType", request.ServiceType.ToString() },
        { "CountryCount", request.Countries.Count.ToString() },
        { "TransactionVolume", request.TransactionVolume.ToString() }
    });
    
    return result;
}
```

### Frontend Instrumentation

1. Add Application Insights to Blazor WebAssembly application:

```html
<!-- In wwwroot/index.html -->
<script type="text/javascript">
    !function(T,l,y){/* Application Insights initialization code */}(window,document,{instrumentationKey:"YOUR_INSTRUMENTATION_KEY"});
</script>
```

2. Configure custom events for user interactions:

```javascript
// In interop.js
window.appInsights.trackEvent({name: "CalculationStarted"}, {
    serviceType: serviceType,
    countryCount: countries.length,
    transactionVolume: volume
});
```

3. Track page views and user sessions:

```csharp
// In App.razor.cs
protected override void OnInitialized()
{
    NavigationManager.LocationChanged += (sender, e) =>
    {
        JSRuntime.InvokeVoidAsync("appInsightsTrackPage", e.Location);
    };
}
```

### Distributed Tracing

Configure distributed tracing across services:

1. Add correlation headers to HTTP clients:

```csharp
public class CorrelationDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CorrelationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
        request.Headers.Add("x-correlation-id", correlationId);
        return base.SendAsync(request, cancellationToken);
    }
}
```

2. Configure W3C distributed tracing:

```csharp
services.AddApplicationInsightsTelemetry(options =>
{
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnableRequestTrackingTelemetryModule = true;
});
```

### Prometheus Metrics Exposure

Expose custom metrics for Prometheus scraping:

1. Add the Prometheus .NET Client library:

```bash
dotnet add package prometheus-net.AspNetCore
```

2. Configure metrics endpoint:

```csharp
// In Startup.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Other middleware configuration
    
    // Add Prometheus metrics endpoint
    app.UseMetricServer();
    app.UseHttpMetrics();
}
```

3. Define custom metrics:

```csharp
private static readonly Counter CalculationCounter = Metrics
    .CreateCounter("vatfilingpricingtool_calculations_total", "Total number of pricing calculations", 
        new CounterConfiguration
        {
            LabelNames = new[] { "service_type", "status" }
        });
        
private static readonly Histogram CalculationDuration = Metrics
    .CreateHistogram("vatfilingpricingtool_calculation_duration_seconds", 
        "Duration of pricing calculations in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "service_type", "complexity" },
            Buckets = new[] { 0.1, 0.5, 1, 2, 5, 10 }
        });
```

## Dashboard Configuration

The VAT Filing Pricing Tool uses multiple dashboards tailored to different stakeholders.

### Azure Portal Dashboards

Create custom Azure dashboards for high-level monitoring:

```bash
az portal dashboard create \
  --resource-group vatfilingpricingtool-monitoring \
  --name ExecutiveDashboard \
  --input-path infrastructure/azure/dashboards/executive-dashboard.json
```

Available dashboards:
- Executive Dashboard: High-level system health and business KPIs
- Operations Dashboard: Detailed system status and resource utilization
- Security Dashboard: Security posture and compliance status

### Grafana Dashboards

Deploy pre-configured Grafana dashboards:

```bash
kubectl apply -f infrastructure/kubernetes/monitoring/dashboards/
```

Available dashboards:
- System Dashboard: Infrastructure metrics (CPU, memory, disk, network)
- Application Dashboard: Service performance, error rates, and dependencies
- Business Dashboard: Business metrics and KPIs
- SLA Dashboard: Service level agreement compliance

Each dashboard includes:
- Real-time metrics visualization
- Historical trends
- Alert status
- Filtering by environment, service, and time range

### Custom Dashboard Creation

To create custom dashboards:

1. Use the Grafana UI to design the dashboard
2. Export the dashboard JSON
3. Add the JSON to the appropriate file in `infrastructure/kubernetes/monitoring/dashboards/`
4. Apply the changes with kubectl

Alternatively, use the Grafana API:

```bash
curl -X POST -H "Content-Type: application/json" \
  -d @custom-dashboard.json \
  http://admin:password@grafana-service:3000/api/dashboards/db
```

### Dashboard Access Control

Configure dashboard access based on user roles:

1. In Grafana, create teams for different user groups:
   - Administrators: Full access to all dashboards
   - Operators: Access to system and application dashboards
   - Business Users: Access to business dashboards only

2. Configure Azure AD integration to map groups to teams:

```ini
[auth.azure_ad]
enabled = true
allow_sign_up = true
auth_url = https://login.microsoftonline.com/${AZURE_TENANT_ID}/oauth2/v2.0/authorize
token_url = https://login.microsoftonline.com/${AZURE_TENANT_ID}/oauth2/v2.0/token
api_url = https://graph.microsoft.com/v1.0/me
client_id = ${AZURE_CLIENT_ID}
client_secret = ${AZURE_CLIENT_SECRET}
scopes = openid email profile
allowed_domains = vatfilingpricingtool.com
allowed_groups = Monitoring, DevOps, Administrators
```

## Alert Configuration

Configure comprehensive alerting to ensure timely response to system issues.

### Alert Severity Levels

The VAT Filing Pricing Tool uses four alert severity levels:

1. **Critical (P1)**: Severe production issues requiring immediate attention
   - Response Time: 15 minutes
   - Notification Channels: Email, SMS, Teams
   - Example Triggers: Service outage, data breach, critical SLA breach

2. **High (P2)**: Significant issues affecting functionality
   - Response Time: 1 hour
   - Notification Channels: Email, Teams
   - Example Triggers: Performance degradation, high error rates

3. **Medium (P3)**: Issues requiring attention but not affecting critical functionality
   - Response Time: 4 hours
   - Notification Channels: Email
   - Example Triggers: Resource utilization > 80%, slow response times

4. **Low (P4)**: Non-urgent issues
   - Response Time: 24 hours
   - Notification Channels: Email
   - Example Triggers: Non-critical warnings, cost thresholds

### Azure Monitor Alerts

Configure Azure Monitor alerts for infrastructure and application components:

1. Create action groups for different notification channels:

```bash
# Critical alerts (P1)
az monitor action-group create \
  --resource-group vatfilingpricingtool-monitoring \
  --name critical-alerts \
  --short-name CritAlert \
  --email-receiver name=oncall email=oncall@vatfilingpricingtool.com \
  --sms-receiver name=oncall countrycode=1 phoneNumber=5551234567 \
  --webhook-receiver name=teams uri=https://outlook.office.com/webhook/...

# High priority alerts (P2)
az monitor action-group create \
  --resource-group vatfilingpricingtool-monitoring \
  --name high-alerts \
  --short-name HighAlert \
  --email-receiver name=devteam email=devteam@vatfilingpricingtool.com \
  --webhook-receiver name=teams uri=https://outlook.office.com/webhook/...
```

2. Create alert rules for key metrics:

```bash
# Critical CPU alert
az monitor metrics alert create \
  --resource-group vatfilingpricingtool-monitoring \
  --name CriticalCpuAlert \
  --scopes $(az webapp show -g vatfilingpricingtool-app -n vatfilingpricingtool-api --query id -o tsv) \
  --condition "avg Percentage CPU > 95" \
  --window-size 5m \
  --evaluation-frequency 1m \
  --severity 0 \
  --action $(az monitor action-group show -g vatfilingpricingtool-monitoring -n critical-alerts --query id -o tsv)

# High error rate alert
az monitor metrics alert create \
  --resource-group vatfilingpricingtool-monitoring \
  --name HighErrorRateAlert \
  --scopes $(az resource show -g vatfilingpricingtool-monitoring -n vatfilingpricingtool-appinsights --resource-type microsoft.insights/components --query id -o tsv) \
  --condition "avg requests/failed > 5" \
  --window-size 5m \
  --evaluation-frequency 1m \
  --severity 1 \
  --action $(az monitor action-group show -g vatfilingpricingtool-monitoring -n high-alerts --query id -o tsv)
```

### Prometheus Alerting

Configure Prometheus alerting for Kubernetes components:

1. Deploy AlertManager:

```bash
kubectl apply -f infrastructure/kubernetes/monitoring/alertmanager.yaml
```

2. Configure alert rules in Prometheus:

```yaml
groups:
  - name: kubernetes_alerts
    rules:
      - alert: KubernetesPodCrashLooping
        expr: increase(kube_pod_container_status_restarts_total{namespace=~"vatfilingpricingtool|monitoring"}[1h]) > 5
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: Pod {{ $labels.pod }} in {{ $labels.namespace }} is crash looping
          description: Pod {{ $labels.pod }} in namespace {{ $labels.namespace }} has restarted more than 5 times in the last hour.
          runbook_url: https://wiki.vatfilingpricingtool.com/runbooks/pod-crash-looping
```

3. Configure AlertManager to send notifications to appropriate channels:

```yaml
receivers:
  - name: 'critical-alerts'
    webhook_configs:
      - url: 'http://alertwebhook:8080/critical'
        send_resolved: true
    email_configs:
      - to: 'oncall@vatfilingpricingtool.com'
        send_resolved: true

route:
  group_by: ['alertname', 'job']
  group_wait: 30s
  group_interval: 5m
  repeat_interval: 4h
  receiver: 'critical-alerts'
  routes:
  - match:
      severity: critical
    receiver: 'critical-alerts'
  - match:
      severity: warning
    receiver: 'warning-alerts'
```

### SLA Monitoring Alerts

Configure alerts for SLA compliance monitoring:

```yaml
groups:
  - name: sla_alerts
    rules:
      - alert: WebApplicationSLABreach
        expr: avg_over_time(probe_success{job="blackbox", target="https://vatfilingpricingtool.com"}[1h]) * 100 < 99.9
        for: 5m
        labels:
          severity: critical
          sla: "web-application"
        annotations:
          summary: Web Application SLA breach
          description: Web Application availability has dropped below 99.9% SLA in the last hour.
          runbook_url: https://wiki.vatfilingpricingtool.com/runbooks/web-application-sla-breach
```

## Log Management

Configure comprehensive log management for troubleshooting and analysis.

### Log Collection Strategy

The VAT Filing Pricing Tool collects logs from multiple sources:

1. **Application Logs**: Collected via Application Insights SDK
2. **Container Logs**: Collected via Loki/Promtail
3. **Infrastructure Logs**: Collected via Azure Monitor
4. **Database Logs**: Collected via diagnostic settings
5. **Audit Logs**: Collected via Azure Activity Log and custom audit logging

### Log Analytics Configuration

Configure Log Analytics for centralized log storage and analysis:

1. Set up data collection rules:

```bash
az monitor data-collection rule create \
  --resource-group vatfilingpricingtool-monitoring \
  --name app-logs \
  --data-flow destinations=vatfilingpricingtool-logs streams=Microsoft-ApplicationInsights-Log \
  --description "Application logs collection"
```

2. Configure diagnostic settings for Azure resources:

```bash
az monitor diagnostic-settings create \
  --resource $(az sql server show -g vatfilingpricingtool-data -n vatfilingpricingtool-sql --query id -o tsv) \
  --name "SQLDiagnostics" \
  --workspace $(az monitor log-analytics workspace show -g vatfilingpricingtool-monitoring -n vatfilingpricingtool-logs --query id -o tsv) \
  --logs "[{\"category\":\"SQLSecurityAuditEvents\",\"enabled\":true}, {\"category\":\"SQLInsights\",\"enabled\":true}]" \
  --metrics "[{\"category\":\"Basic\",\"enabled\":true}]"
```

3. Create saved queries for common analysis scenarios:

```bash
az monitor log-analytics saved-search create \
  --resource-group vatfilingpricingtool-monitoring \
  --workspace-name vatfilingpricingtool-logs \
  --name ApiErrorRates \
  --display-name "API Error Rates" \
  --category "VAT Filing Pricing Tool" \
  --query "AppRequests | where ResultCode >= 400 | summarize ErrorCount=count() by bin(TimeGenerated, 15m), ResultCode | render timechart"
```

### Loki Configuration

Configure Loki for container log collection:

1. Deploy Loki and Promtail:

```bash
kubectl apply -f infrastructure/kubernetes/monitoring/loki.yaml
```

2. Configure Promtail to collect logs from all pods:

```yaml
scrape_configs:
  - job_name: kubernetes-pods
    kubernetes_sd_configs:
      - role: pod
    relabel_configs:
      - source_labels: [__meta_kubernetes_pod_controller_name]
        regex: ([0-9a-z-.]+?)(-[0-9a-f]{8,10})?
        action: replace
        target_label: __tmp_controller_name
      - source_labels: [__meta_kubernetes_pod_label_app_kubernetes_io_name, __meta_kubernetes_pod_label_app, __tmp_controller_name, __meta_kubernetes_pod_name]
        regex: ^;*([^;]+)(;.*)?$
        action: replace
        target_label: app
      - source_labels: [__meta_kubernetes_pod_label_app_kubernetes_io_component, __meta_kubernetes_pod_label_component]
        regex: ^;*([^;]+)(;.*)?$
        action: replace
        target_label: component
      - action: replace
        source_labels:
        - __meta_kubernetes_pod_node_name
        target_label: node_name
      - action: replace
        source_labels:
        - __meta_kubernetes_namespace
        target_label: namespace
      - action: replace
        replacement: $1
        separator: /
        source_labels:
        - namespace
        - app
        target_label: job
      - action: replace
        source_labels:
        - __meta_kubernetes_pod_name
        target_label: pod
      - action: replace
        source_labels:
        - __meta_kubernetes_pod_container_name
        target_label: container
      - action: replace
        replacement: /var/log/pods/*$1/*.log
        separator: /
        source_labels:
        - __meta_kubernetes_pod_uid
        - __meta_kubernetes_pod_container_name
        target_label: __path__
      - action: replace
        regex: true
        source_labels:
        - __meta_kubernetes_pod_label_app_vatfilingpricingtool_com_logs
        target_label: __tmp_vatfilingpricingtool_logs
      - action: keep
        regex: true
        source_labels:
        - __tmp_vatfilingpricingtool_logs

pipeline_stages:
  - docker: {}
  - multiline:
      firstline: '^\\d{4}-\\d{2}-\\d{2}\\s\\d{2}:\\d{2}:\\d{2}'
  - regex:
      expression: '^(?P<time>\\d{4}-\\d{2}-\\d{2}\\s\\d{2}:\\d{2}:\\d{2},\\d{3})\\s+(?P<level>[A-Z]+)\\s+\\[(?P<service>\\S+)\\]\\s+\\[(?P<trace_id>\\S+)\\]\\s+(?P<message>.*)$'
  - labels:
      level:
      service:
      trace_id:
  - timestamp:
      format: '2006-01-02 15:04:05,000'
      source: time
```

3. Configure log parsing for structured logs:

```yaml
pipeline_stages:
  - docker: {}
  - multiline:
      firstline: '^\\d{4}-\\d{2}-\\d{2}\\s\\d{2}:\\d{2}:\\d{2}'
  - regex:
      expression: '^(?P<time>\\d{4}-\\d{2}-\\d{2}\\s\\d{2}:\\d{2}:\\d{2},\\d{3})\\s+(?P<level>[A-Z]+)\\s+\\[(?P<service>\\S+)\\]\\s+\\[(?P<trace_id>\\S+)\\]\\s+(?P<message>.*)$'
  - labels:
      level:
      service:
      trace_id:
  - timestamp:
      format: '2006-01-02 15:04:05,000'
      source: time
```

### Log Retention and Archiving

Configure log retention policies based on data type and compliance requirements:

1. Application Insights data retention:

```bash
az monitor app-insights component update \
  --resource-group vatfilingpricingtool-monitoring \
  --app vatfilingpricingtool-appinsights \
  --retention-time 90
```

2. Log Analytics data retention:

```bash
az monitor log-analytics workspace update \
  --resource-group vatfilingpricingtool-monitoring \
  --workspace-name vatfilingpricingtool-logs \
  --retention-time 90
```

3. Configure log archiving for long-term storage:

```bash
az storage container create \
  --account-name vatfilingpricingtoollogs \
  --name archived-logs \
  --auth-mode login

az monitor log-analytics workspace data-export create \
  --resource-group vatfilingpricingtool-monitoring \
  --workspace-name vatfilingpricingtool-logs \
  --name LogArchive \
  --destination $(az storage account show -g vatfilingpricingtool-monitoring -n vatfilingpricingtoollogs --query id -o tsv) \
  --destination-resource-id $(az storage account show -g vatfilingpricingtool-monitoring -n vatfilingpricingtoollogs --query id -o tsv) \
  --table-names SecurityEvent
```

## Monitoring Integration with CI/CD

Integrate monitoring with the CI/CD pipeline to ensure proper configuration and validation during deployments.

### Deployment Validation

Add monitoring validation steps to the deployment pipeline:

```yaml
# In .azuredevops/pipelines/deploy.yml
steps:
- task: AzureCLI@2
  displayName: 'Verify Monitoring Configuration'
  inputs:
    azureSubscription: 'Azure Subscription'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      # Verify Application Insights is receiving telemetry
      TELEMETRY_COUNT=$(az monitor app-insights query \
        --app $(appInsightsName) \
        --analytics-query "requests | where timestamp > ago(10m) | count" \
        --query "[0].Count" -o tsv)
      
      if [ "$TELEMETRY_COUNT" -eq "0" ]; then
        echo "##vso[task.logissue type=error]No telemetry received in Application Insights"
        exit 1
      fi
      
      # Verify alert rules are configured
      ALERT_COUNT=$(az monitor metrics alert list \
        --resource-group $(resourceGroup) \
        --query "length([?name.contains(@, '$(appServiceName)')])" -o tsv)
      
      if [ "$ALERT_COUNT" -eq "0" ]; then
        echo "##vso[task.logissue type=error]No alert rules configured for the application"
        exit 1
      fi