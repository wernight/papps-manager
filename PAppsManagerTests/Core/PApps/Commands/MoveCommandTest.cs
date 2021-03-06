﻿using System.IO;
using NUnit.Framework;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps.Commands
{
    [TestFixture]
    public class MoveCommandTest : AssertionHelper
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _targetDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.Combine("PAppsManagerUnitTests/MoveCommandTest", Path.GetRandomFileName())));

            Directory.CreateDirectory(Path.Combine(_targetDirectory.FullName, "a1/b1"));
            File.Create(Path.Combine(_targetDirectory.FullName, "a1/file1")).Dispose();
            File.Create(Path.Combine(_targetDirectory.FullName, "a1/b1/file2")).Dispose();

            Directory.CreateDirectory(Path.Combine(_targetDirectory.FullName, "a2"));
        }

        [TearDown]
        public void TearDown()
        {
            if (_targetDirectory.Exists)
                _targetDirectory.Delete(true);
        }

        #endregion

        private DirectoryInfo _targetDirectory;

        [Test]
        public void DestinationFolderIsAutomaticallyCreated()
        {
            new MoveCommand
                {
                    FromDirectory = "a1",
                    ToDirectory = "a2",
                    IncludeFiles = "*/*",
                }.Execute(_targetDirectory, null);

            Expect(File.Exists(Path.Combine(_targetDirectory.FullName, "a1/file1")));
            Expect(!File.Exists(Path.Combine(_targetDirectory.FullName, "a1/b1/file2")));
            Expect(File.Exists(Path.Combine(_targetDirectory.FullName, "a2/b1/file2")));
        }

        [Test]
        public void MoveToAndFromTheSameDirectoryDoesNothing()
        {
            new MoveCommand
                {
                    FromDirectory = "a1",
                    ToDirectory = "a1",
                }.Execute(_targetDirectory, null);
        }

        [Test]
        public void OnlyMovesIncludedFilesRelativeToTheSourceFolder()
        {
            new MoveCommand
                {
                    FromDirectory = "a1",
                    ToDirectory = "does_not_exist",
                }.Execute(_targetDirectory, null);

            Expect(Directory.Exists(Path.Combine(_targetDirectory.FullName, "does_not_exist")));
        }

        [Test]
        public void SourceFolderMustExist()
        {
            var moveCommand = new MoveCommand
                {
                    FromDirectory = "does_not_exist",
                };

            Expect(() => moveCommand.Execute(_targetDirectory, null), Throws.InstanceOf<DirectoryNotFoundException>());
        }
    }
}