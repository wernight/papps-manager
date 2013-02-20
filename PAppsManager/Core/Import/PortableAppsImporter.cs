using System;
using System.IO;
using System.Linq;
using PAppsManager.Core.PApps;
using PAppsManager.Core.PApps.Commands;
using PAppsManager.Properties;

namespace PAppsManager.Core.Import
{
    internal class PortableAppsImporter : Importer
    {
        private readonly DirectoryInfo[] _portableAppsDirectories;
        private readonly PortableEnvironment _portableEnvironment;
        private readonly string _description;

        public PortableAppsImporter(DriveInfo drive, PortableEnvironment portableEnvironment)
        {
            _portableAppsDirectories = new DirectoryInfo(Path.Combine(drive.Name, "PortableApps")).GetDirectories();
            _portableEnvironment = portableEnvironment;

            _description = string.Format("Import {0} PortableApps applications on {1}", _portableAppsDirectories.Length, drive.Name);
        }

        public static bool CanImport(DriveInfo drive)
        {
            return PortableAppsDirectory(drive).Exists;
        }

        public override string Description
        {
            get { return _description; }
        }

        protected override void PerformImport()
        {
            foreach (PortableApplication application in _portableAppsDirectories.Select(Import))
            {
                _portableEnvironment.Applications.Add(application);
            }
        }

        private PortableApplication Import(DirectoryInfo directoryInfo)
        {
            // Import it manually like a custom application (won't support upgrades).
            return new PortableApplication
                {
                    Url = Resources.LibraryWebsiteBaseUrl + "/PortableApps.com/" + directoryInfo.Name.ToLowerInvariant() + "/",
                    Name = directoryInfo.Name,
                    DataDirectory = "Data",
                    Dependencies = new PortableApplication[0],
                    InstallCommands = new CommandList
                        {
                            new CopyCommand(directoryInfo)
                        }
                };
        }

        private static DirectoryInfo PortableAppsDirectory(DriveInfo drive)
        {
            return new DirectoryInfo(Path.Combine(drive.Name, "PortableApps"));
        }
    }
}