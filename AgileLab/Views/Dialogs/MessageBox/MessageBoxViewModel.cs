using System;
using System.Windows.Input;

namespace AgileLab.Views.Dialogs.MessageBox
{
    class MessageBoxViewModel : ViewModelBase
    {
        #region Fileds
        private string _title = string.Empty;
        private string _text = string.Empty;

        private ICommand _confirmCommand = null;
        #endregion

        internal MessageBoxViewModel()
        {
            _confirmCommand = new Command(
                new Action(
                    delegate
                    {
                        Confirmed?.Invoke(this, null);
                    }),
                null);
        }

        #region Properties
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public ICommand ConfirmCommand
        {
            get => _confirmCommand;
        }
        #endregion

        public event EventHandler Confirmed;
    }
}
