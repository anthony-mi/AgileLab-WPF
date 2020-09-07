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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AgileLab.Views.AllUserTasks
{
    class AllUserTasksViewModel : ViewModelBase
    {
        #region Fields
        private MenuBasedShellViewModel _menuBasedShellViewModel = null;

        private IStoriesDataModel _storiesDataModel = ComponentsContainer.Get<IStoriesDataModel>();
        private IStoryStatusesDataModel _storyStatusesDataModel = ComponentsContainer.Get<IStoryStatusesDataModel>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();

        private ObservableCollection<TaskItem> _allUserTasks = null;

        private ViewModelBase _currentDialogViewModel = null;
        private ConfirmationDialogViewModel _confirmationDialogViewModel = null;
        private MessageBoxViewModel _messageBoxViewModel = null;
        private StoryDialogViewModel _storyDialogViewModel = null;

        private ICommand _requestStoryEditing = null;
        private ICommand _requestStoryRemoving = null;

        private bool _showDialog = false;

        private TaskItem _selectedStory = null;
        #endregion

        #region Constructors
        internal AllUserTasksViewModel(MenuBasedShellViewModel menuBasedShellViewModel)
        {
            _menuBasedShellViewModel = menuBasedShellViewModel;
            InitializeFields();
            InitializeCommands();

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

        public StoryDialogViewModel StoryDialogViewModel
        {
            get => _storyDialogViewModel;
            private set => SetProperty(ref _storyDialogViewModel, value);
        }

        public ObservableCollection<TaskItem> AllUserTasks
        {
            get => _allUserTasks;
            set => SetProperty(ref _allUserTasks, value);
        }

        public TaskItem SelectedTask
        {
            get => _selectedStory;
            set => SetProperty(ref _selectedStory, value);
        }
        #endregion

        #region Methods
        private void SubscribeToDataModelEvents()
        {
            _storiesDataModel.StoryUpdated += StoryUpdatedEventHandler;
            _storiesDataModel.StoryRemoved += StoryRemovedEventHandler;
        }

        private void InitializeFields()
        {
            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                List<TaskItem> userInProgressStories = new List<TaskItem>();
                string inProgressStatusName = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(StoryStatus.InProgress);

                foreach (Project project in GetCurrentUserProjects())
                {
                    Data.Entities.Sprint sprint = ComponentsContainer.Get<ISprintsDataModel>().GetSprintByProjectId(project.Id);

                    if (sprint != null)
                    {
                        Backlog backlog = ComponentsContainer.Get<IBacklogsDataModel>().GetBacklogById(sprint.BacklogId);
                        List<Story> stories = backlog.Stories.Where(
                            item =>
                            item.Status == inProgressStatusName &&
                            item.ExecutorId == _menuBasedShellViewModel.CurrentUser.Id)
                            .ToList();
                        userInProgressStories.AddRange(ConvertToTaskItems(stories, sprint.FinishDate, project.Name));
                    }
                }

                AllUserTasks = new ObservableCollection<TaskItem>(userInProgressStories);

                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
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

        private List<Project> GetCurrentUserProjects()
        {
            List<Project> userProjects = new List<Project>();

            List<Team> userTeams = (List<Team>)ComponentsContainer.Get<ITeamsDataModel>().GetTeamsOfUser(_menuBasedShellViewModel.CurrentUser.Id);

            foreach (Team team in userTeams)
            {
                userProjects.AddRange((List<Project>)ComponentsContainer.Get<IProjectsDataModel>().GetProjectsOfTeam(team));
            }

            return userProjects;
        }

        private void FillStoryDialog(object param)
        {
            if (param is TaskItem && StoryDialogViewModel != null)
            {
                TaskItem item = param as TaskItem;

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
            if (StoryDialogViewModel == null)
            {
                return;
            }

            if (!StoryDialogViewModel.CanExecuteConfirmCommand(null))
            {
                return;
            }

            if (SelectedTask == null)
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    TaskItem clone = SelectedTask.Clone() as TaskItem;

                    clone.Name = StoryDialogViewModel.Name;
                    clone.Importance = StoryDialogViewModel.Importance;
                    clone.InitialEstimate = StoryDialogViewModel.InitialEstimate;
                    clone.HowToDemo = StoryDialogViewModel.HowToDemo;
                    clone.Status = StoryDialogViewModel.Status;
                    clone.Notes = StoryDialogViewModel.Notes;

                    if (SelectedTask.Equals(clone))
                    {
                        StoryDialogViewModel.ErrorMessage = "Story data is not changed";
                        StoryDialogViewModel.ShowErrorMessage = true;
                        _menuBasedShellViewModel.IsLoadingData = false;
                        return;
                    }

                    Backlog backlog = ComponentsContainer.Get<IBacklogsDataModel>().GetBacklogById(clone.Story.BacklogId);
                    List<Story> storiesWithTheSameName = backlog.Stories.Where(si => si.Name == clone.Name).ToList();

                    if (storiesWithTheSameName.Count > 0)
                    {
                        if (storiesWithTheSameName.First().Id != clone.Id) //Selected story was renamed as other story in this sprint (backlog).
                        {
                            StoryDialogViewModel.ErrorMessage = $"Story with name '{clone.Name}' already exists in sprint.";
                            StoryDialogViewModel.ShowErrorMessage = true;
                            _menuBasedShellViewModel.IsLoadingData = false;
                            return;
                        }
                    }

                    _logger.Debug($"Started editing story with id '{SelectedTask.Id}'.");

                    _storiesDataModel.Update(clone.Story);

                    TaskItem item = AllUserTasks.FirstOrDefault(s => s.Id == clone.Id);

                    if (item != null)
                    {
                        ComponentsContainer.Get<Dispatcher>().Invoke(
                            delegate
                            {
                                AllUserTasks.Remove(item);
                                AllUserTasks.Add(clone);
                            });
                    }

                    HideDialog?.Invoke(this, null);
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"User story doesn't edited: technical issues encountered.");
                    _logger.Fatal(ex);
                }

                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
        }

        private void ShowErrorMessage(string text)
        {
            MessageBoxViewModel = new MessageBoxViewModel();
            MessageBoxViewModel.Text = text;
            MessageBoxViewModel.Confirmed += delegate { HideDialog?.Invoke(this, null); };
            SetDialog?.Invoke(this, MessageBoxViewModel);
        }

        private void RemoveSelectedStory(object sender, EventArgs e)
        {
            RemoveSelectedStory();
        }

        private void RemoveSelectedStory()
        {
            if (SelectedTask == null)
            {
                return;
            }

            _menuBasedShellViewModel.IsLoadingData = true;

            new Task(() =>
            {
                try
                {
                    _storiesDataModel.Remove(SelectedTask.Story);

                    ComponentsContainer.Get<Dispatcher>().Invoke(
                            delegate
                            {
                                AllUserTasks.Remove(SelectedTask);
                            });

                    HideDialog?.Invoke(this, null);
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"User story doesn't removed: technical issues encountered.");
                    _logger.Fatal(ex);
                }

                _menuBasedShellViewModel.IsLoadingData = false;
            }).Start();
        }

        private List<TaskItem> ConvertToTaskItems(List<Story> stories, DateTime sprintFinishingDate, string projectName)
        {
            List<TaskItem> items = new List<TaskItem>();

            if (stories != null)
            {
                foreach (Story story in stories)
                {
                    TaskItem newItem = ConvertToTaskItem(story, sprintFinishingDate, projectName);
                    items.Add(newItem);
                }
            }

            return items;
        }

        private TaskItem ConvertToTaskItem(Story story, DateTime sprintFinishingDate, string projectName)
        {
            TaskItem item = null;

            if (story != null)
            {
                item = new TaskItem(story, sprintFinishingDate, projectName, _requestStoryEditing, _requestStoryRemoving);
            }

            return item;
        }
        #endregion

        #region "Event Handlers"
        private void StoryUpdatedEventHandler(object sender, Story story)
        {
            if(story == null)
            {
                return;
            }

            string inProgressStatusName = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(StoryStatus.InProgress);

            TaskItem[] itemsWithSameId = AllUserTasks.Where(item => item.Id == story.Id).ToArray();

            if(itemsWithSameId.Count() > 0) // Updated already existing TaskItem.
            {
                if(story.Status != inProgressStatusName) // Status was changed. In this case, this object does not interest us.
                {
                    TaskItem item = new TaskItem(story, default(DateTime), null, null, null);

                    ComponentsContainer.Get<Dispatcher>().Invoke(
                        delegate
                        {
                            AllUserTasks.Remove(item);
                        });
                }
                else // In this case all updates will be showed in view due to implementation INotifyPropertyChanged.
                {
                    return;
                }
            }
            else // Was updated item not existing in AllUserTasks.
            {
                if (story.Status == inProgressStatusName && story.ExecutorId == _menuBasedShellViewModel.CurrentUser.Id)
                {
                    Data.Entities.Sprint storySprint = GetStorySprint(story);

                    if(storySprint == null)
                    {
                        return;
                    }

                    Project project = ComponentsContainer.Get<IProjectsDataModel>().GetProjectById(storySprint.ProjectId);

                    ComponentsContainer.Get<Dispatcher>().Invoke(
                        delegate
                        {
                            AllUserTasks.Add(new TaskItem(story, storySprint.FinishDate, project.Name, _requestStoryEditing, _requestStoryRemoving));
                        });
                }
            }
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

            TaskItem[] itemsWithSameId = AllUserTasks.Where(item => item.Id == story.Id).ToArray();

            if (itemsWithSameId.Count() > 0) 
            {
                ComponentsContainer.Get<Dispatcher>().Invoke(
                        delegate
                        {
                            AllUserTasks.Remove(itemsWithSameId.First());
                        });
            }
        }
        #endregion

        #region Events
        public event EventHandler<ViewModelBase> SetDialog;
        public event EventHandler HideDialog;
        #endregion

        public class TaskItem : INotifyPropertyChanged, ICloneable
        {
            private Story _story = null;
            private DateTime _sprintFinishingDate;
            private string _projectName;

            private ICommand _editCommand = null;
            private ICommand _removeCommand = null;
            private ICommand _finishCommand = null;

            public TaskItem(Story story, DateTime sprintFinishingDate, string projectName, ICommand requestStoryEditing, ICommand requestStoryRemoving)
            {
                _story = story;
                _sprintFinishingDate = sprintFinishingDate;
                _projectName = projectName;

                EditCommand = requestStoryEditing;
                RemoveCommand = requestStoryRemoving;

                FinishCommand = new Command(SetStatusCompleted, null);
            }

            private void SetStatusCompleted()
            {
                SetStatus(StoryStatus.Completed);
            }

            private void SetStatus(StoryStatus storyStatus)
            {
                Status = ComponentsContainer.Get<IStoryStatusesDataModel>().GetStatusText(storyStatus);
                ComponentsContainer.Get<IStoriesDataModel>().Update(_story);
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

            public string SprintFinishingDate
            {
                get => _sprintFinishingDate.ToShortDateString();
            }

            public string ProjectName
            {
                get => _projectName;
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
                return new TaskItem(story, _sprintFinishingDate, _projectName, _editCommand, _removeCommand);
            }

            public override bool Equals(object obj)
            {
                bool equals = false;

                if (obj is TaskItem)
                {
                    TaskItem item = obj as TaskItem;

                    equals = item.Id == Id &&
                                item.Name == Name &&
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
