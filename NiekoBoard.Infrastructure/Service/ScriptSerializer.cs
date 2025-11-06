using NiekoBoard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using static NiekoBoard.Service.ScriptSerializer;

namespace NiekoBoard.Service
{
    public class ScriptSerializer(Func<ScriptData, IScript> scriptBuilder)
    {
        private readonly char[] Separators = { Item, Field, StringList };
        private readonly Func<ScriptData, IScript> _ScriptBuilder = scriptBuilder;
        public enum MessageType
        {
            None = 0,
            ScriptItems = 1,
            ExecuteMethod =2,
            Result = 3,
            GetScripts = 4
        }

        public sealed class ScriptData : IScript
        {
            public string ScriptId { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public DateTime LastChanged { get; set; }

            public ScriptFeature Features { get; set; }

            public IList<string>? CustomResultsRange { get; set; }

            public IList<string>? Actions { get; set; }
            public DateTime LastPolled { get; set; }

            public IList<HistoricResult> Results { get; private set; } = new List<HistoricResult>();

            public string ExecuteAction(int actionIndex)
            {
                throw new NotImplementedException();
            }

            public string ExecuteAction()
            {
                throw new NotImplementedException();
            }

            public string GetLatestStatus()
            {
                throw new NotImplementedException();
            }
        }

        public sealed class ScriptMethod
        {
            public string MethodName { get; set; } = string.Empty;
            public int ActionIndex { get; set; } = 0;
            public DateTime LastCalled { get; set; } = DateTime.MinValue;
            public string ScriptId { get; set; } = string.Empty;
        }

        public sealed class Message
        {
            public MessageType MessageType { get; internal set; } = MessageType.None;

            public object Data { get; internal set; } = string.Empty;
        }

        private readonly List<string> _EmptyItems = new();

        private const char Item = (char)27;

        private const char Field = (char)29;

        private const char StringList = (char)31;

        private const char ListField = (char)23;

        public string Serialize(IEnumerable<IScript> scripts)
        {
            var data = new StringBuilder();

            data.Append('1').Append(Item);

            foreach (var script in scripts)
            {
                Add(data, script.ScriptId);
                Add(data, script.Name);
                Add(data, script.Description);
                Add(data, ((int)script.Features).ToString());
                AddItems(data, script.Actions);
                AddItems(data, script.CustomResultsRange);
                Add(data, script.LastChanged.ToString());
                data.Append(Item);
            }

            return data.ToString();
        }

        public string Serialize(string methodName, int actionIndex, string scriptId)
        {
            return Serialize(methodName, actionIndex, DateTime.MinValue, scriptId);
        }

        public string Serialize(string methodName, int actionIndex, DateTime lastCalled, string scriptId)
        {
            if (!new[]
            {
                nameof(IScript.GetLatestStatus),
                nameof(IScript.ExecuteAction)

            }.Contains(methodName))
            {
                throw new ArgumentException("Method name not found on " + nameof(IScript), nameof(methodName));
            }

            var data = new StringBuilder();
            data.Append('2').Append(Item);
            data.Append(methodName).Append(Item);
            data.Append(actionIndex).Append(Item);
            data.Append(lastCalled.ToString()).Append(Item);
            data.Append(scriptId).Append(Item);

            return data.ToString();
        }

        public string Serialize(string result)
        {
            var data = new StringBuilder();

            data.Append('3').Append(Item);
            data.Append(result);

            return data.ToString();
        }

        public string Serialize()
        {
            return "4" + Item + ((int)MessageType.GetScripts).ToString();
        }

        public Message Deserialize(string data)
        {
            var typeEndIndex = data.IndexOf(Item);
            var dataType = int.Parse(data.Substring(0, typeEndIndex));
            var valuesSegment = data.Substring(typeEndIndex + 1);

            // Removing any trailing script item separator
            if(dataType == 1 && valuesSegment.Length > 1 && valuesSegment[^1] == Item)
            {
                valuesSegment = valuesSegment.Substring(0, valuesSegment.Length - 1);
            }

            var values = valuesSegment.Split(Item);
            
            object? messageData = null;

            switch(dataType)
            {
                case 1:
                    {
                        messageData = DeserializeScripts(values);
                        break;
                    }
                case 2:
                    {
                        messageData = DeserializeMethod(values);
                        break;
                    }
                case 3:
                    {
                        messageData = DeserializeResult(values);
                        break;
                    }
                case 4:
                    {
                        messageData = ScriptSerializer.MessageType.GetScripts;
                        break;
                    }
            }

            if (messageData != null)
            {
                return new Message
                {
                    MessageType = (MessageType)dataType,
                    Data = messageData
                };
            }

            throw new InvalidDataException("Data Type header segment missing");
        }

        private void Add(StringBuilder data, string? value)
        {
            var formatted = (value ?? string.Empty);

            foreach(var separator in Separators)
            {
                formatted = formatted.Replace(separator, ' ');
            }

            data.Append(formatted).Append(Field);
        }
    
        private void AddItems(StringBuilder data, IEnumerable<string>? items)
        {
            data.Append((items ?? _EmptyItems)
                        .Aggregate(string.Empty, (t, c) => t + (t == string.Empty ? string.Empty : StringList) + c));
            data.Append(Field);
        }

        private List<IScript> DeserializeScripts(string[]values)
        {
            return values.Select(v =>
            {
                var fieldsData = v.Split(Field);

                return new ScriptData
                {
                    ScriptId = fieldsData[0],
                    Name = fieldsData[1],
                    Description = fieldsData[2],
                    Features = (ScriptFeature)int.Parse(fieldsData[3]),
                    Actions = fieldsData[4] == string.Empty ? null : fieldsData[4].Split(StringList)
                        .ToList(),
                    CustomResultsRange = fieldsData[5] == string.Empty ? null : fieldsData[5].Split(StringList)
                        .ToList(),
                    LastChanged = DateTime.Parse(fieldsData[6])
                };
            })
            .Select(_ScriptBuilder)
            .ToList();
        }
    
        private ScriptMethod DeserializeMethod(string[]values)
        {
            return new ScriptMethod
            {
                MethodName = values[0],
                ActionIndex = int.Parse(values[1]),
                LastCalled = DateTime.Parse(values[2]),
                ScriptId = values[3]
            };
        }

        private string DeserializeResult(string[]values)
        {
            return values[0];
        }
    }
}
