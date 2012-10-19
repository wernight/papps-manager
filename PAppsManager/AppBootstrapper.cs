using System;
using System.Reflection;
using System.ServiceModel;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using PAppsManager.Core;
using PAppsManager.Core.PApps;
using PAppsManager.Properties;
using PAppsManager.ViewModels;

namespace PAppsManager
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AppBootstrapper : AutofacBootstrapper<MainWindowViewModel>, ISingleInstanceApp
    {
        private SingleInstance<ISingleInstanceApp> _singleInstance;

        private static string ProductName
        {
            get { return Assembly.GetExecutingAssembly().GetName().Name; }
        }

        #region ISingleInstanceApp Members

        /// <summary>
        /// Call by other instances like when the URL Protocol is used.
        /// </summary>
        /// <param name="args">Command line arguments passed to the other instance of this application.</param>
        public void SignalExternalCommandLineArgs(string[] args)
        {
            ProcessCommandLineArguments(args);
        }

        #endregion

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
            _singleInstance = new SingleInstance<ISingleInstanceApp>(this, ProductName);

            // Process the command line.
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (_singleInstance.IsAlreadyRunning)
                    _singleInstance.AlreadyRunningApplication.SignalExternalCommandLineArgs(args);
                else
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
            UrlProtocol.Associate("papps", exePath);

            base.OnStartup(sender, e);
        }

        private void ProcessCommandLineArguments(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Invalid command line arguments.");
            string url = args[1];

            IoC.Get<PortableApplicationManager>().Install(PortableApplication.LoadFromUrl(url));
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            // Remove the papps:// URL protocol handler.
            UrlProtocol.Disassociate("papps");

            // Allow opening another instance of this application.
            _singleInstance.Dispose();

            base.OnExit(sender, e);
        }
    }
}