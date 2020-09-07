using AgileLab.Data;
using AgileLab.Data.Entities;
using AgileLab.Services.Logging;
using AgileLab.Views.Dialog;
using AgileLab.Views.MenuBasedShell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AgileLab.Views.AllTeams
{
    class AllTeamsViewModel : ViewModelBase
    {
        #region Fields
        private ITeamsDataModel _teamsDataModel = ComponentsContainer.Get<ITeamsDataModel>();
        private IDialogCoordinator _dialogCoordinator = ComponentsContainer.Get<IDialogCoordinator>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();

        private MenuBasedShellViewModel _menuBasedShellViewModel = null;

        private ObservableCollection<TeamItem> _filteredTeams = null;
        private List<TeamItem> _allTeams = null;

        private string _searchingFilter = string.Empty;

        private static readonly string _LEAVE_TEAM_ACTION_NAME = "Leave";
        private static readonly string _JOIN_TEAM_ACTION_NAME = "Join";

        private ICommand _joinCommand;
        private ICommand _leaveCommand;
        #endregion

        #region Constructors
        internal AllTeamsViewModel(MenuBasedShellViewModel menuBasedShellViewModel)
        {
            _menuBasedShellViewModel = menuBasedShellViewModel;

            CreateTeamItems();
            InitializeCommands();
            SubscribeToDataModelEvents();
        }

        private void InitializeCommands()
        {
            JoinCommand = new Command(parameterizedAction: new Action<object>(JoinTeam), canExecute: null);
            LeaveCommand = new Command(LeaveTeam, null);
        }

        private void CreateTeamItems()
        {

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    IList<ExtendedTeam> allTeams = _teamsDataModel.GetAllExtendedTeams();

                    uint userId = _menuBasedShellViewModel.CurrentUser.Id;
                    IList<Team> userTeams = _teamsDataModel.GetTeamsOfUser(userId);

                    _allTeams = new List<TeamItem>();

                    foreach (ExtendedTeam team in allTeams)
                    {
                        TeamItem teamItem = null;

                        if (userTeams.Contains(team))
                        {
                            teamItem = new TeamItem(team, _LEAVE_TEAM_ACTION_NAME, LeaveCommand);
                        }
                        else
                        {
                            teamItem = new TeamItem(team, _JOIN_TEAM_ACTION_NAME, JoinCommand);
                        }

                        _allTeams.Add(teamItem);
                    }

                    Teams = new ObservableCollection<TeamItem>(_allTeams);
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex);
                }

                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
        }

        private void SubscribeToDataModelEvents()
        {
            _teamsDataModel.NewTeamCreated += NewTeamCreatedEventHandler;
            _teamsDataModel.UserJoinedTeam += UserJoinedTeamEventHandler;
            _teamsDataModel.UserLeavedTeam += UserLeavedTeamEventHandler;

            ComponentsContainer.Get<IProjectsDataModel>().NewProjectCreated += NewProjectCreatedEventHandler;
        }
        #endregion

        #region Commands
        public ICommand JoinCommand
        {
            get => _joinCommand;
            set => SetProperty(ref _joinCommand, value);
        }

        public ICommand LeaveCommand
        {
            get => _leaveCommand;
            set => SetProperty(ref _leaveCommand, value);
        }
        #endregion

        #region Properties
        public ObservableCollection<TeamItem> Teams
        {
            get
            {
                if (_filteredTeams == null)
                {
                    CreateTeamItems();
                }

                return _filteredTeams;
            }

            set => SetProperty(ref _filteredTeams, value);
        }

        public string SearchingFilter
        {
            get => _searchingFilter;

            set
            {
                SetProperty(ref _searchingFilter, value);
                FilterTeams(_searchingFilter);
            }
        }
        #endregion

        #region Methods
        private void JoinTeam(object param)
        {
            if(param is TeamItem)
            {
                _menuBasedShellViewModel.IsLoadingData = true;

                new Task(() =>
                {
                    try
                    {
                        TeamItem teamItem = param as TeamItem;
                        Team team = new Team(teamItem.Id, teamItem.Name);

                        User user = _menuBasedShellViewModel.CurrentUser;

                        _logger.Debug($"Started joining user '{user.UserName}' to team '{team.Name}'");

                        _teamsDataModel.AddTeamMember(team, user);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"User doesn't joined: technical issues encountered.";
                        _dialogCoordinator.ShowMessageAsync(ViewContext, "Error", errorMessage);

                        _logger.Fatal(ex);
                    }

                    _menuBasedShellViewModel.IsLoadingData = false;
                }).Start();
            }
        }

        private void LeaveTeam(object param)
        {
            if (param is TeamItem)
            {
                _menuBasedShellViewModel.IsLoadingData = true;

                new Task(() =>
                {
                    try
                    {
                        TeamItem teamItem = param as TeamItem;
                        Team team = new Team(teamItem.Id, teamItem.Name);

                        User user = _menuBasedShellViewModel.CurrentUser;

                        _logger.Debug($"Started leaving user '{user.UserName}' from team '{team.Name}'");

                        _teamsDataModel.RemoveUserFromTeam(user, team);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"User doesn't removed from team: technical issues encountered.";
                        _dialogCoordinator.ShowMessageAsync(ViewContext, "Error", errorMessage);

                        _logger.Fatal(ex);
                    }

                    _menuBasedShellViewModel.IsLoadingData = false;
                }).Start();
            }
        }

        private void FilterTeams(string searchingFilter)
        {
            Teams = new ObservableCollection<TeamItem>(
                _allTeams.Where(
                    item => item.Name.ToLower().Contains(searchingFilter.ToLower()))
                    .ToList());
        }
        #endregion

        #region "Event Handlers"
        private void NewTeamCreatedEventHandler(object sender, Team team)
        {
            ExtendedTeam extendedTeam = _teamsDataModel.GetExtendedTeam(team);
            User currentUser = _menuBasedShellViewModel.CurrentUser;
            TeamItem teamItem = null;

            if (_teamsDataModel.IsTeamMember(team, currentUser))
            {
                teamItem = new TeamItem(extendedTeam, _LEAVE_TEAM_ACTION_NAME, LeaveCommand);
            }
            else
            {
                teamItem = new TeamItem(extendedTeam, _JOIN_TEAM_ACTION_NAME, JoinCommand);
            };

            _allTeams.Add(teamItem);

            if(teamItem.Name.Contains(SearchingFilter))
            {
                ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    Teams.Add(teamItem);
                });
            }
        }

        private void UserJoinedTeamEventHandler(Team team, User user)
        {
            TeamItem teamItem = _allTeams.Where(item => item.Id == team.Id).First();
            teamItem.CountOfMembers++;

            if (_menuBasedShellViewModel.CurrentUser.Equals(user))
            {
                teamItem.ActionName = _LEAVE_TEAM_ACTION_NAME;
                teamItem.TeamAction = LeaveCommand;
            }
        }

        private void UserLeavedTeamEventHandler(Team team, User user)
        {
            TeamItem teamItem = _allTeams.Where(item => item.Id == team.Id).First();
            teamItem.CountOfMembers--;

            if (_menuBasedShellViewModel.CurrentUser.Equals(user))
            {
                teamItem.ActionName = _JOIN_TEAM_ACTION_NAME;
                teamItem.TeamAction = JoinCommand;
            }
        }

        private void NewProjectCreatedEventHandler(object sender, Project project)
        {
            if(project == null || _allTeams == null)
            {
                return;
            }

            IEnumerable<TeamItem> teams = _allTeams.Where(item => item.Id == project.DevelopmentTeamId);

            if(teams.Count() == 0)
            {
                _logger.Error($"AllTeamsViewModel: development team not found in _allTeams list of project {project.Id}.");
                return;
            }

            TeamItem team = teams.First();
            team.CountOfProjects++;
        }
        #endregion

        public class TeamItem : INotifyPropertyChanged
        {
            private ExtendedTeam _team = null;
            private string _actionName = string.Empty;
            private ICommand _teamAction = null;

            public TeamItem(ExtendedTeam team, string actionName, ICommand action)
            {
                _team = team;
                _actionName = actionName;
                _teamAction = action;
            }

            public uint Id
            {
                get => _team.Id;
            }

            public string Name
            {
                get => _team.Name;
            }

            public uint CountOfMembers
            {
                get => _team.CountOfMembers;

                set
                {
                    _team.CountOfMembers = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CountOfMembers"));
                }
            }

            public uint CountOfProjects
            {
                get => _team.CountOfProjects;

                set
                {
                    _team.CountOfProjects = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CountOfProjects"));
                }
            }

            public string ActionName
            {
                get => _actionName;

                set
                {
                    _actionName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActionName"));
                }
            }

            public ICommand TeamAction
            {
                get => _teamAction;

                set
                {
                    _teamAction = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TeamAction"));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
