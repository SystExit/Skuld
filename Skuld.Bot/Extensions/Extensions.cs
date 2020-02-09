using Microsoft.Extensions.DependencyInjection;
using Skuld.Bot.Models;
using Skuld.Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skuld.Bot.Extensions
{
    public static class GenericExtensions
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
    }
}