using System;
using System.Collections.Generic;
using NUnit.Framework;
using PAppsManager.Core.PApps;

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
                                                       "release_date: '2000-01-01'," +
                                                       "dependencies: ['papp://example.com/bar.json']," +
                                                       "install_commands: [{download: {}}]" +
                                                       "}"
                    },
                    {
                        "http://example.com/bar.json", "{" +
                                                       "name: 'Bar'," +
                                                       "version: '1.0.0.0'," +
                                                       "release_date: '2000-01-01'," +
                                                       "dependencies: []," +
                                                       "install_commands: [{download: {}}]" +
                                                       "}"
                    },
                };

            PortableApplication application = PortableApplication.LoadFromUrl("papp://example.com/foo.json",
                                                                              url => fakeWebClient[url]);

            Expect(application.Name, Is.EqualTo("Foo"));
            Expect(application.Version, Is.EqualTo("1.0.0.0"));
            Expect(application.ReleaseDate, Is.EqualTo(new DateTime(2000, 01, 01)));
            Expect(application.Dependencies, Has.Length.EqualTo(1));
            Expect(application.InstallCommands, Has.Count.EqualTo(1));

            Expect(application.Dependencies[0].Name, Is.EqualTo("Bar"));
        }
    }
}