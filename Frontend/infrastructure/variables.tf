# Copyright IBM Corp. 2014, 2026
# SPDX-License-Identifier: MPL-2.0

variable "prefix" {
  default     = "digitalwalletfrontend"
  description = "The prefix used for all resources in this app"
}

variable "resource_group_name" {
  default = "azure-resource-group"
}

variable "resource_group_location" {
  type        = string
  default     = "northeurope"
  description = "The Azure location where all resources for the DigitalWallet will be created"
}



variable "container_group_name_prefix" {
  type        = string
  description = "Prefix of the container group name that's combined with a random value so name is unique in your Azure subscription."
  default     = "acigroup"
}

variable "image" {
  type        = string
  description = "Container image to deploy. Should be of the form repoName/imagename:tag for images stored in public Docker Hub, or a fully qualified URI for other registries. Images from private registries require additional registry credentials."
  default     = "taiwopeter/frontend-wallet:latest"
}

variable "azure_subscription_id" {
  type        = string
  description = "Azure Subscription ID"
  sensitive   = true
}

variable "dockerhub_username" {
  type        = string
  description = "Azure docker hub token"
  default = "taiwopeter"
}

variable "azure_dockerhub_token" {
  type        = string
  description = "Azure docker hub token"
  sensitive   = true
}
