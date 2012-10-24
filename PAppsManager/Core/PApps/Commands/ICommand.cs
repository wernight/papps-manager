using System.IO;

namespace PAppsManager.Core.PApps.Commands
{
    public interface ICommand
    {
        /// <summary>
        /// Verified that all required info are provided and look valid.
        /// Do security checks here.
        /// </summary>
        /// <returns>Null if all is fine, or a message if it failed validation.</returns>
        string Validate();

        /// <summary>
        /// Perform the action.
        /// </summary>
        void Execute(DirectoryInfo targetDirectory);

        /// <summary>
        /// Post-installation operation.
        /// </summary>
        /// <param name="successful">True if the installation was successful.</param>
        void CleanUp(bool successful);
    }
}