using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Discord.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Discord.Extensions
{
    public static class HelpExtensions
    {
        public static async Task<EmbedBuilder> GetCommandHelpAsync(this CommandService commandService, ICommandContext context, string commandname, string prefix)
        {
            if (commandname.ToLower() != "pasta")
            {
                var search = commandService.Search(context, commandname).Commands;

                var summ = await search.GetSummaryAsync(commandService, context, prefix);

                if (summ == null)
                {
                    return null;
                }

                var embed = EmbedExtensions.FromMessage("Help", $"Here is a command with the name **{commandname}**", Color.Teal, context);

                embed.AddField("Attributes", summ);

                return embed;
            }
            else
            {
                var pasta = "Here's how to do stuff with **pasta**:\n\n" +
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
                    "  delete  : deletes a pasta```";

                return EmbedExtensions.FromMessage("Pasta Recipe", pasta, Color.Teal, context);
            }
        }

        public static async Task<string> GetSummaryAsync(this IReadOnlyList<CommandMatch> Variants, CommandService commandService, ICommandContext context, string prefix)
        {
            if (Variants != null)
            {
                if (Variants.Any())
                {
                    var primary = Variants[0];

                    string summ = "**Summary:**\n" + primary.Command.Summary;

                    summ += $"\n\n**Can Execute:**\n{(await primary.CheckPreconditionsAsync(context).ConfigureAwait(false)).IsSuccess}";

                    summ += "\n\n**Usage:**\n";

                    foreach(var att in primary.Command.Attributes)
                    {
                        if(att.GetType() == typeof(UsageAttribute))
                        {
                            var usage = (UsageAttribute)att;

                            summ += $"{prefix}{usage.Usage}";

                            if (att != primary.Command.Attributes.LastOrDefault(x=>x.GetType() == typeof(UsageAttribute)))
                            {
                                summ += "\n";
                            }
                        }
                    }

                    return summ;
                }
            }

            return null;
        }
    }
}