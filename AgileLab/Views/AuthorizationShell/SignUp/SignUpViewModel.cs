using AgileLab.Security;
using AgileLab.Services.Authentication;
using AgileLab.Services.Logging;
using AgileLab.Views.Dialog;
using AgileLab.Views.AuthorizationShell;
using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AgileLab.Views.SignUp
{
    class SignUpViewModel : ViewModelBase
    {
        #region Fields
        private AuthorizationShellViewModel _mainViewModel;

        private IHasher _hasher = ComponentsContainer.Get<IHasher>();
        private IDialogCoordinator _dialogCoordinator = ComponentsContainer.Get<IDialogCoordinator>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();
        private AuthenticationService _authenticationService = new AuthenticationService();

        private string _firstName;
        private string _lastName;
        private string _username;
        private SecureString _password;
        private SecureString _retypedPassword;

        private readonly ushort _passwordMinimalLength = Properties.Settings.Default.passwordMinimalLength;
        #endregion

        #region Constructors
        internal SignUpViewModel(AuthorizationShellViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
        #endregion

        #region Commands
        public ICommand ReturnBackCommand => new Command(
            () =>
            {
                ReturnBackRequested?.Invoke(this, null);
            },
            null);

        public ICommand CancelCommand => new Command(
            () =>
            {
                CancelRequested?.Invoke(this, null);
            },
            null);

        public ICommand SignUpCommand => new Command(
            this.SignUp,
            CanExecuteSignUp);
        #endregion

        #region Properties
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

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

        public SecureString RetypedPassword
        {
            get => _retypedPassword;
            set => SetProperty(ref _retypedPassword, value);
        }
        #endregion

        #region Methods
        private void SignUp()
        {
            if (!CanExecuteSignUp(null))
            {
                return;
            }

            _mainViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    _authenticationService.CreateNewUser(FirstName, LastName, Username, Password, RetypedPassword);
                    _dialogCoordinator.ShowMessageAsync(ViewContext, "Congratulations", "Your profile has been successfully created!");
                    UserSignedUp?.Invoke(this, null);
                }
                catch(AuthenticationException ex)
                {
                    _dialogCoordinator.ShowMessageAsync(ViewContext, "SignUp Error", ex.Message);
                }
                catch(Exception ex)
                {
                    _dialogCoordinator.ShowMessageAsync(ViewContext, "SignUp Error", "Technical issues encountered.");
                    _logger.Fatal(ex);
                }

                _mainViewModel.IsLoadingData = false;
            }).Start();
        }

        private bool CanExecuteSignUp(object obj)
        {
            bool canExecute = true;

            do
            {
                if (string.IsNullOrEmpty(FirstName))
                {
                    canExecute = false;
                    break;
                }

                if (string.IsNullOrEmpty(LastName))
                {
                    canExecute = false;
                    break;
                }

                if (string.IsNullOrEmpty(Username))
                {
                    canExecute = false;
                    break;
                }

                try
                {
                    if (Password.Length < AuthenticationService.PasswordMinimalLength)
                    {
                        canExecute = false;
                        break;
                    }

                    if (RetypedPassword.Length != Password.Length)
                    {
                        canExecute = false;
                        break;
                    }
                }
                catch
                {
                    canExecute = false;
                    break;
                }
                

                //
                //  The code below is commented out because it takes too many resources.
                //  Functionality moved to the registration process.
                //

                //AuthenticationService authenticationService = new AuthenticationService();

                //string passwordHash = authenticationService.GetPasswordHash(Password);
                //string retypedPasswordHash = authenticationService.GetPasswordHash(RetypedPassword);

                //if (!Equals(passwordHash, retypedPasswordHash))
                //{
                //    canExecute = false;
                //    break;
                //}
            } while (false);

            return canExecute;
        }
        #endregion

        #region Events
        public event EventHandler ReturnBackRequested;
        public event EventHandler CancelRequested;
        public event EventHandler UserSignedUp;
        #endregion
    }
}
