using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PAppsManager.Core.PApps.Commands
{
    /// <summary>
    /// Portable application installation action.
    /// </summary>
    public abstract class Command
    {
        protected Command()
        {
            InstallTargerDirectory = "";
        }

        public string InstallTargerDirectory { get; set; }

        protected static string ExeDirectory
        {
            get
            {
                string exeDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                return exeDirectory;
            }
        }

        /// <summary>
        /// Verified that all required info are provided and look valid.
        /// Do security checks here.
        /// </summary>
        /// <returns>Null if all is fine, or a message if it failed validation.</returns>
        public abstract string Validate();

        /// <summary>
        /// Perform the action.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Post-installation operation.
        /// </summary>
        /// <param name="successful">True if the installation was successful.</param>
        public virtual void CleanUp(bool successful)
        {
        }

        /// <summary>
        /// Check that a string is a valid file name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="allowEmpty">True to make it optional, allowing the filename to be null or empty.</param>
        /// <returns>Null if all is fine, or a message if it failed validation.</returns>
        internal protected static string ValidateFileName(string fileName, bool allowEmpty = false)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                if (!allowEmpty)
                    return "File name not provided";
                return null;
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                return "File name contain invalid characters.";

            return null;
        }

        internal protected static string ValidateRegex(string regex)
        {
            try
            {
                new Regex(regex);
            }
            catch (Exception e)
            {
                return "Invalid files pattern: " + e.Message;
            }

            return null;
        }

        internal protected static string WildcardToRegex(string wildcard)
        {
            wildcard = Regex.Replace(wildcard, @"\*{3,}", "**");
            wildcard = Regex.Escape(wildcard);
            wildcard = wildcard.Replace(Regex.Escape("**"), ".*");
            wildcard = wildcard.Replace(Regex.Escape("*"), "[^" + Regex.Escape("" + Path.DirectorySeparatorChar + Path.AltDirectorySeparatorChar) + "]*");
            wildcard = wildcard.Replace(Regex.Escape("?"), "[^" + Regex.Escape("" + Path.DirectorySeparatorChar + Path.AltDirectorySeparatorChar) + "]");
            return wildcard;
        }
    }
}