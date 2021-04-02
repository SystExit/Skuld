using Discord;
using Discord.Commands;
using Skuld.Bot.Discord.Attributes;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Verification;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Bot.Extensions
{
	public static class HelpExtensions
	{
		public static async Task<EmbedBuilder> GetCommandHelpAsync(this CommandService commandService, ICommandContext context, string commandname, string prefix)
		{
			var search = commandService.Search(context, commandname).Commands;

			var summ = await search.GetSummaryAsync(context, prefix).ConfigureAwait(false);

			if (summ is null)
			{
				return null;
			}

			var embed = EmbedExtensions.FromMessage("Help", $"Here is a command with the name **{commandname}**", Color.Teal, context);

			embed.AddField("Attributes", summ);

			return embed;
		}

		public static EmbedBuilder GetModuleHelp(this CommandService commandService, ICommandContext context, string modulename)
		{
			var module = commandService.Modules.FirstOrDefault(x => x.Name.ToLowerInvariant() == modulename.ToLowerInvariant());
			bool didLevenstein = false;

			if (module is null)
			{
				didLevenstein = true;
				module = commandService.Modules.OrderByDescending(x => x.Name.Like(modulename)).FirstOrDefault();
			}

			if (module is null) return null;
			if (module.Commands.Count == 0) return null;

			string prefix = "";

			if (didLevenstein && module.Name.PercentageSimilarity(modulename) != 100)
			{
				prefix += "I assume you mean this module\n\n";
			}

			prefix += $"{module.Remarks}\n";

			if (!module.Group.IsNullOrWhiteSpace())
			{
				prefix += $"Prefix: {module.Group}\n\n";
			}

			var embed = EmbedExtensions.FromMessage(
				$"Help - {module.GetModulePath()}",
				$"{prefix}`{string.Join(", ", module.Commands.Select(cmd => cmd.Name))}`",
				Color.Teal,
				context);

			if (module.Submodules.Count > 0)
			{
				embed.AddField("Submodules:", $"`{string.Join(", ", module.Submodules.Select(mod => mod.Name))}`");
			}

			return embed;
		}

		public static async Task<string> GetSummaryAsync(this IReadOnlyList<CommandMatch> Variants, ICommandContext context, string prefix)
		{
			if (Variants is not null)
			{
				if (Variants.Any())
				{
					StringBuilder summ = new();

					int counter = 1;
					foreach (var variant in Variants)
					{
						summ.Append("**Variant ").Append(counter).Append("**").AppendLine();

						if (!string.IsNullOrEmpty(variant.Command.Summary))
						{
							summ.AppendLine("**Summary:**")
								.AppendLine(variant.Command.Summary)
								.AppendLine();
						}

						summ.AppendLine("**Can Execute:**")
							.AppendLine((await variant.CheckPreconditionsAsync(context).ConfigureAwait(false)).IsSuccess.ToString())
							.AppendLine();

						summ.AppendLine("**Usage:**");


						string pref = "";

						if (variant.Command.Module is not null && !variant.Command.Module.Group.IsNullOrWhiteSpace())
						{
							pref = $"{variant.Command.Module.Group} ";
						}

						if (!variant.Command.Parameters.Any() || variant.Command.Parameters.All(x => x.IsOptional))
						{
							summ.Append(prefix)
								.Append(pref)
								.Append(variant.Command.Name.ToLowerInvariant())
								.AppendLine();
						}

						foreach (var att in variant.Command.Attributes)
						{
							if (att.GetType() == typeof(UsageAttribute))
							{
								var usage = (UsageAttribute)att;

								foreach (var usg in usage.Usage)
								{
									summ.Append(prefix)
										.Append(pref)
										.Append(variant.Command.Name.ToLowerInvariant() + " ")
										.Append(usg.Replace("<@0>", context.User.Mention));

									if (usg != usage.Usage.LastOrDefault())
									{
										summ.AppendLine();
									}
								}
							}
						}

						summ.AppendLine().AppendLine();

						counter++;
					}

					return summ.ToString();
				}
			}

			return null;
		}
	}
}