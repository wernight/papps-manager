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
            Directory.CreateDirectory("DeleteCommandTest/a/b/c");
            File.Create("DeleteCommandTest/a/b/c/file.txt").Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists("DeleteCommandTest"))
                Directory.Delete("DeleteCommandTest", true);
        }

        #endregion

        [Test]
        public void CanWorkWithVariousPathDelimiters()
        {
            new DeleteCommand
                {IncludeFiles = "a" + Path.DirectorySeparatorChar + "b" + Path.AltDirectorySeparatorChar + "c/file.txt"}
                .Execute(new DirectoryInfo("DeleteCommandTest"));

            Expect(!File.Exists("DeleteCommandTest/a/b/c/file.txt"));
        }

        [Test]
        public void DeletesEmptyDirectories()
        {
            new DeleteCommand {IncludeFiles = @"a\b\c\file.txt"}.Execute(new DirectoryInfo("DeleteCommandTest"));

            Expect(!File.Exists("DeleteCommandTest/a/b/c/file.txt"));
            Expect(!Directory.Exists("DeleteCommandTest/a"));
            Expect(Directory.Exists("DeleteCommandTest"));
        }
    }
}