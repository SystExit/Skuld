using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using IqdbApi;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Skuld.API;
using Skuld.APIS;
using Skuld.APIS.Animals.Models;
using Skuld.APIS.NekoLife.Models;
using Skuld.APIS.WebComics.CAD.Models;
using Skuld.APIS.WebComics.Explosm.Models;
using Skuld.APIS.WebComics.XKCD.Models;
using Skuld.APIS.Wikipedia.Models;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Bot.Extensions;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Conversion;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Helpers;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services;
using Skuld.Services.Globalization;
using Skuld.Services.Messaging.Extensions;
using StatsdClient;
using SysEx.Net;
using SysEx.Net.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TimeZoneConverter;
using WenceyWang.FIGlet;

namespace Skuld.Bot.Commands
{
	[Group, Name("Fun"), RequireEnabledModule]
	[Remarks("🕹 Entertainment commands")]
	public class FunModule : InteractiveBase<ShardedCommandContext>
	{
		public SkuldConfig Configuration { get; set; }
		public AnimalClient Animals { get; set; }
		public Locale Locale { get; set; }
		public WebComicClients ComicClients { get; set; }
		public SysExClient SysExClient { get; set; }
		public YNWTFClient YNWTFcli { get; set; }
		public NekosLifeClient NekoLife { get; set; }
		public IqdbClient IqdbClient { get; set; }
		public WikipediaClient WikiClient { get; set; }
		public ISkuldAPIClient ApiClient { get; set; }

		private static readonly string[] eightball = {
			"SKULD_FUN_8BALL_YES1",
			"SKULD_FUN_8BALL_YES2",
			"SKULD_FUN_8BALL_YES3",
			"SKULD_FUN_8BALL_YES4",
			"SKULD_FUN_8BALL_YES5",
			"SKULD_FUN_8BALL_YES6",
			"SKULD_FUN_8BALL_YES7",
			"SKULD_FUN_8BALL_YES8",
			"SKULD_FUN_8BALL_YES9",
			"SKULD_FUN_8BALL_YES10",
			"SKULD_FUN_8BALL_MAYBE1",
			"SKULD_FUN_8BALL_MAYBE2",
			"SKULD_FUN_8BALL_MAYBE3",
			"SKULD_FUN_8BALL_MAYBE4",
			"SKULD_FUN_8BALL_MAYBE5",
			"SKULD_FUN_8BALL_NO1",
			"SKULD_FUN_8BALL_NO2",
			"SKULD_FUN_8BALL_NO3",
			"SKULD_FUN_8BALL_NO4",
			"SKULD_FUN_8BALL_NO5"
		};

		private readonly string PokemonUrl =
			"http://images.alexonsager.net/pokemon/fused/{1}/{1}.{2}.png";

		private readonly StringComparison Comparison =
			StringComparison.InvariantCultureIgnoreCase;

