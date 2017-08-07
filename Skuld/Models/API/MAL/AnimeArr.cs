using Newtonsoft.Json;
using System.Collections.Generic;
using Skuld.Tools;

namespace Skuld.Models.API.MAL
{
    public class AnimeArr
    {
        [JsonProperty(PropertyName = "entry")]
        [JsonConverter(typeof(SingleOrArrayConverter<Anime>))]
        public List<Anime> Entry { get; set; }
    }
}
