using Pulumi;
using Pulumi.AzureNative.Resources.V20240701;

namespace MagicalYatzyAzureResources.BaseInfra;

public class BaseInfra
{
    StackReference baseInfraStack = new StackReference("sanetby/MagicalYatzyInfra.AwsLZ/my-aws-lz-dev");

    public ResourceGroup GetResourceGroup()
    {
        // Get the resource group name from the output exports of the base infra stack
        var resourceGroupName = baseInfraStack.GetOutput("resourceGroup")
            .Apply(resourceGroupName => (string)resourceGroupName);

        // Import the existing resource group
        var resourceGroup = new ResourceGroup("importedResourceGroup", new ResourceGroupArgs
        {
            ResourceGroupName = resourceGroupName,
        }, new CustomResourceOptions
        {
            ImportId = resourceGroupName
        });

        // Export the resource group name
        return resourceGroup;
    }
}