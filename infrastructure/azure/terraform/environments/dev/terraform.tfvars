environment = "dev"

# Azure regions
location = "West Europe"
secondary_location = "North Europe"

# Application naming
app_name = "vatfilingpricingtool"

# Resource tagging
tags = {
  Environment = "Development"
  Project     = "VAT Filing Pricing Tool"
  Department  = "Finance"
  ManagedBy   = "Terraform"
  CostCenter  = "IT-12345"
}

# Resource group
resource_group_name = "rg-vatfilingpricingtool-dev"

# Networking
vnet_address_space = ["10.0.0.0/16"]
app_subnet_prefix  = "10.0.1.0/24"
aks_subnet_prefix  = "10.0.2.0/24"
db_subnet_prefix   = "10.0.3.0/24"

# App Service - Standard tier for development
app_service_sku = {
  tier = "Standard"
  size = "S1"
}

# AKS - reduced resources for development
aks_node_count = 2
aks_vm_size    = "Standard_D2s_v3" # 2 vCPU, 8GB RAM as per requirements

# SQL Database - Standard tier for development
sql_sku = "Standard_S2"

# Cosmos DB
cosmos_db_consistency_level = "Session"

# Redis Cache - Standard tier for development
redis_cache_sku = "Standard"

# API Management - Developer tier for cost optimization
api_management_sku = "Developer"

# Storage Account - Locally redundant for development
storage_account_tier = "Standard"
storage_account_replication_type = "LRS"

# Authentication credentials
admin_username = "vatadmin"
admin_password = "P@ssw0rd1234!"

# Scaling configuration
aks_min_node_count = 2
aks_max_node_count = 4
enable_auto_scaling = true
enable_availability_zones = false

# Kubernetes configuration
kubernetes_version = "1.24.9"