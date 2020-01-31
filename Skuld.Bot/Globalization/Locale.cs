using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;

namespace Skuld.Bot.Globalization
{
    public class Locale
    {
        private static Dictionary<string, ResourceManager> locales = new Dictionary<string, ResourceManager>();
        private static Dictionary<string, string> localehumannames = new Dictionary<string, string>();
        public Dictionary<string, ResourceManager> Locales { get => locales; }
        public Dictionary<string, string> LocaleHumanNames { get => localehumannames; }
        public static readonly string defaultLocale = "en-GB";

        public Locale()
        {
        }

        public async Task InitialiseLocalesAsync()
        {
            locales.Add("en-GB", en_GB.ResourceManager);
            localehumannames.Add("English (Great Britain)", "en-GB");

            locales.Add("nl-nl", nl_nl.ResourceManager);
            localehumannames.Add("Dutch (Netherlands)", "nl-nl");

            locales.Add("fi-FI", fi_FI.ResourceManager);
            localehumannames.Add("Finnish (Finland)", "fi-FI");

            locales.Add("tr-TR", tr_TR.ResourceManager);
            localehumannames.Add("Turkish (Turkey)", "tr-TR");

            //await GenericLogger.AddToLogsAsync(new LogMessage("LocaleInit", "Initialized all the languages", Discord.LogSeverity.Info));
        }

        public ResourceManager GetLocale(string id)
        {
            var locale = locales.FirstOrDefault(x => x.Key == id);
            if (locale.Value != null)
                return locale.Value;
            else
                return null;
        }
    }
}