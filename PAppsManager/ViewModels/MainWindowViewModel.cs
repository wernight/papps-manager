using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using PAppsManager.Core.PApps;

namespace PAppsManager.ViewModels
{
    internal class MainWindowViewModel : PropertyChangedBase
    {
        private readonly PortableEnvironment _portableEnvironment;
        private static readonly FileInfo EnvironmentJson = new FileInfo("Environment.json");

        public MainWindowViewModel()
        {
            _portableEnvironment = SetUpPortableEnvironment();
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
                try
                {
                    using (var webClient = new WebClient())
                        application = PortableApplication.LoadFromUrl(url, webClient.DownloadString);
                }
                catch (Exception e)
                {
                    throw new Exception("Couldn't load the application's information:" + Environment.NewLine
                                        + e.Message + Environment.NewLine
                                        + Environment.NewLine
                                        + "You may have to upgrade your PApps Manager to a newer version.", e);
                }

                // Confirm installation.
                if (MessageBox.Show(string.Format("Do you want to the install the portable application {0}?", application.Name), "PApps Manager - Install Application", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // Install the application.
                    _portableEnvironment.Applications.Add(application);

                    MessageBox.Show(
                        string.Format("The portable application {0} has been install successfully. You can now use it like a regular application.", application.Name),
                        "PApps Manager - Install Applicaiton");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to install the application:" + Environment.NewLine + e.Message, "PApps Manager - Install Applicaiton",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Uninstall()
        {
            MessageBox.Show("Simply delete the portable application directory.");

            if (!_portableEnvironment.Applications.DefaultInstallationDirectory.Exists)
                _portableEnvironment.Applications.DefaultInstallationDirectory.Create();
            Process.Start(new ProcessStartInfo("explorer.exe", "/n, /e, " + _portableEnvironment.Applications.DefaultInstallationDirectory));
        }

        public void Exit()
        {
            TearDownPortableEnvironment();
            App.Current.Shutdown();
        }

        private PortableEnvironment SetUpPortableEnvironment()
        {
            PortableEnvironment portableEnvironment = null;
            try
            {
                if (EnvironmentJson.Exists)
                {
                    using (var reader = new StreamReader(EnvironmentJson.OpenRead()))
                        portableEnvironment = PortableEnvironment.Load(reader);
                }

                if (portableEnvironment == null)
                {
                    portableEnvironment = new PortableEnvironment();
                }

                // Create ShellLink from the shortcut info.
                portableEnvironment.Shortcuts.Add(new Shortcut
                {
                    FileName = @"%PAppsStartMenuDir%\Find more applications.lnk",
                    Target = "http://compareason.com/",
                    IconPath = Assembly.GetExecutingAssembly().Location,
                    Description = "Find more applications",
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to restore the current environment (shortcuts, environement, registry...): " + ex.Message,
                    "PApps Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            return portableEnvironment;
        }

        public void TearDownPortableEnvironment()
        {
            // Save the current portable environment configuration.
            try
            {
                if (_portableEnvironment != null)
                    using (var writer = new StreamWriter(EnvironmentJson.OpenWrite()))
                        _portableEnvironment.Save(writer);
            }
            catch (Exception ex)
            {
                MessageBox.Show("WARNING: Failed to save the current environment (shortcuts, environement, registry...): " + ex.Message, "PApps Manager",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Restore original configuration.
            try
            {
                if (_portableEnvironment != null)
                    _portableEnvironment.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("WARNING: Failed to clean-up and restore the original configuration (shortcuts, environement, registry...): " + ex.Message, "PApps Manager",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}