using Newtonsoft.Json;
using RestEase;
using Skuld.Core.Models;
using Skuld.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Skuld.API
{
	public class SkuldAPI : ISkuldAPIClient, IDisposable
	{
		public static string WrapperVersion { get; } =
			typeof(SkuldAPI).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
			typeof(SkuldAPI).GetTypeInfo().Assembly.GetName().Version.ToString(3) ??
			"Unknown";

		private readonly ISkuldAPIClient _api;

		static string ApiBase;
		static string Token;
		private bool disposedValue;

		public SkuldAPI()
		{

		}

		public SkuldAPI(string apiBase, string token)
		{
			ApiBase = apiBase;
			Token = token;

			var httpClient = new HttpClient
			{
				BaseAddress = new Uri(ApiBase)
			};
			httpClient.DefaultRequestHeaders.Add("User-Agent", $"Skuld.API.Client/v{WrapperVersion} (https://github.com/skuldbot/Skuld)");

			if (!string.IsNullOrWhiteSpace(Token))
			{
				httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");
			}

			JsonSerializerSettings settings = new()
			{
				Formatting = Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore
			};

			_api = new RestClient(httpClient)
			{
				JsonSerializerSettings = settings
			}.For<ISkuldAPIClient>();
		}

		public Task<EventResult<Guild>> GetGuildAsync(ulong id)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetGuildAsync(id) : null;

		public Task<EventResult<User>> GetUserAsync(ulong id)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetUserAsync(id) : null;

		public Task<Stream> GetProfileCardAsync(ulong id)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetProfileCardAsync(id) : null;

		public Task<Stream> GetProfileCardAsync(ulong id, ulong guildId)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetProfileCardAsync(id, guildId) : null;

		public Task<Stream> GetRankCardAsync(ulong id, ulong guildId)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetRankCardAsync(id, guildId) : null;

		public Task<Stream> GetExampleProfileCardAsync(ulong id, string previewBackground)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetExampleProfileCardAsync(id, previewBackground) : null;

		public Task<EventResult<UserExperience>> GetExperienceLeaderboardAsync(ulong guildId, int page = 0)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetExperienceLeaderboardAsync(guildId, page) : null;

		public Task<EventResult<User>> GetMoneyLeaderboardAsync(ulong guildId, int page = 0)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetMoneyLeaderboardAsync(guildId, page) : null;

		public Task<Stream> GetLiquidRescaledAsync(string image)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetLiquidRescaledAsync(image) : null;

		public Task<Stream> GetJoinCardAsync(ulong id, ulong guildId)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetJoinCardAsync(id, guildId) : null;
		public Task<Stream> GetLeaveCardAsync(ulong id, ulong guildId)
			=> !string.IsNullOrWhiteSpace(Token) ? _api.GetLeaveCardAsync(id, guildId) : null;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~SkuldAPI()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
