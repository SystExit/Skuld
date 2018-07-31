﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Skuld.Models
{
    public class WebSocketUser
    {
        public string Username { get; internal set; }
        public string UserIconUrl { get; internal set; }
        public string Discriminator { get; internal set; }
        public string FullName { get => Username + "#" + Discriminator; }
        public ulong Id { get; internal set; }
    }
}
