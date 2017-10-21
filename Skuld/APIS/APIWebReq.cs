using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Text;

namespace Skuld.APIS
{
    public partial class APIWebReq
    {
        public static async Task<string> ReturnString(Uri url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var responce = await resp.Content.ReadAsStringAsync();
                        resp.Dispose();
                        client.Dispose();
                        return responce;
                    }
                    else
                    {
                        resp.Dispose();
                        client.Dispose();
                        return null;
                    }
                }
            }
        }
        public static async Task<Stream> ReturnStream(Uri url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var responce = await resp.Content.ReadAsStreamAsync();
                        resp.Dispose();
                        client.Dispose();
                        return responce;
                    }
                    else
                    {
                        resp.Dispose();
                        client.Dispose();
                        return null;
                    }
                }
            }
        }
        public static async Task<byte[]> ReturnByteArray(Uri url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var responce = await resp.Content.ReadAsByteArrayAsync();
                        resp.Dispose();
                        client.Dispose();
                        return responce;
                    }
                    else
                    {
                        resp.Dispose();
                        client.Dispose();
                        return null;
                    }
                }
            }
        }
        public static async Task<string> ReturnString(Uri url, byte[] headers)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(headers));
                using (HttpResponseMessage resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var responce = await resp.Content.ReadAsStringAsync();
                        resp.Dispose();
                        client.Dispose();
                        return responce;
                    }
                    else
                    {
                        resp.Dispose();
                        client.Dispose();
                        return null;
                    }
                }
            }
        }
        public static async Task<Stream> ReturnStream(Uri url, byte[] headers)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(headers));
                using (HttpResponseMessage resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var responce = await resp.Content.ReadAsStreamAsync();
                        resp.Dispose();
                        client.Dispose();
                        return responce;
                    }
                    else
                    {
                        resp.Dispose();
                        client.Dispose();
                        return null;
                    }
                }
            }
        }
        public static async Task<byte[]> ReturnByteArray(Uri url, byte[] headers)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(headers));
                using (HttpResponseMessage resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var responce = await resp.Content.ReadAsByteArrayAsync();
                        resp.Dispose();
                        client.Dispose();
                        return responce;
                    }
                    else
                    {
                        resp.Dispose();
                        client.Dispose();
                        return null;
                    }
                }
            }
        }
        public static async Task<string> PostString(Uri url, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                using (HttpResponseMessage resp = await client.PostAsync(url, content))
                {
                    return await resp.Content.ReadAsStringAsync();
                }
            }
        }
        public static async Task<Stream> PostStream(Uri url, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                using (HttpResponseMessage resp = await client.PostAsync(url, content))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        return await resp.Content.ReadAsStreamAsync();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        public static async Task<byte[]> PostByteArray(Uri url, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                using (HttpResponseMessage resp = await client.PostAsync(url, content))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        return await resp.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        public static Task<string> DownloadFile(Uri url, string filepath)
        {
            var client = new WebClient();
            var thing = client.DownloadFileTaskAsync(url, filepath);
            while (thing.Status != TaskStatus.RanToCompletion)
            { }         
            return Task.FromResult(filepath);
        }
        public static async Task<HtmlDocument> ScrapeUrl(Uri url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = true;
            var response = (HttpWebResponse)await request.GetResponseAsync();
            string data = null;
            var doc = new HtmlDocument();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                doc.Load(response.GetResponseStream(), Encoding.UTF8);
                request.Abort();
            }
            if (doc!=null)
                return doc;
            else
                return null;
        }
    }
}
