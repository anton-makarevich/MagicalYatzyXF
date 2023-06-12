using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.Views;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MVVM.Core.Services;
using Sanet.MVVM.Navigation.Avalonia.Services;
using MainWindow = Sanet.MagicalYatzy.Avalonia.Views.MainWindow;

namespace Sanet.MagicalYatzy.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Resources[MVVM.DI.Avalonia.Extensions.AppBuilderExtensions.ServiceCollectionResourceKey] is not IServiceCollection services)
        {
            throw new Exception("Game services are not initialized");
        }

        services.RegisterServices();
        services.RegisterViewModels();
        
        var serviceProvider = services.BuildServiceProvider();
       
        INavigationService navigationService;

        MainMenuViewModel? viewModel;
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                navigationService = new NavigationService(desktop, serviceProvider);
                RegisterViews(navigationService);
                viewModel = navigationService.GetViewModel<MainMenuViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    Content = new MainMenuView()
                    {
                        ViewModel = viewModel
                    }
                };
                break;
            }
            case ISingleViewApplicationLifetime singleViewPlatform:
                var mainViewWrapper = new ContentControl();
                navigationService = new SingleViewNavigationService(singleViewPlatform, mainViewWrapper, serviceProvider);
                viewModel = navigationService.GetViewModel<MainMenuViewModel>(); 
                mainViewWrapper.Content = new MainMenuView
                {
                    ViewModel = viewModel
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterViews(INavigationService navigationService)
    {
        navigationService.RegisterViews(typeof(MainMenuView), typeof(MainMenuViewModel));
    }
}