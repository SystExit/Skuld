namespace Skuld.Bot.Models.Commands.GamblingModule
{
    public enum RockPaperScissors
    {
        Rock = 0,
        Paper = 1,
        Scissors = 2
    }

    public static class RockPaperScissorsHelper
    {
        public static RockPaperScissors FromString(string input)
            => input.ToLowerInvariant() switch
            {
                "rock" => RockPaperScissors.Rock,
                "r" => RockPaperScissors.Rock,
                "paper" => RockPaperScissors.Paper,
                "p" => RockPaperScissors.Paper,
                "scissors" => RockPaperScissors.Scissors,
                "s" => RockPaperScissors.Scissors
            };
    }
}