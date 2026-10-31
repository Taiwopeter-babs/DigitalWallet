# Copyright IBM Corp. 2014, 2026
# SPDX-License-Identifier: MPL-2.0

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 5.10.0"
    }
  }

  required_version = ">= 1.1.0"

  cloud {
    organization = "babalolataiwop-org"
    workspaces {
      name = "digital-wallet-frontend"
    }
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
    }
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }

  use_cli         = false
  subscription_id = var.azure_subscription_id
}

data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "frontend_wallet_rg" {
  name     = "${var.prefix}-${var.resource_group_name}"
  location = var.resource_group_location

  tags = {
    Environment = "Digital Wallet Production"
    Team        = "Taiwo-Devops"
  }

  lifecycle {
    create_before_destroy = true
  }
}

resource "azurerm_key_vault" "kv" {
  name                        = "kv-nextjs-secrets-prod"
  location                    = azurerm_resource_group.frontend_wallet_rg.location
  resource_group_name         = azurerm_resource_group.frontend_wallet_rg.name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  sku_name                    = "standard"
  rbac_authorization_enabled  = false
  purge_protection_enabled    = false


  # Grant your deployment user/pipeline full access to create secrets
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    key_permissions = ["Get"]

    secret_permissions = ["Get", "List", "Set", "Delete", "Purge"]
  }
}

resource "azurerm_key_vault_secret" "api_url_secret" {
  name         = "digital-wallet-api-url"
  value        = "https://yourdomain.com"
  key_vault_id = azurerm_key_vault.kv.id
}

resource "azurerm_container_registry" "frontend_cr" {
  name                = "${var.prefix}cr"
  resource_group_name = azurerm_resource_group.frontend_wallet_rg.name
  location            = azurerm_resource_group.frontend_wallet_rg.location
  sku                 = "Basic"

  lifecycle {
    create_before_destroy = true
  }
}

resource "azurerm_log_analytics_workspace" "frontend_cla" {
  name                = "${var.prefix}-log-analytics"
  location            = azurerm_resource_group.frontend_wallet_rg.location
  resource_group_name = azurerm_resource_group.frontend_wallet_rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  lifecycle {
    create_before_destroy = true
  }
}

resource "azurerm_container_app_environment" "frontend_cae" {
  name                       = "${var.prefix}-container-app-env"
  location                   = azurerm_resource_group.frontend_wallet_rg.location
  resource_group_name        = azurerm_resource_group.frontend_wallet_rg.name
  logs_destination           = "log-analytics"
  log_analytics_workspace_id = azurerm_log_analytics_workspace.frontend_cla.id

  lifecycle {
    create_before_destroy = true
  }
}


variable "container_password_name" {
  type        = string
  default     = "acr-password"
  description = "The container app password name"
}

# import {
#   to = azurerm_container_app.frontend_ca
#   id = "/subscriptions/${var.azure_subscription_id}/resourceGroups/${var.prefix}-${var.resource_group_name}/providers/Microsoft.App/containerApps/${var.prefix}ca"
# }

resource "azurerm_container_app" "frontend_ca" {
  name                         = "${var.prefix}ca"
  container_app_environment_id = azurerm_container_app_environment.frontend_cae.id
  #   resource_group_name          = azurerm_container_registry.frontend_cr.name
  resource_group_name = azurerm_resource_group.frontend_wallet_rg.name
  revision_mode       = "Single"


  identity {
    type = "SystemAssigned"
  }

  secret {
    name                = "kv-api-url"
    key_vault_secret_id = azurerm_key_vault_secret.api_url_secret.id
    identity            = "System"
  }

  #   secret {
  #     name  = var.container_password_name
  #     value = azurerm_container_registry.frontend_cr.admin_password
  #   }

  #   registry {
  #     server               = azurerm_container_registry.frontend_cr.login_server
  #     username             = azurerm_container_registry.frontend_cr.admin_username
  #     password_secret_name = var.container_password_name
  #   }

  registry {
    server               = "index.docker.io"
    username             = var.dockerhub_username
    password_secret_name = "dockerhub-token"
  }

  secret {
    name  = "dockerhub-token"
    value = var.azure_dockerhub_token
  }


  ingress {
    allow_insecure_connections = false
    target_port                = 3000
    external_enabled           = true
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  template {
    container {
      name = "${var.prefix}nextjs"
      #   image  = "${azurerm_container_registry.frontend_cr.login_server}/frontend-wallet:latest"
      image = var.image

      cpu    = "0.25"
      memory = "0.5Gi"

      env {
        name  = "NODE_ENV"
        value = "production"
      }

      env {
        name  = "API_URL"
        value = "kv-api-url"
      }
    }
  }

  lifecycle {
    create_before_destroy = true
  }
}

resource "azurerm_key_vault_access_policy" "app_vault_access" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = azurerm_container_app.frontend_ca.identity[0].tenant_id
  object_id    = azurerm_container_app.frontend_ca.identity[0].principal_id

  secret_permissions = ["Get"]
}

output "acr_login_server" {
  value = azurerm_container_registry.frontend_cr.login_server
}

output "app_url" {
  value = azurerm_container_app.frontend_ca.ingress[0].fqdn
}
