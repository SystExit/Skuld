using Discord.Commands;
using Skuld.Core;
using Skuld.Models;
using System;
using System.Threading.Tasks;

namespace Skuld.Bot.Discord.Preconditions
{
	public class RequireConfiguration : PreconditionAttribute
	{
		private readonly ConfigParam RequiredParam;

		public RequireConfiguration(ConfigParam configParam)
		{
			RequiredParam = configParam;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			using var database = new SkuldDbContextFactory().CreateDbContext();

			var config = database.Configurations.Find(SkuldAppContext.ConfigurationId);

			switch (RequiredParam)
			{
				case ConfigParam.Discord:
					{
						if (string.IsNullOrEmpty(config.DiscordToken))
							return Task.FromResult(PreconditionResult.FromError("Discord Token not given"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Websocket:
					{
						if (string.IsNullOrEmpty(config.WebsocketHost) || config.WebsocketPort == 0)
							return Task.FromResult(PreconditionResult.FromError("Websocket host not given"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Github:
					{
						if (string.IsNullOrEmpty(config.GithubClientPassword) || string.IsNullOrEmpty(config.GithubClientUsername) || config.GithubRepository == 0)
							return Task.FromResult(PreconditionResult.FromError("Github not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Google:
					{
						if (string.IsNullOrEmpty(config.GoogleAPI) || string.IsNullOrEmpty(config.GoogleCx))
							return Task.FromResult(PreconditionResult.FromError("Google not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Stands4:
					{
						if (string.IsNullOrEmpty(config.STANDSToken) || config.STANDSUid == 0)
							return Task.FromResult(PreconditionResult.FromError("Stands4 not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Twitch:
					{
						if (string.IsNullOrEmpty(config.TwitchClientID) || string.IsNullOrEmpty(config.TwitchToken))
							return Task.FromResult(PreconditionResult.FromError("Twitch not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Imgur:
					{
						if (string.IsNullOrEmpty(config.ImgurClientSecret) || string.IsNullOrEmpty(config.ImgurClientID))
							return Task.FromResult(PreconditionResult.FromError("Imgur not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.NASA:
					{
						if (string.IsNullOrEmpty(config.NASAApiKey))
							return Task.FromResult(PreconditionResult.FromError("NASA not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Datadog:
					{
						if (string.IsNullOrEmpty(config.DataDogHost) || config.DataDogPort == 0)
							return Task.FromResult(PreconditionResult.FromError("Datadog not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Pinboard:
					{
						if (config.PinboardDateLimit == -1 || config.PinboardThreshold == -1)
							return Task.FromResult(PreconditionResult.FromError("Pinboard not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Daily:
					{
						if (config.DailyAmount == 0)
							return Task.FromResult(PreconditionResult.FromError("Daily not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.VoiceExp:
					{
						if (config.VoiceExpDeterminate == -1 || config.VoiceExpMaxGrant == 0 || config.VoiceExpMinMinutes == 0)
							return Task.FromResult(PreconditionResult.FromError("Voice Experience not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.StreakLimit:
					{
						if (config.StreakLimitDays == 0)
							return Task.FromResult(PreconditionResult.FromError("StreakLimit not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Dbots:
					{
						if (string.IsNullOrEmpty(config.DBotsOrgKey))
							return Task.FromResult(PreconditionResult.FromError("DBots not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.TopGG:
					{
						if (string.IsNullOrEmpty(config.DiscordGGKey))
							return Task.FromResult(PreconditionResult.FromError("TOPGG not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Bots4Discord:
					{
						if (string.IsNullOrEmpty(config.B4DToken))
							return Task.FromResult(PreconditionResult.FromError("Bots4Discord not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				case ConfigParam.Twitter:
					{
						if (string.IsNullOrEmpty(config.TwitterAccessSec) || string.IsNullOrEmpty(config.TwitterAccessTok) || string.IsNullOrEmpty(config.TwitterConKey) || string.IsNullOrEmpty(config.TwitterConSec))
							return Task.FromResult(PreconditionResult.FromError("Twitter not configured"));
						return Task.FromResult(PreconditionResult.FromSuccess());
					}

				default:
					return Task.FromResult(PreconditionResult.FromError("Unknown parameter"));
			}
		}
	}

	public enum ConfigParam
	{
		Discord,
		Websocket,
		Github,
		Google,
		Stands4,
		Twitch,
		Imgur,
		NASA,
		Datadog,
		Pinboard,
		Daily,
		VoiceExp,
		StreakLimit,
		Dbots,
		TopGG,
		Bots4Discord,
		Twitter,
	}
}