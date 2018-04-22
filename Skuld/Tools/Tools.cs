using Discord;
using System.IO;
using System.Text;
using System;
using System.Resources;
using Discord.Commands;
using System.Linq;
using System.Collections.Generic;

namespace Skuld.Tools
{
    public class Tools
    {
		static readonly Random random = new Random();

        private static readonly string[] _validExtensions = { ".jpg", ".bmp", ".gif", ".png" };
        public static bool IsImageExtension(string url)
        {
            foreach (var ext in _validExtensions)
            {
                if (url.Contains(ext))
                {
                    return true;
                }
            }

            return false;
        }
        public static Color RandomColor()
        {
            var bytes = new byte[3];
            random.NextBytes(bytes);
            return new Color(bytes[0], bytes[1], bytes[2]);
        }

        public static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }
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

        public static string CheckForEmpty(string s)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                return "SKULD_GENERIC_EMPTY";
            else
                return s;
        }
        public static string CheckForEmptyWithLocale(string s, ResourceManager locale)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                return locale.GetString("SKULD_GENERIC_EMPTY");
            else
                return s;
        }

        public static string CheckForNull(string s)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                return null;
            else
                return s;
        }

		public static bool IsWebsite(string input)
		{
			if (input.Contains('.') || input.Contains("www.") || input.Contains("http://") || input.Contains("https://"))
				return true;
			return false;
		}

        public static ConsoleColor ColorBasedOnSeverity(LogSeverity sev)
        {
            if (sev == LogSeverity.Critical)
                return ConsoleColor.DarkRed;
            if (sev == LogSeverity.Error)
                return ConsoleColor.Red;
            if (sev == LogSeverity.Info)
                return ConsoleColor.Green;
            if (sev == LogSeverity.Warning)
                return ConsoleColor.Yellow;
            if (sev == LogSeverity.Verbose)
                return ConsoleColor.Cyan;
            return ConsoleColor.White;
        }

		public static Embed GetCommandHelp(CommandService commandService, ICommandContext context, string command)
		{
			if (command.ToLower() != "pasta")
			{
				var result = commandService.Search(context, command);

				if (!result.IsSuccess)
				{
					return null;
				}

				var embed = new EmbedBuilder()
				{
					Description = $"Here are some commands like **{command}**",
					Color = RandomColor()
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
			foreach (var com in Commands)
				totalparams += com.Command.Parameters.Count;

			if (totalparams > 0)
			{
				summ += "\nParameters:\n";

				foreach (var param in cmd.Parameters)
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
			return summ + "\nParameters: None";
		}
	}
}
