using Avalonia.Threading;
using DynamicData.Binding;
using NiekoBoard.Polling;
using NiekoBoard.Windows;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NiekoBoard.ViewModels;

public class MainViewModel : ViewModelBase, IMainViewModel
{
    private bool _ComponentsInitialized = false;
    private object _Lock = new ();
    private IEnumerable<IUIComponentInitializer> _ComponentInitializer;
    private UISettings _Settings;
    private IEnumerable<IChartUpdater> _ChartUpdaters;
    private int _WidgetRows = 1;
    private int _WidgetColumns = 1;
    private CancellationTokenSource _TaskCancelToken;
    private string _ServerErrors;

    public string Title { get; private set; } = "Nieko Powerboard";

    public string ServerErrors
    {
        get => _ServerErrors;
        set =>this.RaiseAndSetIfChanged(ref _ServerErrors, value);
    }

    public int WidgetRows
    {
        get => _WidgetRows; set => this.RaiseAndSetIfChanged(ref _WidgetRows, value);

    }

    public int WidgetColumns
    {
        get => _WidgetColumns; set => this.RaiseAndSetIfChanged(ref _WidgetColumns, value);

    }

    public IList<IWidgetChartViewModel> Widgets { get; set; } = new ObservableCollection<IWidgetChartViewModel>();

    public MainViewModel(UISettings settings, IEnumerable<IChartUpdater> chartUpdaters, IEnumerable<IUIComponentInitializer> componentInitializer)
    {
        _ComponentInitializer = componentInitializer;

        if (!string.IsNullOrEmpty(settings.Title ))
        {
            Title = settings.Title;
        }

        _Settings = settings;
        _ChartUpdaters = chartUpdaters;
        ((ObservableCollection<IWidgetChartViewModel>)Widgets).CollectionChanged += OnWidgetsChanged;
    }

    public void StartUpdating()
    {
        lock (_Lock)
        {
            if (!_ComponentsInitialized)
            {
                foreach (var initializer in _ComponentInitializer)
                {
                    initializer.InitializeComponent();
                }

                _ComponentsInitialized = true;
            }
        }

        Widgets.Clear();
        
        _TaskCancelToken = new CancellationTokenSource();

        foreach (var updater in _ChartUpdaters)
        {
            updater.Start(work =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    lock (_Lock)
                    {
                        work(Widgets);
                    }
                });
            });
        }

        var lifetimeServerErrors = new ServerErrors();
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                if (_TaskCancelToken.IsCancellationRequested)
                {
                    return;
                }

                var currentErrors = lifetimeServerErrors.GetCurrentError();

                if (currentErrors.Any())
                {
                    ServerErrors = currentErrors
                        .Take(3)
                        .Aggregate(string.Empty, (total, current) => total + (string.IsNullOrEmpty(total) ? string.Empty : total) + current);
                }
                else
                {
                    ServerErrors = string.Empty;
                }

                Task.Delay(_Settings.ServerErrorLifetime / 2);
            }
        });
    }
    
    public void StopUpdating()
    {
        foreach (var updater in _ChartUpdaters)
        {
            updater.Stop();
        }

        _TaskCancelToken.Cancel();
    }

    private void OnWidgetsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var sides = decimal.Round((decimal)Math.Sqrt(Widgets.Count), 4);

        if (sides < 1)
        {
            sides = 1;
        }

        WidgetRows = (int)sides;

        if ((decimal)(int)sides == sides)
        {
            WidgetColumns = (int)sides;
        }
        else
        {
            WidgetColumns = ((int)sides + 1);
        }
    }
}
