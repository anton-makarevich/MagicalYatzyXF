## Azure Infra

To authenticate Pulumi requires a Service Principal.
Locally it should be saved in `Pulumi.{pulume-stack-name}.yaml` but should not be tracked in Git.

```yaml

```

### Auth/z

#### Local CLI

az login --tenant {main tenant ID}
Default subscription.

#### GitHub Actions
Use Service Principal for authentication:
https://www.pulumi.com/registry/packages/azure-native/installation-configuration/#authenticate-using-a-service-principal

