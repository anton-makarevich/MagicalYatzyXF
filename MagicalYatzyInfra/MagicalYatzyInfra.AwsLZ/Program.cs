using Pulumi;
using Pulumi.Aws.Organizations;

return await Deployment.RunAsync(() =>
{
    var organization = new Organization("sanetby");
    // Create an AWS OU
    var magicalYatzyOU = new OrganizationalUnit("MagicalYatzy", new()
    {
        Name = "MagicalYatzy",
        ParentId = organization.Id,
    });
    
});