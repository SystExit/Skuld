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
        //Users
        [Command("slap", RunMode = RunMode.Async), Summary("Slap a user")]
        public async Task Slap([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
                await Send($"B-Baka.... {botguild.Nickname ?? botguild.Username} slapped {contuser.Nickname??contuser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=slap")));
            else
                await Send($"{contuser.Nickname??contuser.Username} slapped {guilduser.Nickname??guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=slap")));
        }
        
        [Command("kill", RunMode = RunMode.Async), Summary("Kills a user")]
        public async Task Kill([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            if (contuser == guilduser)
                await Send($"{contuser.Nickname ?? contuser.Username} killed themself", "http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif");
            else
                await Send($"{contuser.Nickname ?? contuser.Username} killed {guilduser.Nickname??guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=kill")));
        }
        
        [Command("stab", RunMode = RunMode.Async), Summary("Stabs a user")]
        public async Task Stab([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (contuser == guilduser)
                await Send($"URUSAI!! {botguild.Nickname??botguild.Username} stabbed {guilduser.Nickname??guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=stab")));
            else if (guilduser.IsBot)
                await Send($"{contuser.Nickname ?? contuser.Username} stabbed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=stab")));
            else
            {
                if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
                {
                    int dhp = Bot.random.Next(0, 100);
                    MySqlCommand command = new MySqlCommand("select HP from accounts where ID = @userid");
                    command.Parameters.AddWithValue("@userid", guilduser.Id);
                    var resp = await SqlTools.GetSingleAsync(command);
                    if (String.IsNullOrEmpty(resp))
                        await InsertUser(guilduser);
                    else
                    {
                        var uhp = Convert.ToUInt32(resp);
                        var hp = uhp - dhp;
                        if (hp > 0)
                        {
                            command = new MySqlCommand("UPDATE accounts SET hp = @hp where ID = @userid");
                            command.Parameters.AddWithValue("@hp", hp);
                            command.Parameters.AddWithValue("@userid", guilduser.Id);
                            await SqlTools.InsertAsync(command);
                            await Send($"{contuser.Nickname ?? contuser.Username} just stabbed {guilduser.Nickname ?? guilduser.Username} for {dhp} HP, they now have {hp} HP left", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=stab")));
                        }
                        else
                        {
                            command = new MySqlCommand("UPDATE accounts SET hp = 0 where ID = @userid");
                            command.Parameters.AddWithValue("@userid", guilduser.Id);
                            await SqlTools.InsertAsync(command);
                            await Send($"{contuser.Nickname ?? contuser.Username} just stabbed {guilduser.Nickname ?? guilduser.Username} for {dhp} HP, they have no HP left", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=stab")));
                        }
                    }
                }
                else
                {
                    await Send($"{contuser.Nickname ?? contuser.Username} just stabbed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=stab")));
                }
            }
        }
        
        [Command("hug", RunMode = RunMode.Async), Summary("hugs a user")]
        public async Task Hug([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
                await Send($"{botguild.Nickname ?? botguild.Username} hugs {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=hug")));
            else
                await Send($"{contuser.Nickname ?? contuser.Username} just hugged {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=hug")));
        }
        
        [Command("punch", RunMode = RunMode.Async), Summary("Punch a user")]
        public async Task Punch([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
                await Send($"URUSAI!! {botguild.Nickname ?? botguild.Username} just punched {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=punch")));
            else
                await Send($"{contuser.Nickname ?? contuser.Username} just punched {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=punch")));
        }

        [Command("shrug", RunMode = RunMode.Async), Summary("Shrugs")]
        public async Task Shrug() => 
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} shrugs.", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=shrug")));
                
        [Command("adore", RunMode = RunMode.Async), Summary("Adore a user")]
        public async Task Adore([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
                await Send($"I-it's not like I like you or anything... {botguild.Nickname ?? botguild.Username} adores {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=adore")));
            else
                await Send($"{contuser.Nickname ?? contuser.Username} adores {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=adore")));

        }

        [Command("kiss", RunMode = RunMode.Async), Summary("Kiss a user")]
        public async Task Kiss([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
                await Send($"I-it's not like I like you or anything... {botguild.Nickname ?? botguild.Username} just kissed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=kiss")));
            else
                await Send($"{contuser.Nickname ?? contuser.Username} just kissed {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=kiss")));
        }

        [Command("grope", RunMode = RunMode.Async), Summary("Grope a user")]
        public async Task Grope([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (guilduser == contuser)
                await Send($"{botguild.Nickname??botguild.Username} just groped {guilduser.Nickname??guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=grope")));
            else
                await Send($"{contuser.Nickname?? contuser.Username} just groped {guilduser.Nickname??guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=grope")));
        }

        [Command("pet", RunMode = RunMode.Async), Summary("Pets a user")]
        public async Task Pet([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (contuser == guilduser)
                await Send($"{botguild.Nickname??botguild.Username} just petted {guilduser.Nickname??guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=pet")));
            else if (guilduser.IsBot)
                await Send($"{contuser.Nickname?? contuser.Username} just petted {guilduser.Nickname??guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=pet")));
            else
            {
                if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
                {
                    MySqlCommand command = new MySqlCommand("SELECT pets from accounts where id = @userid");
                    command.Parameters.AddWithValue("@userid", Context.User.Id);

                    var resp1 = await SqlTools.GetSingleAsync(command);
                    if (String.IsNullOrEmpty(resp1))
                    {
                        await InsertUser(contuser);
                    }
                    else
                    {
                        int pets = Convert.ToInt32(resp1);
                        int newpets = pets + 1;

                        command = new MySqlCommand("UPDATE accounts SET pets = @newpets WHERE id = @userid");
                        command.Parameters.AddWithValue("@newpets", newpets);
                        command.Parameters.AddWithValue("@userid", contuser.Id);

                        await SqlTools.InsertAsync(command);

                        command = new MySqlCommand("SELECT petted from accounts where id = @userid");
                        command.Parameters.AddWithValue("@userid", guilduser.Id);

                        var resp2 = await SqlTools.GetSingleAsync(command);
                        if (resp2 == null)
                        {
                            await InsertUser(guilduser);
                        }
                        else
                        {
                            int petted = Convert.ToInt32(resp2);
                            int newpetted = petted + 1;

                            command = new MySqlCommand("UPDATE accounts SET petted = @newpetted WHERE id = @userid");
                            command.Parameters.AddWithValue("@newpetted", newpetted);
                            command.Parameters.AddWithValue("@userid", guilduser.Id);

                            await SqlTools.InsertAsync(command);
                            await Send($"{contuser.Nickname ?? contuser.Username} just petted {guilduser.Nickname ?? guilduser.Username}, they've been petted {newpetted} time(s)!", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=pet")));
                        }
                    }
                }
                else
                {
                    await Send($"{contuser.Nickname ?? contuser.Username} just petted {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=pet")));
                }
            }                
        }

        [Command("glare", RunMode = RunMode.Async),Summary("Glares at a user")]
        public async Task Glare([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = await Context.Guild.GetUserAsync(Bot.bot.CurrentUser.Id) as IGuildUser;
            if (contuser == guilduser)
                await Send($"{botguild.Nickname??botguild.Username} glares at {guilduser.Nickname??guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=glare")));
            else if(guilduser.IsBot)
                await Send($"{contuser.Nickname?? contuser.Username} glares at {guilduser.Nickname??guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=glare")));
            else
            {
                if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
                {
                    MySqlCommand command = new MySqlCommand("SELECT glares from accounts where id = @userid");
                    command.Parameters.AddWithValue("@userid", contuser.Id);
                    var resp1 = await SqlTools.GetSingleAsync(command);

                    if (String.IsNullOrEmpty(resp1))
                    {
                        await InsertUser(contuser);
                    }
                    else
                    {
                        int glares = Convert.ToInt32(resp1);
                        int newglares = glares + 1;

                        command = new MySqlCommand("UPDATE accounts SET glares = @newglares WHERE id = @userid");
                        command.Parameters.AddWithValue("@newglares", newglares);
                        command.Parameters.AddWithValue("@userid", contuser.Id);
                        await SqlTools.InsertAsync(command);

                        command = new MySqlCommand("SELECT glaredat from accounts where id = @userid");
                        command.Parameters.AddWithValue("@userid", guilduser.Id);
                        var resp2 = await SqlTools.GetSingleAsync(command);

                        if (String.IsNullOrEmpty(resp2))
                        {
                            await InsertUser(guilduser);
                        }
                        else
                        {
                            int glaredat = Convert.ToInt32(resp2);
                            int newglaredat = glaredat + 1;

                            command = new MySqlCommand("UPDATE accounts SET glaredat = @glaredat WHERE id = @userid");
                            command.Parameters.AddWithValue("@glaredat", newglaredat);
                            command.Parameters.AddWithValue("@userid", guilduser.Id);
                            await SqlTools.InsertAsync(command);

                            await Send($"{contuser.Nickname ?? contuser.Username} glares at {guilduser.Nickname ?? guilduser.Username}, they've been glared at {newglaredat} time(s)!", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=glare")));
                        }
                    }
                }
                else
                {
                    await Send($"{contuser.Nickname ?? contuser.Username} glares at {guilduser.Nickname ?? guilduser.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=glare")));
                }

            }
        }
        //End Users

        //Roles
        [Command("slap", RunMode = RunMode.Async), Summary("Slap everyone in a role")]
        public async Task Slap([Remainder]IRole role) =>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} slaps everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=slap")));

        [Command("kill", RunMode = RunMode.Async), Summary("Kill everyone in a role")]
        public async Task Kill([Remainder]IRole role) =>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} kills everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=kill")));

        [Command("stab", RunMode = RunMode.Async), Summary("Stabs everyone in a role")]
        public async Task Stab([Remainder]IRole role) =>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} stabs everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=stab")));

        [Command("hug", RunMode = RunMode.Async), Summary("hugs everyone in a role")]
        public async Task Hug([Remainder]IRole role) =>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} hugs everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=hug")));

        [Command("punch", RunMode = RunMode.Async), Summary("Punch everyone in a role")]
        public async Task Punch([Remainder]IRole role) =>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} punches everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=punch")));

        [Command("adore", RunMode = RunMode.Async), Summary("Adore everyone in a role")]
        public async Task Adore([Remainder]IRole role) =>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} adores everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=adore")));

        [Command("kiss", RunMode = RunMode.Async), Summary("Kiss everyone in a role")]
        public async Task Kiss([Remainder]IRole role) =>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} kisses everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=kiss")));

        [Command("grope", RunMode = RunMode.Async), Summary("Grope everyone in a role")]
        public async Task Grope([Remainder]IRole role) =>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} gropes everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=grope")));

        [Command("pet", RunMode = RunMode.Async), Summary("Pets everyone in a role")]
        public async Task Pet([Remainder]IRole role)=>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} pets everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=pet")));

        [Command("glare", RunMode = RunMode.Async), Summary("Glares at everyone in a role")]
        public async Task Glare([Remainder]IRole role)=>
            await Send($"{(Context.User as IGuildUser).Nickname ?? Context.User.Username} glares at everyone in {role.Name}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=glare")));

        private async Task InsertUser(IUser user)
        {
            MySqlCommand command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username, \"I have no description\");");
            command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\").Replace("\'", "\\'")}#{user.DiscriminatorValue}");
            command.Parameters.AddWithValue("@userid",user.Id);
            await SqlTools.InsertAsync(command);
        }
        private async Task Send(string message, string image) =>
            await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Description = message, Color = RandColor.RandomColor(), ImageUrl = image }.Build());
    }
}