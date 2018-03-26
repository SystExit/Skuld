using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Text;
using System.Reflection;

namespace Skuld.APIS
{
	public class WebHandler
	{
		static HttpWebRequest client;
		static string UAGENT = "Skuld/" + Assembly.GetEntryAssembly().GetName().Version + " (https://github.com/exsersewo/Skuld)";

		public static async Task<string> ReturnStringAsync(Uri url)
		{
			client = (HttpWebRequest)WebRequest.Create(url);
			client.UserAgent = UAGENT;

			var resp = (HttpWebResponse)(await client.GetResponseAsync());
			if (resp.StatusCode == HttpStatusCode.OK)
			{
				var reader = new StreamReader(resp.GetResponseStream());
				var responce = await reader.ReadToEndAsync();
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
        public static async Task<string> ReturnStringAsync(Uri url, byte[] headers)
        {
			client.Headers.Add("Basic", Convert.ToBase64String(headers));
			client.UserAgent = UAGENT;

			var resp = (HttpWebResponse)(await client.GetResponseAsync());
			if (resp.StatusCode == HttpStatusCode.OK)
			{
				var reader = new StreamReader(resp.GetResponseStream());
				var responce = await reader.ReadToEndAsync();
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
		public static async Task<HtmlDocument> ScrapeUrlAsync(Uri url)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = UAGENT;
			request.AllowAutoRedirect = true;
			var response = (HttpWebResponse)await request.GetResponseAsync();
			var doc = new HtmlDocument();
			if (response.StatusCode == HttpStatusCode.OK)
			{
				doc.Load(response.GetResponseStream(), Encoding.UTF8);
				request.Abort();
			}
			if (doc != null)
			{ return doc; }
			else
			{ return null; }
		}
		public static async Task<string> DownloadFileAsync(Uri url, string filepath)
		{
			var client = new WebClient();
			client.Headers.Add("User-Agent", UAGENT);
			await client.DownloadFileTaskAsync(url, filepath);
			client.Dispose();
			return filepath;
		}

		public static async Task<string> PostStringAsync(Uri url, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("User-Agent", UAGENT);
				client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
                using (HttpResponseMessage resp = await client.PostAsync(url, content))
                {
                    return await resp.Content.ReadAsStringAsync();
                }
            }
        }
        public static async Task<Stream> PostStreamAsync(Uri url, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("User-Agent", UAGENT);
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
        public static async Task<byte[]> PostByteArrayAsync(Uri url, HttpContent content)
        {
            using (HttpClient client = new HttpClient())
            {
				client.DefaultRequestHeaders.Add("User-Agent", UAGENT);
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
