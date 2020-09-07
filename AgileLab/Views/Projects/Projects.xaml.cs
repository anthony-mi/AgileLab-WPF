using AgileLab.Data.Entities;
using AgileLab.Views.Dialog;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AgileLab.Views.Projects
{
    /// <summary>
    /// Interaction logic for Projects.xaml
    /// </summary>
    public partial class ProjectsView : UserControl
    {
        ProjectsViewModel _viewModel = null;
        private const double _TILES_MARGIN = 5;

        public ProjectsView()
        {
            InitializeComponent();

            if (DataContext is ProjectsViewModel)
            {
                _viewModel = DataContext as ProjectsViewModel;

                Control createProjectControl = CreateCpControl();
                currentUserProjectsContainer.Children.Add(createProjectControl);

                SubscribeToViewModelEvents();

                RefreshShowErrorMessageDialogVisibility();
                RefreshCreateNewProjectDialogVisibility();
            }
            else
            {
                DataContextChanged += new DependencyPropertyChangedEventHandler(DataContextChangedEventHandler);
            }
        }

        private void DataContextChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is ProjectsViewModel)
            {
                _viewModel = DataContext as ProjectsViewModel;

                Control createProjectControl = CreateCpControl();
                currentUserProjectsContainer.Children.Add(createProjectControl);
                _viewModel.ViewContext = this;

                SubscribeToViewModelEvents();

                RefreshShowErrorMessageDialogVisibility();
                RefreshCreateNewProjectDialogVisibility();
            }
        }

        private void SubscribeToViewModelEvents()
        {
            if (_viewModel.Projects.Count > 0)
            {
                AddProjectsItems(_viewModel.Projects.ToList() as IList<Project>);
            }

            _viewModel.PropertyChanged += ViewModelPropertyChangedEventHandler;
            _viewModel.Projects.CollectionChanged += OnProjectsCollectionChanged;
            _viewModel.ProjectSelected += OnProjectSelected;
        }

        private void ViewModelPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            string ShowCreateNewProjectDialogPropertyName = GetPropertyName<ProjectsViewModel>(item => item.ShowCreateNewProjectDialog);
            string ShowErrorMessageDialogPropertyName = GetPropertyName<ProjectsViewModel>(item => item.ShowErrorMessageDialog);

            if (e.PropertyName.Equals(ShowCreateNewProjectDialogPropertyName)) // Can't use switch-case statement because property names are not constants.
            {
                RefreshCreateNewProjectDialogVisibility();
            }
            else if (e.PropertyName.Equals(ShowErrorMessageDialogPropertyName))
            {
                RefreshShowErrorMessageDialogVisibility();
            }
        }

        // Can't use boolToVisibility converter in XAML of this view
        // because for unknown reasons changing of one property value changes both dialogs.
        // TODO: fix this crutch.
        private void RefreshShowErrorMessageDialogVisibility()
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    ErrorMessageDialog.Visibility = _viewModel.ShowErrorMessageDialog ? Visibility.Visible : Visibility.Hidden;
                });
        }

        private void RefreshCreateNewProjectDialogVisibility()
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    NewProjectDialog.Visibility = _viewModel.ShowCreateNewProjectDialog ? Visibility.Visible : Visibility.Hidden;
                });
        }

        static string GetPropertyName<TObject>(Expression<Func<TObject, object>> exp)
        {
            System.Linq.Expressions.Expression body = exp.Body;
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

        private void SubscribeToViewModelEvents(ProjectsViewModel projectsViewModel)
        {
            if (projectsViewModel.Projects.Count > 0)
            {
                AddProjectsItems(projectsViewModel.Projects.ToList() as IList<Project>);
            }

            projectsViewModel.Projects.CollectionChanged += OnProjectsCollectionChanged;
            projectsViewModel.ProjectSelected += OnProjectSelected;
        }

        private void AddProjectsItems(System.Collections.IList projects)
        {
            try
            {
                AddProjectsItems(projects.Cast<Project>().ToList() as IList<Project>);
            }
            catch { }
        }

        private void AddProjectsItems(IList<Project> projects)
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    try
                    {
                        foreach (Project project in projects)
                        {
                            Control projectControl = CreateProjectControl(project);
                            currentUserProjectsContainer.Children.Add(projectControl);
                        }
                    }
                    catch { }
                });
        }

        private void RemoveProjectsItems(System.Collections.IList projects)
        {
            try
            {
                RemoveProjectsItems(projects.Cast<Project>().ToList() as IList<Project>);
            }
            catch { }
        }

        private void RemoveProjectsItems(IList<Project> projects)
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    try
                    {
                        List<UIElement> controlsForRemoving = new List<UIElement>();

                        foreach (Project project in projects)
                        {
                            foreach (UIElement uiElement in currentUserProjectsContainer.Children)
                            {
                                if (uiElement is Tile)
                                {
                                    Tile tile = uiElement as Tile;

                                    if (tile.Tag is Project)
                                    {
                                        Project tempProject = tile.Tag as Project;

                                        if (project.Equals(tempProject))
                                        {
                                            controlsForRemoving.Add(uiElement);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (UIElement element in controlsForRemoving)
                        {
                            currentUserProjectsContainer.Children.Remove(element);
                        }
                    }
                    catch { }
                });
        }

        private Control CreateCpControl() // CP means "create project". Abbreviated to avoid complicating the name of the method.
        {
            Tile tile = new Tile();
            tile.Title = "Create new project";
            tile.Click += CreateNewProjectClick;
            tile.Cursor = System.Windows.Input.Cursors.Hand;
            tile.Margin = new Thickness(_TILES_MARGIN);
            tile.TitleFontSize = 10;

            PackIconOcticons plusIcon = new PackIconOcticons();
            plusIcon.Kind = PackIconOcticonsKind.Plus;
            tile.Content = plusIcon;

            return tile;
        }

        private Control CreateProjectControl(Project project)
        {
            Tile tile = new Tile();
            tile.Content = project.Name;
            tile.Margin = new Thickness(_TILES_MARGIN);
            tile.Tag = project;

            if (project.Equals(_viewModel.SelectedProject))
            {
                tile.IsEnabled = false;
            }
            else
            {
                tile.Click += OnProjectClick;
                tile.Cursor = System.Windows.Input.Cursors.Hand;
            }

            return tile;
        }

        private void OnProjectClick(object sender, RoutedEventArgs e)
        {
            if (sender is Tile)
            {
                Project project = (sender as Tile).Tag as Project;
                _viewModel.SelectProject(project);
            }
        }

        private void CreateNewProjectClick(object sender, RoutedEventArgs e)
        {
            _viewModel.RequestProjectCreation.Execute(e);
        }

        private void OnProjectsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RemoveProjectsItems(e.OldItems);
            AddProjectsItems(e.NewItems);
        }

        private void OnProjectSelected(object sender, Project project)
        {
            foreach (UIElement el in currentUserProjectsContainer.Children)
            {
                if (el is Tile)
                {
                    Tile tile = el as Tile;
                    Project tempProject = tile.Tag as Project;

                    if (tempProject == null)
                    {
                        continue;
                    }

                    if (tempProject.Equals(project))
                    {
                        if (!tile.IsEnabled)
                        {
                            // Selection of already selected project/UIElement
                            return;
                        }

                        SetElementAsSelected(el);
                        continue;
                    }

                    if (!el.IsEnabled) // Already selected.
                    {
                        SetElementAsUnselected(el);
                    }
                }
            }
        }

        private void SetElementAsSelected(UIElement el)
        {
            el.IsEnabled = false;

            if (el is Tile)
            {
                (el as Tile).Click -= OnProjectClick;
            }

            if (el is Control)
            {
                (el as Control).Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void SetElementAsUnselected(UIElement el)
        {
            el.IsEnabled = true;

            if (el is Tile)
            {
                (el as Tile).Click += OnProjectClick;
            }

            if (el is Control)
            {
                (el as Control).Cursor = System.Windows.Input.Cursors.Hand;
            }
        }
    }
}
