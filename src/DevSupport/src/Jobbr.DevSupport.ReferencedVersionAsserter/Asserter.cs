using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.String;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    /// <summary>
    /// The asserter base class.
    /// </summary>
    public class Asserter
    {
        private readonly string _packagesFile;
        private readonly string _nuspecDefinitionFile;
        private readonly List<IAssertionRule> _rules = new List<IAssertionRule>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Asserter"/> class.
        /// </summary>
        /// <param name="packagesFile">Name of the package file.</param>
        /// <param name="nuspecDefinitionFile">Name of the .nuspec file.</param>
        public Asserter(string packagesFile, string nuspecDefinitionFile)
        {
            _packagesFile = packagesFile;
            _nuspecDefinitionFile = nuspecDefinitionFile;
        }

        /// <summary>
        /// Adds an assertion rule.
        /// </summary>
        /// <param name="rule">Assertion rule to add.</param>
        /// <returns>Returns a reference to this <see cref="Asserter"/> instance.</returns>
        public Asserter Add(IAssertionRule rule)
        {
            _rules.Add(rule);

            return this;
        }

        /// <summary>
        /// Validates based on the assertion rules.
        /// </summary>
        /// <returns>An <see cref="AssertionResult"/>.</returns>
        public AssertionResult Validate()
        {
            return Execute();
        }

        /// <summary>
        /// Adds the given assertion rule to the <see cref="Asserter"/> and validates.
        /// </summary>
        /// <param name="rule">Rule to add.</param>
        /// <returns>An <see cref="AssertionResult"/>.</returns>
        public AssertionResult Validate(IAssertionRule rule)
        {
            _rules.Add(rule);

            return Execute();
        }

        /// <summary>
        /// Resolves the path name for the packages configuration.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="fileName">Name of the file. Defaults to "packages.config".</param>
        /// <returns>Filepath to packages configuration.</returns>
        public static string ResolvePackagesConfig(string projectName, string fileName = "packages.config")
        {
            return Path.Combine(GetSolutionDirectory(), projectName, fileName);
        }

        /// <summary>
        /// Resolves solution path for the file.
        /// </summary>
        /// <param name="fileName">Name of the target file.</param>
        /// <returns>Path from solution root combined with filename.</returns>
        public static string ResolveRootFile(string fileName)
        {
            return Path.Combine(GetSolutionDirectory(), fileName);
        }

        /// <summary>
        /// Resolve file path for a project file.
        /// </summary>
        /// <param name="projectName">Name of the target project.</param>
        /// <param name="fileName">Name of the target file.</param>
        /// <returns>Path from solution root combined with project name and filename.</returns>
        public static string ResolveProjectFile(string projectName, string fileName)
        {
            return Path.Combine(GetSolutionDirectory(), projectName, fileName);
        }

        /// <summary>
        /// Gets solution file path.
        /// </summary>
        /// <returns>Solution file path.</returns>
        public static string GetSolutionDirectory()
        {
            var slnFile = Empty;
            var currentPath = Directory.GetCurrentDirectory();

            while (IsNullOrWhiteSpace(slnFile) && currentPath.Length > 3)
            {
                if (Directory.EnumerateFiles(currentPath, "*.sln").Any())
                {
                    return currentPath;
                }

                currentPath = Directory.GetParent(currentPath).FullName;
            }

            return null;
        }

        private AssertionResult Execute()
        {
            var assertionResult = new AssertionResult();

            var extension = Path.GetExtension(_packagesFile);

            var packageDependencies = extension == ".csproj" ? new ProjectParser(_packagesFile).Dependencies : new PackagesParser(_packagesFile).Dependencies;
            var nuspecDependencies = new NuspecParser(_nuspecDefinitionFile).Dependencies;

            foreach (var rule in _rules)
            {
                var validationResult = rule.Validate(nuspecDependencies, packageDependencies, out var message);

                if (!validationResult)
                {
                    var messages = message.Split('\n');
                    foreach (var s in messages)
                    {
                        assertionResult.AddMessage(rule.GetType().Name, s);
                    }
                }
            }

            assertionResult.IsSuccessful = !assertionResult.Messages.Any();

            return assertionResult;
        }
    }
}
