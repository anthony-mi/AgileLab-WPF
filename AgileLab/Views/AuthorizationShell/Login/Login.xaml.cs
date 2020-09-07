using System.Windows;
using System.Windows.Controls;

namespace AgileLab.Views.Login
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(DataContextChangedEventHandler);
        }

        private void DataContextChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is LoginViewModel)
            {
                LoginViewModel viewModel = DataContext as LoginViewModel;
                viewModel.ViewContext = this;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null &&
                DataContext is LoginViewModel &&
                sender is PasswordBox)
            {
                LoginViewModel viewModel = DataContext as LoginViewModel;
                PasswordBox passwordbox = sender as PasswordBox;
                viewModel.Password = passwordbox.SecurePassword;
            }
        }
    }
}
