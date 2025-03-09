using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Scafsln.Cli.CliCommands;

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
        public string? NewEditorconfig { get; set; }

        [CommandOption("--change-gitignore")]
        [Description("Change the .gitignore file")]
        public string? NewGitignore { get; set; }

        [CommandOption("--reset")]
        [Description("Reset templates to default values")]
        public bool Reset { get; set; }

        [CommandArgument(0, "[path]")]
        [Description("Path to the solution directory")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();

        public override ValidationResult Validate()
        {
            // Only validate path if we're doing an operation that requires it
            if (NewEditorconfig != null || NewGitignore != null)
            {
                if (string.IsNullOrWhiteSpace(Path))
                {
                    return ValidationResult.Error("Path must be provided when using --change-editorconfig or --change-gitignore.");
                }

                if (!Directory.Exists(Path))
                {
                    return ValidationResult.Error($"Directory {Path} does not exist.");
                }
            }

            if (NewEditorconfig != null && !File.Exists(NewEditorconfig))
            {
                return ValidationResult.Error($"Editor config file {NewEditorconfig} does not exist.");
            }

            if (NewGitignore != null && !File.Exists(NewGitignore))
            {
                return ValidationResult.Error($"Gitignore file {NewGitignore} does not exist.");
            }

            return ValidationResult.Success();
        }
    }

    public override int Execute(CommandContext context, ConfigSettings settings)
    {
        if (settings.Reset)
        {
            if (AnsiConsole.Confirm("This will reset any templates. Are you sure you want to continue?"))
            {
                FileContentUtility.Reset();
                AnsiConsole.MarkupLine("[green]Templates have been reset to default values[/]");
                return 0;
            }
            return 0;
        }

        if (settings.ShowGitignore)
        {
            AnsiConsole.MarkupLine("[blue].gitignore contents:[/]");
            AnsiConsole.WriteLine(FileContentUtility.GitIgnoreContent);
        }

        if (settings.ShowEditorconfig)
        {
            AnsiConsole.MarkupLine("[blue].editorconfig contents:[/]");
            AnsiConsole.WriteLine(FileContentUtility.EditorConfigContent);
        }

        if (settings.NewEditorconfig != null)
        {
            try
            {
                FileContentUtility.UpdateEditorconfigContent(settings.NewEditorconfig);
                AnsiConsole.MarkupLine("[green]Successfully saved new .editorconfig template[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error updating .editorconfig template: {ex.Message}[/]");
                return 1;
            }
        }

        if (settings.NewGitignore != null)
        {
            try
            {
                FileContentUtility.UpdateGitIgnoreContent(settings.NewGitignore);
                AnsiConsole.MarkupLine("[green]Successfully saved new .gitignore template[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error updating .gitignore template: {ex.Message}[/]");
                return 1;
            }
        }

        if (!settings.ShowGitignore && !settings.ShowEditorconfig && 
            settings.NewEditorconfig == null && settings.NewGitignore == null &&
            !settings.Reset)
        {
            AnsiConsole.MarkupLine("[yellow]No options selected. Use -h or --help to see available options.[/]");
        }

        return 0;
    }
}