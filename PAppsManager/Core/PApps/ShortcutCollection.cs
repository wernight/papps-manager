using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using PAppsManager.Properties;
using vbAccelerator.Components.Shell;

namespace PAppsManager.Core.PApps
{
    internal class ShortcutCollection : ICollection<Shortcut>
    {
        private readonly PortableEnvironment _environment;

        public ShortcutCollection(PortableEnvironment environment)
        {
            _environment = environment;
        }

        public string StartMenuTargetDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                                    Settings.Default.StartMenuDirectoryName);
            }
        }

        #region ICollection<Shortcut> Members

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int Count
        {
            get
            {
                int count = 0;
                IEnumerator<Shortcut> enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                    ++count;
                return count;
            }
        }

        public IEnumerator<Shortcut> GetEnumerator()
        {
            // Helper: Create shortcut from a ShellLink.
            var environmentVariables = GetEnvironmentVariables();
            var makeShortcut = new Func<ShellLink, Shortcut>(
                link => new Shortcut
                            {
                                FileName = environmentVariables.Contract(link.ShortCutFile),
                                Target = environmentVariables.Contract(link.Target),
                                Arguments = environmentVariables.Contract(link.Arguments),
                                WorkingDirectory = environmentVariables.Contract(link.WorkingDirectory),
                                IconPath = environmentVariables.Contract(link.IconPath),
                                DisplayMode = link.DisplayMode,
                                Description = link.Description,
                            });

            // Look in the start menu.
            if (Directory.Exists(StartMenuTargetDirectory))
                foreach (string linkFileName in Directory.EnumerateFiles(StartMenuTargetDirectory, "*.lnk", SearchOption.AllDirectories))
                {
                    using (var shellLink = new ShellLink(linkFileName))
                    {
                        if (shellLink.ShortCutFile.StartsWith(_environment.Applications.InstallationBaseDirectory))
                            yield return makeShortcut(shellLink);
                    }
                }

            // Look on the desktop.
            var desktopDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            foreach (FileInfo linkFile in desktopDirectory.EnumerateFiles("*.lnk", SearchOption.TopDirectoryOnly))
            {
                using (var shellLink = new ShellLink(linkFile.FullName))
                {
                    if (shellLink.ShortCutFile.StartsWith(_environment.Applications.InstallationBaseDirectory))
                        yield return makeShortcut(shellLink);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add([NotNull] Shortcut item)
        {
            if (item == null)
                throw new ArgumentNullException("item", "Shortcut cannot be null.");
            if (string.IsNullOrWhiteSpace(item.FileName))
                throw new ArgumentException("Shortcut file name not defined or empty.", "item");
            if (string.IsNullOrWhiteSpace(item.Target))
                throw new ArgumentException("Shortcut target not defined or empty.", "item");

            // Create the folder if not already present.
            EnvironmentVariables environmentVariables = GetEnvironmentVariables();
            var linkFile = new FileInfo(environmentVariables.Expand(item.FileName));
            if (linkFile.Directory != null)
                linkFile.Directory.Create();

            // Create the link.
            using (var shellLink = new ShellLink())
            {
                shellLink.Target = environmentVariables.Expand(item.Target);
                if (!string.IsNullOrWhiteSpace(item.Arguments))
                    shellLink.Arguments = environmentVariables.Expand(item.Arguments);
                if (!string.IsNullOrWhiteSpace(item.WorkingDirectory))
                    shellLink.WorkingDirectory = environmentVariables.Expand(item.WorkingDirectory);
                if (!string.IsNullOrWhiteSpace(item.IconPath))
                    shellLink.IconPath = environmentVariables.Expand(item.IconPath);
                else if (string.Compare(Path.GetExtension(shellLink.Target), ".exe", StringComparison.InvariantCultureIgnoreCase) == 0)
                    shellLink.IconPath = shellLink.Target;
                shellLink.DisplayMode = item.DisplayMode;
                if (!string.IsNullOrWhiteSpace(item.Description))
                    shellLink.Description = item.Description;

                shellLink.Save(linkFile.FullName);
            }
        }

        public void Clear()
        {
            foreach (Shortcut shortcut in this)
            {
                Remove(shortcut);
            }

            if (Directory.Exists(StartMenuTargetDirectory))
                Directory.Delete(StartMenuTargetDirectory, true);
        }

        public bool Contains(Shortcut item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Shortcut[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove([CanBeNull] Shortcut item)
        {
            if (item == null)
                return false;

            // Shortcut exists?
            EnvironmentVariables environmentVariables = GetEnvironmentVariables();
            var linkFile = new FileInfo(environmentVariables.Expand(item.FileName));
            if (!linkFile.Exists)
                return false;

            // Delete the shortcut.
            linkFile.Delete();

            // Delete the directory if it's empty.
            DirectoryInfo directoryInfo = linkFile.Directory;
            while (directoryInfo != null && !directoryInfo.GetFiles().Any() && !directoryInfo.GetDirectories().Any())
            {
                directoryInfo.Delete();
                directoryInfo = directoryInfo.Parent;
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Environment variables used to save relative path so that we can portabilize shortcuts accross machines.
        /// </summary>
        /// <returns></returns>
        private EnvironmentVariables GetEnvironmentVariables()
        {
            // Note: We use Path.GetFullPath() also to normalize the path (like always use \ on Windows)
            // for later string comparison.
            var environmentVariables = new EnvironmentVariables();
            environmentVariables.Add("PAppsBaseDir", Path.GetFullPath(_environment.Applications.InstallationBaseDirectory));
            environmentVariables.Add("PAppsStartMenuDir", Path.GetFullPath(StartMenuTargetDirectory));
            return environmentVariables;
        }
    }
}