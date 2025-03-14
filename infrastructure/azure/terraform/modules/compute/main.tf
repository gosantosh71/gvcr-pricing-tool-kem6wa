# Azure provider configuration required for this module.
# Provider version: hashicorp/azurerm ~> 3.0
# Random provider for generating unique identifiers: hashicorp/random ~> 3.1

# Local variables for resource naming
locals {
  app_service_plan_name = "${var.resource_name_prefix}-asp"
  web_app_name = "${var.resource_name_prefix}-web"
  api_app_name = "${var.resource_name_prefix}-api"
  aks_cluster_name = "${var.resource_name_prefix}-aks"
  secondary_app_service_plan_name = "${var.resource_name_prefix}-asp-secondary"
  secondary_web_app_name = "${var.resource_name_prefix}-web-secondary"
  secondary_api_app_name = "${var.resource_name_prefix}-api-secondary"
  acr_name = "${replace(lower(var.resource_name_prefix), "-", "")}acr"
}

# Data sources to reference existing resources
data "azurerm_key_vault" "key_vault" {
  name                = split("/", var.key_vault_id)[8]
  resource_group_name = var.resource_group_name
}

data "azurerm_log_analytics_workspace" "log_analytics_workspace" {
  name                = "law-${var.resource_name_prefix}"
  resource_group_name = var.resource_group_name
}

# App Service Plan in the primary region
resource "azurerm_service_plan" "app_service_plan" {
  name                = local.app_service_plan_name
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = "${var.app_service_sku.tier}_${var.app_service_sku.size}"
  zone_balancing_enabled = true
  tags                = var.tags
}

# Web App for the VAT Filing Pricing Tool frontend in the primary region
resource "azurerm_linux_web_app" "web_app" {
  name                = local.web_app_name
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.app_service_plan.id
  https_only          = true
  virtual_network_subnet_id = var.app_subnet_id

  site_config {
    always_on          = true
    minimum_tls_version = "1.2"
    health_check_path  = "/health"
    application_stack {
      docker_image     = "${azurerm_container_registry.container_registry.login_server}/vatfilingpricingtool/web"
      docker_image_tag = "latest"
    }
    cors {
      allowed_origins  = ["https://${local.api_app_name}.azurewebsites.net"]
    }
    ip_restriction    = []
  }

  app_settings = {
    DOCKER_REGISTRY_SERVER_URL      = "https://${azurerm_container_registry.container_registry.login_server}"
    DOCKER_REGISTRY_SERVER_USERNAME = azurerm_container_registry.container_registry.admin_username
    DOCKER_REGISTRY_SERVER_PASSWORD = azurerm_container_registry.container_registry.admin_password
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    APPLICATIONINSIGHTS_CONNECTION_STRING = var.application_insights_connection_string
    API_BASE_URL                    = "https://${local.api_app_name}.azurewebsites.net"
    AZURE_AD_TENANT_ID              = "common"
    AZURE_AD_CLIENT_ID              = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.web_client_id_secret.id})"
    AZURE_AD_SCOPE                  = "api://vatfilingpricingtool/user_impersonation"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Web App for the VAT Filing Pricing Tool API in the primary region
resource "azurerm_linux_web_app" "api_app" {
  name                = local.api_app_name
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.app_service_plan.id
  https_only          = true
  virtual_network_subnet_id = var.app_subnet_id

  site_config {
    always_on          = true
    minimum_tls_version = "1.2"
    health_check_path  = "/health"
    application_stack {
      docker_image     = "${azurerm_container_registry.container_registry.login_server}/vatfilingpricingtool/api"
      docker_image_tag = "latest"
    }
    cors {
      allowed_origins  = ["https://${local.web_app_name}.azurewebsites.net"]
    }
    ip_restriction    = []
  }

  app_settings = {
    DOCKER_REGISTRY_SERVER_URL      = "https://${azurerm_container_registry.container_registry.login_server}"
    DOCKER_REGISTRY_SERVER_USERNAME = azurerm_container_registry.container_registry.admin_username
    DOCKER_REGISTRY_SERVER_PASSWORD = azurerm_container_registry.container_registry.admin_password
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    APPLICATIONINSIGHTS_CONNECTION_STRING = var.application_insights_connection_string
    KeyVault__Endpoint             = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.key_vault_endpoint_secret.id})"
    ConnectionStrings__SqlDatabase = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.sql_connection_string_secret.id})"
    ConnectionStrings__CosmosDb    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.cosmos_connection_string_secret.id})"
    ConnectionStrings__RedisCache  = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.redis_connection_string_secret.id})"
    Storage__ConnectionString      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.storage_connection_string_secret.id})"
    Storage__ReportsContainer      = "reports"
    Storage__TemplatesContainer    = "templates"
    Storage__DocumentsContainer    = "documents"
    AzureAd__Instance              = "https://login.microsoftonline.com/"
    AzureAd__TenantId              = "common"
    AzureAd__ClientId              = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.api_client_id_secret.id})"
    AzureAd__Audience              = "api://vatfilingpricingtool"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# App Service Plan in the secondary region for disaster recovery
