namespace PAppsManager.Core
{
    internal interface ISelection
    {
        bool IsChecked { get; set; }

        string Description { get; }
    }
}