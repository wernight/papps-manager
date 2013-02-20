using System;
using System.IO;
using System.Linq;
using PAppsManager.Core.PApps;
using PAppsManager.Core.PApps.Commands;
using PAppsManager.Properties;

namespace PAppsManager.Core.Import
{
    internal class LiberKeyImporter : Importer
    {
        private readonly string _description;
        private readonly DirectoryInfo[] _appsDirectores;
        private readonly DirectoryInfo[] _myAppsDirectores;
        private readonly PortableEnvironment _portableEnvironment;
        private readonly PortableApplication _portabilizer;

        public LiberKeyImporter(DriveInfo drive, PortableEnvironment portableEnvironment, Func<string, string> webClient)
        {
            _appsDirectores = new DirectoryInfo(Path.Combine(drive.Name, "Apps")).GetDirectories();
            _myAppsDirectores = new DirectoryInfo(Path.Combine(drive.Name, "MyApps")).GetDirectories();
            _portableEnvironment = portableEnvironment;
            _portabilizer = PortableApplication.LoadFromUrl(Resources.LibraryWebsiteBaseUrl + "/LiberKey/liberkey-portabilizer/", webClient);



            int applicationCount = _appsDirectores.Length + _myAppsDirectores.Length;
            _description = string.Format("Import {0} LiberKey applications on {1}", applicationCount, drive.Name);
        }

        public static bool CanImport(DriveInfo drive)
        {
            return Directory.Exists(Path.Combine(drive.Name, "LiberKeyTools"));
        }

        public override string Description
        {
            get { return _description; }
        }

        protected override void PerformImport()
        {
            var applications = _appsDirectores.Select(ImportLiberKeyApplication)
                                              .Concat(_myAppsDirectores.Select(ImportCustomApplication));
            foreach (PortableApplication application in applications)
            {
                _portableEnvironment.Applications.Add(application);
            }
        }

        private PortableApplication ImportLiberKeyApplication(DirectoryInfo directoryInfo)
        {
            // Import it manually like a custom application (won't support upgrades).
            return new PortableApplication
                {
                    Url = Resources.LibraryWebsiteBaseUrl + "/LiberKey/" + directoryInfo.Name.ToLowerInvariant() + "/",
                    Name = directoryInfo.Name,
                    Dependencies = new[]
                        {
                            _portabilizer
                        },
                    InstallCommands = new CommandList
                        {
                            new CopyCommand(directoryInfo)
                        }
                };
        }

        private static PortableApplication ImportCustomApplication(DirectoryInfo directoryInfo)
        {
            return new PortableApplication
                {
                    Name = directoryInfo.Name,
                    Dependencies = new PortableApplication[0],
                    InstallCommands = new CommandList
                        {
                            new CopyCommand(directoryInfo)
                        }
                };
        }
    }
}