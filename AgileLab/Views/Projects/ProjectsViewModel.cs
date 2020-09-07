using AgileLab.Data;
using AgileLab.Data.Entities;
using AgileLab.Views.Dialog;
using AgileLab.Views.AuthorizationShell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AgileLab.Views.MenuBasedShell;
using AgileLab.Services.Logging;
using System.ComponentModel;
using System.Linq.Expressions;
using AgileLab.Views.Teams;

namespace AgileLab.Views.Projects
{
    class ProjectsViewModel : ViewModelBase
    {
        #region Fields
        private MenuBasedShellViewModel _menuBasedShellViewModel = null;

        private IProjectsDataModel _projectsDataModel = ComponentsContainer.Get<IProjectsDataModel>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();

        private ObservableCollection<Project> _projects = new ObservableCollection<Project>();

        private bool _showCreateNewProjectDialog = false;
        private bool _showErrorMessageDialog = false;
        private string _errorText = string.Empty;

        private string _projectCreationErrorMessage = string.Empty;
        private bool _showProjectCreationErrorMessage = false;
        private string _newProjectName = string.Empty;
        #endregion

        #region Constructors
        internal ProjectsViewModel(MenuBasedShellViewModel menuBasedShellViewModel)
        {
            _menuBasedShellViewModel = menuBasedShellViewModel;
            _menuBasedShellViewModel.PropertyChanged += MenuBasedShellViewModelPropertyChangedEventHandler;

            InitializeProjects();
            SubscribeToDataModelEvents();
        }
        #endregion

        private void InitializeProjects()
        {
            Team currentTeam = _menuBasedShellViewModel.CurrentTeam;

            if(currentTeam == null)
            {
                ShowErrorMessage("Select team!");
                return;
            }

            IEnumerable<Project> projects = _projectsDataModel.GetProjectsOfTeam(currentTeam);
            Projects = new ObservableCollection<Project>(projects);
        }

        private void SubscribeToDataModelEvents()
        {
            _projectsDataModel.NewProjectCreated += NewProjectCreatedEventHandler;
        }

        #region Commands
        public ICommand HideErrorMessageDialog => new Command(
            () =>
            {
                ShowErrorMessageDialog = false;

                if(_menuBasedShellViewModel.CurrentTeam == null)
                {
                    _menuBasedShellViewModel.SetCurrentViewModel(typeof(TeamsViewModel));
                }
            },
            null);

        public ICommand RequestProjectCreation => new Command(
            new Action(
                delegate 
                {
                    if (_menuBasedShellViewModel.CurrentTeam == null)
                    {
                        ShowErrorMessage("Select team!");
                    }
                    else
                    {
                        ShowCreateNewProjectDialog = true;
                    }
                }),
            null);

        public ICommand CreateProjectCommand => new Command(
            CreateProject,
            CanCreateProject);

        public ICommand CancelProjectCreation => new Command(
            new Action(delegate { ShowCreateNewProjectDialog = false; }),
            null);
        #endregion

        #region Properties
        public ObservableCollection<Project> Projects
        {
            get
            {
                if(_projects == null)
                {
                    InitializeProjects();
                }

                return _projects;
            }

            private set => SetProperty(ref _projects, value);
        }

        public bool ShowCreateNewProjectDialog
        {
            get => _showCreateNewProjectDialog;

            set
            {
                if (value == true)
                {
                    NewProjectName = string.Empty;
                    ProjectCreationErrorMessage = string.Empty;
                    ShowProjectCreationErrorMessage = false;
                }

                SetProperty(ref _showCreateNewProjectDialog, value);
            }
        }

        public bool ShowErrorMessageDialog
        {
            get => _showErrorMessageDialog;
            set => SetProperty(ref _showErrorMessageDialog, value);
        }

        public string ErrorText
        {
            get => _errorText;

            private set => SetProperty(ref _errorText, value);
        }

        public bool ShowProjectCreationErrorMessage
        {
            get => _showProjectCreationErrorMessage;
            set => SetProperty(ref _showProjectCreationErrorMessage, value);
        }
        
