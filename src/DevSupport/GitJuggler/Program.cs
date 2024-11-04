using ConsoleTools;
using LibGit2Sharp;

var repositories = new List<string>
{
    "jobbr-server",
    "jobbr-cm-management",
    "jobbr-cm-registration",
    "jobbr-cm-execution",
    "jobbr-execution-forked",
    "jobbr-runtime",
    "jobbr-cm-jobstorage",
    "jobbr-storage-mssql",
    "jobbr-cm-artefactstorage",
    "jobbr-artefactstorage-filesystem",
    "jobbr-dashboard",
    "jobbr-webapi",
};

var menu = new ConsoleMenu(args, level: 0)
    .Add("Clone all repositories", CloneRepositories)
    .Add("Create and/or switch branch", () => throw new NotImplementedException())
    .Add("Update package...", () => throw new NotImplementedException())
    .Add("Exit", () => Environment.Exit(0));

while (true)
{
    menu.Show();
}

void CloneRepositories(ConsoleMenu activeMenu)
{
    activeMenu.CloseMenu();

    var cloneDirectory = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."), "..", "..", "..");

    while (true)
    {
        Console.Write($"Do you want to use '{Path.GetFullPath(cloneDirectory)}' as clone directory? [no/YES]: ");
        var response = Console.ReadLine();

        if (IsYes(response))
        {
            break;
        }
        
        Console.Write("Specify clone directory: ");
        cloneDirectory = Console.ReadLine() ?? cloneDirectory;
    }
    
    var cloneOptions = new CloneOptions
    {
        RecurseSubmodules = true,
        Checkout = true,
        BranchName = "develop",
        RepositoryOperationStarting = context =>
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", context.RecursionDepth))}Cloning {context.RemoteUrl} ...");
            return true;
        }
    };

    foreach (var repository in repositories.Concat(new List<string>{ "demo" }))
    {
        try
        {
            Repository.Clone($"https://github.com/jobbrIO/{repository}.git", Path.Combine(cloneDirectory, repository), cloneOptions);
        }
        catch (LibGit2SharpException e)
        {
            Console.WriteLine($"Unable to clone '{repository}' due to: {e.Message}");
        }
    }

    try
    {
        cloneOptions.BranchName = "master";
        Repository.Clone($"https://github.com/jobbrIO/docs.git", Path.Combine(cloneDirectory, "docs"), cloneOptions);
    }
    catch (LibGit2SharpException e)
    {
        Console.WriteLine($"Unable to clone 'docs' due to: {e.Message}");
    }

    Console.WriteLine("Successfully cloned all repositories!");
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}

bool IsYes(string? s)
{
    return s is null
           || s.Length == 0
           || s.Equals("YES", StringComparison.InvariantCultureIgnoreCase);
}