using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.Bot.Models
{
    public struct MemeResponse
    {
        public bool Successful;
        public string Example;

        [JsonProperty(PropertyName = "availabletemplates")]
        public List<MemeEndpoints> Endpoints;
    }
}
