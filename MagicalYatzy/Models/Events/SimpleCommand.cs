using System;
using System.Windows.Input;

namespace Sanet.MagicalYatzy.Models.Events
{
    public class SimpleCommand : ICommand
    {
        private readonly Action _execute;

        public SimpleCommand(Action execute)
        {
            _execute = execute;
            CanExecuteChanged?.Invoke(this, null);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
