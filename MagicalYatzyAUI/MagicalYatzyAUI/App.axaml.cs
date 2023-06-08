using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.DependencyInjection;
using Sanet.MagicalYatzy.Avalonia.Views;
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
        var services = Resources[Sanet.MVVM.DI.Avalonia.Extensions.AppBuilderExtensions.ServiceCollectionResourceKey] as IServiceCollection;
        services?.RegisterServices();
        services?.RegisterViewModels();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                //DataContext = new MainMenuViewModel()
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
}