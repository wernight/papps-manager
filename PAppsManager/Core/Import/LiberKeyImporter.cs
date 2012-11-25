using System;
using System.IO;

namespace PAppsManager.Core.Import
{
    internal class LiberKeyImporter : Importer
    {
        public LiberKeyImporter(DriveInfo drive) :
            base(GetDescription(drive))
        {
        }

        public static bool CanImport(DriveInfo drive)
        {
            return Directory.Exists(Path.Combine(drive.Name, "LiberKeyTools"));
        }

        private static string GetDescription(DriveInfo drive)
        {
            int applicationCount = Directory.GetDirectories(Path.Combine(drive.Name, "Apps")).Length;
            return string.Format("Import {0} LiberKey applications on {1}", applicationCount, drive.Name);
        }

        protected override void PerformImport()
        {
            throw new NotImplementedException();
        }
    }
}