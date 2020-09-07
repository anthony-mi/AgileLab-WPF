using AgileLab.Data;
using AgileLab.Data.Entities;
using AgileLab.Services.Logging;
using AgileLab.Views.Dialogs.MessageBox;
using AgileLab.Views.MenuBasedShell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AgileLab.Views.Review
{
    class ReviewViewModel : ViewModelBase
    {
        #region Fields
        private MenuBasedShellViewModel _menuBasedShellViewModel = null;

        private IStoriesDataModel _storiesDataModel = ComponentsContainer.Get<IStoriesDataModel>();
        private IStoryStatusesDataModel _storyStatusesDataModel = ComponentsContainer.Get<IStoryStatusesDataModel>();
        private ISprintsDataModel _sprintsDataModel = ComponentsContainer.Get<ISprintsDataModel>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();

        private Project _currentProject = null;
        private List<Data.Entities.Sprint> _allProjectSprints = null;
        private List<Story> _allProjectStories = null;

        private ViewModelBase _currentDialogViewModel = null;
        private MessageBoxViewModel _messageBoxViewModel = null;

        private bool _showDialog = false;

        private static readonly string _inProgressStatusName = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(StoryStatus.InProgress);
        private static readonly string _completedStatusName = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(StoryStatus.Completed);

        private string _projectName = string.Empty;
        private string _startedAtDate = string.Empty;
        private uint _totalSprints = 0;
        private uint _pointsOnOneSprint = 0;
        private uint _pointsDone = 0;
        private uint _pointsOpen = 0;
        #endregion

        #region Constructors
        internal ReviewViewModel(MenuBasedShellViewModel menuBasedShellViewModel)
        {
            _menuBasedShellViewModel = menuBasedShellViewModel;
            _menuBasedShellViewModel.PropertyChanged += MenuBasedShellViewModelPropertyChangedEventHandler;

            InitializeFields();

            SetDialog += delegate (object sender, ViewModelBase dialogViewModel)
            {
                CurrentDialogViewModel = dialogViewModel;
                ShowDialog = true;
            };

            HideDialog += delegate
            {
                ShowDialog = false;
            };

            SubscribeToDataModelEvents();
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

        public MessageBoxViewModel MessageBoxViewModel
        {
            get => _messageBoxViewModel;
            private set => SetProperty(ref _messageBoxViewModel, value);
        }

        public string ProjectName
        {
            get => _projectName;
            private set => SetProperty(ref _projectName, value);
        }

        public string StartedAtDate
        {
            get => _startedAtDate;
            private set => SetProperty(ref _startedAtDate, value);
    }

        public uint TotalSprints
        {
            get => _totalSprints;
            private set => SetProperty(ref _totalSprints, value);
        }

        public uint PointsOnOneSprint
        {
            get => _pointsOnOneSprint;
            private set => SetProperty(ref _pointsOnOneSprint, value);
        }

        public uint PointsDone
        {
            get => _pointsDone;
            private set => SetProperty(ref _pointsDone, value);
        }

        public uint PointsOpen
        {
            get => _pointsOpen;
            private set => SetProperty(ref _pointsOpen, value);
        }
        #endregion

        #region Methods
        private void SubscribeToDataModelEvents()
        {
            _storiesDataModel.StoryUpdated += StoryUpdatedEventHandler;
            _storiesDataModel.StoryRemoved += StoryRemovedEventHandler;
            _sprintsDataModel.NewSprintCreated += NewSprintCreatedEventHandler;
        }

        private void InitializeFields()
        {
            if (_menuBasedShellViewModel.CurrentProject == null)
            {
                MessageBoxViewModel = new MessageBoxViewModel();
                MessageBoxViewModel.Text = "Select project!";
                MessageBoxViewModel.Confirmed += delegate
                {
                    _menuBasedShellViewModel.SetCurrentViewModel(typeof(Projects.ProjectsViewModel));
                };
                SetDialog?.Invoke(this, MessageBoxViewModel);
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    _currentProject = _menuBasedShellViewModel.CurrentProject;

                    ProjectName = _currentProject.Name;

                    _allProjectSprints = (List<Data.Entities.Sprint>)ComponentsContainer.Get<ISprintsDataModel>().GetAllProjectSprints(_currentProject.Id);
                    _allProjectStories = new List<Story>();

                    TotalSprints = Convert.ToUInt32(_allProjectSprints.Count);

                    PointsDone = 0;
                    PointsOpen = 0;

                    DateTime minimalSprintsStartingDate = DateTime.Now;

                    if(_allProjectSprints.Count > 0)
                    {
                        minimalSprintsStartingDate = _allProjectSprints.First().StartDate;
                    }

                    foreach (Data.Entities.Sprint sprint in _allProjectSprints)
                    {
                        List<Data.Entities.Story> stories = (List<Data.Entities.Story>)ComponentsContainer.Get<IStoriesDataModel>().GetStoriesByBacklogId(sprint.BacklogId);

                        foreach (Data.Entities.Story story in stories)
                        {
                            if (story.Status == _inProgressStatusName)
                            {
                                PointsOpen += story.InitialEstimate;
                            }
                            else if (story.Status == _completedStatusName)
                            {
                                PointsDone += story.InitialEstimate;
                            }
                        }

                        _allProjectStories.AddRange(stories);

                        if (minimalSprintsStartingDate > sprint.StartDate)
                        {
                            minimalSprintsStartingDate = sprint.StartDate;
                        }
                    }

                    PointsOnOneSprint = PointsDone / TotalSprints;

                    if (_allProjectSprints.Count > 0)
                    {
                        StartedAtDate = minimalSprintsStartingDate.ToShortDateString();
                    }
                }
                catch
                {
                    StartedAtDate = string.Empty;
                    TotalSprints = 0;
                    PointsOnOneSprint = 0;
                    PointsDone = 0;
                    PointsOpen = 0;
                }

                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
        }

        private void ShowMessage(string text)
        {
            MessageBoxViewModel = new MessageBoxViewModel();
            MessageBoxViewModel.Text = text;
            MessageBoxViewModel.Confirmed += delegate { HideDialog?.Invoke(this, null); };
            SetDialog?.Invoke(this, MessageBoxViewModel);
        }
        #endregion

        #region "Event Handlers"
        private void NewSprintCreatedEventHandler(object sender, Data.Entities.Sprint sprint)
        {
            if(sprint == null || _currentProject == null)
            {
                return;
            }

            if(sprint.ProjectId == _currentProject.Id)
            {
                _allProjectSprints.Add(sprint);
                TotalSprints++;
            }
        }

        private void StoryUpdatedEventHandler(object sender, Story story)
        {
            if (story == null)
            {
                return;
            }

            InitializeFields(); // Just recalculate statistic.
        }

        private Data.Entities.Sprint GetStorySprint(Story story)
        {
            return ComponentsContainer.Get<ISprintsDataModel>().GetSprintByBacklogId(story.BacklogId);
        }

        private void StoryRemovedEventHandler(object sender, Story story)
        {
            if (story == null)
            {
                return;
            }

            InitializeFields(); // Just recalculate statistic.
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

        private void MenuBasedShellViewModelPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            string CurrentProjectPropertyName = GetPropertyName<MenuBasedShellViewModel>(item => item.CurrentProject);
            string CurrentViewModelPropertyName = GetPropertyName<MenuBasedShellViewModel>(item => item.CurrentViewModel);

            if (e.PropertyName.Equals(CurrentProjectPropertyName)) // Can't use switch-case statement because property names are not constants.
            {
                CurrentProjectChangedEventHandler();
            }
            else if (e.PropertyName.Equals(CurrentViewModelPropertyName))
            {
                CurrentViewModelChangedEventHandler();
            }
        }

        private void CurrentProjectChangedEventHandler()
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    if (_menuBasedShellViewModel.CurrentViewModel.Equals(this))
                    {
                        InitializeFields();
                    }
                });
        }

        private void CurrentViewModelChangedEventHandler()
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    if (_menuBasedShellViewModel.CurrentViewModel.Equals(this))
                    {
                        InitializeFields();
                    }
                });
        }
        #endregion

        #region Events
        public event EventHandler<ViewModelBase> SetDialog;
        public event EventHandler HideDialog;
        #endregion
    }
}
