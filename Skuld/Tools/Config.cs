using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Skuld.Tools
{
    public class Config
    {
        [JsonIgnore]
        private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;
        //Bot Configuration
        public string Prefix { get; set; }
        public ulong[] Owners { get; set; }
        public string Token { get; set; }
        public int Shards { get; set; }
        //End Region

        //Database Information
        public string SqlDBHost { get; set; }
        public string SqlUser { get; set; }
        public string SqlPass { get; set; }
        public string SqlDB { get; set; }
        //End Region

        //Variables
        public int StarboardThreshold { get; set; }
        public int PinboardThreshold { get; set; }
        public int StarboardDateLimit { get; set; }
        public int PinboardDateLimit { get; set; }
        public ulong DailyAmount { get; set; }
        public string MoneyName { get; set; }
        public string MoneySymbol { get; set; }
        //End Region

        //Client Api Keys
        public string MALUName { get; set; }
        public string MALPassword { get; set; }
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
        //End Region

        //Bot Listing Tokens
        public string DBotsOrgKey { get; set; }
        public string DiscordPWKey { get; set; }
        //End Region

        //Module Management
        public bool TwitchModule { get; set; }
        public bool AccountsModuleEnabled { get; set; }
        public bool ActionsModuleEnabled { get; set; }
        public bool AdminModuleEnabled { get; set; }
        public bool FunModuleEnabled { get; set; }
        public bool InformationModuleEnabled { get; set; }
        public bool SearchModuleEnabled { get; set; }
        public bool StatsModuleEnabled { get; set; }
        //End Region        

        public Config()
        {
            //Bot
            Prefix = "";
            Owners = new ulong[] { 0 };
            Token = "";
            Shards = 0;

            //Sql
            SqlDBHost = "";
            SqlUser = "";
            SqlPass = "";
            SqlDB = "";

            //Variables
            StarboardThreshold = 5;
            PinboardThreshold = 5;
            StarboardDateLimit = 7;
            PinboardDateLimit = 7;
            DailyAmount = 50;
            MoneyName = "";
            MoneySymbol = "";

            //Clients
            MALUName = "";
            MALPassword = "";
            GoogleAPI = "";
            GoogleCx = "";
            STANDSUid = 0;
            STANDSToken = "";
            TwitchToken = "";
            TwitchClientID = "";
            NASAApiKey = "";
            DataDogHost = "127.0.0.1";

            //Modules
            TwitchModule = false;
            AccountsModuleEnabled = true;
            ActionsModuleEnabled = true;
            AdminModuleEnabled = true;
            FunModuleEnabled = true;
            InformationModuleEnabled = true;
            SearchModuleEnabled = true;
            StatsModuleEnabled = true;
        }

        public void Save(string dir = "skuld/storage/configuration.json") { File.WriteAllText(Path.Combine(appdir, dir), JsonConvert.SerializeObject(this, Formatting.Indented)); }

        public static Config Load(string dir = "skuld/storage/configuration.json")
        {
            Bot.EnsureConfigExists();
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(appdir, dir)));
        }
    }
}
