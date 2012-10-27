using System.IO;
using NUnit.Framework;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps.Commands
{
    [TestFixture]
    public class DeleteCommandTest : AssertionHelper
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _targetDirectory =
                new DirectoryInfo(Path.Combine(Path.GetTempPath(),
                                               Path.Combine("PAppsManagerUnitTests/DeleteCommandTest",
                                                            Path.GetRandomFileName())));

            Directory.CreateDirectory(Path.Combine(_targetDirectory.FullName, "a/b/c"));
            File.Create(Path.Combine(_targetDirectory.FullName, "a/b/c/file.txt")).Dispose();
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
        public void CanWorkWithVariousPathDelimiters()
        {
            new DeleteCommand
                {IncludeFiles = "a" + Path.DirectorySeparatorChar + "b" + Path.AltDirectorySeparatorChar + "c/file.txt"}
                .Execute(_targetDirectory, null);

            Expect(!File.Exists(Path.Combine(_targetDirectory.FullName, "a/b/c/file.txt")));
        }

        [Test]
        public void DeletesEmptyDirectories()
        {
            new DeleteCommand {IncludeFiles = @"a\b\c\file.txt"}.Execute(_targetDirectory, null);

            Expect(!File.Exists(Path.Combine(_targetDirectory.FullName, "a/b/c/file.txt")));
            Expect(!Directory.Exists(Path.Combine(_targetDirectory.FullName, "a")));
            Expect(_targetDirectory.Exists);
        }
    }
}