resource "azurerm_service_plan" "secondary_app_service_plan" {
  name                = local.secondary_app_service_plan_name
  location            = var.secondary_location
  resource_group_name = var.secondary_resource_group_name
  os_type             = "Linux"
  sku_name            = "${var.app_service_sku.tier}_${var.app_service_sku.size}"
  zone_balancing_enabled = true
  tags                = var.tags
}

# Web App for the VAT Filing Pricing Tool frontend in the secondary region for disaster recovery
resource "azurerm_linux_web_app" "secondary_web_app" {
  name                = local.secondary_web_app_name
  location            = var.secondary_location
  resource_group_name = var.secondary_resource_group_name
  service_plan_id     = azurerm_service_plan.secondary_app_service_plan.id
  https_only          = true

  site_config {
    always_on          = true
    minimum_tls_version = "1.2"
    health_check_path  = "/health"
    application_stack {
      docker_image     = "${azurerm_container_registry.container_registry.login_server}/vatfilingpricingtool/web"
      docker_image_tag = "latest"
    }
    cors {
      allowed_origins  = ["https://${local.secondary_api_app_name}.azurewebsites.net"]
    }
    ip_restriction    = []
  }

  app_settings = {
    DOCKER_REGISTRY_SERVER_URL      = "https://${azurerm_container_registry.container_registry.login_server}"
    DOCKER_REGISTRY_SERVER_USERNAME = azurerm_container_registry.container_registry.admin_username
    DOCKER_REGISTRY_SERVER_PASSWORD = azurerm_container_registry.container_registry.admin_password
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    APPLICATIONINSIGHTS_CONNECTION_STRING = var.application_insights_connection_string
    API_BASE_URL                    = "https://${local.secondary_api_app_name}.azurewebsites.net"
    AZURE_AD_TENANT_ID              = "common"
    AZURE_AD_CLIENT_ID              = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.web_client_id_secret.id})"
    AZURE_AD_SCOPE                  = "api://vatfilingpricingtool/user_impersonation"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Web App for the VAT Filing Pricing Tool API in the secondary region for disaster recovery
resource "azurerm_linux_web_app" "secondary_api_app" {
  name                = local.secondary_api_app_name
  location            = var.secondary_location
  resource_group_name = var.secondary_resource_group_name
  service_plan_id     = azurerm_service_plan.secondary_app_service_plan.id
  https_only          = true

  site_config {
    always_on          = true
    minimum_tls_version = "1.2"
    health_check_path  = "/health"
    application_stack {
      docker_image     = "${azurerm_container_registry.container_registry.login_server}/vatfilingpricingtool/api"
      docker_image_tag = "latest"
    }
    cors {
      allowed_origins  = ["https://${local.secondary_web_app_name}.azurewebsites.net"]
    }
    ip_restriction    = []
  }

  app_settings = {
    DOCKER_REGISTRY_SERVER_URL      = "https://${azurerm_container_registry.container_registry.login_server}"
    DOCKER_REGISTRY_SERVER_USERNAME = azurerm_container_registry.container_registry.admin_username
    DOCKER_REGISTRY_SERVER_PASSWORD = azurerm_container_registry.container_registry.admin_password
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    APPLICATIONINSIGHTS_CONNECTION_STRING = var.application_insights_connection_string
    KeyVault__Endpoint             = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.key_vault_endpoint_secret.id})"
    ConnectionStrings__SqlDatabase = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.sql_connection_string_secret.id})"
    ConnectionStrings__CosmosDb    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.cosmos_connection_string_secret.id})"
    ConnectionStrings__RedisCache  = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.redis_connection_string_secret.id})"
    Storage__ConnectionString      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.storage_connection_string_secret.id})"
    Storage__ReportsContainer      = "reports"
    Storage__TemplatesContainer    = "templates"
    Storage__DocumentsContainer    = "documents"
    AzureAd__Instance              = "https://login.microsoftonline.com/"
    AzureAd__TenantId              = "common"
    AzureAd__ClientId              = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.api_client_id_secret.id})"
    AzureAd__Audience              = "api://vatfilingpricingtool"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Azure Container Registry for storing Docker images
