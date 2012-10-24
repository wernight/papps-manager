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
        /// Source files, relative to the <see cref="FromDirectory"/>.
        /// 
        /// Supporting wildcards:
        ///   - '?' matches a single character, except path delimiters.
        ///   - '*' matches zero or more characters, except path delimiters.
        ///   - '**' matches zero or more characters, including path delimiters.
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
            return ValidateFileName(FromDirectory, true) ?? ValidateRegex(WildcardToRegex(IncludeFiles)) ?? ValidateFileName(ToDirectory, true);
        }

        public override void Execute(DirectoryInfo targetDirectory)
        {
            if (FromDirectory == ToDirectory)
                return;

            var possiblyEmptyDirectories = new HashSet<DirectoryInfo>();

            var regex = new Regex(WildcardToRegex(IncludeFiles));

            var baseDirectory = new DirectoryInfo(Path.Combine(targetDirectory.FullName, FromDirectory));
            foreach (FileInfo file in baseDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string relativeFile = file.FullName.Substring(baseDirectory.FullName.Length);
                if (regex.IsMatch(relativeFile))
                {
                    file.MoveTo(Path.Combine(ToDirectory, relativeFile));

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