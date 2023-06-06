using System.Windows.Input;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.Models
{
    public class MainMenuAction : BaseViewModel
    {
        #region Fields
        public ICommand MenuAction { get; set; }
        #endregion
        #region Properties
        /// <summary>
        /// Main Label
        /// </summary>
        private string _label;
        virtual public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                SetProperty(ref _label, value);
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                SetProperty(ref _description, value);
            }
        }

        private string _image;
        public string Image
        {
            get { return _image; }
            set
            {
                SetProperty(ref _image, value);
            }
        }
        #endregion
    }
}
