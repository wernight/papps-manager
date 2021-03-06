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
                                "{delete: {files: 'foo.tmp'}}," +
                                "{display: {message: 'Hello World!'}}," +
                                "{download: {url: 'http://example.com', destination_file: 'example.zip'}}," +
                                "{error: {message: 'Hello World!'}}," +
                                "{eula: {text: 'Terms and Conditions'}}," +
                                "{execute: {fail_on_error: false}}," +
                                "{extract: {file: 'example.zip'}}," +
                                "{move: {}}," +
                                "{shortcut: {name: 'foo.lnk', target: 'paf.exe'}}," +
                                "{uniextract: {file: 'paf.exe'}}" +
                                "]";

            var commandList = JsonConvert.DeserializeObject<CommandList>(json);

            Expect(commandList, Has.Count.EqualTo(10));

            Expect(commandList[0], Is.InstanceOf<DeleteCommand>());
            Expect(commandList[1], Is.InstanceOf<DisplayCommand>());
            Expect(commandList[2], Is.InstanceOf<DownloadCommand>().With.Property("Url").EqualTo("http://example.com"));
            Expect(commandList[3], Is.InstanceOf<ErrorCommand>().With.Property("Message").EqualTo("Hello World!"));
            Expect(commandList[4], Is.InstanceOf<EulaCommand>().With.Property("Text").Not.Empty);
            Expect(commandList[5], Is.InstanceOf<ExecuteCommand>().With.Property("FailOnError").False);
            Expect(commandList[6], Is.InstanceOf<ExtractCommand>().With.Property("FileName").EqualTo("example.zip"));
            Expect(commandList[7], Is.InstanceOf<MoveCommand>());
            Expect(commandList[8], Is.InstanceOf<ShortcutCommand>());
            Expect(commandList[9], Is.InstanceOf<UniExtractCommand>().With.Property("FileName").EqualTo("paf.exe"));
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
