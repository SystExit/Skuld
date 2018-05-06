using System;
using System.Threading.Tasks;
using Skuld.Models.API.Pokemon;
using Newtonsoft.Json;
using System.IO;
using Skuld.Services;

namespace Skuld.APIS
{
    public class PokeSharpClient
    {
		Random Random { get; set; }
		LoggingService Logger { get; set; }

        public static int? HighestPokeID = GetHighestPokemon().Result;
        private static async Task<int> GetHighestPokemon()
        {
            try
            {
                var result = await WebHandler.ReturnStringAsync(new Uri($"https://pokeapi.co/api/v2/pokemon/802/"));
                if(result!=null)
                {
                    return 802;
                }
                else
                {
                    return 721;
                }
            }
            catch
            {
                return 0;
            }
        }

        public async Task<Pokemon> GetPokemonAsync(int? id = null)
        {
            Pokemon pokemon = null;
            try
            {
                if(id.HasValue)
                {
                    if (!Directory.Exists(AppContext.BaseDirectory + "/skuld/storage/pokemon/"))
                        Directory.CreateDirectory(AppContext.BaseDirectory + "/skuld/storage/pokemon/");
                    string pokejson = AppContext.BaseDirectory + $"/skuld/storage/pokemon/{id.Value}.json";
                    var result = await WebHandler.ReturnStringAsync(new Uri($"https://pokeapi.co/api/v2/pokemon/{id.Value}/"));
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
                    if(HighestPokeID.HasValue && HighestPokeID > 0)
                    {
                        var rand = Random.Next(0, HighestPokeID.Value);
                        if (!Directory.Exists(AppContext.BaseDirectory + "/skuld/storage/pokemon/"))
                            Directory.CreateDirectory(AppContext.BaseDirectory + "/skuld/storage/pokemon/");
                        string pokejson = AppContext.BaseDirectory + $"/skuld/storage/pokemon/{rand}.json";
                        var result = await WebHandler.ReturnStringAsync(new Uri($"https://pokeapi.co/api/v2/pokemon/{rand}/"));
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
                await Logger.AddToLogsAsync(new Models.LogMessage("PokeAPI-G", "Error", Discord.LogSeverity.Error, ex));
                return null;
            }
        }
    }
}
