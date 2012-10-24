using System.IO;
using NUnit.Framework;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps.Commands
{
    [TestFixture]
    public class ExtractCommandTest : AssertionHelper
    {
        [TestCase("example.zip")]
        [TestCase("example.7z")]
        [TestCase("nsis.exe")]
        public void DecompressNsis(string filename)
        {
            new ExtractCommand{FileName = filename}.Execute(new DirectoryInfo("Resources"));
            // TODO: Expect to find "example.txt" decompressed.
        }
    }
}