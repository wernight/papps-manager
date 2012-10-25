using System;
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
        private SingleInstance _singleInstance;

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
            builder.RegisterInstance(new PortableApplicationManager());
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
            string exePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            UrlProtocol.Associate("papp", exePath);

            base.OnStartup(sender, e);
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
                // Remove the papps:// URL protocol handler.
                UrlProtocol.Disassociate("papp");
            }

            // Allow opening another instance of this application.
            _singleInstance.Dispose();

            base.OnExit(sender, e);
        }
    }
}