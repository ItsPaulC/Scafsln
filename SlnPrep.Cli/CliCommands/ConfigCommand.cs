using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace SlnPrep.Cli.CliCommands;

public class ConfigCommand : Command<ConfigCommand.ConfigSettings>
{
    public class ConfigSettings : CommandSettings
    {
        [CommandOption("--show-gitignore")]
        [Description("Show the contents of .gitignore")]
        public bool ShowGitignore { get; set; }

        [CommandOption("--show-editorconfig")]
        [Description("Show the contents of .editorconfig")]
        public bool ShowEditorconfig { get; set; }

        [CommandOption("--change-editorconfig")]
        [Description("Change the .editorconfig file")]
        public string? EditorConfigPath { get; set; }

        [CommandOption("--change-gitignore")]
        [Description("Change the .gitignore file")]
        public string? GitignorePath { get; set; }

        [CommandArgument(0, "[path]")]
        [Description("Path to the solution directory")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                return ValidationResult.Error("Path must be provided.");
            }

            if (!Directory.Exists(Path))
            {
                return ValidationResult.Error($"Directory {Path} does not exist.");
            }

            if (EditorConfigPath != null && !File.Exists(EditorConfigPath))
            {
                return ValidationResult.Error($"Editor config file {EditorConfigPath} does not exist.");
            }

            if (GitignorePath != null && !File.Exists(GitignorePath))
            {
                return ValidationResult.Error($"Gitignore file {GitignorePath} does not exist.");
            }

            return ValidationResult.Success();
        }
    }

    public override int Execute(CommandContext context, ConfigSettings settings)
    {
        if (settings.ShowGitignore)
        {
            var gitignorePath = Path.Combine(settings.Path, ".gitignore");
            if (!File.Exists(gitignorePath))
            {
                AnsiConsole.MarkupLine("[red]No .gitignore file found in the specified directory.[/]");
                return 1;
            }
            var content = File.ReadAllText(gitignorePath);
            AnsiConsole.MarkupLine("[blue].gitignore contents:[/]");
            AnsiConsole.WriteLine(content);
        }

        if (settings.ShowEditorconfig)
        {
            var editorconfigPath = Path.Combine(settings.Path, ".editorconfig");
            if (!File.Exists(editorconfigPath))
            {
                AnsiConsole.MarkupLine("[red]No .editorconfig file found in the specified directory.[/]");
                return 1;
            }
            var content = File.ReadAllText(editorconfigPath);
            AnsiConsole.MarkupLine("[blue].editorconfig contents:[/]");
            AnsiConsole.WriteLine(content);
        }

        if (settings.EditorConfigPath != null)
        {
            try
            {
                File.Copy(settings.EditorConfigPath, Path.Combine(settings.Path, ".editorconfig"), true);
                AnsiConsole.MarkupLine("[green]Successfully updated .editorconfig[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error updating .editorconfig: {ex.Message}[/]");
                return 1;
            }
        }

        if (settings.GitignorePath != null)
        {
            try
            {
                File.Copy(settings.GitignorePath, Path.Combine(settings.Path, ".gitignore"), true);
                AnsiConsole.MarkupLine("[green]Successfully updated .gitignore[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error updating .gitignore: {ex.Message}[/]");
                return 1;
            }
        }

        if (!settings.ShowGitignore && !settings.ShowEditorconfig && 
            settings.EditorConfigPath == null && settings.GitignorePath == null)
        {
            AnsiConsole.MarkupLine("[yellow]No options selected. Use -h or --help to see available options.[/]");
        }

        return 0;
    }
}