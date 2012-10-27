using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using Caliburn.Micro;
using PAppsManager.Core.PApps;

namespace PAppsManager.ViewModels
{
    internal class MainWindowViewModel : PropertyChangedBase
    {
        private readonly PortableEnvironment _environment;

        public MainWindowViewModel(PortableEnvironment environment)
        {
            _environment = environment;
        }

        public void Install()
        {
            Process.Start("http://compareason.com");
        }

        public void InstallApplication(string url)
        {
            try
            {
                // Load the application info.
                PortableApplication application;
                using (var webClient = new WebClient())
                    application = PortableApplication.LoadFromUrl(url, webClient.DownloadString);

                // Confirm installation.
                if (MessageBox.Show(string.Format("Do you want to the install the portable application {0}?", application.Name), "PApps Manager - Install Application", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // Install the application.
                    _environment.Applications.Add(application);

                    MessageBox.Show(
                        string.Format("The portable application {0} has been install successfully. You can now use it like a regular application.", application.Name),
                        "PApps Manager - Install Applicaiton");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to install the application: " + e.Message, "PApps Manager - Install Applicaiton",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Uninstall()
        {
            MessageBox.Show("Simply delete the portable application directory.");

            if (!_environment.Applications.DefaultInstallationDirectory.Exists)
                _environment.Applications.DefaultInstallationDirectory.Create();
            Process.Start(new ProcessStartInfo("explorer.exe", "/n, /e, " + _environment.Applications.DefaultInstallationDirectory));
        }

        public void Exit()
        {
            App.Current.Shutdown();
        }
    }
}