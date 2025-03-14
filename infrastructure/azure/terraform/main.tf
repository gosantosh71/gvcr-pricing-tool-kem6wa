# Main Terraform configuration for the VAT Filing Pricing Tool Infrastructure
# This file orchestrates the deployment of all Azure resources required for the application.

# Configure Terraform providers
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.1"
    }
  }
}

# Configure the Azure Provider
provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = false
      recover_soft_deleted_key_vaults = true
    }
    resource_group {
      prevent_deletion_if_contains_resources = true
    }
  }
}

# Define local values for resource naming and tagging
locals {
  resource_name_prefix = "${var.app_name}-${var.environment}"
  primary_resource_group_name = "rg-${local.resource_name_prefix}"
  secondary_resource_group_name = "rg-${local.resource_name_prefix}-dr"
  common_tags = {
    Project = "VAT Filing Pricing Tool"
    Environment = "${var.environment}"
    ManagedBy = "Terraform"
  }
}

# Create the primary resource group
resource "azurerm_resource_group" "resource_group" {
  name     = local.primary_resource_group_name
  location = var.location
  tags     = merge(var.tags, local.common_tags)
}

# Create the secondary resource group for disaster recovery
resource "azurerm_resource_group" "secondary_resource_group" {
  name     = local.secondary_resource_group_name
  location = var.secondary_location
  tags     = merge(var.tags, local.common_tags)
}

# Create Log Analytics Workspace for monitoring
resource "azurerm_log_analytics_workspace" "log_analytics_workspace" {
  name                = "law-${local.resource_name_prefix}"
  location            = var.location
  resource_group_name = azurerm_resource_group.resource_group.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = merge(var.tags, local.common_tags)
}

# Create Application Insights for application monitoring
resource "azurerm_application_insights" "application_insights" {
  name                = "ai-${local.resource_name_prefix}"
  location            = var.location
  resource_group_name = azurerm_resource_group.resource_group.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.log_analytics_workspace.id
  tags                = merge(var.tags, local.common_tags)
}

# Deploy networking resources
module "networking" {
  source = "./modules/networking"
  
  resource_group_name = azurerm_resource_group.resource_group.name
  location = var.location
  secondary_location = var.secondary_location
  secondary_resource_group_name = azurerm_resource_group.secondary_resource_group.name
  resource_name_prefix = local.resource_name_prefix
  
  vnet_address_space = var.vnet_address_space
  app_subnet_prefix = var.app_subnet_prefix
  aks_subnet_prefix = var.aks_subnet_prefix
  db_subnet_prefix = var.db_subnet_prefix
  
  api_management_sku = var.api_management_sku
  api_management_capacity = 1
  enable_waf = true
  waf_mode = "Detection"
  enable_private_endpoints = false
  
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics_workspace.id
  
  tags = merge(var.tags, local.common_tags)
}

# Deploy security resources
module "security" {
  source = "./modules/security"
  
  resource_group_name = azurerm_resource_group.resource_group.name
  location = var.location
  secondary_location = var.secondary_location
  secondary_resource_group_name = azurerm_resource_group.secondary_resource_group.name
  resource_name_prefix = local.resource_name_prefix
  
  storage_account_tier = var.storage_account_tier
  storage_account_replication_type = var.storage_account_replication_type
  key_vault_sku = "Standard"
  enable_purge_protection = true
  soft_delete_retention_days = 90
  
  network_acls = {
    default_action = "Allow"
    bypass = "AzureServices"
    ip_rules = []
    virtual_network_subnet_ids = []
  }
  
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics_workspace.id
  
  tags = merge(var.tags, local.common_tags)
}

# Deploy database resources
module "database" {
  source = "./modules/database"
  
  resource_group_name = azurerm_resource_group.resource_group.name
  location = var.location
  secondary_location = var.secondary_location
  secondary_resource_group_name = azurerm_resource_group.secondary_resource_group.name
  resource_name_prefix = local.resource_name_prefix
  
  app_subnet_id = module.networking.app_subnet_id
  db_subnet_id = module.networking.db_subnet_id
  key_vault_id = module.security.key_vault_id
  
  sql_sku = var.sql_sku
  cosmos_db_consistency_level = var.cosmos_db_consistency_level
  redis_cache_sku = {
    name = "Premium"
    family = "P"
    capacity = 1
  }
  
  enable_private_endpoints = false
  
  admin_username = var.admin_username
  admin_password = var.admin_password
  
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics_workspace.id
  
  tags = merge(var.tags, local.common_tags)
  
  depends_on = [
    module.networking,
    module.security
  ]
}

# Deploy compute resources
module "compute" {
  source = "./modules/compute"
  
  resource_group_name = azurerm_resource_group.resource_group.name
  location = var.location
  secondary_location = var.secondary_location
  secondary_resource_group_name = azurerm_resource_group.secondary_resource_group.name
  resource_name_prefix = local.resource_name_prefix
  
  app_subnet_id = module.networking.app_subnet_id
  aks_subnet_id = module.networking.aks_subnet_id
  key_vault_id = module.security.key_vault_id
  storage_account_id = module.security.storage_account_id
  
  application_insights_instrumentation_key = azurerm_application_insights.application_insights.instrumentation_key
  application_insights_connection_string = azurerm_application_insights.application_insights.connection_string
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics_workspace.id
  
  app_service_sku = var.app_service_sku
  aks_node_count = var.aks_node_count
  aks_min_node_count = 3
  aks_max_node_count = 10
  aks_vm_size = var.aks_vm_size
  kubernetes_version = "1.24.9"
  enable_auto_scaling = true
  enable_availability_zones = true
  
  tags = merge(var.tags, local.common_tags)
  
  depends_on = [
    module.networking,
    module.security,
    module.database
  ]
}

# Output resource identifiers
output "resource_group_name" {
  value       = azurerm_resource_group.resource_group.name
  description = "The name of the main resource group"
}

output "secondary_resource_group_name" {
  value       = azurerm_resource_group.secondary_resource_group.name
  description = "The name of the secondary resource group for disaster recovery"
}

output "primary_location" {
  value       = azurerm_resource_group.resource_group.location
  description = "The primary Azure region where resources are deployed"
}

output "secondary_location" {
  value       = azurerm_resource_group.secondary_resource_group.location
  description = "The secondary Azure region for disaster recovery"
}

output "web_app_name" {
  value       = module.compute.web_app_name
  description = "The name of the web application"
}

output "api_app_name" {
  value       = module.compute.api_app_name
  description = "The name of the API application"
}

output "aks_cluster_name" {
  value       = module.compute.aks_cluster_name
  description = "The name of the AKS cluster"
}

output "sql_server_name" {
  value       = module.database.sql_server_name
  description = "The name of the SQL Server"
}

output "sql_database_name" {
  value       = module.database.sql_database_name
  description = "The name of the SQL Database"
}

output "cosmos_account_name" {
  value       = module.database.cosmos_account_name
  description = "The name of the Cosmos DB account"
}

output "redis_cache_name" {
  value       = module.database.redis_cache_name
  description = "The name of the Redis Cache"
}

output "key_vault_name" {
  value       = module.security.key_vault_name
  description = "The name of the Key Vault"
}

output "storage_account_name" {
  value       = module.security.storage_account_name
  description = "The name of the Storage Account"
}

output "front_door_name" {
  value       = module.networking.front_door_name
  description = "The name of the Front Door"
}

output "api_management_name" {
  value       = module.networking.api_management_name
  description = "The name of the API Management"
}

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