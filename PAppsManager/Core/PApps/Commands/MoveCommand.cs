using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PAppsManager.Core.PApps.Commands
{
    /// <summary>
    /// Rename one of more files keeping the directory structure.
    /// </summary>
    public class MoveCommand : Command
    {
        public MoveCommand()
        {
            FromDirectory = "";
            IncludeFiles = "**";
            ToDirectory = "";
        }

        /// <summary>
        /// Source directory to move files from, relative to the portable application installation directory.
        /// 
        /// Empty by default.
        /// </summary>
        [JsonProperty("from_directory")]
        public string FromDirectory { get; set; }

        /// <summary>
        /// Expression defining files to move, relative to the <see cref="FromDirectory"/>.
        /// All matching files will be from <see cref="ToDirectory"/>. By default, include all files recursively.
        /// 
        /// Supporting wildcards:
        ///   - '?' matches a single character, except path delimiters.
        ///   - '*' matches zero or more characters, except path delimiters.
        ///   - '**' matches zero or more characters, including path delimiters (i.e., recursive).
        /// </summary>
        [JsonProperty("include_files")]
        public string IncludeFiles { get; set; }
        
        /// <summary>
        /// Destination directory to move files to, relative to the portable application installation directory.
        /// 
        /// Empty by default.
        /// </summary>
        [JsonProperty("to_directory")]
        public string ToDirectory { get; set; }

        public override string Validate()
        {
            return ValidateRelativePath(FromDirectory, true) ?? ValidateRegex(WildcardToRegex(IncludeFiles)) ?? ValidateRelativePath(ToDirectory, true);
        }

        public override void Execute(DirectoryInfo targetDirectory)
        {
            var fromDirectory = new DirectoryInfo(Path.Combine(targetDirectory.FullName, FromDirectory));
            var toDirectory = new DirectoryInfo(Path.Combine(targetDirectory.FullName, ToDirectory));

            if (fromDirectory == toDirectory)
                return;

            // Create the target directory.
            toDirectory.Create();

            var possiblyEmptyDirectories = new HashSet<DirectoryInfo>();

            var regex = new Regex(WildcardToRegex(IncludeFiles));

            foreach (FileInfo file in fromDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string relativePath = file.FullName.Substring(fromDirectory.FullName.Length + 1);
                if (regex.IsMatch(relativePath))
                {
                    var destFile = new FileInfo(Path.Combine(toDirectory.FullName, relativePath));
                    if (destFile.Directory != null)
                        destFile.Directory.Create();
                    file.MoveTo(destFile.FullName);

                    DirectoryInfo directoryInfo = file.Directory;
                    if (directoryInfo != null)
                        possiblyEmptyDirectories.Add(directoryInfo);
                }
            }

            // Recursively delete emptied directories below the target directory.
            foreach (DirectoryInfo directoryInfo in possiblyEmptyDirectories)
            {
                DeleteIfEmpty(targetDirectory, directoryInfo);
            }
        }
    }
}