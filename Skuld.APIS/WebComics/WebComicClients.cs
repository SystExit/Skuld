using Skuld.APIS.WebComics.CAD;
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
        private readonly CADClient CADClient;

        public WebComicClients(Random ran)
        {
            explosmClient = new ExplosmClient();
            XKCDClient = new XKCDClient(ran);
            CADClient = new CADClient();
        }

        #region CAD
        public async Task<object> GetCADComicAsync()
            => await CADClient.GetComicAsync().ConfigureAwait(false);
        #endregion CAD

        #region CAH

        public async Task<object> GetCAHComicAsync()
            => await explosmClient.GetComicAsync().ConfigureAwait(false);

        #endregion

        #region XKCD

        public async Task<object> GetRandomXKCDComic()
            => await XKCDClient.GetRandomComicAsync().ConfigureAwait(false);

        public async Task<object> GetXKCDComic(int id)
            => await XKCDClient.GetComicAsync(id).ConfigureAwait(false);

        #endregion
    }
}