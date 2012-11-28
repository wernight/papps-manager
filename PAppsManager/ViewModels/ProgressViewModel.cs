using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using PAppsManager.Core;
using PAppsManager.Core.Import;

namespace PAppsManager.ViewModels
{
    /// <summary>
    /// Displays an operation progress, like the import progress.
    /// </summary>
    internal class ProgressViewModel : Screen
    {
        public ObservableCollection<ISelection> Items { get; private set; }

        /// <summary>
        /// Set to true while it's performing the import.
        /// </summary>
        public bool Importing { get; private set; }

        public ProgressViewModel()
        {
            if (Execute.InDesignMode)
            {
                Items = new ObservableCollection<ISelection>
                                    {
                                        new SampleImporter("Import 36 PortableApps.com applications on X:\\"),
                                        new SampleImporter("Import 12 LiberKey applications on X:\\"),
                                        new OpenWebsiteImporter(),
                                    };
            }
        }

        public ProgressViewModel(IEnumerable<ISelection> importChoices)
        {
            Items = new ObservableCollection<ISelection>(importChoices);
        }

        private class SampleImporter : ISelection
        {
            public SampleImporter(string description)
            {
                Enabled = true;
                Description = description;
            }

            public bool Enabled { get; set; }

            public string Description { get; private set; }
        }
    }
}