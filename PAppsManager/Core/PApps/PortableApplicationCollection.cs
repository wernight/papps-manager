using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PAppsManager.Properties;

namespace PAppsManager.Core.PApps
{
    internal class PortableApplicationCollection : ICollection<PortableApplication>
    {
        private readonly HashSet<PortableApplication> _applications = new HashSet<PortableApplication>();
        private readonly PortableEnvironment _environment;

        public PortableApplicationCollection(PortableEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// Folder under whitch the applications will be installed by default.
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo DefaultInstallationDirectory
        {
            get
            {
                return new DirectoryInfo(Path.Combine(BaseDirectory.FullName,
                                                      Settings.Default.PortableApplicationsDefaultInstallDirectory));
            }
        }

        /// <summary>
        /// Folder under whitch all applications should be installed (no portable application should be above this path).
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo BaseDirectory
        {
            get
            {
                var exeDirectory = Directory.CreateDirectory(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath));
                if (exeDirectory == null)
                    throw new Exception("Unable to locate current executable.");
                return exeDirectory;
            }
        }

        #region ICollection<PortableApplication> Members

        public int Count
        {
            get { return _applications.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<PortableApplication> GetEnumerator()
        {
            return _applications.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(PortableApplication item)
        {
            if (Contains(item))
                throw new Exception("Portable application already installed: " + item);

            foreach (PortableApplication dependency in item.Dependencies)
            {
                if (!Contains(dependency))
                    Add(dependency);
            }

            DirectoryInfo targetDirectory = null;
            try
            {
                item.Validate();

                // Perform the installation
                targetDirectory = Directory.CreateDirectory(
                    Path.Combine(DefaultInstallationDirectory.FullName,
                                 SafeFileName(item.Name)));
                item.InstallCommands.Execute(targetDirectory, _environment);

                _applications.Add(item);
                item.InstallCommands.CleanUp(true);
            }
            catch (Exception ex)
            {
                // Clean-up partially done work.
                try
                {
                    item.InstallCommands.CleanUp(false);
                    if (targetDirectory != null && targetDirectory.Exists)
                        targetDirectory.Delete(true);
                }
                catch
                {
                    // Swallow the exception.
                }
                throw new Exception("Installation of " + item.Name + " failed: " + ex.Message, ex);
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(PortableApplication item)
        {
            return _applications.Contains(item);
        }

        public void CopyTo(PortableApplication[] array, int arrayIndex)
        {
            _applications.CopyTo(array, arrayIndex);
        }

        public bool Remove(PortableApplication item)
        {
            throw new NotImplementedException();
        }

        #endregion

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