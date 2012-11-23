using System;
using System.IO;
using Moq;
using NUnit.Framework;
using PAppsManager.Core.PApps;
using PAppsManager.Core.PApps.Commands;
using PAppsManager.Properties;

namespace PAppsManagerTests.Core.PApps
{
    [TestFixture]
    public class PortableApplicationCollectionTest : AssertionHelper
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            // Create a new empty list of applications.
            Settings.Default.PortableApplicationsBaseDirectory = Path.Combine(Path.GetTempPath(), "Applications-UnitTests-Temp-" + Randomizer.RandomSeed);
            _applications = new PortableEnvironment().Applications;
            _applications.Clear();
            Expect(_applications.Count, Is.EqualTo(0));

            if (Directory.Exists(_applications.InstallationBaseDirectory))
                Directory.Delete(_applications.InstallationBaseDirectory, true);
            Directory.CreateDirectory(_applications.InstallationBaseDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (_applications != null && _applications.InstallationBaseDirectory != null &&
                Directory.Exists(_applications.InstallationBaseDirectory))
                Directory.Delete(_applications.InstallationBaseDirectory, true);
        }

        #endregion

        private PortableApplicationCollection _applications;

        private PortableApplication GetSampleApplication()
        {
            // Set up with a mock command.
            var mockCommand = new Mock<ICommand>();

            mockCommand.Setup(cmd => cmd.Execute(It.IsAny<DirectoryInfo>(), It.IsAny<PortableEnvironment>()))
                .Callback<DirectoryInfo, PortableEnvironment>((targetDir, env) => _lastInstallTargetDirectory = targetDir);

            return new PortableApplication
                {
                    Url = "http://example.com/",
                    Name = "UnitTest",
                    Version = "1.0.0.0",
                    ReleaseDate = new DateTime(2000, 1, 1),
                    DataDirectory = "Data",
                    Dependencies = new PortableApplication[0],
                    InstallCommands = new CommandList {mockCommand.Object},
                };
        }

        private DirectoryInfo _lastInstallTargetDirectory;

        [Test]
        public void CanUpgradeOrReinstallAlreadyInstalledApplications()
        {
            // Sample application.
            PortableApplication app = GetSampleApplication();
            Expect(Directory.GetDirectories(_applications.InstallationBaseDirectory), Has.Length.EqualTo(0));

            // First install.
            _applications.Add(app);
            Expect(app.InstallDirectory, Is.EqualTo(_lastInstallTargetDirectory.FullName));
            Expect(app.PreviousVersionInstallDirectory, Is.Null);
            Expect(Directory.GetDirectories(_applications.InstallationBaseDirectory), Has.Length.EqualTo(1));

            // Can upgrade/downgrade/reinstall.
            app = GetSampleApplication();
            app.Name = "new name";
            app.ReleaseDate += new TimeSpan(1, 0, 0, 0);
            app.Version = "2.0";
            _applications.Add(app);
            Expect(app.InstallDirectory, Is.EqualTo(_lastInstallTargetDirectory.FullName));
            Expect(app.PreviousVersionInstallDirectory, Is.Not.Null);
            Expect(app.PreviousVersionInstallDirectory, Is.Not.EqualTo(app.InstallDirectory));

            // Check that a backup has been done.
            Expect(Directory.GetDirectories(_applications.InstallationBaseDirectory), Has.Length.EqualTo(2));

            // Do another reinstall with the exact same object instance.
            _applications.Add(app);
            Expect(app.InstallDirectory, Is.EqualTo(_lastInstallTargetDirectory.FullName));
            Expect(app.PreviousVersionInstallDirectory, Is.Not.Null);
            Expect(app.PreviousVersionInstallDirectory, Is.Not.EqualTo(app.InstallDirectory));
            Expect(Directory.GetDirectories(_applications.InstallationBaseDirectory), Has.Length.EqualTo(2));
        }

