using Pulumi.AzureNative.Resources;
using System.Collections.Generic;

return await Pulumi.Deployment.RunAsync(() =>
{
    // Create an Azure Resource Group
    var resourceGroup = new ResourceGroup("MagicalYatzyInfra");
    
    // Export the primary key of the Storage Account
    return new Dictionary<string, object?>
    {
        ["resourceGroup"] = resourceGroup.Name,
    };
});