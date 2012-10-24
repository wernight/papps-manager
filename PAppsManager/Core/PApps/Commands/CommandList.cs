using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace PAppsManager.Core.PApps.Commands
{
    [JsonConverter(typeof(CommandListJsonConverter))]
    internal class CommandList : List<ICommand>, ICommand
    {
        public CommandList()
        {
        }

        public CommandList(IEnumerable<ICommand> commands)
            : base(commands)
        {
        }

        public virtual string Validate()
        {
            return this.Select(action => action.Validate()).FirstOrDefault(validate => validate != null);
        }

        public virtual void Execute(DirectoryInfo targetDirectory)
        {
            foreach (ICommand action in this)
            {
                action.Execute(targetDirectory);
            }
        }

        public virtual void CleanUp(bool successful)
        {
            foreach (ICommand action in this)
            {
                action.CleanUp(successful);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} Install Commands", Count);
        }
    }
}
