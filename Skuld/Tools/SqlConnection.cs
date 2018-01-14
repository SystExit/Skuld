using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System;
using System.Data.Common;

namespace Skuld.Tools
{
    public class SqlConnection : Bot
    {
        private static string cs = $@"server={Bot.Configuration.SqlDBHost};user={Bot.Configuration.SqlUser};password={Bot.Configuration.SqlPass};database={Bot.Configuration.SqlDB};charset=utf8mb4";

        public static async Task<bool> InsertAsync(MySqlCommand command)
        {
            var conn = new MySqlConnection(cs);
            await conn.OpenAsync();
            if (conn.State == ConnectionState.Open)
            {
                command.Connection = conn;
                try
                {
                    await command.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                    StatsdClient.DogStatsd.Increment("mysql.queries");
                    StatsdClient.DogStatsd.Increment("mysql.insert");
                    return true;
                }
                catch (Exception ex)
                {
                    Logs.Add(new Models.LogMessage("SQL-Ins", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public readonly static MySqlConnection getconn = new MySqlConnection(cs);
        public static async Task<DbDataReader> GetAsync(MySqlCommand command)
        {
            await getconn.CloseAsync();
            await getconn.OpenAsync();
            if (getconn.State == ConnectionState.Open)
            {
                command.Connection = getconn;
                try
                {
                    StatsdClient.DogStatsd.Increment("mysql.queries");
                    var reader = await command.ExecuteReaderAsync();
                    int rows = 0;
                    rows = reader.Depth+1;
                    StatsdClient.DogStatsd.Set("mysql.rows-ret", rows);
                    return reader;
                }
                catch (Exception ex)
                {
                    Logs.Add(new Models.LogMessage("SQL-Get", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                }
            }
            return null;
        }
        public static async Task<string> GetSingleAsync(MySqlCommand command)
        {
            var conn = new MySqlConnection(cs);
            await conn.OpenAsync();
            if (conn.State == ConnectionState.Open)
            {
                command.Connection = conn;
                try
                {
                    var result = Convert.ToString(await command.ExecuteScalarAsync());
                    await conn.CloseAsync();
                    StatsdClient.DogStatsd.Increment("mysql.queries");
                    StatsdClient.DogStatsd.Set("mysql.rows-ret",1);
                    return result;
                }
                catch (Exception ex)
                {
                    Logs.Add(new Models.LogMessage("SQL-Get", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                }
            }
            return null;
        }
    }
}
