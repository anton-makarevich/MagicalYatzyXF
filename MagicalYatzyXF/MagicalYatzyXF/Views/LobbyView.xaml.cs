﻿using Sanet.MagicalYatzy.XF.Views.Base;
using Sanet.MagicalYatzy.ViewModels;
using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.XF.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LobbyView : NavigationBackView<LobbyViewModel>
    {
        public LobbyView()
        {
            InitializeComponent();
        }
    }
}
