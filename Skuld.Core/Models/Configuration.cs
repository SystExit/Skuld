using Newtonsoft.Json;
using System;
using System.IO;

namespace Skuld.Core.Models
{
    public class SkuldConfig
    {
        [JsonIgnore]
        private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;

        //Bot Configuration
        public DiscordConfig Discord { get; set; }

        //Database Information
        public DatabaseConfig SQL { get; set; }

        //Variables
        public BotPreferences Preferences { get; set; }

        //Client Api Keys
        public APIConfig APIS { get; set; }

        //Bot Listing Tokens
        public BotListingAPI BotListing { get; set; }

        //Module Management
        public ModuleOverride Modules { get; set; }

        public SkuldConfig()
        {
            //Bot
            Discord = new DiscordConfig();

            //Sql
            SQL = new DatabaseConfig();

            //Variables
            Preferences = new BotPreferences();

            //Clients
            APIS = new APIConfig();

            //Modules
            Modules = new ModuleOverride();

            //BotListing
            BotListing = new BotListingAPI();
        }

        public void Save(string dir = "skuld/storage/configuration.json")
            => File.WriteAllText(Path.Combine(appdir, dir), JsonConvert.SerializeObject(this, Formatting.Indented));

        public static SkuldConfig Load(string dir = "skuld/storage/configuration.json")
            => JsonConvert.DeserializeObject<SkuldConfig>(File.ReadAllText(Path.Combine(appdir, dir)));
    }

    public class DiscordConfig
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string AltPrefix { get; set; }
        public ulong[] Owners { get; set; }
        public ushort Shards { get; set; }

        public DiscordConfig()
        {
            Token = "";
            Prefix = "";
            AltPrefix = "";
            Owners = new ulong[] { 0 };
            Shards = 0;
        }
    }

    public class DatabaseConfig
    {
        public string Host { get; set; }
        public ushort Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public bool SSL { get; set; }

        public DatabaseConfig()
        {
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
        public string DiscordPWKey { get; set; }
        public string SysExToken { get; set; }

        public BotListingAPI()
        {
            DBotsOrgKey = "";
            DiscordPWKey = "";
        }
    }
}