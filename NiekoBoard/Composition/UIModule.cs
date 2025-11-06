using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NiekoBoard.Logging;
using NiekoBoard.Polling;
using NiekoBoard.ViewModels;
using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Composition
{
    public class UIModule : ILibraryModule
    {
        public IEnumerable<ILibraryObject> GetLibraryObjects()
        {
            return [new ServerErrors(), new ClientLogs()];
        }

        public void Register(IServiceCollection container)
        {
            container.AddSingleton<UISettings>(sp =>
            {
                var config = sp.GetService<IConfigurationRoot>();
                return config?.GetSection("UI").Get<UISettings>() ?? new UISettings();
            });

            container.AddTransient<IMainViewModel, MainViewModel>();
            container.AddSingleton(typeof(Func<WidgetName, Type, Type, IWidgetFactory>), (IServiceProvider sp) => (WidgetName name, Type viewType, Type viewModelType) =>
            {
                return new WidgetFactory
                {
                    Widget = name,
                    BuildView = () => sp.GetRequiredService(viewType),
                    BuildViewModel = s =>
                    {
                        var viewModel = (IWidgetChartViewModel)sp.GetRequiredService(viewModelType);

                        viewModel.Script = s;

                        return viewModel;
                    }
                };
            });
            container.AddSingleton<IAvaloniaWidgetCharts, WidgetCharts>();
            container.AddSingleton<IWidgetCharts>(sp => sp.GetRequiredService<IAvaloniaWidgetCharts>());

            container.AddTransient<Views.StatusWidget>()
                .AddTransient<ViewModels.StatusWidget>();

            container.AddMultiple<IUIComponentInitializer, WidgetViewModelConverterInitializer>();

            container.AddMultiple<IChartUpdater, WidgetUpdater>()
                .AddMultiple<IChartUpdater, ScriptStatusUpdater>();
            container.AddSingleton<IServerErrors, LifetimedServerErrors>()
                .AddTransient<IClientLogs, MemoryLogger>();
        }
    }
}
