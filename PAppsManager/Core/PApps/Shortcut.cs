using JetBrains.Annotations;
using vbAccelerator.Components.Shell;

namespace PAppsManager.Core.PApps
{
    internal class Shortcut
    {
        public Shortcut()
        {
            DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
        }

        /// <summary>
        /// Full shortcut file name (should end by *.lnk).
        /// 
        /// Supports %ENV% notation (which should be used).
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Shortcut target.
        /// 
        /// Supports %ENV% notation (which should be used).
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Command line arguments.
        /// </summary>
        [CanBeNull]
        public string Arguments { get; set; }

        /// <summary>
        /// Working directory.
        /// </summary>
        [CanBeNull]
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Icon to use.
        /// </summary>
        [CanBeNull]
        public string IconPath { get; set; }

        /// <summary>
        /// Startup display mode.
        /// </summary>
        public ShellLink.LinkDisplayMode DisplayMode { get; set; }

        /// <summary>
        /// Link description.
        /// </summary>
        [CanBeNull]
        public string Description { get; set; }
    }
}