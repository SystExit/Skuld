using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.APIS.UrbanDictionary.Models
{
    public class UrbanWordContainer
    {
        [JsonProperty("list")]
        public IReadOnlyList<UrbanWord> List { get; private set; }
    }
}
