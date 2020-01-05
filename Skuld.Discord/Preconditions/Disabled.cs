using Discord.Commands;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class DisabledAttribute : PreconditionAttribute
    {
        bool DisabledForAdmins = false;
        bool DisabledForTesters = false;

        public DisabledAttribute() { }

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

            if ((!DisabledForTesters && usr.Flags.IsBitSet(Utils.BotTester)) || usr.Flags.IsBitSet(Utils.BotCreator) || (!DisabledForAdmins && usr.Flags.IsBitSet(Utils.BotAdmin)))
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("Disabled Command");
        }
    }
}