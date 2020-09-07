using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AgileLab.Views.Sprint
{
    class SprintDialogViewModel : ViewModelBase
    {
        #region Fields
        private string _mainGoal = string.Empty;
        private DateTime _startDate = default(DateTime);
        private DateTime _finishDate = default(DateTime);

        private ICommand _confirmCommand = null;
        private ICommand _cancelCommand = null;
        #endregion

        public SprintDialogViewModel()
        {
            InitializeCommands();
            InitializeDates();
        }

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

        private void InitializeDates()
        {
            StartDate = DateTime.Now;
            FinishDate = DateTime.Now;
            FinishDate = FinishDate.AddDays(1);
        }

        #region Properties
        public string MainGoal
        {
            get => _mainGoal;
            set => SetProperty(ref _mainGoal, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime FinishDate
        {
            get => _finishDate;
            set => SetProperty(ref _finishDate, value);
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
        public bool CanExecuteConfirmCommand(object param)
        {
            bool canExecute = true;

            do
            {
                if(string.IsNullOrEmpty(MainGoal))
                {
                    canExecute = false;
                    break;
                }

                if(FinishDate < StartDate)
                {
                    canExecute = false;
                    break;
                }

                if (FinishDate < DateTime.Now)
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
