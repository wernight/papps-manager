﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;

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
        [JsonProperty("files")]
        public string IncludeFiles { get; set; }

        public override string Validate()
        {
            return ValidateRegex(WildcardToRegex(IncludeFiles));
        }

        public override void Execute(DirectoryInfo targetDirectory)
        {
            var possiblyEmptyDirectories = new HashSet<DirectoryInfo>();

            var regex = new Regex(WildcardToRegex(IncludeFiles));
            foreach (FileInfo file in targetDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string relativeFile = file.FullName.Substring(targetDirectory.FullName.Length + 1);
                if (regex.IsMatch(relativeFile))
                {
                    file.Delete();

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