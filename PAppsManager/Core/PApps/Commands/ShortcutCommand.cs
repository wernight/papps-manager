using System;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using vbAccelerator.Components.Shell;

namespace PAppsManager.Core.PApps.Commands
{
    internal class ShortcutCommand : Command
    {
        public ShortcutCommand()
        {
            DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
        }

        /// <summary>
        /// Full shortcut file name (should end by *.lnk), relative to the portable applications start menu directory.
        /// </summary>
        [JsonProperty("file")]
        public string FileName { get; set; }

        /// <summary>
        /// Shortcut target, relative to the portable application installation directory.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Command line arguments.
        /// </summary>
        [CanBeNull]
        public string Arguments { get; set; }

        /// <summary>
        /// Working directory, relative to the portable application installation directory.
        /// </summary>
        [CanBeNull, JsonProperty("working_directory")]
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Icon to use, relative to the portable application installation directory.
        /// </summary>
        [CanBeNull, JsonProperty("icon")]
        public string IconPath { get; set; }

        /// <summary>
        /// Startup display mode.
        /// </summary>
        [JsonProperty("display_mode")]
        public ShellLink.LinkDisplayMode DisplayMode { get; set; }

        /// <summary>
        /// Link description.
        /// </summary>
        [CanBeNull]
        public string Description { get; set; }

        public override string Validate()
        {
            string validate = ValidateRelativePath(FileName);
            if (validate != null)
                return validate;
            if (!FileName.EndsWith(".lnt", StringComparison.OrdinalIgnoreCase))
                return "Shortcut name should be a *.lnk file.";

            if (string.IsNullOrWhiteSpace(Target))
                return "Target is not defined.";

            validate = ValidateRelativePath(WorkingDirectory, true);
            if (validate != null)
                return validate;

            validate = ValidateRelativePath(IconPath, true);
            if (validate != null)
                return validate;

            return null;
        }

        public override void Execute(DirectoryInfo targetDirectory, PortableEnvironment portableEnvironment)
        {
            var shortcut = new Shortcut
                               {
                                   FileName = Path.Combine(portableEnvironment.Shortcuts.StartMenuTargetDirectory.FullName, FileName),
                                   Target = Path.Combine(targetDirectory.FullName, Target),
                                   Arguments = Arguments,
                                   WorkingDirectory = string.IsNullOrWhiteSpace(WorkingDirectory) ? null : Path.Combine(targetDirectory.FullName, WorkingDirectory),
                                   IconPath = string.IsNullOrWhiteSpace(IconPath) ? null : Path.Combine(targetDirectory.FullName, IconPath),
                                   DisplayMode = DisplayMode,
                                   Description = Description,
                               };

            // Create the link file.
            portableEnvironment.Shortcuts.Add(shortcut);
        }
    }
}