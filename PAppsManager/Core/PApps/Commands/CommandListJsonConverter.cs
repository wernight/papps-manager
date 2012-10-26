using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PAppsManager.Core.PApps.Commands
{
    internal class CommandListJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (CommandList);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            // Deserialize.
            List<JsonAction> jsonActions;
            try
            {
                jsonActions = serializer.Deserialize<List<JsonAction>>(reader);
            }
            catch (Exception e)
            {
                throw new JsonException("Invalid JSON: Failed to deserialize: " + e.Message, e);
            }

            // Retrieve the single action of each.
            var actions = jsonActions.Select(jsonAction => jsonAction.Command);
            return new CommandList(actions);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        [UsedImplicitly]
        private class JsonAction
        {
#pragma warning disable 649
            public DeleteCommand Delete;
            public DisplayCommand Display;
            public DownloadCommand Download;
            public ErrorCommand Error;
            public EulaCommand Eula;
            public ExecuteCommand Execute;
            public ExtractCommand Extract;
            public MoveCommand Move;
            public UniExtractCommand UniExtract;
#pragma warning restore 649

            public Command Command
            {
                get
                {
                    var actions = Actions.Where(a => a != null).ToList();
                    if (!actions.Any())
                        throw new JsonException("Invalid JSON: No known action type defined.");
                    if (actions.Count() > 1)
                        throw new JsonException("Invalid JSON: More than one type of action defined per action.");
                    return actions.First();
                }
            }

            private IEnumerable<Command> Actions
            {
                get
                {
                    yield return Delete;
                    yield return Display;
                    yield return Download;
                    yield return Error;
                    yield return Eula;
                    yield return Execute;
                    yield return Extract;
                    yield return Move;
                    yield return UniExtract;
                }
            }
        }
    }
}