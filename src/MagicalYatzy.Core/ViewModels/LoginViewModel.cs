using Sanet.MagicalYatzy.Services.Game;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
#region Fields
        private readonly IPlayerService _playerService;

        private string _newUsername;
        private string _newPassword;
#endregion

        public LoginViewModel(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        #region Properties
        public string NewUsername
        {
            get { return _newUsername; }
            set
            {
                SetProperty(ref _newUsername, value);
            }
        }

        public string NewPassword
        {
            get { return _newPassword; }
            set { SetProperty(ref _newPassword, value); }
        }
        #endregion

        #region Commands
        public ICommand LoginCommand => new SimpleCommand(async () => 
        {
            var result = await _playerService.LoginAsync(NewUsername,NewPassword);
            if (result != null)
                await CloseAsync(result);
        });

        public ICommand CloseCommand => new SimpleCommand(async () => await CloseAsync());

        public string CloseImage => "close.png";
        #endregion
    }
}