resource "azurerm_container_registry" "container_registry" {
  name                = local.acr_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "Premium"
  admin_enabled       = true
  
  georeplications {
    location                = var.secondary_location
    zone_redundancy_enabled = true
  }
  
  tags = var.tags
}

# AKS cluster for running containerized microservices
resource "azurerm_kubernetes_cluster" "aks_cluster" {
  name                = local.aks_cluster_name
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = var.resource_name_prefix
  kubernetes_version  = var.kubernetes_version
  node_resource_group = "${var.resource_group_name}-aks-nodes"

  default_node_pool {
    name                = "default"
    node_count          = var.enable_auto_scaling ? null : var.aks_node_count
    vm_size             = var.aks_vm_size
    vnet_subnet_id      = var.aks_subnet_id
    enable_auto_scaling = var.enable_auto_scaling
    min_count           = var.enable_auto_scaling ? var.aks_min_node_count : null
    max_count           = var.enable_auto_scaling ? var.aks_max_node_count : null
    max_pods            = 30
    os_disk_size_gb     = 128
    type                = "VirtualMachineScaleSets"
    zones               = var.enable_availability_zones ? [1, 2, 3] : null
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin     = "azure"
    network_policy     = "azure"
    load_balancer_sku  = "standard"
    service_cidr       = "10.0.0.0/16"
    dns_service_ip     = "10.0.0.10"
    docker_bridge_cidr = "172.17.0.1/16"
  }

  role_based_access_control_enabled = true
  
  azure_active_directory_role_based_access_control {
    managed                = true
    admin_group_object_ids = []
    azure_rbac_enabled     = true
  }

  oms_agent {
    log_analytics_workspace_id = var.log_analytics_workspace_id
  }

  tags = var.tags
}

# Grant AKS cluster access to ACR
resource "azurerm_role_assignment" "aks_acr_role_assignment" {
  principal_id                     = azurerm_kubernetes_cluster.aks_cluster.kubelet_identity[0].object_id
  role_definition_name             = "AcrPull"
  scope                            = azurerm_container_registry.container_registry.id
}

