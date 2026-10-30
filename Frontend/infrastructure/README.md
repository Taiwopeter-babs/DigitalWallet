# Frontend infrastructure deployment

The GitHub Actions workflow at [`.github/workflows/deploy-frontend.yml`](../../.github/workflows/deploy-frontend.yml) builds and pushes a commit-tagged image to Docker Hub, then applies this Terraform configuration to deploy that image to the Azure Container App.

Configure these GitHub Actions secrets:

- `DOCKERHUB_USERNAME`: Docker Hub account that owns `frontend-wallet`.
- `DOCKERHUB_TOKEN`: Docker Hub access token with permission to push the image and pull it from the Container App.
- `TF_API_TOKEN`: Terraform Cloud API token with access to the `digital-wallet-frontend` workspace.
- `AZURE_SUBSCRIPTION_ID`: Azure subscription containing the deployment.

Terraform uses the Terraform Cloud workspace configured in `main.tf`. Add the following **environment variables** to that workspace so its remote runs can authenticate to Azure:

- `ARM_CLIENT_ID`
- `ARM_CLIENT_SECRET` (mark sensitive)
- `ARM_TENANT_ID`

The workflow supplies the Terraform input variables for the subscription, Docker Hub credentials, and commit-specific image tag. The Container App is updated by Terraform to use each new image tag.
