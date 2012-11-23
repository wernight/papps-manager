using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PAppsManager.Properties;

namespace PAppsManager.Core.PApps
{
    internal class PortableApplicationCollection : ICollection<PortableApplication>
    {
        private const string ApplicationInfoFileName = ".papps-manager.json";
        private readonly HashSet<PortableApplication> _applications = new HashSet<PortableApplication>();
        private readonly PortableEnvironment _environment;

        public PortableApplicationCollection(PortableEnvironment environment)
        {
            _environment = environment;

            // Load a list of installed applications.
            if (Directory.Exists(InstallationBaseDirectory))
            {
                foreach (string directory in Directory.GetDirectories(InstallationBaseDirectory))
                {
                    string infoFileName = Path.Combine(directory, ApplicationInfoFileName);
                    if (File.Exists(infoFileName))
                    {
                        string json = File.ReadAllText(infoFileName);
                        var portableApplication = JsonConvert.DeserializeObject<PortableApplication>(json);
                        portableApplication.InstallDirectory = Path.GetFullPath(directory);
                        _applications.Add(portableApplication);
                    }
                }
            }
        }

        /// <summary>
        /// Folder under whitch the applications will be installed by default.
        /// All portable applications should be below this path.
        /// </summary>
        [JsonIgnore]
        public string InstallationBaseDirectory
        {
            get
            {
                string exeDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                if (exeDirectory != null)
                    exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (exeDirectory == null)
                    throw new Exception("Couldn't locate current application's directory.");

                return Path.Combine(exeDirectory, Settings.Default.PortableApplicationsBaseDirectory);
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

        /// <summary>
        /// Install, upgrade, downgrade or reinstall an application.
        /// </summary>
        /// <param name="item"></param>
        public void Add([NotNull] PortableApplication item)
        {
            // Check the argument.
            if (item == null)
                throw new ArgumentNullException("item");
            item.Validate();
            Debug.Assert(item.Dependencies != null);
            Debug.Assert(item.InstallCommands != null);

            // Get the existing instance it's already installed.
            PortableApplication existingInstance = _applications.FirstOrDefault(x => Equals(x, item));
            Debug.Assert((existingInstance != null) == Contains(item));

            // Install all dependencies.
            foreach (PortableApplication dependency in item.Dependencies)
            {
                if (!Contains(dependency))
                    Add(dependency);
            }

            // Backup and get install directory.
            if (existingInstance == null)
            {
                item.InstallDirectory = GetDefaultTargetDirectory(item);
                item.PreviousVersionInstallDirectory = null;
            }
            else
            {
                // Get the target directory.
                item.InstallDirectory = existingInstance.InstallDirectory ?? GetDefaultTargetDirectory(item);

                // Delete previous backup.
                if (existingInstance.PreviousVersionInstallDirectory != null &&
                    Directory.Exists(existingInstance.PreviousVersionInstallDirectory) &&
                    Path.GetFullPath(item.InstallDirectory) == Path.GetFullPath(existingInstance.PreviousVersionInstallDirectory))
                    Directory.Delete(existingInstance.PreviousVersionInstallDirectory, true);

                // Get the backup directory.
                item.PreviousVersionInstallDirectory = item.InstallDirectory + ".bak";
                if (Directory.Exists(existingInstance.PreviousVersionInstallDirectory))
                    Directory.Delete(existingInstance.PreviousVersionInstallDirectory, true);

                if (Directory.Exists(item.InstallDirectory))
                {
                    // Backup the application.
                    try
                    {
                        Directory.Move(item.InstallDirectory, item.PreviousVersionInstallDirectory);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to backup the existing version: " + ex.Message, ex);
                    }

                    // Migrate the user data (but since we don't know which files we cope them all before the install).
                    if (string.IsNullOrWhiteSpace(item.DataDirectory))
                        CopyDirectory(item.PreviousVersionInstallDirectory,
                                      item.InstallDirectory);
                }
            }

            // Install the new version.
            try
            {
                // Perform the installation.
                if (Directory.Exists(item.InstallDirectory) && !string.IsNullOrWhiteSpace(item.DataDirectory))
                    // Shouldn't happen but for safety measure...
                    throw new Exception("Application installation directory shouldn't exist at this point.");
                DirectoryInfo targetDirectoryInfo = Directory.CreateDirectory(item.InstallDirectory);
                item.InstallCommands.Execute(targetDirectoryInfo, _environment);

                // Migrate the user data.
                if (!string.IsNullOrWhiteSpace(item.DataDirectory) && item.PreviousVersionInstallDirectory != null)
                {
                    string source = Path.Combine(item.PreviousVersionInstallDirectory, item.DataDirectory);
                    string destination = Path.Combine(item.InstallDirectory, item.DataDirectory);
                    if (Directory.Exists(source))
                        CopyDirectory(source, destination);
                }

                // Mark this application has having been installed.
                using (var writer = new StreamWriter(Path.Combine(item.InstallDirectory, ApplicationInfoFileName)))
                    writer.Write(item.ToJson());
                if (existingInstance != null)
                    _applications.Remove(existingInstance);
                _applications.Add(item);

                item.InstallCommands.CleanUp(true);
            }
            catch (Exception ex)
            {
                // Clean-up partially done work.
                try
                {
                    // Delete partial installation folder.
                    item.InstallCommands.CleanUp(false);
                    if (item.InstallDirectory != null)
                    {
                        if (Directory.Exists(item.InstallDirectory))
                            Directory.Delete(item.InstallDirectory, true);

                        // Restore the previous version, if any.
                        if (item.PreviousVersionInstallDirectory != null &&
                            Directory.Exists(item.PreviousVersionInstallDirectory))
                            Directory.Move(item.PreviousVersionInstallDirectory, item.InstallDirectory);
                    }
                }
                catch
                {
                    // Swallow the exception.
                }
                throw new Exception("Installation of " + item.Name + " failed: " + ex.Message, ex);
            }
        }

        private static void CopyDirectory(string source, string destination)
        {
            // Create this folder and copy files in this folder.
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source, "*.*"))
            {
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            }

            // Recurse
            foreach (var folder in Directory.GetDirectories(source))
            {
                CopyDirectory(folder, Path.Combine(destination, Path.GetFileName(folder)));
            }
        }

