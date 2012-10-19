using System.IO;
using System.Text.RegularExpressions;

namespace PAppsManager.Core.PApps.Commands
{
    public class DeleteCommand : Command
    {
        /// <summary>
        /// Files to delete, relative to the installation directory.
        /// 
        /// Supporting wildcards:
        ///   - '?' matches a single character, except path delimiters.
        ///   - '*' matches zero or more characters, except path delimiters.
        ///   - '**' matches zero or more characters, including path delimiters.
        /// </summary>
        public string IncludeFiles { get; set; }

        public override string Validate()
        {
            return ValidateRegex(WildcardToRegex(IncludeFiles));
        }

        public override void Execute()
        {
            var regex = new Regex(WildcardToRegex(IncludeFiles));
            foreach (string file in Directory.EnumerateFiles(InstallTargerDirectory, "*", SearchOption.AllDirectories))
            {
                string relativeFile = file.Substring(InstallTargerDirectory.Length);
                if (regex.IsMatch(relativeFile))
                    File.Delete(file);
            }
        }
    }
}