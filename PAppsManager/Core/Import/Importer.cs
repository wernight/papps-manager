using System;
using Caliburn.Micro;

namespace PAppsManager.Core.Import
{
    internal abstract class Importer : ISelection, IResult
    {
        protected Importer()
        {
            Enabled = true;
        }

        public bool Enabled { get; set; }

        public abstract string Description { get; }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public void Execute(ActionExecutionContext context)
        {
            PerformImport();

            Completed(this, new ResultCompletionEventArgs());
        }

        protected abstract void PerformImport();
    }
}