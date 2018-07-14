﻿using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class Thumbnails
    {
        [JsonProperty(PropertyName = "src")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "config_width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "config_height")]
        public int Height { get; set; }
    }
}