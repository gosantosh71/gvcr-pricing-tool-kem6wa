# Environment configuration
environment = "staging"

# Azure regions
location            = "West Europe"
secondary_location  = "North Europe"

# Application naming
app_name = "vatfilingpricingtool"

# Resource tagging
tags = {
  Environment = "Staging"
  Project     = "VAT Filing Pricing Tool"
  Department  = "Finance"
  ManagedBy   = "Terraform"
  CostCenter  = "IT-12345"
}

# Resource group
resource_group_name = "rg-vatfilingpricingtool-staging"

# Networking - Using 10.1.x.x for staging (differentiating from other environments)
vnet_address_space = ["10.1.0.0/16"]
app_subnet_prefix  = "10.1.1.0/24"
aks_subnet_prefix  = "10.1.2.0/24"
db_subnet_prefix   = "10.1.3.0/24"

# App Service - Using P1v2 for staging (smaller than production)
app_service_sku = {
  tier = "Premium"
  size = "P1v2"
}

# AKS - Standard_D4s_v3 matches the 4 vCPU, 16GB RAM requirement for staging
aks_node_count = 3
aks_vm_size    = "Standard_D4s_v3"

# SQL Database - Using Standard tier for staging instead of Business Critical
sql_sku = "Standard_S4"

# Cosmos DB
cosmos_db_consistency_level = "Session"

# Redis Cache - Using Standard tier for staging instead of Premium
redis_cache_sku = "Standard"

# API Management
api_management_sku = "Standard"

# Storage Account - Using ZRS for staging instead of GRS to optimize costs
storage_account_tier             = "Standard"
storage_account_replication_type = "ZRS"

# Authentication credentials
# Note: In a production environment, these would be managed through Azure Key Vault
admin_username = "vatadmin"
admin_password = "P@ssw0rd1234!"

# Scaling and High Availability
aks_min_node_count = 3
aks_max_node_count = 6
enable_auto_scaling = true
enable_availability_zones = true

# Kubernetes configuration
kubernetes_version = "1.24.9"