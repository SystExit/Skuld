using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Reactive.Linq;
using Skuld.Services;

namespace Skuld.Modules
{
    [Group, Name("Help"), ]
    public class Help : ModuleBase<ShardedCommandContext>
    {
		public DatabaseService Database { get; set; }
		public MessageService MessageService { get; set; }

        [Command("help"), Summary("Gets all commands or a specific command's information")]
        public async Task _Help([Remainder]string command=null)
        {
			if (command == null)
			{
				string prefix = Bot.Configuration.Discord.Prefix;

				var guild = await Database.GetGuildAsync(Context.Guild.Id);
				if (guild != null)
				{
					prefix = guild.Prefix;
				}

				var embed = new EmbedBuilder
				{
					Author = new EmbedAuthorBuilder
					{
						Name = "Commands of: " + Context.Client.CurrentUser.Username + "#" + Context.Client.CurrentUser.DiscriminatorValue,
						IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
					},
					Color = Tools.Tools.RandomColor()
				};
				foreach (var module in MessageService.commandService.Modules)
				{
					if (module.Name != "Help")
					{
						string desc = "";
						foreach (var cmd in module.Commands)
						{
							var result = await cmd.CheckPreconditionsAsync(Context);
							if (result.IsSuccess)
								desc += $"{cmd.Aliases.First()}, ";
							else continue;
						}
						string description = "";
						foreach (var str in desc.Split(' ').Distinct())
							description += str + " ";
						if (!string.IsNullOrWhiteSpace(description))
							embed.AddField(module.Name, $"`{description.Remove(description.Length - 3)}`");
					}
				}
				embed.Description = $"The prefix of **{Context.Guild.Name}** is: `{prefix}`";

				await MessageService.SendDMsAsync(Context.Channel, (await Context.User.GetOrCreateDMChannelAsync()), "", embed.Build());
			}
			else
			{
				var cmd = Tools.Tools.GetCommandHelp(MessageService.commandService, Context, command);
				if(cmd==null)
				{
					await MessageService.SendChannelAsync(Context.Channel, $"Sorry, I couldn't find a command like **{command}**.");
					return;
				}
				else
				{
					await MessageService.SendChannelAsync(Context.Channel, "", cmd);
				}
			}
        }
    }
}
