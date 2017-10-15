using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Tools;
using Discord;
using MySql.Data.MySqlClient;

namespace Skuld.Commands
{
    [Group, Name("Help")]
    public class Help : ModuleBase
    {
        [Command("help", RunMode = RunMode.Async), Summary("Gets all commands")]
        public async Task _Help()
        {
            var cmd = new MySqlCommand("select prefix from guild where id = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            string resp = await SqlTools.GetSingleAsync(cmd);
            string prefix = Config.Load().Prefix;
            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = "Commands of: " + Context.Client.CurrentUser.Username + "#" + Context.Client.CurrentUser.DiscriminatorValue,
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
                },
                Color = Tools.Tools.RandomColor()
            };
            foreach (var module in Bot.commands.Modules)
            {

                if (module.Name == "Help") { }
                else
                {
                    string desc = "";
                    foreach (var command in module.Commands)
                    {
                        var result = await command.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                            desc += $"{command.Aliases.First()}, ";
                        else { continue; }
                    }
                    string description = "";
                    foreach (var str in desc.Split(' ').Distinct())
                        description += str + " ";
                    if (!string.IsNullOrWhiteSpace(description))
                        embed.AddField(module.Name, $"`{description.Remove(description.Length - 3)}`");
                }
            }
            embed.Description = $"The prefix of **{Context.Guild.Name}** is: `{resp ?? prefix}`";
            await MessageHandler.SendDMs(Context.Channel, (await Context.User.GetOrCreateDMChannelAsync()), "", embed);
        }
        [Command("help", RunMode = RunMode.Async), Summary("Gets specific command information")]
        public async Task _Help(string command)
        {
            if (command != "pasta")
            {
                var result = Bot.commands.Search(Context, command);

                if (!result.IsSuccess)
                {
                    await MessageHandler.SendChannel(Context.Channel,$"Sorry, I couldn't find a command like **{command}**.");
                    return;
                }

                var embed = new EmbedBuilder()
                {
                    Description = $"Here are some commands like **{command}**",
                    Color = Tools.Tools.RandomColor()
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;

                    embed.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Summary: {cmd.Summary}\n" +
                                  $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}";
                        x.IsInline = false;
                    });
                }
                await MessageHandler.SendChannel(Context.Channel, "", embed);
            }
            else { }
        }
    }
}
