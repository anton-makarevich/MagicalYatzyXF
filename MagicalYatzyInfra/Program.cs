﻿using Pulumi.AzureNative.Resources;
using System.Collections.Generic;
using MagicalYatzy.Infra.Resources;

return await Pulumi.Deployment.RunAsync(() =>
{
    // Create an Azure Resource Group
    var resourceGroup = new ResourceGroup("MagicalYatzyInfra");
    
    var azureAd = new AzureAd(resourceGroup);
    
    // Export the primary key of the Storage Account
    return new Dictionary<string, object?>
    {
        ["tenantId"] = azureAd.TenantId
    };
});