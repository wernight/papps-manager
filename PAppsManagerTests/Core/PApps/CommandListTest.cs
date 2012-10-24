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
                                "{execute: {fail_on_error: false}}," +
                                "{extract: {file: 'example.zip'}}," +
                                "{move: {}}," +
                                "{7zip: {arguments: 'x paf.exe'}}" +
                                "]";

            var commandList = JsonConvert.DeserializeObject<CommandList>(json);

            Expect(commandList, Has.Count.EqualTo(8));

            Expect(commandList[0], Is.InstanceOf<DeleteCommand>());
            Expect(commandList[1], Is.InstanceOf<DisplayCommand>());
            Expect(commandList[2], Is.InstanceOf<DownloadCommand>().With.Property("Url").EqualTo("http://example.com"));
            Expect(commandList[3], Is.InstanceOf<ErrorCommand>().With.Property("Message").EqualTo("Hello World!"));
            Expect(commandList[4], Is.InstanceOf<ExecuteCommand>().With.Property("FailOnError").False);
            Expect(commandList[5], Is.InstanceOf<ExtractCommand>().With.Property("FileName").EqualTo("example.zip"));
            Expect(commandList[6], Is.InstanceOf<MoveCommand>());
            Expect(commandList[7], Is.InstanceOf<SevenZipCommand>().With.Property("Arguments").EqualTo("x paf.exe"));
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
