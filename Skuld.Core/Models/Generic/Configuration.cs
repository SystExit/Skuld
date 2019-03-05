using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using Discor = Discord;

namespace Skuld.Core.Models
{
    public class SkuldConfig
    {
        [JsonIgnore]
        private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;

        //Bot Configuration
        public DiscordConfig Discord { get; set; }

        //SQL Information
        public DatabaseConfig SQL { get; set; }

        //Variables
        public BotPreferences Preferences { get; set; }

        //Client Api Keys
        public APIConfig APIS { get; set; }

        //Bot Listing Tokens
        public BotListingAPI BotListing { get; set; }

        //Module Management
        public ModuleOverride Modules { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Discor.LogSeverity LogLevel { get; set; }

        public SkuldConfig()
        {
            //Bot
            Discord = new DiscordConfig();

            //SQL Configuration
            SQL = new DatabaseConfig();

            //Variables
            Preferences = new BotPreferences();

            //Clients
            APIS = new APIConfig();

            //Modules
            Modules = new ModuleOverride();

            //BotListing
            BotListing = new BotListingAPI();

            //Logger
            LogLevel = Discor.LogSeverity.Verbose;
        }

        public void Save(string dir = "configuration.json")
            => File.WriteAllText(Path.Combine(appdir, dir), JsonConvert.SerializeObject(this, Formatting.Indented));

        public static SkuldConfig Load(string dir = "configuration.json")
            => JsonConvert.DeserializeObject<SkuldConfig>(File.ReadAllText(Path.Combine(appdir, dir)));
    }

    public class DiscordConfig
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string AltPrefix { get; set; }
        public ulong[] BotAdmins { get; set; }
        public ushort Shards { get; set; }

        public DiscordConfig()
        {
            Token = "";
            Prefix = "";
            AltPrefix = "";
            BotAdmins = new ulong[] { 160256824099078144 };
            Shards = 0;
        }
    }

    public class DatabaseConfig
    {
        public bool Enabled { get; set; }
        public string Host { get; set; }
        public ushort Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public bool SSL { get; set; }

        public DatabaseConfig()
        {
            Enabled = false;
            Host = "";
            Port = 3306;
            Username = "";
            Password = "";
            Database = "";
        }
    }

    public class BotPreferences
    {
        public int PinboardThreshold { get; set; }
        public int PinboardDateLimit { get; set; }
        public ulong DailyAmount { get; set; }
        public string MoneyName { get; set; }
        public string MoneySymbol { get; set; }

        public BotPreferences()
        {
            PinboardThreshold = 5;
            PinboardDateLimit = 7;
            DailyAmount = 50;
            MoneyName = "";
            MoneySymbol = "";
        }
    }

    public class APIConfig
    {
        public string GoogleAPI { get; set; }
        public string GoogleCx { get; set; }
        public int STANDSUid { get; set; }
        public string STANDSToken { get; set; }
        public string TwitchToken { get; set; }
        public string TwitchClientID { get; set; }
        public string ImgurClientID { get; set; }
        public string ImgurClientSecret { get; set; }
        public string NASAApiKey { get; set; }
        public string DataDogHost { get; set; }
        public ushort? DataDogPort { get; set; }

        public APIConfig()
        {
            GoogleAPI = "";
            GoogleCx = "";
            STANDSUid = 0;
            STANDSToken = "";
            TwitchToken = "";
            TwitchClientID = "";
            NASAApiKey = "";
            DataDogHost = "127.0.0.1";
            DataDogPort = 8125;
            ImgurClientID = "";
            ImgurClientSecret = "";
        }
    }

    public class ModuleOverride
    {
        public bool TwitchModule { get; set; }

        public ModuleOverride()
        {
            TwitchModule = false;
        }
    }

    public class BotListingAPI
    {
        public string DBotsOrgKey { get; set; }
        public string DiscordGGKey { get; set; }
        public string SysExToken { get; set; }
        public string B4DToken { get; set; }

        public BotListingAPI()
        {
            DBotsOrgKey = "";
            DiscordGGKey = "";
            SysExToken = "";
            B4DToken = "";
        }
    }
}