using Sanet.MagicalYatzy.Models.Chat;
using Sanet.MagicalYatzy.Models.Events;
using System;
using System.Collections.Generic;

namespace Sanet.MagicalYatzy.Models.Game
{
    public interface IGame
    {
        event EventHandler<PlayerEventArgs> PlayerLeft;

        event EventHandler<RollEventArgs> DiceChanged;
        event EventHandler<FixDiceEventArgs> DiceFixed;
        event EventHandler<RollEventArgs> DiceRolled;
        event EventHandler<PlayerEventArgs> PlayerReady;
        event EventHandler<PlayerEventArgs> PlayerRerolled;
        event EventHandler<PlayerEventArgs> MagicRollUsed;
        event EventHandler<PlayerEventArgs> StyleChanged;

        IPlayer CurrentPlayer { get; }
        void ApplyScore(IRollResult result);
        void ChangeStyle(IPlayer player, DiceStyle style);
        void DoTurn();
        void FixAllDices(int value, bool isfixed);
        void FixDice(int value, bool isfixed);
        int NumberOfFixedDice { get; }
        event EventHandler GameFinished;
        string GameId { get; }
        bool IsDiceFixed(int value);
        bool IsPlaying { get; set; }
        string MyName { get; set; }
        void JoinGame(IPlayer player);
        DieResult LastDiceResult { get; }

        void ManualChange(bool isfixed, int oldvalue, int newvalue);
        int Turn { get; }
        int Roll { get; }
        event EventHandler<MoveEventArgs> MoveChanged;
        event EventHandler<ChatMessageEventArgs> OnChatMessage;
        void NextTurn();
        string Password { get; set; }
        event EventHandler<PlayerEventArgs> PlayerJoined;
        List<Player> Players { get; }
        int NumberOfPlayers { get; }
        void ReportMagictRoll();
        void ReportRoll();
        bool ReRollMode { get; set; }
        void ResetRolls();
        void RestartGame();
        event EventHandler<ResultEventArgs> ResultApplied;
        Rule Rules { get; }

        void LeaveGame(IPlayer player);
        void SetPlayerReady(IPlayer player, bool isready);
        void SetPlayerReady(bool isready);
        void SendChatMessage(ChatMessage message);
    }
}
