using System;
using System.IO;

namespace PAppsManager.Core.Import
{
    internal class PortableAppsImporter : Importer
    {
        public PortableAppsImporter(DriveInfo drive)
            : base(GetDescription(drive))
        {
        }

        public static bool CanImport(DriveInfo drive)
        {
            return Directory.Exists(Path.Combine(drive.Name, "PortableApps"));
        }

        private static string GetDescription(DriveInfo drive)
        {
            int applicationCount = Directory.GetDirectories(Path.Combine(drive.Name, "PortableApps")).Length;
            return string.Format("Import {0} PortableApps applications on {1}", applicationCount, drive.Name);
        }

        protected override void PerformImport()
        {
            throw new NotImplementedException();
        }
    }
}