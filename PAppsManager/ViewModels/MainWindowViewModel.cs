using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;

namespace PAppsManager.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        public void Install()
        {
            Process.Start("http://compareason.com");
        }

        public void Uninstall()
        {
            MessageBox.Show("Simply delete the portable application directoryy.");
        }

        public void Exit()
        {
            App.Current.Shutdown();
        }
    }
}