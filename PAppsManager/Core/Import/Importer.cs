using System;
using Caliburn.Micro;

namespace PAppsManager.Core.Import
{
    internal abstract class Importer : ISelection, IResult
    {
        protected Importer(string description)
        {
            IsChecked = true;
            Description = description;
        }

        public bool IsChecked { get; set; }

        public string Description { get; private set; }

        public bool Importing { get; protected set; }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public void Execute(ActionExecutionContext context)
        {
            PerformImport();
            Completed(this, new ResultCompletionEventArgs());
        }

        protected abstract void PerformImport();
    }
}