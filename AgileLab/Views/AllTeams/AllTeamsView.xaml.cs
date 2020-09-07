using AgileLab.Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace AgileLab.Views.AllTeams
{
    /// <summary>
    /// Interaction logic for AllTeamsView.xaml
    /// </summary>
    public partial class AllTeamsView : UserControl
    {
        private AllTeamsViewModel _viewModel = null;

        public AllTeamsView()
        {
            InitializeComponent();

            if (DataContext is AllTeamsViewModel)
            {
                _viewModel = DataContext as AllTeamsViewModel;
            }
            else
            {
                DataContextChanged += new DependencyPropertyChangedEventHandler(DataContextChangedEventHandler);
            }
        }

        private void DataContextChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is AllTeamsViewModel)
            {
                _viewModel = DataContext as AllTeamsViewModel;
            }
        }
    }
}
