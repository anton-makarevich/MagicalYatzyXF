using Sanet.MagicalYatzy.Services;

namespace Sanet.MagicalYatzy.XF.Helpers
{
    public static class StringsHelper //TODO check where we use it?
    {
        public static string Get(string text)
        {
            var localizationService = App.Container.GetInstance<ILocalizationService>();

            return localizationService.GetLocalizedString(text);
        }
    }
}
