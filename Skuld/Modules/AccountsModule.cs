using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Skuld.Services;
using Skuld.Tools;
using MySql.Data.MySqlClient;
#pragma warning disable GCop126 
#pragma warning disable GCop646

namespace Skuld.Modules
{
    [Group, Name("Accounts")]

    public class Accounts : ModuleBase<ShardedCommandContext>
    {
		readonly DatabaseService database;
		readonly LoggingService logger;
		readonly MessageService messageService;

		public Accounts(DatabaseService db,
			LoggingService log,
			MessageService msgSrv) //depinj
		{
			database = db;
			logger = log;
			messageService = msgSrv;
		}

        [Command("money", RunMode = RunMode.Async), Summary("Gets a user's money"), RequireDatabase]
        public async Task GetMoney([Remainder]IUser user = null)
        {
			if (user == null)
				user = Context.User;

            var usr = await database.GetUserAsync(user.Id);

            if (Context.User == user)
            { await messageService.SendChannelAsync(Context.Channel, $"You have: {Bot.Configuration.Utils.MoneySymbol + usr.Money.ToString("N0")}"); }
            else
            { await messageService.SendChannelAsync(Context.Channel, message: $"**{user.Username}** has: {Bot.Configuration.Utils.MoneySymbol + usr.Money.ToString("N0")}"); }
        }

        [Command("profile"), Summary("Get a user's profile"), RequireDatabase]
        public async Task GetProfile([Remainder]IUser user = null)
        {
			if (user == null)
				user = Context.User;
            try
            {
                var userLocal = await database.GetUserAsync(user.Id);
                if(userLocal != null)
                {
                    var embed = new EmbedBuilder
                    {
                        Color = Tools.Tools.RandomColor(),
                        Author = new EmbedAuthorBuilder
                        {
                            Name = Context.Client.GetUser(userLocal.ID).Username,
                            IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png"
                        }
                    };
                    embed.AddField(Bot.Configuration.Utils.MoneyName, userLocal.Money.ToString("N0") ?? "No Money", inline: true);
                    embed.AddField("Luck Factor", userLocal.LuckFactor.ToString("P2") ?? "No LuckFactor",inline: true);
                    if (!string.IsNullOrEmpty(userLocal.Daily))
                    { embed.AddField("Daily", userLocal.Daily, inline: true); }
                    else
                    { embed.AddField("Daily", "Not used Daily", inline: true); }
                    if (userLocal.FavCmd != null && userLocal.FavCmdUsg != 0)
                    { embed.AddField("Favourite Command", $"`{userLocal.FavCmd}` and it has been used {userLocal.FavCmdUsg} times", inline: true); }
                    else
                    { embed.AddField("Favourite Command", "No favourite Command", inline: true); }
					embed.AddField("Description", userLocal.Description ?? "No Description", inline: false);
					await messageService.SendChannelAsync(Context.Channel, "", embed.Build());
                }
                else
                {
                    await logger.AddToLogsAsync(new Models.LogMessage("CMD-Prof", "User doesn't exist", LogSeverity.Error));
                    var msg = await messageService.SendChannelAsync(Context.Channel, "Error!! Fixing...");
                    StatsdClient.DogStatsd.Increment("commands.errors",1,1, new string[]{ "generic" });
                    await InsertUser(user).ConfigureAwait(false);
                    await msg.ModifyAsync(x => x.Content = "Try again now. ~~You may delete this message~~");
                }                
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("CMD-Prof", "Error in Profile", LogSeverity.Error, ex));
                var msg = await messageService.SendChannelAsync(Context.Channel, "Error!! Fixing...");
                StatsdClient.DogStatsd.Increment("commands.errors",1,1, new string[]{ "generic" });
                await InsertUser(user).ConfigureAwait(false);
                await msg.ModifyAsync(x=>x.Content = "Try again now. ~~You may delete this message~~");
            }            
        }

