using Newtonsoft.Json;
using System.Collections.Generic;
using Skuld.Tools;

namespace Skuld.Models.API.MAL
{
    public class MangaArr
    {
        [JsonProperty(PropertyName = "entry")]
        [JsonConverter(typeof(SingleOrArrayConverter<Manga>))]
        public List<Manga> Entry { get; set; }
    }
}
