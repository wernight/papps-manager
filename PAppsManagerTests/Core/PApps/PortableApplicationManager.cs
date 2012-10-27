using System;
using NUnit.Framework;
using PAppsManager.Core.PApps;
using PAppsManager.Core.PApps.Commands;
using PAppsManagerTests.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps
{
    [TestFixture]
    public class PortableApplicationManagerTest : AssertionHelper
    {
        private readonly PortableEnvironment _environment = new PortableEnvironment();

        [Test]
        public void CannotInstallAlreadyInstalledApplication()
        {
            var app = new PortableApplication
                          {
                              Url = "http://example.com/",
                              Name = "UnitTest",
                              Version = "1.0.0.0",
                              ReleaseDate = new DateTime(2000, 1, 1),
                              Dependencies = new PortableApplication[0],
                              InstallCommands = new CommandList { new DummyCommand() },
                          };

            _environment.Applications.Add(app);
            Expect(() => _environment.Applications.Add(app), Throws.Exception);
        }
    }
}