using Discord;
using Discord.Commands;
using Skuld.Core.Extensions;
using System.Linq;
using System.Text;

namespace Skuld.Core.Generic.Models
{
    public class RoleConfig
    {
        public bool? Hoistable;
        public bool? Mentionable;
        public int? Position;
        public Color? Color;

        public RoleConfig()
        {
            Hoistable = null;
            Mentionable = null;
            Position = null;
            Color = null;
        }

        public static bool FromString(string input, ICommandContext context, out RoleConfig roleConfig)
        {
            roleConfig = new RoleConfig();
            string[] inputsplit = input.Split(' ');

            if (inputsplit.Where(x => x.StartsWith("hoisted=")).Any())
            {
                if (bool.TryParse(inputsplit.FirstOrDefault(x => x.StartsWith("hoisted=")).Replace("hoisted=", ""), out bool result))
                {
                    roleConfig.Hoistable = result;
                }
                else
                {
                    return false;
                }
            }

            if (inputsplit.Where(x => x.StartsWith("mentionable=")).Any())
            {
                if (bool.TryParse(inputsplit.FirstOrDefault(x => x.StartsWith("mentionable=")).Replace("mentionable=", ""), out bool result))
                {
                    roleConfig.Mentionable = result;
                }
                else
                {
                    return false;
                }
            }

            if (inputsplit.Where(x => x.StartsWith("position=")).Any())
            {
                if (int.TryParse(inputsplit.FirstOrDefault(x => x.StartsWith("position=")).Replace("position=", ""), out int result))
                {
                    roleConfig.Position = result;
                }
                else
                {
                    return false;
                }
            }

            if (inputsplit.Where(x => x.StartsWith("color=")).Any())
            {
                var col = inputsplit.FirstOrDefault(x => x.StartsWith("color=")).Replace("color=", "");

                if (!col.StartsWith("#"))
                    col = "#" + col;

                roleConfig.Color = col.FromHex();
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder message = new StringBuilder();

            if (Hoistable.HasValue)
                message.Append($"hoisted={Hoistable} ");
            if (Mentionable.HasValue)
                message.Append($"mentionable={Mentionable} ");
            if (Position.HasValue)
                message.Append($"position={Position.Value} ");
            if (Color != null)
                message.Append($"color={Color?.ToHex()} ");

            return message.ToString()[0..^1];
        }
    }
}