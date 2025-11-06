using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NiekoBoard.Polling
{
    public class ScriptStatusUpdater : IChartUpdater
    {
        private object _lock = new object();
        private UISettings _Settings;
        private IScriptConfigStore _ConfigStore;
        private Action<Action<ICollection<IWidgetChartViewModel>>> _WidgetsWorker;
        private Dictionary<string, TimeSpan> _UpdateFrequencies;
        private Func<DateTime, IList<IScript>> _GetDueUpdates;
        private Timer? _Timer;
        private ISet<string> _IsUpdating = new HashSet<string>();

        public event EventHandler<WidgetUpdateEventArgs> ConfigsChanged = delegate { };

        public ScriptStatusUpdater(UISettings settings, IScriptConfigStore configStore)
        {
            _Settings = settings;
            _ConfigStore = configStore;
            _WidgetsWorker = o => { };
            _UpdateFrequencies = new Dictionary<string, TimeSpan>();
            _GetDueUpdates = dt => new List<IScript>();
        }

        public void Start(Action<Action<ICollection<IWidgetChartViewModel>>> widgetsWorker)
        {
            _WidgetsWorker = widgetsWorker;
            _UpdateFrequencies = _ConfigStore.GetConfigurations()
                .ToDictionary(c => c.Script.Name, c => c.PollingFrequency);
            var pulse = _UpdateFrequencies.Values.Any() ? _UpdateFrequencies.Values.Min() : _Settings.StatusPollFrequencyDefault;

            lock (_lock)
            {
                RemoveTimer();
                SetDueUpdates();

                _Timer = new System.Threading.Timer(OnPollTick, null, 0, (int)pulse.TotalMilliseconds);
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                RemoveTimer();
            }
        }

        private void RemoveTimer()
        {
            _Timer?.Dispose();
            _Timer = null;
        }

        private void SetDueUpdates()
        {
            DateTime syncMark = DateTime.Now;

            _WidgetsWorker(widgets =>
            {
                var scriptsOrderedByNextDue = widgets
                    .Select(w => w.Script)
                    .Distinct()
                    .Select(s => new
                    {
                        NextDue = syncMark.AddTicks(_UpdateFrequencies[s.Name].Ticks),
                        Script = s
                    })
                    .OrderBy(s => s.NextDue)
                    .ToList();

                Func<int, int, int?> findOverdueIndex = (istart, iend) => default(int?);
                findOverdueIndex = (istart, iend) =>
                {
                    if(scriptsOrderedByNextDue.Count == 0)
                    {
                        return null;
                    }

                    if (scriptsOrderedByNextDue[istart].NextDue > syncMark)
                    {
                        return null;
                    }

                    if(istart == iend)
                    {
                        return iend;
                    }

                    var imid = (int)Math.Round((decimal)(istart + iend) / 2.0M, 0);
                    var lowerResult = findOverdueIndex(istart, imid);

                    if(lowerResult != null)
                    {
                        return lowerResult;
                    }

                    if(imid == iend)
                    {
                        return null;
                    }

                    return findOverdueIndex(imid + 1, iend);
                };

                _GetDueUpdates = (DateTime asAt) =>
                {
                    var dueScripts = new List<IScript>();

                    _WidgetsWorker(widgets =>
                    {
                        if(widgets.Count == 0)
                        {
                            dueScripts = new List<IScript>();
                        }

                        var overdueIndex = findOverdueIndex(0, widgets.Count - 1);

                        if(overdueIndex == null)
                        {
                            dueScripts = new List<IScript>();
                        }

                        dueScripts = widgets
                            .Take(overdueIndex??0)
                            .Select(s => s.Script)
                            .ToList();
                    });

                    return dueScripts
                        .Where(s => _IsUpdating.Contains(s.Name))
                        .ToList();
                };
            });
        }

        private void OnPollTick(object? state)
        {
            IList<IScript>? overdueScripts = null;

            lock (_lock)
            {
                overdueScripts = _GetDueUpdates(DateTime.Now);

                if (overdueScripts.Any())
                {
                    return;
                }

                foreach(var scriptName in overdueScripts
                    .Select(os => os.Name))
                {
                    _IsUpdating.Add(scriptName);
                }
            }

            foreach (var overdue in overdueScripts)
            {
                var updateTasks = new List<Task>();

                updateTasks.Add(Task.Factory.StartNew(() =>
                {
                    _WidgetsWorker(widgets =>
                    {
                        foreach (var widget in widgets
                            .Where(w => w.Script.Name == overdue.Name))
                        {
                            updateTasks.Add(widget.Update());
                        }
                    });
                }));

                Task.WhenAll(updateTasks)
                    .ContinueWith(t =>
                    {
                        lock (_lock)
                        {
                            _IsUpdating.Remove(overdue.Name);
                        }
                    });
            }
        }
    }
}
