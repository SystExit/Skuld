﻿using System;
using Discord.Commands;
using System.Threading.Tasks;

namespace Skuld.Utilities
{
	public class UriTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (Uri.TryCreate(input, UriKind.Absolute, out Uri uri))
				return Task.FromResult(TypeReaderResult.FromSuccess(uri));
			else
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid url."));
		}
	}
}
