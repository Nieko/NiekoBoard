using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    public class LifetimedServerErrors : IServerErrors
    {
        private object _Lock = new();
        private IDictionary<DateTime, string> _Errors = new SortedList<DateTime, string>();
        private UISettings _Settings;

        public LifetimedServerErrors(UISettings settings)
        {
            _Settings = settings;
        }

        public IServerErrors AddError(string message)
        {
            lock(_Lock)
            {
                _Errors.Add(DateTime.Now, message);
            }

            return this;
        }

        public IServerErrors AddErrors(IEnumerable<string> messages)
        {
            DateTime timestamp = DateTime.Now;

            lock (_Lock)
            {
                foreach (var message in messages)
                {
                    _Errors.Add(timestamp, message);
                }
            }

            return this;
        }

        public IList<string> GetCurrentError()
        {
            var old = DateTime.Now.Subtract(_Settings.ServerErrorLifetime);

            lock(_Lock)
            {
                var expired = _Errors
                    .Keys
                    .TakeWhile(k => k < old)
                    .ToList();

                foreach(var oldKey in expired)
                {
                    _Errors.Remove(oldKey);
                }

                return _Errors.Values.ToList();
            }
        }
    }
}
