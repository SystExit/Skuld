﻿using System;
using System.Threading.Tasks;
using System.Net;
using Skuld.Tools;
using Skuld.APIS.NASA.Models;
using System.IO;
using Newtonsoft.Json;

namespace Skuld.APIS
{
    public partial class APIWebReq
    {
        public static async Task<APOD> NasaAPOD()
        {
            var client = (HttpWebRequest)WebRequest.Create("https://api.nasa.gov/planetary/apod?api_key=" + Bot.Configuration.NASAApiKey);
            var response = (HttpWebResponse)await client.GetResponseAsync();
            int remainingcalls = 0;
            for(int x=0;x<response.Headers.Count;x++)
            {
                if (response.Headers.Keys[x] == "X-RateLimit-Remaining")
                {
                    remainingcalls = Convert.ToInt32(response.Headers[x]);
                    break;
                }
            }
            if(remainingcalls>0)
            {
                var streamresp = response.GetResponseStream();
                var sr = new StreamReader(streamresp);
                var stringifiedresp = await sr.ReadToEndAsync();
                return JsonConvert.DeserializeObject<APOD>(stringifiedresp);
            }
            else            
                return null;            
        }
    }
}
