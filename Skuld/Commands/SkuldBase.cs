using System;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.Extensions;
using Skuld.Services;
using Discord;
using System.Threading.Tasks;
using Skuld.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;

namespace Skuld.Commands
{
    public class SkuldBase : InteractiveBase<ShardedCommandContext>
    {

    }
    public class SkuldBase<T> : InteractiveBase<T>
        where T : ShardedCommandContext
    {
        public async Task<IUserMessage> ReplyAsync(ITextChannel channel, string message)
            => await ReplyAsync(channel, message);

        public async Task<IUserMessage> ReplyAsync(ITextChannel channel, string message, Embed embed)
            => await ReplyAsync(channel, message, embed);

        public async Task<IUserMessage> ReplyAsync(ITextChannel channel, Embed embed)
            => await ReplyAsync(channel, embed);

        public async Task<IUserMessage> ReplyFailableAsync(IDMChannel channel, string message)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task<IUserMessage> ReplyFailableAsync(IDMChannel channel, string message, Embed embed)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync(message, false, embed);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task<IUserMessage> ReplyFailableAsync(IDMChannel channel, Embed embed)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync("", false, embed);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task<IUserMessage> ReplyAsync(IDMChannel channel, ISocketMessageChannel backupchannel, string message)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return await ReplyAsync(backupchannel, "I couldn't send the message to your DMs, so I sent it here instead\n\n" + message);
            }
        }

        public async Task<IUserMessage> ReplyAsync(IDMChannel channel, ISocketMessageChannel backupchannel, string message, Embed embed)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync(message, false, embed);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return await ReplyAsync(backupchannel, "I couldn't send the message to your DMs, so I sent it here instead\n\n" + message, embed);
            }
        }

        public async Task<IUserMessage> ReplyAsync(IDMChannel channel, ISocketMessageChannel backupchannel, Embed embed)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync("", false, embed);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return await ReplyAsync(backupchannel, "I couldn't send the message to your DMs, so I sent it here instead", embed);
            }
        }

        public async Task<IUserMessage> ReplyAsync(ISocketMessageChannel channel, string message)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return await mesgChan.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task<IUserMessage> ReplyAsync(ISocketMessageChannel channel, string message, Embed embed)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                if (await textChan.CanEmbedAsync())
                {
                    return await mesgChan.SendMessageAsync(message, false, embed);
                }
                else
                {
                    return await mesgChan.SendMessageAsync(message + "\n\n" + embed.ToMessage());
                }
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task<IUserMessage> ReplyAsync(ISocketMessageChannel channel, Embed embed)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                if (await textChan.CanEmbedAsync())
                {
                    return await mesgChan.SendMessageAsync("", false, embed);
                }
                else
                {
                    return await mesgChan.SendMessageAsync(embed.ToMessage());
                }
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task<IUserMessage> ReplyWithMentionAsync(ISocketMessageChannel channel, IUser user, string message)
            => await ReplyAsync(channel, user.Mention + " " + message);

        public async Task<IUserMessage> ReplyWithMentionAsync(ISocketMessageChannel channel, IUser user, string message, Embed embed)
            => await ReplyAsync(channel, user.Mention + " " + message, embed);

        public async Task<IUserMessage> ReplyWithMentionAsync(ISocketMessageChannel channel, IUser user, Embed embed)
            => await ReplyAsync(channel, user.Mention, embed);

        public async Task<IUserMessage> ReplyWithMentionAsync(ITextChannel channel, IUser user, string message)
            => await ReplyAsync(channel, user.Mention + " " + message);

        public async Task<IUserMessage> ReplyWithMentionAsync(ITextChannel channel, IUser user, string message, Embed embed)
            => await ReplyAsync(channel, user.Mention + " " + message, embed);

        public async Task<IUserMessage> ReplyWithMentionAsync(ITextChannel channel, IUser user, Embed embed)
            => await ReplyAsync(channel, user.Mention, embed);

        public async Task<IUserMessage> ReplyWithFileAsync(ISocketMessageChannel channel, string filepath, string message = null)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return await mesgChan.SendFileAsync(filepath, message);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task<IUserMessage> ReplyWithFileAndMentionAsync(ISocketMessageChannel channel, IUser user, string message = null)
        {
            if (message == null)
            {
                return await ReplyWithFileAsync(channel, user.Mention);
            }
            else
            {
                return await ReplyWithFileAsync(channel, user.Mention + " " + message);
            }
        }

        public async Task ReplyWithTimedMessage(ISocketMessageChannel channel, string message, double timeout)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                var msg = await mesgChan.SendMessageAsync(message);
                await msg.DeleteAfterSecondsAsync((int)timeout);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return;
            }
        }

        public async Task ReplyWithTimedMessage(ITextChannel channel, string message, double timeout)
        {
            var logger = HostService.Services.GetRequiredService<GenericLogger>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                var msg = await mesgChan.SendMessageAsync(message);
                await msg.DeleteAfterSecondsAsync((int)timeout);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return;
            }
        }

        public async Task ReplyFailedAsync(ISocketMessageChannel channel)
            => await channel.SendMessageAsync("Something happened <:blobsick:350673776071147521>");
    }
}
