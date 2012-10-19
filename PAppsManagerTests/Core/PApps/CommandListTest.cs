using NUnit.Framework;
using Newtonsoft.Json;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps
{
    [TestFixture]
    public class CommandListTest : AssertionHelper
    {
        [Test]
        public void CanDeserializeFromJson()
        {
            const string json = "[" +
                                "{delete: {includefiles: 'foo.tmp'}}," +
                                "{display: {message: 'Hello World!'}}," +
                                "{download: {url: 'http://example.com', filename: 'example.zip'}}," +
                                "{error: {message: 'Hello World!'}}," +
                                "{execute: {failonerror: false}}," +
                                "{extract: {filename: 'example.zip'}}," +
                                "{move: {}}" +
                                "]";

            var commandList = JsonConvert.DeserializeObject<CommandList>(json);

            Expect(commandList.Commands, Has.Count.EqualTo(7));

            Expect(commandList.Commands[0], Is.InstanceOf<DeleteCommand>());
            Expect(commandList.Commands[1], Is.InstanceOf<DisplayCommand>());
            Expect(commandList.Commands[2], Is.InstanceOf<DownloadCommand>().With.Property("Url").EqualTo("http://example.com"));
            Expect(commandList.Commands[3], Is.InstanceOf<ErrorCommand>().With.Property("Message").EqualTo("Hello World!"));
            Expect(commandList.Commands[4], Is.InstanceOf<ExecuteCommand>().With.Property("FailOnError").False);
            Expect(commandList.Commands[5], Is.InstanceOf<ExtractCommand>().With.Property("FileName").EqualTo("example.zip"));
            Expect(commandList.Commands[6], Is.InstanceOf<MoveCommand>());
        }

        [TestCase("[]")]
        [TestCase("[{}]", ExpectedException = typeof(JsonException))]
        [TestCase("[{unknown: {foo: 1}}]", ExpectedException = typeof(JsonException))]
        [TestCase("[{error: {}}]")]
        [TestCase("[{error: {}}, {error: {}}]")]
        [TestCase("[{error: {}, extract: {}}]", ExpectedException = typeof(JsonException))]
        public void ThrownExceptionFromInvalidJson(string json)
        {
            JsonConvert.DeserializeObject<CommandList>(json);
        }
    }
}
