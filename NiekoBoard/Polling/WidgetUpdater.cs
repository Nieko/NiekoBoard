using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NiekoBoard.Polling
{
    internal class WidgetUpdater : IChartUpdater
    {
        private IScriptConfigStore _ConfigStore;
        private UISettings _Settings;
        private IWidgetCharts _Charts;
        private IScriptSource _ScriptsSource;
        private ISet<ScriptConfig>? _Configs;
        private Dictionary<string, IScript> _Scripts;
        private Action<Action<ICollection<IWidgetChartViewModel>>> _WidgetsWork;
        private Timer? _Timer;
        private readonly object _Lock = new ();

        public event EventHandler<WidgetUpdateEventArgs> ConfigsChanged = delegate { };

        public WidgetUpdater(IScriptConfigStore configStore, UISettings settings, IWidgetCharts charts, IScriptSource scriptsSource) 
        { 
            _ConfigStore = configStore;
            _Settings = settings;
            _Charts = charts;
            _ScriptsSource = scriptsSource;
            _Scripts = new Dictionary<string, IScript>();
            _WidgetsWork = o => { };
        }

        public void Start(Action<Action<ICollection<IWidgetChartViewModel>>> widgetsWorker)
        {
            _Scripts = _ScriptsSource.GetScripts()
                .ToDictionary(s => s.Name);
            _WidgetsWork = widgetsWorker;

            RemoveTimer();

            lock (_Lock)
            {
                widgetsWorker(widgets =>
                {
                    widgets.Clear();
                    OnTimerTick(null);
                });

                _Timer = new System.Threading.Timer(OnTimerTick, null, 0, (int)_Settings.SettingsPollFrequency.TotalMilliseconds);
            }
        }
        public void Stop()
        {
            lock (_Lock)
            {
                RemoveTimer();
            }
        }

        protected void RaiseConfigsChanged(IList<ScriptConfig> removed, IList<ScriptConfig> added)
        {
            ConfigsChanged(this, new WidgetUpdateEventArgs(removed, added));
        }

        private void RemoveTimer()
        {
            if (_Timer != null)
            {
                _Timer.Dispose();
                _Timer = null;
                _Configs = null;
            }
        }

        private void OnTimerTick(object? state)
        {
            lock (_Lock)
            {
                var currentConfigs = _ConfigStore.GetConfigurations();
                var localConfigs = _Configs ?? new HashSet<ScriptConfig>();
                var removedConfigs = localConfigs
                    .Where(lc => !currentConfigs.Any(cc => lc.ScriptId == cc.ScriptId))
                    .ToList();
                var newConfigs = currentConfigs
                    .Where(cc => !localConfigs.Any(lc => lc.ScriptId == cc.ScriptId))
                    .ToList();

                if (newConfigs.Any() || removedConfigs.Any())
                {
                    UpdateWidgets(removedConfigs, newConfigs);
                    RaiseConfigsChanged(removedConfigs, newConfigs);
                }

                _Configs = new HashSet<ScriptConfig>(currentConfigs);
            }
        }

        private void UpdateWidgets(IList<ScriptConfig> removedConfigs, IEnumerable<ScriptConfig> newConfigs)
        {
            lock (_Lock)
            {
                _WidgetsWork(Widgets =>
                {
                    foreach(var added in newConfigs
                        .Where(nc => !nc.Widgets.Any()))
                    {
                        added.Widgets.Add(new ());
                    }

                    foreach (var added in newConfigs
                        .SelectMany(a => a.Widgets
                            .Select(w => new
                            {
                                Widget = w,
                                Config = a
                            })))
                    {
                        var chart = _Charts.GetChart(Enum.Parse<WidgetName>(added.Widget.WidgetName));
                        var script = _Scripts[added.Config.Script.Name];
                        var widgetViewModel = chart.BuildViewModel(script);
                        widgetViewModel.Update();

                        Widgets.Add(widgetViewModel);
                    }

                    var allRemoved = new HashSet<string>(removedConfigs
                        .Select(r => r.Script.Name));

                    foreach (var removed in Widgets
                        .Where(w => allRemoved.Contains(w.Script.Name))
                        .ToList())
                    {
                        Widgets.Remove(removed);
                    }
                });
            }
        }
    }
}
