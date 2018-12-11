using Autofac;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Sanet.MagicalYatzy.XF.Helpers
{
    public static class StringsHelper
    {
        public static string Get(string text)
        {
            if (text == null)
                return "null string";

            var resourcePath = "Sanet.MagicalYatzy.Resources.Strings";
            ILocalizationService localization = App.Container.Resolve<ILocalizationService>();

            ResourceManager resourceManager = new ResourceManager(resourcePath, typeof(Die).GetTypeInfo().Assembly);
            CultureInfo ci = localization.SystemCulture;
            var resourceString = resourceManager.GetString(text, ci);

            if (resourceString == null)
            {
                resourceString = text;
            }
            return resourceString;
        }
    }
}
