using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace PAppsManager.Core.PApps.Commands
{
    /// <summary>
    /// Portable application installation action.
    /// </summary>
    public abstract class Command : ICommand
    {
        protected static string ExeDirectory
        {
            get { return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath); }
        }

        public abstract string Validate();

        public abstract void Execute(DirectoryInfo targetDirectory);

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
                if (allowEmpty && fileName != null)
                    return null;
                return "File name not provided";
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
            wildcard = wildcard.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            wildcard = Regex.Replace(wildcard, @"\*{3,}", "**");
            wildcard = Regex.Escape(wildcard);
            wildcard = wildcard.Replace(Regex.Escape("**"), ".*");
            wildcard = wildcard.Replace(Regex.Escape("*"), "[^" + Regex.Escape(string.Concat(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) + "]*");
            wildcard = wildcard.Replace(Regex.Escape("?"), "[^" + Regex.Escape(string.Concat(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) + "]");
            return wildcard;
        }

        /// <summary>
        /// Recursively delete empty directories below the base directory.
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <param name="directory"></param>
        protected static void DeleteIfEmpty([NotNull] DirectoryInfo baseDirectory, [CanBeNull] DirectoryInfo directory)
        {
            Debug.Assert(directory == null || directory.FullName.StartsWith(baseDirectory.FullName));

            string baseDirectoryFullName = baseDirectory.FullName + Path.DirectorySeparatorChar;

            while (directory != null && directory.FullName.StartsWith(baseDirectoryFullName) &&
                   !directory.GetDirectories().Any() && !directory.GetFiles().Any())
            {
                // Delete empty directories.
                directory.Delete();
                directory = directory.Parent;
            }
        }
    }
}