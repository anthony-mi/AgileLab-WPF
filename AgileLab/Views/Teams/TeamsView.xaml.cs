using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using AgileLab.Data.Entities;
using System.Windows.Threading;

namespace AgileLab.Views.Teams
{
    /// <summary>
    /// Interaction logic for TeamsView.xaml
    /// </summary>
    public partial class TeamsView : UserControl
    {
        private TeamsViewModel _viewModel = null;
        private const double _TILES_MARGIN = 5;

        public TeamsView()
        {
            InitializeComponent();

            if (DataContext is TeamsViewModel)
            {
                Control createTeamControl = CreateCtControl();
                currentUserTeamsContainer.Children.Add(createTeamControl);

                SubscribeToViewModelEvents(DataContext as TeamsViewModel);
            }
            else
            {
                DataContextChanged += new DependencyPropertyChangedEventHandler(DataContextChangedEventHandler);
            }
        }

        private void SubscribeToViewModelEvents(TeamsViewModel teamsViewModel)
        {
            if(teamsViewModel.CurrentUserTeams.Count > 0)
            {
                AddTeamsItems(teamsViewModel.CurrentUserTeams.ToList() as IList<Team>);
            }

            teamsViewModel.CurrentUserTeams.CollectionChanged += OnTeamsCollectionChanged;
            teamsViewModel.TeamSelected += OnTeamSelected;
        }

        private void AddTeamsItems(System.Collections.IList teams)
        {
            try
            {
                AddTeamsItems(teams.Cast<Team>().ToList() as IList<Team>);
            }
            catch { }
        }

        private void AddTeamsItems(IList<Team> teams)
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    try
                    {
                        foreach (Team team in teams)
                        {
                            Control teamControl = CreateTeamControl(team);
                            currentUserTeamsContainer.Children.Add(teamControl);
                        }
                    }
                    catch { }
                });
        }

        private void RemoveTeamsItems(System.Collections.IList teams)
        {
            try
            {
                RemoveTeamsItems(teams.Cast<Team>().ToList() as IList<Team>);
            }
            catch { }
        }

        private void RemoveTeamsItems(IList<Team> teams)
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    try
                    {
                        List<UIElement> controlsForRemoving = new List<UIElement>();

                        foreach (Team team in teams)
                        {
                            foreach (UIElement uiElement in currentUserTeamsContainer.Children)
                            {
                                if (uiElement is Tile)
                                {
                                    Tile tile = uiElement as Tile;

                                    if (tile.Tag is Team)
                                    {
                                        Team tmpTeam = tile.Tag as Team;

                                        if (team.Id == tmpTeam.Id)
                                        {
                                            controlsForRemoving.Add(uiElement);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (UIElement element in controlsForRemoving)
                        {
                            currentUserTeamsContainer.Children.Remove(element);
                        }
                    }
                    catch { }
                });
        }

        private Control CreateCtControl() // CT means "create team". Abbreviated to avoid complicating the name of the method.
        {
            Tile tile = new Tile();
            tile.Title = "Create new team";
            tile.Click += CreateNewTeamClick;
            tile.Cursor = System.Windows.Input.Cursors.Hand;
            tile.Margin = new Thickness(_TILES_MARGIN);
            tile.TitleFontSize = 10;

            PackIconOcticons plusIcon = new PackIconOcticons();
            plusIcon.Kind = PackIconOcticonsKind.Plus;
            tile.Content = plusIcon;

            return tile;
        }

        private Control CreateTeamControl(Team team)
        {
            Tile tile = new Tile();           
            tile.Content = team.Name;
            tile.Margin = new Thickness(_TILES_MARGIN);
            tile.Tag = team;
            
            if(team.Equals(_viewModel.SelectedTeam))
            {
                tile.IsEnabled = false;
            }
            else
            {
                tile.Click += OnTeamClick;
                tile.Cursor = System.Windows.Input.Cursors.Hand;
            }

            return tile;
        }

        private void OnTeamClick(object sender, RoutedEventArgs e)
        {
            if(sender is Tile)
            {
                Team team = (sender as Tile).Tag as Team;
                _viewModel.SelectTeam(team);
            }
        }

        private void CreateNewTeamClick(object sender, RoutedEventArgs e)
        {
            _viewModel.RequestTeamCreationCommand.Execute(e);
        }

        private void OnTeamsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RemoveTeamsItems(e.OldItems);
            AddTeamsItems(e.NewItems);
        }

        private void OnTeamSelected(object sender, Team team)
        {
            foreach(UIElement el in currentUserTeamsContainer.Children)
            {
                if(el is Tile)
                {
                    Tile tile = el as Tile;
                    Team tmpTeam = tile.Tag as Team;

                    if(tmpTeam == null)
                    {
                        continue;
                    }

                    if(tmpTeam.Equals(team))
                    {
                        if(!tile.IsEnabled)
                        {
                            // Selection of already selected team/UIElement
                            return;
                        }

                        SetElementAsSelected(el);
                        continue;
                    }

                    if(!el.IsEnabled) // Already selected.
                    {
                        SetElementAsUnselected(el);
                    }
                }
            }
        }

        private void SetElementAsSelected(UIElement el)
        {
            el.IsEnabled = false;

            if(el is Tile)
            {
                (el as Tile).Click -= OnTeamClick;
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
                (el as Tile).Click += OnTeamClick;
            }

            if (el is Control)
            {
                (el as Control).Cursor = System.Windows.Input.Cursors.Hand;
            }
        }

        private void DataContextChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is TeamsViewModel)
            {
                Control createTeamControl = CreateCtControl();
                currentUserTeamsContainer.Children.Add(createTeamControl);

                TeamsViewModel viewModel = DataContext as TeamsViewModel;
                _viewModel = viewModel;
                SubscribeToViewModelEvents(viewModel);
                viewModel.ViewContext = this;
            }
        }
    }
}
