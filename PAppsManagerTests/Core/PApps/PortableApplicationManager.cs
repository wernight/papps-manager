using System;
using System.IO;
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
                              Url = "http://example.com/",
                              Name = "UnitTest",
                              Version = "1.0.0.0",
                              ReleaseDate = new DateTime(2000, 1, 1),
                              Dependencies = new PortableApplication[0],
                              InstallCommands = new CommandList { new DummyCommand() },
                          };

            _manager.Install(app);
            Expect(() => _manager.Install(app), Throws.Exception);
        }

        private class DummyCommand : ICommand
        {
            public string Validate()
            {
                return null;
            }

            public void Execute(DirectoryInfo targetDirectory)
            {
            }

            public void CleanUp(bool successful)
            {
            }
        }
    }
}