using System.Diagnostics;

namespace PAppsManager.Core.Import
{
    internal class OpenWebsiteImporter : Importer
    {
        public OpenWebsiteImporter()
            : base("Discover applications on CompaReason.com")
        {
        }

        protected override void PerformImport()
        {
            Process.Start("http://compareason.com");
        }
    }
}