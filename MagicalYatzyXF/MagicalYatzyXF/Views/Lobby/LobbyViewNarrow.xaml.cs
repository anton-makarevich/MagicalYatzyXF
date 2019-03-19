﻿using Sanet.MagicalYatzy.XF.Views.Base;
using Sanet.MagicalYatzy.ViewModels;
using Xamarin.Forms.Xaml;
using Sanet.MagicalYatzy.XF.Views.Controls.TabControl;
using Sanet.MagicalYatzy.XF.Views.Fragments;

namespace Sanet.MagicalYatzy.XF.Views.Lobby
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LobbyViewNarrow : NavigationBackView<LobbyViewModel>
    {
        public LobbyViewNarrow()
        {
            InitializeComponent();
        }

        protected override void OnViewModelSet()
        {
            TabBar.TabChildren.Add(new TabItem(ViewModel.PlayersTitle, new PlayersFragment()));
            TabBar.TabChildren.Add(new TabItem(ViewModel.RulesTitle, new RulesFragment()));
        }
    }
}