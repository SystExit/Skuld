using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Skuld.Services;
using Skuld.APIS;

namespace Skuld.Models.API.TVDB
{
    public class SeriesSearchData
	{
		public string[] Aliases;
		public string Banner;
		public string FirstAired;
		public ulong ID;
		public string Network;
		public string Overview;
		public string SeriesName;
		public string Status;

		public async Task<TVShow> GetInformationAsync()
		{
			//TvAPIClient.BaseTVDBUri;
			return null;
		}
    }
}
