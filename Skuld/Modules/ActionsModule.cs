using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Skuld.Utilities;
using Skuld.APIS;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Skuld.Models.API.SysEx;
using System.Linq;
using Discord.WebSocket;
using Skuld.Services;

namespace Skuld.Modules
{
    [Group, Name("Actions")]
    public class Actions : ModuleBase<ShardedCommandContext>
    {
		readonly Random random;
		readonly DatabaseService database;
		readonly MessageService messageService;
		readonly SysExClient sysExClient;

		public Actions(Random rnd,
			DatabaseService db,
			SysExClient sysEx,
			MessageService msg)
		{
			random = rnd;
			database = db;
			sysExClient = sysEx;
			messageService = msg;
		}

		List<WeebGif> actiongifs;
		async Task CheckFillGifs()
		{
			if (actiongifs != null) return;
			else
			{
				actiongifs = await sysExClient.GetAllWeebActionGifsAsync();
			}
		}

		WeebGif GetWeebActionFromType(GifType type)
		{
			var sorted = actiongifs.Where(x => x.GifType == type).ToList();
			return sorted[random.Next(sorted.Count())];
		}

        [Command("slap", RunMode = RunMode.Async), Summary("Slap a user")]
        public async Task Slap([Remainder]IGuildUser guilduser)
        {
			await CheckFillGifs();

            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Slap);

            if (guilduser == contuser)
            { await Send($"B-Baka.... {botguild.Nickname ?? botguild.Username} slapped {contuser.Nickname ?? contuser.Username}", gif.URL).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} slapped {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
        }
        
