using AgileLab.Data;
using AgileLab.Data.Entities;
using AgileLab.Services.Logging;
using AgileLab.Services.Registry;
using AgileLab.Views.AllTeams;
using AgileLab.Views.AllUserTasks;
using AgileLab.Views.MainShell;
using AgileLab.Views.ProductBacklog;
using AgileLab.Views.Projects;
using AgileLab.Views.Review;
using AgileLab.Views.Sprint;
using AgileLab.Views.Teams;
using Autofac;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Threading;

namespace AgileLab.Views.MenuBasedShell
{
    class MenuBasedShellViewModel : ViewModelBase
    {
        #region Fields
        private MainShellViewModel _mainShellViewModel = null;

        private ILogger _logger = ComponentsContainer.Get<ILogger>();
        private IContainer _viewModelsContainer = null;

        private HamburgerMenuItemCollection _menuItems = null;
        private HamburgerMenuItemCollection _menuOptionItems = null;
        private int _selectedMenuIndex = 0;
        private ViewModelBase _currentViewModel = null;

        private Dispatcher _dispatcher = ComponentsContainer.Get<Dispatcher>();
        private IRegistryService _registryService = ComponentsContainer.Get<IRegistryService>();
        #endregion

        #region Methods
        internal void Logout()
        {
            try
            {
                _mainShellViewModel.Logout();
            }
            catch
            {
                throw;
            }
        }

        internal MenuBasedShellViewModel(MainShellViewModel mainShellViewModel)
        {
            _mainShellViewModel = mainShellViewModel;

            InitializeViewModelsContainer();
            CreateMenuItems();

            if(InitializeCurrentTeam())
            {
                if(InitializeCurrentProject())
                {
                    CurrentViewModel = _viewModelsContainer.Resolve<SprintViewModel>();
                }
                else
                {
                    CurrentViewModel = _viewModelsContainer.Resolve<ProjectsViewModel>();
                }
            }
            else
            {
                CurrentViewModel = _viewModelsContainer.Resolve<TeamsViewModel>();
            }
        }

        private bool InitializeCurrentTeam()
        {
            bool initialized = false;

            uint currentTeamId = _registryService.GetCurrentTeamId();

            if(currentTeamId != 0)
            {
                Team team = ComponentsContainer.Get<ITeamsDataModel>().GetTeamById(currentTeamId);

                if(team != null)
                {
                    _mainShellViewModel.CurrentTeam = team;
                    initialized = true;
                }
            }

            return initialized;
        }

        private bool InitializeCurrentProject()
        {
            bool initialized = false;

            uint currentProjectId = _registryService.GetCurrentProjectId();

            if (currentProjectId != 0)
            {
                Project project = ComponentsContainer.Get<IProjectsDataModel>().GetProjectById(currentProjectId);

                if (project != null)
                {
                    _mainShellViewModel.CurrentProject = project;
                    initialized = true;
                }
            }

            return initialized;
        }

        private void InitializeViewModelsContainer()
        {
            TeamsViewModel teamsViewModel = new TeamsViewModel(this);
            teamsViewModel.TeamSelected += OnTeamSelected;

            AllTeamsViewModel allTeamsViewModel = new AllTeamsViewModel(this);
            // TODO: subscribe to AllTeamsViewModel events

            ProjectsViewModel projectsViewModel = new ProjectsViewModel(this);
            projectsViewModel.ProjectSelected += OnProjectSelected;

            ProductBacklogViewModel productBacklogViewModel = new ProductBacklogViewModel(this);
            SprintViewModel sprintViewModel = new SprintViewModel(this);
            AllUserTasksViewModel allUserTasksViewModel = new AllUserTasksViewModel(this);
            ReviewViewModel reviewViewModel = new ReviewViewModel(this);
            Logout.LogoutViewModel logoutViewModel = new Logout.LogoutViewModel(this);

            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterInstance(projectsViewModel);
            builder.RegisterInstance(teamsViewModel);
            builder.RegisterInstance(allTeamsViewModel);
            builder.RegisterInstance(productBacklogViewModel);
            builder.RegisterInstance(sprintViewModel);
            builder.RegisterInstance(allUserTasksViewModel);
            builder.RegisterInstance(reviewViewModel);
            builder.RegisterInstance(logoutViewModel);

            _viewModelsContainer = builder.Build();
        }

