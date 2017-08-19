using System;
using Newtonsoft.Json;
using System.IO;

namespace Skuld.Tools
{
    public class Config
    {
        [JsonIgnore]
        private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;
        public string Prefix { get; set; }
        public ulong[] Owners { get; set; }
        public string Token { get; set; }
        public ulong DailyAmount { get; set; }
        public string MALUName { get; set; }
        public string MALPassword { get; set; }
        public string GoogleAPI { get; set; }
        public string GoogleCx { get; set; }
        public int STANDSUid { get; set; }
        public string STANDSToken { get; set; }
        public int Shards { get; set; }
        public string SqlDBHost { get; set; }
        public string SqlUser { get; set; }
        public string SqlPass { get; set; }
        public string SqlDB { get; set; }
        public string MoneyName { get; set; }
        public string MoneySymbol { get; set; }
        public string TwitchToken { get; set; }
        public string TwitchClientID { get; set; }
        public string ImgurClientID { get; set; }
        public string ImgurClientSecret { get; set; }

        public Config()
        {
            Prefix = "";
            Owners = new ulong[] { 0 };
            Token = "";
            DailyAmount = 50;
            MALUName = "";
            MALPassword = "";
            GoogleAPI = "";
            GoogleCx = "";
            STANDSUid = 0;
            STANDSToken = "";
            Shards = 0;
            SqlDBHost = "";
            SqlUser = "";
            SqlPass = "";
            SqlDB = "";
            MoneyName = "";
            MoneySymbol = "";
            TwitchToken = "";
            TwitchClientID = "";
        }

        public void Save(string dir = "storage/configuration.json") { File.WriteAllText(Path.Combine(appdir, dir), JsonConvert.SerializeObject(this, Formatting.Indented)); }

        public static Config Load(string dir = "storage/configuration.json")
        {
            Bot.EnsureConfigExists();
            string file = Path.Combine(appdir, dir);
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
        }
    }
}
