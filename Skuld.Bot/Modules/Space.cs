using Discord;
using Discord.Commands;
using Skuld.APIS;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Verification;
using Skuld.Services.Discord.Attributes;
using Skuld.Services.Discord.Preconditions;
using Skuld.Services.Messaging.Extensions;
using StatsdClient;
using System;
using System.Threading.Tasks;

namespace Skuld.Bot.Modules
{
    [Group, Name("Space"), RequireEnabledModule]
    public class SpaceModule : ModuleBase<ShardedCommandContext>
    {
        public NASAClient NASAClient { get; set; }
        public ISSClient ISSClient { get; set; }

        [Command("apod"), Summary("Gets NASA's \"Astronomy Picture of the Day\""), Ratelimit(20, 1, Measure.Minutes)]
        public async Task APOD()
        {
            var aPOD = await NASAClient.GetAPODAsync().ConfigureAwait(false);
            DogStatsd.Increment("web.get");

            if (aPOD.HDUrl != null && (!aPOD.HDUrl.IsVideoFile() || (!aPOD.Url.Contains("youtube") || !aPOD.Url.Contains("youtu.be"))))
            {
                await new EmbedBuilder()
                    .WithColor(EmbedExtensions.RandomEmbedColor())
                    .WithTitle(aPOD.Title)
                    .WithUrl("https://apod.nasa.gov/")
                    .WithImageUrl(aPOD.HDUrl)
                    .WithTimestamp(Convert.ToDateTime(aPOD.Date))
                    .WithAuthor(aPOD.CopyRight)
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await aPOD.Url.QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("astronauts"), Summary("Gets a list of astronauts currently in space"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Astros()
        {
            var astros = await ISSClient.GetAstronautsInSpace().ConfigureAwait(false);

            string msg = "Current list of astronauts in space:\n";

            foreach (var astro in astros.Astronauts)
            {
                msg += $"{astro.Name} - {astro.Craft}\n";
            }

            await msg.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("iss"), Summary("Gets the current position of the International Space Station"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task ISS()
        {
            var iss = await ISSClient.GetISSPositionAsync().ConfigureAwait(false);

            await $"The ISS is currently at:\nLAT: {iss.IISPosition.Latitude} LONG: {iss.IISPosition.Longitude}".QueueMessageAsync(Context).ConfigureAwait(false);
        }
    }
}