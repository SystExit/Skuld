using Discord;
using Microsoft.EntityFrameworkCore;
using Skuld.Core.Generic.Models;
using Skuld.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Core.Models
{
    public class SkuldDatabaseContext : DbContext
    {
        public SkuldDatabaseContext(DbContextOptions<SkuldDatabaseContext> options) : base(options)
        {
        }

        public DbSet<BlockedAction> BlockedActions { get; set; }
        public DbSet<SkuldConfig> Configurations { get; set; }
        public DbSet<CustomCommand> CustomCommands { get; set; }
        public DbSet<GuildFeatures> Features { get; set; }
        public DbSet<IAmRole> IAmRoles { get; set; }
        public DbSet<LevelRewards> LevelRewards { get; set; }
        public DbSet<GuildModules> Modules { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Pasta> Pastas { get; set; }
        public DbSet<PastaVotes> PastaVotes { get; set; }
        public DbSet<Reputation> Reputations { get; set; }
        public DbSet<UserCommandUsage> UserCommandUsage { get; set; }
        public DbSet<UserExperience> UserXp { get; set; }
        public DbSet<User> Users { get; set; }

        public bool IsConnected
        {
            get
            {
                return Database.CanConnect();
            }
        }

        public async Task DropGuildAsync(ulong guildId)
        {
            Guilds.Remove(await Guilds.FirstAsync(x => x.Id == guildId).ConfigureAwait(false));

            Modules.Remove(await Modules.FirstAsync(x => x.Id == guildId).ConfigureAwait(false));
            IAmRoles.RemoveRange(IAmRoles.Where(x => x.GuildId == guildId));
            Features.Remove(await Features.FirstAsync(x => x.Id == guildId).ConfigureAwait(false));
            CustomCommands.RemoveRange(CustomCommands.Where(x => x.GuildId == guildId));
            UserXp.RemoveRange(UserXp.Where(x => x.GuildId == guildId));

            await SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DropUserAsync(ulong userId)
        {
            Users.Remove(await Users.FirstAsync(x => x.Id == userId).ConfigureAwait(false));
            Pastas.RemoveRange(Pastas.Where(x => x.OwnerId == userId));
            UserCommandUsage.RemoveRange(UserCommandUsage.Where(x => x.UserId == userId));
            Reputations.RemoveRange(Reputations.Where(x => x.Repee == userId || x.Reper == userId));
            UserXp.RemoveRange(UserXp.Where(x => x.UserId == userId));

            await SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<User> InsertUserAsync(IUser user)
            => await InsertUserAsync(new User
            {
                AvatarUrl = new Uri(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()),
                Id = user.Id,
                Username = user.Username
            }).ConfigureAwait(false);

        public async Task<User> InsertUserAsync(User user)
        {
            Users.Add(user);

            await SaveChangesAsync().ConfigureAwait(false);

            return Users.FirstOrDefault(x => x == user);
        }

        public async Task<Guild> InsertGuildAsync(IGuild guild, string prefix, string moneyname, string moneyicon)
            => await InsertGuildAsync(new Guild
            {
                Id = guild.Id,
                Prefix = prefix,
                MoneyName = moneyname,
                MoneyIcon = moneyicon
            }).ConfigureAwait(false);

        public async Task<Guild> InsertGuildAsync(Guild guild)
        {
            Guilds.Add(guild);
            Features.Add(new GuildFeatures
            {
                Id = guild.Id
            });
            Modules.Add(new GuildModules
            {
                Id = guild.Id
            });

            await SaveChangesAsync().ConfigureAwait(false);

            return Guilds.FirstOrDefault(x => x == guild);
        }

        public long GetPastaKarma(ulong UserId)
        {
            long returnkarma = 0;

            if (Pastas != null && Pastas.Count() > 0)
            {
                var ownedpastas = Pastas.Where(x => x.OwnerId == UserId);
                var pastaVotes = new List<PastaVotes>();

                foreach (var pasta in ownedpastas)
                {
                    var p = PastaVotes.FirstOrDefault(x => x.PastaId == pasta.Id);

                    if (p != null)
                        pastaVotes.Add(p);
                }

                if (pastaVotes != null)
                {
                    long upkarma = 0;
                    long downkarma = 0;
                    foreach (var pasta in pastaVotes)
                    {
                        if (pasta.Upvote)
                            upkarma += 1;
                        else
                            downkarma += 1;
                    }
                    returnkarma = upkarma - (downkarma / 5);
                }
            }

            return returnkarma;
        }

        public IReadOnlyList<UserExperience> GetOrderedGlobalExperienceLeaderboard()
        {
            if (UserXp.Any())
            {
                List<UserExperience> experiences = new List<UserExperience>();
                List<UserExperience> entries = new List<UserExperience>();

                foreach (var xp in UserXp)
                {
                    experiences.Add(xp);
                }

                foreach (var xp in experiences)
                {
                    if (!entries.Any(x => x.UserId == xp.UserId))
                    {
                        var uxp = new UserExperience
                        {
                            UserId = xp.UserId,
                            XP = (ulong)experiences.Where(x => x.UserId == xp.UserId).Sum(x => (float)x.XP),
                            TotalXP = (ulong)experiences.Where(x => x.UserId == xp.UserId).Sum(x => (float)x.TotalXP)
                        };
                        uxp.Level = DiscordTools.GetLevelFromTotalXP(uxp.TotalXP, DiscordTools.PHI);

                        entries.Add(uxp);
                    }
                }

                return entries.OrderByDescending(x => x.TotalXP).ToList().AsReadOnly();
            }
            else
            {
                return null;
            }
        }

        public IReadOnlyList<User> GetOrderedGlobalMoneyLeaderboard()
        {
            if (Users.Any())
            {
                List<User> entries = new List<User>();

                foreach (var user in Users)
                {
                    if (!entries.Any(x => x.Id == user.Id))
                    {
                        entries.Add(user);
                    }
                }

                return entries.OrderByDescending(x => x.Money).ToList().AsReadOnly();
            }
            else
            {
                return null;
            }
        }

        public async Task<IReadOnlyList<UserExperience>> GetOrderedGuildExperienceLeaderboardAsync(IGuild guild)
        {
            if (UserXp.Any())
            {
                List<UserExperience> experiences = new List<UserExperience>();
                List<UserExperience> entries = new List<UserExperience>();

                foreach (var xp in UserXp)
                {
                    if (await guild.GetUserAsync(xp.UserId).ConfigureAwait(false) != null)
                    {
                        experiences.Add(xp);
                    }
                }

                foreach (var xp in experiences)
                {
                    if (!entries.Any(x => x.UserId == xp.UserId))
                    {
                        var uxp = new UserExperience
                        {
                            UserId = xp.UserId,
                            XP = (ulong)experiences.Where(x => x.UserId == xp.UserId).Sum(x => (float)x.XP),
                            TotalXP = (ulong)experiences.Where(x => x.UserId == xp.UserId).Sum(x => (float)x.TotalXP)
                        };
                        uxp.Level = DiscordTools.GetLevelFromTotalXP(uxp.TotalXP, DiscordTools.PHI);

                        entries.Add(uxp);
                    }
                }

                return entries.OrderByDescending(x => x.TotalXP).ToList().AsReadOnly();
            }
            else
            {
                return null;
            }
        }

        public async Task<IReadOnlyList<User>> GetOrderedGuildMoneyLeaderboardAsync(IGuild guild)
        {
            if (Users.Any())
            {
                List<User> entries = new List<User>();

                foreach (var user in Users)
                {
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) != null)
                    {
                        if (!entries.Any(x => x.Id == user.Id))
                        {
                            entries.Add(user);
                        }
                    }
                }

                return entries.OrderByDescending(x => x.Money).ToList().AsReadOnly();
            }
            else
            {
                return null;
            }
        }

        public async Task<Guild> GetGuildAsync(IGuild guild)
        {
            var gld = Guilds.FirstOrDefault(x => x.Id == guild.Id);

            var config = await GetConfigAsync(SkuldAppContext.ConfigurationId).ConfigureAwait(false);

            if (gld == null)
                return await InsertGuildAsync(guild, config.Prefix, "Diamonds", "💠").ConfigureAwait(false);

            return gld;
        }

        public async Task<User> GetUserAsync(IUser user)
        {
            var usr = Users.FirstOrDefault(x => x.Id == user.Id);

            if (usr == null)
                return await InsertUserAsync(user).ConfigureAwait(false);

            return usr;
        }

        public async Task<SkuldConfig> GetConfigAsync(string configId = null)
        {
            if (configId == null)
                return await Configurations.FirstOrDefaultAsync().ConfigureAwait(false);
            else
                return await Configurations.FirstOrDefaultAsync(x => x.Id == configId).ConfigureAwait(false);
        }
    }
}