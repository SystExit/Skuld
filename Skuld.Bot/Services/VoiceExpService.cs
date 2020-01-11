using Discord.WebSocket;
using Skuld.Bot.Models.Services.VoiceExp;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Services
{
    public class VoiceExpService
    {
        private static DiscordShardedClient DiscordClient;
        static SkuldConfig Configuration => HostSerivce.Configuration;

        private static ConcurrentBag<VoiceEvent> Targets;

        public VoiceExpService(DiscordShardedClient client)
        {
            DiscordClient = client ?? throw new ArgumentNullException($"{typeof(DiscordShardedClient).Name} cannot be null");

            Targets = new ConcurrentBag<VoiceEvent>();

            DiscordClient.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
        }

        private static SocketVoiceChannel GetVoiceChannel(SocketVoiceState previousState, SocketVoiceState currentState)
        {
            if (previousState.VoiceChannel != null && currentState.VoiceChannel != null)
            {
                if (previousState.VoiceChannel.Guild == currentState.VoiceChannel.Guild)
                    return currentState.VoiceChannel;
            }

            if (previousState.VoiceChannel == null && currentState.VoiceChannel != null)
                return currentState.VoiceChannel;

            return previousState.VoiceChannel;
        }

        private static async Task DoLeaveXpGrantAsync(SocketGuildUser user, SocketVoiceChannel channel)
        {
            var userEvents = Targets.Where(x => x.User == user && x.VoiceChannel.Id == channel.Id).ToList();

            { // Remove All Events that exist for this user and channel
                ConcurrentBag<VoiceEvent> events = new ConcurrentBag<VoiceEvent>();

                foreach (var Target in Targets)
                {
                    if (Target.User != user)
                    {
                        events.Add(Target);
                    }
                }

                Targets.Clear();
                foreach (var e in events)
                {
                    Targets.Add(e);
                }
            }

            var connect = userEvents.FirstOrDefault(x => x.IsValid);
            var disconn = DateTime.UtcNow.ToEpoch();

            var timeDiff = disconn - connect.Time;

            var disallowedPoints = userEvents.Where(x => !x.IsValid).ToList();

            ulong totalTime = 0;

            if (disallowedPoints.Any() && disallowedPoints.Count >= 2)
            {
                var disallowedTime = disallowedPoints.LastOrDefault().Time - disallowedPoints.FirstOrDefault().Time;

                totalTime = timeDiff - disallowedTime;
            }
            else
            {
                totalTime = timeDiff;
            }

            var xpToGrant = Utils.GetExpMultiFromMinutesInVoice(Configuration.VoiceExpDeterminate, Configuration.VoiceExpMinMinutes, 100000, totalTime);

            {
                using var Database = new SkuldDbContextFactory().CreateDbContext();

                var skUser = await Database.GetUserAsync(user).ConfigureAwait(false);
                await skUser.GrantExperienceAsync((ulong)xpToGrant, channel.Guild).ConfigureAwait(false);
            }
        }

        private static async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState previousState, SocketVoiceState currentState)
        {
            var difference = new VoiceStateDifference(previousState, currentState);

            var channel = GetVoiceChannel(previousState, currentState);
            var guild = channel.Guild;
            GuildFeatures feats = null;

            {
                using var Database = new SkuldDbContextFactory().CreateDbContext();
                feats = Database.Features.FirstOrDefault(x => x.Id == guild.Id);
            }

            if (feats != null && feats.Experience)
            {
                if (previousState.VoiceChannel == null && currentState.VoiceChannel != null) // Connect
                {
                    Targets.Add(new VoiceEvent(channel, guild, user, DateTime.UtcNow.ToEpoch(), true));
                    return;
                }
                if (previousState.VoiceChannel != null && currentState.VoiceChannel == null) // Disconnect
                {
                    if (Targets.Any(x => x.User.Id == user.Id))
                    {
                        await DoLeaveXpGrantAsync(channel.Guild.GetUser(user.Id), channel).ConfigureAwait(false);
                    }
                    return;
                }

                if (!difference.DidMute)
                {
                    Targets.Add(new VoiceEvent(channel, guild, user, DateTime.UtcNow.ToEpoch(), true));
                }
                else
                {
                    Targets.Add(new VoiceEvent(channel, guild, user, DateTime.UtcNow.ToEpoch(), false));
                }

                if (!difference.DidDeafen)
                {
                    Targets.Add(new VoiceEvent(channel, guild, user, DateTime.UtcNow.ToEpoch(), true));
                }
                else
                {
                    Targets.Add(new VoiceEvent(channel, guild, user, DateTime.UtcNow.ToEpoch(), false));
                }
            }
        }

        private class VoiceStateDifference
        {
            public bool DidSelfMute { get; private set; }
            public bool DidSelfDeafen { get; private set; }
            public bool DidServerMute { get; private set; }
            public bool DidServerDeafen { get; private set; }
            public bool DidDeafen { get => DidSelfDeafen || DidServerDeafen; }
            public bool DidMute { get => DidSelfMute || DidServerMute; }
            public bool DidMoveToAFKChannel { get; private set; }
            public bool DidDisconnect { get; private set; }
            public bool DidConnect { get; private set; }
            public IList<SocketGuildUser> UserDifference { get; private set; } = new List<SocketGuildUser>();

            public VoiceStateDifference(SocketVoiceState previousState, SocketVoiceState newState)
            {
                SetStateDifference(previousState, newState);
            }

            public VoiceStateDifference SetStateDifference(SocketVoiceState previousState, SocketVoiceState newState)
            {
                DidSelfMute = !previousState.IsSelfMuted && newState.IsSelfMuted;
                DidSelfDeafen = !previousState.IsSelfDeafened && newState.IsSelfDeafened;
                DidServerMute = !previousState.IsMuted && newState.IsMuted;
                DidServerDeafen = !previousState.IsDeafened && newState.IsDeafened;
                DidDisconnect = newState.VoiceChannel == null;
                DidConnect = previousState.VoiceChannel == null && newState.VoiceChannel != null;

                if (newState.VoiceChannel != null)
                    DidMoveToAFKChannel = newState.VoiceChannel.Guild.AFKChannel != null ? newState.VoiceChannel == newState.VoiceChannel.Guild.AFKChannel : false;
                else
                    DidMoveToAFKChannel = false;

                if (newState.VoiceChannel == null && previousState.VoiceChannel != null)
                    UserDifference = previousState.VoiceChannel.Users.ToList();
                else if (newState.VoiceChannel != null && previousState.VoiceChannel != null)
                    UserDifference = newState.VoiceChannel.Users.Where(x => !previousState.VoiceChannel.Users.Contains(x)).ToList();
                else if (newState.VoiceChannel != null && previousState.VoiceChannel == null)
                    UserDifference = newState.VoiceChannel.Users.ToList();

                return this;
            }
        }
    }
}