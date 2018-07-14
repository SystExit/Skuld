using Discord;
using System;
using Discord.Commands;
using System.Collections.Generic;

namespace Skuld.Utilities.Discord
{
    public class DiscordUtilities
    {
		static readonly Random random = new Random();

        public static Embed GetCommandHelp(CommandService commandService, ICommandContext context, string commandname)
		{
			if (commandname.ToLower() != "pasta")
			{
                var embed = new EmbedBuilder
                {
                    Description = $"Here are some commands like **{commandname}**",
                    Color = EmbedUtils.RandomColor()
                };

                var serch = commandService.Search(context, commandname).Commands;

                var summ = GetSummaryAsync(serch);

				embed.AddField(string.Join(", ", serch[0].Command.Aliases), summ, false);

				return embed.Build();
			}
            else
            {
                var embed = new EmbedBuilder
                {
                    Description = "Here's how to do stuff with **pasta**:\n\n" +
                    "```cs\n" +
                    "   give   : Give a user your pasta\n" +
                    "   list   : List all pasta\n" +
                    "   edit   : Change the content of your pasta\n" +
                    "  change  : Same as above\n" +
                    "   new    : Creates a new pasta\n" +
                    "    +     : Same as above\n" +
                    "   who    : Gets information about a pasta\n" +
                    "    ?     : Same as above\n" +
                    "  upvote  : Upvotes a pasta\n" +
                    " downvote : Downvotes a pasta\n" +
                    "  delete  : deletes a pasta```"
                };
                //embed.WithRandomColor();
                return embed.Build();
            }
		}

		public static string GetSummaryAsync(IReadOnlyList<CommandMatch> Variants)
		{
            var primary = Variants[0];

			string summ = "Summary: " + primary.Command.Summary;
			int totalparams = 0;

			foreach (var com in Variants)
			{
				totalparams += com.Command.Parameters.Count;
			}

			if (totalparams > 0)
			{
				summ += "\nParameters:\n";

                int instance = 0;

                foreach(var cmd in Variants)
                {
                    instance++;
                    if(Variants.Count>1)
                    {
                        summ += $"**Command Version: {instance}**\n";
                    }
                    if (cmd.Command.Parameters.Count == 0)
                    {
                        summ += "No Parameters\n";
                    }
                    foreach (var param in cmd.Command.Parameters)
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
                }

				return summ;
			}
			return summ + "\nParameters: None";
		}
    }
}
