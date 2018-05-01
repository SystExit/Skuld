using System;
using Newtonsoft.Json;
using System.IO;

namespace Skuld.Tools
{
    public class Config
    {
        [JsonIgnore]
        private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;
        //Bot Configuration
		public ConfigDiscord Discord { get; set; }
        //End Region

        //Database Information
		public ConfigSQL SQL { get; set; }
        //End Region

        //Variables
		public ConfigBotUtils Utils { get; set; }
        //End Region

        //Client Api Keys
		public ConfigAPIS APIS { get; set; }
        //End Region

        //Bot Listing Tokens
		public ConfigBotLists BotListing { get; set; }
        //End Region

        //Module Management
		public ConfigModules Modules { get; set; }
        //End Region        

        public Config()
        {
			//Bot
			Discord = new ConfigDiscord();

			//Sql
			SQL = new ConfigSQL();

			//Variables
			Utils = new ConfigBotUtils();

			//Clients
			APIS = new ConfigAPIS();

			//Modules
			Modules = new ConfigModules();

			//BotListing
			BotListing = new ConfigBotLists();
        }

        public void Save(string dir = "skuld/storage/configuration.json") 
			=> File.WriteAllText(Path.Combine(appdir, dir), JsonConvert.SerializeObject(this, Formatting.Indented));

        public static Config Load(string dir = "skuld/storage/configuration.json")
            => JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(appdir, dir)));
    }

	public class ConfigDiscord
	{
		public string Token { get; set; }
		public string Prefix { get; set; }
		public string AltPrefix { get; set; }
		public ulong[] Owners { get; set; }
		public ushort Shards { get; set; }

		public ConfigDiscord()
		{
			Token = "";
			Prefix = "";
			AltPrefix = "";
			Owners = new ulong[] { 0 };
			Shards = 0;
		}
	}

	public class ConfigSQL
	{
		public string Host { get; set; }
		public ushort Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Database { get; set; }

		public ConfigSQL()
		{
			Host = "";
			Port = 3306;
			Username = "";
			Password = "";
			Database = "";
		}
	}

	public class ConfigBotUtils
	{
		public int PinboardThreshold { get; set; }
		public int PinboardDateLimit { get; set; }
		public ulong DailyAmount { get; set; }
		public string MoneyName { get; set; }
		public string MoneySymbol { get; set; }

		public ConfigBotUtils()
		{
			PinboardThreshold = 5;
			PinboardDateLimit = 7;
			DailyAmount = 50;
			MoneyName = "";
			MoneySymbol = "";
		}
	}

	public class ConfigAPIS
	{
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

		public ConfigAPIS()
		{
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
		}
	}

	public class ConfigModules
	{
		public bool TwitchModule { get; set; }
		public bool AccountsModuleEnabled { get; set; }
		public bool ActionsModuleEnabled { get; set; }
		public bool AdminModuleEnabled { get; set; }
		public bool FunModuleEnabled { get; set; }
		public bool InformationModuleEnabled { get; set; }
		public bool SearchModuleEnabled { get; set; }
		public bool StatsModuleEnabled { get; set; }

		public ConfigModules()
		{
			TwitchModule = false;
			AccountsModuleEnabled = true;
			ActionsModuleEnabled = true;
			AdminModuleEnabled = true;
			FunModuleEnabled = true;
			InformationModuleEnabled = true;
			SearchModuleEnabled = true;
			StatsModuleEnabled = true;
		}
	}

	public class ConfigBotLists
	{
		public string DBotsOrgKey { get; set; }
		public string DiscordPWKey { get; set; }
		public string SysExToken { get; set; }

		public ConfigBotLists()
		{
			DBotsOrgKey = "";
			DiscordPWKey = "";
		}
	}
}
