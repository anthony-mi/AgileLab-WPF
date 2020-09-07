using AgileLab.Data;
using AgileLab.Data.Entities;
using AgileLab.Services.Logging;
using AgileLab.Views.Dialog;
using AgileLab.Views.MenuBasedShell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AgileLab.Views.Teams
{
    class TeamsViewModel : ViewModelBase
    {
        #region Fields
        private ITeamsDataModel _teamsDataModel = ComponentsContainer.Get<ITeamsDataModel>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();

        private MenuBasedShellViewModel _menuBasedShellViewModel = null;

        private ObservableCollection<Team> _currentUserTeams = null;

        private bool _showDialog = false;

        private string _teamCreationErrorMessage = string.Empty;
        private bool _showTeamCreationErrorMessage = false;
        private string _newTeamName = string.Empty;

        private ICommand _requestTeamCreationCommand;
        private ICommand _createTeamCommand;
        private ICommand _cancelTeamCreationCommand;
        #endregion

        #region Constructors
        internal TeamsViewModel(MenuBasedShellViewModel menuBasedShellViewModel)
        {
            _menuBasedShellViewModel = menuBasedShellViewModel;

            InitializeCommands();
            InitializeTeams();
            SubscribeToDataModelEvents();
        }

        private void InitializeCommands()
        {
            RequestTeamCreationCommand = new Command(
                new Action(delegate { ShowDialog = true; }),
                null);

            CreateTeamCommand = new Command(
                CreateTeam,
                CanCreateTeam);

            CancelTeamCreationCommand = new Command(
                new Action(delegate { ShowDialog = false; }),
                null);
        }
        #endregion

        #region Commands
        public ICommand RequestTeamCreationCommand
        {
            get => _requestTeamCreationCommand;
            set => SetProperty(ref _requestTeamCreationCommand, value);
        }

        public ICommand CreateTeamCommand
        {
            get => _createTeamCommand;
            set => SetProperty(ref _createTeamCommand, value);
        }

        public ICommand CancelTeamCreationCommand
        {
            get => _cancelTeamCreationCommand;
            set => SetProperty(ref _cancelTeamCreationCommand, value);
        }
        #endregion

        #region Properties
        public ObservableCollection<Team> CurrentUserTeams
        {
            get
            {
                if (_currentUserTeams == null)
                {
                    InitializeTeams();
                }

                return _currentUserTeams;
            }

            private set => SetProperty(ref _currentUserTeams, value);
        }

        public Team SelectedTeam
        {
            get => _menuBasedShellViewModel.CurrentTeam;
        }

        public bool ShowDialog
        {
            get => _showDialog;

            set
            {
                if(value == true)
                {
                    NewTeamName = string.Empty;
                    TeamCreationErrorMessage = string.Empty;
                    ShowTeamCreationErrorMessage = false;
                }

                SetProperty(ref _showDialog, value);
            }
        }

        public string TeamCreationErrorMessage
        {
            get => _teamCreationErrorMessage;

            private set
            {
                SetProperty(ref _teamCreationErrorMessage, value);
            }
        }

        public bool ShowTeamCreationErrorMessage
        {
            get => !string.IsNullOrEmpty(_teamCreationErrorMessage);

            private set
            {
                SetProperty(ref _showTeamCreationErrorMessage, value);
            }
        }

        public string NewTeamName
        {
            get => _newTeamName;

            set
            {
                SetProperty(ref _newTeamName, value);
            }
        }
        #endregion

        #region Methods
        private void InitializeTeams()
        {
            uint userId = _menuBasedShellViewModel.CurrentUser.Id;
            IEnumerable<Team> teams = _teamsDataModel.GetTeamsOfUser(userId);
            CurrentUserTeams = new ObservableCollection<Team>(teams);
        }

        private void SubscribeToDataModelEvents()
        {
            _teamsDataModel.UserJoinedTeam += UserJoinedTeamEventHandler;
            _teamsDataModel.UserLeavedTeam += UserLeavedTeamEventHandler;
        }

        public void SelectTeam(Team team)
        {
            TeamSelected?.Invoke(this, team);
        }

        private void CreateTeam()
        {
            if(!CanCreateTeam(null))
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    if(_teamsDataModel.TeamExists(NewTeamName))
                    {
                        TeamCreationErrorMessage = $"Team '{NewTeamName}' already exists.";
                        ShowTeamCreationErrorMessage = true;
                        _menuBasedShellViewModel.IsLoadingData = false;
                        return;
                    };

                    Team newTeam = _teamsDataModel.CreateNewTeam(NewTeamName, _menuBasedShellViewModel.CurrentUser);
                    //CurrentUserTeams.Add(newTeam); // it will be done in UserJoinedTeamEventHandler method

                    ShowDialog = false;
                }
                catch (Exception ex)
                {
                    TeamCreationErrorMessage = $"Team doesn't created: technical issues encountered.";
                    ShowTeamCreationErrorMessage = true;

                    _logger.Fatal(ex);
                }
                
                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
        }

        private bool CanCreateTeam(object obj)
        {
            return !string.IsNullOrEmpty(NewTeamName);
        }
        #endregion

        #region "Event Handlers"
        private void UserJoinedTeamEventHandler(Team team, User user)
        {
            if(_menuBasedShellViewModel.CurrentUser.Equals(user))
            {
                if(!CurrentUserTeams.Contains(team))
                {
                    CurrentUserTeams.Add(team);
                }
            }
        }

        private void UserLeavedTeamEventHandler(Team team, User user)
        {
            if (_menuBasedShellViewModel.CurrentUser.Equals(user))
            {
                CurrentUserTeams.Remove(team);
            }
        }
        #endregion

        #region Events
        public event EventHandler<Team> TeamSelected;
        #endregion
    }
}
