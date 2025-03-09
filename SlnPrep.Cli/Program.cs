using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using SlnPrep.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<InitCommand>("init")
        .WithDescription("Initialize solution with common files and configurations");
});

return app.Run(args);

public class InitCommand : Command<InitCommand.InitSettings>
{
    public class InitSettings : CommandSettings
    {
        [CommandOption("-c|--addCpm")]
        [Description("Convert to Nuget CPM to the solution")]
        public bool AddCpm { get; set; }

        [CommandOption("-b|--addBuildProps")]
        [Description("Add Directory.Build.props to the solution")]
        public bool AddBuildProps { get; set; }

        [CommandOption("-g|--addGitignore")]
        [Description("Add .gitignore file to the solution")]
        public bool AddGitignore { get; set; }

        [CommandArgument(0, "<path>")]
        [Description("Path to run init against")]
        public required string Path { get; set; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                return ValidationResult.Error("Path must be provided.");
            }

            if (!System.IO.Path.IsPathRooted(Path))
            {
                return ValidationResult.Error("Path must be a full path (e.g., C:\\path\\to\\solution).");
            }

            var drive = System.IO.Path.GetPathRoot(Path);
            if (!Directory.Exists(drive))
            {
                return ValidationResult.Error($"Drive {drive} does not exist.");
            }

            return ValidationResult.Success();
        }
    }

    public override int Execute(CommandContext context, InitSettings settings)
    {
        AnsiConsole.MarkupLine($"[blue]Running initialization in path: {settings.Path}[/]");

        if (!Directory.Exists(settings.Path))
        {
            AnsiConsole.MarkupLine($"[red]Error: Path {settings.Path} does not exist[/]");
            return 1;
        }

        if (settings.AddCpm)
        {
            AnsiConsole.MarkupLine("[green]Adding CPM...[/]");
            try
            {
                ProjectPrepUtility.CreatePackagesPropsFile(settings.Path);
                AnsiConsole.MarkupLine("[green]Successfully created Directory.Packages.props[/]");
            }
            catch (Exception ex) when (ex is ArgumentNullException or ArgumentException or DirectoryNotFoundException)
            {
                AnsiConsole.MarkupLine($"[red]Error creating Directory.Packages.props: {ex.Message}[/]");
                return 1;
            }
        }
        
        if (settings.AddBuildProps)
        {
            AnsiConsole.MarkupLine("[green]Adding Directory.Build.props...[/]");
            try
            {
                ProjectPrepUtility.CreateBuildPropsFile(settings.Path);
                AnsiConsole.MarkupLine("[green]Successfully created Directory.Build.props[/]");
            }
            catch (Exception ex) when (ex is ArgumentNullException or ArgumentException or DirectoryNotFoundException)
            {
                AnsiConsole.MarkupLine($"[red]Error creating Directory.Build.props: {ex.Message}[/]");
                return 1;
            }
        }
        
        if (settings.AddGitignore)
        {
            AnsiConsole.MarkupLine("[green]Adding .gitignore...[/]");
            try
            {
                ProjectPrepUtility.AddGitIgnore(settings.Path);
                AnsiConsole.MarkupLine("[green]Successfully created .gitignore[/]");
            }
            catch (Exception ex) when (ex is ArgumentNullException or ArgumentException or DirectoryNotFoundException)
            {
                AnsiConsole.MarkupLine($"[red]Error creating .gitignore: {ex.Message}[/]");
                return 1;
            }
        }

        if (!settings.AddCpm && !settings.AddBuildProps && !settings.AddGitignore)
            AnsiConsole.MarkupLine("[yellow]No options selected. Use -h or --help to see available options.[/]");

        return 0;
    }
}
