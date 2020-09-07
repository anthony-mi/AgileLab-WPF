using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

namespace AgileLab.Views.Sprint
{
    /// <summary>
    /// Interaction logic for Sprint.xaml
    /// </summary>
    public partial class SprintView : UserControl
    {
        private SprintViewModel _viewModel = null;
        private List<BaseMetroDialog> _dialogs = null;

        public SprintView()
        {
            InitializeComponent();
            InitializeDialogs();

            if (DataContext is SprintViewModel)
            {
                _viewModel = DataContext as SprintViewModel;
                //_viewModel.PropertyChanged += ViewModelPropertyChangedEventHandler;
                _viewModel.SetDialog += ShowDialogEventHandler;
                _viewModel.HideDialog += HideDialogEventHandler;
                CheckDialogsState();
            }
            else
            {
                DataContextChanged += new DependencyPropertyChangedEventHandler(DataContextChangedEventHandler);
            }
        }

        private void InitializeDialogs()
        {
            _dialogs = new List<BaseMetroDialog>();

            _dialogs.Add(ErrorMessageDialog);
            _dialogs.Add(ConfirmationDialog);
            _dialogs.Add(SprintDialog);
            _dialogs.Add(StoryDialog);

            ErrorMessageDialog.DataContextChanged += CheckDialogsState;
            ConfirmationDialog.DataContextChanged += CheckDialogsState;
            SprintDialog.DataContextChanged += CheckDialogsState;
            StoryDialog.DataContextChanged += CheckDialogsState;
        }

        private void DataContextChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is SprintViewModel)
            {
                _viewModel = DataContext as SprintViewModel;
                //_viewModel.PropertyChanged += ViewModelPropertyChangedEventHandler;
                _viewModel.SetDialog += ShowDialogEventHandler;
                _viewModel.HideDialog += HideDialogEventHandler;
                CheckDialogsState();
            }
        }

        private void CheckDialogsState()
        {
            if (_viewModel.ShowDialog && _viewModel.CurrentDialogViewModel != null)
            {
                ShowDialogEventHandler(this, _viewModel.CurrentDialogViewModel);
            }
        }

        private void CheckDialogsState(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel.ShowDialog && _viewModel.CurrentDialogViewModel != null)
            {
                ShowDialogEventHandler(this, _viewModel.CurrentDialogViewModel);
            }
        }

        private void ShowDialogEventHandler(object sender, ViewModelBase viewModel)
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    if (_dialogs != null)
                    {
                        foreach (BaseMetroDialog dialog in _dialogs)
                        {
                            if (dialog.DataContext == null)
                            {
                                continue;
                            }

                            if (dialog.DataContext.GetType() == viewModel.GetType())
                            {
                                dialog.Visibility = Visibility.Visible;
                                DialogsMask.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                dialog.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                });  
        }

        private void HideDialogEventHandler(object sender, EventArgs e)
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    if (_dialogs != null)
                    {
                        foreach (BaseMetroDialog dialog in _dialogs)
                        {
                            if (dialog.Visibility == Visibility.Visible)
                            {
                                dialog.Visibility = Visibility.Hidden;
                            }
                        }
                    }

                    DialogsMask.Visibility = Visibility.Hidden;
                });   
        }
    }
}
