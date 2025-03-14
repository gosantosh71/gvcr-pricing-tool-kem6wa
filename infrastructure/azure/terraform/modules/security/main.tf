# Azure Security Module for VAT Filing Pricing Tool
# Terraform version: 1.0+
# Azure Provider version: ~> 3.0
# Random Provider version: ~> 3.1

# Import required providers
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

# Get current Azure client configuration
data "azurerm_client_config" "current" {}

# Local variables for resource naming
locals {
  key_vault_name               = "${var.resource_name_prefix}-kv"
  storage_account_name         = "${replace(lower(var.resource_name_prefix), "-", "")}sa"
  secondary_storage_account_name = "${replace(lower(var.resource_name_prefix), "-", "")}sadr"
}

# Key Vault for storing secrets, keys, and certificates
resource "azurerm_key_vault" "key_vault" {
  name                        = local.key_vault_name
  location                    = var.location
  resource_group_name         = var.resource_group_name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = var.key_vault_sku
  soft_delete_retention_days  = var.soft_delete_retention_days
  purge_protection_enabled    = var.enable_purge_protection
  enabled_for_disk_encryption = true
  enabled_for_deployment      = true
  enabled_for_template_deployment = true
  enable_rbac_authorization   = false

  network_acls {
    default_action             = var.network_acls.default_action
    bypass                     = var.network_acls.bypass
    ip_rules                   = var.network_acls.ip_rules
    virtual_network_subnet_ids = var.network_acls.virtual_network_subnet_ids
  }

  tags = var.tags
}

# Access policy for the current user/service principal
resource "azurerm_key_vault_access_policy" "key_vault_admin_access_policy" {
  key_vault_id = azurerm_key_vault.key_vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  key_permissions = [
    "Get", "List", "Create", "Delete", "Update", "Import", "Backup", "Restore", "Recover"
  ]

  secret_permissions = [
    "Get", "List", "Set", "Delete", "Backup", "Restore", "Recover"
  ]

  certificate_permissions = [
    "Get", "List", "Create", "Delete", "Update", "Import", "Backup", "Restore", "Recover"
  ]

  storage_permissions = [
    "Get", "List", "Set", "Delete", "Backup", "Restore", "Recover"
  ]
}

