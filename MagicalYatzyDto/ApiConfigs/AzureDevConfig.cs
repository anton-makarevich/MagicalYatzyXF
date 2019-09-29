namespace Sanet.MagicalYatzy.Dto.ApiConfigs
{
    public class AzureDevConfig:IApiConfig
    {
        public string BaseUrl => "https://yatzy.azure-api.net/v0/";
        public string VersionSuffix => "scores";
        public string LoginResource => "players";
    }
}