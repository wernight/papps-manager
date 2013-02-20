using System.IO;
using JetBrains.Annotations;
using PAppsManager.Core.PApps;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManager.Core.Import
{
    /// <summary>
    /// Copy an existing local directory to the portable application installation direcotry.
    /// </summary>
    internal class CopyCommand : Command
    {
        private readonly DirectoryInfo _fromDirectory;

        public CopyCommand([NotNull] DirectoryInfo fromDirectory)
        {
            _fromDirectory = fromDirectory;
        }

        public override string Validate()
        {
            return null;
        }

        public override void Execute(DirectoryInfo targetDirectory, PortableEnvironment portableEnvironment)
        {
            if (_fromDirectory == targetDirectory)
                return;

            foreach (FileInfo file in _fromDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string relativePath = file.FullName.Substring(_fromDirectory.FullName.Length + 1);

                var destFile = new FileInfo(Path.Combine(_fromDirectory.FullName, relativePath));
                if (destFile.Directory != null)
                    destFile.Directory.Create();
                file.CopyTo(destFile.FullName);
            }
        }
    }
}