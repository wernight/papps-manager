using System;
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
            return (from ICommand command in this
                    let validate = command.Validate()
                    where validate != null
                    select string.Format("Command {0} validation failed: {1}", command.GetType().Name, validate)
                   ).FirstOrDefault();
        }

        public virtual void Execute(DirectoryInfo targetDirectory, PortableEnvironment portableEnvironment)
        {
            foreach (ICommand command in this)
            {
                try
                {
                    command.Execute(targetDirectory, portableEnvironment);
                }
                catch (Exception e)
                {
                    throw new CommandException("Failed to execute the command " + command.GetType().Name + ": " + e.Message, e);
                }
            }
        }

        public virtual void CleanUp(bool successful)
        {
            foreach (ICommand command in this)
            {
                try
                {
                    command.CleanUp(successful);
                }
                catch (Exception e)
                {
                    throw new CommandException("Failed to clean-up after the command " + command.GetType().Name + ": " + e.Message, e);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0} Install Commands", Count);
        }
    }
}
