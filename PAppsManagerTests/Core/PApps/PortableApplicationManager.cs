using NUnit.Framework;
using PAppsManager.Core.PApps;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps
{
    [TestFixture]
    public class PortableApplicationManagerTest : AssertionHelper
    {
        private readonly PortableApplicationManager _manager = new PortableApplicationManager();

        [Test]
        public void CannotInstallAlreadyInstalledApplication()
        {
            var app = new PortableApplication
                          {
                              InstallCommands = new CommandList()
                          };

            _manager.Install(app);
            Expect(() => _manager.Install(app), Throws.Exception);
        }
    }
}