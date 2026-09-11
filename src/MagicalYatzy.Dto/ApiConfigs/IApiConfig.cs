namespace Sanet.MagicalYatzy.Dto.ApiConfigs
{
    public interface IApiConfig
    {
        string BaseUrl { get; }
        string VersionSuffix { get; }
        string LoginResource { get; }
    }
}