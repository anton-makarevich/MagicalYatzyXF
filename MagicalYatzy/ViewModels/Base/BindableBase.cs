using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sanet.MagicalYatzy.ViewModels.Base
{
    public abstract class BindableBase: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(backingStore, value))
                return;
            backingStore = value;
            NotifyPropertyChanged(propertyName);
        }
    }
}
