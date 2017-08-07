using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Tools;
using Discord;

namespace Skuld.Commands
{
    [Group, Name("Help")]
    public class Help : ModuleBase
    {
        [Command("help", RunMode = RunMode.Async), Summary("Gets all commands")]
        public async Task _Help(){await MessageHandler.SendChannel(Context.Channel, "All of the commands are available on the wiki: <https://github.com/exsersewo/Skuld/wiki/Commands>"); }
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

                EmbedBuilder embed = new EmbedBuilder();
                embed.Description = $"Here are some commands like **{command}**";
                embed.Color = RandColor.RandomColor();

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
