using Discord.Commands;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class DisabledAttribute : PreconditionAttribute
    {
        private readonly bool DisabledForAdmins = false;
        private readonly bool DisabledForTesters = false;

        public DisabledAttribute()
        {
        }

        /// <summary>
        /// Constructor with BotAdmin & BotTester Flags
        /// </summary>
        /// <param name="forAdmins">Disabled For Bot Admins</param>
        /// <param name="forTesters">Disabled For Bot Testers</param>
        public DisabledAttribute(bool forAdmins, bool forTesters)
        {
            DisabledForAdmins = forAdmins;
            DisabledForTesters = forTesters;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database.GetUserAsync(context.User);

            if ((!DisabledForTesters && usr.Flags.IsBitSet(DiscordUtilities.BotTester)) || usr.Flags.IsBitSet(DiscordUtilities.BotCreator) || (!DisabledForAdmins && usr.Flags.IsBitSet(DiscordUtilities.BotAdmin)))
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("Disabled Command");
        }
    }
}