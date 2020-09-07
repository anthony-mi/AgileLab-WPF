using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace AgileLab.Views.SignUp
{
    /// <summary>
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : UserControl
    {
        public SignUp()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(DataContextChangedEventHandler);
        }

        private void DataContextChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(DataContext != null && DataContext is SignUpViewModel)
            {
                SignUpViewModel viewModel = DataContext as SignUpViewModel;
                viewModel.ViewContext = this;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null &&
                DataContext is SignUpViewModel &&
                sender is PasswordBox)
            {
                SignUpViewModel viewModel = DataContext as SignUpViewModel;
                PasswordBox passwordbox = sender as PasswordBox;
                viewModel.Password = passwordbox.SecurePassword;
            }
        }

        private void RetypedPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null &&
                DataContext is SignUpViewModel &&
                sender is PasswordBox)
            {
                SignUpViewModel viewModel = DataContext as SignUpViewModel;
                PasswordBox passwordbox = sender as PasswordBox;
                viewModel.RetypedPassword = passwordbox.SecurePassword;
            }
        }

        private void OnBackBtnClick(object sender, RoutedEventArgs e)
        {
            
        }  
    }
}
