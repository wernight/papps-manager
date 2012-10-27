using System.IO;
using PAppsManager.Core.PApps;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps.Commands
{
    internal class DummyCommand : ICommand
    {
        public string Validate()
        {
            return null;
        }

        public void Execute(DirectoryInfo targetDirectory, PortableEnvironment portableEnvironment)
        {
        }

        public void CleanUp(bool successful)
        {
        }
    }
}