using Discord;
using Discord.Commands;
using Miki.API.Images;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Bot.Extensions;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Extensions.Verification;
using Skuld.Models;
using Skuld.Services.Messaging.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Bot.Modules
{
	[Group, Name("Actions"), RequireEnabledModule]
	[Remarks("💥 Slap users")]
	public class ActionsModule : ModuleBase<ShardedCommandContext>
	{
		public ImghoardClient Imghoard { get; set; }

		private EmbedBuilder DoAction(string gif, string action, string target)
		{
			List<ulong> prune = new();

			{
				using SkuldDbContext Database = new SkuldDbContextFactory().CreateDbContext(null);

				if (Context.Message.MentionedUsers.Any())
				{
					foreach (var mentionedUser in Context.Message.MentionedUsers)
					{
						var res = Database.BlockedActions.FirstOrDefault(x => x.Blocker == mentionedUser.Id && x.Blockee == Context.User.Id);

						if (res is not null)
							prune.Add(mentionedUser.Id);
					}
				}
			}

			foreach (var id in prune)
			{
				target.PruneMention(id);
			}

			return new EmbedBuilder()
				.WithImageUrl(gif)
				.WithTitle(action.CapitaliseFirstLetter())
				.WithDescription(target)
				.WithRandomColor()
				.AddAuthor()
				.AddFooter(Context);
		}

		private string GetMessage(string target, string isnull, string notnull)
			=> target is null ? isnull : notnull;

		[Command("slap"), Summary("Slap a user")]
		[Usage("<@0>", "you")]
		public async Task Slap([Remainder] string target = null)
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			var action = DoAction(
				image,
				SkuldAppContext.GetCaller(),
				GetMessage(target,
					$"B-Baka.... {Context.Client.CurrentUser.Mention} slaps {Context.User.Mention}",
					$"{Context.User.Mention} slaps {target}"
				)
			);

			await action.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("stab"), Summary("Stabs a user")]
		[Usage("<@0>", "you")]
		public async Task Stab([Remainder] string target = null)
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			var action = DoAction(
				image,
				SkuldAppContext.GetCaller(),
				GetMessage(target,
					$"{Context.Client.CurrentUser.Mention} stabs {target}",
					$"{Context.User.Mention} stabs {target}"
				)
			);

			await action.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("hug"), Summary("hugs a user")]
		[Usage("<@0>", "you")]
		public async Task Hug([Remainder] string target = null)
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			var action = DoAction(
				image,
				SkuldAppContext.GetCaller(),
				GetMessage(target,
					$"{Context.Client.CurrentUser.Mention} hugs {target}",
					$"{Context.User.Mention} hugs {target}"
				)
			);

			await action.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("punch"), Summary("Punch a user")]
		[Usage("<@0>", "you")]
		public async Task Punch([Remainder] string target = null)
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			var action = DoAction(
				image,
				SkuldAppContext.GetCaller(),
				GetMessage(target,
					$"{Context.Client.CurrentUser.Mention} punches {Context.User.Mention}",
					$"{Context.User.Mention} punches {target}"
				)
			);

			await action.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("shrug"), Summary("Shrugs")]
		public async Task Shrug()
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			await
				new EmbedBuilder()
				.WithImageUrl(image)
				.WithTitle(SkuldAppContext.GetCaller())
				.WithDescription($"{Context.User.Mention} shrugs")
				.WithRandomColor()
				.AddAuthor()
				.AddFooter(Context)
			.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("adore"), Summary("Adore a user")]
		[Usage("<@0>", "you")]
		public async Task Adore([Remainder] string target = null)
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			var action = DoAction(
				image,
				SkuldAppContext.GetCaller(),
				GetMessage(target,
					$"I-it's not like I like you or anything... {Context.Client.CurrentUser.Mention} adores {Context.User.Mention}",
					$"{Context.User.Mention} adores {target}"
				)
			);

			await action.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("kiss"), Summary("Kiss a user")]
		[Usage("<@0>", "you")]
		public async Task Kiss([Remainder] string target = null)
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			var action = DoAction(
				image,
				SkuldAppContext.GetCaller(),
				GetMessage(target,
					$"I-it's not like I like you or anything... {Context.Client.CurrentUser.Mention} kisses {Context.User.Mention}",
					$"{Context.User.Mention} kisses {target}"
				)
			);

			await action.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("grope"), Summary("Grope a user")]
		[Usage("<@0>", "you")]
		public async Task Grope([Remainder] string target = null)
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			var action = DoAction(
				image,
				SkuldAppContext.GetCaller(),
				GetMessage(target,
					$"{Context.Client.CurrentUser.Mention} gropes {Context.User.Mention}",
					$"{Context.User.Mention} gropes {target}"
				)
			);

			await action.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("pet"), Summary("Pat a user"), Alias("pat", "headpat")]
		[Usage("<@0>", "you")]
		public async Task Pet([Remainder] string target = null)
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			var action =
				new EmbedBuilder()
				.WithImageUrl(image)
				.WithTitle(SkuldAppContext.GetCaller().CapitaliseFirstLetter())
				.WithRandomColor()
				.AddAuthor()
				.AddFooter(Context);

			if (target is not null)
			{
				if (Context.Message.MentionedUsers.Any())
				{
					List<ulong> prune = new();

					{
						using SkuldDbContext Database = new SkuldDbContextFactory().CreateDbContext(null);

						foreach (var mentionedUser in Context.Message.MentionedUsers)
						{
							var res = Database.BlockedActions.FirstOrDefault(x => x.Blockee == Context.User.Id && x.Blocker == mentionedUser.Id);

							if (res is not null)
								prune.Add(mentionedUser.Id);
						}
					}

					{
						using SkuldDbContext Database = new SkuldDbContextFactory().CreateDbContext(null);
						var initiator = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

						StringBuilder message = new($"{Context.User.Mention} pets ");

						var msg = target;

						foreach (var usr in Context.Message.MentionedUsers)
						{
							if (usr.IsBot || usr.IsWebhook || usr.Discriminator == "0000" || usr.DiscriminatorValue == 0 || prune.Contains(usr.Id)) continue;

							var uzr = await Database.InsertOrGetUserAsync(usr).ConfigureAwait(false);

							if (!(uzr.RecurringBlock && uzr.Patted.IsRecurring(3)))
							{
								uzr.Patted += 1;
								initiator.Pats += 1;

								message.Append(usr.Mention + " ");
							}
							else
							{
								msg.PruneMention(usr.Id);
							}
						}

						await Database.SaveChangesAsync().ConfigureAwait(false);

						if (message.ToString() != $"{Context.User.Mention} pets ")
						{
							action.WithDescription(message.ToString());
						}
						else
						{
							action.WithDescription($"{Context.Client.CurrentUser.Mention} pets {Context.User.Mention}");
						}
					}
				}
				else
				{
					action.WithDescription($"{Context.User.Mention} pets {target}");
				}
			}
			else
			{
				action.WithDescription($"{Context.Client.CurrentUser.Mention} pets {Context.User.Mention}");
			}

			await action.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("glare"), Summary("Glares at a user"), Alias("stare")]
		[Usage("<@0>", "you")]
		public async Task Stare([Remainder] string target = null)
		{
			var images = await Imghoard.GetImagesAsync(SkuldAppContext.GetCaller().LowercaseFirstLetter()).ConfigureAwait(false);

			var image = images.Images.Random().Url;

			var action = DoAction(
				image,
				SkuldAppContext.GetCaller(),
				GetMessage(target,
					$"{Context.Client.CurrentUser.Mention} glares at {Context.User.Mention}",
					$"{Context.User.Mention} glares at {target}"
				)
			);

			await action.QueueMessageAsync(Context).ConfigureAwait(false);
		}
	}
}