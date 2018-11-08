using Discord;
using Discord.WebSocket;
using Raven.Client.Documents;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Globalization;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database.Extensions;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Database
{
    public static class RavenDatabaseClient
    {
        DocumentStore ClientStore;
        public static SkuldUser GetUserAsync(ulong id)
        {

        }
    }
}
