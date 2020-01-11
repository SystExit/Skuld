using Discord;
using Discord.Commands;
using Skuld.APIS;
using Skuld.APIS.NASA.Models;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Discord.Attributes;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using StatsdClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Modules
{
    [Group, RequireEnabledModule]
    public class Space : ModuleBase<ShardedCommandContext>
    {
        public NASAClient NASAClient { get; set; }
        public ISSClient ISSClient { get; set; }

        [Command("apod"), Summary("Gets NASA's \"Astronomy Picture of the Day\""), Ratelimit(20, 1, Measure.Minutes)]
        public async Task APOD()
        {
            var aPOD = await NASAClient.GetAPODAsync();
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

        [Command("curiosity"), Summary("Gets stuff from NASA's Curiosity Rover"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Curiosity(int SOL = 2199, string camera = null)
        {
            NasaRoverCamera cam;
            if (camera != null)
                Enum.TryParse(camera.ToUpperInvariant(), out cam);
            else
            {
                cam = NasaRoverCamera.FHAZ;
                camera = "FHAZ";
            }

            var image = await GetRoverAsync(NasaRover.Curiosity, cam, SOL);

            if (image.Successful)
            {
                if (!(image.Data is RoverPhotoWrapper imgdata) || imgdata.Photos.Count() == 0)
                {
                    await EmbedExtensions.FromError($"No images found for camera: **{camera.ToUpper()}** at SOL: **{SOL}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }

                var photo = imgdata.Photos.FirstOrDefault();

                var date = DateTime.ParseExact(photo.EarthDate, "yyyy-MM-dd", null);

                await new EmbedBuilder()
                    .WithImageUrl(photo.ImageUrl)
                    .WithColor(EmbedExtensions.RandomEmbedColor())
                    .WithTitle($"Camera: {photo.Camera.FullName}")
                    .WithTimestamp(date)
                    .WithFooter("Rover: Curiosity")
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
                await EmbedExtensions.FromError(image.Error, Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("opportunity"), Summary("Gets stuff from NASA's Opportunity Rover"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Opportunity(int SOL = 5111, string camera = null)
        {
            NasaRoverCamera cam;
            if (camera != null)
                Enum.TryParse(camera.ToUpperInvariant(), out cam);
            else
            {
                cam = NasaRoverCamera.PANCAM;
                camera = "pancam";
            }

            var image = await GetRoverAsync(NasaRover.Opportunity, cam, SOL);

            if (image.Successful)
            {
                if (!(image.Data is RoverPhotoWrapper imgdata) || imgdata.Photos.Count() == 0)
                {
                    await EmbedExtensions.FromError($"No images found for camera: **{camera.ToUpper()}** at SOL: **{SOL}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }

                var photo = imgdata.Photos.FirstOrDefault();

                var date = DateTime.ParseExact(photo.EarthDate, "yyyy-MM-dd", null);

                await new EmbedBuilder()
                    .WithImageUrl(photo.ImageUrl)
                    .WithColor(EmbedExtensions.RandomEmbedColor())
                    .WithTitle($"Camera: {photo.Camera.FullName}")
                    .WithTimestamp(date)
                    .WithFooter("Rover: Opportunity")
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
                await EmbedExtensions.FromError(image.Error, Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("spirit"), Summary("Gets stuff from NASA's Spirit Rover"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Spirit(int SOL = 2199, string camera = null)
        {
            NasaRoverCamera cam;
            if (camera != null)
                Enum.TryParse(camera.ToUpperInvariant(), out cam);
            else
            {
                cam = NasaRoverCamera.PANCAM;
                camera = "pancam";
            }

            var image = await GetRoverAsync(NasaRover.Spirit, cam, SOL).ConfigureAwait(false);

            if (image.Successful)
            {
                if (!(image.Data is RoverPhotoWrapper imgdata) || !imgdata.Photos.Any())
                {
                    await EmbedExtensions.FromError($"No images found for camera: **{camera.ToUpper()}** at SOL: **{SOL}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }

                var photo = imgdata.Photos.FirstOrDefault();

                var date = DateTime.ParseExact(photo.EarthDate, "yyyy-MM-dd", null);

                await new EmbedBuilder()
                    .WithImageUrl(photo.ImageUrl)
                    .WithRandomColor()
                    .WithTitle($"Camera: {photo.Camera.FullName}")
                    .WithTimestamp(date)
                    .WithFooter("Rover: Spirit")
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
                await EmbedExtensions.FromError(image.Error, Context).QueueMessageAsync(Context).ConfigureAwait(false);
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

        private EventResult IncorrectCamera = EventResult.FromFailure("Incorrect camera for rover");

        private async Task<EventResult> GetRoverAsync(NasaRover rover, NasaRoverCamera camera, int SOL = 2199)
        {
            if (rover == NasaRover.Opportunity || rover == NasaRover.Spirit)
            {
                switch (camera)
                {
                    case NasaRoverCamera.MAST:
                        return IncorrectCamera;

                    case NasaRoverCamera.CHEMCAM:
                        return IncorrectCamera;

                    case NasaRoverCamera.MAHLI:
                        return IncorrectCamera;

                    case NasaRoverCamera.MARDI:
                        return IncorrectCamera;
                }
            }
            if (rover == NasaRover.Curiosity)
            {
                switch (camera)
                {
                    case NasaRoverCamera.PANCAM:
                        return IncorrectCamera;

                    case NasaRoverCamera.ENTRY:
                        return IncorrectCamera;

                    case NasaRoverCamera.MINITES:
                        return IncorrectCamera;
                }
            }

            try
            {
                var resp = await NASAClient.GetRoverPhotoAsync(rover, camera, SOL).ConfigureAwait(false);

                if (resp == null)
                    return EventResult.FromFailure("Error parsing JSON");

                return EventResult.FromSuccess(resp);
            }
            catch (Exception ex)
            {
                return EventResult.FromFailureException(ex.Message, ex);
            }
        }
    }
}