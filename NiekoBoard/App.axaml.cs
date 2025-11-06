using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NiekoBoard.Composition;
using NiekoBoard.ViewModels;
using NiekoBoard.Views;
using NiekoBoard.Windows;
using System.Collections.Generic;
using NiekoBoard.Data;
using NiekoBoard.Client;
using Microsoft.Extensions.Logging;
using NiekoBoard.Logging;

namespace NiekoBoard;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var container = new ServiceCollection();

        container.AddModules(new IModule[]
        {
            new CoreModule(),
            new UIModule(),
            new ClientModule()
        }).AddLogging(logging =>
         {
             logging.ClearProviders();
#pragma warning disable CA1416 // Validate platform compatibility
             logging.AddProvider(new MemoryLogger());
#pragma warning restore CA1416 // Validate platform compatibility
         });

        var providers = container.BuildServiceProvider();

        providers.GetRequiredService<LibraryInitializer>().Initialize();
        var widgets = providers.GetRequiredService<IAvaloniaWidgetCharts>();

        widgets.AddWidget<Views.StatusWidget, ViewModels.StatusWidget>(WidgetName.Status);

        var mainViewModel = providers.GetRequiredService<IMainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();

        mainViewModel.StartUpdating();
    }
}
