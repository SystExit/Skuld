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
        public async Task Slap(IGuildUser usertoslap)
        {
            if (usertoslap == Context.User)
            {
                await Send($"B-Baka.... {Bot.bot.CurrentUser.Username} slapped {usertoslap.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=slap")));
            }
            else
            {
                await Send($"{Context.User.Username} slapped {usertoslap.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=slap")));
            }
        }

        [Command("kill", RunMode = RunMode.Async), Summary("Kill a user with an item")]
        public async Task Kill(IGuildUser usertokill)
        {
            var curruser = Context.User as IGuildUser;
            if (Context.User.Id == usertokill.Id)
                await Send($"{curruser.Username} killed themself", "http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif");
            else
                await Send($"{curruser.Username} killed {usertokill.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=kill")));
        }

        [Command("stab", RunMode = RunMode.Async), Summary("Stabs a user")]
        public async Task Stab(IGuildUser usertostab)
        {
            if (Context.User == usertostab) { await Send($"{Bot.bot.CurrentUser.Username} stabbed {usertostab.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=stab"))); }
            else
            {
                int dhp = Bot.random.Next(0, 100);

                MySqlCommand command = new MySqlCommand();
                command.CommandText = "select HP from accounts where ID = @userid";
                command.Parameters.AddWithValue("@userid", usertostab.Id);
                var resp = await Sql.GetSingleAsync(command);

                var curruser = Context.User as IGuildUser;
                if (String.IsNullOrEmpty(resp))
                {
                    await InsertUser(usertostab);
                }
                else
                {
                    var uhp = Convert.ToUInt32(resp);
                    var hp = uhp - dhp;
                    if (hp > 0)
                    {
                        command = new MySqlCommand();
                        command.CommandText = "UPDATE accounts SET hp = @hp where ID = @userid";
                        command.Parameters.AddWithValue("@hp", hp);
                        command.Parameters.AddWithValue("@userid", usertostab.Id);
                        await Sql.InsertAsync(command);

                        await Send($"{curruser.Username} just stabbed {usertostab.Username} for {dhp} HP, they now have {hp} HP left", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=stab")));
                    }
                    else
                    {
                        command = new MySqlCommand();
                        command.CommandText = "UPDATE accounts SET hp = 0 where ID = @userid";
                        command.Parameters.AddWithValue("@userid", usertostab.Id);
                        await Sql.InsertAsync(command);

                        await Send($"{curruser.Username} just stabbed {usertostab.Username} for {dhp} HP, they have no HP left", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=stab")));
                    }
                }
            }
        }

        [Command("hug", RunMode = RunMode.Async), Summary("hugs a user")]
        public async Task Hug(IGuildUser usertohug)
        {
            if (usertohug == Context.User)
            {
                await Send($"{Bot.bot.CurrentUser.Username} hugs {usertohug.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=hug")));
            }
            else
            {
                var curruser = Context.User as IGuildUser;
                await Send($"{curruser.Username} just hugged {usertohug.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=hug")));
            }
        }

        [Command("punch", RunMode = RunMode.Async), Summary("Punch a user")]
        public async Task Punch(IGuildUser usertopunch)
        {
            if(usertopunch == Context.User)
            {
                await Send($"B-Baka... {Bot.bot.CurrentUser.Username} glares at {usertopunch.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=punch")));
            }
            else
            {
                var curruser = Context.User as IGuildUser;
                await Send($"{curruser.Username} just punched {usertopunch.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=punch")));
            }
        }
        [Command("shrug", RunMode = RunMode.Async), Summary("Shrugs")]
        public async Task Punch()
        {
            await Send($"{Context.User.Username} shrugs.", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=shrug")));
        }
        [Command("adore", RunMode = RunMode.Async), Summary("Adore a user")]
        public async Task Adore(IGuildUser usertoadore)
        {
            if (usertoadore == Context.User)
            {
                await Send($"I-it's not like I like you or anything... {Bot.bot.CurrentUser.Username} glares at {usertoadore.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=adore")));
            }
            else
            {
                var curruser = Context.User as IGuildUser;
                await Send($"{curruser.Username} adores {usertoadore.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=adore")));
            }
        }
        [Command("kiss", RunMode = RunMode.Async), Summary("Kiss a user")]
        public async Task Kiss(IGuildUser usertokiss)
        {
            if (usertokiss == Context.User)
            {
                await Send($"I-it's not like I like you or anything... {Bot.bot.CurrentUser.Username} glares at {usertokiss.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=kiss")));
            }
            else
            {
                var curruser = Context.User as IGuildUser;
                await Send($"{curruser.Username} just kissed {usertokiss.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=kiss")));
            }
        }
        [Command("grope", RunMode = RunMode.Async), Summary("Grope a user")]
        public async Task Grope(IGuildUser usertogrope)
        {
            var curruser = Context.User as IGuildUser;
            await Send($"{curruser.Username} just groped {usertogrope.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=grope")));
        }

        [Command("pet", RunMode = RunMode.Async), Summary("Pets a user")]
        public async Task Pet(IGuildUser usertopet)
        {
            if (Context.User == usertopet)
            {
                await Send($"{Bot.bot.CurrentUser.Username} glares at {usertopet.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=pet")));
            }
            else
            {
                var curruser = Context.User as IGuildUser;

                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT pets from accounts where id = @userid";
                command.Parameters.AddWithValue("@userid", Context.User.Id);

                var resp1 = await Sql.GetSingleAsync(command);
                if(String.IsNullOrEmpty(resp1)) {
                    await InsertUser(Context.User);
                }
                else {
                    int pets = Convert.ToInt32(resp1);
                    int newpets = pets+1;

                    command = new MySqlCommand();
                    command.CommandText = "UPDATE accounts SET pets = @newpets WHERE id = @userid";
                    command.Parameters.AddWithValue("@newpets", newpets);
                    command.Parameters.AddWithValue("@userid", Context.User.Id);

                    await Sql.InsertAsync(command);

                    command = new MySqlCommand();
                    command.CommandText = "SELECT petted from accounts where id = @userid";
                    command.Parameters.AddWithValue("@userid", Context.User.Id);

                    var resp2 = await Sql.GetSingleAsync(command);
                    if (resp2==null) {
                        await InsertUser(usertopet);
                    }
                    else {
                        int petted = Convert.ToInt32(resp2);
                        int newpetted = petted+1;

                        command = new MySqlCommand();
                        command.CommandText = "UPDATE accounts SET petted = @newpetted WHERE id = @userid";
                        command.Parameters.AddWithValue("@newpetted", newpetted);
                        command.Parameters.AddWithValue("@userid", Context.User.Id);

                        await Sql.InsertAsync(command);
                        await Send($"{curruser.Username} just petted {usertopet.Username}, they've been petted {newpetted} time(s)!", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=pet")));
                    }
                }
            }                
        }

        [Command("glare", RunMode = RunMode.Async),Summary("Glares at a user")]
        public async Task Glare(IGuildUser usertoglareat)
        {
            if (Context.User == usertoglareat)
            {
                await Send($"{Bot.bot.CurrentUser.Username} glares at {usertoglareat.Username}", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=glare")));
            }
            else
            {
                var curruser = Context.User as IGuildUser;

                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT glares from accounts where id = @userid";
                command.Parameters.AddWithValue("@userid", Context.User.Id);
                var resp1 = await Sql.GetSingleAsync(command);

                if (String.IsNullOrEmpty(resp1)) {
                    await InsertUser(usertoglareat);
                }
                else {
                    int glares = Convert.ToInt32(resp1);
                    int newglares = glares+1;
                    
                    command = new MySqlCommand();
                    command.CommandText = "UPDATE accounts SET glares = @newglares WHERE id = @userid";
                    command.Parameters.AddWithValue("@newglares", newglares);
                    command.Parameters.AddWithValue("@userid", Context.User.Id);
                    await Sql.InsertAsync(command);

                    command = new MySqlCommand();
                    command.CommandText = "SELECT glaredat from accounts where id = @userid";
                    command.Parameters.AddWithValue("@userid", Context.User.Id);
                    var resp2 = await Sql.GetSingleAsync(command);

                    if (String.IsNullOrEmpty(resp2)) {
                        await InsertUser(usertoglareat);
                    }
                    else
                    {
                        int glaredat = Convert.ToInt32(resp2);
                        int newglaredat = glaredat+1;

                        command = new MySqlCommand();
                        command.CommandText = "UPDATE accounts SET glaredat = @glaredat WHERE id = @userid";
                        command.Parameters.AddWithValue("@glaredat", newglaredat);
                        command.Parameters.AddWithValue("@userid", Context.User.Id);
                        await Sql.InsertAsync(command);
                        
                        await Send($"{curruser.Username} glares at {usertoglareat.Username}, they've been glared at {newglaredat} time(s)!", await APIWebReq.ReturnString(new Uri("https://lucoa.systemexit.co.uk/gifs/actions/?f=glare")));
                    }
                }
            }
        }

        private async Task InsertUser(IUser user)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username , \"I have no description\");";
            command.Parameters.AddWithValue("@userid",user.Id);
            command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\\"").Replace("\'", "\\'")}#{user.DiscriminatorValue}");
            await Sql.InsertAsync(command);
        }
        private async Task Send(string message, string image)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.Color = RandColor.RandomColor();
            embed.ImageUrl = image;
            await MessageHandler.SendChannel(Context.Channel, message, embed);
        }
    }
}