        public void Clear()
        {
            var applications = new List<PortableApplication>(_applications);
            foreach (PortableApplication application in applications)
            {
                Remove(application);
            }

            Debug.Assert(_applications.Count == 0);
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
            // The application should exist.
            if (!Contains(item))
                return false;

            // Delete that folder and the backup folder.
            if (item.PreviousVersionInstallDirectory != null && Directory.Exists(item.PreviousVersionInstallDirectory))
                Directory.Delete(item.PreviousVersionInstallDirectory, true);
            if (item.InstallDirectory != null && Directory.Exists(item.InstallDirectory))
                Directory.Delete(item.InstallDirectory, true);

            return _applications.Remove(item);
        }

        #endregion

        public IEnumerable<PortableApplication> Updates(string updateUrl)
        {
            // No applications to check?
            if (Count == 0)
                return Enumerable.Empty<PortableApplication>();

            // Send a list of the current version of each application.
            Dictionary<string, DateTime> requestDictionary = this.ToDictionary(app => app.Url, app => app.ReleaseDate);
            var jsonRequest = JsonConvert.SerializeObject(requestDictionary);

            Dictionary<string, PortableApplication> updates;
            using (var webClient = new WebClient())
            {
                byte[] responseData = webClient.UploadData(updateUrl, Encoding.UTF8.GetBytes(jsonRequest));
                var jsonResponse = Encoding.UTF8.GetString(responseData);
                updates = JsonConvert.DeserializeObject<Dictionary<string, PortableApplication>>(jsonResponse);
            }

            // Set the URL.
            foreach (KeyValuePair<string, PortableApplication> portableApplication in updates)
            {
                portableApplication.Value.Url = portableApplication.Key;
            }

            // For each application that can be upgraded.
            return updates.Values;
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

        [NotNull]
        private string GetDefaultTargetDirectory(PortableApplication item)
        {
            var targetDirectory = new DirectoryInfo(Path.Combine(InstallationBaseDirectory, SafeFileName(item.Name)));
            if (!targetDirectory.Exists)
                return targetDirectory.FullName;

            for (int i = 2; i < 100; ++i)
            {
                var targetDirectory2 = new DirectoryInfo(targetDirectory.FullName + " (" + i + ")");
                if (!targetDirectory2.Exists)
                    return targetDirectory2.FullName;
            }

            // This exception should really be exceptionally rare.
            throw new Exception(
                "Another appliction is already installed in that folder and no alternative name could be found.");
        }
    }
}