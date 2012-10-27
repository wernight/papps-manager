using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using PAppsManager.Core;
using PAppsManager.Core.PApps;
using PAppsManager.Core.SingleInstance;
using PAppsManager.Properties;
using PAppsManager.ViewModels;

namespace PAppsManager
{
    internal class AppBootstrapper : AutofacBootstrapper<MainWindowViewModel>
    {
        private static readonly FileInfo EnvironmentJson = new FileInfo("Environment.json");
        private SingleInstance _singleInstance;
        private PortableEnvironment _portableEnvironment;

        private static string ProductName
        {
            get { return Assembly.GetExecutingAssembly().GetName().Name; }
        }

        /// <summary>
        /// Override to include your own Autofac configuration after the framework has finished its configuration, but 
        /// before the container is created.
        /// </summary>
        /// <param name="builder">The Autofac configuration builder.</param>
        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            // Set up the portable environment.
            _portableEnvironment = SetUpPortableEnvironment();

            builder.RegisterInstance(_portableEnvironment);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            _singleInstance = new SingleInstance(ProductName, otherInstanceArgs => Execute.OnUIThread(() => ProcessCommandLineArguments(otherInstanceArgs)));
            
            // Process the command line.
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (_singleInstance.IsAlreadyRunning)
                {
                    _singleInstance.SignalExternalCommandLineArgs(args);
                    Application.Shutdown();
                    return;
                }
                
                ProcessCommandLineArguments(args);
            }

            // Single instance of the applicaiton allowed.
            if (_singleInstance.IsAlreadyRunning)
            {
                MessageBox.Show(string.Format(Resources.ApplicationAlreadyRunning, ProductName), ProductName,
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);

                // Terminate this instance.
                Application.Shutdown();
                return;
            }

            // Associate the papps:// URL protocol handler.
            try
            {
                string exePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
                UrlProtocol.Associate("papp", exePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to associate the PApps Manager with the papp:// protocol: " + ex.Message,
                                "PApps Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            base.OnStartup(sender, e);
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

        private void ProcessCommandLineArguments(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Invalid command line arguments.");
            string url = args[1];

            IoC.Get<MainWindowViewModel>().InstallApplication(url);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            if (!_singleInstance.IsAlreadyRunning)
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

                // Remove the papps:// URL protocol handler.
                UrlProtocol.Disassociate("papp");

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

            // Allow opening another instance of this application.
            _singleInstance.Dispose();

            base.OnExit(sender, e);
        }
    }
}