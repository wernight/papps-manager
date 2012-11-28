using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using PAppsManager.Core;
using PAppsManager.Core.Import;

namespace PAppsManager.ViewModels
{
    /// <summary>
    /// Displays a list of choices to select from.
    /// The user can select one or more items.
    /// </summary>
    internal class ImportSelectionViewModel : Screen
    {
        private readonly IEventAggregator _events;
        public ObservableCollection<ISelection> Items { get; private set; }

        /// <summary>
        /// Set to true while it's performing the import.
        /// </summary>
        public bool Importing { get; private set; }

        public ImportSelectionViewModel()
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

        public ImportSelectionViewModel(IEnumerable<ISelection> importChoices, IEventAggregator events)
        {
            Items = new ObservableCollection<ISelection>(importChoices);
            _events = events;
        }

        public void Ok()
        {
            // Close this screen.
            _events.Publish(new ImportEvent(Items.Where(i => i.Enabled)));
        }

        /// <summary>
        /// Same as unchecking all and clicking on Ok.
        /// </summary>
        public void Skip()
        {
            foreach (Importer importer in Items)
                importer.Enabled = false;
            Ok();
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