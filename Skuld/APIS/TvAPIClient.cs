using System;
using System.Threading.Tasks;
using Skuld.Services;
using System.Collections.Generic;
using Skuld.Models.API.TVDB;

namespace Skuld.APIS
{
    public class TvAPIClient
	{
		public LoggingService Logger { get; }

		public static readonly Uri BaseTVDBUri = new Uri("https://api.thetvdb.com");

		public async Task<IReadOnlyList<SeriesSearchData>> GetTVShowAsync(string name)
		{
		    return null;
		}
	}
}
