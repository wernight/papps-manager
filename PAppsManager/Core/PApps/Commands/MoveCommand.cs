using System.IO;
using System.Text.RegularExpressions;

namespace PAppsManager.Core.PApps.Commands
{
    /// <summary>
    /// Rename one of more files keeping the directory structure.
    /// </summary>
    public class MoveCommand : Command
    {
        public MoveCommand()
        {
            BaseDirectory = "";
            ToDirectory = "";
        }

        /// <summary>
        /// Source directory to move files from, relative to the portable application installation directory.
        /// 
        /// Empty by default.
        /// </summary>
        public string BaseDirectory { get; set; }

        /// <summary>
        /// Source files, relative to the <see cref="BaseDirectory"/>.
        /// 
        /// Supporting wildcards:
        ///   - '?' matches a single character, except path delimiters.
        ///   - '*' matches zero or more characters, except path delimiters.
        ///   - '**' matches zero or more characters, including path delimiters.
        /// </summary>
        public string IncludeFiles { get; set; }
        
        /// <summary>
        /// Destination directory to move files to, relative to the portable application installation directory.
        /// 
        /// Empty by default.
        /// </summary>
        public string ToDirectory { get; set; }

        public override string Validate()
        {
            return ValidateFileName(BaseDirectory, true) ?? ValidateRegex(WildcardToRegex(IncludeFiles)) ?? ValidateFileName(ToDirectory, true);
        }

        public override void Execute()
        {
            var regex = new Regex(WildcardToRegex(IncludeFiles));

            var baseDirectory = Path.Combine(InstallTargerDirectory, BaseDirectory);
            foreach (string file in Directory.EnumerateFiles(baseDirectory, "*", SearchOption.AllDirectories))
            {
                string relativeFile = file.Substring(baseDirectory.Length);
                if (regex.IsMatch(relativeFile))
                    File.Move(file, Path.Combine(ToDirectory, relativeFile));
            }
        }
    }
}