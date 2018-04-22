using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace Skuld.Utilities
{
    public static class EmbedCreator
    {
		static private EmbedBuilder embed = new EmbedBuilder();

		public static void SetTitle(string title)
		{
			embed.Title = title;
		}

		public static void SetDescription(string description)
		{
			embed.Description = description;
		}

		public static void AddField(string title, string content, bool inline = true)
		{
			embed.AddField(title, content, inline);
		}

		public static void SetAuthor(string author, string avatarurl, string url = "")
		{
			embed.Author = new EmbedAuthorBuilder { Name = author, IconUrl = avatarurl, Url = url };
		}

		public static void SetColor(Color color)
		{
			embed.Color = color;
		}

		public static void SetTimestamp(DateTimeOffset dateTime)
		{
			embed.Timestamp = dateTime;
		}

		public static void SetFooter(string text, string imageurl)
		{
			embed.Footer = new EmbedFooterBuilder { Text = text, IconUrl = imageurl };
		}

		public static Embed Build(bool showtimetaken = false, ulong timetaken = 0)
		{
			if(!showtimetaken)
			{
				return embed.Build();
			}
			else
			{
				if(embed.Footer!=null)
				{
					return embed.Build();
				}
				else
				{
					embed.Footer = new EmbedFooterBuilder { Text = "Command took: " + timetaken + "ms" };
					return embed.Build();
				}
			}
		}
    }
}
