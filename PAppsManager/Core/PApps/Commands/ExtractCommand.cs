using System;
using System.IO;
using System.Reflection;
using SevenZip;

namespace PAppsManager.Core.PApps.Commands
{
    /// <summary>
    /// Decompress a file.
    /// </summary>
    internal class ExtractCommand : Command
    {
        /// <summary>
        /// Relative file name to extract.
        /// </summary>
        public string FileName { get; set; }

        public override string Validate()
        {
            return ValidateFileName(FileName);
        }

        public override void Execute()
        {
            // Decompress the file.
            SevenZipBase.SetLibraryPath(SevenZipLibPath);
            using (var extractor = new SevenZipExtractor(Path.Combine(InstallTargerDirectory, FileName)))
                extractor.ExtractArchive(ExeDirectory);
        }

        private static string SevenZipLibPath
        {
            get
            {
                // Tell where 7z.dll is located
                string relativePath = Environment.Is64BitProcess ? @"7-Zip\x64\7z.dll" : @"7-Zip\x86\7z.dll";
                string sevenZipLibPath = Path.Combine(ExeDirectory, relativePath);
                return sevenZipLibPath;
            }
        }

        public override string ToString()
        {
            return string.Format("Extract {0}", FileName);
        }
    }
}