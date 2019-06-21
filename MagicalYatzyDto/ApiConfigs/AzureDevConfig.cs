namespace Sanet.MagicalYatzy.Dto.ApiConfigs
{
    public class AzureDevConfig:IApiConfig
    {
        public string BaseUrl { get; }
        public string VersionSuffix { get; }
        public string LoginResource { get; }
    }
}