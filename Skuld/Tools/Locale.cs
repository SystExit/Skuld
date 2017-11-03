using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using Skuld.Languages;

namespace Skuld.Tools
{
	public class Locale
	{
		public static Dictionary<string, ResourceManager> Locales = new Dictionary<string, ResourceManager>();
		public static Dictionary<string, string> LocaleHumanNames = new Dictionary<string, string>();

		public static string defaultLocale = "en-GB";

		public static void InitialiseLocales()
		{
			Locales.Add("en-GB", en_GB.ResourceManager);
			LocaleHumanNames.Add("English (Great Britain)", "en-GB");

			Locales.Add("nl-nl", nl_nl.ResourceManager);
			LocaleHumanNames.Add("Dutch (Netherlands)", "nl-nl");

			Locales.Add("fi-FI", fi_FI.ResourceManager);
			LocaleHumanNames.Add("Finnish (Finland)", "fi-FI");

			Locales.Add("tr-TR", tr_TR.ResourceManager);
			LocaleHumanNames.Add("Turkish (Turkey)", "tr-TR");


			Bot.Logs.Add(new Models.LogMessage("LocaleInit", "Initialized all the languages", Discord.LogSeverity.Info));
		}

		public static ResourceManager GetLocale(string id)
		{
			var locale = Locales.FirstOrDefault(x => x.Key == id);
			if (locale.Value != null)
				return locale.Value;
			else
				return null;
		}
	}
}
