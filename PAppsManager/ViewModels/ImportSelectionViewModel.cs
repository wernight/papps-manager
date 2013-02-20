using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using PAppsManager.Core.Import;

namespace PAppsManager.ViewModels
{
    /// <summary>
    /// Displays a list of choices to select from.
    /// The user can select one or more items.
    /// </summary>
    internal class ImportSelectionViewModel : Screen
    {
        public ObservableCollection<Importer> Items { get; private set; }

        /// <summary>
        /// Set to true while it's performing the import.
        /// </summary>
        public bool Importing { get; private set; }

        public ImportSelectionViewModel()
        {
            DisplayName = "Import Portable Applications";

            if (Execute.InDesignMode)
            {
                Items = new ObservableCollection<Importer>
                                    {
                                        new SampleImporter("Import 36 PortableApps.com applications on X:\\"),
                                        new OpenWebsiteImporter(),
                                    };
            }
        }

        public ImportSelectionViewModel(IEnumerable<Importer> importChoices)
        {
            Items = new ObservableCollection<Importer>(importChoices);
        }

        public IEnumerable<IResult> Ok()
        {
            foreach (Importer selection in Items.Where(i => i.Enabled))
            {
                yield return Loader.Show("Importing " + selection.Description + "...");
                yield return selection;
            }
            yield return Loader.Hide();

            // Close this screen.
            TryClose();
        }

        /// <summary>
        /// Same as unchecking all and clicking on Ok.
        /// </summary>
        public void Skip()
        {
            foreach (Importer importer in Items)
                importer.Enabled = false;
            TryClose();
        }

        private class SampleImporter : Importer
        {
            private readonly string _description;

            public SampleImporter(string description)
            {
                _description = description;
            }

            public override string Description
            {
                get { return _description; }
            }

            protected override void PerformImport()
            {
                throw new NotImplementedException();
            }
        }

        private class Loader : IResult
        {
            private readonly string _message;

            private Loader(string message)
            {
                _message = message;
            }

            public void Execute(ActionExecutionContext context)
            {
                var view = context.View as FrameworkElement;
                view.IsEnabled = false;
/*                while (view != null)
                {
                    var busyIndicator = view as BusyIndicator;
                    if (busyIndicator != null)
                    {
                        if (!string.IsNullOrEmpty(_message))
                            busyIndicator.BusyContent = _message;
                        busyIndicator.IsBusy = !string.IsNullOrEmpty(_message);
                        break;
                    }

                    view = view.Parent as FrameworkElement;
                }*/

                Completed(this, new ResultCompletionEventArgs());
            }

            public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

            public static IResult Show(string message = null)
            {
                return new Loader(message);
            }

            public static IResult Hide()
            {
                return new Loader(null);
            }
        }
    }
}