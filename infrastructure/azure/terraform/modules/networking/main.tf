# Terraform module for networking resources for the VAT Filing Pricing Tool on Azure
# Version: 1.0

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

# Input variables
variable "resource_group_name" {
  type        = string
  description = "The name of the resource group where the networking resources will be deployed"
}

variable "location" {
  type        = string
  description = "The primary Azure region where the networking resources will be deployed"
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

variable "vnet_address_space" {
  type        = list(string)
  description = "Address space for the virtual network"
  default     = ["10.0.0.0/16"]
}

variable "app_subnet_prefix" {
  type        = string
  description = "Subnet prefix for App Service"
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

variable "api_management_sku" {
  type        = string
  description = "The SKU for API Management"
  default     = "Standard"
}

variable "api_management_capacity" {
  type        = number
  description = "The capacity of the API Management service"
  default     = 1
}

variable "enable_waf" {
  type        = bool
  description = "Whether to enable Web Application Firewall for Front Door"
  default     = true
}

variable "waf_mode" {
  type        = string
  description = "The mode of the Web Application Firewall (Detection or Prevention)"
  default     = "Detection"
}

variable "enable_private_endpoints" {
  type        = bool
  description = "Whether to create private endpoints for PaaS services"
  default     = false
}

variable "tags" {
  type        = map(string)
  description = "Tags to apply to all resources"
  default     = {}
}

variable "log_analytics_workspace_id" {
  type        = string
  description = "The ID of the Log Analytics Workspace for diagnostics"
}

# Local values
locals {
  vnet_name                    = "${var.resource_name_prefix}-vnet"
  app_subnet_name              = "${var.resource_name_prefix}-app-subnet"
  aks_subnet_name              = "${var.resource_name_prefix}-aks-subnet"
  db_subnet_name               = "${var.resource_name_prefix}-db-subnet"
  app_nsg_name                 = "${var.resource_name_prefix}-app-nsg"
  aks_nsg_name                 = "${var.resource_name_prefix}-aks-nsg"
  db_nsg_name                  = "${var.resource_name_prefix}-db-nsg"
  front_door_name              = "${var.resource_name_prefix}-fd"
  front_door_waf_policy_name   = "${var.resource_name_prefix}-waf-policy"
  api_management_name          = "${var.resource_name_prefix}-apim"
  secondary_vnet_name          = "${var.resource_name_prefix}-vnet-secondary"
  secondary_app_subnet_name    = "${var.resource_name_prefix}-app-subnet-secondary"
  secondary_aks_subnet_name    = "${var.resource_name_prefix}-aks-subnet-secondary"
  secondary_db_subnet_name     = "${var.resource_name_prefix}-db-subnet-secondary"
}

# Data sources
data "azurerm_application_insights" "app_insights" {
  name                = "ai-${var.resource_name_prefix}"
  resource_group_name = var.resource_group_name
}

# Virtual Network in the primary region
resource "azurerm_virtual_network" "vnet" {
  name                = local.vnet_name
  location            = var.location
  resource_group_name = var.resource_group_name
  address_space       = var.vnet_address_space
  tags                = var.tags
}

# App Service subnet
resource "azurerm_subnet" "app_subnet" {
  name                 = local.app_subnet_name
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = [var.app_subnet_prefix]
  
  service_endpoints = [
    "Microsoft.Web",
    "Microsoft.Sql",
    "Microsoft.AzureCosmosDB",
    "Microsoft.KeyVault",
    "Microsoft.Storage"
  ]
  
  delegation {
    name = "delegation"
    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

# AKS subnet
resource "azurerm_subnet" "aks_subnet" {
  name                 = local.aks_subnet_name
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = [var.aks_subnet_prefix]
  
  service_endpoints = [
    "Microsoft.Sql",
    "Microsoft.AzureCosmosDB",
    "Microsoft.KeyVault",
    "Microsoft.Storage"
  ]
}

# Database subnet
resource "azurerm_subnet" "db_subnet" {
  name                 = local.db_subnet_name
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = [var.db_subnet_prefix]
  
  service_endpoints = [
    "Microsoft.Sql",
    "Microsoft.AzureCosmosDB",
    "Microsoft.KeyVault",
    "Microsoft.Storage"
  ]
  
  private_endpoint_network_policies_enabled = !var.enable_private_endpoints
}

# Network Security Group for App Service
resource "azurerm_network_security_group" "app_nsg" {
  name                = local.app_nsg_name
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

# Allow HTTPS inbound for App Service
resource "azurerm_network_security_rule" "app_nsg_rule_https" {
  name                        = "AllowHTTPS"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "443"
  source_address_prefix       = "*"
  destination_address_prefix  = "*"
  resource_group_name         = var.resource_group_name
  network_security_group_name = azurerm_network_security_group.app_nsg.name
}

# Associate NSG with App Service subnet
resource "azurerm_subnet_network_security_group_association" "app_subnet_nsg_association" {
  subnet_id                 = azurerm_subnet.app_subnet.id
  network_security_group_id = azurerm_network_security_group.app_nsg.id
}

# Network Security Group for AKS
resource "azurerm_network_security_group" "aks_nsg" {
  name                = local.aks_nsg_name
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

# Allow HTTPS inbound for AKS
resource "azurerm_network_security_rule" "aks_nsg_rule_https" {
  name                        = "AllowHTTPS"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "443"
  source_address_prefix       = "*"
  destination_address_prefix  = "*"
  resource_group_name         = var.resource_group_name
  network_security_group_name = azurerm_network_security_group.aks_nsg.name
}

# Associate NSG with AKS subnet
resource "azurerm_subnet_network_security_group_association" "aks_subnet_nsg_association" {
  subnet_id                 = azurerm_subnet.aks_subnet.id
  network_security_group_id = azurerm_network_security_group.aks_nsg.id
}

# Network Security Group for database
resource "azurerm_network_security_group" "db_nsg" {
  name                = local.db_nsg_name
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

# Allow SQL inbound for database
resource "azurerm_network_security_rule" "db_nsg_rule_sql" {
  name                        = "AllowSQL"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "1433"
  source_address_prefixes     = [var.app_subnet_prefix, var.aks_subnet_prefix]
  destination_address_prefix  = "*"
  resource_group_name         = var.resource_group_name
  network_security_group_name = azurerm_network_security_group.db_nsg.name
}

# Associate NSG with database subnet
resource "azurerm_subnet_network_security_group_association" "db_subnet_nsg_association" {
  subnet_id                 = azurerm_subnet.db_subnet.id
  network_security_group_id = azurerm_network_security_group.db_nsg.id
}

# Web Application Firewall policy for Front Door
resource "azurerm_frontdoor_firewall_policy" "front_door_waf_policy" {
  count               = var.enable_waf ? 1 : 0
  name                = local.front_door_waf_policy_name
  resource_group_name = var.resource_group_name
  enabled             = var.enable_waf
  mode                = var.waf_mode
  
  redirect_url = null
  custom_block_response_status_code = 403
  custom_block_response_body        = "PGh0bWw+CjxoZWFkPgogICAgPHRpdGxlPkFjY2VzcyBkZW5pZWQ8L3RpdGxlPgo8L2hlYWQ+Cjxib2R5PgogICAgPGgxPkFjY2VzcyBkZW5pZWQ8L2gxPgogICAgPHA+VGhpcyByZXF1ZXN0IGhhcyBiZWVuIGJsb2NrZWQgYnkgV0FGPC9wPgo8L2JvZHk+CjwvaHRtbD4="
  
  # Default rules
  managed_rule {
    type    = "DefaultRuleSet"
    version = "1.0"
    action  = "Block"
  }
  
  # Bot Manager rules
  managed_rule {
    type    = "Microsoft_BotManagerRuleSet"
    version = "1.0"
    action  = "Block"
  }
  
  tags = var.tags
}

# Azure Front Door
resource "azurerm_frontdoor" "front_door" {
  name                                         = local.front_door_name
  resource_group_name                          = var.resource_group_name
  enforce_backend_pools_certificate_name_check = true
  
  # Routing rule for web application
  routing_rule {
    name               = "web-rule"
    accepted_protocols = ["Https"]
    patterns_to_match  = ["/*"]
    frontend_endpoints = ["${local.front_door_name}-frontend"]
    
    forwarding_configuration {
      forwarding_protocol = "HttpsOnly"
      backend_pool_name   = "web-backend"
    }
  }
  
  # Routing rule for API
  routing_rule {
    name               = "api-rule"
    accepted_protocols = ["Https"]
    patterns_to_match  = ["/api/*"]
    frontend_endpoints = ["${local.front_door_name}-frontend"]
    
    forwarding_configuration {
      forwarding_protocol = "HttpsOnly"
      backend_pool_name   = "api-backend"
    }
  }
  
  # Backend pool for web application
  backend_pool {
    name = "web-backend"
    
    load_balancing_name = "web-lb"
    health_probe_name   = "web-probe"
    
    backend {
      host_header  = "${var.resource_name_prefix}-web.azurewebsites.net"
      address      = "${var.resource_name_prefix}-web.azurewebsites.net"
      http_port    = 80
      https_port   = 443
      priority     = 1
      weight       = 100
    }
    
    backend {
      host_header  = "${var.resource_name_prefix}-web-secondary.azurewebsites.net"
      address      = "${var.resource_name_prefix}-web-secondary.azurewebsites.net"
      http_port    = 80
      https_port   = 443
      priority     = 2
      weight       = 100
    }
  }
  
  # Backend pool for API
  backend_pool {
    name = "api-backend"
    
    load_balancing_name = "api-lb"
    health_probe_name   = "api-probe"
    
    backend {
      host_header  = "${local.api_management_name}.azure-api.net"
      address      = "${local.api_management_name}.azure-api.net"
      http_port    = 80
      https_port   = 443
      priority     = 1
      weight       = 100
    }
  }
  
  # Health probe for web application
  backend_pool_health_probe {
    name                = "web-probe"
    protocol            = "Https"
    path                = "/health"
    interval_in_seconds = 30
    probe_method        = "GET"
  }
  
  # Health probe for API
  backend_pool_health_probe {
    name                = "api-probe"
    protocol            = "Https"
    path                = "/health"
    interval_in_seconds = 30
    probe_method        = "GET"
  }
  
  # Load balancing for web application
  backend_pool_load_balancing {
    name                            = "web-lb"
    sample_size                     = 4
    successful_samples_required     = 2
    additional_latency_milliseconds = 0
  }
  
  # Load balancing for API
  backend_pool_load_balancing {
    name                            = "api-lb"
    sample_size                     = 4
    successful_samples_required     = 2
    additional_latency_milliseconds = 0
  }
  
  # Frontend endpoint
  frontend_endpoint {
    name                                    = "${local.front_door_name}-frontend"
    host_name                               = "${local.front_door_name}.azurefd.net"
    session_affinity_enabled                = true
    session_affinity_ttl_seconds            = 300
    web_application_firewall_policy_link_id = var.enable_waf ? azurerm_frontdoor_firewall_policy.front_door_waf_policy[0].id : null
  }
  
  tags = var.tags
}

# API Management
resource "azurerm_api_management" "api_management" {
  name                = local.api_management_name
  location            = var.location
  resource_group_name = var.resource_group_name
  publisher_name      = "VAT Filing Pricing Tool"
  publisher_email     = "admin@example.com"
  
  sku_name            = "${var.api_management_sku}_${var.api_management_capacity}"
  virtual_network_type = "None"
  min_api_version     = "2019-12-01"
  
  identity {
    type = "SystemAssigned"
  }
  
  protocols {
    enable_http2 = true
  }
  
  security {
    enable_backend_ssl30 = false
    enable_backend_tls10 = false
    enable_backend_tls11 = false
    enable_frontend_ssl30 = false
    enable_frontend_tls10 = false
    enable_frontend_tls11 = false
  }
  
  tags = var.tags
}

# API Management logger
resource "azurerm_api_management_logger" "api_management_logger" {
  name                = "apim-logger"
  api_management_name = azurerm_api_management.api_management.name
  resource_group_name = var.resource_group_name
  resource_id         = var.log_analytics_workspace_id
  
  credentials {
    name              = "instrumentationKey"
    connection_string = data.azurerm_application_insights.app_insights.instrumentation_key
  }
}

# API Management diagnostic settings
resource "azurerm_api_management_diagnostic" "api_management_diagnostic" {
  identifier          = "applicationinsights"
  api_management_name = azurerm_api_management.api_management.name
  resource_group_name = var.resource_group_name
  api_management_logger_id = azurerm_api_management_logger.api_management_logger.id
  
  sampling_percentage = 100
  always_log_errors   = true
  log_client_ip       = true
  http_correlation_protocol = "W3C"
  verbosity           = "information"
  
  backend_request {
    body_bytes    = 8192
    headers_to_log = ["content-type", "accept", "origin"]
  }
  
  backend_response {
    body_bytes    = 8192
    headers_to_log = ["content-type", "content-length", "origin"]
  }
  
  frontend_request {
    body_bytes    = 8192
    headers_to_log = ["content-type", "accept", "origin"]
  }
  
  frontend_response {
    body_bytes    = 8192
    headers_to_log = ["content-type", "content-length", "origin"]
  }
}

# API Management API definition
resource "azurerm_api_management_api" "api_management_api" {
  name                = "vatfilingpricingtool-api"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.api_management.name
  revision            = "1"
  display_name        = "VAT Filing Pricing Tool API"
  path                = "api"
  protocols           = ["https"]
  service_url         = "https://${var.resource_name_prefix}-api.azurewebsites.net"
  subscription_required = true
  
  import {
    content_format = "openapi+json"
    content_value  = file("${path.module}/api_definition.json")
  }
}

# API Management product
resource "azurerm_api_management_product" "api_management_product" {
  product_id          = "vatfilingpricingtool"
  api_management_name = azurerm_api_management.api_management.name
  resource_group_name = var.resource_group_name
  display_name        = "VAT Filing Pricing Tool"
  subscription_required = true
  approval_required   = false
  published           = true
}

# Associate API with product
resource "azurerm_api_management_product_api" "api_management_product_api" {
  api_name            = azurerm_api_management_api.api_management_api.name
  product_id          = azurerm_api_management_product.api_management_product.product_id
  api_management_name = azurerm_api_management.api_management.name
  resource_group_name = var.resource_group_name
}

# API policy
resource "azurerm_api_management_api_policy" "api_management_policy" {
  api_name            = azurerm_api_management_api.api_management_api.name
  api_management_name = azurerm_api_management.api_management.name
  resource_group_name = var.resource_group_name
  
  xml_content = <<XML
<policies>
  <inbound>
    <base />
    <cors>
      <allowed-origins>
        <origin>https://${local.front_door_name}.azurefd.net</origin>
        <origin>https://${var.resource_name_prefix}-web.azurewebsites.net</origin>
        <origin>https://${var.resource_name_prefix}-web-secondary.azurewebsites.net</origin>
      </allowed-origins>
      <allowed-methods>
        <method>GET</method>
        <method>POST</method>
        <method>PUT</method>
        <method>DELETE</method>
        <method>OPTIONS</method>
      </allowed-methods>
      <allowed-headers>
        <header>*</header>
      </allowed-headers>
      <expose-headers>
        <header>*</header>
      </expose-headers>
    </cors>
    <set-header name="X-API-Version" exists-action="override">
      <value>1.0</value>
    </set-header>
    <rate-limit calls="300" renewal-period="60" />
    <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized">
      <openid-config url="https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration" />
      <audiences>
        <audience>api://vatfilingpricingtool</audience>
      </audiences>
    </validate-jwt>
  </inbound>
  <backend>
    <base />
  </backend>
  <outbound>
    <base />
    <set-header name="X-Powered-By" exists-action="delete" />
    <set-header name="X-AspNet-Version" exists-action="delete" />
  </outbound>
  <on-error>
    <base />
  </on-error>
</policies>
XML
}

# Virtual Network in the secondary region
resource "azurerm_virtual_network" "secondary_vnet" {
  name                = local.secondary_vnet_name
  location            = var.secondary_location
  resource_group_name = var.secondary_resource_group_name
  address_space       = var.vnet_address_space
  tags                = var.tags
}

# App Service subnet in the secondary region
resource "azurerm_subnet" "secondary_app_subnet" {
  name                 = local.secondary_app_subnet_name
  resource_group_name  = var.secondary_resource_group_name
  virtual_network_name = azurerm_virtual_network.secondary_vnet.name
  address_prefixes     = [var.app_subnet_prefix]
  
  service_endpoints = [
    "Microsoft.Web",
    "Microsoft.Sql",
    "Microsoft.AzureCosmosDB",
    "Microsoft.KeyVault",
    "Microsoft.Storage"
  ]
  
  delegation {
    name = "delegation"
    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

# AKS subnet in the secondary region
resource "azurerm_subnet" "secondary_aks_subnet" {
  name                 = local.secondary_aks_subnet_name
  resource_group_name  = var.secondary_resource_group_name
  virtual_network_name = azurerm_virtual_network.secondary_vnet.name
  address_prefixes     = [var.aks_subnet_prefix]
  
  service_endpoints = [
    "Microsoft.Sql",
    "Microsoft.AzureCosmosDB",
    "Microsoft.KeyVault",
    "Microsoft.Storage"
  ]
}

# Database subnet in the secondary region
resource "azurerm_subnet" "secondary_db_subnet" {
  name                 = local.secondary_db_subnet_name
  resource_group_name  = var.secondary_resource_group_name
  virtual_network_name = azurerm_virtual_network.secondary_vnet.name
  address_prefixes     = [var.db_subnet_prefix]
  
  service_endpoints = [
    "Microsoft.Sql",
    "Microsoft.AzureCosmosDB",
    "Microsoft.KeyVault",
    "Microsoft.Storage"
  ]
  
  private_endpoint_network_policies_enabled = !var.enable_private_endpoints
}

# VNet peering from primary to secondary
resource "azurerm_virtual_network_peering" "vnet_peering_primary_to_secondary" {
  name                      = "primary-to-secondary"
  resource_group_name       = var.resource_group_name
  virtual_network_name      = azurerm_virtual_network.vnet.name
  remote_virtual_network_id = azurerm_virtual_network.secondary_vnet.id
  allow_virtual_network_access = true
  allow_forwarded_traffic   = true
  allow_gateway_transit     = false
  use_remote_gateways       = false
}

# VNet peering from secondary to primary
resource "azurerm_virtual_network_peering" "vnet_peering_secondary_to_primary" {
  name                      = "secondary-to-primary"
  resource_group_name       = var.secondary_resource_group_name
  virtual_network_name      = azurerm_virtual_network.secondary_vnet.name
  remote_virtual_network_id = azurerm_virtual_network.vnet.id
  allow_virtual_network_access = true
  allow_forwarded_traffic   = true
  allow_gateway_transit     = false
  use_remote_gateways       = false
}

# Diagnostic settings for Front Door
resource "azurerm_monitor_diagnostic_setting" "front_door_diagnostic_setting" {
  name                       = "front-door-diagnostics"
  target_resource_id         = azurerm_frontdoor.front_door.id
  log_analytics_workspace_id = var.log_analytics_workspace_id
  
  log {
    category = "FrontdoorAccessLog"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "FrontdoorWebApplicationFirewallLog"
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

# Diagnostic settings for API Management
resource "azurerm_monitor_diagnostic_setting" "api_management_diagnostic_setting" {
  name                       = "api-management-diagnostics"
  target_resource_id         = azurerm_api_management.api_management.id
  log_analytics_workspace_id = var.log_analytics_workspace_id
  
  log {
    category = "GatewayLogs"
    enabled  = true
    
    retention_policy {
      enabled = true
      days    = 30
    }
  }
  
  log {
    category = "WebSocketConnectionLogs"
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
output "vnet_id" {
  value       = azurerm_virtual_network.vnet.id
  description = "ID of the created Virtual Network"
}

output "app_subnet_id" {
  value       = azurerm_subnet.app_subnet.id
  description = "ID of the App Service subnet"
}

output "aks_subnet_id" {
  value       = azurerm_subnet.aks_subnet.id
  description = "ID of the AKS subnet"
}

output "db_subnet_id" {
  value       = azurerm_subnet.db_subnet.id
  description = "ID of the database subnet"
}

output "front_door_name" {
  value       = azurerm_frontdoor.front_door.name
  description = "Name of the Azure Front Door"
}

output "front_door_id" {
  value       = azurerm_frontdoor.front_door.id
  description = "ID of the Azure Front Door"
}

output "front_door_endpoint" {
  value       = "https://${azurerm_frontdoor.front_door.frontend_endpoint[0].host_name}"
  description = "Endpoint URL of the Azure Front Door"
}

output "api_management_name" {
  value       = azurerm_api_management.api_management.name
  description = "Name of the API Management service"
}

output "api_management_id" {
  value       = azurerm_api_management.api_management.id
  description = "ID of the API Management service"
}

output "api_management_gateway_url" {
  value       = "https://${azurerm_api_management.api_management.gateway_url}"
  description = "Gateway URL of the API Management service"
}

output "secondary_vnet_id" {
  value       = azurerm_virtual_network.secondary_vnet.id
  description = "ID of the secondary Virtual Network for disaster recovery"
}