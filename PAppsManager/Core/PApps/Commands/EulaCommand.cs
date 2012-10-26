using System.IO;
using System.Windows;

namespace PAppsManager.Core.PApps.Commands
{
    /// <summary>
    /// Displays a EULA that the user has to accept in order to continue.
    /// </summary>
    public class EulaCommand : Command
    {
        public string Text { get; set; }

        #region Overrides of Command

        public override string Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return "Text is not defined.";

            return null;
        }

        public override void Execute(DirectoryInfo targetDirectory)
        {
            if (MessageBox.Show(Text, "EULA", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                throw new CommandException("EULA has been refused, installation is being aborted.");
        }

        #endregion
    }
}