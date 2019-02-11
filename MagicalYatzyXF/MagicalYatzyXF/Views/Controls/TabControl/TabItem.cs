using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Views.Controls.TabControl
{
    public class TabItem
    {
        public string Title { get; set; }

        public View View { get; set; }

        public TabItem()
        {
        }

        public TabItem(string title, View view)
        {
            Title = title;
            View = view;
        }
    }
}
