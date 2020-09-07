using System;
using System.Collections.Generic;
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

namespace AgileLab.Views.ProductBacklog
{
    /// <summary>
    /// Interaction logic for ProductBacklog.xaml
    /// </summary>
    public partial class ProductBacklog : UserControl
    {
        private ProductBacklogViewModel _viewModel = null;

        public ProductBacklog()
        {
            InitializeComponent();

            if (DataContext is ProductBacklogViewModel)
            {
                _viewModel = DataContext as ProductBacklogViewModel;
                _viewModel.PropertyChanged += ViewModelPropertyChangedEventHandler;

                RefreshErrorMessageDialogVisibility();
                RefreshStoryDialogVisibility();
                RefreshRemoveStoryConfirmationDialogVisibility();
            }
            else
            {
                DataContextChanged += new DependencyPropertyChangedEventHandler(DataContextChangedEventHandler);
            }
        }

        private void DataContextChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is ProductBacklogViewModel)
            {
                _viewModel = DataContext as ProductBacklogViewModel;
                _viewModel.PropertyChanged += ViewModelPropertyChangedEventHandler;

                RefreshErrorMessageDialogVisibility();
                RefreshStoryDialogVisibility();
                RefreshRemoveStoryConfirmationDialogVisibility();
            }
        }

        // Can't use boolToVisibility converter in XAML of this view
        // because for unknown reasons changing of one property value changes both dialogs.
        // TODO: fix this crutch.
        private void RefreshErrorMessageDialogVisibility()
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    ErrorMessageDialog.Visibility = _viewModel.ShowErrorMessageDialog ? Visibility.Visible : Visibility.Hidden;
                });
        }

        private void RefreshStoryDialogVisibility()
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    StoryDialog.Visibility = _viewModel.ShowStoryDialog ? Visibility.Visible : Visibility.Hidden;
                });
        }

        private void RefreshRemoveStoryConfirmationDialogVisibility()
        {
            ComponentsContainer.Get<Dispatcher>().Invoke(
                delegate
                {
                    UserStoryRemovingConfirmationDialog.Visibility = _viewModel.ShowRemoveStoryConfirmationDialog ? Visibility.Visible : Visibility.Hidden;
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

        private void ViewModelPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            string ShowStoryDialogPropertyName = GetPropertyName<ProductBacklogViewModel>(item => item.ShowStoryDialog);
            string ShowErrorMessageDialogPropertyName = GetPropertyName<ProductBacklogViewModel>(item => item.ShowErrorMessageDialog);
            string ShowRemoveStoryConfirmationDialogPropertyName = GetPropertyName<ProductBacklogViewModel>(item => item.ShowRemoveStoryConfirmationDialog);

            if (e.PropertyName.Equals(ShowStoryDialogPropertyName)) // Can't use switch-case statement because property names are not constants.
            {
                RefreshStoryDialogVisibility();
            }
            else if (e.PropertyName.Equals(ShowErrorMessageDialogPropertyName))
            {
                RefreshErrorMessageDialogVisibility();
            }
            else if (e.PropertyName.Equals(ShowRemoveStoryConfirmationDialogPropertyName))
            {
                RefreshRemoveStoryConfirmationDialogVisibility();
            }
        }
    }
}
