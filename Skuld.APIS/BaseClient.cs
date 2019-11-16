using HtmlAgilityPack;
using Skuld.Core;
using Skuld.Core.Utilities;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class BaseClient
    {
        public BaseClient()
        {
        }

        public static string UAGENT = "Mozilla/5.0 (compatible; SkuldBot/" + SkuldAppContext.Skuld.Key.Version.ToString().Substring(0, 3) + "; +https://github.com/Skuldbot/Skuld/)";

        public HttpWebRequest CreateWebRequest(Uri uri, byte[] auth = null)
        {
            var returncli = (HttpWebRequest)WebRequest.Create(uri);
            if (auth != null)
            {
                returncli.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(auth));
            }
            returncli.UserAgent = UAGENT;
            returncli.AllowAutoRedirect = true;
            returncli.KeepAlive = false;
            returncli.Timeout = 20000;
            returncli.ProtocolVersion = HttpVersion.Version11;

            return returncli;
        }

        public async Task<string> ReturnStringAsync(Uri url, byte[] headers = null)
        {
            try
            {
                var client = CreateWebRequest(url, headers);

                var resp = (HttpWebResponse)(await client.GetResponseAsync().ConfigureAwait(false));
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var reader = new StreamReader(resp.GetResponseStream());
                    var responce = await reader.ReadToEndAsync().ConfigureAwait(false);
                    StatsdClient.DogStatsd.Increment("web.get");
                    resp.Dispose();
                    client.Abort();
                    return responce;
                }
                else
                {
                    resp.Dispose();
                    client.Abort();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("WebHandler", ex.Message, ex);
                return null;
            }
        }

        public async Task<(HtmlDocument, Uri)> ScrapeUrlAsync(Uri url)
        {
            try
            {
                var request = CreateWebRequest(url);
                request.Timeout = 2000;
                request.UserAgent = UAGENT;

                try
                {
                    var response = (HttpWebResponse)await request.GetResponseAsync();
                    var doc = new HtmlDocument();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        doc.Load(response.GetResponseStream(), Encoding.UTF8);
                        StatsdClient.DogStatsd.Increment("web.get");
                        request.Abort();
                    }
                    if (doc != null)
                        return (doc, response.ResponseUri);
                    else
                        return (null, response.ResponseUri);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        return (null, null);
                    }
                    throw;
                }
                catch
                {
                    return (null, null);
                }
            }
            catch (Exception ex)
            {
                Log.Error("WebHandler", ex.Message, ex);
                return (null, null);
            }
        }

        public async Task<string> DownloadFileAsync(Uri url, string filepath)
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", UAGENT);
            await client.DownloadFileTaskAsync(url, filepath).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.download");
            client.Dispose();
            return filepath;
        }

        public async Task<string> PostStringAsync(Uri url, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", UAGENT);
                client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                using (HttpResponseMessage resp = await client.PostAsync(url, content))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        StatsdClient.DogStatsd.Increment("web.post");
                        return await resp.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public async Task<Stream> PostStreamAsync(Uri url, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", UAGENT);
                client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                using (HttpResponseMessage resp = await client.PostAsync(url, content))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        StatsdClient.DogStatsd.Increment("web.post");
                        return await resp.Content.ReadAsStreamAsync();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public async Task<byte[]> PostByteArrayAsync(Uri url, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", UAGENT);
                client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                using (HttpResponseMessage resp = await client.PostAsync(url, content))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        StatsdClient.DogStatsd.Increment("web.post");
                        return await resp.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}