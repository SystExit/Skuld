using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using MySql.Data.MySqlClient;
using System.Reactive.Linq;
using System.Collections.Generic;
using Skuld.Services;

namespace Skuld.Modules
{
    [Group, Name("Help"), ]
    public class Help : ModuleBase<ShardedCommandContext>
    {
		readonly DatabaseService database;
		readonly MessageService messageService;

		public Help(DatabaseService db,
			MessageService msgsrv)//depinj
		{
			database = db;
			messageService = msgsrv;
		}

        [Command("help", RunMode = RunMode.Async), Summary("Gets all commands or a specific command's information")]
        public async Task _Help([Remainder]string command=null)
        {
			if (command == null)
			{
				var mcmd = new MySqlCommand("select prefix from guild where id = @guildid");
				mcmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
				string resp = await database.GetSingleAsync(mcmd);

				string prefix = Bot.Configuration.Prefix;

				var embed = new EmbedBuilder
				{
					Author = new EmbedAuthorBuilder
					{
						Name = "Commands of: " + Context.Client.CurrentUser.Username + "#" + Context.Client.CurrentUser.DiscriminatorValue,
						IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
					},
					Color = Tools.Tools.RandomColor()
				};
				foreach (var module in messageService.commandService.Modules)
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
				embed.Description = $"The prefix of **{Context.Guild.Name}** is: `{resp ?? prefix}`";

				await messageService.SendDMsAsync(Context.Channel, (await Context.User.GetOrCreateDMChannelAsync()), "", embed.Build());
			}
			else
			{
				var cmd = GetCommandHelp(Context, command);
				if(cmd==null)
				{
					await messageService.SendChannelAsync(Context.Channel, $"Sorry, I couldn't find a command like **{command}**.");
					return;
				}
				else
				{
					await messageService.SendChannelAsync(Context.Channel, "", cmd);
				}
			}
        }

		public Embed GetCommandHelp(ICommandContext context, string command)
		{
			if (command.ToLower() != "pasta")
			{
				var result = messageService.commandService.Search(context, command);

				if (!result.IsSuccess)
				{					
					return null;
				}

				var embed = new EmbedBuilder()
				{
					Description = $"Here are some commands like **{command}**",
					Color = Tools.Tools.RandomColor()
				};

				var cmd = result.Commands.FirstOrDefault();

				var summ = GetSummaryAsync(cmd.Command, result.Commands, command);

				embed.AddField(x =>
				{
					x.Name = string.Join(", ", cmd.Command.Aliases);
					x.Value = summ;
					x.IsInline = false;
				});

				return embed.Build();
			}
			return null;
		}

		public static string GetSummaryAsync(CommandInfo cmd, IReadOnlyList<CommandMatch> Commands, string comm)
		{
			string summ = "Summary: " + cmd.Summary;
			int totalparams = 0;
			foreach(var com in Commands)			
				totalparams += com.Command.Parameters.Count;			

			if (totalparams > 0)
			{
				summ += "\nParameters:\n";

				foreach(var param in cmd.Parameters)
				{
					if (param.IsOptional)
					{
						summ += $"**[Optional]** {param.Name} - {param.Type.Name}\n";
					}
					else
					{
						summ += $"**[Required]** {param.Name} - {param.Type.Name}\n";
					}
				}					
				
				return summ;
			}
			return summ+"\nParameters: None";
		}
    }
}
