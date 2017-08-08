using System.Threading.Tasks;
using Discord.WebSocket;
using Skuld.Tools;
using Discord;
using System.Linq;
using System;
using MySql.Data.MySqlClient;

namespace Skuld.Events
{
    public class DiscordEvents : Bot
    {
        private static string SqlHost = Config.Load().SqlDBHost;
        public static void RegisterEvents()
        {
            /*All Events needed for running Skuld*/
            bot.MessageReceived += CommandManager.Bot_MessageReceived;
            bot.JoinedGuild += Bot_JoinedGuild;
            bot.LeftGuild += Bot_LeftGuild;
            bot.UserJoined += Bot_UserJoined;
            bot.UserLeft += Bot_UserLeft;
            bot.UserBanned += Bot_UserBanned;
            foreach(var shard in bot.Shards)
                shard.Connected += Shard_Connected;
        }

        public static void UnRegisterEvents()
        {
            bot.MessageReceived -= CommandManager.Bot_MessageReceived;
            bot.JoinedGuild -= Bot_JoinedGuild;
            bot.LeftGuild -= Bot_LeftGuild;
            bot.UserJoined -= Bot_UserJoined;
            bot.UserLeft -= Bot_UserLeft;
            bot.UserBanned -= Bot_UserBanned;
            foreach (var shard in bot.Shards)
                shard.Connected -= Shard_Connected;
        }

        private static async Task Shard_Connected()
        {
            bool[] connected = new bool[bot.Shards.Count];
            foreach (var shard in Bot.bot.Shards)
            {
                if (shard.ConnectionState == ConnectionState.Connected)
                    connected[shard.ShardId] = true;
                else
                {
                    connected[shard.ShardId] = false;
                }
            }
            string game = Bot.bot.Game.ToString();
            string shardgame = $"{Config.Load().Prefix}help | {Bot.random.Next(0, bot.Shards.Count) + 1}/{bot.Shards.Count}";
            if (!String.IsNullOrEmpty(game)) { }
            else
            {
                await bot.SetGameAsync(shardgame);
            }
        }

