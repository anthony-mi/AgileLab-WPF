using AgileLab.Views.MainShell;
using System;
using System.Windows;

namespace AgileLab
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Window mainWindow = new MainShellWindow();
                mainWindow.Show();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"There are an error occurred.\nException: {ex.Message}");

                if(ex.InnerException != null)
                {
                    MessageBox.Show($"There are an error occurred.\nInner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}
