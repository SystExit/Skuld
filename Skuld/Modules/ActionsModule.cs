using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Skuld.Tools;
using Skuld.APIS;
using MySql.Data.MySqlClient;

namespace Skuld.Commands
{
    [Group, Name("Actions")]
    public class Actions : ModuleBase
    {
        [Command("slap", RunMode = RunMode.Async), Summary("Slap a user")]
        public async Task Slap([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
            { await Send($"B-Baka.... {botguild.Nickname ?? botguild.Username} slapped {contuser.Nickname ?? contuser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=slap"))).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} slapped {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=slap"))).ConfigureAwait(false); }
        }
        
        [Command("kill", RunMode = RunMode.Async), Summary("Kills a user")]
        public async Task Kill([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            if (contuser == guilduser)
            { await Send($"{contuser.Nickname ?? contuser.Username} killed themself", "http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif").ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} killed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=kill"))).ConfigureAwait(false); }
        }
        
        [Command("stab", RunMode = RunMode.Async), Summary("Stabs a user")]
        public async Task Stab([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (contuser == guilduser)
            { await Send($"URUSAI!! {botguild.Nickname ?? botguild.Username} stabbed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=stab"))).ConfigureAwait(false); }
            else if (guilduser.IsBot)
            { await Send($"{contuser.Nickname ?? contuser.Username} stabbed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=stab"))).ConfigureAwait(false); }
            else
            {
                if (!String.IsNullOrEmpty(Bot.Configuration.SqlDBHost))
                {
                    int dhp = Bot.random.Next(0, 100);
                    var command = new MySqlCommand("select HP from accounts where ID = @userid");
                    command.Parameters.AddWithValue("@userid", guilduser.Id);
                    var resp = await Bot.Database.GetSingleAsync(command);
                    if (String.IsNullOrEmpty(resp))
                    { await InsertUser(guilduser).ConfigureAwait(false); }
                    else
                    {
                        var uhp = Convert.ToUInt32(resp);
                        var hp = uhp - dhp;
                        if (hp > 0)
                        {
                            command = new MySqlCommand("UPDATE accounts SET hp = @hp where ID = @userid");
                            command.Parameters.AddWithValue("@hp", hp);
                            command.Parameters.AddWithValue("@userid", guilduser.Id);
                            await Bot.Database.InsertAsync(command);
                            await Send($"{contuser.Nickname ?? contuser.Username} just stabbed {guilduser.Nickname ?? guilduser.Username} for {dhp} HP, they now have {hp} HP left", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=stab"))).ConfigureAwait(false);
                        }
                        else
                        {
                            command = new MySqlCommand("UPDATE accounts SET hp = 0 where ID = @userid");
                            command.Parameters.AddWithValue("@userid", guilduser.Id);
                            await Bot.Database.InsertAsync(command);
                            await Send($"{contuser.Nickname ?? contuser.Username} just stabbed {guilduser.Nickname ?? guilduser.Username} for {dhp} HP, they have no HP left", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=stab"))).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await Send($"{contuser.Nickname ?? contuser.Username} just stabbed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=stab"))).ConfigureAwait(false);
                }
            }
        }
        
        [Command("hug", RunMode = RunMode.Async), Summary("hugs a user")]
        public async Task Hug([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
            { await Send($"{botguild.Nickname ?? botguild.Username} hugs {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=hug"))).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} just hugged {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=hug"))).ConfigureAwait(false); }
        }
        
        [Command("punch", RunMode = RunMode.Async), Summary("Punch a user")]
        public async Task Punch([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
            { await Send($"URUSAI!! {botguild.Nickname ?? botguild.Username} just punched {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=punch"))).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} just punched {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=punch"))).ConfigureAwait(false); }
        }

        [Command("shrug", RunMode = RunMode.Async), Summary("Shrugs")]
        public async Task Shrug() => 
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} shrugs.", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=shrug"))).ConfigureAwait(false);
                
        [Command("adore", RunMode = RunMode.Async), Summary("Adore a user")]
        public async Task Adore([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
            { await Send($"I-it's not like I like you or anything... {botguild.Nickname ?? botguild.Username} adores {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=adore"))).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} adores {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=adore"))).ConfigureAwait(false); }
        }

        [Command("kiss", RunMode = RunMode.Async), Summary("Kiss a user")]
        public async Task Kiss([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
            { await Send($"I-it's not like I like you or anything... {botguild.Nickname ?? botguild.Username} just kissed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=kiss"))).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} just kissed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=kiss"))).ConfigureAwait(false); }
        }

        [Command("grope", RunMode = RunMode.Async), Summary("Grope a user")]
        public async Task Grope([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
            { await Send($"{botguild.Nickname ?? botguild.Username} just groped {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=grope"))).ConfigureAwait(false); }
            else
            { await Send($"{contuser.Nickname ?? contuser.Username} just groped {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=grope"))).ConfigureAwait(false); }
        }

        [Command("pet", RunMode = RunMode.Async), Summary("Pets a user")]
        public async Task Pet([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (contuser == guilduser)
            { await Send($"{botguild.Nickname ?? botguild.Username} just petted {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=pet"))).ConfigureAwait(false); }
            else if (guilduser.IsBot)
            { await Send($"{contuser.Nickname ?? contuser.Username} just petted {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=pet"))).ConfigureAwait(false); }
            else
            {
                if (!String.IsNullOrEmpty(Bot.Configuration.SqlDBHost))
                {
                    var command = new MySqlCommand("SELECT pets from accounts where id = @userid");
                    command.Parameters.AddWithValue("@userid", Context.User.Id);

                    var resp1 = await Bot.Database.GetSingleAsync(command);
                    if (String.IsNullOrEmpty(resp1))
                    {
                        await InsertUser(contuser).ConfigureAwait(false);
                    }
                    else
                    {
                        int pets = Convert.ToInt32(resp1);
                        int newpets = pets + 1;

                        command = new MySqlCommand("UPDATE accounts SET pets = @newpets WHERE id = @userid");
                        command.Parameters.AddWithValue("@newpets", newpets);
                        command.Parameters.AddWithValue("@userid", contuser.Id);

                        await Bot.Database.InsertAsync(command);

                        command = new MySqlCommand("SELECT petted from accounts where id = @userid");
                        command.Parameters.AddWithValue("@userid", guilduser.Id);

                        var resp2 = await Bot.Database.GetSingleAsync(command);
                        if (resp2 == null)
                        {
                            await InsertUser(guilduser).ConfigureAwait(false);
                        }
                        else
                        {
                            int petted = Convert.ToInt32(resp2);
                            int newpetted = petted + 1;

                            command = new MySqlCommand("UPDATE accounts SET petted = @newpetted WHERE id = @userid");
                            command.Parameters.AddWithValue("@newpetted", newpetted);
                            command.Parameters.AddWithValue("@userid", guilduser.Id);

                            await Bot.Database.InsertAsync(command);
                            await Send($"{contuser.Nickname ?? contuser.Username} just petted {guilduser.Nickname ?? guilduser.Username}, they've been petted {newpetted} time(s)!", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=pet"))).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await Send($"{contuser.Nickname ?? contuser.Username} just petted {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=pet"))).ConfigureAwait(false);
                }
            }                
        }

        [Command("glare", RunMode = RunMode.Async),Summary("Glares at a user")]
        public async Task Glare([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (contuser == guilduser)
            { await Send($"{botguild.Nickname ?? botguild.Username} glares at {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=glare"))).ConfigureAwait(false); }
            else if (guilduser.IsBot)
            { await Send($"{contuser.Nickname ?? contuser.Username} glares at {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=glare"))).ConfigureAwait(false); }
            else
            {
                if (!String.IsNullOrEmpty(Bot.Configuration.SqlDBHost))
                {
                    var command = new MySqlCommand("SELECT glares from accounts where id = @userid");
                    command.Parameters.AddWithValue("@userid", contuser.Id);
                    var resp1 = await Bot.Database.GetSingleAsync(command);

                    if (String.IsNullOrEmpty(resp1))
                    {
                        await InsertUser(contuser).ConfigureAwait(false);
                    }
                    else
                    {
                        int glares = Convert.ToInt32(resp1);
                        int newglares = glares + 1;

                        command = new MySqlCommand("UPDATE accounts SET glares = @newglares WHERE id = @userid");
                        command.Parameters.AddWithValue("@newglares", newglares);
                        command.Parameters.AddWithValue("@userid", contuser.Id);
                        await Bot.Database.InsertAsync(command);

                        command = new MySqlCommand("SELECT glaredat from accounts where id = @userid");
                        command.Parameters.AddWithValue("@userid", guilduser.Id);
                        var resp2 = await Bot.Database.GetSingleAsync(command);

                        if (String.IsNullOrEmpty(resp2))
                        {
                            await InsertUser(guilduser).ConfigureAwait(false);
                        }
                        else
                        {
                            int glaredat = Convert.ToInt32(resp2);
                            int newglaredat = glaredat + 1;

                            command = new MySqlCommand("UPDATE accounts SET glaredat = @glaredat WHERE id = @userid");
                            command.Parameters.AddWithValue("@glaredat", newglaredat);
                            command.Parameters.AddWithValue("@userid", guilduser.Id);
                            await Bot.Database.InsertAsync(command);

                            await Send($"{contuser.Nickname ?? contuser.Username} glares at {guilduser.Nickname ?? guilduser.Username}, they've been glared at {newglaredat} time(s)!", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=glare"))).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await Send($"{contuser.Nickname ?? contuser.Username} glares at {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://gaia.systemexit.co.uk/gifs/actions/?f=glare")));
                }
            }
        }

        private async Task InsertUser(IUser user)
        {
            var command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username, \"I have no description\");");
            command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\").Replace("\'", "\\'")}");
            command.Parameters.AddWithValue("@userid",user.Id);
            await Bot.Database.InsertAsync(command);
        }
        private async Task Send(string message, string image)
            => await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder() { Description = message, Color = Tools.Tools.RandomColor(), ImageUrl = image }.Build());
    }
}