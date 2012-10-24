using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PAppsManager.Core.PApps.Commands
{
    [JsonConverter(typeof(CommandListJsonConverter))]
    internal class CommandList : Command
    {
        public CommandList(IEnumerable<Command> actions = null)
        {
            Commands = actions != null ? new List<Command>(actions) : new List<Command>();
        }

        public List<Command> Commands { get; private set; }

        public override string Validate()
        {
            return Commands.Select(action => action.Validate()).FirstOrDefault(validate => validate != null);
        }

        public override void Execute()
        {
            foreach (Command action in Commands)
            {
                action.Execute();
            }
        }

        public override void CleanUp(bool successful)
        {
            foreach (Command action in Commands)
            {
                action.CleanUp(successful);
            }
            base.CleanUp(successful);
        }

        public override string ToString()
        {
            return string.Format("{0} Install Commands", Commands.Count);
        }
    }
}
