using Discord;
using Discord.Commands;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Models;
using Skuld.Discord.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
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

            var access = await GetPermissionAsync(context, Database.Users.FirstOrDefault(x=>x.Id == context.User.Id)).ConfigureAwait(false);
            if (access >= Level)
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("Insufficient permissions.");
        }

        public async Task<BotAccessLevel> GetPermissionAsync(ICommandContext context, User user)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

            if (user.Flags.IsBitSet(Utils.BotCreator) || appInfo.Owner.Id == context.User.Id)
                return BotAccessLevel.BotOwner;
            if (user.Flags.IsBitSet(Utils.BotAdmin))
                return BotAccessLevel.BotAdmin;
            if (user.Flags.IsBitSet(Utils.BotDonator))
                return BotAccessLevel.BotDonator;
            if (user.Flags.IsBitSet(Utils.BotTester))
                return BotAccessLevel.BotTester;

            return BotAccessLevel.Normal;
        }
    }
}