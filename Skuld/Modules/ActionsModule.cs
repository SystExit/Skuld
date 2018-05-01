using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Skuld.APIS;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Skuld.Models.API.SysEx;
using System.Linq;
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
			else actiongifs = await sysExClient.GetAllWeebActionGifsAsync();
		}

		WeebGif GetWeebActionFromType(GifType type)
		{
			var sorted = actiongifs.Where(x => x.GifType == type).ToList();
			return sorted[random.Next(sorted.Count())];
		}

        [Command("slap"), Summary("Slap a user")]
        public async Task Slap([Remainder]IGuildUser guilduser)
        {
			await CheckFillGifs();

            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Slap);

            if (guilduser == contuser)
            {
				await SendAsync($"B-Baka.... {botguild.Mention} slapped {contuser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else
            {
				await SendAsync($"{contuser.Mention} slapped {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
        }
        
        [Command("kill"), Summary("Kills a user")]
        public async Task Kill([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();
			var gif = GetWeebActionFromType(GifType.Kill);

			var contuser = Context.User as IGuildUser;
            if (contuser == guilduser)
            {
				await SendAsync($"{contuser.Mention} killed themself", "http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif").ConfigureAwait(false);
			}
            else
            {
				await SendAsync($"{contuser.Mention} killed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
        }
        
        [Command("stab"), Summary("Stabs a user")]
        public async Task Stab([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Stab);

			if (contuser == guilduser)
            {
				await SendAsync($"URUSAI!! {botguild.Mention} stabbed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else if (guilduser.IsBot)
            {
				await SendAsync($"{contuser.Mention} stabbed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else
            {
                if (database.CanConnect)
                {
                    uint dhp = (uint)random.Next(0, 100);

					var usr = await database.GetUserAsync(guilduser.Id).ConfigureAwait(false);

                    if (usr == null)
                    {
						await InsertUser(guilduser).ConfigureAwait(false);
					}
                    else
                    {
                        usr.HP -= dhp;
                        if (usr.HP > 0)
                        {
							await database.UpdateUserAsync(usr);

                            await SendAsync($"{contuser.Mention} just stabbed {guilduser.Mention} for {dhp} HP, they now have {usr.HP} HP left", gif.URL).ConfigureAwait(false);
                        }
                        else
						{
							usr.HP = 0;
							await database.UpdateUserAsync(usr);

							await SendAsync($"{contuser.Mention} just stabbed {guilduser.Mention} for {dhp} HP, they now have {usr.HP} HP left", gif.URL).ConfigureAwait(false);
						}
                    }
                }
                else
				{
					await SendAsync($"{contuser.Mention} just stabbed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
				}
            }
        }
        
        [Command("hug"), Summary("hugs a user")]
        public async Task Hug([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Hug);

			if (guilduser == contuser)
            {
				await SendAsync($"{botguild.Mention} hugs {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else
            {
				await SendAsync($"{contuser.Mention} just hugged {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
        }
        
        [Command("punch"), Summary("Punch a user")]
        public async Task Punch([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Punch);

			if (guilduser == contuser)
            {
				await SendAsync($"URUSAI!! {botguild.Mention} just punched {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else
            {
				await SendAsync($"{contuser.Mention} just punched {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
        }

		[Command("shrug"), Summary("Shrugs")]
		public async Task Shrug()
		{
			await CheckFillGifs();
			var gif = GetWeebActionFromType(GifType.Shrug);

			await SendAsync($"{Context.User.Mention} shrugs.", gif.URL).ConfigureAwait(false);
		}
                
        [Command("adore"), Summary("Adore a user")]
        public async Task Adore([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Adore);

			if (guilduser == contuser)
            {
				await SendAsync($"I-it's not like I like you or anything... {botguild.Mention} adores {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else
            {
				await SendAsync($"{contuser.Mention} adores {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
        }

        [Command("kiss"), Summary("Kiss a user")]
        public async Task Kiss([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Kiss);

			if (guilduser == contuser)
            {
				await SendAsync($"I-it's not like I like you or anything... {botguild.Mention} just kissed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else
            {
				await SendAsync($"{contuser.Mention} just kissed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
        }

        [Command("grope"), Summary("Grope a user")]
        public async Task Grope([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Grope);

			if (guilduser == contuser)
            {
				await SendAsync($"{botguild.Mention} just groped {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else
            {
				await SendAsync($"{contuser.Mention} just groped {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
        }

        [Command("pet"), Summary("Pets a user")]
        public async Task Pet([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Pet);

			if (contuser == guilduser)
            {
				await SendAsync($"{botguild.Mention} just petted {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else if (guilduser.IsBot)
            {
				await SendAsync($"{contuser.Mention} just petted {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else
            {
                if (database.CanConnect)
                {
					var cusr = await database.GetUserAsync(Context.User.Id).ConfigureAwait(false);

					if (cusr == null)
                    {
                        await InsertUser(contuser).ConfigureAwait(false);
                    }
                    else
                    {
						cusr.Pets += 1;

						await database.UpdateUserAsync(cusr).ConfigureAwait(false);
						
						var gusr = await database.GetUserAsync(guilduser.Id).ConfigureAwait(false);
                        if (gusr == null)
                        {
                            await InsertUser(guilduser).ConfigureAwait(false);
                        }
                        else
                        {
							gusr.Petted += 1;

							await database.UpdateUserAsync(gusr).ConfigureAwait(false);

                            await SendAsync($"{contuser.Mention} just petted {guilduser.Mention}, they've been petted {gusr.Petted} time(s)!", gif.URL).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await SendAsync($"{contuser.Mention} just petted {guilduser.Mention}", gif.URL).ConfigureAwait(false);
                }
            }                
        }

        [Command("glare"),Summary("Glares at a user")]
        public async Task Glare([Remainder]IGuildUser guilduser)
		{
			await CheckFillGifs();

			var contuser = Context.User as IGuildUser;
			var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
			var gif = GetWeebActionFromType(GifType.Glare);

			if (contuser == guilduser)
            {
				await SendAsync($"{botguild.Mention} glares at {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else if (guilduser.IsBot)
            {
				await SendAsync($"{contuser.Mention} glares at {guilduser.Mention}", gif.URL).ConfigureAwait(false);
			}
            else
            {
                if (database.CanConnect)
                {
					var usr = await database.GetUserAsync(contuser.Id).ConfigureAwait(false);

                    if (usr == null)
                    {
                        await InsertUser(contuser).ConfigureAwait(false);
                    }
                    else
                    {
                        usr.Glares += 1;
						
						await database.UpdateUserAsync(usr).ConfigureAwait(false);
						
						var usr2 = await database.GetUserAsync(guilduser.Id).ConfigureAwait(false);

						if(usr2 == null)
						{
                            await InsertUser(guilduser).ConfigureAwait(false);
                        }
                        else
                        {
                            usr2.GlaredAt += 1;

							await database.UpdateUserAsync(usr2).ConfigureAwait(false);
							
							await SendAsync($"{contuser.Mention} glares at {guilduser.Mention}, they've been glared at {usr2.GlaredAt} time(s)!", gif.URL).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await SendAsync($"{contuser.Mention} glares at {guilduser.Mention}", gif.URL);
                }
            }
        }

        private async Task InsertUser(IUser user)
        {
            await database.InsertUserAsync(user);
        }
        private async Task SendAsync(string message, string image)
            => await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Description = message, Color = Tools.Tools.RandomColor(), ImageUrl = image }.Build());
    }
}