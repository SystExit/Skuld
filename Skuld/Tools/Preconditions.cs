using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Skuld.Services;
using Discord.WebSocket;

namespace Skuld.Tools
{
    public class RequireRole : PreconditionAttribute
    {
		private readonly AccessLevel Level;
		readonly DiscordShardedClient client = (DiscordShardedClient)Bot.services.GetService(typeof(DiscordShardedClient));

        public RequireRole(AccessLevel level)
        {
            Level = level;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var access = GetPermission(context);
            if (access >= Level)
            { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else
            { return Task.FromResult(PreconditionResult.FromError("Insufficient permissions.")); }
        }

        public AccessLevel GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)
            { return AccessLevel.Blocked; }
            if (Bot.Configuration.Owners.Contains(c.User.Id) || (client.GetApplicationInfoAsync().Result).Owner.Id == c.User.Id)
            { return AccessLevel.BotOwner; }
            IGuildUser user = (IGuildUser)c.User;
            if (user != null)
            {
                if (c.Guild.OwnerId == user.Id)
                { return AccessLevel.ServerOwner; }
                if (user.GuildPermissions.Administrator)
                { return AccessLevel.ServerAdmin; }
                if (user.GuildPermissions.ManageMessages && user.GuildPermissions.BanMembers && user.GuildPermissions.KickMembers && user.GuildPermissions.ManageRoles)
                { return AccessLevel.ServerMod; }
            }

            return AccessLevel.User;                             // If nothing else, return a default permission.
        }
    }
	
	public class RequireBotAndUserPermission : PreconditionAttribute
	{
		private readonly GuildPermission Permission;

		public RequireBotAndUserPermission(GuildPermission perm)
		{
			Permission = perm;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			var perm = GetPermission(context);
			if (perm)
			{ return Task.FromResult(PreconditionResult.FromSuccess()); }
			else
			{ return Task.FromResult(PreconditionResult.FromError("Insufficient permissions.")); }
		}

		public bool GetPermission(ICommandContext c)
		{
			if (c.User.IsBot)
			{ return false; }

			IGuildUser user = (IGuildUser)c.User;
			IGuildUser botu = c.Guild.GetCurrentUserAsync().Result;

			foreach(var perm in user.GuildPermissions.ToList())
			{
				foreach(var bperm in botu.GuildPermissions.ToList())
				{
					if (perm == Permission && bperm == Permission)
						return true;
				}
			}

			return false;                             // If nothing else, return a default value of false.
		}
	}

	public class RequireDatabase : PreconditionAttribute
	{
		public RequireDatabase() { }

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			var dbserv = (DatabaseService)services.GetService(typeof(DatabaseService));

			if(dbserv!=null)
			{
				if (dbserv.CanConnect)
					return Task.FromResult(PreconditionResult.FromSuccess());
			}

			return Task.FromResult(PreconditionResult.FromError("Command requires an active Database Connection"));
		}
	}
}
