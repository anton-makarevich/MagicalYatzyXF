using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Helpers
{
    public static class ImageHelper
    {
        public static string Get(string source)
        {
            if (source == null)
                return "null source";

            var resourceImage = source;

            if (Device.RuntimePlatform == Device.UWP)
                    resourceImage = "assets/" + resourceImage;

            return resourceImage;
        }
    }
}
