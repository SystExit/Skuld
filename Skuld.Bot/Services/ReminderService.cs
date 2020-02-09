using Discord;
using Skuld.Bot.Models.Services.Reminder;
using Skuld.Core.Extensions;
using Skuld.Discord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Services
{
    public class ReminderService
    {
        private static readonly List<Reminder> reminders = new List<Reminder>();
        public static IReadOnlyList<Reminder> Reminders { get => reminders.AsReadOnly(); }

        private static async Task ExecuteAsync()
        {
            while (true)
            {
                if (reminders.Any())
                {
                    var currentTime = DateTime.Now.ToEpoch();

                    List<Reminder> RemoveReminders = new List<Reminder>();

                    foreach (var entry in reminders)
                    {
                        if (entry.Timeout <= currentTime)
                        {
                            await MessageSender.ReplyAsync((await entry.User.GetOrCreateDMChannelAsync().ConfigureAwait(false) ?? entry.Channel), $"On {entry.Created.ToString("yyyy'/'MM'/'dd HH:mm:ss")} you asked me to remind you: {entry.Content}").ConfigureAwait(false);

                            RemoveReminders.Add(entry);
                        }
                    }

                    foreach (var rem in RemoveReminders)
                    {
                        reminders.Remove(rem);
                    }
                }

                await Task.Delay(50).ConfigureAwait(false);
            }
        }

        public static void AddReminder(Reminder reminder)
            => reminders.Add(reminder);

        public static void RemoveReminder(ushort Id, IUser user)
            => reminders.Remove(reminders.FirstOrDefault(x => x.Id == Id && x.User.Id == user.Id));

        public static void Run()
            => Task.Run(async () => await ExecuteAsync().ConfigureAwait(false));
    }
}