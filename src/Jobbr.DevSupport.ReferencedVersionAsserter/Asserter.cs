using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.String;

namespace Jobbr.DevSupport.ReferencedVersionAsserter
{
    public class Asserter
    {
        private readonly string packagesFile;
        private readonly string nuspecDefinitionFile;
        private readonly List<IAssertionRule> rules = new List<IAssertionRule>();

        public Asserter(string packagesFile, string nuspecDefinitionFile)
        {
            this.packagesFile = packagesFile;
            this.nuspecDefinitionFile = nuspecDefinitionFile;
        }

        public Asserter Add(IAssertionRule rule)
        {
            this.rules.Add(rule);

            return this;
        }

        public AssertionResult Validate()
        {
            return this.Execute();
        }

        public AssertionResult Validate(IAssertionRule rule)
        {
            this.rules.Add(rule);

            return this.Execute();
        }

        private AssertionResult Execute()
        {
            var assertionResult = new AssertionResult();

            List<NuspecDependency> packageDependencies;
            var extension = Path.GetExtension(this.packagesFile);

            if (extension == ".csproj")
            {
                packageDependencies = new ProjectParser(this.packagesFile).Dependencies;
            }
            else
            {
                packageDependencies = new PackagesParser(this.packagesFile).Dependencies;
            }
             
            var nuspecDependencies = new NuspecParser(this.nuspecDefinitionFile).Dependencies;

            foreach (var rule in this.rules)
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

        public static string ResolvePackagesConfig(string projectName, string fileName = "packages.config")
        {
            return Path.Combine(GetSolutionDirectory(), projectName, fileName);
        }

        public static string ResolveRootFile(string fileName)
        {
            return Path.Combine(GetSolutionDirectory(), fileName);
        }

        public static string ResolveProjectFile(string projectName, string fileName)
        {
            return Path.Combine(GetSolutionDirectory(), projectName, fileName);
        }

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
    }

    public interface IAssertionRule
    {
        bool Validate(List<NuspecDependency> nuspecDependencies, List<NuspecDependency> packageDependencies, out string message);
    }

    public class AssertionResult
    {
        internal void AddMessage(string name, string message)
        {
            this.Messages.Add($"[{name}] {message}");
        }

        public List<string> Messages { get; } = new List<string>();

        public bool IsSuccessful { get; set; }

        public string Message => "Reason(s) below:\n\n" + Join("\n", this.Messages) + "\n";
    }
}
