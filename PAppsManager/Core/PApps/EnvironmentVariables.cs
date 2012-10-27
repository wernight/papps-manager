using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PAppsManager.Core.PApps
{
    /// <summary>
    /// Expand/contract common and extra environment variables.
    /// 
    /// Example: "%ProgramFiles%" to/from "C:\Program Files".
    /// </summary>
    internal class EnvironmentVariables
    {
        private readonly Dictionary<string, string> _replacementsDictionary;

        public EnvironmentVariables()
        {
            // Prepare an ordered dictionary of Key => Value.
            _replacementsDictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string specialFolderName in Enum.GetNames(typeof(Environment.SpecialFolder)))
            {
                var specialFolder = (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), specialFolderName);
                string folderPath = Environment.GetFolderPath(specialFolder);

                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    Add(specialFolderName, folderPath);
                }
            }
        }

        public void Add(string key, string value)
        {
            _replacementsDictionary.Add(key, value);
        }

        public string Expand(string value)
        {
            return Regex.Replace(value, @"%([^%]*)%", Evaluator);
        }

        public string Contract(string value)
        {
            char invalid = Path.GetInvalidPathChars().First();

            // In order to work properly the path should not contain that value.
            if (value.Contains(invalid))
                throw new Exception("The path contain an invalid character: " + invalid);

            // Replace all values by their keys.
            return _replacementsDictionary
                .OrderByDescending(pair => pair.Value.Length)   // That's more a heuristic to prefer %ProgramFiles(x86)% or %ProgramFiles% in 32-bit OS.
                .ThenByDescending(pair => pair.Key.Length)
                .Aggregate(value, (current, kvp) => Regex.Replace(current,
                                                                  Regex.Escape(kvp.Value),
                                                                  Regex.Escape(invalid + kvp.Key + invalid),
                                                                  RegexOptions.IgnoreCase |
                                                                  RegexOptions.CultureInvariant))
                .Replace("%", "%%")
                .Replace(invalid, '%');
        }

        private string Evaluator(Match match)
        {
            if (match.Value == "%%")
                return "%";

            string expandedPath;
            if (_replacementsDictionary.TryGetValue(match.Groups[1].Value, out expandedPath))
                return expandedPath;

            return match.Value;
        }
    }
}