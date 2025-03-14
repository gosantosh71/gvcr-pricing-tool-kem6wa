# Get current Azure client configuration
data "azurerm_client_config" "current" {}

# Local variables for resource naming
locals {
  sql_server_name = "${var.resource_name_prefix}-sql"
  sql_database_name = "${var.resource_name_prefix}-db"
  cosmos_account_name = "${var.resource_name_prefix}-cosmos"
  redis_cache_name = "${var.resource_name_prefix}-redis"
  secondary_sql_server_name = "${var.resource_name_prefix}-sql-secondary"
  failover_group_name = "${var.resource_name_prefix}-failover-group"
}

# Generate a random password for SQL Server if not provided
resource "random_password" "random_password" {
  count = var.admin_password == null ? 1 : 0
  
  length = 16
  special = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

# Primary SQL Server for the VAT Filing Pricing Tool
resource "azurerm_mssql_server" "sql_server" {
  name                         = local.sql_server_name
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = var.admin_username
  administrator_login_password = var.admin_password != null ? var.admin_password : random_password.random_password[0].result
  minimum_tls_version          = "1.2"
  public_network_access_enabled = !var.enable_private_endpoints
  
  azuread_administrator {
    login_username = "AzureAD Admin"
    object_id      = data.azurerm_client_config.current.object_id
    tenant_id      = data.azurerm_client_config.current.tenant_id
  }
  
  identity {
    type = "SystemAssigned"
  }
  
  tags = var.tags
}

# SQL Database for the VAT Filing Pricing Tool
resource "azurerm_mssql_database" "sql_database" {
  name                = local.sql_database_name
  server_id           = azurerm_mssql_server.sql_server.id
  sku_name            = var.sql_sku
  max_size_gb         = 256
  zone_redundant      = true
  read_scale          = true
  auto_pause_delay_in_minutes = -1 # Disabled
  
  short_term_retention_policy {
    retention_days           = 35
    backup_interval_in_hours = 12
  }
  
  long_term_retention_policy {
    weekly_retention  = "P4W"
    monthly_retention = "P12M"
    yearly_retention  = "P7Y"
    week_of_year      = 1
  }
  
  tags = var.tags
}

# Allow Azure services to access the SQL Server
resource "azurerm_mssql_firewall_rule" "sql_server_firewall_rule_azure_services" {
  name      = "AllowAzureServices"
  server_id = azurerm_mssql_server.sql_server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Allow access from the application subnet
resource "azurerm_mssql_virtual_network_rule" "sql_server_vnet_rule" {
  name      = "app-subnet-access"
  server_id = azurerm_mssql_server.sql_server.id
  subnet_id = var.app_subnet_id
}

# Enable Transparent Data Encryption for SQL Server
resource "azurerm_mssql_server_transparent_data_encryption" "sql_tde" {
  server_id        = azurerm_mssql_server.sql_server.id
  key_vault_key_id = null # Use service-managed key
}

# Secondary SQL Server for disaster recovery
resource "azurerm_mssql_server" "secondary_sql_server" {
  name                         = local.secondary_sql_server_name
  resource_group_name          = var.secondary_resource_group_name
  location                     = var.secondary_location
  version                      = "12.0"
  administrator_login          = var.admin_username
  administrator_login_password = var.admin_password != null ? var.admin_password : random_password.random_password[0].result
  minimum_tls_version          = "1.2"
  public_network_access_enabled = !var.enable_private_endpoints
  
  azuread_administrator {
    login_username = "AzureAD Admin"
    object_id      = data.azurerm_client_config.current.object_id
    tenant_id      = data.azurerm_client_config.current.tenant_id
  }
  
  identity {
    type = "SystemAssigned"
  }
  
  tags = var.tags
}

# Failover group for SQL Server
resource "azurerm_mssql_failover_group" "sql_failover_group" {
  name                = local.failover_group_name
  server_id           = azurerm_mssql_server.sql_server.id
  databases           = [azurerm_mssql_database.sql_database.id]
  
  partner_server {
    id = azurerm_mssql_server.secondary_sql_server.id
  }
  
  read_write_endpoint_failover_policy {
    mode          = "Automatic"
    grace_minutes = 60
  }
  
  readonly_endpoint_failover_policy {
    mode = "Enabled"
  }
}

# Cosmos DB account for the VAT Filing Pricing Tool
resource "azurerm_cosmosdb_account" "cosmos_account" {
  name                = local.cosmos_account_name
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  enable_automatic_failover = true
  
  consistency_policy {
    consistency_level       = var.cosmos_db_consistency_level
    max_interval_in_seconds = 5
    max_staleness_prefix    = 100
  }
  
  geo_location {
    location          = var.location
    failover_priority = 0
  }
  
  geo_location {
    location          = var.secondary_location
    failover_priority = 1
  }
  
  capabilities {
    name = "EnableServerless"
  }
  
  public_network_access_enabled    = !var.enable_private_endpoints
  is_virtual_network_filter_enabled = true
  
  virtual_network_rule {
    id = var.app_subnet_id
  }
  
  virtual_network_rule {
    id = var.db_subnet_id
  }
  
  backup {
    type = "Continuous"
  }
  
  tags = var.tags
}

# Cosmos DB SQL database for the VAT Filing Pricing Tool
resource "azurerm_cosmosdb_sql_database" "cosmos_sql_database" {
  name                = "vatfilingpricingtool"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.cosmos_account.name
}

# Container for storing country-specific VAT rules
resource "azurerm_cosmosdb_sql_container" "cosmos_sql_container_rules" {
  name                = "rules"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.cosmos_account.name
  database_name       = azurerm_cosmosdb_sql_database.cosmos_sql_database.name
  partition_key_path  = "/countryCode"
  
  indexing_policy {
    indexing_mode = "consistent"
    
    included_path {
      path = "/countryCode/?"
    }
    
    included_path {
      path = "/ruleType/?"
    }
    
    included_path {
      path = "/effectiveFrom/?"
    }
    
    included_path {
      path = "/effectiveTo/?"
    }
    
    excluded_path {
      path = "/description/?"
    }
  }
  
  unique_key {
    paths = ["/ruleId"]
  }
}

# Container for storing audit logs
resource "azurerm_cosmosdb_sql_container" "cosmos_sql_container_audit" {
  name                = "auditLogs"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.cosmos_account.name
  database_name       = azurerm_cosmosdb_sql_database.cosmos_sql_database.name
  partition_key_path  = "/userId"
  default_ttl         = 31536000 # 1 year in seconds
  
  indexing_policy {
    indexing_mode = "consistent"
    
    included_path {
      path = "/userId/?"
    }
    
    included_path {
      path = "/eventType/?"
    }
    
    included_path {
      path = "/timestamp/?"
    }
    
    excluded_path {
      path = "/details/?"
    }
  }
}

# Container for storing application configuration
resource "azurerm_cosmosdb_sql_container" "cosmos_sql_container_config" {
  name                = "configurations"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.cosmos_account.name
  database_name       = azurerm_cosmosdb_sql_database.cosmos_sql_database.name
  partition_key_path  = "/configType"
  
  indexing_policy {
    indexing_mode = "consistent"
    
    included_path {
      path = "/configType/?"
    }
    
    included_path {
      path = "/name/?"
    }
    
    included_path {
      path = "/version/?"
    }
    
    excluded_path {
      path = "/value/?"
    }
  }
}

# Redis Cache for the VAT Filing Pricing Tool
resource "azurerm_redis_cache" "redis_cache" {
  name                = local.redis_cache_name
  location            = var.location
  resource_group_name = var.resource_group_name
  capacity            = var.redis_cache_sku.capacity
  family              = var.redis_cache_sku.family
  sku_name            = var.redis_cache_sku.name
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"
  public_network_access_enabled = !var.enable_private_endpoints
  
  redis_configuration {
    maxmemory_policy          = "volatile-lru"
    maxmemory_reserved        = 50
    maxfragmentationmemory_reserved = 50
    maxmemory_delta           = 50
  }
  
  subnet_id = var.enable_private_endpoints ? null : var.db_subnet_id
  redis_version      = 6
  replicas_per_master = 1
  zones              = ["1", "2", "3"]
  
  tags = var.tags
}

# Allow access from the application subnet (only if not using private endpoints)
resource "azurerm_redis_firewall_rule" "redis_firewall_rule" {
  count               = !var.enable_private_endpoints ? 1 : 0
  name                = "AllowAppSubnet"
  redis_cache_name    = azurerm_redis_cache.redis_cache.name
  resource_group_name = var.resource_group_name
  start_ip            = "0.0.0.0"
  end_ip              = "0.0.0.0"
}

# Private endpoint for SQL Server
resource "azurerm_private_endpoint" "sql_private_endpoint" {
  count               = var.enable_private_endpoints ? 1 : 0
  name                = "${local.sql_server_name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.db_subnet_id
  
  private_service_connection {
    name                           = "${local.sql_server_name}-privateserviceconnection"
    private_connection_resource_id = azurerm_mssql_server.sql_server.id
    is_manual_connection           = false
    subresource_names              = ["sqlServer"]
  }
  
  tags = var.tags
}

# Private endpoint for Cosmos DB
resource "azurerm_private_endpoint" "cosmos_private_endpoint" {
  count               = var.enable_private_endpoints ? 1 : 0
  name                = "${local.cosmos_account_name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.db_subnet_id
  
  private_service_connection {
    name                           = "${local.cosmos_account_name}-privateserviceconnection"
    private_connection_resource_id = azurerm_cosmosdb_account.cosmos_account.id
    is_manual_connection           = false
    subresource_names              = ["Sql"]
  }
  
  tags = var.tags
}

# Private endpoint for Redis Cache
resource "azurerm_private_endpoint" "redis_private_endpoint" {
  count               = var.enable_private_endpoints ? 1 : 0
  name                = "${local.redis_cache_name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.db_subnet_id
  
  private_service_connection {
    name                           = "${local.redis_cache_name}-privateserviceconnection"
    private_connection_resource_id = azurerm_redis_cache.redis_cache.id
    is_manual_connection           = false
    subresource_names              = ["redisCache"]
  }
  
  tags = var.tags
}

# Store SQL connection string in Key Vault
resource "azurerm_key_vault_secret" "sql_connection_string_secret" {
  name         = "SqlConnectionString"
  key_vault_id = var.key_vault_id
  value        = "Server=tcp:${azurerm_mssql_server.sql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.sql_database.name};Persist Security Info=False;User ID=${var.admin_username};Password=${var.admin_password != null ? var.admin_password : random_password.random_password[0].result};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}

# Store Cosmos DB connection string in Key Vault
resource "azurerm_key_vault_secret" "cosmos_connection_string_secret" {
  name         = "CosmosConnectionString"
  key_vault_id = var.key_vault_id
  value        = azurerm_cosmosdb_account.cosmos_account.primary_key
}

# Store Redis connection string in Key Vault
resource "azurerm_key_vault_secret" "redis_connection_string_secret" {
  name         = "RedisConnectionString"
  key_vault_id = var.key_vault_id
  value        = azurerm_redis_cache.redis_cache.primary_connection_string
}

# Diagnostic settings for SQL Database
resource "azurerm_monitor_diagnostic_setting" "sql_diagnostic_setting" {
  name                       = "sql-diagnostics"
  target_resource_id         = azurerm_mssql_database.sql_database.id
  log_analytics_workspace_id = var.log_analytics_workspace_id
  
  log {
    category = "SQLInsights"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "AutomaticTuning"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "QueryStoreRuntimeStatistics"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "Errors"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "DatabaseWaitStatistics"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "Timeouts"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "Blocks"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "Deadlocks"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  metric {
    category = "Basic"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  metric {
    category = "InstanceAndAppAdvanced"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  metric {
    category = "WorkloadManagement"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
}

# Diagnostic settings for Cosmos DB
resource "azurerm_monitor_diagnostic_setting" "cosmos_diagnostic_setting" {
  name                       = "cosmos-diagnostics"
  target_resource_id         = azurerm_cosmosdb_account.cosmos_account.id
  log_analytics_workspace_id = var.log_analytics_workspace_id
  
  log {
    category = "DataPlaneRequests"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "QueryRuntimeStatistics"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "PartitionKeyStatistics"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "ControlPlaneRequests"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  metric {
    category = "Requests"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
}

# Diagnostic settings for Redis Cache
resource "azurerm_monitor_diagnostic_setting" "redis_diagnostic_setting" {
  name                       = "redis-diagnostics"
  target_resource_id         = azurerm_redis_cache.redis_cache.id
  log_analytics_workspace_id = var.log_analytics_workspace_id
  
  log {
    category = "ConnectedClientList"
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

variable "resource_group_name" {
  type        = string
  description = "The name of the resource group where the database resources will be deployed"
}

variable "location" {
  type        = string
  description = "The primary Azure region where the database resources will be deployed"
}

variable "secondary_location" {
  type        = string
  description = "The secondary Azure region for disaster recovery"
}

variable "secondary_resource_group_name" {
  type        = string
  description = "The name of the secondary resource group for disaster recovery"
}

variable "resource_name_prefix" {
  type        = string
  description = "Prefix for resource names"
}

variable "app_subnet_id" {
  type        = string
  description = "The ID of the subnet for App Service"
}

variable "db_subnet_id" {
  type        = string
  description = "The ID of the subnet for database resources"
}

variable "key_vault_id" {
  type        = string
  description = "The ID of the Key Vault for storing database credentials"
}

variable "log_analytics_workspace_id" {
  type        = string
  description = "The ID of the Log Analytics workspace for diagnostic settings"
}

variable "sql_sku" {
  type        = string
  description = "The SKU for Azure SQL Database"
  default     = "BusinessCritical_Gen5_8"
}

variable "cosmos_db_consistency_level" {
  type        = string
  description = "The consistency level for Cosmos DB"
  default     = "Session"
}

variable "redis_cache_sku" {
  type        = object({
    name     = string
    family   = string
    capacity = number
  })
  description = "The SKU for Redis Cache"
  default     = {
    name     = "Premium"
    family   = "P"
    capacity = 1
  }
}

variable "enable_private_endpoints" {
  type        = bool
  description = "Whether to create private endpoints for database services"
  default     = false
}

variable "admin_username" {
  type        = string
  description = "The admin username for SQL Server"
  sensitive   = true
}

variable "admin_password" {
  type        = string
  description = "The admin password for SQL Server"
  default     = null
  sensitive   = true
}

variable "tags" {
  type        = map(string)
  description = "Tags to apply to all resources"
  default     = {}
}

output "sql_server_name" {
  value       = azurerm_mssql_server.sql_server.name
  description = "Name of the primary SQL Server"
}

output "sql_database_name" {
  value       = azurerm_mssql_database.sql_database.name
  description = "Name of the SQL Database"
}

output "cosmos_account_name" {
  value       = azurerm_cosmosdb_account.cosmos_account.name
  description = "Name of the Cosmos DB account"
}

output "redis_cache_name" {
  value       = azurerm_redis_cache.redis_cache.name
  description = "Name of the Redis Cache"
}

output "sql_connection_string" {
  value       = "Server=tcp:${azurerm_mssql_server.sql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.sql_database.name};Persist Security Info=False;User ID=${var.admin_username};Password=${var.admin_password != null ? var.admin_password : random_password.random_password[0].result};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  description = "Connection string for the SQL Database"
  sensitive   = true
}

output "cosmos_connection_string" {
  value       = azurerm_cosmosdb_account.cosmos_account.primary_key
  description = "Connection string for the Cosmos DB account"
  sensitive   = true
}

output "redis_connection_string" {
  value       = azurerm_redis_cache.redis_cache.primary_connection_string
  description = "Connection string for the Redis Cache"
  sensitive   = true
}