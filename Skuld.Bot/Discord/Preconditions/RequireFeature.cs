using Discord;
using Discord.Commands;
using Skuld.Models;
using Skuld.Bot.Discord.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Discord.Preconditions
{
    public class RequireFeatureAttribute : PreconditionAttribute
    {
        private readonly GuildFeature Feature;

        public RequireFeatureAttribute(GuildFeature feature)
        {
            Feature = feature;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null) return Task.FromResult(PreconditionResult.FromSuccess());

            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var feats = Database.Features.FirstOrDefault(x => x.Id == context.Guild.Id);

            bool isEnabled = false;

            switch(Feature)
            {
                case GuildFeature.Experience:
                    {
                        isEnabled = feats.Experience;
                    }
                    break;
                case GuildFeature.Pinning:
                    {
                        isEnabled = feats.Pinning;
                    }
                    break;
                case GuildFeature.StackingRoles:
                    {
                        isEnabled = feats.StackingRoles;
                    }
                    break;
                case GuildFeature.Starboard:
                    {
                        isEnabled = feats.Starboard;
                    }
                    break;
            }

            if(isEnabled)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError($"The feature: `{Feature}` is disabled, contact a server administrator to enable it"));
            }
        }
    }
}