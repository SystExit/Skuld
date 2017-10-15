using System.Collections.Generic;
using MyAnimeListSharp.Auth;
using MyAnimeListSharp.Core;
using MyAnimeListSharp.Facade.Async;
using System.Threading.Tasks;
using System;

namespace Skuld.APIS
{
    public class MALClient
    {
        public static async Task<List<AnimeEntry>> GetAnimes(string title)
        {
            try
            {
                var search = new AnimeSearchMethodsAsync(GetOrCreateClient());
                
                return (await search.SearchDeserializedAsync(title)).Entries;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
        public static async Task<List<MangaEntry>> GetMangas(string title)
        {
            try
            {
                var search = new MangaSearchMethodsAsync(GetOrCreateClient());
                return (await search.SearchDeserializedAsync(title)).Entries;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
        private static ICredentialContext GetOrCreateClient()
        {
            var config = Tools.Config.Load();
            var client = Bot.MalClient;
            if (client != null)
                return new CredentialContext()
                {
                    UserName = config.MALUName,
                    Password = config.MALPassword
                };
            else
                return Bot.MalClient;
        }
    }
}