        //Start Users
        private static async Task Bot_UserJoined(SocketGuildUser arg)
        {
            Logs.Add(new Models.LogMessage("UsrJoin", $"User {arg.Username} joined {arg.Guild.Name}", LogSeverity.Info));
            if (!string.IsNullOrEmpty(SqlHost))
            {
                var guild = arg.Guild;
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username , \"I have no description\");";
                command.Parameters.AddWithValue("@userid", arg.Id);
                command.Parameters.AddWithValue("@username", $"{arg.Username.Replace("\"", "\\\"").Replace("\'", "\\'")}#{arg.DiscriminatorValue}");
                await Sql.InsertAsync(command);

                command = new MySqlCommand();
                command.CommandText = "select autojoinrole from guild where id = @guildid";
                command.Parameters.AddWithValue("@guildid", guild.Id);
                ulong joinroleid = Convert.ToUInt64(await Sql.GetSingleAsync(command));

                var joinrole = guild.GetRole(joinroleid);
                await arg.AddRoleAsync(joinrole);
                Logs.Add(new Models.LogMessage("UsrJoin", $"Gave User {arg.Username} the role {joinrole.Name} as per request.", LogSeverity.Info));


                command = new MySqlCommand();
                command.CommandText = "SELECT JoinMessage from guild WHERE ID = @guildid";
                command.Parameters.AddWithValue("@guildid", guild.Id);
                string welcomemessage = await Sql.GetSingleAsync(command);

                if(!String.IsNullOrEmpty(welcomemessage))
                {
                    command = new MySqlCommand();
                    command.CommandText = "SELECT UserJoinChan FROM guild WHERE ID = @guildid";
                    command.Parameters.AddWithValue("@guildid", guild.Id);
                    ulong eventchan = Convert.ToUInt64(await Sql.GetSingleAsync(command));

                    var channel = guild.GetTextChannel(eventchan);
                    welcomemessage = welcomemessage.Replace("-m", "**"+arg.Mention+"**");
                    welcomemessage = welcomemessage.Replace("-s", "**" + guild.Name + "**");
                    welcomemessage = welcomemessage.Replace("-uc", Convert.ToString(guild.MemberCount));
                    welcomemessage = welcomemessage.Replace("-u", "**" + arg.Username + "**");
                    await MessageHandler.SendChannel(channel,welcomemessage);
                }
            }                
        }
        private static async Task Bot_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            //Do something with this
        }
        private static async Task Bot_UserLeft(SocketGuildUser arg)
        {
            Logs.Add(new Models.LogMessage("UsrLeft", $"User {arg.Username} just left {arg.Guild.Name}", LogSeverity.Info));
            if (!string.IsNullOrEmpty(SqlHost))
            {
                var guild = arg.Guild;

                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT LeaveMessage from guild WHERE ID = @guildid";
                command.Parameters.AddWithValue("@guildid", guild.Id);
                string leavemessage = await Sql.GetSingleAsync(command);

                if(!String.IsNullOrEmpty(leavemessage))
                {
                    command = new MySqlCommand();
                    command.CommandText = "SELECT UserLeaveChan FROM guild WHERE ID = @guildid";
                    command.Parameters.AddWithValue("@guildid", guild.Id);
                    ulong eventchan = Convert.ToUInt64(await Sql.GetSingleAsync(command));

                    var channel = guild.GetTextChannel(eventchan);
                    leavemessage = leavemessage.Replace("-m", "**" + arg.Mention + "**");
                    leavemessage = leavemessage.Replace("-s", "**" + guild.Name + "**");
                    leavemessage = leavemessage.Replace("-uc", Convert.ToString(guild.MemberCount));
                    leavemessage = leavemessage.Replace("-u", "**" + arg.Username + "**");
                    await MessageHandler.SendChannel(channel, leavemessage);
                }
            }
        }
        //End Users

        //Start Guilds
        private static async Task Bot_LeftGuild(SocketGuild arg)
        {
            if (!string.IsNullOrEmpty(SqlHost))
            {
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "DELETE FROM guild WHERE id = @guildid";
                command.Parameters.AddWithValue("@guildid", arg.Id);
                await Sql.InsertAsync(command);
            }
            Logs.Add(new Models.LogMessage("LeftGld", $"Left {arg.Name}", LogSeverity.Info));
        }

        private static async Task Bot_JoinedGuild(SocketGuild arg)
        {
            int bots = arg.Users.Where(x => x.IsBot == true).Count();
            int humans = arg.Users.Where(x => x.IsBot == false).Count();
            int guildusers = arg.MemberCount;
            double perc = bots / guildusers;
            double ratio = Math.Round(perc, 2) * 100;
            var ratioperc = ratio + "%";
            Logs.Add(new Models.LogMessage("JoinGld",arg.Name + " has a bot-to-user-ratio of " + ratio+"%",LogSeverity.Info));
            if (ratio > 60)
            {
                Logs.Add(new Models.LogMessage("JoinGld", $"Leaving {arg.Name} most likely a Bot Farm server, ratio of Huams-to-Bots is: {ratio}$",LogSeverity.Info));
                await arg.LeaveAsync();
            }
            else
            {
                var bot = Bot.bot.CurrentUser as IGuildUser;
                if (!string.IsNullOrEmpty(SqlHost))
                    await PopulateGuilds();
            }
        }
        //End Guilds
        
        public static async Task PopulateGuilds()
        {
            int guildcount=0;
            MySqlCommand gcmd = new MySqlCommand();
            gcmd.CommandText = "INSERT IGNORE INTO `guild` (`ID`,`name`,`users`,`prefix`,`logenabled`) VALUES ";
            
            foreach (var guild in bot.Guilds)
            {
                int nonbots = guild.Users.Where(x => x.IsBot == false).Count();
                gcmd.CommandText += $"( {guild.Id} , \"{guild.Name.Replace("\"", "\\\"").Replace("\'", "\\\'")}\" , {nonbots} , \"{Config.Load().Prefix}\" , 0 ), ";
                guildcount = guildcount + 1;
            }
            if (gcmd.CommandText.Contains(Config.Load().Prefix))
            {
                if (gcmd.CommandText.EndsWith(", "))
                {
                    gcmd.CommandText = gcmd.CommandText.Substring(0, gcmd.CommandText.Length - 2);
                    Logs.Add(new Models.LogMessage("IsrtGld", $"Added {guildcount} Guild(s) to the database", LogSeverity.Info));
                    await Sql.InsertAsync(gcmd);
                    await PopulateUsers();
                }
            }
            Logs.Add(new Models.LogMessage("IsrtGld", $"Finished!", LogSeverity.Info));
            await PopulateUsers();
        }
        private static async Task PopulateUsers()
        {
            int usercount = 0;
            MySqlCommand ucmd = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES ");
            
            foreach (var guild in bot.Guilds)
            {
                foreach (var user in guild.Users)
                {
                    if (user.IsBot == false && user != null)
                    {
                        var cmd = new MySqlCommand("select ID from accounts where ID = @userid");
                        cmd.Parameters.AddWithValue("@userid", user.Id);
                        var resp = await Sql.GetAsync(cmd);
                        while (await resp.ReadAsync())
                        {
                            if (await resp.IsDBNullAsync(0))
                            {
                                ucmd.CommandText += $"( {user.Id} , {user.Username.Replace("\"", "\\\"").Replace("\'", "\\'")}#{user.DiscriminatorValue} , \"I have no description\" ), ";
                                usercount = usercount + 1;
                            }
                            else { }
                        }
                        resp.Close();
                        await Sql.getconn.CloseAsync();
                    }
                }
            }
            if (ucmd.CommandText.Contains("#"))
            {
                ucmd.CommandText = ucmd.CommandText.Substring(0, ucmd.CommandText.Length - 2);
                Logs.Add(new Models.LogMessage("IsrtUsr", $"Added {usercount} Users to the database", LogSeverity.Info));
                await Sql.InsertAsync(ucmd);
            }
            Logs.Add(new Models.LogMessage("IsrtUsr", $"Finished!", LogSeverity.Info));
        }        
    }
}
