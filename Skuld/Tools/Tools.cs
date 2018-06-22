using Discord;
using System;
using Discord.Commands;
using System.Collections.Generic;
using Skuld.Extensions;
using SteamStoreQuery;
using Skuld.Utilities;

namespace Skuld.Tools
{
    public class Tools
    {
		static readonly Random random = new Random();
        static string BaseSteamRunUrl = "https://skuld.systemexit.co.uk/tools/steam.php?action=run&appid=";
        static string BaseSteamStoreUrl = "https://skuld.systemexit.co.uk/tools/steam.php?action=store&appid=";

        public static int ParseInt32OrDefault(string s)
        {
            if (Int32.TryParse(s, out int tmp))
                return tmp;
            else
                return 0;
        }
        public static uint ParseUInt32OrDefault(string s)
        {
            if (UInt32.TryParse(s, out uint tmp))
                return tmp;
            else
                return 0;
        }
        public static UInt64 ParseUInt64OrDefault(string input)
        {
            if (UInt64.TryParse(input, out ulong tmp))
                return tmp;
            else
                return 0;
        }
        
        public static Color HexToDiscordColor(string hex)
        {
            var col = System.Drawing.ColorTranslator.FromHtml(hex);
            return new Color(col.R, col.G, col.B);
        }
        
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
                    "  delete  : deletes a pasta```",
                    Color = EmbedUtils.RandomColor()
                };
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

        public static string GetBooruMessage(int score, string fileurl, string posturl, bool isvideo)
        {
            string message = $"`Score: {score}` <{posturl}>\n{fileurl}";

            if (isvideo)
            {
                message += "(Video)";
            }

            return message;
        }

        public static string GetSteamGameDescription(Listing game, Steam.Models.SteamStore.StoreAppDetailsDataModel appdata)
        {
            var launchurl = BaseSteamRunUrl += appdata.SteamAppId;
            var storeurl = BaseSteamStoreUrl += appdata.SteamAppId;

            var fulldesc = appdata.AboutTheGame;
            var clean = fulldesc.Replace("<br /> ", "").Replace("<br/> ", "").Replace("<br />", "").Replace("<br/>", "").Replace("<br> ", "\n").Replace("<br>", "\n");
            var split = clean.Split("\n");

            int count = 0;
            while (count < split.Length)
            {
                if (count == split.Length - 1)
                {
                    count = -1; break;
                }
                if (split[count] == "")
                {
                    count++; continue;
                }
                else
                {
                    break;
                }
            }

            var desc = split[count].StripHtml();

            string returnstring = "";
            //Description
            returnstring += desc;
            //seperator
            returnstring += "\n\n";
            //Cost
            if (!appdata.IsFree)
            {
                returnstring += "Price (USD): $" + game.PriceUSD;
            }
            else
            {
                returnstring += "Price (USD): Free";
            }
            //seperator
            returnstring += "\n\n";
            //LaunchURL
            returnstring += $"[Launch]({launchurl}) | [Store]({storeurl})";

            return returnstring;
        }
    }
}
