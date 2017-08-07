using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace Skuld.APIS
{
    public class APIWebReq
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
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(headers));
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
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(headers));
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
    }
}
