using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PAppsManager.Core.PApps;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps
{
    [TestFixture]
    public class PortableApplicationTest : AssertionHelper
    {
        [Test]
        public void CanDeserializeFromJson()
        {
            var fakeWebClient = new Dictionary<string, string>
                {
                    {
                        "http://example.com/foo.json", "{" +
                                                       "name: 'Foo'," +
                                                       "version: '1.0.0.0'," +
                                                       "release_date: '2000-01-02T01:23:45'," +
                                                       "dependencies: ['papp://example.com/bar.json']," +
                                                       "install_commands: [{download: {}}]," +
                                                       "data_directory: \"Data\"" +
                                                       "}"
                    },
                    {
                        "http://example.com/bar.json", "{" +
                                                       "name: 'Bar'," +
                                                       "version: '1.0.0.0'," +
                                                       "release_date: '2000-01-01T00:00:00'," +
                                                       "dependencies: []," +
                                                       "install_commands: [{download: {}}]," +
                                                       "data_directory: \"\"" +
                                                       "}"
                    },
                };

            PortableApplication application = PortableApplication.LoadFromUrl("papp://example.com/foo.json",
                                                                              url => fakeWebClient[url]);

            Expect(application.Name, Is.EqualTo("Foo"));
            Expect(application.Version, Is.EqualTo("1.0.0.0"));
            Expect(application.ReleaseDate, Is.EqualTo(new DateTime(2000, 01, 02, 1, 23, 45)));
            Expect(application.Dependencies, Has.Length.EqualTo(1));
            Expect(application.InstallCommands, Has.Count.EqualTo(1));
            Expect(application.DataDirectory, Is.EqualTo("Data"));

            Expect(application.Dependencies[0].Name, Is.EqualTo("Bar"));
        }

        [Test]
        public void CanSerializeToJsonWithoutInstallationCommands()
        {
            var application = new PortableApplication
                {
                    Url = "http://example.com/foo.json",
                    Name = "UnitTest",
                    Version = "1.0.0.0",
                    ReleaseDate = new DateTime(2000, 1, 25, 1, 23, 45),
                    Dependencies = new[]
                        {
                            new PortableApplication {Url = "http://example.com/bar.json"}
                        },
                    InstallCommands = new CommandList {new Mock<ICommand>().Object},
                    DataDirectory = "",
                };

            string json = application.ToJson();

            Expect(json, Is.Not.Null);
            Expect(json, Is.StringContaining("http://example.com/foo.json"));
            Expect(json, Is.StringContaining("http://example.com/bar.json"));

            // Can deserialize it back to the application.
            var fakeWebClient = new Dictionary<string, string>
                {
                    {"http://example.com/foo.json", json},
                    {"http://example.com/bar.json", "{}"},
                };
            var deserialized = PortableApplication.LoadFromUrl("http://example.com/foo.json", url => fakeWebClient[url]);

            Expect(deserialized.Url, Is.EqualTo(application.Url));
            Expect(deserialized.Name, Is.EqualTo(application.Name));
            Expect(deserialized.Version, Is.EqualTo(application.Version));
            Expect(deserialized.ReleaseDate, Is.EqualTo(application.ReleaseDate));
            Expect(deserialized.Dependencies, Is.EquivalentTo(application.Dependencies));
            Expect(deserialized.InstallCommands, Is.Null);
            Expect(deserialized.DataDirectory, Is.Empty);
        }
    }
}