# Grant Web App access to Key Vault secrets
resource "azurerm_key_vault_access_policy" "web_app_key_vault_access_policy" {
  key_vault_id = var.key_vault_id
  tenant_id    = azurerm_linux_web_app.web_app.identity[0].tenant_id
  object_id    = azurerm_linux_web_app.web_app.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# Grant API App access to Key Vault secrets
resource "azurerm_key_vault_access_policy" "api_app_key_vault_access_policy" {
  key_vault_id = var.key_vault_id
  tenant_id    = azurerm_linux_web_app.api_app.identity[0].tenant_id
  object_id    = azurerm_linux_web_app.api_app.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# Grant Secondary Web App access to Key Vault secrets
resource "azurerm_key_vault_access_policy" "secondary_web_app_key_vault_access_policy" {
  key_vault_id = var.key_vault_id
  tenant_id    = azurerm_linux_web_app.secondary_web_app.identity[0].tenant_id
  object_id    = azurerm_linux_web_app.secondary_web_app.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# Grant Secondary API App access to Key Vault secrets
resource "azurerm_key_vault_access_policy" "secondary_api_app_key_vault_access_policy" {
  key_vault_id = var.key_vault_id
  tenant_id    = azurerm_linux_web_app.secondary_api_app.identity[0].tenant_id
  object_id    = azurerm_linux_web_app.secondary_api_app.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# Store Key Vault endpoint in Key Vault
resource "azurerm_key_vault_secret" "key_vault_endpoint_secret" {
  name         = "KeyVaultEndpoint"
  key_vault_id = var.key_vault_id
  value        = data.azurerm_key_vault.key_vault.vault_uri
}

# Store Web App client ID in Key Vault
resource "azurerm_key_vault_secret" "web_client_id_secret" {
  name         = "WebClientId"
  key_vault_id = var.key_vault_id
  value        = "00000000-0000-0000-0000-000000000000"  # Placeholder, should be replaced with actual value
}

# Store API App client ID in Key Vault
resource "azurerm_key_vault_secret" "api_client_id_secret" {
  name         = "ApiClientId"
  key_vault_id = var.key_vault_id
  value        = "00000000-0000-0000-0000-000000000000"  # Placeholder, should be replaced with actual value
}

# Store SQL connection string in Key Vault
resource "azurerm_key_vault_secret" "sql_connection_string_secret" {
  name         = "SqlConnectionString"
  key_vault_id = var.key_vault_id
  value        = "Server=sql-server.database.windows.net;Database=vatfilingpricingtool;User Id=admin;Password=password;"  # Placeholder
}

# Store Cosmos DB connection string in Key Vault
resource "azurerm_key_vault_secret" "cosmos_connection_string_secret" {
  name         = "CosmosConnectionString"
  key_vault_id = var.key_vault_id
  value        = "AccountEndpoint=https://cosmos-account.documents.azure.com:443/;AccountKey=key;"  # Placeholder
}

# Store Redis connection string in Key Vault
resource "azurerm_key_vault_secret" "redis_connection_string_secret" {
  name         = "RedisConnectionString"
  key_vault_id = var.key_vault_id
  value        = "redis-cache.redis.cache.windows.net:6380,password=password,ssl=True,abortConnect=False"  # Placeholder
}

# Store Storage connection string in Key Vault
resource "azurerm_key_vault_secret" "storage_connection_string_secret" {
  name         = "StorageConnectionString"
  key_vault_id = var.key_vault_id
  value        = "DefaultEndpointsProtocol=https;AccountName=storageaccount;AccountKey=key;EndpointSuffix=core.windows.net"  # Placeholder
}

# Diagnostic settings for Web App
resource "azurerm_monitor_diagnostic_setting" "web_app_diagnostic_setting" {
  name                       = "web-app-diagnostics"
  target_resource_id         = azurerm_linux_web_app.web_app.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "AppServiceHTTPLogs"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "AppServiceConsoleLogs"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "AppServiceAppLogs"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "AppServiceAuditLogs"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  metric {
    category = "AllMetrics"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }
}

# Diagnostic settings for API App
resource "azurerm_monitor_diagnostic_setting" "api_app_diagnostic_setting" {
  name                       = "api-app-diagnostics"
  target_resource_id         = azurerm_linux_web_app.api_app.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "AppServiceHTTPLogs"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "AppServiceConsoleLogs"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "AppServiceAppLogs"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "AppServiceAuditLogs"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  metric {
    category = "AllMetrics"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }
}

# Diagnostic settings for AKS cluster
resource "azurerm_monitor_diagnostic_setting" "aks_diagnostic_setting" {
  name                       = "aks-diagnostics"
  target_resource_id         = azurerm_kubernetes_cluster.aks_cluster.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "kube-apiserver"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "kube-controller-manager"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "kube-scheduler"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "kube-audit"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }

  metric {
    category = "AllMetrics"
    enabled  = true
    retention_policy {
      enabled = true
      days    = 30
    }
  }
}

# Output values
output "app_service_plan_id" {
  value       = azurerm_service_plan.app_service_plan.id
  description = "ID of the primary App Service Plan"
}

output "web_app_name" {
  value       = azurerm_linux_web_app.web_app.name
  description = "Name of the Web App for the VAT Filing Pricing Tool"
}

output "web_app_url" {
  value       = "https://${azurerm_linux_web_app.web_app.default_hostname}"
  description = "Default URL of the Web App"
}

output "api_app_name" {
  value       = azurerm_linux_web_app.api_app.name
  description = "Name of the API App for the VAT Filing Pricing Tool"
}

output "api_app_url" {
  value       = "https://${azurerm_linux_web_app.api_app.default_hostname}"
  description = "Default URL of the API App"
}

output "aks_cluster_name" {
  value       = azurerm_kubernetes_cluster.aks_cluster.name
  description = "Name of the AKS cluster"
}

output "aks_cluster_id" {
  value       = azurerm_kubernetes_cluster.aks_cluster.id
  description = "ID of the AKS cluster"
}

output "aks_kube_config" {
  value       = azurerm_kubernetes_cluster.aks_cluster.kube_config_raw
  description = "Raw Kubernetes configuration for the AKS cluster"
  sensitive   = true
}

output "secondary_app_service_plan_id" {
  value       = azurerm_service_plan.secondary_app_service_plan.id
  description = "ID of the secondary App Service Plan for disaster recovery"
}

output "secondary_web_app_name" {
  value       = azurerm_linux_web_app.secondary_web_app.name
  description = "Name of the secondary Web App for disaster recovery"
}

output "secondary_api_app_name" {
  value       = azurerm_linux_web_app.secondary_api_app.name
  description = "Name of the secondary API App for disaster recovery"
}

output "container_registry_name" {
  value       = azurerm_container_registry.container_registry.name
  description = "Name of the Azure Container Registry"
}

output "container_registry_login_server" {
  value       = azurerm_container_registry.container_registry.login_server
  description = "Login server for the Azure Container Registry"
}