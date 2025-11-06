using Microsoft.CodeAnalysis.CSharp.Syntax;
using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceContainerExtension
    {
        public static IServiceCollection AddWidgetClass<TView, TViewModel>(this IServiceCollection container, WidgetName name)
            where TView: class
            where TViewModel : class, IWidgetChartViewModel
        {
            var widgetClass = new WidgetChart
            {
                Widget = name
            };
            container.AddTransient<TView>()
                .AddTransient<TViewModel>();
            container.AddSingleton<IWidgetFactory>(sp =>
            {
                widgetClass.BuildView = sp.GetRequiredService<TView>;
                widgetClass.BuildViewModel = s =>
                {
                    var viewModel = sp.GetService<TViewModel>();
                    if (viewModel != null)
                    {
                        viewModel.Script = s;

                        return viewModel;
                    }
                    else 
                    {
                        throw new InvalidOperationException($@"Cannot instantiate ViewModel { typeof(TViewModel) }");
                        
                    }
                };

                widgetClass.Widget = name;

                return widgetClass;
            });

            return container;
        }
    }
}
