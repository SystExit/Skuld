using Skuld.APIS.WebComics.Explosm;
using Skuld.APIS.WebComics.XKCD;
using System;
using System.Threading.Tasks;
using Skuld.Core.Services;

namespace Skuld.APIS
{
    public class WebComicClients
    {
        private readonly ExplosmClient explosmClient;
        private readonly XKCDClient XKCDClient;

        public WebComicClients(GenericLogger log, Random ran)
        {
            explosmClient = new ExplosmClient(log);
            XKCDClient = new XKCDClient(ran, log);
        }

        //cah
        public async Task<object> GetCAHComicAsync()
            => await explosmClient.GetComicAsync();

        //xkcd
        public async Task<object> GetRandomXKCDComic()
            => await XKCDClient.GetRandomComicAsync();
        public async Task<object> GetXKCDComic(int id)
            => await XKCDClient.GetComicAsync(id);

    }
}