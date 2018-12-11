using Sanet.MagicalYatzy.XF.Helpers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.XF.MarkupExtensions
{
    [ContentProperty("Text")]
    public class StringsExtension : IMarkupExtension
    {
        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return StringsHelper.Get(Text);
        }
    }
}
