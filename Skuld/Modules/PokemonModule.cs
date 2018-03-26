using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using Skuld.Tools;
using PokeSharp.Models;
using PokeSharp.Deserializer;

namespace Skuld.Modules
{
    public class Search : ModuleBase
    {
        [Command("pokemon", RunMode = RunMode.Async), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(string pokemon, string group) =>
            await SendPokemon(await PokeSharpClient.GetPocketMonster(pokemon.ToLowerInvariant()), group).ConfigureAwait(false);
        [Command("pokemon", RunMode = RunMode.Async), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(string pokemon) =>
            await SendPokemon(await PokeSharpClient.GetPocketMonster(pokemon.ToLowerInvariant()), "default").ConfigureAwait(false);
        [Command("pokemon", RunMode = RunMode.Async), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(int pokemonid, string group) =>
            await SendPokemon(await PokeSharpClient.GetPocketMonster(pokemonid), group).ConfigureAwait(false);
        [Command("pokemon", RunMode = RunMode.Async), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(int pokemonid) =>
            await SendPokemon(await PokeSharpClient.GetPocketMonster(pokemonid), "default").ConfigureAwait(false);
        public async Task SendPokemon(PocketMonster pokemon, string group)
        {
            EmbedBuilder embed;
            if (pokemon == null)
            {
                embed = new EmbedBuilder
                {
                    Color = Tools.Tools.RandomColor(),
                    Title = "Command Error!",
                    Description = "This pokemon doesn't exist. Please try again.\nIf it is a Generation 7, pokeapi.co hasn't updated for it yet."
                };
                StatsdClient.DogStatsd.Increment("commands.errors.generic");
            }
            else
            {
                var rnd = Bot.random;
                group = group.ToLower();
                string sprite = null;
                //if it equals 8 out of a random integer between 1 and 8192 then give shiny
                if (rnd.Next(1, 8193) == 8)
                { sprite = pokemon.Sprites.FrontShiny; }
                else
                { sprite = pokemon.Sprites.Front; }
                embed = new EmbedBuilder();
                var auth = new EmbedAuthorBuilder();
                embed.Color = Tools.Tools.RandomColor();
                if (group == "stat" || group == "stats")
                {
                    foreach (var stat in pokemon.Stats)
                    {
                        embed.AddField(stat.Stat.Name, "Base Stat: " + stat.BaseStat, inline: true);
                    }
                }
                if (group == "abilities" || group == "ability")
                {
                    foreach (var ability in pokemon.Abilities)
                    {
                        embed.AddField(ability.Ability.Name, "Slot: " + ability.Slot, inline: true);
                    }
                }
                if (group == "helditems" || group == "hitems" || group == "hitem" || group == "items")
                {
                    if (pokemon.HeldItems.Length > 0)
                    {
                        foreach (var hitem in pokemon.HeldItems)
                        {
                            foreach (var game in hitem.VersionDetails)
                            {
                                embed.AddField("Item", hitem.Item.Name + "\n**Game**\n" + game.Version.Name + "\n**Rarity**\n" + game.Rarity, inline: true);
                            }
                        }
                    }
                    else
                    {
                        embed.Description = "This pokemon doesn't hold any items in the wild";
                    }
                }
                if (group == "default")
                {
                    embed.AddField("Height", pokemon.Height + "mm", inline: true);
                    embed.AddField("Weight", pokemon.Weight + "kg", inline: true);
                    embed.AddField("ID", pokemon.ID.ToString(), inline: true);
                    embed.AddField("Base Experience", pokemon.BaseExperience.ToString(), inline: true);
                }
                if (group == "move" || group == "moves")
                {
                    var moves = pokemon.Moves.Take(4).Select(i => i).ToArray();
                    foreach (var move in moves)
                    {
                        string mve = move.Move.Name;
                        mve += "\n**Learned at:**\n" + "Level " + move.VersionGroupDetails.FirstOrDefault().LevelLearnedAt;
                        mve += "\n**Method:**\n" + move.VersionGroupDetails.FirstOrDefault().MoveLearnMethod.Name;
                        embed.AddField("Move", mve, inline: true);
                    }
                    auth.Url = "https://bulbapedia.bulbagarden.net/wiki/" + pokemon.Name + "_(Pokémon)";
                    embed.Footer = new EmbedFooterBuilder() { Text = "Click the name to view more moves, I limited it to 4 to prevent a wall of text" };
                }
                if (group == "games" || group == "game")
                {
                    string games = null;
                    foreach (var game in pokemon.GameIndices)
                    {
                        games += game.Version.Name + "\n";
                        if (game == pokemon.GameIndices.Last())
                        { games += game.Version.Name; }
                    }
                    embed.AddField("Game", games, inline: true);
                }
                string name = pokemon.Name;
                auth.Name = char.ToUpper(name[0]) + name.Substring(1);
                embed.Author = auth;
                embed.ThumbnailUrl = sprite;
            }
            await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
        }
    }
}
