using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Skuld.Services;
using System.Collections.Generic;
using Skuld.Models.API.TVDB;

namespace Skuld.APIS
{
    public class TvAPIClient
	{
		readonly LoggingService logger;
		public TvAPIClient(LoggingService log) //depinj
		{
			logger = log;
		}

		public static readonly Uri BaseTVDBUri = new Uri("https://api.thetvdb.com");

		public async Task<IReadOnlyList<SeriesSearchData>> GetTVShowAsync(string name)
		{

			return null;
		}
	}
}
