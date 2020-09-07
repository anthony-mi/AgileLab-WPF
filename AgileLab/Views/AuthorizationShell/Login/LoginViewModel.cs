using AgileLab.Services.Authentication;
using AgileLab.Services.Registry;
using AgileLab.Views.Dialog;
using AgileLab.Views.AuthorizationShell;
using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using AgileLab.Services.Logging;
using System.Threading;

namespace AgileLab.Views.Login
{
    public class LoginViewModel : ViewModelBase
    {
        #region Fields
        private AuthorizationShellViewModel _mainViewModel;

        private IRegistryService _registryService = ComponentsContainer.Get<IRegistryService>();
        private IDialogCoordinator _dialogCoordinator = ComponentsContainer.Get<IDialogCoordinator>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();
        private AuthenticationService _authenticationService = new AuthenticationService();

        private string _username;
        private SecureString _password;
        private bool _rememberUser = true;
        #endregion

        #region Constructors
        internal LoginViewModel(AuthorizationShellViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
        #endregion

        #region Methods
        public void TryAutoLogIn()
        {
            _mainViewModel.IsLoadingData = true;

            Thread.Sleep(1200);

            new Task(() =>
            {
                try
                {
                    string username = _registryService.GetUsername();
                    string hash = _registryService.GetPasswordHash();
                    string message = string.Empty;

                    if (_authenticationService.LogIn(username, hash, ref message))
                    {
                        UserLoggedIn?.Invoke(this, username);
                    }
                    else
                    {
                        _logger.Warning($"Failed auto log in. Message: {message}.");
                    }
                }
                catch (AuthenticationException ex)
                {
                    
                }
                catch (Exception ex)
                {
                    _logger.Error($"Auto log in error");
                    _logger.Error(ex);
                }

                _mainViewModel.IsLoadingData = false;
            }).Start();
        }

        private bool CanExecuteLogin(object obj)
        {
            bool canExecute = true;

            do
            {
                if (Password == null)
                {
                    canExecute = false;
                    break;
                }

                if (string.IsNullOrEmpty(_username) || Password.Length < AuthenticationService.PasswordMinimalLength)
                {
                    canExecute = false;
                    break;
                }
            } while (false);
            
            return canExecute;
        }
        #endregion

        #region Properties
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public SecureString Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool RememberUser
        {
            get => _rememberUser;
            set => SetProperty(ref _rememberUser, value);
        }
        #endregion

        #region Commands
        public ICommand LoginCommand => new Command(Login, CanExecuteLogin);
        public ICommand SignUpCommand => new Command(
            () =>
            {
                SignUpRequested?.Invoke(this, null);
            },
            null);

        private void Login()
        {
            if (!CanExecuteLogin(null))
            {
                return;
            }

            _mainViewModel.IsLoadingData = true;

            new Task(() =>
            {
                string message = string.Empty;

                bool isLoggedIn = _authenticationService.LogIn(Username, Password, ref message);

                if (isLoggedIn)
                {
                    UserLoggedIn.Invoke(this, Username);

                    if (RememberUser)
                    {
                        string hash = _authenticationService.GetPasswordHash(Password);

                        _registryService.SetUsername(Username);
                        _registryService.SetPasswordHash(hash);
                    };
                }
                else
                {
                    _dialogCoordinator.ShowMessageAsync(this, "LogIn Error", message);
                }

                _mainViewModel.IsLoadingData = false;
            }).Start();
        }
        #endregion

        #region Events
        public event EventHandler<string> UserLoggedIn;
        public event EventHandler SignUpRequested;
        #endregion
    }
}
