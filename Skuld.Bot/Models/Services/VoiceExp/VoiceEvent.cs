using Discord.WebSocket;

namespace Skuld.Bot.Models.Services.VoiceExp
{
    public class VoiceEvent
    {
        public SocketVoiceChannel VoiceChannel;
        public SocketGuild Guild;
        public SocketUser User;
        public ulong Time;
        public bool IsValid;

        public VoiceEvent(SocketVoiceChannel voiceChannel, SocketGuild guild, SocketUser user, ulong time, bool isValid)
        {
            VoiceChannel = voiceChannel;
            Guild = guild;
            User = user;
            Time = time;
            IsValid = isValid;
        }
    }
}