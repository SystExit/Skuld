﻿using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Text;
using System.Reflection;
using Skuld.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Skuld.APIS
{
	public class WebHandler
	{
		static LoggingService loggingService { get { return Bot.services.GetRequiredService<LoggingService>(); } }

		static string UAGENT = "Mozilla/5.0 (compatible; SkuldBot/" + Assembly.GetEntryAssembly().GetName().Version.ToString().Substring(0, 3) + "; +https://github.com/exsersewo/Skuld/)";

		public static HttpWebRequest CreateWebRequest(Uri uri, byte[] auth = null)
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
			returncli.ProtocolVersion = HttpVersion.Version10;

			return returncli;
		}

		public static async Task<string> ReturnStringAsync(Uri url)
		{
			try
			{
				var client = CreateWebRequest(url);

				var resp = (HttpWebResponse)(await client.GetResponseAsync());
				if (resp.StatusCode == HttpStatusCode.OK)
				{
					var reader = new StreamReader(resp.GetResponseStream());
					var responce = await reader.ReadToEndAsync();
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
			catch(Exception ex)
			{
				await loggingService.AddToLogsAsync(new Models.LogMessage("WebHandler", ex.Message, Discord.LogSeverity.Error, ex));
				return null;
			}
		}
        public static async Task<string> ReturnStringAsync(Uri url, byte[] headers)
		{
			try
			{
				var client = CreateWebRequest(url, headers);

				var resp = (HttpWebResponse)(await client.GetResponseAsync());
				if (resp.StatusCode == HttpStatusCode.OK)
				{
					var reader = new StreamReader(resp.GetResponseStream());
					var responce = await reader.ReadToEndAsync();
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
				await loggingService.AddToLogsAsync(new Models.LogMessage("WebHandler", ex.Message, Discord.LogSeverity.Error, ex));
				return null;
			}
		}
		public static async Task<HtmlDocument> ScrapeUrlAsync(Uri url)
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
						return doc;
					else
						return null;
				}
				catch (WebException ex)
				{
					if (ex.Status == WebExceptionStatus.Timeout)
					{
						return null;
					}
					throw;
				}
				catch
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				await loggingService.AddToLogsAsync(new Models.LogMessage("WebHandler", ex.Message, Discord.LogSeverity.Error, ex));
				return null;
			}
		}
		public static async Task<string> DownloadFileAsync(Uri url, string filepath)
		{
			var client = new WebClient();
			client.Headers.Add("User-Agent", UAGENT);
			await client.DownloadFileTaskAsync(url, filepath);
            StatsdClient.DogStatsd.Increment("web.download");
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
