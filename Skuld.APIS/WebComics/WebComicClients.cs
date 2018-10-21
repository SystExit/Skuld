using Skuld.APIS.WebComics.Explosm;
using Skuld.APIS.WebComics.XKCD;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class WebComicClients
    {
        private readonly ExplosmClient explosmClient;
        private readonly XKCDClient XKCDClient;

        public WebComicClients(Random ran)
        {
            explosmClient = new ExplosmClient();
            XKCDClient = new XKCDClient(ran);
        }

        //cah
        public async Task<object> GetCAHComicAsync()
            => await explosmClient.GetComicAsync().ConfigureAwait(false);

        //xkcd
        public async Task<object> GetRandomXKCDComic()
            => await XKCDClient.GetRandomComicAsync().ConfigureAwait(false);

        public async Task<object> GetXKCDComic(int id)
            => await XKCDClient.GetComicAsync(id).ConfigureAwait(false);
    }
}