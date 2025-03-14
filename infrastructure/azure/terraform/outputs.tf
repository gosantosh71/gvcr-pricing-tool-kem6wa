# Resource Groups
output "resource_group_name" {
  value       = azurerm_resource_group.resource_group.name
  description = "The name of the main resource group"
}

output "secondary_resource_group_name" {
  value       = azurerm_resource_group.secondary_resource_group.name
  description = "The name of the secondary resource group for disaster recovery"
}

# Locations
output "primary_location" {
  value       = azurerm_resource_group.resource_group.location
  description = "The primary Azure region where resources are deployed"
}

output "secondary_location" {
  value       = azurerm_resource_group.secondary_resource_group.location
  description = "The secondary Azure region for disaster recovery"
}

# Web Apps
output "web_app_name" {
  value       = module.compute.web_app_name
  description = "The name of the web application"
}

output "web_app_url" {
  value       = module.compute.web_app_url
  description = "The URL of the web application"
}

output "api_app_name" {
  value       = module.compute.api_app_name
  description = "The name of the API application"
}

output "api_app_url" {
  value       = module.compute.api_app_url
  description = "The URL of the API application"
}

# AKS
output "aks_cluster_name" {
  value       = module.compute.aks_cluster_name
  description = "The name of the AKS cluster"
}

# SQL Server
output "sql_server_name" {
  value       = module.database.sql_server_name
  description = "The name of the SQL Server"
}

output "sql_database_name" {
  value       = module.database.sql_database_name
  description = "The name of the SQL Database"
}

output "sql_failover_group_name" {
  value       = module.database.sql_failover_group_name
  description = "The name of the SQL Failover Group"
}

# Cosmos DB
output "cosmos_account_name" {
  value       = module.database.cosmos_account_name
  description = "The name of the Cosmos DB account"
}

# Redis Cache
output "redis_cache_name" {
  value       = module.database.redis_cache_name
  description = "The name of the Redis Cache"
}

# Key Vault
output "key_vault_name" {
  value       = module.security.key_vault_name
  description = "The name of the Key Vault"
}

# Storage Account
output "storage_account_name" {
  value       = module.security.storage_account_name
  description = "The name of the Storage Account"
}

# Front Door
output "front_door_name" {
  value       = module.networking.front_door_name
  description = "The name of the Front Door"
}

output "front_door_endpoint" {
  value       = module.networking.front_door_endpoint
  description = "The endpoint URL of the Front Door"
}

# API Management
output "api_management_name" {
  value       = module.networking.api_management_name
  description = "The name of the API Management"
}

output "api_management_gateway_url" {
  value       = module.networking.api_management_gateway_url
  description = "The gateway URL of the API Management"
}

# Application Insights
output "application_insights_name" {
  value       = azurerm_application_insights.application_insights.name
  description = "The name of the Application Insights"
}

output "application_insights_instrumentation_key" {
  value       = azurerm_application_insights.application_insights.instrumentation_key
  description = "The instrumentation key for Application Insights"
  sensitive   = true
}

output "application_insights_connection_string" {
  value       = azurerm_application_insights.application_insights.connection_string
  description = "The connection string for Application Insights"
  sensitive   = true
}

# Connection Strings
output "sql_connection_string" {
  value       = module.database.sql_connection_string
  description = "The connection string for the SQL Database"
  sensitive   = true
}

output "cosmos_connection_string" {
  value       = module.database.cosmos_connection_string
  description = "The connection string for the Cosmos DB account"
  sensitive   = true
}

output "redis_connection_string" {
  value       = module.database.redis_connection_string
  description = "The connection string for the Redis Cache"
  sensitive   = true
}

# Secondary Region Resources
output "secondary_web_app_name" {
  value       = module.compute.secondary_web_app_name
  description = "The name of the secondary Web App for disaster recovery"
}

output "secondary_api_app_name" {
  value       = module.compute.secondary_api_app_name
  description = "The name of the secondary API App for disaster recovery"
}

output "secondary_key_vault_name" {
  value       = module.security.secondary_key_vault_name
  description = "The name of the secondary Key Vault for disaster recovery"
}

output "secondary_storage_account_name" {
  value       = module.security.secondary_storage_account_name
  description = "The name of the secondary Storage Account for disaster recovery"
}