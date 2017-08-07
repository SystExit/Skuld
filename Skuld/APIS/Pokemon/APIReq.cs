using System;
using System.Threading.Tasks;
using Skuld.Models.API.Pokemon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Skuld.APIS.PokeAPI
{
    public class PokeAPI
    {
        public static int? HighestID = GetHighestPokemon().Result;
        private static async Task<int> GetHighestPokemon()
        {
            try
            {
                var result = await APIWebReq.ReturnString(new Uri($"https://pokeapi.co/api/v2/pokemon/802/"));
                if(result!=null)
                {
                    return 802;
                }
                else
                {
                    return 721;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 0;
            }
        }

        public static async Task<Pokemon> GetPokemon(int? id = null)
        {
            Pokemon pokemon = null;
            try
            {
                if(id.HasValue)
                {
                    if (!Directory.Exists(AppContext.BaseDirectory + "/storage/pokemon/"))
                        Directory.CreateDirectory(AppContext.BaseDirectory + "/storage/pokemon/");
                    string pokejson = AppContext.BaseDirectory + $"/storage/pokemon/{id.Value}.json";
                    var result = await APIWebReq.ReturnString(new Uri($"https://pokeapi.co/api/v2/pokemon/{id.Value}/"));
                    if (!String.IsNullOrEmpty(result))
                    {
                        File.WriteAllText(pokejson, result);
                        return pokemon = JsonConvert.DeserializeObject<Pokemon>(result);
                    }
                    else
                        return null;
                }
                else
                {
                    if(HighestID.HasValue && HighestID.Value > 0)
                    {
                        var random = Bot.random.Next(0, HighestID.Value);
                        if (!Directory.Exists(AppContext.BaseDirectory + "/storage/pokemon/"))
                            Directory.CreateDirectory(AppContext.BaseDirectory + "/storage/pokemon/");
                        string pokejson = AppContext.BaseDirectory + $"/storage/pokemon/{random}.json";
                        var result = await APIWebReq.ReturnString(new Uri($"https://pokeapi.co/api/v2/pokemon/{random}/"));
                        if (!String.IsNullOrEmpty(result))
                        {
                            File.WriteAllText(pokejson, result);
                            return pokemon = JsonConvert.DeserializeObject<Pokemon>(result);
                        }
                        else
                            return null;
                    }
                    else
                    {
                        await GetHighestPokemon();
                        return null;
                    }
                }
            }
            catch(Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("PokeAPI-G", "Error", Discord.LogSeverity.Error, ex));
                return null;
            }
        }
    }
}
