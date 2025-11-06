using NiekoBoard.Windows;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.ViewModels
{
    public class StatusWidget : ViewModelBase, IWidgetChartViewModel
    {
        private string _Status = Enum.GetName(default(ScriptResult)) ?? string.Empty;
        private string _Message = string.Empty;

        public string Status
        {
            get
            {
                return _Status;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _Status, value);
            }
        }
        public string Message
        {
            get
            {
                return _Message;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _Message, value);
            }
        }
        public required IScript Script { get; set; }

        public WidgetName Widget => WidgetName.Status;

        public Task Update()
        {
            if (Script == null)
            {
                Status = nameof(ScriptResult.Fatal);
                Message = "Script object is null";

                return Task.CompletedTask;
            }

            return Task.Factory.StartNew(() =>
            {
                var status = Script.GetLatestStatus();

                if(Script.Features == ScriptFeature.ExecuteOnly)
                {
                    Message = status == null ? string.Empty: status.ToString() ?? string.Empty;
                    Status = Message == string.Empty ?
                        nameof(ScriptResult.Success) :
                        nameof(ScriptResult.Fatal);

                    return;
                }

                if(status == null)
                {
                    Message = string.Empty;
                    Status = Script.GetDefaultStatus();

                    return;
                }

                var txtStatus = status.ToString() ?? string.Empty;
                
                Message = txtStatus;
                Status = status;
            });
        }
    }
}
