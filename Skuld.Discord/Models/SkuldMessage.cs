using DiscNet = Discord;
using System;

namespace Skuld.Discord.Models
{
    public struct SkuldMessageMeta
    {
        public Exception Exception;
        public double Timeout;
        public MessageType Type;
    }
    public struct SkuldMessageContent
    {
        public string Message;
        public DiscNet.IUser User;
        public DiscNet.Embed Embed;
        public string File;
    }
    public struct SkuldMessage
    {
        public DiscNet.IMessageChannel Channel;
        public SkuldMessageMeta Meta;
        public SkuldMessageContent Content;
    }
}
