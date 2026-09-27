using System.Linq;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online.Commands.Server.State;

namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// Maps the authoritative game to a value-only <see cref="GameState"/> snapshot.
/// All values are null-safe defaults before the first roll.
/// </summary>
public static class GameStateMapper
{
    public static GameState Map(IGame game)
    {
        return new GameState
        {
            GameId = game.GameId,
            Rule = game.Rules.CurrentRule,
            Round = game.Round,
            IsPlaying = game.IsPlaying,
            ReRollMode = game.ReRollMode,
            CurrentPlayerId = game.CurrentPlayer?.InGameId ?? string.Empty,
            LastDiceValues = game.LastDiceResult?.DiceResults?.ToArray() ?? [],
            FixedDiceValues = [.. game.FixedRollResults],
            Players = game.Players?.Select(MapPlayer).ToList() ?? []
        };
    }

    private static PlayerState MapPlayer(IPlayer player)
    {
        return new PlayerState
        {
            PlayerId = player.InGameId ?? string.Empty,
            Name = player.Name ?? string.Empty,
            SeatNo = player.SeatNo,
            Type = player.Type,
            IsReady = player.IsReady,
            Roll = player.Roll,
            SelectedStyle = player.SelectedStyle,
            Total = player.Total,
            Scores = player.Results?.Select(MapScore).ToList() ?? [],
            Artifacts = player.MagicalArtifactsForGame?
                .Select(a => new ArtifactState { Type = a.Type, IsUsed = a.IsUsed })
                .ToList() ?? []
        };
    }

    private static ScoreState MapScore(IRollResult result)
    {
        return new ScoreState
        {
            ScoreType = result.ScoreType,
            Value = result.HasValue ? result.Value : 0,
            HasValue = result.HasValue,
            HasBonus = result.HasBonus
        };
    }
}
