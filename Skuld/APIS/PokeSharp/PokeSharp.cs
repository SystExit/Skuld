using PokeSharp.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PokeSharp.Deserializer
{
    public class WebReq
    {
        public static async Task<PocketMonster> GetPocketMonster(int id)
        {
            var path = Path.Combine(AppContext.BaseDirectory + "/storage/pokemon/");
            PocketMonster poket = null;
            Directory.CreateDirectory(path);
            string get = null;
            List<string> files = Directory.GetFiles(path).ToList();
            if (files.Any(s => s.Contains(Convert.ToString(id))))
            {
                get = File.ReadAllText(files.FirstOrDefault(x => x.Contains(Convert.ToString(id))));
                poket = JsonConvert.DeserializeObject<PocketMonster>(get);
                File.WriteAllText(path + poket.Name + " - " + poket.ID + ".json", get);
                return poket;
            }
            else
            {
                if (id>721) { return null; }
                else
                {
                    get = await WebRequest(Convert.ToString(id));
                    if(get.ToLowerInvariant().Contains("notfound"))
                    { return null; }
                    else
                    {
                        poket = JsonConvert.DeserializeObject<PocketMonster>(get);
                        File.WriteAllText(path + poket.Name + " - " + poket.ID + ".json", get);
                        return poket;
                    }
                }                
            }
        }
        public static async Task<PocketMonster> GetPocketMonster(string name)
        {
            var path = Path.Combine(AppContext.BaseDirectory + "/storage/pokemon/");
            PocketMonster poket = null;
            Directory.CreateDirectory(path);
            string get = null;
            List<string> files = Directory.GetFiles(path).ToList();
            if (files.Any(s=>s.Contains(name)))
            {
                get = File.ReadAllText(files.FirstOrDefault(x => x.Contains(name)));
                poket = JsonConvert.DeserializeObject<PocketMonster>(get);
                File.WriteAllText(path + poket.Name + " - " + poket.ID + ".json", get);
                return poket;
            }                
            else
            {
                get = await WebRequest(name);
                if (get.ToLowerInvariant().Contains("notfound")) { return null; }
                else
                {
                    poket = JsonConvert.DeserializeObject<PocketMonster>(get);
                    File.WriteAllText(path + poket.Name + " - " + poket.ID + ".json", get);
                    return poket;
                }
            }
        }
        private static async Task<string> WebRequest(string name)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage resp = await client.GetAsync(new Uri("http://pokeapi.co/api/v2/pokemon/" + name), HttpCompletionOption.ResponseContentRead))
                {
                    if(resp.IsSuccessStatusCode)
                    {
                        var responce = await resp.Content.ReadAsStringAsync();
                        resp.Dispose();
                        client.Dispose();
                        return responce;
                    }
                    else
                    {
                        var error = resp.StatusCode;
                        resp.Dispose();
                        client.Dispose();
                        return "Error with retrieving. Error code is: "+error;
                    }
                }
            }
        }
    }
}
