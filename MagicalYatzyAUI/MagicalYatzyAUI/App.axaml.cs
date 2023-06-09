using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.Views;
using Sanet.MagicalYatzy.ViewModels;
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
        var services = Resources[MVVM.DI.Avalonia.Extensions.AppBuilderExtensions.ServiceCollectionResourceKey] as IServiceCollection;
        services?.RegisterServices();
        services?.RegisterViewModels();
        if (services == null)
        {
            throw new Exception("Services are not initialized");
        }
        var serviceProvider = services.BuildServiceProvider();
       
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var navigationService = new NavigationService(desktop, serviceProvider);
            RegisterViews(navigationService);
            var vm = navigationService.GetViewModel<MainMenuViewModel>();
            desktop.MainWindow = new MainWindow
            {
                Content = new MainMenuView()
                {
                    ViewModel = vm
                }
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainMenuView
            {
                //DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterViews(NavigationService navigationService)
    {
        navigationService.RegisterViewModels(typeof(MainMenuViewModel), typeof(MainMenuView));
    }
}