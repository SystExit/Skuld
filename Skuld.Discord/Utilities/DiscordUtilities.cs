using Discord;
using Discord.Commands;
using Skuld.Core.Extensions;
using Skuld.Core.Utilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Skuld.Discord.Utilities
{
    public class DiscordUtilities
    {
        public const double PHI = 1.618;

        public static Regex UserMentionRegex = new Regex("<@.?[0-9]*?>");
        public static Regex RoleMentionRegex = new Regex("<&[0-9]*?>");
        public static Regex ChannelMentionRegex = new Regex("<#[0-9]*?>");

        public static Embed GetCommandHelp(CommandService commandService, ICommandContext context, string commandname)
        {
            if (commandname.ToLower() != "pasta")
            {
                var serch = commandService.Search(context, commandname).Commands;

                var summ = GetSummary(serch);

                if (summ == null)
                {
                    return null;
                }

                var embed = new EmbedBuilder
                {
                    Description = $"Here are some commands like **{commandname}**",
                    Color = EmbedUtils.RandomColor()
                };

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
                embed.Color = "#339966".FromHex();
                return embed.Build();
            }
        }

        public static string GetSummary(IReadOnlyList<CommandMatch> Variants)
        {
            if(Variants != null)
            {
                if (Variants.Count > 0)
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

                        foreach (var cmd in Variants)
                        {
                            instance++;
                            if (Variants.Count > 1)
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

            return null;
        }

        public static GuildPermissions ModeratorPermissions = new GuildPermissions(268443650);

        public static ulong GetXPLevelRequirement(ulong level, double growthmod)
            => (ulong)((level * 50) * (level * growthmod));
    }
}