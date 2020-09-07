using System;
using System.Windows.Input;

namespace AgileLab.Views.Dialogs.ConfirmationDialog
{
    class ConfirmationDialogViewModel : ViewModelBase
    {
        #region Fileds
        private string _text = string.Empty;

        private ICommand _confirmCommand = null;
        private ICommand _cancelCommand = null;
        #endregion

        public ConfirmationDialogViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            _confirmCommand = new Command(
                new Action(delegate
                {
                    ConfirmSelected?.Invoke(this, null);
                }),
                null);

            _cancelCommand = new Command(
                new Action(delegate
                {
                    CancelSelected?.Invoke(this, null);
                }),
                null);
        }

        #region Properties
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public ICommand ConfirmCommand
        {
            get => _confirmCommand;
            set => SetProperty(ref _confirmCommand, value);
        }

        public ICommand CancelCommand
        {
            get => _cancelCommand;
            set => SetProperty(ref _cancelCommand, value);
        }
        #endregion

        #region Events
        public event EventHandler ConfirmSelected;
        public event EventHandler CancelSelected;
        #endregion
    }
}
