using System.IO;
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
            // Clean-up last execution (just in case).
            TearDown();

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

        private readonly DirectoryInfo _targetDirectory = new DirectoryInfo("MoveCommentTest");

        [Test]
        public void DestinationFolderIsAutomaticallyCreated()
        {
            new MoveCommand
                {
                    FromDirectory = "a1",
                    ToDirectory = "a2",
                    IncludeFiles = "*/*",
                }.Execute(_targetDirectory);

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
                }.Execute(_targetDirectory);
        }

        [Test]
        public void OnlyMovesIncludedFilesRelativeToTheSourceFolder()
        {
            new MoveCommand
                {
                    FromDirectory = "a1",
                    ToDirectory = "does_not_exist",
                }.Execute(_targetDirectory);

            Expect(Directory.Exists(Path.Combine(_targetDirectory.FullName, "does_not_exist")));
        }

        [Test]
        public void SourceFolderMustExist()
        {
            var moveCommand = new MoveCommand
                {
                    FromDirectory = "does_not_exist",
                };

            Expect(() => moveCommand.Execute(_targetDirectory), Throws.InstanceOf<DirectoryNotFoundException>());
        }
    }
}