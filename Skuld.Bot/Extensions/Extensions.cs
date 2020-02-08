using Microsoft.Extensions.DependencyInjection;
using Skuld.Bot.Models;
using Skuld.Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skuld.Bot.Extensions
{
    public static class Extensions
    {
        public static Weightable<T> GetRandomWeightedValue<T>(this IList<Weightable<T>> items)
        {
            var randomWeight = HostSerivce.Services.GetRequiredService<Random>().Next(0, items.Sum(x => x.Weight));

            Weightable<T> item = null;

            foreach (var entry in items)
            {
                item = entry;

                randomWeight -= entry.Weight;

                if (randomWeight <= 0)
                    break;
            }

            return item;
        }

        public static int FindIndex<T>(this IReadOnlyList<T> collection, Predicate<T> match)
            => collection.ToArray().FindIndex(match);

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = HostSerivce.Services.GetRequiredService<Random>().Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}