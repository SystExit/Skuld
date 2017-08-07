using Newtonsoft.Json;

namespace Skuld.Models.API.MAL
{
    public class Manga
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; internal set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; internal set; }
        [JsonProperty(PropertyName = "english")]
        public string EnglishTitle { get; internal set; }
        [JsonProperty(PropertyName = "synonyms")]
        public string Synonyms { get; internal set; }
        [JsonProperty(PropertyName = "chapters")]
        public string Chapters { get; internal set; }
        [JsonProperty(PropertyName = "volumes")]
        public string Volumes { get; internal set; }
        [JsonProperty(PropertyName = "score")]
        public string Score { get; internal set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; internal set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; internal set; }
        [JsonProperty(PropertyName = "start_date")]
        public string StartDate { get; internal set; }
        [JsonProperty(PropertyName = "end_date")]
        public string EndDate { get; internal set; }
        [JsonProperty(PropertyName = "synopsis")]
        public string Synopsis { get; internal set; }
        [JsonProperty(PropertyName = "image")]
        public string Image { get; internal set; }
    }
}