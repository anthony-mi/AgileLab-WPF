using AgileLab.Data;
using AgileLab.Data.Entities;
using AgileLab.Services.Logging;
using AgileLab.Views.Login;
using AgileLab.Views.MainShell;
using AgileLab.Views.Projects;
using AgileLab.Views.SignUp;
using MahApps.Metro.Controls;
using System;

namespace AgileLab.Views.AuthorizationShell
{
    class AuthorizationShellViewModel : ViewModelBase
    {
        #region Fields
        private ViewModelBase _currentViewModel = null;
        private MainShellViewModel _mainShellViewModel = null;

        private LoginViewModel _loginViewModel = null;

        private ILogger _logger = ComponentsContainer.Get<ILogger>();
        private IUsersDataModel _usersDataModel = ComponentsContainer.Get<IUsersDataModel>();
        #endregion

        internal AuthorizationShellViewModel(MainShellViewModel mainShellViewModel)
        {
            _mainShellViewModel = mainShellViewModel;

            _loginViewModel = new LoginViewModel(this);

            _loginViewModel.UserLoggedIn += UserLoggedInEventHandler;
            _loginViewModel.SignUpRequested += SignUpRequestedEventHandler;

            CurrentViewModel = _loginViewModel;
            IsLoadingData = false;
        }

        public void TryAutoLigIn()
        {
            _loginViewModel.TryAutoLogIn();
        }

        private void UserLoggedInEventHandler(object sender, string username)
        {
            User user = _usersDataModel.GetUser(username);
            UserLoggedIn?.Invoke(this, user);
        }

        private void SignUpRequestedEventHandler(object sender, object args)
        {
            SignUpViewModel signUpViewModel = new SignUpViewModel(this);

            CurrentViewModel = signUpViewModel;

            signUpViewModel.ReturnBackRequested += ReturnBackRequestEventHandler;
            signUpViewModel.CancelRequested += ReturnBackRequestEventHandler;
            signUpViewModel.UserSignedUp += UserSignedUpEventHandler;
        }

        private void ReturnBackRequestEventHandler(object sender, EventArgs e)
        {
            if (sender is SignUpViewModel)
            {
                CurrentViewModel = _loginViewModel;
            }
        }

        private void UserSignedUpEventHandler(object sender, EventArgs e)
        {
            CurrentViewModel = _loginViewModel;
        }

        #region Properties
        public bool IsLoadingData
        {
            set => _mainShellViewModel.IsLoadingData = value;
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                SetProperty(ref _currentViewModel, value);
            }
        }

        //public User CurrentUser
        //{
        //    get => _currentUser;
        //    private set
        //    {
        //        SetProperty(ref _currentUser, value);
        //    }
        //}
        #endregion

        #region Events
        public event EventHandler<User> UserLoggedIn;
        #endregion
    }
}
