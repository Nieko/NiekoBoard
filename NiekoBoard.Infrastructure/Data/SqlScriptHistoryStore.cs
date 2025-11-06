using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Data
{
    public class SqlScriptHistoryStore : IScriptHistoryStore
    {
        private SqlStoreConfig _Config;

        public SqlScriptHistoryStore(SqlStoreConfig config) 
        {
            _Config = config;
        }

        public void Delete(string scriptId, HistoricResult result)
        {
            DeleteResults(scriptId, result);
        }

        public void DeleteAll(string scriptId)
        {
            DeleteResults(scriptId, null);
        }

        public ScriptHistory GetHistory(string scriptId)
        {
            ScriptHistory results = new()
            {
                ScriptId = scriptId
            };

            RunSql(cnx =>
            {
                var command = new SqlCommand
                {
                    Connection = cnx,
                    CommandText = @"SELECT r.Created, r.RawValue 
FROM Results r
INNER JOIN Scripts s ON r.ScriptId = s.Id
WHERE s.ScriptId = @ScriptId",
                    CommandType = CommandType.Text
                };

                command.Parameters.Add(new()
                {
                    ParameterName = "@ScriptId",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = scriptId
                });

                return command;
            },
            reader =>
            {
                var result = new HistoricResult
                {
                    AsAt = reader.GetDateTime(0),
                    Result = reader.GetString(1)
                };

                results.Results.Add(result);
            });

            return results;
        }

        public void Save(string scriptId, HistoricResult result)
        {
            RunSql(cnx =>
            {
                var command = new SqlCommand
                {
                    Connection = cnx,
                    CommandText = @"SaveResult",
                    CommandType = CommandType.StoredProcedure
                };

                foreach(var paramData in new[]
                {
                    new { p = "@ScriptId", t = SqlDbType.NVarChar, v = (object)scriptId },
                    new { p = "@Created", t = SqlDbType.DateTime, v = (object)result.AsAt },
                    new { p = "@RawValue", t = SqlDbType.NVarChar, v = (object)result.Result }
                })
                {
                    command.Parameters.Add(new()
                    {
                        ParameterName = paramData.p,
                        SqlDbType = paramData.t,
                        Direction = System.Data.ParameterDirection.Input,
                        Value = paramData.v
                    });
                }

                return command;
            },
            reader => { });
        }

        private void DeleteResults(string scriptId, HistoricResult? result)
        {
            RunSql(cnx =>
            {
                var command = new SqlCommand
                {
                    Connection = cnx,
                    CommandText = @"DeleteResults",
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(new()
                {
                    ParameterName = "@ScriptId",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = scriptId
                });

                if (result != null)
                {
                    command.Parameters.Add(new()
                    {
                        ParameterName = "@Created",
                        SqlDbType = System.Data.SqlDbType.DateTime,
                        Direction = System.Data.ParameterDirection.Input,
                        Value = result.AsAt
                    });
                }

                return command;
            },
            reader => { });
        }

        private void RunSql(Func<SqlConnection, SqlCommand> commandBuilder, Action<SqlDataReader> rowAction)
        {
            using (SqlConnection connection = new(_Config.ConnectionString))
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
