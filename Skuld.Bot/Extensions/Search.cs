using Discord;
using PokeAPI;
using Skuld.APIS.Pokemon.Models;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Discord.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Extensions
{
    public static class Search
    {
        public static async Task<Embed> GetEmbedAsync(this PokemonSpecies pokemon, PokemonDataGroup group)
        {
            if (pokemon == null) return null;

            var poke = await DataFetcher.GetApiObject<Pokemon>(pokemon.ID).ConfigureAwait(false);

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"{char.ToUpperInvariant(pokemon.Name[0])}{pokemon.Name.Substring(1)} - {pokemon.ID}"
                },
                Color = Color.Blue
            };

            var result = SkuldRandom.Next(0, 8193);
            string sprite = null;
            //if it equals 8 out of a random integer between 1 and 8192 then give shiny
            if (result == 8)
            {
                sprite = poke.Sprites.FrontShinyMale ?? poke.Sprites.FrontShinyFemale;
            }
            else
            {
                sprite = poke.Sprites.FrontMale ?? poke.Sprites.FrontFemale;
            }

            switch (group)
            {
                case PokemonDataGroup.Default:
                    embed.AddInlineField("Height", poke.Height + "dm");
                    //embed.AddInlineField("Weight", poke.Weight + "hg");
                    embed.AddInlineField("Base Experience", $"{poke.BaseExperience}xp");
                    break;

                case PokemonDataGroup.Abilities:
                    foreach (var ability in poke.Abilities)
                    {
                        embed.AddInlineField(ability.Ability.Name, "Slot: " + ability.Slot);
                    }
                    break;

                case PokemonDataGroup.Games:
                    string games = null;
                    foreach (var game in poke.GameIndices)
                    {
                        games += game.Version.Name + "\n";
                        if (game.GameIndex == poke.GameIndices.Last().GameIndex)
                        {
                            games += game.Version.Name;
                        }
                    }
                    embed.AddInlineField("Game", games);
                    break;

                case PokemonDataGroup.HeldItems:
                    if (poke.HeldItems.Length > 0)
                    {
                        foreach (var hitem in poke.HeldItems)
                        {
                            foreach (var game in hitem.VersionDetails)
                            {
                                embed.AddInlineField("Item", hitem.Item.Name + "\n**Game**\n" + game.Version.Name + "\n**Rarity**\n" + game.Rarity);
                            }
                        }
                    }
                    else
                    {
                        embed.Description = "This pokemon doesn't hold any items in the wild";
                    }
                    break;

                case PokemonDataGroup.Moves:
                    var moves = poke.Moves.Take(4).Select(i => i).ToArray();
                    foreach (var move in moves)
                    {
                        string mve = move.Move.Name;
                        mve += "\n**Learned at:**\n" + "Level " + move.VersionGroupDetails.FirstOrDefault().LearnedAt;
                        mve += "\n**Method:**\n" + move.VersionGroupDetails.FirstOrDefault().LearnMethod.Name;
                        embed.AddInlineField("Move", mve);
                    }
                    embed.Author.Url = "https://bulbapedia.bulbagarden.net/wiki/" + pokemon.Name + "_(Pokémon)";
                    embed.Footer = new EmbedFooterBuilder { Text = "Click the name to view more moves, I limited it to 4 to prevent a wall of text" };
                    break;

                case PokemonDataGroup.Stats:
                    foreach (var stat in poke.Stats)
                    {
                        embed.AddInlineField(stat.Stat.Name, "Base Stat: " + stat.BaseValue);
                    }
                    break;
            }
            embed.ThumbnailUrl = sprite;

            return embed.Build();
        }
    }
}