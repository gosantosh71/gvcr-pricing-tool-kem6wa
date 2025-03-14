environment               = "prod"
location                  = "West Europe"
secondary_location        = "North Europe"
app_name                  = "vatfilingpricingtool"

# Tags
tags = {
  Environment = "Production"
  Project     = "VAT Filing Pricing Tool"
  Department  = "Finance"
  ManagedBy   = "Terraform"
  CostCenter  = "IT-12345"
}

# Resource Group
resource_group_name       = "rg-vatfilingpricingtool-prod"

# Networking
vnet_address_space        = ["10.2.0.0/16"]
app_subnet_prefix         = "10.2.1.0/24"
aks_subnet_prefix         = "10.2.2.0/24"
db_subnet_prefix          = "10.2.3.0/24"

# App Service
app_service_sku = {
  tier = "Premium"
  size = "P2v3"
}

# AKS
aks_node_count            = 3
aks_vm_size               = "Standard_D4s_v3"
aks_min_node_count        = 3
aks_max_node_count        = 10
enable_auto_scaling       = true
enable_availability_zones = true
kubernetes_version        = "1.24.9"

# SQL Database
sql_sku                   = "BusinessCritical_Gen5_8"

# Cosmos DB
cosmos_db_consistency_level = "Strong"

# Redis Cache
redis_cache_sku           = "Premium"

# API Management
api_management_sku        = "Standard"

# Storage Account
storage_account_tier              = "Standard"
storage_account_replication_type  = "GRS"

# Authentication credentials
# Note: In a production environment, these should be stored in Azure Key Vault
admin_username            = "vatadmin"
admin_password            = "P@ssw0rd1234!"