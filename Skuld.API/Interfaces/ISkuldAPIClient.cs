using RestEase;
using Skuld.Core.Models;
using Skuld.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Skuld.API
{
	public interface ISkuldAPIClient : IDisposable
	{
		#region Guild
		/// <summary>
		/// Get a guild
		/// </summary>
		/// <param name="id">Guild Id</param>
		/// <returns><see cref="EventResult{T}"/> wrapped <see cref="Guild"/></returns>
		[Get("/guild/{id}")]
		public Task<EventResult> GetGuildAsync([Path] ulong id);
		#endregion Guild

		#region User
		/// <summary>
		/// Get a user
		/// </summary>
		/// <param name="id">User Id</param>
		/// <returns><see cref="EventResult{T}"/> wrapped <see cref="User"/></returns>
		[Get("/user/{id}")]
		public Task<EventResult> GetUserAsync([Path] ulong id);
		#endregion User

		#region Experience
		/// <summary>
		/// Gets the experience leaderboard
		/// </summary>
		/// <param name="guildId">Guild Id, Accepts 0 for all</param>
		/// <param name="page">%10 offset page</param>
		/// <returns><see cref="EventResult{T}"/> wrapped <see cref="UserExperience"/></returns>
		[Get("/experience/{guildId}/{page}")]
		public Task<EventResult> GetExperienceLeaderboardAsync([Path] ulong guildId, [Path] int page = 0);
		#endregion Experience

		#region Money
		/// <summary>
		/// Gets the money leaderboard
		/// </summary>
		/// <param name="guildId">Guild Id, Accepts 0 for all</param>
		/// <param name="page">%10 offset page</param>
		/// <returns><see cref="EventResult{T}"/> wrapped <see cref="User"/></returns>
		[Get("/money/{guildId}/{page}")]
		public Task<EventResult> GetMoneyLeaderboardAsync([Path] ulong guildId, [Path] int page = 0);
		#endregion Money

		#region Profile
		/// <summary>
		/// Get a user's profile card
		/// </summary>
		/// <param name="id">User Id</param>
		/// <returns><see cref="Stream"/> of profile card data</returns>
		[Get("/image/profile/{id}")]
		public Task<Stream> GetProfileCardAsync([Path] ulong id);

		/// <summary>
		/// Get a user's profile card with guild attributes
		/// </summary>
		/// <param name="id">User Id</param>
		/// <param name="id">Guild Id</param>
		/// <returns><see cref="Stream"/> of profile card data</returns>
		[Get("/image/profile/{id}/{guildId}")]
		public Task<Stream> GetProfileCardAsync([Path] ulong id, [Path] ulong guildId);

		/// <summary>
		/// Preview's a custom background image
		/// </summary>
		/// <param name="id">User Id</param>
		/// <param name="previewBackground">Background to preview</param>
		/// <returns><see cref="Stream"/> of profile card data</returns>
		[Get("/image/profile/{id}/example")]
		public Task<Stream> GetExampleProfileCardAsync([Path] ulong id, [Query("previewBackground")] string previewBackground);

		#endregion Profile

		#region Rank

		/// <summary>
		/// Get a user's rank card
		/// </summary>
		/// <param name="id">User Id</param>
		/// <param name="guildId">Guild Id</param>
		/// <returns><see cref="Stream"/> of profile card data</returns>
		[Get("/image/rank/{id}/{guildId}")]
		public Task<Stream> GetRankCardAsync([Path] ulong id, [Path] ulong guildId);

		#endregion Rank

		#region Leave/Join Card

		/// <summary>
		/// Get's the join card for a guild
		/// </summary>
		/// <param name="id">User Id</param>
		/// <param name="guildId">Guild Id</param>
		/// <returns><see cref="Stream"/> of join card</returns>
		[Get("/image/join/{id}/{guildId}")]
		public Task<Stream> GetJoinCardAsync([Path] ulong id, [Path] ulong guildId);

		/// <summary>
		/// Get's the leave card for a guild
		/// </summary>
		/// <param name="id">User Id</param>
		/// <param name="guildId">Guild Id</param>
		/// <returns><see cref="Stream"/> of leave card</returns>
		[Get("/image/leave/{id}/{guildId}")]
		public Task<Stream> GetLeaveCardAsync([Path] ulong id, [Path] ulong guildId);

		#endregion Leave/Join Card

		[Get("/image/magik")]
		public Task<Stream> GetLiquidRescaledAsync([Query] string image);
	}
}
