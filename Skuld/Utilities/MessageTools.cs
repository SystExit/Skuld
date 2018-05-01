using System.Threading.Tasks;
using Discord.WebSocket;
using Skuld.Models;
using Skuld.Services;
using Discord;
using Discord.Commands;

namespace Skuld.Utilities
{
    public class MessageTools
    {
		public static async Task<CustomCommand> GetCustomCommandAsync(SocketGuild guild, string command, DatabaseService database)
		{			
			if (database.CanConnect)
			{
				var cmd = await database.GetCustomCommandAsync(guild.Id, command).ConfigureAwait(false);
				if (cmd != null) return cmd;
			}
			return null;
		}

		public static string GetPrefixFromCommand(SkuldGuild guild, string command, Tools.Config config)
		{
			if (guild != null) { if (command.StartsWith(guild.Prefix)) return guild.Prefix; }

			if (command.StartsWith(config.Discord.Prefix)) return config.Discord.Prefix;

			if (command.StartsWith(config.Discord.AltPrefix)) return config.Discord.AltPrefix;

			return null;
		}

		public static string GetCommandName(string prefix, int argpos, SocketMessage message)
		{
			string cmdname = message.Content;

			if (cmdname.StartsWith(prefix))
			{ cmdname = cmdname.Remove(argpos, prefix.Length); }

			var content = cmdname.Split(' ');

			return content[0];
		}

		public static bool IsEnabledChannel(ITextChannel channel)
		{
			if (channel == null) return true;
			if (channel.Topic == null) return true;
			if (channel.Topic.EndsWith("-commands") || channel.Topic.StartsWith("-commands")) return false;
			return true;
		}

		public static bool IsPrefixReset(ShardedCommandContext ShardCon, DiscordShardedClient client)
		{
			if (ShardCon.Message.Content.Contains($"{Bot.Configuration.Discord.Prefix}resetprefix")) return true;
			if (ShardCon.Message.Content.Contains($"{Bot.Configuration.Discord.AltPrefix}resetprefix")) return true;
			return false;
		}
	}
}
