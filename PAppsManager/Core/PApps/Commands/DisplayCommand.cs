using System.Windows;

namespace PAppsManager.Core.PApps.Commands
{
    /// <summary>
    /// Display a messages and continues afterwards.
    /// </summary>
    internal class DisplayCommand : Command
    {
        public string Message { get; set; }

        public override string Validate()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return "Message is not defined.";

            return null;
        }

        public override void Execute()
        {
            MessageBox.Show(Message);
        }

        public override string ToString()
        {
            return "Message: " + Message;
        }
    }
}