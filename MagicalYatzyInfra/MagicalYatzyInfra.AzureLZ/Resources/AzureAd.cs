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
        var tenant = new B2CTenant("MagicalYatzyId", new B2CTenantArgs
        {
            CountryCode = "Netherlands",
            Location = "Europe",
            DisplayName = "MagicalYatzy B2C Tenant",
            ResourceName = "magicalyatzy",
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