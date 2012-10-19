using System;
using System.Collections.Generic;
using PAppsManager.Properties;

namespace PAppsManager.Core.PApps
{
    internal class PortableApplicationManager
    {
        private readonly HashSet<PortableApplication> _applications = new HashSet<PortableApplication>();

        /// <summary>
        /// Folder under whitch the applications will be installed.
        /// </summary>
        public string BaseDirectory
        {
            get { return Settings.Default.PortableApplicationsBaseDirectory; }
        }

        public IEnumerable<PortableApplication> PortableApplications
        {
            get { return _applications; }
        }

        public void Install(PortableApplication application)
        {
            if (_applications.Contains(application))
                throw new Exception("Portable application already installed: " + application);
            application.InstallCommands.Validate();
            application.InstallCommands.Execute();
            _applications.Add(application);
        }
    }
}