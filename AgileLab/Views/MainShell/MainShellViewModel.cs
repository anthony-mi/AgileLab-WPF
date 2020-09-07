using System;
using System.Collections.Generic;
using AgileLab.Data;
using AgileLab.Data.Entities;
using AgileLab.Services.Logging;
using AgileLab.Services.Registry;
using AgileLab.Views.AuthorizationShell;
using AgileLab.Views.MenuBasedShell;

namespace AgileLab.Views.MainShell
{
    class MainShellViewModel : ViewModelBase
    {
        #region Fields
        private ViewModelBase _currentViewModel = null;

        private string _titleBarText = string.Empty;

        private User _currentUser = null;
        private Team _currentTeam = null;
        private Project _currentProject = null;

        private bool _isLoadingData = false;

        private ILogger _logger = ComponentsContainer.Get<ILogger>();
        private IUsersDataModel _usersDataModel = ComponentsContainer.Get<IUsersDataModel>();
        #endregion

        #region Constructors
        internal MainShellViewModel()
        {
            AuthorizationShellViewModel authorizationShellViewModel = new AuthorizationShellViewModel(this);
            authorizationShellViewModel.UserLoggedIn += UserLoggedInEventHandler;

            CurrentViewModel = authorizationShellViewModel;

            IsLoadingData = false;

            authorizationShellViewModel.TryAutoLigIn();
        }
        #endregion

        #region Properties
        public string TitleBarText
        {
            get => _titleBarText;
            private set => SetProperty(ref _titleBarText, value);
        }

        public bool IsLoadingData
        {
            get => _isLoadingData;
            set => SetProperty(ref _isLoadingData, value);
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                SetProperty(ref _currentViewModel, value);
            }
        }

        public User CurrentUser
        {
            get => _currentUser;

            private set
            {
                SetProperty(ref _currentUser, value);
                UpdateTitleBarText();
            }
        }

        public Team CurrentTeam
        {
            get => _currentTeam;

            set
            {
                SetProperty(ref _currentTeam, value);
                UpdateTitleBarText();
            }
        }

        public Project CurrentProject
        {
            get => _currentProject;

            set
            {
                SetProperty(ref _currentProject, value);
                UpdateTitleBarText();
            }
        }
        #endregion

        #region Methods
        public void UserLoggedInEventHandler(object sender, User user)
        {
            CurrentUser = user;
            CurrentViewModel = new MenuBasedShellViewModel(this);
        }

        internal void Logout()
        {
            try
            {
                ComponentsContainer.Get<IRegistryService>().RemoveAllData();
            }
            catch
            {
                throw;
            }

            CurrentUser = null;
            CurrentTeam = null;
            CurrentProject = null;

            AuthorizationShellViewModel authorizationShellViewModel = new AuthorizationShellViewModel(this);
            authorizationShellViewModel.UserLoggedIn += UserLoggedInEventHandler;

            CurrentViewModel = authorizationShellViewModel;
        }

        private void UpdateTitleBarText()
        {
            string indentation = "    ";
            string delimiter = "|";

            List<string> titleBarElements = new List<string>();

            titleBarElements.Add(System.Reflection.Assembly.GetEntryAssembly().GetName().Name); // Project (app) name

            if (CurrentUser != null)
            {
                titleBarElements.Add($"User: {CurrentUser.FirstName} {CurrentUser.LastName}");
            }

            if (CurrentTeam != null)
            {
                titleBarElements.Add($"Team: {CurrentTeam.Name}");
            }

            if (CurrentProject != null)
            {
                titleBarElements.Add($"Project: {CurrentProject.Name}");
            }

            TitleBarText = string.Join($"{indentation}{delimiter}{indentation}", titleBarElements.ToArray());
        }
        #endregion

    }
}
