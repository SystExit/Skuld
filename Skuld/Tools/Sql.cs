using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System;
using System.Data.Common;

namespace Skuld.Tools
{
    public class Sql : Bot
    {
        private static string cs = $@"server={Config.Load().SqlDBHost};user={Config.Load().SqlUser};password={Config.Load().SqlPass};database={Config.Load().SqlDB};charset=utf8mb4";
        /*public static async Task InsertAsync(string query)
        {
            MySqlConnection conn = new MySqlConnection(cs);
            await conn.OpenAsync();
            if (conn.State == ConnectionState.Open)
            {
                MySqlCommand command = new MySqlCommand(query, conn);
                try
                {
                    await command.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                }
                catch (Exception ex)
                {
                    Logs.Add(new Models.LogMessage("SQL-Ins", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                }
            }        
        }*/
        public static async Task InsertAsync(MySqlCommand command)
        {
            MySqlConnection conn = new MySqlConnection(cs);
            await conn.OpenAsync();
            if (conn.State == ConnectionState.Open)
            {
                command.Connection = conn;
                try
                {
                    await command.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                }
                catch (Exception ex)
                {
                    Logs.Add(new Models.LogMessage("SQL-Ins", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                }
            }
        }

        public static MySqlConnection getconn = new MySqlConnection(cs);
        /*public static async Task<DbDataReader> GetAsync(string query)
        {
            await getconn.CloseAsync();
            await getconn.OpenAsync();
            if (getconn.State == ConnectionState.Open)
            {
                MySqlCommand command = new MySqlCommand(query, getconn);                
                try
                {
                    return await command.ExecuteReaderAsync();
                }
                catch (Exception ex)
                {
                    Logs.Add(new Models.LogMessage("SQL-Get", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                }
            }
            return null;
        }*/
        public static async Task<DbDataReader> GetAsync(MySqlCommand command)
        {
            await getconn.CloseAsync();
            await getconn.OpenAsync();
            if (getconn.State == ConnectionState.Open)
            {
                command.Connection = getconn;
                try
                {
                    return await command.ExecuteReaderAsync();
                }
                catch (Exception ex)
                {
                    Logs.Add(new Models.LogMessage("SQL-Get", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                }
            }
            return null;
        }
        /*public static async Task<string> GetSingleAsync(string query)
        {
            MySqlConnection conn = new MySqlConnection(cs);
            await conn.OpenAsync();
            if (conn.State == ConnectionState.Open)
            {
                MySqlCommand command = new MySqlCommand(query, conn);
                try
                {
                    var result = Convert.ToString(await command.ExecuteScalarAsync());
                    await conn.CloseAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    Logs.Add(new Models.LogMessage("SQL-Get", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                }
            }
            return null;
        }*/
        public static async Task<string> GetSingleAsync(MySqlCommand command)
        {
            MySqlConnection conn = new MySqlConnection(cs);
            await conn.OpenAsync();
            if (conn.State == ConnectionState.Open)
            {
                command.Connection = conn;
                try
                {
                    var result = Convert.ToString(await command.ExecuteScalarAsync());
                    await conn.CloseAsync();
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
