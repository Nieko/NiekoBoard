using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Data
{
    public class SqlScriptConfigStore : IScriptConfigStore
    {
        private const char ArraySplit = (char)29;

        private SqlStoreConfig _Config;

        public SqlScriptConfigStore(SqlStoreConfig config)
        {
            _Config = config;
        }

        public void Delete(ScriptConfig config)
        {
            RunSql(cnx =>
            {
                var command = new SqlCommand
                {
                    Connection = cnx,
                    CommandText = @"DeleteScript",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter parameter = new()
                {
                    ParameterName = "@ScriptId",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = config.ScriptId
                };

                command.Parameters.Add(parameter);

                return command;
            },
            reader => { });
        }

        public IList<ScriptConfig> GetConfigurations()
        {
            List<ScriptConfig> configs = [];
            var widgets = new Dictionary<string, List<ScriptWidget>>();

            RunSql(cnx =>
            {
                var command = new SqlCommand
                {
                    Connection = cnx,
                    CommandText = @"
Select s.ScriptId,w.WidgetName,w.LastChanged,w.ConfigData,w.ConfigUI 
FROM [dbo].[Widgets] w 
INNER JOIN [Scripts] s ON s.Id = w.ScriptId",
                    CommandType = System.Data.CommandType.Text
                };

                return command;
            },
            reader =>
            {
                var scriptId = reader.GetString(0);

                if(!widgets.ContainsKey(scriptId))
                {
                    widgets[scriptId] = new ();
                }

                widgets[scriptId].Add(new ScriptWidget
                {
                    WidgetName = reader.GetString(1),
                    LastChanged = reader.GetDateTime(2),
                    ConfigData = reader.GetString(3),
                    ConfigUI = reader.GetString(4)
                });
            });

            RunSql(cnx =>
            {
                var command = new SqlCommand
                {
                    Connection = cnx,
                    CommandText = "Select ScriptId,Name,Description,LastChanged, Features, CustomResultsRange, Actions, PollingFrequency, PositionX, PositionY, PositionWidth, PositionHeight FROM dbo.Scripts",
                    CommandType = System.Data.CommandType.Text
                };

                return command;
            },
            reader => 
            {
                var scriptId = reader.GetString(0);
                var customResultsRange = reader.GetString(5);
                var actions = reader.GetString(6);
                List<ScriptWidget>? scriptWidgets = null;

                widgets.TryGetValue(scriptId, out scriptWidgets);

                var config = new ScriptConfig
                {
                    ScriptId = scriptId,
                    Script = new ScriptHeader
                    {
                        ScriptId = scriptId,
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        LastChanged = reader.GetDateTime(3),
                        Features = (ScriptFeature)reader.GetInt32(4),
                        CustomResultsRange = string.IsNullOrEmpty(customResultsRange) ? null : customResultsRange.Split(ArraySplit),
                        Actions = string.IsNullOrEmpty(customResultsRange) ? null : customResultsRange.Split(actions)
                    },
                    PollingFrequency = new TimeSpan(reader.GetInt64(7)),
                    Position = new System.Drawing.Rectangle(reader.GetInt16(8), reader.GetInt16(9), reader.GetInt16(10), reader.GetInt16(11)),
                    Widgets = scriptWidgets ?? new List<ScriptWidget>()
                };

                configs.Add(config);
            });

            return configs;
        }

        public void Save(ScriptConfig config)
        {
            Func<DateTime, SqlDateTime> floorSqlDate = dt => dt < ((DateTime)SqlDateTime.MinValue) ? SqlDateTime.MinValue : (SqlDateTime)dt;

            RunSql(cnx =>
            {
                var command = new SqlCommand
                {
                    Connection = cnx,
                    CommandText = "SaveScript",
                    CommandType = CommandType.StoredProcedure
                };

                Func<IList<string>?, object> delimit = sl =>
                {
                    return (sl ?? new List<string>()).Aggregate(string.Empty, (total, current) => total + (string.IsNullOrEmpty(total) ? string.Empty : ArraySplit.ToString()) + current);
                };

                foreach(var datum in new[]
                {
                    new { p = nameof(config.ScriptId), t = SqlDbType.NVarChar, v = (object)config.ScriptId },
                    new { p = nameof(config.Script.Name), t = SqlDbType.NVarChar, v = (object)config.Script.Name },
                    new { p = nameof(config.Script.Description), t = SqlDbType.NVarChar, v = (object)config.Script.Description },
                    new { p = nameof(config.Script.LastChanged), t = SqlDbType.DateTime, v = (object)(floorSqlDate(config.Script.LastChanged)) },
                    new { p = nameof(config.Script.Features), t = SqlDbType.Int, v = (object)(int)config.Script.Features },
                    new { p = nameof(config.Script.CustomResultsRange), t = SqlDbType.NVarChar, v = delimit(config.Script.CustomResultsRange) },
                    new { p = nameof(config.Script.Actions), t = SqlDbType.NVarChar, v = delimit(config.Script.Actions) },
                    new { p = nameof(config.PollingFrequency), t = SqlDbType.BigInt, v = (object)config.PollingFrequency.Ticks },
                    new { p = "PositionX", t = SqlDbType.SmallInt, v = (object)config.Position.X },
                    new { p = "PositionY", t = SqlDbType.SmallInt, v = (object)config.Position.Y },
                    new { p = "PositionWidth", t = SqlDbType.SmallInt, v = (object)config.Position.Width },
                    new { p = "PositionHeight", t = SqlDbType.SmallInt, v = (object)config.Position.Height }
                })
                {
                    command.Parameters.Add(new SqlParameter
                    {
                        ParameterName = datum.p,
                        SqlDbType = datum.t,
                        Direction = ParameterDirection.Input,
                        Value = datum.v
                    });
                }

                return command;
            },
            reader => { });

            foreach (var widget in config.Widgets)
            {
                RunSql(cnx =>
                {
                    var command = new SqlCommand
                    {
                        Connection = cnx,
                        CommandText = "SaveWidget",
                        CommandType = CommandType.StoredProcedure
                    };

                    foreach (var datum in new[]
                    {
                            new { p = nameof(config.ScriptId), t = SqlDbType.NVarChar, v = (object)config.ScriptId },
                            new { p = nameof(widget.WidgetName), t = SqlDbType.NVarChar, v = (object)widget.WidgetName },
                            new { p = nameof(widget.LastChanged), t = SqlDbType.DateTime, v = (object)floorSqlDate(widget.LastChanged) },
                            new { p = nameof(widget.ConfigData), t = SqlDbType.NVarChar, v = (object)widget.ConfigData},
                            new { p = nameof(widget.ConfigUI), t = SqlDbType.NVarChar, v = (object)widget.ConfigUI }
                        })
                    {
                        command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = datum.p,
                            SqlDbType = datum.t,
                            Direction = ParameterDirection.Input,
                            Value = datum.v
                        });
                    }

                    return command;
                },
                reader => { });
            }
        }

        private void RunSql(Func<SqlConnection, SqlCommand> commandBuilder, Action<SqlDataReader> rowAction)
        {
            using(SqlConnection connection = new(_Config.ConnectionString))
            {
                var command = commandBuilder(connection);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            rowAction(reader);
                        }
                    }

                    reader.Close();
                }
            }
        }
    }
}
