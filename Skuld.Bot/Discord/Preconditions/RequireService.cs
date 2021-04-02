using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Skuld.Bot.Discord.Preconditions
{
	public class RequireService : PreconditionAttribute
	{
		private readonly Type reqService;

		public RequireService(Type requiredService)
		{
			reqService = requiredService;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (IsServiceLoaded(services))
				return Task.FromResult(PreconditionResult.FromSuccess());
			else
				return Task.FromResult(PreconditionResult.FromError($"Service: {reqService.Name} is not currently loaded."));
		}

		private bool IsServiceLoaded(IServiceProvider services)
		{
			object service = services.GetService(reqService);

			return service is not null;
		}
	}
}