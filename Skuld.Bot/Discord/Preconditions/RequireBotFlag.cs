using Discord.Commands;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Bot.Discord.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Discord.Preconditions
{
    public class RequireBotFlag : PreconditionAttribute
    {
        private readonly BotAccessLevel Level;

        public RequireBotFlag(BotAccessLevel level)
        {
            Level = level;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var access = await GetPermissionAsync(context, Database.Users.FirstOrDefault(x => x.Id == context.User.Id)).ConfigureAwait(false);
            if (access >= Level)
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("Insufficient permissions.");
        }

        public async Task<BotAccessLevel> GetPermissionAsync(ICommandContext context, User user)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

            if (user.Flags.IsBitSet(DiscordUtilities.BotCreator) || appInfo.Owner.Id == context.User.Id)
                return BotAccessLevel.BotOwner;
            if (user.Flags.IsBitSet(DiscordUtilities.BotAdmin))
                return BotAccessLevel.BotAdmin;
            if (user.IsDonator)
                return BotAccessLevel.BotDonator;
            if (user.Flags.IsBitSet(DiscordUtilities.BotTester))
                return BotAccessLevel.BotTester;

            return BotAccessLevel.Normal;
        }
    }
}