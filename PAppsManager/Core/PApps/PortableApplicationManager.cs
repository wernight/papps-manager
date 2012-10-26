using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
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
            get
            {
                string exeDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                if (exeDirectory == null)
                    throw new Exception("Unable to locate current executable.");
                return Path.Combine(exeDirectory, Settings.Default.PortableApplicationsBaseDirectory);
            }
        }

        public IEnumerable<PortableApplication> PortableApplications
        {
            get { return _applications; }
        }

        public bool IsAlreadyInstalled(PortableApplication application)
        {
            return _applications.Contains(application);
        }

        public void Install(PortableApplication application)
        {
            if (IsAlreadyInstalled(application))
                throw new Exception("Portable application already installed: " + application);

            foreach (PortableApplication dependency in application.Dependencies)
            {
                if (!IsAlreadyInstalled(dependency))
                    Install(dependency);
            }

            try
            {
                application.Validate();

                // Perform the installation
                var targetDirectory = Directory.CreateDirectory(Path.Combine(BaseDirectory, SafeFileName(application.Name)));
                application.InstallCommands.Execute(targetDirectory);

                _applications.Add(application);
                application.InstallCommands.CleanUp(true);
            }
            catch
            {
                application.InstallCommands.CleanUp(false);
                throw;
            }
        }

        /// <summary>
        /// Strip down any invalid character.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string SafeFileName([NotNull] string name)
        {
            return Path.GetInvalidFileNameChars().Aggregate(name, (current, ch) => current.Replace(ch, '_'));
        }
    }
}