        public string ProjectCreationErrorMessage
        {
            get
            {
                if(_projectCreationErrorMessage == null)
                {
                    _projectCreationErrorMessage = string.Empty;
                }

                return _projectCreationErrorMessage;
            }

            set => SetProperty(ref _projectCreationErrorMessage, value);
        }

        public Project SelectedProject
        {
            get => _menuBasedShellViewModel.CurrentProject;
        }

        public string NewProjectName
        {
            get => _newProjectName;

            set
            {
                SetProperty(ref _newProjectName, value);
            }
        }
        #endregion

        #region Methods
        private void ShowErrorMessage(string errorText)
        {
            ErrorText = errorText;
            ShowErrorMessageDialog = true;
        }

        public void SelectProject(Project project)
        {
            ProjectSelected?.Invoke(this, project);
        }

        private void CreateProject()
        {
            if (!CanCreateProject(null) || _menuBasedShellViewModel.CurrentTeam == null)
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    if (_projectsDataModel.ProjectExists(NewProjectName, _menuBasedShellViewModel.CurrentTeam.Id))
                    {
                        ProjectCreationErrorMessage = $"Project '{NewProjectName}' already exists.";
                        ShowProjectCreationErrorMessage = true;
                        _menuBasedShellViewModel.IsLoadingData = false;
                        return;
                    };

                    Project newProject = _projectsDataModel.CreateNewProject(NewProjectName, _menuBasedShellViewModel.CurrentUser, _menuBasedShellViewModel.CurrentTeam);
                    //Projects.Add(newProject); // it will be done in UserJoinedTeamEventHandler method

                    ShowCreateNewProjectDialog = false;
                }
                catch (Exception ex)
                {
                    ProjectCreationErrorMessage = $"Project doesn't created: technical issues encountered.";
                    ShowProjectCreationErrorMessage = true;

                    _logger.Fatal(ex);
                }

                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
        }

        private bool CanCreateProject(object obj)
        {
            return !string.IsNullOrEmpty(NewProjectName);
        }

        static string GetPropertyName<TObject>(Expression<Func<TObject, object>> exp)
        {
            Expression body = exp.Body;
            UnaryExpression convertExpression = body as UnaryExpression;

            if (convertExpression != null)
            {
                if (convertExpression.NodeType != ExpressionType.Convert)
                {
                    throw new ArgumentException("Invalid property expression.", "exp");
                }
                body = convertExpression.Operand;
            }
            return ((MemberExpression)body).Member.Name;
        }
        #endregion

        #region "Event Handlers"
        private void MenuBasedShellViewModelPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            string CurrentTeamPropertyName = GetPropertyName<MenuBasedShellViewModel>(item => item.CurrentTeam);
            string CurrentViewModelPropertyName = GetPropertyName<MenuBasedShellViewModel>(item => item.CurrentViewModel);

            if(e.PropertyName.Equals(CurrentTeamPropertyName)) // Can't use switch-case statement because property names are not constants.
            {
                CurrentTeamChangedEventHandler();
            }
            else if(e.PropertyName.Equals(CurrentViewModelPropertyName))
            {
                CurrentViewModelChangedEventHandler();
            }
        }

        private void CurrentTeamChangedEventHandler()
        {
            InitializeProjects();
        }

        private void CurrentViewModelChangedEventHandler()
        {
            if (_menuBasedShellViewModel.CurrentViewModel.Equals(this))
            {
                Team currentTeam = _menuBasedShellViewModel.CurrentTeam;

                if (currentTeam == null)
                {
                    ShowErrorMessage("Select team!");
                }
                else
                {
                    ShowErrorMessageDialog = false;
                    InitializeProjects();
                }
            }
        }

        private void NewProjectCreatedEventHandler(object sender, Project project)
        {
            if(project.DevelopmentTeamId == _menuBasedShellViewModel.CurrentTeam.Id)
            {
                Projects.Add(project);
            }
        }
        #endregion

        #region Events
        public event EventHandler<Project> ProjectSelected;
        #endregion
    }
}
