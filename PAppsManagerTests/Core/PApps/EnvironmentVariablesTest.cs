using System;
using System.Collections;
using NUnit.Framework;
using PAppsManager.Core.PApps;

namespace PAppsManagerTests.Core.PApps
{
    [TestFixture]
    public class EnvironmentVariablesTest : AssertionHelper
    {
        [TestCaseSource("ExpandDataSource")]
        public string Expand(string contracted)
        {
            return new EnvironmentVariables().Expand(contracted);
        }

        [TestCaseSource("ContractDataSource")]
        public string Contract(string expanded)
        {
            return new EnvironmentVariables().Contract(expanded);
        }

        public static IEnumerable ExpandDataSource
        {
            get
            {
                string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                yield return new TestCaseData("Hello % ")
                    .Returns("Hello % ")
                    .SetDescription("No replacement.");

                yield return new TestCaseData("%ProgramFilesX86%")
                    .Returns(programFilesX86)
                    .SetDescription("Simple replacement.");

                yield return new TestCaseData("%MyDOCumENTS%")
                    .Returns(myDocuments)
                    .SetDescription("Ignores case.");

                yield return new TestCaseData("%%%MyDocuments%%ProgramFilesX86%%%ProgramFilesX86%%")
                    .Returns("%" + myDocuments + programFilesX86 + "%ProgramFilesX86%")
                    .SetDescription("Replaces %% by %.");
            }
        }

        public static IEnumerable ContractDataSource
        {
            get
            {
                string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                yield return new TestCaseData("Hello % ")
                    .Returns("Hello %% ")
                    .SetDescription("Replaces % by %%.");

                yield return new TestCaseData(programFilesX86)
                    .Returns("%ProgramFilesX86%")
                    .SetDescription("Simple replacement.");

                yield return new TestCaseData(myDocuments.ToUpperInvariant())
                    .Returns("%MyDocuments%")
                    .SetDescription("Ignores case.");

                yield return new TestCaseData("%" + myDocuments + programFilesX86 + "%ProgramFilesX86%")
                    .Returns("%%%MyDocuments%%ProgramFilesX86%%%ProgramFilesX86%%")
                    .SetDescription("Multiple matches and replaces % by %%.");
            }
        }
    }
}