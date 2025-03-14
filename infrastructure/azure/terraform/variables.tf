# Variables for the VAT Filing Pricing Tool Infrastructure

# Environment configuration
variable "environment" {
  type        = string
  description = "Environment name (dev, staging, prod)"
}

# Azure regions
variable "location" {
  type        = string
  description = "Primary Azure region for resource deployment"
  default     = "West Europe"
}

variable "secondary_location" {
  type        = string
  description = "Secondary Azure region for disaster recovery"
  default     = "North Europe"
}

# Application naming
variable "app_name" {
  type        = string
  description = "Application name used in resource naming"
  default     = "vatfilingpricingtool"
}

# Resource tagging
variable "tags" {
  type        = map(string)
  description = "Tags to apply to all resources"
  default     = {
    Project   = "VAT Filing Pricing Tool"
    ManagedBy = "Terraform"
  }
}

# Resource group
variable "resource_group_name" {
  type        = string
  description = "Name of the resource group"
  default     = "rg-vatfilingpricingtool"
}

# Networking
variable "vnet_address_space" {
  type        = list(string)
  description = "Address space for the virtual network"
  default     = ["10.0.0.0/16"]
}

variable "app_subnet_prefix" {
  type        = string
  description = "Subnet prefix for App Service VNet integration"
  default     = "10.0.1.0/24"
}

variable "aks_subnet_prefix" {
  type        = string
  description = "Subnet prefix for AKS cluster"
  default     = "10.0.2.0/24"
}

variable "db_subnet_prefix" {
  type        = string
  description = "Subnet prefix for database services"
  default     = "10.0.3.0/24"
}

# App Service
variable "app_service_sku" {
  type = object({
    tier = string
    size = string
  })
  description = "SKU for App Service Plan"
  default = {
    tier = "Premium"
    size = "P2v3"
  }
}

# AKS
variable "aks_node_count" {
  type        = number
  description = "Number of nodes in the AKS cluster"
  default     = 3
}

variable "aks_vm_size" {
  type        = string
  description = "VM size for AKS nodes"
  default     = "Standard_D4s_v3"
}

# SQL Database
variable "sql_sku" {
  type        = string
  description = "SKU for Azure SQL Database"
  default     = "BusinessCritical_Gen5_8"
}

# Cosmos DB
variable "cosmos_db_consistency_level" {
  type        = string
  description = "Consistency level for Cosmos DB"
  default     = "Session"
}

# Redis Cache
variable "redis_cache_sku" {
  type        = string
  description = "SKU for Redis Cache"
  default     = "Premium"
}

# API Management
variable "api_management_sku" {
  type        = string
  description = "SKU for API Management"
  default     = "Standard"
}

# Storage Account
variable "storage_account_tier" {
  type        = string
  description = "Storage account performance tier"
  default     = "Standard"
}

variable "storage_account_replication_type" {
  type        = string
  description = "Storage account replication strategy"
  default     = "GRS"
}

# Authentication credentials
variable "admin_username" {
  type        = string
  description = "Admin username for database services"
  sensitive   = true
}

variable "admin_password" {
  type        = string
  description = "Admin password for database services"
  sensitive   = true
}

# Scaling and High Availability
variable "aks_min_node_count" {
  type        = number
  description = "Minimum number of nodes when using auto-scaling"
  default     = 3
}

variable "aks_max_node_count" {
  type        = number
  description = "Maximum number of nodes when using auto-scaling"
  default     = 10
}

variable "enable_auto_scaling" {
  type        = bool
  description = "Enable auto-scaling for the AKS cluster"
  default     = true
}

variable "enable_availability_zones" {
  type        = bool
  description = "Enable availability zones for the AKS cluster"
  default     = true
}

# Kubernetes configuration
variable "kubernetes_version" {
  type        = string
  description = "Kubernetes version for the AKS cluster"
  default     = "1.24.9"
}