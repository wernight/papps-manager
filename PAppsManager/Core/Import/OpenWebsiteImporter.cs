using System.Diagnostics;

namespace PAppsManager.Core.Import
{
    internal class OpenWebsiteImporter : Importer
    {
        public override string Description
        {
            get { return "Discover applications on CompaReason.com"; }
        }

        protected override void PerformImport()
        {
            Process.Start("http://compareason.com");
        }
    }
}