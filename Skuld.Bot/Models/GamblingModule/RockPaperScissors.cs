namespace Skuld.Bot.Models.GamblingModule
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
                "ぐう" => RockPaperScissors.Rock,
                "🅱️ock" => RockPaperScissors.Rock,
                "paper" => RockPaperScissors.Paper,
                "p" => RockPaperScissors.Paper,
                "ぱあ" => RockPaperScissors.Paper,
                "🅱️aper" => RockPaperScissors.Paper,
                "scissors" => RockPaperScissors.Scissors,
                "s" => RockPaperScissors.Scissors
                "ちょき" => RockPaperScissors.Scissors
                "🅱️issors" => RockPaperScissors.Scissors
            };
    }
}