# Storage Account for storing application data, reports, and documents
resource "azurerm_storage_account" "storage_account" {
  name                     = local.storage_account_name
  location                 = var.location
  resource_group_name      = var.resource_group_name
  account_tier             = var.storage_account_tier
  account_replication_type = var.storage_account_replication_type
  account_kind             = "StorageV2"
  access_tier              = "Hot"
  enable_https_traffic_only = true
  min_tls_version          = "TLS1_2"
  allow_nested_items_to_be_public = false
  shared_access_key_enabled = true

  blob_properties {
    versioning_enabled     = true
    change_feed_enabled    = true
    
    container_delete_retention_policy {
      days = 7
    }
    
    delete_retention_policy {
      days = 30
    }
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Secondary Storage Account for disaster recovery
resource "azurerm_storage_account" "secondary_storage_account" {
  name                     = local.secondary_storage_account_name
  location                 = var.secondary_location
  resource_group_name      = var.secondary_resource_group_name
  account_tier             = var.storage_account_tier
  account_replication_type = var.storage_account_replication_type
  account_kind             = "StorageV2"
  access_tier              = "Hot"
  enable_https_traffic_only = true
  min_tls_version          = "TLS1_2"
  allow_nested_items_to_be_public = false
  shared_access_key_enabled = true

  blob_properties {
    versioning_enabled     = true
    change_feed_enabled    = true
    
    container_delete_retention_policy {
      days = 7
    }
    
    delete_retention_policy {
      days = 30
    }
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Storage container for reports
resource "azurerm_storage_container" "reports_container" {
  name                  = "reports"
  storage_account_name  = azurerm_storage_account.storage_account.name
  container_access_type = "private"
}

# Storage container for report templates
resource "azurerm_storage_container" "templates_container" {
  name                  = "templates"
  storage_account_name  = azurerm_storage_account.storage_account.name
  container_access_type = "private"
}

# Storage container for uploaded documents
resource "azurerm_storage_container" "documents_container" {
  name                  = "documents"
  storage_account_name  = azurerm_storage_account.storage_account.name
  container_access_type = "private"
}

# Storage container for reports in the secondary region
resource "azurerm_storage_container" "secondary_reports_container" {
  name                  = "reports"
  storage_account_name  = azurerm_storage_account.secondary_storage_account.name
  container_access_type = "private"
}

# Storage container for report templates in the secondary region
resource "azurerm_storage_container" "secondary_templates_container" {
  name                  = "templates"
  storage_account_name  = azurerm_storage_account.secondary_storage_account.name
  container_access_type = "private"
}

# Storage container for uploaded documents in the secondary region
resource "azurerm_storage_container" "secondary_documents_container" {
  name                  = "documents"
  storage_account_name  = azurerm_storage_account.secondary_storage_account.name
  container_access_type = "private"
}

# Store Storage Account connection string in Key Vault
resource "azurerm_key_vault_secret" "storage_account_key_vault_secret" {
  name         = "StorageConnectionString"
  key_vault_id = azurerm_key_vault.key_vault.id
  value        = azurerm_storage_account.storage_account.primary_connection_string

  depends_on = [
    azurerm_key_vault_access_policy.key_vault_admin_access_policy
  ]
}

# Store Secondary Storage Account connection string in Key Vault
resource "azurerm_key_vault_secret" "secondary_storage_account_key_vault_secret" {
  name         = "SecondaryStorageConnectionString"
  key_vault_id = azurerm_key_vault.key_vault.id
  value        = azurerm_storage_account.secondary_storage_account.primary_connection_string

  depends_on = [
    azurerm_key_vault_access_policy.key_vault_admin_access_policy
  ]
}

# Diagnostic settings for Key Vault
resource "azurerm_monitor_diagnostic_setting" "key_vault_diagnostic_setting" {
  name                       = "key-vault-diagnostics"
  target_resource_id         = azurerm_key_vault.key_vault.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "AuditEvent"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "AzurePolicyEvaluationDetails"
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

# Diagnostic settings for Storage Account
resource "azurerm_monitor_diagnostic_setting" "storage_account_diagnostic_setting" {
  name                       = "storage-account-diagnostics"
  target_resource_id         = azurerm_storage_account.storage_account.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  metric {
    category = "Transaction"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }

  metric {
    category = "Capacity"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }
}

# Diagnostic settings for Blob Storage
resource "azurerm_monitor_diagnostic_setting" "storage_blob_diagnostic_setting" {
  name                       = "storage-blob-diagnostics"
  target_resource_id         = "${azurerm_storage_account.storage_account.id}/blobServices/default"
  log_analytics_workspace_id = var.log_analytics_workspace_id

  log {
    category = "StorageRead"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "StorageWrite"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }

  log {
    category = "StorageDelete"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }

  metric {
    category = "Capacity"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }

  metric {
    category = "Transaction"
    enabled  = true

    retention_policy {
      enabled = true
      days    = 30
    }
  }
}

# Define outputs for the module
output "key_vault_id" {
  value       = azurerm_key_vault.key_vault.id
  description = "ID of the Key Vault"
}

output "key_vault_name" {
  value       = azurerm_key_vault.key_vault.name
  description = "Name of the Key Vault"
}

output "key_vault_uri" {
  value       = azurerm_key_vault.key_vault.vault_uri
  description = "URI of the Key Vault"
}

output "storage_account_id" {
  value       = azurerm_storage_account.storage_account.id
  description = "ID of the Storage Account"
}

output "storage_account_name" {
  value       = azurerm_storage_account.storage_account.name
  description = "Name of the Storage Account"
}

output "primary_blob_endpoint" {
  value       = azurerm_storage_account.storage_account.primary_blob_endpoint
  description = "Primary endpoint for Blob Storage"
}

output "secondary_storage_account_id" {
  value       = azurerm_storage_account.secondary_storage_account.id
  description = "ID of the Secondary Storage Account"
}

output "secondary_storage_account_name" {
  value       = azurerm_storage_account.secondary_storage_account.name
  description = "Name of the Secondary Storage Account"
}

output "secondary_blob_endpoint" {
  value       = azurerm_storage_account.secondary_storage_account.primary_blob_endpoint
  description = "Secondary endpoint for Blob Storage"
}