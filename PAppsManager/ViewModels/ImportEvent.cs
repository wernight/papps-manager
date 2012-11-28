using System.Collections.Generic;
using PAppsManager.Core;

namespace PAppsManager.ViewModels
{
    internal class ImportEvent : List<ISelection>
    {
        public ImportEvent(IEnumerable<ISelection> items) : base(items)
        {
        }
    }
}