using System.Collections.ObjectModel;
using System.Linq;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.ViewModels
{
    public class GameViewModel: DicePanelViewModel
    {
        private readonly IGameService _gameService;

        public GameViewModel(IGameService gameService,
            IDicePanel dicePanel):base(dicePanel)
        {
            _gameService = gameService;
            foreach (var player in Game.Players)
            {
                Players.Add(new PlayerViewModel(player));
            }
        }

        public IGame Game => _gameService?.CurrentLocalGame;
        
        public PlayerViewModel CurrentPlayer
        {
            get
            {
                return Game.CurrentPlayer == null ? null : Players.FirstOrDefault(f=>f.Player.InGameId==Game.CurrentPlayer.InGameId);
            }
        }

        public ObservableCollection<PlayerViewModel> Players { get; } = new ObservableCollection<PlayerViewModel>();

        public override void AttachHandlers()
        {
            base.AttachHandlers();
            Game.DiceFixed += GameOnDiceFixed;
            Game.DiceRolled += GameOnDiceRolled;
        }

        private void GameOnDiceRolled(object sender, RollEventArgs e)
        {
            do
            {
                DicePanel.RollDice(e.Value.ToList());
            } while (!DicePanel.IsRolling);
            
//            CurrentPlayer.Player.CheckRollResults();
//            _rollingPlayer = e.Player.Name;
//            RollResults = null;
//            SetCanRoll(false);
//            NotifyPlayerChanged();
        }

        private void GameOnDiceFixed(object sender, FixDiceEventArgs e)
        {
            if (!CurrentPlayer.Player.IsHuman)
                DicePanel.FixDice(e.Value,e.Isfixed);        
        }

        public override void DetachHandlers()
        {
            base.DetachHandlers();
            Game.DiceFixed -= GameOnDiceFixed;
            Game.DiceRolled -= GameOnDiceRolled;
        }
    }
}