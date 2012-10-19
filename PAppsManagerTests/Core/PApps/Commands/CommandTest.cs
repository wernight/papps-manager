using NUnit.Framework;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManagerTests.Core.PApps.Commands
{
    [TestFixture]
    public class CommandTest : AssertionHelper
    {
        [TestCase("Foo/**/?*.exe", Result = @"Foo/.*/[^\\/][^\\/]*\.exe")]
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
            return Command.ValidateRegex(regex) == null;
        }
    }
}