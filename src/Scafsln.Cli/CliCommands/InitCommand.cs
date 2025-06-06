using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Scafsln.Cli.CliCommands;

public class InitCommand : Command<InitCommand.InitSettings>
{
    public class InitSettings : CommandSettings
    {
        [CommandOption("-c|--cpm")]
        [Description("Convert to Nuget CPM in the solution")]
        public bool UseCpm { get; set; }

        [CommandOption("-g|--gitignore")]
        [Description("Add .gitignore file to the solution")]
        public bool UseGitignore { get; set; }

        [CommandOption("-e|--editorconfig")]
        [Description("Add .editorconfig file to the solution")]
        public bool UseEditorConfig { get; set; }

        [CommandOption("-a|--all")]
        [Description("Add all configuration files to the solution")]
        public bool UseAll { get; set; }

        [CommandArgument(0, "[path]")]
        [Description("Path to run init against (defaults to current directory if not specified)")]
        public string Path { get; set; } = Environment.CurrentDirectory;

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = Environment.CurrentDirectory;
                return ValidationResult.Success();
            }

            if (!System.IO.Path.IsPathRooted(Path))
            {
                return ValidationResult.Error("Path must be a full path (e.g., C:\\path\\to\\solution).");
            }

            string? drive = System.IO.Path.GetPathRoot(Path);
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

        bool shouldAddCpm = settings.UseCpm || settings.UseAll;
        bool shouldAddGitignore = settings.UseGitignore || settings.UseAll;
        bool shouldAddEditorConfig = settings.UseEditorConfig || settings.UseAll;

        if (shouldAddCpm)
        {
            AnsiConsole.MarkupLine("[green]Adding CPM...[/]");
            try
            {
                ProjectPrepUtility.CreateDirectoryPackagesPropsFile(settings.Path);
                AnsiConsole.MarkupLine("[green]Successfully created Directory.Packages.props[/]");
            }
            catch (Exception ex) when (ex is ArgumentNullException or ArgumentException or DirectoryNotFoundException)
            {
                AnsiConsole.MarkupLine($"[red]Error creating Directory.Packages.props: {ex.Message}[/]");
                return 1;
            }
        }

        if (shouldAddGitignore)
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

        if (shouldAddEditorConfig)
        {
            AnsiConsole.MarkupLine("[green]Adding .editorconfig...[/]");
            try
            {
                ProjectPrepUtility.CreateEditorConfig(settings.Path);
                AnsiConsole.MarkupLine("[green]Successfully created .editorconfig[/]");
            }
            catch (Exception ex) when (ex is ArgumentNullException or ArgumentException or DirectoryNotFoundException)
            {
                AnsiConsole.MarkupLine($"[red]Error creating .editorconfig: {ex.Message}[/]");
                return 1;
            }
        }

        if (!shouldAddCpm && !shouldAddGitignore && !shouldAddEditorConfig)
            AnsiConsole.MarkupLine("[yellow]No options selected. Use -h or --help to see available options.[/]");

        return 0;
    }
}