using Discord.Commands;
using Skuld.Core.Extensions;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Dis = Discord;

namespace Skuld.Bot.Discord.TypeReaders
{
	public class ColorTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (input.StartsWith("#"))
			{
				Dis.Color col = input.FromHex();

				return Task.FromResult(TypeReaderResult.FromSuccess(Color.FromArgb(col.R, col.G, col.B)));
			}

			Color? result = null;

			if (!AngleSharp.Text.CharExtensions.IsDigit(input[0]))
			{
				result = Color.FromName(input);
			}

			if (result is not null)
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(result));
			}

			string[] split;

			if (input.Contains(","))
			{
				split = input.Split(",");
			}
			else
			{
				split = input.Split(" ");
			}

			split[0] = split[0].Replace(" ", "");
			split[1] = split[1].Replace(" ", "");
			split[2] = split[2].Replace(" ", "");

			if (!int.TryParse(split[0], out int r))
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid Color Input"));
			}
			if (!int.TryParse(split[1], out int g))
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid Color Input"));
			}
			if (!int.TryParse(split[2], out int b))
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid Color Input"));
			}

			result = Color.FromArgb(r, g, b);

			return Task.FromResult(TypeReaderResult.FromSuccess(result.Value));
		}
	}
}