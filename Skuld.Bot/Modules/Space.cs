using Discord;
using Discord.Commands;
using Skuld.APIS;
using Skuld.APIS.NASA.Models;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord;
using Skuld.Discord.Attributes;
using StatsdClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Modules
{
    public class Space : SkuldBase<SkuldCommandContext>
    {
        public NASAClient NASAClient { get; set; }
        public ISSClient ISSClient { get; set; }

        [Command("apod"), Summary("Gets NASA's \"Astronomy Picture of the Day\""), Ratelimit(20, 1, Measure.Minutes)]
        public async Task APOD()
        {
            var aPOD = await NASAClient.GetAPODAsync();
            DogStatsd.Increment("web.get");

            if (!aPOD.HDUrl.IsVideoFile() || (!aPOD.HDUrl.Contains("youtube") || aPOD.HDUrl.Contains("youtu.be")))
            {
                var embed = new EmbedBuilder
                {
                    Color = EmbedUtils.RandomColor(),
                    Title = aPOD.Title,
                    Url = "https://apod.nasa.gov/",
                    ImageUrl = aPOD.HDUrl,
                    Timestamp = Convert.ToDateTime(aPOD.Date),
                    Author = new EmbedAuthorBuilder
                    {
                        Name = aPOD.CopyRight
                    }
                };
                await ReplyAsync(Context.Channel, embed.Build());
            }
            else
            {
                await ReplyAsync(Context.Channel, aPOD.HDUrl);
            }
        }

        [Command("curiosity"), Summary("Gets stuff from NASA's Curiosity Rover"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Curiosity(int SOL = 2199, string camera = "")
        {
            Enum.TryParse(camera, out NasaRoverCamera cam);

            var image = await GetRoverAsync(NasaRover.Curiosity, cam, SOL);

            if (image.Successful)
            {
                var imgdata = image.Data as RoverPhotoWrapper;

                if (imgdata.Photos.Count() == 0)
                { await ReplyFailedAsync(Context.Channel, $"No images found for camera: **{camera}** at SOL: **{SOL}**"); return; }

                var photo = imgdata.Photos.FirstOrDefault();

                var date = DateTime.ParseExact(photo.EarthDate, "yyyy-MM-dd", null);

                var embed = new EmbedBuilder
                {
                    ImageUrl = photo.ImageUrl,
                    Color = EmbedUtils.RandomColor(),
                    Title = "Camera: " + photo.Camera.FullName,
                    Timestamp = date,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Rover: Curiosity"
                    }
                }.Build();

                await ReplyAsync(Context.Channel, embed);
            }
            else { await ReplyFailedAsync(Context.Channel, image.Error); }
        }

        [Command("opportunity"), Summary("Gets stuff from NASA's Opportunity Rover"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Opportunity(int SOL = 2199, string camera = "")
        {
            Enum.TryParse(camera.ToUpperInvariant(), out NasaRoverCamera cam);

            var image = await GetRoverAsync(NasaRover.Opportunity, cam, SOL);

            if (image.Successful)
            {
                var imgdata = image.Data as RoverPhotoWrapper;

                if (imgdata.Photos.Count() == 0)
                { await ReplyFailedAsync(Context.Channel, $"No images found for camera: **{camera}** at SOL: **{SOL}**"); return; }

                var photo = imgdata.Photos.FirstOrDefault();

                var date = DateTime.ParseExact(photo.EarthDate, "yyyy-MM-dd", null);

                var embed = new EmbedBuilder
                {
                    ImageUrl = photo.ImageUrl,
                    Color = EmbedUtils.RandomColor(),
                    Title = "Camera: " + photo.Camera.FullName,
                    Timestamp = date,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Rover: Opportunity"
                    }
                }.Build();

                await ReplyAsync(Context.Channel, embed);
            }
            else { await ReplyFailedAsync(Context.Channel, image.Error); }
        }

        [Command("spirit"), Summary("Gets stuff from NASA's Spirit Rover"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Spirit(int SOL = 2199, string camera = "")
        {
            Enum.TryParse(camera, out NasaRoverCamera cam);

            var image = await GetRoverAsync(NasaRover.Spirit, cam, SOL);

            if (image.Successful)
            {
                var imgdata = image.Data as RoverPhotoWrapper;

                if (imgdata.Photos.Count() == 0)
                { await ReplyFailedAsync(Context.Channel, $"No images found for camera: **{camera}** at SOL: **{SOL}**"); return; }

                var photo = imgdata.Photos.FirstOrDefault();

                var date = DateTime.ParseExact(photo.EarthDate, "yyyy-MM-dd", null);

                var embed = new EmbedBuilder
                {
                    ImageUrl = photo.ImageUrl,
                    Color = EmbedUtils.RandomColor(),
                    Title = "Camera: " + photo.Camera.FullName,
                    Timestamp = date,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Rover: Spirit"
                    }
                }.Build();

                await ReplyAsync(Context.Channel, embed);
            }
            else { await ReplyFailedAsync(Context.Channel, image.Error); }
        }

        [Command("astronauts"), Summary("Gets a list of astronauts currently in space"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Astros()
        {
            var astros = await ISSClient.GetAstronautsInSpace();

            string msg = "Current list of astronauts in space:\n";

            foreach(var astro in astros.Astronauts)
            {
                msg += $"{astro.Name} - {astro.Craft}\n";
            }

            await ReplyAsync(Context.Channel, msg);
        }

        [Command("iss"), Summary("Gets the current position of the International Space Station"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task ISS()
        {
            var iss = await ISSClient.GetISSPositionAsync();

            await ReplyAsync(Context.Channel, $"The ISS is currently at:\nLAT: {iss.IISPosition.Latitude} LONG: {iss.IISPosition.Longitude}");
        }

        private EventResult IncorrectCamera = EventResult.FromFailure("Incorrect camera for rover");

        private async Task<EventResult> GetRoverAsync(NasaRover rover, NasaRoverCamera camera, int SOL = 2199)
        {
            if(rover == NasaRover.Opportunity || rover == NasaRover.Spirit)
            {
                switch(camera)
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
            if(rover == NasaRover.Curiosity)
            {
                switch(camera)
                {
                    case NasaRoverCamera.PANCAM:
                        return IncorrectCamera;
                    case NasaRoverCamera.ENTRY:
                        return IncorrectCamera;
                    case NasaRoverCamera.MINITES:
                        return IncorrectCamera;
                }
            }

            var resp = await NASAClient.GetRoverPhotoAsync(rover, camera, SOL);

            if (resp == null)
                return EventResult.FromFailure("Error parsing JSON");

            return EventResult.FromSuccess(resp);
        }
    }
}