		[Command("fuse")]
		[Summary("Fuses 2 of the 1st generation pokemon")]
		[Usage("5 96")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Fuse(int int1, int int2)
		{
			if (int1 > 151 || int1 < 0)
			{
				await
					EmbedExtensions
						.FromError($"{int1} over/under limit. (151)", Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
			else if (int2 > 151 || int2 < 0)
			{
				await
					EmbedExtensions
						.FromError($"{int2} over/under limit. (151)", Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
			else
			{
				var url = PokemonUrl
					.Replace("{1}", Convert.ToString(int1, null), Comparison)
					.Replace("{2}", Convert.ToString(int2, null), Comparison);
				await
					EmbedExtensions
						.FromMessage(Context)
						.WithImageUrl(url)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		#region Reminders

		[Group("reminder"), RequireDatabase]
		[Remarks("⏰ Reminders")]
		public class Reminders : InteractiveBase<ShardedCommandContext>
		{
			[
				Command,
				Summary("What was that thing I needed to do again?"),
				Usage(
					"Get eggs in 1h 30m", "Get eggs in 1h 30m --repeat",
					"dab on the haters in 2d 1h 30m 5s",
					"dab on the haters in 2d 1h 30m 5s --repeat"
				),
				Ratelimit(20, 1, Measure.Minutes)
			]
			public async Task Reminder([Remainder] string Reminder)
			{
				if (Reminder.StartsWith("me to"))
				{
					Reminder = Reminder.Replace("me to", "");
				}
				if (Reminder.StartsWith("me"))
				{
					Reminder = Reminder.Replace("me", "");
				}

				bool doesRepeat = false;

				if (Reminder.Contains("--repeat"))
				{
					doesRepeat = true;
					Reminder = Reminder.Replace("--repeat", "");
				}

				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (Reminder.Split(" in ").Length < 2)
				{
					string Prefix = Context.IsPrivate ? SkuldApp.MessageServiceConfig.Prefix : (await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false)).Prefix;
					await
						EmbedExtensions.FromError($"Try using `{Prefix}help reminder` to learn how to format a reminder", Context)
						.QueueMessageAsync(Context)
						.ConfigureAwait(false);
					return;
				}

				DateTime time;
				if (StringTimeHelper.TryParse(Reminder.Split(" in ")[1], out TimeSpan? output))
				{
					if (output.Value.TotalSeconds < TimeSpan.FromMinutes(10).TotalSeconds)
					{
						await
							EmbedExtensions.FromError($"Cannot queue reminders of less than 10 minutes", Context)
							.QueueMessageAsync(Context)
							.ConfigureAwait(false);
						return;
					}
					time = DateTime.UtcNow + output.Value;
				}
				else
				{
					await
						EmbedExtensions.FromError("Can't process time input, try again", Context)
						.QueueMessageAsync(Context)
						.ConfigureAwait(false);
					return;
				}

				if (Database.Reminders.ToList().Where(x => x.UserId == Context.User.Id).Count() == ushort.MaxValue)
				{
					await
						EmbedExtensions.FromError("You have reached the maximum allowed reminders", Context)
						.QueueMessageAsync(Context)
						.ConfigureAwait(false);
					return;
				}

				ushort localId = (ushort)SkuldRandom.Next(0, ushort.MaxValue);

				while (Database.Reminders.ToList()
					.Any(x => x.LocalId == localId && x.UserId == Context.User.Id)
				)
				{
					localId = (ushort)SkuldRandom.Next(0, ushort.MaxValue);
				}

				Database.Reminders.Add(new ReminderObject
				{
					Created = DateTime.Now.ToEpoch(),
					Content = Reminder.Split(" in ")[0],
					LocalId = localId,
					Timeout = time.ToEpoch(),
					MessageLink = Context.Message.GetJumpUrl(),
					UserId = Context.User.Id,
					Repeats = doesRepeat
				});

				await Database.SaveChangesAsync().ConfigureAwait(false);

				var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

				var timeLocalised = TimeZoneInfo.ConvertTimeFromUtc(time, TZConvert.GetTimeZoneInfo(user.TimeZone ?? "UTC"));

				await
					EmbedExtensions.FromSuccess($"Got it!! I'll remind you at: {timeLocalised.ToDMYString()}. Your reminder is #`{localId}`", Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[Command("list")]
			[Summary("Gets your current reminders")]
			[Ratelimit(20, 1, Measure.Minutes)]
			public async Task ReminderList()
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var reminders = Database.Reminders.ToList().Where(x => x.UserId == Context.User.Id);

				EmbedBuilder builder = new();

				builder.AddAuthor(Context.Client);
				builder.AddFooter(Context);

				StringBuilder message = new();

				foreach (ReminderObject reminder in reminders)
				{
					message.AppendLine($"`{reminder.LocalId}` | Created: {reminder.Created.FromEpoch().ToDMYString()} | Expires in: `{(reminder.Timeout.FromEpoch() - DateTime.UtcNow).ToDifferenceString()}`");
				}

				if (message.Length > 0)
				{
					builder.WithDescription(message.ToString());
				}
				else
				{
					builder.WithDescription("You have no reminders. <:blobcrying:662304318531305492>");
				}

				await
					builder
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[Command("delete")]
			[Summary("Deletes a reminder")]
			[Usage("1234")]
			[RequireDatabase]
			[Ratelimit(20, 1, Measure.Minutes)]
			public async Task DelReminder(ushort reminderId)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (Database.Reminders.Any(x => x.LocalId == reminderId && x.UserId == Context.User.Id))
				{
					Database.Reminders.Remove(Database.Reminders.ToList().FirstOrDefault(x => x.LocalId == reminderId && x.UserId == Context.User.Id));

					await Database.SaveChangesAsync().ConfigureAwait(false);

					await
						EmbedExtensions.FromSuccess($"Deleted your reminder `{reminderId}`", Context)
						.QueueMessageAsync(Context)
						.ConfigureAwait(false);
				}
				else
				{
					await
						EmbedExtensions.FromInfo($"You don't have a reminder with the id `{reminderId}`", Context)
						.QueueMessageAsync(Context)
						.ConfigureAwait(false);
				}
			}
		}

		#endregion Reminders

		#region WeebAnimals

		[Command("neko")]
		[Summary("neko grill")]
		[Disabled(true, true, "TOS Concerns")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Neko()
		{
			var neko = await NekoLife.GetAsync(NekoImageType.Neko).ConfigureAwait(false);
			DogStatsd.Increment("web.get");
			if (neko is not null) await EmbedExtensions.FromMessage(Context).WithImageUrl(neko).QueueMessageAsync(Context).ConfigureAwait(false);
			else await EmbedExtensions.FromError("Hmmm <:Thunk:350673785923567616> I got an empty response.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("kitsune")]
		[Summary("Kitsunemimi Grill")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Kitsune()
		{
			var kitsu = await SysExClient.GetKitsuneAsync().ConfigureAwait(false);
			DogStatsd.Increment("web.get");
			await EmbedExtensions.FromMessage(Context).WithImageUrl(kitsu).QueueMessageAsync(Context).ConfigureAwait(false);
		}

		#endregion WeebAnimals

		#region Animals

		[Command("kitty")]
		[Summary("kitty")]
		[Ratelimit(20, 1, Measure.Minutes)]
		[Alias("cat", "cats", "kittycat", "kitty cat", "meow", "kitties", "kittys")]
		public async Task Kitty()
		{
			var kitty = await Animals.GetAnimalAsync(AnimalType.Kitty).ConfigureAwait(false);
			DogStatsd.Increment("web.get");

			if (kitty.IsVideoFile())
			{
				await kitty.QueueMessageAsync(Context).ConfigureAwait(false);
			}
			if (kitty == "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg")
			{
				await
					EmbedExtensions.FromImage(kitty, Color.Red, Context)
					.QueueMessageAsync(Context, content: "Both the api's are down, that makes the sad a big sad. <:blobcry:350681079415439361>")
				.ConfigureAwait(false);
			}
			else
			{
				await
					EmbedExtensions.FromImage(kitty, EmbedExtensions.RandomEmbedColor(), Context).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		[Command("doggo")]
		[Summary("doggo")]
		[Alias("dog", "dogs", "doggy")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Doggo()
		{
			var doggo = await Animals.GetAnimalAsync(AnimalType.Doggo).ConfigureAwait(false);
			DogStatsd.Increment("web.get");
			if (doggo.IsVideoFile())
			{
				await doggo.QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}
			if (doggo == "https://i.imgur.com/ZSMi3Zt.jpg")
			{
				await
					EmbedExtensions.FromImage(doggo, Color.Red, Context)
					.QueueMessageAsync(
						Context,
						content: "Both the api's are down, that makes the sad a big sad. <:blobcry:350681079415439361>"
					)
				.ConfigureAwait(false);
			}
			else
			{
				await
					EmbedExtensions.FromImage(doggo, EmbedExtensions.RandomEmbedColor(), Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		[Command("bird")]
		[Summary("birb")]
		[Alias("birb")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Birb()
		{
			var birb = await Animals.GetAnimalAsync(AnimalType.Bird).ConfigureAwait(false);
			DogStatsd.Increment("web.get");
			if (birb.IsVideoFile())
			{
				await birb.QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				await
					EmbedExtensions.FromImage(birb, Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		[Command("llama"), Summary("Llama"), Ratelimit(20, 1, Measure.Minutes)]
		public async Task Llama()
		{
			var llama = await SysExClient.GetLlamaAsync().ConfigureAwait(false);
			DogStatsd.Increment("web.get");
			await EmbedExtensions.FromImage(llama, Context).QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("seal"), Summary("Seal"), Ratelimit(20, 1, Measure.Minutes)]
		public async Task Seal()
		{
			var seal = await SysExClient.GetSealAsync().ConfigureAwait(false);
			DogStatsd.Increment("web.get");
			await EmbedExtensions.FromImage(seal, Context).QueueMessageAsync(Context).ConfigureAwait(false);
		}

		#endregion Animals

		#region RNG

		[Command("eightball"), Summary("Eightball")]
		[Alias("8ball")]
		[Usage("will today be good")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Eightball([Remainder] string question = null)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();
			var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			var answer = Locale.GetLocale(user.Language).GetString(eightball[SkuldRandom.Next(eightball.Length)], CultureInfo.InvariantCulture);

			var message = "";

			if (!string.IsNullOrEmpty(question))
				message = $"Your Question:\n{question}\n\n";

			message += $"And the :8ball: says:\n{answer}";

			await
				EmbedExtensions
				.FromMessage(message, Context)
				.QueueMessageAsync(
					Context,
					type: Services.Messaging.Models.MessageType.Mention
				)
			.ConfigureAwait(false);
		}

		[Command("roll"), Summary("Roll a die")]
		[Usage("5")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Roll(ulong roll)
		{
			await
				EmbedExtensions
				.FromMessage(SkuldAppContext.GetCaller(),
							 $"{Context.User.Mention} just rolled and got a {SkuldRandom.Next(1, (roll + 1))}",
							 Color.Teal,
							 Context)
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[Command("choose"), Summary("Choose from things")]
		[Usage("\"reading books\" \"playing games\"")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Choose(params string[] choices)
		{
			if (choices.Any())
			{
				var choice = choices[SkuldRandom.Next(0, choices.Length)];

				int critereon = 0;
				int maxCritereon = 3;
				while (choice.IsNullOrWhiteSpace())
				{
					if (critereon <= maxCritereon)
						break;

					choice = choices[SkuldRandom.Next(0, choices.Length)];
					critereon++;
				}

				if (choice.IsNullOrWhiteSpace())
				{
					await
						EmbedExtensions.FromError("Couldn't choose a viable result. Please verify input and try again", Context)
						.QueueMessageAsync(Context)
						.ConfigureAwait(false);
				}
				else
				{
					await
						new EmbedBuilder()
							.AddAuthor(Context.Client)
							.AddFooter(Context)
							.WithDescription($"I choose: **{choice}**")
							.WithThumbnailUrl("https://cdn.discordapp.com/emojis/350673785923567616.png")
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
			}
			else
			{
				await
					EmbedExtensions.FromError("Please give me a choice or two ☹☹", Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		[
			Command("yn"),
			Summary(
				"Yes, no, maybe. I don't know, can you repeat the question?"
			),
			Usage("Will it rain today?"),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task YN([Remainder] string question = null)
		{
			var YNResp = await YNWTFcli.AskYNWTF().ConfigureAwait(false);

			var lowered = YNResp.Answer.ToLowerInvariant();

			var message = "";

			if (!string.IsNullOrEmpty(question))
				message = $"Your Question:\n{question}\n\n";

			message += $"I'd say {YNResp.Answer}";

			if (lowered != "yes" && lowered != "no")
				message += "¯\\_(ツ)_/¯";

			await
				EmbedExtensions
					.FromImage(
						YNResp.Image,
						message.CapitaliseFirstLetter(),
						Context
					)
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		#endregion RNG

		#region Pasta

		[
			Command("pasta"),
			Summary("Pastas are nice"),
			RequireDatabase,
			Usage(
				"new naenae dab",
				"edit naenae watch me whip",
				"who naenae",
				"upvote naenae",
				"downvote naenae",
				"delete naenae",
				"naenae",
				"help",
				"list"
			),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task Pasta(
			string cmd,
			string title = null,
			[Remainder] string content = null
		)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			Pasta pasta = null;
			if (cmd is not null)
			{
				pasta = PastaManager.GetPasta(cmd);
			}
			if (title is not null)
			{
				pasta = PastaManager.GetPasta(title);
			}

			string prefix = Configuration.Prefix;
			if (Context.Guild is not null)
			{
				prefix = (await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false)).Prefix;
			}

			switch (cmd.ToLowerInvariant())
			{
				case "new":
				case "+":
				case "create":
					{
						switch (title.ToLowerInvariant())
						{
							case "list":
							case "help":
								{
									await EmbedExtensions.FromError($"Cannot create a Pasta with the name: {title}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
									DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
								}
								break;

							default:
								{
									if (pasta is not null)
									{
										await EmbedExtensions.FromError($"Pasta already exists with name: **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
										DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
									}
									else
									{
										if (Context.Message.Attachments.Any())
										{
											foreach (var att in Context.Message.Attachments)
											{
												content += $"\n{att.Url}";
											}
										}

										Database.Pastas.Add(new Pasta
										{
											OwnerId = Context.User.Id,
											Name = title,
											Content = content,
											Created = DateTime.UtcNow.ToEpoch()
										});

										await Database.SaveChangesAsync().ConfigureAwait(false);

										if (Database.Pastas.FirstOrDefault(x => x.Name.IsSameUpperedInvariant(title)) is not null)
										{
											await EmbedExtensions.FromSuccess($"Added: **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
										}
									}
								}
								break;
						}
					}
					break;

				case "edit":
				case "change":
				case "modify":
					{
						if (pasta is not null)
						{
							if (pasta.IsOwner(user))
							{
								content = content.Replace("\'", "\\\'");
								content = content.Replace("\"", "\\\"");

								pasta.Content = content;

								await Database.SaveChangesAsync().ConfigureAwait(false);

								await EmbedExtensions.FromSuccess($"Changed the content of **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							}
							else
							{
								DogStatsd.Increment("commands.errors", 1, 1, new string[] { "unm-precon" });
								await EmbedExtensions.FromError("I'm sorry, but you don't own the Pasta", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							}
						}
						else
						{
							DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
							await $"Whoops, `{title}` doesn't exist".QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
					break;

				case "who":
				case "?":
					{
						if (pasta is not null)
						{
							var embed = new EmbedBuilder
							{
								Color = EmbedExtensions.RandomEmbedColor(),
								Title = pasta.Name
							};

							var usr = Context.Client.GetUser(pasta.OwnerId);

							if (usr is not null)
							{
								embed.AddField("Creator", usr.Mention, inline: true);
							}
							else
							{
								embed.AddField("Creator", $"Unknown User ({pasta.OwnerId})");
							}

							embed.AddField("Created", pasta.Created.FromEpoch().ToString(new CultureInfo((await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false)).Language)), inline: true);
							embed.AddField("UpVotes", ":arrow_double_up: " + Database.PastaVotes.ToList().Count(x => x.PastaId == pasta.Id && x.Upvote));
							embed.AddField("DownVotes", ":arrow_double_down: " + Database.PastaVotes.ToList().Count(x => x.PastaId == pasta.Id && !x.Upvote));

							await embed.QueueMessageAsync(Context).ConfigureAwait(false);
						}
						else
						{
							DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
							await EmbedExtensions.FromError($"Pasta `{title}` doesn't exist. :/ Sorry.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
					break;

				case "upvote":
					{
						if (pasta is not null)
						{
							var successful = await PastaManager.AddUpvoteAsync(pasta, user);

							if (successful)
							{
								await EmbedExtensions.FromSuccess("Pasta Kitchen", "Successfully casted your vote", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							}
							else
							{
								await EmbedExtensions.FromError("Pasta Kitchen", $"You have already voted for \"{pasta.Name}\"", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							}
						}
						else
						{
							DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
							await EmbedExtensions.FromError($"Pasta `{title}` doesn't exist. :/ Sorry.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
					break;

				case "downvote":
					{
						if (pasta is not null)
						{
							var successful = await PastaManager.AddDownvoteAsync(pasta, user);

							if (successful)
							{
								await EmbedExtensions.FromSuccess("Pasta Kitchen", "Successfully casted your vote", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							}
							else
							{
								await EmbedExtensions.FromError("Pasta Kitchen", $"You have already voted for \"{pasta.Name}\"", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							}
						}
						else
						{
							DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
							await EmbedExtensions.FromError($"Pasta `{title}` doesn't exist. :/ Sorry.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
					break;

				case "delete":
					{
						if (pasta is not null)
						{
							var successful = await PastaManager.DeletePastaAsync(pasta, user);

							if (successful)
							{
								await EmbedExtensions.FromSuccess($"Successfully deleted: **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							}
							else
							{
								await EmbedExtensions.FromError($"Couldn't delete this pasta, are you sure you own it?", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							}
						}
						else
						{
							DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
							await EmbedExtensions.FromError($"Pasta `{title}` doesn't exist. :/ Sorry.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
					break;

				case "list":
					{
						var pastas = PastaManager.GetPastas();
						if (pastas.Any())
						{
							string pastaNames = string.Join(", ", pastas.Select(p => p.Name));

							StringBuilder sb = new("I found:\n```\n");
							sb.AppendLine(pastaNames);
							sb.AppendLine("```");

							string pastaMessage = sb.ToString();

							if (pastaMessage.Length < 2000)
							{
								await pastaMessage.QueueMessageAsync(Context).ConfigureAwait(false);
							}
							else
							{
								var stream = pastaNames.ToStream();

								await $"Here's a list".QueueMessageAsync(Context, fileStream: stream, fileName: "pastas.txt").ConfigureAwait(false);
							}
						}
						else
						{
							await EmbedExtensions.FromError("No pastas exist", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
					break;

				case "help":
					{
						await (await SkuldApp.CommandService.GetCommandHelpAsync(Context, "pasta", prefix).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
					}
					break;

				case "search":
					{
						var names = Database.Pastas.ToList().Select(x => x.Name).ToList();

						Dictionary<string, int> orderedPastas = new();

						names.ForEach(x =>
						{
							var confidence = FuzzyString.ComparisonMetrics.LevenshteinDistance(title, x);
							orderedPastas.Add(x, confidence);
						});

						var searchResults = orderedPastas.OrderByDescending(x => x.Value);

						if (searchResults.Any())
						{
							StringBuilder pastas = new();

							foreach (var entry in searchResults)
							{
								var p = Database.Pastas.FirstOrDefault(x => x.Name == entry.Key);

								pastas.Append(p.Name);

								if (p.Name != searchResults.LastOrDefault().Key)
								{
									pastas.Append(", ");
								}
							}

							if (pastas.Length >= 900)
							{
								var stream = pastas.ToString().ToStream();

								stream.Position = 0;

								await "Here's all that are related to the search term: \"{title}\"".QueueMessageAsync(Context, true, fileStream: stream, fileName: "pastas.txt");
							}
							else
							{
								StringBuilder response = new('`');

								response.Append(pastas);

								response.Append('`');

								await
									EmbedExtensions.FromMessage("Pasta Kitchen", $"Here's all that are related to the search term: \"{title}\"\n{response}", Context)
									.QueueMessageAsync(Context)
									.ConfigureAwait(false);
							}
						}
						else
						{
							await
								EmbedExtensions.FromError("Pasta Kitchen", "You have no pastas", Context)
								.QueueMessageAsync(Context)
							.ConfigureAwait(false);
						}

					}
					break;

				default:
					{
						if (pasta is not null)
						{
							var owner = Context.Client.GetUser(pasta.OwnerId);

							string username = "Unknown User";

							if (owner is not null)
							{
								username = owner.FullName();
							}

							if (pasta.Content.IsImageExtension())
							{
								var links = new List<string>();
								MatchCollection mactches = SkuldAppContext.LinkRegex.Matches(pasta.Content);
								foreach (Match match in mactches)
								{
									links.Add(match.Value);
								}

								await
									EmbedExtensions.FromImage(
										links.FirstOrDefault(),
										pasta.Content,
										Context
									)
									.WithTitle($"{pasta.Name} - {username}")
									.QueueMessageAsync(Context)
								.ConfigureAwait(false);
							}
							else
							{
								await
									EmbedExtensions.FromMessage(
										$"{pasta.Name} - {username}",
										pasta.Content,
										Context
									)
									.QueueMessageAsync(Context)
								.ConfigureAwait(false);
							}
						}
						else
						{
							DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
							await
								EmbedExtensions.FromError("Pasta Kitchen", $"Pasta `{cmd}` doesn't exist. :/ Sorry.", Context)
								.QueueMessageAsync(Context)
							.ConfigureAwait(false);
						}
					}
					break;
			}
		}

		[Command("pasta give"), Summary("Give someone your pasta"), RequireDatabase]
		[Usage("give naenae <@0>")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Pasta(string title, [Remainder] IGuildUser user)
		{
			if (user is null)
			{
				await EmbedExtensions.FromError("Pasta Kitchen", "You can't give no one your pasta", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			Pasta pasta = null;
			SocketMessage response = null;

			if (Database.Pastas.ToList().Any(x => x.Name == title))
			{
				pasta = Database.Pastas.ToList().FirstOrDefault(x => x.Name == title);
			}
			else
			{
				await EmbedExtensions.FromError("Pasta Kitchen", $"Pasta, `{title}` doesn't exist", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			await $"{user.Mention} please respond with Y/N if you wish to receive pasta `{title}` from {Context.User.Mention}".QueueMessageAsync(Context).ConfigureAwait(false);

			{
				using var tokenSource = new CancellationTokenSource();

				Task handler(SocketMessage arg)
				{
					if (arg.Author.Id == user.Id && Context.Message.Channel.Id == arg.Channel.Id)
					{
						response = arg;
						tokenSource.Cancel();
					}

					return Task.CompletedTask;
				}

				try
				{
					Context.Client.MessageReceived += handler;

					await Task.Delay(TimeSpan.FromSeconds(60), tokenSource.Token).ConfigureAwait(false);

					if (response is not null)
					{
						if (response.Content == "Y")
						{
							pasta.OwnerId = response.Author.Id;

							await Database.SaveChangesAsync();
						}
					}
				}
				catch { }
				finally
				{
					Context.Client.MessageReceived -= handler;
				}
			}

			if (response is null)
			{
				await EmbedExtensions.FromMessage("Pasta Kitchen", $"User {user.Mention} didn't respond, you get to keep it", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}
			else
			{
				if (response.Content.ToUpperInvariant().StartsWith("Y"))
				{
					try
					{
						pasta.OwnerId = user.Id;

						await Database.SaveChangesAsync().ConfigureAwait(false);

						await EmbedExtensions.FromSuccess("Pasta Kitchen", $"Successfully transferred the pasta \"{title}\" to {user.Mention}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
					catch
					{
						await EmbedExtensions.FromError("Pasta Kitchen", $"Error transferring \"{title}\" over to {user.Mention}. Please try again.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
					return;
				}
				else if (response.Content.ToUpperInvariant().StartsWith("N"))
				{
					await EmbedExtensions.FromError("Pasta Kitchen", $"Apologies {Context.User.Mention} they don't wish to receive the pasta: {title}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
				else
				{
					await EmbedExtensions.FromError("Pasta Kitchen", $"Unknown Repsonce received", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
			}
		}

		[Command("mypasta"), Summary("Spaghetti Meatballs"), RequireDatabase]
		[Alias("mypastas")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task MyPasta()
		{
			SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

			IReadOnlyList<Pasta> ownedPastas = database.Pastas.ToList().Where(x => x.OwnerId == Context.User.Id).ToList();

			if (ownedPastas.Any())
			{
				StringBuilder pastas = new();

				foreach (Pasta pasta in ownedPastas)
				{
					pastas.Append(pasta.Name);

					if (pasta != ownedPastas[ownedPastas.Count - 1])
					{
						pastas.Append(", ");
					}
				}

				if (pastas.Length >= 900)
				{
					using MemoryStream stream = new();
					using StreamWriter writer = new(stream);
					writer.Write(pastas.ToString());

					stream.Position = 0;

					await Context.Channel.SendFileAsync(stream, "pastas.txt", "Your pastas have arrived").ConfigureAwait(false);
				}
				else
				{
					StringBuilder response = new("`");

					response.Append(pastas);

					response.Append('`');

					await
						EmbedExtensions.FromMessage("Pasta Kitchen", $"Your pastas are: {response}", Context)
						.QueueMessageAsync(Context)
						.ConfigureAwait(false);
				}
			}
			else
			{
				await
					EmbedExtensions.FromError("Pasta Kitchen", "You have no pastas", Context)
					.QueueMessageAsync(Context)
					.ConfigureAwait(false);
			}
		}

		#endregion Pasta

		#region Emoji

		[Command("emoji"), Summary("Turns text into bigmoji")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Emojify([Remainder] string message)
			=> await message.ToRegionalIndicator().QueueMessageAsync(Context).ConfigureAwait(false);

		[Command("emojidance"), Summary("Dancing Emoji")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task DanceEmoji([Remainder] string message)
			=> await message.ToDancingEmoji().QueueMessageAsync(Context).ConfigureAwait(false);

		#endregion Emoji

		#region Webcomics

		[Command("xkcd"), Summary("Get's Random XKCD comic"), Ratelimit(5, 1, Measure.Minutes)]
		public async Task XKCD(int comicid = -1)
		{
			if (comicid == -1)
			{
				await SendXKCD((await ComicClients.GetRandomXKCDComic().ConfigureAwait(false)) as XKCDComic).ConfigureAwait(false);
				DogStatsd.Increment("web.get");
			}
			else
			{
				await SendXKCD((await ComicClients.GetXKCDComic(comicid).ConfigureAwait(false)) as XKCDComic).ConfigureAwait(false);
				DogStatsd.Increment("web.get");
			}
		}

		public async Task SendXKCD(XKCDComic comic)
		{
			if (comic is not null)
			{
				string datefixed = comic.Day;
				string monthfixed = comic.Month;
				DateTime dateTime;

				if (comic.Day.Length == 1)
				{
					datefixed = "0" + comic.Day;
				}

				if (comic.Month.Length == 1)
				{
					monthfixed = "0" + comic.Month;
				}

				dateTime = DateTime.ParseExact($"{datefixed} {monthfixed} {comic.Year}", "dd MM yyyy", CultureInfo.InvariantCulture);

				await new EmbedBuilder()
					.WithAuthor(
						new EmbedAuthorBuilder()
						.WithName("Randall Patrick Munroe - XKCD")
						.WithUrl($"https://xkcd.com/{comic.Number}/")
						.WithIconUrl("https://pbs.twimg.com/profile_images/602808103281692673/8lIim6cB_400x400.png")
					)
					.WithColor(Color.Teal)
					.WithFooter(
						new EmbedFooterBuilder()
						.WithText("Strip released on")
					)
					.WithTimestamp(dateTime)
					.WithImageUrl(comic.Image)
					.WithDescription(comic.Alt)
					.WithUrl(comic.Link)
					.QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Command("cah"),
			Summary("Gets a Random Cynaide & Happiness Comic"),
			Alias("cyanide&happiness", "c&h"),
			Ratelimit(5, 1, Measure.Minutes)
		]
		public async Task CAH()
		{
			try
			{
				var comic = await
					ComicClients.GetCAHComicAsync()
				.ConfigureAwait(false) as CAHComic;

				DogStatsd.Increment("web.get");

				await new EmbedBuilder()
					.WithAuthor(
					new EmbedAuthorBuilder()
						.WithName($"Strip done {comic.Author}")
						.WithUrl(comic.AuthorURL)
						.WithIconUrl(comic.AuthorAvatar)
					)
					.WithImageUrl(comic.ImageURL)
					.WithUrl(comic.URL)
					.WithColor(Color.Teal)
					.QueueMessageAsync(Context).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Error("CAH-Cmd", "Error parsing website", Context, ex);
			}
		}

		[
			Command("cad"),
			Summary("Gets a random CAD comic"),
			Ratelimit(5, 1, Measure.Minutes)
		]
		public async Task CAD()
		{
			try
			{
				var comic = await
					ComicClients.GetCADComicAsync()
				.ConfigureAwait(false) as CADComic;
				DogStatsd.Increment("web.get");
				await
					EmbedExtensions.FromImage(
						comic.ImageURL,
						EmbedExtensions.RandomEmbedColor(),
						Context)
						.WithAuthor(
							new EmbedAuthorBuilder()
							.WithName("Tim Buckley")
						)
						.WithTitle(comic.Title)
						.WithUrl(comic.URL)
					.QueueMessageAsync(Context).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Error("CAD-Cmd", "Error parsing website", Context, ex);
				await EmbedExtensions.FromError(
					$"Error parsing website, try again later",
					Context).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		#endregion Webcomics

		#region Magik

		[Command("magik"), Summary("Magiks an image"), Alias("magick", "magic", "liquify"), Ratelimit(5, 1, Measure.Minutes)]
		public async Task Magik()
		{
			string url = "";

			var msgsRaw = Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, 25);
			var msgs = await msgsRaw.FlattenAsync().ConfigureAwait(false);

			foreach (var msg in msgs)
			{
				if (msg.Attachments.Any())
				{
					url = msg.Attachments.FirstOrDefault().Url;
					break;
				}
				if (msg.Embeds.Any())
				{
					var embed = msg.Embeds.FirstOrDefault();
					if (embed.Image.HasValue)
					{
						url = embed.Image.Value.Url;
						break;
					}
					if (embed.Thumbnail.HasValue)
					{
						url = embed.Thumbnail.Value.Url;
						break;
					}
				}
			}

			if (!string.IsNullOrEmpty(url))
			{
				await Magik(new Uri(url)).ConfigureAwait(false);
				return;
			}

			await EmbedExtensions.FromError("Couldn't find an image", Context).QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("magik"), Summary("Magiks an image"), Alias("magick", "magic", "liquify"), Ratelimit(5, 1, Measure.Minutes)]
		public async Task Magik([Remainder] IGuildUser user)
			=> await Magik(
					new Uri(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
				).ConfigureAwait(false);

		[Command("magik"), Summary("Magiks an image"), Alias("magick", "magic", "liquify"), Ratelimit(5, 1, Measure.Minutes)]
		public async Task Magik(Uri image)
		{
			var img = await ApiClient.GetLiquidRescaledAsync(image.OriginalString);

			if (img is not null)
			{
				await "".QueueMessageAsync(
				Context,
				fileStream: img,
				fileName: "image.png"
			).ConfigureAwait(false);
			}
		}

		#endregion Magik

		#region Jokes

		[Command("roast"), Summary("Yo momma so big fat thaht she didn\'t was was a McDonald\'s Big Mac")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task RoastCmd(IUser user = null)
		{
			if (user is null)
				user = Context.User;
			var roast = await SysExClient.GetRoastAsync().ConfigureAwait(false);
			DogStatsd.Increment("web.get");

			await $"{user.Mention} {roast}".QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("dadjoke"), Summary("Gives you a bad dad joke to facepalm at")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task DadJoke()
		{
			var joke = await SysExClient.GetDadJokeAsync().ConfigureAwait(false);
			DogStatsd.Increment("web.get");

			await
				EmbedExtensions.FromMessage(joke.Setup, joke.Punchline, Context)
				.WithRandomColor()
			.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("pickup", RunMode = RunMode.Async), Summary("Cringe at these bad user-submitted pick up lines. (Don't actually use these or else you'll get laughed at. :3)"), Alias("pickupline")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task PickUp()
		{
			var pickup = await SysExClient.GetPickupLineAsync().ConfigureAwait(false);
			DogStatsd.Increment("web.get");

			await
				EmbedExtensions.FromMessage(pickup.Setup, pickup.Punchline, Context)
				.WithRandomColor()
			.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		#endregion Jokes

		#region Figlet

		private const int FIGLETWIDTH = 16;

		[Command("figlet"), Summary("Make a big ascii text lol")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Figlet([Remainder] string text)
		{
			var splittext = text.ToCharArray();
			var textrows = new List<string>();
			if (splittext.Length > FIGLETWIDTH)
			{
				int count = (int)Math.Round(splittext.Length / (decimal)FIGLETWIDTH, MidpointRounding.AwayFromZero);

				int prevamount = 0;
				for (int x = 1; x <= count; x++)
				{
					int amount = x * FIGLETWIDTH;
					string txt = string.Concat(splittext.Skip(prevamount).Take(amount));
					textrows.Add(txt);
					prevamount = amount;
				}

				if (count * FIGLETWIDTH < splittext.Length)
				{
					var temp = splittext.ToList();
					temp.RemoveRange(0, count * FIGLETWIDTH);

					textrows.Add(string.Join(' ', temp));
				}
			}
			else
			{
				textrows.Add(text);
			}

			string result = "```\n";

			foreach (var row in textrows)
			{
				var figlet = new AsciiArt(row);
				foreach (var line in figlet.Result)
				{
					result += line + "\n";
				}
				result += "\n";
			}

			result = result[0..^2];
			result += "```";

			await result.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		#endregion Figlet

		#region Images

		[Command("memegen"), Summary("Does a funny haha meme"), Ratelimit(20, 1, Measure.Minutes)]
		public async Task MemeGenerator(string template = null, params string[] sources)
		{
			if (template is null && !sources.Any())
			{
				var endpoints = JsonConvert.DeserializeObject<MemeResponse>(await HttpWebClient.ReturnStringAsync(new Uri("https://api.skuld.bot/meme/?endpoints")).ConfigureAwait(false)).Endpoints;

				var pages = endpoints.Paginate(35);

				int index = 0;
				foreach (var page in pages)
				{
					await EmbedExtensions.FromMessage($"__Current Templates ({index + 1}/{pages.Count})__", page, Context)
						.QueueMessageAsync(Context).ConfigureAwait(false);
					index++;
				}

				return;
			}

			List<string> imageLinks = new();

			foreach (var str in sources)
			{
				if (str.IsImageExtension())
				{
					imageLinks.Add(str.TrimEmbedHiders());
				}

				if (DiscordUtilities.UserMentionRegex.IsMatch(str))
				{
					var userid = str.Replace("<@!", "").Replace("<@", "").Replace(">", "");
					ulong.TryParse(userid, out ulong useridl);

					var user = Context.Guild.GetUser(useridl);

					imageLinks.Add(user.GetAvatarUrl(ImageFormat.Png, 1024) ?? user.GetDefaultAvatarUrl());
				}
			}

			if (imageLinks.All(x => x.IsImageExtension()))
			{
				var endpoints = JsonConvert.DeserializeObject<MemeResponse>(await HttpWebClient.ReturnStringAsync(new Uri("https://api.skuld.bot/meme/?endpoints")).ConfigureAwait(false)).Endpoints;

				if (endpoints.Any(x => x.Name.IsSameUpperedInvariant(template)))
				{
					var endpoint = endpoints.First(x => x.Name.IsSameUpperedInvariant(template));
					if (endpoint.RequiredSources == imageLinks.Count)
					{
						var resp = await SysExClient.GetMemeImageAsync(endpoint.Name, imageLinks.ToArray()).ConfigureAwait(false);

						if (resp is not null && resp is Stream)
						{
							var folderPath = Path.Combine(AppContext.BaseDirectory, "storage/meme/");

							var filePath = Path.Combine(folderPath, $"{template}-{Context.User.Id}-{Context.Channel.Id}.png");

							if (!Directory.Exists(folderPath))
							{
								Directory.CreateDirectory(folderPath);
							}

							await "".QueueMessageAsync(Context, filestream: resp as Stream).ConfigureAwait(false);
						}
					}
					else
					{
						await EmbedExtensions.FromError($"You don't have enough sources. You need {endpoint.RequiredSources} source images", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				}
				else
				{
					await EmbedExtensions.FromError($"Template `{template}` does not exist", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}
			else
			{
				await EmbedExtensions.FromError("Sources need to be an image link", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[Command("iqdb"), Summary("Reverse image-search")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task IQDB(string image)
		{
			var results = await IqdbClient.SearchUrl(image).ConfigureAwait(false);

			if (results.IsFound)
			{
				var sorted = results.Matches.OrderByDescending(x => x.Similarity);
				var mostlikely = sorted.FirstOrDefault();
				string url = !mostlikely.Url.Contains("https:") && !mostlikely.Url.Contains("http:") ? "https:" + mostlikely.Url : mostlikely.Url;

				await EmbedExtensions.FromMessage(SkuldAppContext.GetCaller(), $"Similarity: {mostlikely.Similarity}%", Context)
					.WithImageUrl(url).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				await EmbedExtensions.FromError("No results found", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		#endregion Images

		[
			Command("wikismash"),
			Summary("Smash the image of the first article, with the content of the second"),
			Usage("Joker_(character) Insanity")
		]
		public async Task WikiSmash(string firstArticle, string secondArticle)
		{
			if (string.IsNullOrEmpty(firstArticle))
			{
				await
					EmbedExtensions.FromError(
						"Incorrect Argument for firstArticle",
						Context
					)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
				return;
			}
			if (string.IsNullOrEmpty(secondArticle))
			{
				await
					EmbedExtensions.FromError(
						"Incorrect Argument for secondArticle",
						Context
					)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
				return;
			}

			WikipediaArticle first;

			try
			{
				first = await
					WikiClient.GetArticleAsync("en", firstArticle)
				.ConfigureAwait(false);

				if (first is not null)
				{
					if (first.ImageUrl.IsNullOrWhiteSpace() || first.ImageUrl.EndsWith(".svg"))
					{
						await
							EmbedExtensions.FromError(
								"firstArticle doesn't have an image",
								Context
							)
							.QueueMessageAsync(Context)
						.ConfigureAwait(false);

						return;
					}
				}
				else
				{
					await
						EmbedExtensions.FromError(
							"firstArticle doesn't exist",
							Context
						)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);

					return;
				}
			}
			catch (Exception ex)
			{
				await
					EmbedExtensions.FromError("firstArticle doesn't exist", Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
				Log.Error("WikiSmash", ex.Message, Context, ex);

				return;
			}

			WikipediaArticle second;

			try
			{
				second = await WikiClient.GetArticleAsync("en", secondArticle).ConfigureAwait(false);

				if (second is null)
				{
					await
						EmbedExtensions.FromError("secondArticle doesn't exist", Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);

					return;
				}
			}
			catch (Exception ex)
			{
				await
					EmbedExtensions.FromError("secondArticle doesn't exist", Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
				Log.Error("WikiSmash", ex.Message, Context, ex);

				return;
			}

			await
				EmbedExtensions.FromImage(first.ImageUrl, Color.Default, Context)
					.WithTitle(second.Name)
					.WithDescription(second.Description.ReplaceLast("Read more at the article.", ""))
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("yandev"),
			Summary("I, EvaX humbly submit a toast"),
			Usage("@Skuld instability be a discord bot")
		]
		public async Task YanDev(
			IGuildUser target,
			string finish,
			[Remainder] string action
		)
		{
			ITextChannel channel = Context.Channel as ITextChannel;
			if (Context.Message.MentionedChannels.Any())
			{
				channel = Context.Message.MentionedChannels.FirstOrDefault() as ITextChannel;
				Context.Message.MentionedChannels.ToList().ForEach(x =>
				{
					action = action.Replace($"<#{x.Id}>", "");
					finish = finish.Replace($"<#{x.Id}>", "");
				});
			}
			if (finish.Length <= 0)
			{
				await
					EmbedExtensions.FromError($"{nameof(finish)} is empty, check your input and try again", Context)
					.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				return;
			}
			if (action.Length <= 0)
			{
				await
					EmbedExtensions.FromError($"{nameof(action)} is empty, check your input and try again", Context)
					.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				return;
			}

			if (action.LastOrDefault() == ' ')
				action = action[0..^1];

			await
				channel
					.SendMessageAsync($"I, {Context.User.Mention}, humbly submit a toast, to {target.Mention}, for successfully managing to {action}, congratulations {target.Mention}, enjoy your {finish}.")
			.ConfigureAwait(false);
		}
	}
}