        [Test]
        public void UpgradeCopiesPreviousData([Values("", "Da/t\\a")] string dataDirectory)
        {
            // Sample application.
            PortableApplication app = GetSampleApplication();
            app.DataDirectory = dataDirectory;
            Expect(Directory.GetDirectories(_applications.InstallationBaseDirectory), Has.Length.EqualTo(0));

            // Install command that will create a setting file.
            var relativeSettingFiles = new[]
                                           {
                                               Path.Combine(app.DataDirectory, "Foo.ini"),
                                               Path.Combine(app.DataDirectory, "Bar.ini"),
                                           };

            var mockCommand = new Mock<ICommand>();
            mockCommand.Setup(cmd => cmd.Execute(It.IsAny<DirectoryInfo>(), It.IsAny<PortableEnvironment>()))
                .Callback<DirectoryInfo, PortableEnvironment>(
                    (targetDir, env) =>
                        {
                            // Create a default setting file content for "Foo.ini".
                            string settingFile = Path.Combine(targetDir.FullName, relativeSettingFiles[0]);
                            Directory.CreateDirectory(Path.GetDirectoryName(settingFile));
                            using (var writer = new StreamWriter(settingFile))
                                writer.Write("default settings");
                        });
            app.InstallCommands.Add(mockCommand.Object);

            // First install.
            _applications.Add(app);

            // Change the default setting file content.
            foreach (string relativeSettingFile in relativeSettingFiles)
            {
                using (var writer = new StreamWriter(Path.Combine(_lastInstallTargetDirectory.FullName, relativeSettingFile)))
                    writer.Write("new settings");
            }

            // Upgrade it.
            _applications.Add(app);

            // Check that we the user data has been migrated.
            foreach (string relativeSettingFile in relativeSettingFiles)
            {
                // Backup should keep their settings.
                Expect(File.Exists(Path.Combine(app.PreviousVersionInstallDirectory, relativeSettingFile)));

                // New version should have the settings migrated.
                Expect(File.Exists(Path.Combine(app.InstallDirectory, relativeSettingFile)));
            }

            // Check that the default settings values have been overwritten by the user settings.
            if (string.IsNullOrWhiteSpace(dataDirectory))
                Expect(File.ReadAllText(Path.Combine(app.InstallDirectory, relativeSettingFiles[0])), Is.EqualTo("default settings"));
            else
                Expect(File.ReadAllText(Path.Combine(app.InstallDirectory, relativeSettingFiles[0])), Is.EqualTo("new settings"));
            Expect(File.ReadAllText(Path.Combine(app.InstallDirectory, relativeSettingFiles[1])), Is.EqualTo("new settings"));
        }

        [Test]
        public void ContainsApplicationUniqueByUrl()
        {
            PortableApplication app = GetSampleApplication();
            _applications.Add(app);
            Expect(_applications.Contains(app));

            app = GetSampleApplication();
            Expect(_applications.Contains(app));

            app.Name = "x";
            app.Version = "x";
            app.ReleaseDate = new DateTime(1999, 1, 1);
            app.InstallCommands = new CommandList();
            Expect(_applications.Contains(app));

            app.Url += "x";
            Expect(_applications.Contains(app), Is.False);
        }

        [Test(Description = "Should check that if the target directory already exists, then another target directory is offered unless it's the same application.")]
        public void InstallTargetDirectoryMustNotExist()
        {
            PortableApplication app = GetSampleApplication();

            // First to get the default directory.
            _applications.Add(app);
            Expect(_lastInstallTargetDirectory, Is.Not.Null);
            var defaultInstallTargetDirectory = _lastInstallTargetDirectory;

            // Remove it.
            _applications.Remove(app);
            Expect(_lastInstallTargetDirectory.Exists, Is.False);

            // Create a dummy application in that folder.
            _lastInstallTargetDirectory.Create();
            using (new FileStream(Path.Combine(_lastInstallTargetDirectory.FullName, "Foo.exe"), FileMode.CreateNew))
            {
            }

            // Installing the application now should use another folder.
            _applications.Add(app);
            Expect(_lastInstallTargetDirectory, Is.Not.EqualTo(defaultInstallTargetDirectory));
        }

        [Test(Description = "Failing an install or an upgrade, rollsback the changes")]
        public void InstallFailureRollbacksChanges([Values("", "Da/t\\a")] string dataDirectory)
        {
            // Sample application.
            PortableApplication app = GetSampleApplication();
            app.DataDirectory = dataDirectory;

            // First install.
            _applications.Add(app);
            
            // Remember the application's properties
            string installDirectory = app.InstallDirectory;

            // Save some settings
            string settingFilePath = Path.Combine(app.InstallDirectory, Path.Combine(app.DataDirectory, "Foo.ini"));
            Directory.CreateDirectory(Path.GetDirectoryName(settingFilePath));
            using (var writer = new StreamWriter(settingFilePath))
                writer.Write("something");

            // Upgrade but simulate a failure during the process.
            var mockCommand = new Mock<ICommand>();
            mockCommand.Setup(x => x.Execute(It.IsAny<DirectoryInfo>(), It.IsAny<PortableEnvironment>()))
                .Callback(() => { throw new Exception("test"); });
            app.InstallCommands = new CommandList { mockCommand.Object };
            Expect(() => _applications.Add(app), Throws.Exception);

            // The original files should still be there as if the upgrade never happened.
            Expect(File.ReadAllText(settingFilePath), Is.EqualTo("something"));

            // Application's properties should remain the same.
            Expect(app.InstallDirectory, Is.EqualTo(installDirectory));
        }
    }
}