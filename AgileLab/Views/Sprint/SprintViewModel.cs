using AgileLab.Data;
using AgileLab.Data.Entities;
using AgileLab.Services.Logging;
using AgileLab.Views.Dialogs.ConfirmationDialog;
using AgileLab.Views.Dialogs.MessageBox;
using AgileLab.Views.Dialogs.Story;
using AgileLab.Views.MenuBasedShell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AgileLab.Views.Sprint
{
    class SprintViewModel : ViewModelBase
    {
        #region Fields
        private MenuBasedShellViewModel _menuBasedShellViewModel = null;

        private ISprintsDataModel _sprintsDataModel = ComponentsContainer.Get<ISprintsDataModel>();
        private IStoriesDataModel _storiesDataModel = ComponentsContainer.Get<IStoriesDataModel>();
        private IStoryStatusesDataModel _storyStatusesDataModel = ComponentsContainer.Get<IStoryStatusesDataModel>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();

        private Data.Entities.Sprint _currentSprint = null;
        private ObservableCollection<StoryItem> _allUserStories = null;

        private ViewModelBase _currentDialogViewModel = null;
        private ConfirmationDialogViewModel _confirmationDialogViewModel = null;
        private MessageBoxViewModel _messageBoxViewModel = null;
        private SprintDialogViewModel _sprintDialogViewModel = null;
        private StoryDialogViewModel _storyDialogViewModel = null;

        private bool _showDialog = false;

        private ICommand _requestStoryEditing = null;
        private ICommand _requestStoryRemoving = null;
        //private Action _removeStoryAction = null; // After confirmation.
        private StoryItem _selectedStory = null;

        private string _mainGoal = string.Empty;

        private string _sprintStartDate = string.Empty;
        private string _sprintFinishDate = string.Empty;

        private int _progressMinimumValue = default(int);
        private int _progressMaximumValue = default(int);
        private int _progressValue = default(int);
        #endregion

        #region Constructors
        internal SprintViewModel(MenuBasedShellViewModel menuBasedShellViewModel)
        {
            _menuBasedShellViewModel = menuBasedShellViewModel;
            _menuBasedShellViewModel.PropertyChanged += MenuBasedShellViewModelPropertyChangedEventHandler;
            InitializeCommands();
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

        #region Methods
        private void InitializeFields()
        {
            Project currentProject = _menuBasedShellViewModel.CurrentProject;

            if (currentProject == null)
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

            Data.Entities.Sprint currentSprint = _sprintsDataModel.GetSprintByProjectId(_menuBasedShellViewModel.CurrentProject.Id);

            if (currentSprint == null)
            {
                SprintDialogViewModel = new SprintDialogViewModel();
                SprintDialogViewModel.ConfirmSelected += delegate
                {
                    CreateSprintFromDialog(); // Here sets value of '_currentSprint'.
                    HideDialog?.Invoke(this, null);
                };
                SprintDialogViewModel.CancelSelected += delegate { /*HideDialog?.Invoke(this, null);*/ };
                SetDialog?.Invoke(this, SprintDialogViewModel);
            }
            else
            {
                if(_currentSprint != null)
                {
                    if (!_currentSprint.Equals(currentSprint))
                    {
                        _currentSprint = currentSprint;
                        Backlog currentSprintBacklog = ComponentsContainer.Get<IBacklogsDataModel>().GetBacklogById(currentSprint.BacklogId);
                        List<StoryItem> items = ConvertToStoryItems(currentSprintBacklog.Stories);
                        AllUserStories = new ObservableCollection<StoryItem>(items);
                        InitializeSprintDependentFields();
                    }
                }
                else
                {
                    _currentSprint = currentSprint;
                    Backlog currentSprintBacklog = ComponentsContainer.Get<IBacklogsDataModel>().GetBacklogById(currentSprint.BacklogId);
                    List<StoryItem> items = ConvertToStoryItems(currentSprintBacklog.Stories);
                    AllUserStories = new ObservableCollection<StoryItem>(items);
                    InitializeSprintDependentFields();
                }
            }
        }

        private void InitializeCommands()
        {
            _requestStoryEditing = new Command(
                            parameterizedAction:
                                delegate (object param)
                                {
                                    StoryDialogViewModel = new StoryDialogViewModel();
                                    StoryDialogViewModel.IsStatusEditable = true;
                                    StoryDialogViewModel.ConfirmSelected += EditSelectedStory;
                                    StoryDialogViewModel.CancelSelected += delegate { HideDialog?.Invoke(this, null); };
                                    FillStoryDialog(param);
                                    SetDialog?.Invoke(this, StoryDialogViewModel);
                                },
                            canExecute: null);

            _requestStoryRemoving = new Command(
                parameterizedAction:
                        delegate (object param)
                        {
                            ConfirmationDialogViewModel = new ConfirmationDialogViewModel();
                            ConfirmationDialogViewModel.Text = "Are you sure you want to delete the user story?";
                            ConfirmationDialogViewModel.ConfirmSelected += RemoveSelectedStory;
                            ConfirmationDialogViewModel.CancelSelected += delegate { HideDialog?.Invoke(this, null); };
                            SetDialog?.Invoke(this, ConfirmationDialogViewModel);
                        },
                    canExecute: null);
        }

        private void InitializeSprintDependentFields()
        {
            if(_currentSprint == null)
            {
                MainGoal = string.Empty;
                SprintStartDate = string.Empty;
                SprintFinishDate = string.Empty;
                ProgressMinimumValue = 0;
                ProgressMaximumValue = 1;
                ProgressValue = 1;
            }
            else
            {
                MainGoal = _currentSprint.MainGoal;
                SprintStartDate = _currentSprint.StartDate.ToShortDateString();
                SprintFinishDate = _currentSprint.FinishDate.ToShortDateString();
                ProgressMinimumValue = 0;
                ProgressMaximumValue = (int)(_currentSprint.FinishDate - _currentSprint.StartDate).TotalDays;
                //ProgressValue = ProgressMaximumValue - (int)(_currentSprint.FinishDate - DateTime.Now).TotalDays;
                ProgressValue = ProgressMaximumValue - (int)Math.Ceiling((_currentSprint.FinishDate - DateTime.Now).TotalDays);
            }
        }

        private void SubscribeToDataModelEvents()
        {
            _storiesDataModel.StoryUpdated += StoryUpdatedEventHandler;
            _storiesDataModel.StoryRemoved += StoryRemovedEventHandler;
        }

        private void FillStoryDialog(object param)
        {
            if (param is StoryItem && StoryDialogViewModel != null)
            {
                StoryItem item = param as StoryItem;

                StoryDialogViewModel.Name = item.Name;
                StoryDialogViewModel.Importance = item.Importance;
                StoryDialogViewModel.InitialEstimate = item.InitialEstimate;
                StoryDialogViewModel.Status = item.Status;
                StoryDialogViewModel.HowToDemo = item.HowToDemo;
                StoryDialogViewModel.Notes = item.Notes;
            }
        }

        private void EditSelectedStory(object sender, EventArgs e)
        {
            EditSelectedStory();
        }

        private void EditSelectedStory()
        {
            if(StoryDialogViewModel == null)
            {
                return;
            }

            if (!StoryDialogViewModel.CanExecuteConfirmCommand(null))
            {
                return;
            }

            if(SelectedStory == null)
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    StoryItem newStoryItem = SelectedStory.Clone() as StoryItem;

                    newStoryItem.Name = StoryDialogViewModel.Name;
                    newStoryItem.Importance = StoryDialogViewModel.Importance;
                    newStoryItem.InitialEstimate = StoryDialogViewModel.InitialEstimate;
                    newStoryItem.Status = StoryDialogViewModel.Status;
                    newStoryItem.HowToDemo = StoryDialogViewModel.HowToDemo;
                    newStoryItem.Notes = StoryDialogViewModel.Notes;

                    string previousStatus = SelectedStory.Status;
                    string newStatus = newStoryItem.Status;

                    if(previousStatus != newStatus) // status was changed
                    {
                        if(previousStatus == _storyStatusesDataModel.GetStatusText(StoryStatus.WaitingForExecutor)) // it was "WaitingForExecutor" status
                        {
                            newStoryItem.ExecutorId = _menuBasedShellViewModel.CurrentUser.Id;
                        }
                        else if(previousStatus == _storyStatusesDataModel.GetStatusText(StoryStatus.InProgress)) // it was "InProgress" status
                        {
                            if(newStoryItem.ExecutorId != _menuBasedShellViewModel.CurrentUser.Id)
                            {
                                StoryDialogViewModel.ErrorMessage = "You can't change the task executing by another user.";
                                StoryDialogViewModel.ShowErrorMessage = true;
                                _menuBasedShellViewModel.IsLoadingData = false;
                                return;
                            }
                            else
                            {
                                newStoryItem.ExecutorId = (newStatus == _storyStatusesDataModel.GetStatusText(StoryStatus.WaitingForExecutor)) ? 0 : _menuBasedShellViewModel.CurrentUser.Id;
                            }
                        }
                        else if (previousStatus == _storyStatusesDataModel.GetStatusText(StoryStatus.Completed)) // it was "Completed" status
                        {
                            if (newStoryItem.ExecutorId != _menuBasedShellViewModel.CurrentUser.Id)
                            {
                                StoryDialogViewModel.ErrorMessage = "You can't change the task executed by another user.";
                                StoryDialogViewModel.ShowErrorMessage = true;
                                _menuBasedShellViewModel.IsLoadingData = false;
                                return;
                            }
                            else
                            {
                                newStoryItem.ExecutorId = (newStatus == _storyStatusesDataModel.GetStatusText(StoryStatus.WaitingForExecutor)) ? 0 : _menuBasedShellViewModel.CurrentUser.Id;
                            }
                        }
                    }

                    if (SelectedStory.Equals(newStoryItem))
                    {
                        StoryDialogViewModel.ErrorMessage = "Story data is not changed";
                        StoryDialogViewModel.ShowErrorMessage = true;
                        _menuBasedShellViewModel.IsLoadingData = false;
                        return;
                    }

                    List<StoryItem> storyItemsWithTheSameName = AllUserStories.Where(si => si.Name == newStoryItem.Name).ToList();

                    if (storyItemsWithTheSameName.Count > 0)
                    {
                        if(storyItemsWithTheSameName.First().Id != newStoryItem.Id)
                        {
                            StoryDialogViewModel.ErrorMessage = $"Story with name '{newStoryItem.Name}' already exists.";
                            StoryDialogViewModel.ShowErrorMessage = true;
                            _menuBasedShellViewModel.IsLoadingData = false;
                            return;
                        }
                        
                    }

                    _logger.Debug($"Started editing story with id '{SelectedStory.Id}'.");

                    _storiesDataModel.Update(newStoryItem.Story);

                    StoryItem item = AllUserStories.FirstOrDefault(s => s.Id == newStoryItem.Id);

                    if (item != null)
                    {
                        ComponentsContainer.Get<Dispatcher>().Invoke(
                            delegate
                            {
                                AllUserStories.Remove(item);
                                newStoryItem.StoryUpdated += OnStoryItemUpdated;
                                AllUserStories.Add(newStoryItem);
                            });
                    }

                    HideDialog?.Invoke(this, null);
                }
                catch (Exception ex)
                {
                    ShowMessage($"User story doesn't edited: technical issues encountered.");
                    _logger.Fatal(ex);
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

        private void ShowMessage(string text, EventHandler confirmEventHandler)
        {
            MessageBoxViewModel = new MessageBoxViewModel();
            MessageBoxViewModel.Text = text;
            MessageBoxViewModel.Confirmed += confirmEventHandler;
            SetDialog?.Invoke(this, MessageBoxViewModel);
        }

        private void OnStoryItemUpdated(object sender, StoryItem item)
        {
            if (item != null)
            {
                ComponentsContainer.Get<Dispatcher>().Invoke(
                                            delegate
                                            {
                                                OnPropertyChanged("WaitingForExecutorStories");
                                                OnPropertyChanged("InProgressStories");
                                                OnPropertyChanged("CompletedStories");

                                                //AllUserStories.Remove(item); // It will throw exception in view.
                                                //AllUserStories.Add(item);
                                            });
            }
        }

        private void RemoveSelectedStory(object sender, EventArgs e)
        {
            RemoveSelectedStory();
        }

        private void RemoveSelectedStory()
        {
            if(SelectedStory == null)
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    _storiesDataModel.Remove(SelectedStory.Story);

                    ComponentsContainer.Get<Dispatcher>().Invoke(
                            delegate
                            {
                                AllUserStories.Remove(SelectedStory);
                            });

                    HideDialog?.Invoke(this, null);
                }
                catch (Exception ex)
                {
                    ShowMessage($"User story doesn't removed: technical issues encountered.");
                    _logger.Fatal(ex);
                }

                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
        }

        private List<StoryItem> ConvertToStoryItems(List<Story> stories)
        {
            List<StoryItem> items = new List<StoryItem>();

            if(stories != null)
            {
                foreach(Story story in stories)
                {
                    StoryItem newItem = ConvertToStoryItem(story);
                    items.Add(newItem);
                }
            }

            return items;
        }

        private StoryItem ConvertToStoryItem(Story story)
        {
            StoryItem item = null;

            if(story != null)
            {
                item = new StoryItem(story, _requestStoryEditing, _requestStoryRemoving, SetUserAsExecutor, _menuBasedShellViewModel.CurrentUser.Id);
                item.StoryUpdated += OnStoryItemUpdated;
            }

            return item;
        }

        private void SetUserAsExecutor(StoryItem obj)
        {
            if(SelectedStory == null)
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    Story story = SelectedStory.Story;
                    SelectedStory.ExecutorId = _menuBasedShellViewModel.CurrentUser.Id;
                    SetStatus(ref story, StoryStatus.InProgress);

                    _logger.Debug($"Started editing story with id '{SelectedStory.Id}'.");

                    _storiesDataModel.Update(story);

                    SelectedStory.Story = story;

                    ComponentsContainer.Get<Dispatcher>().Invoke(
                            delegate
                            {
                                OnPropertyChanged("WaitingForExecutorStories");
                                OnPropertyChanged("InProgressStories");
                                OnPropertyChanged("CompletedStories");

                                //AllUserStories.Remove(SelectedStory); // It will throw exception in view.
                                //AllUserStories.Add(SelectedStory);
                            });
                }
                catch (Exception ex)
                {
                    ShowMessage($"User story doesn't edited: technical issues encountered.");
                    _logger.Fatal(ex);
                }

                _menuBasedShellViewModel.IsLoadingData = false;
                HideDialog?.Invoke(this, null);
            }).Start();
        }

        private static void SetStatus(ref Story story, StoryStatus storyStatus)
        {
            story.Status = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(storyStatus);
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

        public MessageBoxViewModel MessageBoxViewModel
        {
            get => _messageBoxViewModel;
            private set => SetProperty(ref _messageBoxViewModel, value);
        }

        public SprintDialogViewModel SprintDialogViewModel
        {
            get => _sprintDialogViewModel;
            private set => SetProperty(ref _sprintDialogViewModel, value);
        }

        public StoryDialogViewModel StoryDialogViewModel
        {
            get => _storyDialogViewModel;
            private set => SetProperty(ref _storyDialogViewModel, value);
        }

        public ObservableCollection<StoryItem> AllUserStories
        {
            get
            {
                if(_allUserStories == null)
                {
                    _allUserStories = new ObservableCollection<StoryItem>();
                }

                return _allUserStories;
            }

            private set
            {
                SetProperty(ref _allUserStories, value);

                _allUserStories.CollectionChanged += delegate
                {
                    // Dependent properties.
                    OnPropertyChanged("WaitingForExecutorStories");
                    OnPropertyChanged("InProgressStories");
                    OnPropertyChanged("CompletedStories");
                };

                // Dependent properties.
                OnPropertyChanged("WaitingForExecutorStories");
                OnPropertyChanged("InProgressStories");
                OnPropertyChanged("CompletedStories");
            }
        }

        public ObservableCollection<StoryItem> WaitingForExecutorStories
        {
            get
            {
                string waitingForExecutorStatusName = _storyStatusesDataModel.GetStatusText(StoryStatus.WaitingForExecutor);
                return  new ObservableCollection<StoryItem>(AllUserStories.Where(item => item.Status == waitingForExecutorStatusName));
            }

            private set => OnPropertyChanged("WaitingForExecutorStories");
        }

        public ObservableCollection<StoryItem> InProgressStories
        {
            get
            {
                string inProgressStatusName = _storyStatusesDataModel.GetStatusText(StoryStatus.InProgress);
                return  new ObservableCollection<StoryItem>(AllUserStories.Where(item => item.Status == inProgressStatusName));
            }

            private set => OnPropertyChanged("InProgressStories");
        }

        public ObservableCollection<StoryItem> CompletedStories
        {
            get
            {
                string completedStatusName = _storyStatusesDataModel.GetStatusText(StoryStatus.Completed);
                return new ObservableCollection<StoryItem>(AllUserStories.Where(item => item.Status == completedStatusName));
            }
            private set => OnPropertyChanged("CompletedStories");
        }

        public StoryItem SelectedStory
        {
            get => _selectedStory;
            set => SetProperty(ref _selectedStory, value);
        }

        public ICommand RequestUserStoryCreation => new Command(
            new Action(
                delegate
                {
                    StoryDialogViewModel = new StoryDialogViewModel();
                    StoryDialogViewModel.IsStatusEditable = false;
                    StoryDialogViewModel.Status = _storyStatusesDataModel.GetStatusText(StoryStatus.WaitingForExecutor);
                    StoryDialogViewModel.ConfirmSelected += CreateStoryFromDialog;
                    StoryDialogViewModel.CancelSelected += 
                        delegate 
                        {
                            HideDialog?.Invoke(this, null);
                        };
                    SetDialog?.Invoke(this, StoryDialogViewModel);
                }),
            null);

        public ICommand FinishCommand => new Command(
            new Action(
                delegate
                {
                    ConfirmationDialogViewModel = new ConfirmationDialogViewModel();
                    ConfirmationDialogViewModel.Text = "Do you really want to finish the sprint now?";
                    ConfirmationDialogViewModel.ConfirmSelected += FinishCurrentSprint;
                    ConfirmationDialogViewModel.CancelSelected +=
                        delegate
                        {
                            HideDialog?.Invoke(this, null);
                        };
                    SetDialog?.Invoke(this, ConfirmationDialogViewModel);
                }),
            null);

        public string MainGoal
        {
            get => _mainGoal;
            private set => SetProperty(ref _mainGoal, value);
        }

        public string SprintStartDate
        {
            get => _sprintStartDate;
            private set => SetProperty(ref _sprintStartDate, value);
        }

        public string SprintFinishDate
        {
            get => _sprintFinishDate;
            private set => SetProperty(ref _sprintFinishDate, value);
        }

        public int ProgressMinimumValue
        {
            get => _progressMinimumValue;
            private set => SetProperty(ref _progressMinimumValue, value);
        }

        public int ProgressMaximumValue
        {
            get => _progressMaximumValue;
            private set => SetProperty(ref _progressMaximumValue, value);
        }

        public int ProgressValue
        {
            get => _progressValue;
            private set => SetProperty(ref _progressValue, value);
        }
        #endregion

        #region Methods
        private void FinishCurrentSprint(object sender, EventArgs e)
        {
            if(_currentSprint == null)
            {
                return;
            }

            try
            {
                _sprintsDataModel.Finish(_currentSprint);
                _currentSprint = null;
                InitializeSprintDependentFields();
                ShowMessage("Sprint successfully finished!",
                    delegate
                    {
                        SprintDialogViewModel = new SprintDialogViewModel();
                        SprintDialogViewModel.ConfirmSelected += delegate
                        {
                            CreateSprintFromDialog(); // Here sets value of '_currentSprint'.
                            HideDialog?.Invoke(this, null);
                        };
                        SprintDialogViewModel.CancelSelected += delegate { /*HideDialog?.Invoke(this, null);*/ };
                        SetDialog?.Invoke(this, SprintDialogViewModel);
                    });
            }
            catch(Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        private void CreateStoryFromDialog(object sender, EventArgs e)
        {
            CreateStoryFromDialog();
        }

        private void CreateStoryFromDialog()
        {
            if (StoryDialogViewModel != null)
            {
                _menuBasedShellViewModel.IsLoadingData = true;

                new Task(() =>
                {
                    try
                    {
                        if (StoryDialogViewModel.CanExecuteConfirmCommand(null))
                        {
                            _logger.Debug("Started user story creation.");

                            string name = StoryDialogViewModel.Name;
                            uint importance = StoryDialogViewModel.Importance;
                            uint initialEstimate = StoryDialogViewModel.InitialEstimate;
                            string howToDemo = StoryDialogViewModel.HowToDemo;
                            string notes = StoryDialogViewModel.Notes;

                            List<StoryItem> storyItemsWithTheSameName = AllUserStories.Where(item => item.Name == name).ToList();

                            if(storyItemsWithTheSameName.Count > 0)
                            {
                                StoryDialogViewModel.ErrorMessage = $"Story with name '{name}' already exists.";
                                StoryDialogViewModel.ShowErrorMessage = true;
                                _menuBasedShellViewModel.IsLoadingData = false;
                                return;
                            }

                            Story newStory = _storiesDataModel.CreateNewStory(name, importance, initialEstimate, howToDemo, notes, _currentSprint.BacklogId);

                            ComponentsContainer.Get<Dispatcher>().Invoke(
                                delegate
                                {
                                    AllUserStories.Add(ConvertToStoryItem(newStory));
                                });

                            HideDialog?.Invoke(this, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("User story doesn't created: technical issues encountered.");
                        _logger.Fatal(ex);
                    }

                    _menuBasedShellViewModel.IsLoadingData = false;
                }).Start();
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

        private void CreateSprintFromDialog()
        {
            if(SprintDialogViewModel != null)
            {
                _menuBasedShellViewModel.IsLoadingData = true;

                new Task(() =>
                {
                    try
                    {
                        if (SprintDialogViewModel.CanExecuteConfirmCommand(null))
                        {
                            string mainGoal = SprintDialogViewModel.MainGoal;
                            DateTime startDate = SprintDialogViewModel.StartDate;
                            DateTime finishDate = SprintDialogViewModel.FinishDate;

                            _currentSprint = _sprintsDataModel.CreateNewSprint(mainGoal, startDate, finishDate, _menuBasedShellViewModel.CurrentProject.Id);
                            AllUserStories = new ObservableCollection<StoryItem>();
                            InitializeSprintDependentFields();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex);
                    }

                    _menuBasedShellViewModel.IsLoadingData = false;
                }).Start();
            }
        }
        #endregion

        #region "Event Handlers"
        private void StoryUpdatedEventHandler(object sender, Story story)
        {
            if (story == null)
            {
                return;
            }

            StoryItem[] itemsWithSameId = AllUserStories.Where(item => item.Id == story.Id).ToArray();

            if (itemsWithSameId.Count() == 0)
            {
                return;
            }

            StoryItem previousItemState = itemsWithSameId.First();
            StoryItem newItemState = ConvertToStoryItem(story);

            string previousStatusValue = previousItemState.Status;

            previousItemState.HowToDemo = newItemState.HowToDemo;
            previousItemState.Importance = newItemState.Importance;
            previousItemState.InitialEstimate = newItemState.InitialEstimate;
            previousItemState.Name = newItemState.Name;
            previousItemState.Notes = newItemState.Notes;
            previousItemState.Status = newItemState.Status;

            if (previousStatusValue != newItemState.Status) // status was changed
            {
                OnPropertyChanged("WaitingForExecutorStories");
                OnPropertyChanged("InProgressStories");
                OnPropertyChanged("CompletedStories");
            }
        }

        private void StoryRemovedEventHandler(object sender, Story story)
        {
            if (story == null)
            {
                return;
            }

            StoryItem[] itemsWithSameId = AllUserStories.Where(item => item.Id == story.Id).ToArray();

            if (itemsWithSameId.Count() > 0)
            {
                ComponentsContainer.Get<Dispatcher>().Invoke(
                        delegate
                        {
                            AllUserStories.Remove(itemsWithSameId.First());
                        });
            }
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
            InitializeFields();
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

        public class StoryItem : INotifyPropertyChanged, ICloneable
        {
            private Story _story = null;

            private ICommand _editCommand = null;
            private ICommand _removeCommand = null;
            private ICommand _takeforExecutionCommand = null;
            private ICommand _finishCommand = null;

            private Action<StoryItem> _setUserAsExecutorAction = null;

            private uint _currentUserId = default(uint);

            public event EventHandler<StoryItem> StoryUpdated;

            public StoryItem(Story story, ICommand requestStoryEditing, ICommand requestStoryRemoving, Action<StoryItem> setUserAsExecutor, uint currentUserId)
            {
                _story = story;
                EditCommand = requestStoryEditing;
                RemoveCommand = requestStoryRemoving;
                _setUserAsExecutorAction = setUserAsExecutor;

                TakeForExecutionCommand = new Command(
                    new Action(
                        delegate
                    {
                        setUserAsExecutor(this);
                        //SetStatusInProgress(); // this perfomed in viewmodel
                    }),
                    CanSetUserAsExecutor);

                FinishCommand = new Command(SetStatusCompleted, CanExecuteSetStatusCompleted);

                _currentUserId = currentUserId;
            }

            private bool CanSetUserAsExecutor(object obj)
            {
                return _story.ExecutorId == default(uint);
            }

            private void SetStatusCompleted()
            {
                if (CanExecuteSetStatusCompleted(null))
                {
                    SetStatus(StoryStatus.Completed);
                    StoryUpdated?.Invoke(this, this);
                }
            }

            private void SetStatus(StoryStatus storyStatus)
            {
                Status = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(storyStatus);
                ComponentsContainer.Get<IStoriesDataModel>().Update(_story);
                StoryUpdated?.Invoke(this, this);
            }

            private bool CanExecuteSetStatusInProgres(object param)
            {
                string inProgressStatusName = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(StoryStatus.InProgress);
                return !Status.Equals(inProgressStatusName);
            }

            private bool CanExecuteSetStatusCompleted(object param)
            {
                bool canExecute = true;

                do
                {
                    string completedStatusName = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(StoryStatus.Completed);

                    if(Status.Equals(completedStatusName))
                    {
                        canExecute = false;
                        break;
                    }

                    if(ExecutorId != _currentUserId)
                    {
                        canExecute = false;
                        break;
                    }
                } while (false);

                return canExecute;
            }

            public Story Story
            {
                get => _story;

                set
                {
                    _story = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Story"));

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Importance"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InitialEstimate"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HowToDemo"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Notes"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutorId"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutorName"));
                }
            }

            public uint Id
            {
                get => _story.Id;
            }

            public string Name
            {
                get => _story.Name;

                set
                {
                    _story.Name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }

            public uint Importance
            {
                get => _story.Importance;

                set
                {
                    _story.Importance = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Importance"));
                }
            }

            public uint InitialEstimate
            {
                get => _story.InitialEstimate;

                set
                {
                    _story.InitialEstimate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InitialEstimate"));
                }
            }

            public string HowToDemo
            {
                get => _story.HowToDemo;

                set
                {
                    _story.HowToDemo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HowToDemo"));
                }
            }

            public string Status
            {
                get => _story.Status;

                set
                {
                    _story.Status = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
                }
            }

            public string Notes
            {
                get => _story.Notes;

                set
                {
                    _story.Notes = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Notes"));
                }
            }

            public string ExecutorName
            {
                get => _story.GetExecutorName();
            }

            public uint ExecutorId
            {
                get => _story.ExecutorId;

                set
                {
                    _story.ExecutorId = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutorId"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutorName"));
                }
            }

            public ICommand EditCommand
            {
                get => _editCommand;

                set
                {
                    _editCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditCommand"));
                }
            }

            public ICommand RemoveCommand
            {
                get => _removeCommand;

                set
                {
                    _removeCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RemoveCommand"));
                }
            }

            public ICommand TakeForExecutionCommand
            {
                get => _takeforExecutionCommand;

                set
                {
                    _takeforExecutionCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TakeForExecutionCommand"));
                }
            }

            public ICommand FinishCommand
            {
                get => _finishCommand;

                set
                {
                    _finishCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FinishCommand"));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public object Clone()
            {
                Story story = new Story(
                    Id,
                    Name.Clone() as string,
                    Importance,
                    InitialEstimate,
                    HowToDemo.Clone() as string,
                    Notes.Clone() as string,
                    Status.Clone() as string,
                    _story.ExecutorId,
                    _story.BacklogId);
                return new StoryItem(story, _editCommand, _removeCommand, _setUserAsExecutorAction, _currentUserId);
            }

            public override bool Equals(object obj)
            {
                bool equals = false;

                if (obj is StoryItem)
                {
                    StoryItem item = obj as StoryItem;

                    equals = item.Id == Id &&
                                item.Name == Name &&
                                item.ExecutorName == ExecutorName &&
                                item.HowToDemo == HowToDemo &&
                                item.Importance == Importance &&
                                item.InitialEstimate == InitialEstimate &&
                                item.Notes == Notes &&
                                item.Status == Status;
                }

                return equals;
            }
        } 
    }
}
