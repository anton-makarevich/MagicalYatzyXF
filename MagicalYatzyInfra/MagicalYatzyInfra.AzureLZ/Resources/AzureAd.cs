using Pulumi;
using Pulumi.AzureNative.AzureActiveDirectory;
using Pulumi.AzureNative.AzureActiveDirectory.Inputs;
using Pulumi.AzureNative.Resources;

namespace Sanet.MagicalYatzy.Infra.Azure.LZ.Resources;

public class AzureAd
{
    public AzureAd(ResourceGroup resourceGroup)
    {
        // Create an Azure AD B2C Tenant
        var tenant = new B2CTenant("MyTenant", new B2CTenantArgs
        {
            CountryCode = "NL",
            Location = "europe",
            DisplayName = "MagicalYatzy B2C Tenant",
            ResourceGroupName = resourceGroup.Name,
            Sku = new B2CResourceSKUArgs
            {
                Name = "Standard",
                Tier = "A0",
            }
        });

        TenantId = tenant.TenantId;
    }

    [Output]
    public Output<string?> TenantId { get; private set; }
}