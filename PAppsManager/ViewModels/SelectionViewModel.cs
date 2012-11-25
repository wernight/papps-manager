using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using PAppsManager.Core;
using PAppsManager.Core.Import;

namespace PAppsManager.ViewModels
{
    /// <summary>
    /// Displays a list of choices to select from.
    /// The user can select one or more items.
    /// </summary>
    internal class SelectionViewModel : Screen
    {
        public ObservableCollection<ISelection> Items { get; private set; }

        /// <summary>
        /// Set to true while it's performing the import.
        /// </summary>
        public bool Importing { get; private set; }

        public SelectionViewModel()
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

        public SelectionViewModel(IEnumerable<ISelection> importChoices)
        {
            Items = new ObservableCollection<ISelection>(importChoices);
        }

        public void Ok()
        {
            // Close this screen.
            TryClose();
        }

        /// <summary>
        /// Same as unchecking all and clicking on Ok.
        /// </summary>
        public void Skip()
        {
            foreach (Importer importer in Items)
                importer.IsChecked = false;
            Ok();
        }

        private class SampleImporter : ISelection
        {
            public SampleImporter(string description)
            {
                IsChecked = true;
                Description = description;
            }

            public bool IsChecked { get; set; }

            public string Description { get; private set; }
        }
    }
}