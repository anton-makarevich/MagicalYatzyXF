using Sanet.MagicalYatzy.Models.Chat;
using Sanet.MagicalYatzy.Models.Events;
using System;

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

        IPlayer CurrentPlayer { get; set; }
        void ApplyScore(IRollResult result);
        void ChangeStyle(IPlayer player, DiceStyle style);
        void DoMove();
        void FixAllDices(int value, bool isfixed);
        void FixDice(int value, bool isfixed);
        int FixedDicesCount { get; }
        event EventHandler GameFinished;
        int GameId { get; set; }
        //int Roll { get; }
        bool IsDiceFixed(int value);
        bool IsPlaying { get; set; }
        string MyName { get; set; }
        void JoinGame(IPlayer player);
        DieResult LastDiceResult { get; }

        void ManualChange(bool isfixed, int oldvalue, int newvalue);
        int Move { get; set; }
        event EventHandler<MoveEventArgs> MoveChanged;
        event EventHandler<ChatMessageEventArgs> OnChatMessage;
        void NextMove();
        string Password { get; set; }
        event EventHandler<PlayerEventArgs> PlayerJoined;
        global::System.Collections.Generic.List<IPlayer> Players { get; set; }
        int PlayersNumber { get; }
        void ReporMagictRoll();
        void ReportRoll();
        bool RerollMode { get; set; }
        void ResetRolls();
        void RestartGame();
        event EventHandler<ResultEventArgs> ResultApplied;
        Rule Rules { get; set; }

        void LeaveGame(IPlayer player);
        void SetPlayerReady(IPlayer player, bool isready);
        void SetPlayerReady(bool isready);
        void SendChatMessage(ChatMessage message);
    }
}