        [Command("profile-ext"), Summary("Gets extended information about a user"), RequireDatabase]
        public async Task GetProfileExt([Remainder]IUser user = null)
        {
			if (user == null)
				user = Context.User;
            try
            {
                var userLocal = await database.GetUserAsync(user.Id);
                if(userLocal != null)
                {
                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder { Name = Context.Client.GetUser(userLocal.ID).Username, IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png" },
                        Color = Tools.Tools.RandomColor()
                    };

                    embed.AddField(Bot.Configuration.Utils.MoneyName, userLocal.Money.ToString("N0") ?? "No Money", inline: true);
                    embed.AddField("Luck Factor", userLocal.LuckFactor.ToString("P2") ?? "No LuckFactor", inline: true);

                    if (!String.IsNullOrEmpty(userLocal.Daily))
                    { embed.AddField("Daily", userLocal.Daily, inline: true); }
                    else
                    { embed.AddField("Daily", "Not used Daily", inline: true); }

                    if (userLocal.Glares > 0)
                    { embed.AddField("Glares", userLocal.Glares + " times", inline: true); }
                    else
                    { embed.AddField("Glares", "Not glared at anyone", inline: true); }

                    if (userLocal.GlaredAt > 0)
                    { embed.AddField("Glared At", userLocal.GlaredAt + " times", inline: true); }
                    else
                    { embed.AddField("Glared At", "Not been glared at", inline: true); }

                    if (userLocal.Pets > 0)
                    { embed.AddField("Pets", userLocal.Pets + " times", inline: true); }
                    else
                    { embed.AddField("Pets", "Not been petted", inline: true); }

                    if (userLocal.Petted > 0)
                    { embed.AddField("Petted", userLocal.Petted + " times", inline: true); }
                    else
                    { embed.AddField("Petted", "Not petted anyone", inline: true); }

                    if (userLocal.HP > 0)
                    { embed.AddField("HP", userLocal.HP + " HP", inline: true); }
                    else
                    { embed.AddField("HP", "No HP", inline: true); }

                    if (userLocal.FavCmd != null && userLocal.FavCmdUsg != 0)
                    { embed.AddField("Favourite Command", $"`{userLocal.FavCmd}` and it has been used {userLocal.FavCmdUsg} times", inline: true); }
                    else
                    { embed.AddField("Favourite Command", "No favourite Command", inline: true); }

					embed.AddField("Description", userLocal.Description ?? "No Description", inline: false);
					await messageService.SendChannelAsync(Context.Channel, "", embed.Build());
                }
                else
                {
                    await logger.AddToLogsAsync(new Models.LogMessage("CMD-ProfExt", "User doesn't exist", LogSeverity.Error));
                    var msg = await messageService.SendChannelAsync(Context.Channel, "Error!! Fixing...");
                    StatsdClient.DogStatsd.Increment("commands.errors",1,1, new string[]{ "generic" });
                    await InsertUser(user).ConfigureAwait(false);
                    await msg.ModifyAsync(x => x.Content = "Try again now. ~~You may delete this message~~");
                }
            }
            catch(Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("CMD-ProfExt", "Error in Profile-Ext", LogSeverity.Error, ex));
                var msg = await messageService.SendChannelAsync(Context.Channel, "Error!! Fixing...");
                StatsdClient.DogStatsd.Increment("commands.errors",1,1, new string[]{ "generic" });
                await InsertUser(user).ConfigureAwait(false);
                await msg.ModifyAsync(x => x.Content = "Try again now. ~~You may delete this message~~");
            }            
        }

        [Command("description"), Summary("Sets description, if no argument is passed, cleans the description"), RequireDatabase]
        public async Task SetDescription([Remainder]string description = null)
        {
			if (description != null)
			{
				var user = await database.GetUserAsync(Context.User.Id);
				user.Description = description;
				var result = await database.UpdateUserAsync(user);
				if(result.Successful)
				{
					await messageService.SendChannelAsync(Context.Channel, $"Successfully set your description to **{description}**");
				}
				else
				{
					await messageService.SendChannelAsync(Context.Channel, $"Something happened <:blobsick:350673776071147521>");
				}
			}
			else
			{
				var user = await database.GetUserAsync(Context.User.Id);
				user.Description = "";
				var result = await database.UpdateUserAsync(user);
				if (result.Successful)
				{
					await messageService.SendChannelAsync(Context.Channel, $"Successfully cleared your description.");
				}
				else
				{
					await messageService.SendChannelAsync(Context.Channel, $"Something happened <:blobsick:350673776071147521>");
				}
			}
        }

        private async Task NewDaily(IUser user)
        {
            var suser = await database.GetUserAsync(user.Id);

            if(suser!=null)
            {
                suser.Daily = Convert.ToString(DateTime.UtcNow);
                suser.Money += Bot.Configuration.Utils.DailyAmount;
                await database.UpdateUserAsync(suser);
                await messageService.SendChannelAsync(Context.Channel, $"You got your daily of: `{Bot.Configuration.Utils.MoneySymbol + Bot.Configuration.Utils.DailyAmount}`, you now have: {Bot.Configuration.Utils.MoneySymbol}{(suser.Money.ToString("N0"))}");
            }
            else
            {
                await InsertUser(user).ConfigureAwait(false);
            }
        }
        [Command("daily"), Summary("Daily Money"), RequireDatabase]
        public async Task Daily([Remainder]IUser user = null)
		{
			if(user == null)
			{
				var suser = await database.GetUserAsync(Context.User.Id);
				if (!String.IsNullOrEmpty(suser.Daily))
				{
					var olddate = Convert.ToDateTime(suser.Daily);
					var newdate = olddate.AddHours(23).AddMinutes(59).AddSeconds(59);
					if (DateTime.Compare(newdate, DateTime.UtcNow) == 1)
					{
						var remain = olddate.AddDays(1).Subtract(DateTime.UtcNow);
						string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
						await messageService.SendChannelAsync(Context.Channel, $"You must wait `{remaining}`");
					}
					else
					{ await NewDaily(Context.User).ConfigureAwait(false); }
				}
				else
				{ await NewDaily(Context.User).ConfigureAwait(false); }
			}
			else
			{
				var csuser = await database.GetUserAsync(Context.User.Id);
				if (!String.IsNullOrEmpty(csuser.Daily))
				{
					var olddate = Convert.ToDateTime(csuser.Daily);
					var newdate = olddate.AddHours(23).AddMinutes(59).AddSeconds(59);
					if (DateTime.Compare(newdate, DateTime.UtcNow) == 1)
					{
						var remain = olddate.AddDays(1).Subtract(DateTime.UtcNow);
						string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
						await messageService.SendChannelAsync(Context.Channel, $"You must wait `{remaining}`");
					}
					else
					{
						if (user.IsBot)
						{
							await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder
							{
								Author = new EmbedAuthorBuilder
								{
									Name = "Error with the command"
								},
								Description = "Bot Accounts cannot be given any money",
								Color = new Color(255, 0, 0)
							}.Build());
							StatsdClient.DogStatsd.Increment("commands.errors",1,1, new string[]{ "generic" });
						}
						else
						{
							if (user != Context.User)
							{
								var suser = await database.GetUserAsync(user.Id);
								if (suser != null && !user.IsBot)
								{
									suser.Money += Bot.Configuration.Utils.DailyAmount;
									await database.UpdateUserAsync(suser);

									csuser.Daily = Convert.ToString(DateTime.UtcNow);
									await database.UpdateUserAsync(csuser);

									await messageService.SendChannelAsync(Context.Channel, $"Yo, you just gave {user.Username} {Bot.Configuration.Utils.MoneySymbol}{Bot.Configuration.Utils.DailyAmount}! They now have {Bot.Configuration.Utils.MoneySymbol}{suser.Money.ToString("N0")}");
								}
								else
								{
									await InsertUser(user).ConfigureAwait(false);
								}
							}
							else
							{
								await Daily();
							}
						}
					}
				}
			}
        }

        [Command("give"), Summary("Give away ur money"),RequireDatabase]
        public async Task Give(IUser user, ulong amount)
        {
            if (amount == 0)
            {
                await messageService.SendChannelAsync(Context.Channel,"Why would you want to give zero money to someone? :thinking:");
				return;
            }
			if (user != Context.User)
			{
				var oldusergive = await database.GetUserAsync(user.Id);
				if (oldusergive != null && !user.IsBot)
				{
					var oldusersend = await database.GetUserAsync(Context.User.Id);

					if (oldusersend.Money >= amount)
					{
						oldusergive.Money += amount;
						oldusersend.Money -= amount;

						var resp = await database.UpdateUserAsync(oldusergive);
						var resp2 = await database.UpdateUserAsync(oldusersend);

						if (resp.Successful && resp2.Successful)
						{
							await messageService.SendChannelAsync(Context.Channel, $"Successfully gave **{user.Username}** {Bot.Configuration.Utils.MoneySymbol + amount}");
						}
						else
						{
							await logger.AddToLogsAsync(new Models.LogMessage("DailyGive", $"{resp.Error}\t{resp2.Error}", LogSeverity.Error));
							await messageService.SendChannelAsync(Context.Channel, $"Oops, something happened. :(");
							StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
						}
					}
				}
				else if (user.IsBot)
				{
					await messageService.SendChannelAsync(Context.Channel, $"Hey, uhhh... Robots aren't supported. :(");
				}
				else
				{
					await InsertUser(user);
					await Give(user, amount).ConfigureAwait(false);
				}
			}
			else
			{
				await messageService.SendChannelAsync(Context.Channel, "<:gibeOops:350681606362759173> Can't give money to yourself... Oh, I guess you can... But why would you want to?");
			}
        }

        private async Task InsertUser(IUser user)
        {
            var result = await database.InsertUserAsync((user as Discord.WebSocket.SocketUser));
            if (!result.Successful)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("AccountModule", result.Error, LogSeverity.Error));
                await messageService.SendChannelAsync(Context.Channel, $"I'm sorry there was an issue; `{result.Error}`");
                StatsdClient.DogStatsd.Increment("commands.errors",1,1, new string[]{ "generic" });
            }
        }
    }
}
