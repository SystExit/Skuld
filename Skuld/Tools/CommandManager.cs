using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Skuld.Tools
{
    class CommandManager : Bot
    {
        private static string defaultPrefix = Config.Load().Prefix;
        private static string customPrefix = null;
        public static async Task Bot_MessageReceived(SocketMessage arg)
        {
            if (!arg.Author.IsBot)
            {
                var message = arg as SocketUserMessage;
                
                if (message == null) return;
                int argPos = 0;
                var chan = message.Channel as SocketGuildChannel;
                var context = new ShardedCommandContext(bot, message);
                var modules = commands.Modules;
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT prefix FROM guild WHERE ID = @guildid";
                command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                customPrefix = await Sql.GetSingleAsync(command);
                if (message.Content.ToLower() == $"{bot.CurrentUser.Username.ToLower()}.resetprefix")
                {
                    var guilduser = message.Author as SocketGuildUser;
                    Logs.Add(new Models.LogMessage("HandCmd", $"Prefix reset on guild {context.Guild.Name}", Discord.LogSeverity.Info));
                    if (guilduser.GuildPermissions.ManageGuild)
                    {
                        command = new MySqlCommand();
                        command.CommandText = "UPDATE guild SET prefix = @prefix WHERE ID = @guildid";
                        command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                        command.Parameters.AddWithValue("@prefix", defaultPrefix);
                        await Sql.InsertAsync(command).ContinueWith(async x =>
                        {
                            command = new MySqlCommand();
                            command.CommandText = "SELECT prefix FROM guild WHERE ID = @guildid";
                            command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                            string newprefix = await Sql.GetSingleAsync(command);
                            if (newprefix == defaultPrefix)
                                await MessageHandler.SendChannel(message.Channel as SocketTextChannel, $"Successfully reset the prefix, it is now `{newprefix}`");
                        });
                    }
                    else
                        await MessageHandler.SendChannel(message.Channel, "I'm sorry, you don't have permissions to reset the prefix, you need `MANAGE_SERVER` or `ADMINISTRATOR`");
                }
                await DispatchCommand(message, context, argPos, arg);
            }

        }
        private static async Task DispatchCommand(SocketUserMessage message, ShardedCommandContext context, int argPos, SocketMessage arg)
        {
            try
            { 
                if (!(message.HasStringPrefix(customPrefix, ref argPos)||message.HasStringPrefix(defaultPrefix, ref argPos))) return;
                {                    
                    await InsertCommand(customPrefix??defaultPrefix, arg);
                    string[] messagearr = arg.Content.Split(' ');
                    int prefixlength = 0;
                    if (customPrefix != null)
                        prefixlength = customPrefix.Length;
                    else
                        prefixlength = defaultPrefix.Length;
                    string msg = messagearr[0].Remove(0, prefixlength);
                    var result = await commands.ExecuteAsync(context, argPos, map);
                    if (!result.IsSuccess)
                    {
                        if(result.Error != CommandError.UnknownCommand)
                        {
                            var cmd = commands.Commands.FirstOrDefault(x => x.Name == msg);
                            Logs.Add(new Models.LogMessage("CmdHand", "Error with command, Error is: " + result, Discord.LogSeverity.Error));
                            if(result.Error == CommandError.UnmetPrecondition || result.Error == CommandError.BadArgCount)
                                await MessageHandler.SendChannel(context.Channel, "", new Discord.EmbedBuilder() { Author = new Discord.EmbedAuthorBuilder() { Name = "Error with the command" }, Description = Convert.ToString(result.ErrorReason), Color = new Discord.Color(255, 0, 0) });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logs.Add(new Models.LogMessage("CmdHand", "Error with command dispatching", Discord.LogSeverity.Error, ex));
            }

        }
        private static async Task InsertCommand(string prefix, SocketMessage arg)
        {
            int prefixlength = prefix.Length;
            var user = arg.Author;
            string[] messagearr = arg.Content.Split(' ');
            string message = messagearr[0].Remove(0,prefixlength);
            message = message.ToLower();
            foreach (var item in commands.Commands)
            {
                if (item.Name != message) { }
                else
                {
                    MySqlCommand command = new MySqlCommand();
                    command.CommandText = "SELECT * FROM commandusage WHERE UserID = @userid AND Command = @command";
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@command", message);
                    var respsql = await Sql.GetAsync(command);
                    if (respsql.HasRows)
                    {
                        while(await respsql.ReadAsync())
                        {
                            var cmd = Convert.ToString(respsql["command"]);
                            var cmdusg = Convert.ToUInt64(respsql["UserUsage"]);
                            cmdusg=cmdusg+1;
                            command = new MySqlCommand();
                            command.CommandText = "UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command";
                            command.Parameters.AddWithValue("@userusg", cmdusg);
                            command.Parameters.AddWithValue("@userid", user.Id);
                            command.Parameters.AddWithValue("@command", message);
                            await Sql.InsertAsync(command);
                        }
                        respsql.Close();
                        await Sql.getconn.CloseAsync();
                        return;
                    }
                    else
                    {
                        command = new MySqlCommand();
                        command.CommandText = "INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)";
                        command.Parameters.AddWithValue("@userusg", 1);
                        command.Parameters.AddWithValue("@userid", user.Id);
                        command.Parameters.AddWithValue("@command", message);
                        await Sql.InsertAsync(command);
                        return;
                    }
                }
            }                        
        }
    }
}
