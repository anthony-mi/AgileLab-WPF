using AgileLab.Views.AllUserTasks;
using AgileLab.Views.Dialog;
using AgileLab.Views.Dialogs.ConfirmationDialog;
using AgileLab.Views.MenuBasedShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileLab.Views.Logout
{
    class LogoutViewModel : ViewModelBase
    {
        private MenuBasedShellViewModel _menuBasedShellViewModel = null;

        private bool _showDialog = false;

        private ViewModelBase _currentDialogViewModel = null;
        private ConfirmationDialogViewModel _confirmationDialogViewModel = null;

        #region Constructors
        internal LogoutViewModel(MenuBasedShellViewModel menuBasedShellViewModel)
        {
            _menuBasedShellViewModel = menuBasedShellViewModel;

            SetDialog += delegate (object sender, ViewModelBase dialogViewModel)
            {
                CurrentDialogViewModel = dialogViewModel;
                ShowDialog = true;
            };

            HideDialog += delegate
            {
                ShowDialog = false;
            };

            ConfirmationDialogViewModel = new ConfirmationDialogViewModel();
            ConfirmationDialogViewModel.Text = "Do you really want to log out?";
            ConfirmationDialogViewModel.ConfirmSelected += delegate
            {
                try
                {
                    _menuBasedShellViewModel.Logout();
                }
                catch
                {
                    if(ViewContext != null)
                    {
                        ComponentsContainer.Get<IDialogCoordinator>().ShowMessageAsync(
                        ViewContext,
                        "Log out error",
                        "Please, run applications with administrator rights and try to log out again.");
                    }
                }
            };
            ConfirmationDialogViewModel.CancelSelected += delegate
            {
                _menuBasedShellViewModel.SetCurrentViewModel(typeof(AllUserTasksViewModel));
            };

            SetDialog?.Invoke(this, ConfirmationDialogViewModel);
        }
        #endregion

        #region Properties
        public bool ShowDialog
        {
            get => _showDialog;
            private set => SetProperty(ref _showDialog, value);
        }

        public ViewModelBase CurrentDialogViewModel
        {
            get => _currentDialogViewModel;
            private set => SetProperty(ref _currentDialogViewModel, value);
        }

        public ConfirmationDialogViewModel ConfirmationDialogViewModel
        {
            get => _confirmationDialogViewModel;
            private set => SetProperty(ref _confirmationDialogViewModel, value);
        }
        #endregion

        #region Events
        public event EventHandler<ViewModelBase> SetDialog;
        public event EventHandler HideDialog;
        #endregion
    }
}
