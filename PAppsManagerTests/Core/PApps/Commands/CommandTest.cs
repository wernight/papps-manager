using NUnit.Framework;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps.Commands
{
    [TestFixture]
    public class CommandTest : AssertionHelper
    {
        [TestCase("Foo/**/?*.exe", Result = @"Foo\\.*\\[^\\/][^\\/]*\.exe")]
        [TestCase("*****?***", Result = @".*[^\\/].*")]
        public string WildcardToRegex(string wildcard)
        {
            return Command.WildcardToRegex(wildcard);
        }

        [TestCase(@"^.*/[^\\/]*\.exe$", Result = true)]
        [TestCase(@"^.*/[^\\/*\.exe$", Result = false)]
        [TestCase(@"^.*/([^\\/]*\.exe$", Result = false)]
        public bool ValidateRegex(string regex)
        {
            return Command.ValidateRegex(regex, "Regex") == null;
        }

        [TestCase(@" ", Result = false)]
        [TestCase(@"Foo", Result = true)]
        [TestCase(@"Foo/Bar\.file", Result = true)]
        [TestCase(@"Foo/..", Result = true)]
        [TestCase(@"Foo\Bar\", Result = true)]
        [TestCase(@"../Foo", Result = false)]
        [TestCase(@"C:\Foo", Result = false)]
        [TestCase(@"\\Foo", Result = false)]
        [TestCase(@"/Foo", Result = false)]
        [TestCase(@"*", Result = false)]
        [TestCase(@"?", Result = false)]
        [TestCase(@"Foo<Bar", Result = false)]
        [TestCase(@"Foo|Bar", Result = false)]
        [TestCase(@"Foo""Bar", Result = false)]
        public bool ValidateRelativePath(string path)
        {
            return Command.ValidateRelativePath(() => path) == null;
        }
    }
}