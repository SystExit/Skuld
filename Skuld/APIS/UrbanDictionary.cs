using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Skuld.Models.API;

namespace Skuld.APIS
{
    public class UrbanDictionary
    {
        static Uri RandomEndpoint = new Uri("http://api.urbandictionary.com/v0/random");
        static Uri QueryEndPoint = new Uri("http://api.urbandictionary.com/v0/define?term=");

        public static async Task<UrbanWord> GetRandomWordAsync()
        {
            var raw = await WebHandler.ReturnStringAsync(RandomEndpoint);
            return JsonConvert.DeserializeObject<UrbanWord>(raw);
        }
        
        public static async Task<UrbanWord> GetPhraseAsync(string phrase)
        {
            var raw = await WebHandler.ReturnStringAsync(new Uri(QueryEndPoint + phrase));
            return JsonConvert.DeserializeObject<UrbanWord>(raw);
        }
    }
}
