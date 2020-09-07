using AgileLab.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AgileLab.Views.Dialogs.Story
{
    class StoryDialogViewModel : ViewModelBase
    {
        #region Fileds
        private string _name = string.Empty;
        private uint _importance = 1;
        private uint _initialEstimate = 1;
        private string _status = string.Empty;
        private string _howToDemo = string.Empty;
        private string _notes = string.Empty;

        private bool _isStatusEditable = true;

        private string _errorMessage = string.Empty;
        private bool _showErrorMessage = false;

        private ICommand _confirmCommand = null;
        private ICommand _cancelCommand = null;

        private ObservableCollection<string> _storyStatuses = null;
        #endregion

        #region Constructors
        public StoryDialogViewModel()
        {
            InitializeCommands();
        }
        #endregion

        #region Properties
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public uint Importance
        {
            get => _importance;
            set => SetProperty(ref _importance, value);
        }

        public uint InitialEstimate
        {
            get => _initialEstimate;
            set => SetProperty(ref _initialEstimate, value);
        }

        public ObservableCollection<string> Statuses
        {
            get
            {
                if (_storyStatuses == null)
                {
                    _storyStatuses = new ObservableCollection<string>(ComponentsContainer.Get<IStoryStatusesDataModel>().GetAllStatuses());
                }

                return _storyStatuses;
            }

            set
            {
                SetProperty(ref _storyStatuses, value);
            }
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsStatusEditable
        {
            get => _isStatusEditable;
            set => SetProperty(ref _isStatusEditable, value);
        }

        public string HowToDemo
        {
            get => _howToDemo;
            set => SetProperty(ref _howToDemo, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool ShowErrorMessage
        {
            get => _showErrorMessage;
            set => SetProperty(ref _showErrorMessage, value);
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

        #region Methods
                private void InitializeCommands()
        {
            _confirmCommand = new Command(
                new Action(delegate
                {
                    ConfirmSelected?.Invoke(this, null);
                }),
                CanExecuteConfirmCommand);

            _cancelCommand = new Command(
                new Action(delegate
                {
                    CancelSelected?.Invoke(this, null);
                }),
                null);
        }

        public bool CanExecuteConfirmCommand(object param)
        {
            bool canExecute = true;

            do
            {
                if(string.IsNullOrEmpty(Name))
                {
                    canExecute = false;
                    break;
                }

                if (string.IsNullOrEmpty(Status))
                {
                    canExecute = false;
                    break;
                }
            } while (false);

            return canExecute;
        }
        #endregion
    }
}
