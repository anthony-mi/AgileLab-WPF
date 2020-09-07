using AgileLab.Data;
using AgileLab.Data.Entities;
using AgileLab.Services.Logging;
using AgileLab.Views.MenuBasedShell;
using AgileLab.Views.Projects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AgileLab.Views.ProductBacklog
{
    class ProductBacklogViewModel : ViewModelBase
    {
        #region Fields
        private IBacklogsDataModel _backlogsDataModel = ComponentsContainer.Get<IBacklogsDataModel>();
        private IStoriesDataModel _storiesDataModel = ComponentsContainer.Get<IStoriesDataModel>();
        private IStoryStatusesDataModel _storyStatusesDataModel = ComponentsContainer.Get<IStoryStatusesDataModel>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();

        private MenuBasedShellViewModel _menuBasedShellViewModel = null;

        private Backlog _backlog = null;
        private ObservableCollection<StoryItem> _backlogStories = new ObservableCollection<StoryItem>();

        private bool _showStoryDialog = false;
        private bool _showErrorMessageDialog = false;
        private bool _showRemoveStoryConfirmationDialog = false;
        private string _errorText = string.Empty;
        private bool _isNewStoryStatusEditable = false;

        private string _storyCreationErrorMessage = string.Empty;
        private bool _showStoryCreationErrorMessage = false;

        private string _newStoryName = string.Empty;
        private uint _newStoryImportance = 0;
        private string _newStoryNotes = string.Empty;
        private string _newStoryStatus = string.Empty;

        private StoryItem _storyForRemoving = null; // Temporary saves user story before removing confirmation.
        private StoryItem _storyForEditing = null; // Temporary saves user story before editing confirmation.

        private ObservableCollection<string> _storyStatuses = null;

        private ICommand _removeCommand = null;

        private ICommand _currentConfirmStoryCommand = null;
        private ICommand _editStoryCommand = null;
        private ICommand _createStoryCommand = null;

        private ICommand _requestStoryEditing = null;
        private ICommand _requestStoryRemoving = null;
        #endregion

        #region Constructors
        internal ProductBacklogViewModel(MenuBasedShellViewModel menuBasedShellViewModel)
        {
            _menuBasedShellViewModel = menuBasedShellViewModel;
            _menuBasedShellViewModel.PropertyChanged += MenuBasedShellViewModelPropertyChangedEventHandler;

            Statuses = new ObservableCollection<string>(_storyStatusesDataModel.GetAllStatuses());

            InitializeCommands();
            InitializeBacklog();
        }
        #endregion

        #region Commands
        public ICommand HideErrorMessageDialog => new Command(
            () =>
            {
                ShowErrorMessageDialog = false;

                if (_menuBasedShellViewModel.CurrentProject == null)
                {
                    _menuBasedShellViewModel.SetCurrentViewModel(typeof(ProjectsViewModel));
                }
            },
            null);

        public ICommand RequestUserStoryCreation => new Command(
            new Action(
                delegate
                {
                    if (_menuBasedShellViewModel.CurrentProject == null)
                    {
                        ShowErrorMessage("Select project!");
                    }
                    else
                    {
                        ClearStoryDialogFields();
                        ConfirmStoryCommand = _createStoryCommand; 
                        NewStoryStatus = _storyStatusesDataModel.GetStatusText(StoryStatus.WaitingForExecutor);
                        IsNewStoryStatusEditable = false;
                        ShowStoryDialog = true;
                    }
                }),
            null);

        public ICommand ConfirmStoryCommand
        {
            get
            {
                return _currentConfirmStoryCommand;
            }

            private set
            {
                SetProperty(ref _currentConfirmStoryCommand, value);
            }
        }

        public ICommand CancelStoryCreation => new Command(
            new Action(delegate { ShowStoryDialog = false; }),
            null);

        public ICommand CancelStoryRemoving => new Command(
            new Action(delegate { ShowRemoveStoryConfirmationDialog = false; }),
            null);

        public ICommand RemoveStoryCommand
        {
            get
            {
                return _removeCommand;
            }

            private set
            {
                SetProperty(ref _removeCommand, value);
            }
        }
        #endregion

        #region Properties
        public ObservableCollection<StoryItem> Stories
        {
            get
            {
                if (_backlogStories == null)
                {
                    InitializeBacklog();
                }

                return _backlogStories;
            }

            private set => SetProperty(ref _backlogStories, value);
        }

        public bool ShowStoryDialog
        {
            get => _showStoryDialog;
            set => SetProperty(ref _showStoryDialog, value);
        }

        public bool IsNewStoryStatusEditable
        {
            get => _isNewStoryStatusEditable;
            set => SetProperty(ref _isNewStoryStatusEditable, value);
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

        public bool ShowStoryCreationErrorMessage
        {
            get => _showStoryCreationErrorMessage;
            set => SetProperty(ref _showStoryCreationErrorMessage, value);
        }

        public bool ShowRemoveStoryConfirmationDialog
        {
            get => _showRemoveStoryConfirmationDialog;
            set => SetProperty(ref _showRemoveStoryConfirmationDialog, value);
        }

        public string StoryCreationErrorMessage
        {
            get
            {
                if (_storyCreationErrorMessage == null)
                {
                    _storyCreationErrorMessage = string.Empty;
                }

                return _storyCreationErrorMessage;
            }

            set => SetProperty(ref _storyCreationErrorMessage, value);
        }

        public string NewStoryName
        {
            get => _newStoryName;

            set
            {
                SetProperty(ref _newStoryName, value);
            }
        }

        public uint NewStoryImportance
        {
            get => _newStoryImportance;

            set
            {
                SetProperty(ref _newStoryImportance, value);
            }
        }


        public string NewStoryNotes
        {
            get => _newStoryNotes;

            set
            {
                SetProperty(ref _newStoryNotes, value);
            }
        }

        public string NewStoryStatus
        {
            get => _newStoryStatus;

            set
            {
                SetProperty(ref _newStoryStatus, value);
            }
        }

        public ObservableCollection<string> Statuses
        {
            get
            {
                if(_storyStatuses == null)
                {
                    _storyStatuses = new ObservableCollection<string>(_storyStatusesDataModel.GetAllStatuses());
                }

                return _storyStatuses;
            }

            set
            {
                SetProperty(ref _storyStatuses, value);
            }
        }
        #endregion

        #region Methods
        private void ClearStoryDialogFields()
        {
            NewStoryName = string.Empty;
            NewStoryImportance = CalculateActualImportance();
            NewStoryNotes = string.Empty;
            NewStoryStatus = _storyStatusesDataModel.GetStatusText(StoryStatus.WaitingForExecutor);
        }

        private uint CalculateActualImportance()
        {
            uint maxImportanceValue = 0;

            try
            {
                maxImportanceValue = Stories.Aggregate((s1, s2) => s1.Importance > s2.Importance ? s1 : s2).Importance;
            }
            catch { }

            return ++maxImportanceValue;
        }

        private void InitializeCommands()
        {
            RemoveStoryCommand = new Command(new Action(delegate { RemoveStory(_storyForRemoving); }), null);

            _editStoryCommand = new Command(parameterizedAction: EditStory, canExecute: CanEditStory);
            _createStoryCommand = new Command(CreateStory, CanCreateStory);

            _requestStoryEditing = new Command(
                            parameterizedAction:
                                delegate (object param)
                                {
                                    ConfirmStoryCommand = _editStoryCommand;
                                    FillStoryDialog(param);
                                    _storyForEditing = param as StoryItem;
                                    IsNewStoryStatusEditable = true;
                                    ShowStoryDialog = true;
                                },
                            canExecute: null);

            _requestStoryRemoving = new Command(
                parameterizedAction:
                        delegate (object param)
                        {
                            _storyForRemoving = param as StoryItem;
                            ShowRemoveStoryConfirmationDialog = true;
                        },
                    canExecute: null);
        }

        private bool CanEditStory(object obj)
        {
            bool canEdit = true;

            do
            {
                if (_storyForEditing == null)
                {
                    canEdit = false;
                    break;
                }

                if (string.IsNullOrEmpty(NewStoryName))
                {
                    canEdit = false;
                    break;
                }

                IEnumerable<StoryItem> storiesWithCurrentImportance = Stories.Where(story => story.Importance == NewStoryImportance);

                if (storiesWithCurrentImportance.Count() > 0)
                {
                    StoryItem itemWithSameImportance = storiesWithCurrentImportance.First();

                    if (!_storyForEditing.Equals(itemWithSameImportance)) // Importance is set the same value as importance of another StoryItem.
                    {
                        canEdit = false;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(NewStoryStatus))
                {
                    canEdit = false;
                    break;
                }
            } while (false);

            return canEdit;
        }

        private void InitializeBacklog()
        {
            Project currentProject = _menuBasedShellViewModel.CurrentProject;

            if (currentProject == null)
            {
                ShowErrorMessage("Select project!");
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    _backlog = _backlogsDataModel.GetBacklogById(currentProject.ProductBacklogId);

                    ComponentsContainer.Get<Dispatcher>().Invoke(
                        () =>
                        {
                            Stories.Clear();

                            foreach (Story story in _backlog.Stories)
                            {
                                StoryItem storyItem = new StoryItem(story, _requestStoryEditing, _requestStoryRemoving);
                                storyItem.StoryUpdated += OnStoryItemUpdated;
                                Stories.Add(storyItem);
                            }
                        });
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex);
                }

                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
        }

        private void OnStoryItemUpdated(object sender, StoryItem item)
        {
            if(item != null)
            {
                ComponentsContainer.Get<Dispatcher>().Invoke(
                                            delegate
                                            {
                                                Stories.Remove(item);
                                                Stories.Add(item);
                                            });
            }
        }

        private void FillStoryDialog(object param)
        {
            if (param is StoryItem)
            {
                StoryItem item = param as StoryItem;

                NewStoryName = item.Name;
                NewStoryImportance = item.Importance;
                NewStoryNotes = item.Notes;
                NewStoryStatus = item.Status;
            }
        }

        private void EditStory(object param)
        {
            if(!CanEditStory(null))
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    StoryItem clone = _storyForEditing.Clone() as StoryItem;

                    clone.Name = NewStoryName;
                    clone.Importance = NewStoryImportance;
                    clone.Notes = NewStoryNotes;
                    clone.Status = NewStoryStatus;

                    if (_storyForEditing.Equals(clone))
                    {
                        StoryCreationErrorMessage = "Story data is not changed";
                        ShowStoryCreationErrorMessage = true;
                        _menuBasedShellViewModel.IsLoadingData = false;
                        return;
                    }

                    _logger.Debug($"Started editing story with id '{_storyForEditing.Id}'.");

                    _storiesDataModel.Update(clone.Story);

                    StoryItem item = Stories.FirstOrDefault(s => s.Id == clone.Id);

                    if(item != null)
                    {
                        ComponentsContainer.Get<Dispatcher>().Invoke(
                            delegate
                            {
                                Stories.Remove(item);
                                clone.StoryUpdated += OnStoryItemUpdated;
                                Stories.Add(clone);
                            });
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"User story doesn't edited: technical issues encountered.");
                    _logger.Fatal(ex);
                }

                _menuBasedShellViewModel.IsLoadingData = false;
                ShowStoryDialog = false;
            }).Start();
        }

        private void RemoveStory(StoryItem storyItem)
        {
            if (storyItem == null)
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    _storiesDataModel.Remove(storyItem.Story);

                    ComponentsContainer.Get<Dispatcher>().Invoke(
                            delegate
                            {
                                Stories.Remove(storyItem);
                            });
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"User story doesn't removed: technical issues encountered.");
                    _logger.Fatal(ex);
                }

                _menuBasedShellViewModel.IsLoadingData = false;
                ShowRemoveStoryConfirmationDialog = false;
            }).Start();
        }

        private void ShowErrorMessage(string text)
        {
            ErrorText = text;
            ShowErrorMessageDialog = true;
        }

        private void CreateStory(object param)
        {
            if (!CanCreateStory(null))
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    Story newStory = _storiesDataModel.CreateNewStory(NewStoryName, NewStoryImportance, NewStoryNotes, _menuBasedShellViewModel.CurrentProject.ProductBacklogId);

                    StoryItem storyItem = new StoryItem(newStory, _requestStoryEditing, _requestStoryRemoving);
                    storyItem.StoryUpdated += OnStoryItemUpdated;

                    ComponentsContainer.Get<Dispatcher>().Invoke(
                        delegate
                        {
                            Stories.Add(storyItem);
                        });
                }
                catch (Exception ex)
                {
                    StoryCreationErrorMessage = $"Story doesn't created: technical issues encountered.";
                    ShowStoryCreationErrorMessage = true;

                    _logger.Fatal(ex);
                }

                ShowStoryDialog = false;
                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
        }

        private bool CanCreateStory(object obj)
        {
            bool canCreate = true;

            do
            {
                if (string.IsNullOrEmpty(NewStoryName))
                {
                    canCreate = false;
                    break;
                }

                IEnumerable<StoryItem> storiesWithCurrentImportance = Stories.Where(story => story.Importance == NewStoryImportance);

                if (storiesWithCurrentImportance.Count() > 0)
                {
                    canCreate = false;
                    break;
                }

                if(string.IsNullOrEmpty(NewStoryStatus))
                {
                    canCreate = false;
                    break;
                }
            } while (false);

            return canCreate;
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
            InitializeBacklog();
        }

        private void CurrentViewModelChangedEventHandler()
        {
            if (_menuBasedShellViewModel.CurrentViewModel.Equals(this))
            {
                ShowErrorMessageDialog = false;
                InitializeBacklog();
            }
        }
        #endregion

        public class StoryItem : INotifyPropertyChanged, ICloneable
        {
            private Story _story = null;

            private ICommand _editCommand = null;
            private ICommand _removeCommand = null;
            private ICommand _markAsInProgressCommand = null;
            private ICommand _markAsCompletedCommand = null;

            public event EventHandler<StoryItem> StoryUpdated;
            public event PropertyChangedEventHandler PropertyChanged;

            public StoryItem(Story story, ICommand requestStoryEditing, ICommand requestStoryRemoving)
            {
                _story = story;
                _editCommand = requestStoryEditing;
                _removeCommand = requestStoryRemoving;

                MarkAsInProgress = new Command(SetStatusInProgress, CanExecuteSetStatusInProgres);

                MarkAsCompleted = new Command(SetStatusCompleted, CanExecuteSetStatusCompleted);
            }

            private void SetStatusInProgress()
            {
                if(CanExecuteSetStatusInProgres(null))
                {
                    SetStatus(StoryStatus.InProgress);
                }
            }

            private void SetStatusCompleted()
            {
                if (CanExecuteSetStatusCompleted(null))
                {
                    SetStatus(StoryStatus.Completed);
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
                string completedStatusName = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(StoryStatus.Completed);
                return !Status.Equals(completedStatusName);
            }

            public Story Story
            {
                get => _story;
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

            public uint ExecutorId
            {
                get => _story.ExecutorId;

                set
                {
                    _story.ExecutorId = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutorId"));
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

            public ICommand MarkAsInProgress
            {
                get => _markAsInProgressCommand;

                set
                {
                    _markAsInProgressCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MarkAsInProgress"));
                }
            }

            public ICommand MarkAsCompleted
            {
                get => _markAsCompletedCommand;

                set
                {
                    _markAsCompletedCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MarkAsCompleted"));
                }
            }

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
                    ExecutorId,
                    _story.BacklogId);
                return new StoryItem(story, _editCommand, _removeCommand);
            }

            public override bool Equals(object obj)
            {
                bool equals = false;

                if(obj is StoryItem)
                {
                    StoryItem item = obj as StoryItem;

                    equals =    item.Id == Id &&
                                item.Name == Name &&
                                item.ExecutorId == ExecutorId &&
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
