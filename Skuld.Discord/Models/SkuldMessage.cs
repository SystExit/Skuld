using System;
using System.IO;
using DiscNet = Discord;

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
        public Stream File;
        public string FileName;
    }

    public struct SkuldMessage
    {
        public Guid IdempotencyKey;
        public DiscNet.IMessageChannel Channel;
        public SkuldMessageMeta Meta;
        public SkuldMessageContent Content;
    }
}