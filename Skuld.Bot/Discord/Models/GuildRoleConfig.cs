using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;

namespace Skuld.Bot.Discord.Models
{
	public class GuildRoleConfig
	{
		public ulong? Cost;
		public ulong? RequireLevel;
		public IRole RequiredRole;
		public bool? Automatic;

		public GuildRoleConfig()
		{
			Cost = 0;
			RequireLevel = 0;
			RequiredRole = null;
			Automatic = false;
		}

		public static bool FromString(string input, ICommandContext context, out GuildRoleConfig roleConfig)
		{
			roleConfig = new GuildRoleConfig();

			input = input.ToLowerInvariant();

			string[] inputsplit = input.Split(' ');

			if (inputsplit.Where(x => x.StartsWith("cost=")).Any())
			{
				if (ulong.TryParse(inputsplit.LastOrDefault(x => x.StartsWith("cost=")).Replace("cost=", ""), out ulong result))
				{
					roleConfig.Cost = result;
				}
				else
				{
					return false;
				}
			}
			else
			{
				roleConfig.Cost = 0;
			}

			if (inputsplit.Where(x => x.StartsWith("require-level=")).Any())
			{
				if (ulong.TryParse(inputsplit.LastOrDefault(x => x.StartsWith("require-level=")).Replace("require-level=", ""), out ulong result))
				{
					roleConfig.RequireLevel = result;
				}
				else
				{
					return false;
				}
			}
			else
			{
				roleConfig.RequireLevel = 0;
			}

			if (inputsplit.Where(x => x.StartsWith("require-role=")).Any())
			{
				var first = inputsplit.LastOrDefault(x => x.StartsWith("require-role="));
				if (first["require-role=".Length] == '"')
				{
					var last = inputsplit.LastOrDefault(x => x.EndsWith("\""));

					int firstIndex = 0;
					int lastIndex = 0;
					for (var x = 0; x < inputsplit.Length; x++)
					{
						if (inputsplit[x] == first)
						{
							firstIndex = x;
						}
						if (inputsplit[x] == last)
						{
							lastIndex = x;
						}
					}

					var skipped = inputsplit.Skip(firstIndex).Take(lastIndex - firstIndex);
				}
				var roleraw = inputsplit.FirstOrDefault(x => x.StartsWith("require-role=", StringComparison.InvariantCultureIgnoreCase)).Replace("require-role=", "");
				IRole role = null;
				bool gottenRole = true;

				if (MentionUtils.TryParseRole(roleraw, out ulong roleID))
				{
					role = context.Guild.GetRole(roleID);
				}
				else
				{
					gottenRole = false;
				}

				if (ulong.TryParse(roleraw, out roleID))
				{
					role = context.Guild.GetRole(roleID);
				}
				else
				{
					gottenRole = false;
				}

				if (!gottenRole)
				{
					role = context.Guild.Roles.FirstOrDefault(x => x.Name.ToLowerInvariant() == roleraw.ToLowerInvariant());
				}

				if (role is not null)
				{
					roleConfig.RequiredRole = role;
				}
				else
				{
					return false;
				}
			}
			else
			{
				roleConfig.RequiredRole = null;
			}

			if (inputsplit.Where(x => x.StartsWith("automatic=")).Any())
			{
				if (bool.TryParse(inputsplit.LastOrDefault(x => x.StartsWith("automatic=")).Replace("automatic=", ""), out bool result))
				{
					roleConfig.Automatic = result;
				}
				else
				{
					return false;
				}
			}
			else
			{
				roleConfig.Automatic = false;
			}

			return true;
		}

		public override string ToString()
		{
			StringBuilder message = new();

			if (Cost != 0)
				message.Append($"cost={Cost} ");
			if (RequireLevel != 0)
				message.Append($"require-level={RequireLevel} ");
			if (RequiredRole is not null)
				message.Append($"require-role={RequiredRole.Id} ");

			message.Append($"automatic={Automatic} ");

			return message.ToString()[0..^1];
		}
	}
}