        public void CreateMenuItems()
        {
            _dispatcher.Invoke(delegate
            {
                MenuItems = new HamburgerMenuItemCollection
            {
                    new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.Buffer},
                    Label = "All Your Tasks",
                    ToolTip = "Stories from current sprints of all projects which you executing.",
                    Tag = _viewModelsContainer.Resolve<AllUserTasksViewModel>()
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.AccountMultiple},
                    Label = "Your Teams",
                    ToolTip = "Teams of which you are a member.",
                    Tag = _viewModelsContainer.Resolve<TeamsViewModel>()
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.AccountGroup},
                    Label = "All Teams",
                    ToolTip = "All available teams.",
                    Tag = _viewModelsContainer.Resolve<AllTeamsViewModel>()
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.FileDocument},
                    Label = "Projects",
                    ToolTip = "All projects of your current team.",
                    Tag = _viewModelsContainer.Resolve<ProjectsViewModel>()
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.FormatListChecks},
                    Label = "Product Backlog",
                    ToolTip = "Backlog of your current project.",
                    Tag = _viewModelsContainer.Resolve<ProductBacklogViewModel>()
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.Timer},
                    Label = "Sprint",
                    ToolTip = "Project requirements for current period of time.",
                    Tag = _viewModelsContainer.Resolve<SprintViewModel>()
                }
                ,
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.ChartLine},
                    Label = "Project Review",
                    ToolTip = "Statistical data about current project.",
                    Tag = _viewModelsContainer.Resolve<ReviewViewModel>()
                }
            };

                MenuOptionItems = new HamburgerMenuItemCollection
                {
                    new HamburgerMenuIconItem()
                    {
                        Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.Logout},
                        Label = "Log out",
                        ToolTip = "Log out",
                        Tag =_viewModelsContainer.Resolve<Logout.LogoutViewModel>()
                    }
                };
            });
        }

        private void OnTeamSelected(object sender, Team team)
        {
            if(team != null)
            {
                _mainShellViewModel.CurrentTeam = team;
                CurrentViewModel = _viewModelsContainer.Resolve<ProjectsViewModel>();
                ComponentsContainer.Get<IRegistryService>().SetCurrentTeamId(team.Id);
            }
        }

        private void OnProjectSelected(object sender, Project project)
        {
            if(project != null)
            {
                CurrentProject = project;
                CurrentViewModel = _viewModelsContainer.Resolve<ProductBacklogViewModel>();
                ComponentsContainer.Get<IRegistryService>().SetCurrentProjectId(project.Id);
            }
        }

        public void SetCurrentViewModel(Type viewModelType)
        {
            //if(!(viewModelType is ViewModelBase))
            if (!viewModelType.IsSubclassOf(typeof(ViewModelBase)))
            {
                return;
            }

            object instance = null;

            // Get instance by type.
            for (int i = 0; i < MenuItems.Count; i++)
            {
                object itemTag = MenuItems.ElementAt(i).Tag;

                if (viewModelType == itemTag.GetType())
                {
                    instance = itemTag;
                    break;
                }
            }

            if(instance != null)
            {
                CurrentViewModel = instance as ViewModelBase;
            }
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
                _dispatcher.Invoke(delegate
                {
                    for (int i = 0; i < MenuItems.Count; i++)
                    {
                        object itemTag = MenuItems.ElementAt(i).Tag;

                        if (itemTag.GetType() == value.GetType())
                        {
                            if(SelectedMenuIndex != i)
                            {
                                SelectedMenuIndex = i;
                                break;
                            }
                        }
                    }
                });

                SetProperty(ref _currentViewModel, value);
            }
        }

        public HamburgerMenuItemCollection MenuItems
        {
            get { return _menuItems; }
            set
            {
                if (Equals(value, _menuItems)) return;
                _menuItems = value;
                OnPropertyChanged();
            }
        }

        public HamburgerMenuItemCollection MenuOptionItems
        {
            get { return _menuOptionItems; }
            set
            {
                if (Equals(value, _menuOptionItems)) return;
                _menuOptionItems = value;
                OnPropertyChanged();
            }
        }

        public int SelectedMenuIndex
        {
            get => _selectedMenuIndex;

            set
            {
                SetProperty(ref _selectedMenuIndex, value);

                HamburgerMenuItem item = MenuItems[value];

                if (item.Tag is ViewModelBase)
                {
                    if (item.Tag != CurrentViewModel)
                    {
                        CurrentViewModel = item.Tag as ViewModelBase;
                    }
                }
            }
        }

        public User CurrentUser
        {
            get
            {
                if(_mainShellViewModel != null)
                {
                    return _mainShellViewModel.CurrentUser;
                }
                else
                {
                    throw new Exception($"MainShellViewModel is null ({this.GetType().Name})");
                }
            }
        }

        public Team CurrentTeam
        {
            get
            {
                if (_mainShellViewModel != null)
                {
                    return _mainShellViewModel.CurrentTeam;
                }
                else
                {
                    throw new Exception($"MainShellViewModel is null ({GetType().Name})");
                }
            }
        }

        public Project CurrentProject
        {
            get
            {
                if (_mainShellViewModel != null)
                {
                    return _mainShellViewModel.CurrentProject;
                }
                else
                {
                    throw new Exception($"MainShellViewModel is null ({GetType().Name})");
                }
            }

            set
            {
                string CurrentProjectPropertyName = GetPropertyName<MenuBasedShellViewModel>(item => item.CurrentProject);

                if(_mainShellViewModel == null)
                {
                    return;
                }

                _mainShellViewModel.CurrentProject = value;
                OnPropertyChanged(CurrentProjectPropertyName);
            }
        }
        #endregion
    }
}
