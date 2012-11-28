namespace PAppsManager.Core
{
    internal interface ISelection
    {
        bool Enabled { get; set; }

        string Description { get; }
    }
}