        [Command("kill", RunMode = RunMode.Async), Summary("Kills a user")]
        public async Task Kill([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();
			var gif = GetWeebActionFromType(GifType.Kill);

			var contuser = Context.User as IGuildUser;
            if (contuser == guilduser)
            { await Send($"{contuser.Nickname ?? contuser.Username} killed themself", "http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif").ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} killed {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
        }
        
        [Command("stab", RunMode = RunMode.Async), Summary("Stabs a user")]
        public async Task Stab([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Stab);

			if (contuser == guilduser)
            { await Send($"URUSAI!! {botguild.Nickname ?? botguild.Username} stabbed {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else if (guilduser.IsBot)
            { await Send($"{contuser.Nickname ?? contuser.Username} stabbed {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else
            {
                if (!String.IsNullOrEmpty(Bot.Configuration.SqlDBHost)&&database.CanConnect&&database.CanConnect)
                {
                    int dhp = random.Next(0, 100);

					var usr = await database.GetUserAsync(guilduser.Id).ConfigureAwait(false);

                    if (usr == null)
                    { await InsertUser(guilduser).ConfigureAwait(false); }
                    else
                    {
                        var hp = usr.HP - dhp;
                        if (hp > 0)
                        {
							await database.ModifyUserAsync((Discord.WebSocket.SocketUser)guilduser, "hp", Convert.ToString(hp)).ConfigureAwait(false);

                            await Send($"{contuser.Nickname ?? contuser.Username} just stabbed {guilduser.Nickname ?? guilduser.Username} for {dhp} HP, they now have {hp} HP left", gif.URL).ConfigureAwait(false);
                        }
                        else
						{
							await database.ModifyUserAsync((Discord.WebSocket.SocketUser)guilduser, "hp", "0").ConfigureAwait(false);

							await Send($"{contuser.Nickname ?? contuser.Username} just stabbed {guilduser.Nickname ?? guilduser.Username} for {dhp} HP, they have no HP left", gif.URL).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await Send($"{contuser.Nickname ?? contuser.Username} just stabbed {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false);
                }
            }
        }
        
        [Command("hug", RunMode = RunMode.Async), Summary("hugs a user")]
        public async Task Hug([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Hug);

			if (guilduser == contuser)
            { await Send($"{botguild.Nickname ?? botguild.Username} hugs {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} just hugged {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
        }
        
        [Command("punch", RunMode = RunMode.Async), Summary("Punch a user")]
        public async Task Punch([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Punch);

			if (guilduser == contuser)
            { await Send($"URUSAI!! {botguild.Nickname ?? botguild.Username} just punched {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} just punched {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
        }

		[Command("shrug", RunMode = RunMode.Async), Summary("Shrugs")]
		public async Task Shrug()
		{
			await CheckFillGifs();
			var gif = GetWeebActionFromType(GifType.Shrug);

			await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} shrugs.", gif.URL).ConfigureAwait(false);
		}
                
        [Command("adore", RunMode = RunMode.Async), Summary("Adore a user")]
        public async Task Adore([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Adore);

			if (guilduser == contuser)
            { await Send($"I-it's not like I like you or anything... {botguild.Nickname ?? botguild.Username} adores {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} adores {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
        }

        [Command("kiss", RunMode = RunMode.Async), Summary("Kiss a user")]
        public async Task Kiss([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Kiss);

			if (guilduser == contuser)
            { await Send($"I-it's not like I like you or anything... {botguild.Nickname ?? botguild.Username} just kissed {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} just kissed {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
        }

        [Command("grope", RunMode = RunMode.Async), Summary("Grope a user")]
        public async Task Grope([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Grope);

			if (guilduser == contuser)
            { await Send($"{botguild.Nickname ?? botguild.Username} just groped {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} just groped {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
        }

        [Command("pet", RunMode = RunMode.Async), Summary("Pets a user")]
        public async Task Pet([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Pet);

			if (contuser == guilduser)
            { await Send($"{botguild.Nickname ?? botguild.Username} just petted {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else if (guilduser.IsBot)
            { await Send($"{contuser.Nickname ?? contuser.Username} just petted {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else
            {
                if (!String.IsNullOrEmpty(Bot.Configuration.SqlDBHost)&&database.CanConnect&&database.CanConnect)
                {
					var cusr = await database.GetUserAsync(Context.User.Id).ConfigureAwait(false);

					if (cusr == null)
                    {
                        await InsertUser(contuser).ConfigureAwait(false);
                    }
                    else
                    {
                        uint newpets = cusr.Pets + 1;

						await database.ModifyUserAsync((Discord.WebSocket.SocketUser)Context.User, "pets", Convert.ToString(newpets)).ConfigureAwait(false);
						
						var gusr = await database.GetUserAsync(guilduser.Id).ConfigureAwait(false);
                        if (gusr == null)
                        {
                            await InsertUser(guilduser).ConfigureAwait(false);
                        }
                        else
                        {
                            uint newpetted = gusr.Petted + 1;

							await database.ModifyUserAsync((Discord.WebSocket.SocketUser)guilduser, "petted", Convert.ToString(newpetted)).ConfigureAwait(false);

                            await Send($"{contuser.Nickname ?? contuser.Username} just petted {guilduser.Nickname ?? guilduser.Username}, they've been petted {newpetted} time(s)!", gif.URL).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await Send($"{contuser.Nickname ?? contuser.Username} just petted {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false);
                }
            }                
        }

        [Command("glare", RunMode = RunMode.Async),Summary("Glares at a user")]
        public async Task Glare([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Glare);

			if (contuser == guilduser)
            { await Send($"{botguild.Nickname ?? botguild.Username} glares at {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else if (guilduser.IsBot)
            { await Send($"{contuser.Nickname ?? contuser.Username} glares at {guilduser.Nickname ?? guilduser.Username}", gif.URL).ConfigureAwait(false); }
            else
            {
                if (!String.IsNullOrEmpty(Bot.Configuration.SqlDBHost)&&database.CanConnect)
                {
					var usr = await database.GetUserAsync(contuser.Id).ConfigureAwait(false);

                    if (usr == null)
                    {
                        await InsertUser(contuser).ConfigureAwait(false);
                    }
                    else
                    {
                        uint newglares = usr.Glares + 1;
						
						await database.ModifyUserAsync((Discord.WebSocket.SocketUser)contuser, "glares", Convert.ToString(newglares)).ConfigureAwait(false);
						
						var usr2 = await database.GetUserAsync(guilduser.Id).ConfigureAwait(false);

						if(usr2 == null)
						{
                            await InsertUser(guilduser).ConfigureAwait(false);
                        }
                        else
                        {
                            uint newglaredat = usr2.GlaredAt + 1;

							await database.ModifyUserAsync((Discord.WebSocket.SocketUser)guilduser, "glaredat", Convert.ToString(newglaredat)).ConfigureAwait(false);
							
							await Send($"{contuser.Nickname ?? contuser.Username} glares at {guilduser.Nickname ?? guilduser.Username}, they've been glared at {newglaredat} time(s)!", gif.URL).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await Send($"{contuser.Nickname ?? contuser.Username} glares at {guilduser.Nickname ?? guilduser.Username}", gif.URL);
                }
            }
        }

        private async Task InsertUser(IUser user)
        {
            var command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID` , `description`) VALUES (@userid , \"I have no description\");");
            command.Parameters.AddWithValue("@userid",user.Id);
            await database.NonQueryAsync(command);
        }
        private async Task Send(string message, string image)
            => await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder() { Description = message, Color = Tools.Tools.RandomColor(), ImageUrl = image }.Build());
    }
}