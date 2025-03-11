using Scafsln.Cli.Dto;
using Scafsln.Cli.Services;

namespace Scafsln.Cli;

/// <summary>
/// Utility class providing access to standard configuration file contents
/// </summary>
public static class FileContentUtility
{
    /// <summary>
    /// Gets the default .gitignore content for .NET projects
    /// </summary>
    public static string GitIgnoreContent => LoadGitIgnoreTemplate();

    /// <summary>
    /// Gets the default .editorconfig content for .NET projects
    /// </summary>
    public static string EditorConfigContent => LoadEditorconfigTemplate();

    /// <summary>
    /// Updates the .gitignore template by copying the contents from the specified file
    /// </summary>
    /// <param name="sourcePath">The full path to the .gitignore file to read from</param>
    /// <exception cref="ArgumentNullException">Thrown when path is null</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    /// <exception cref="IOException">Thrown when there's an error creating the Templates directory or saving the file</exception>
    public static void UpdateGitIgnoreContent(string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"File not found: {sourcePath}");
        }

        // Read the contents from the provided file and update in database
        string content = File.ReadAllText(sourcePath);
        
        using TemplateService service = new();
        service.UpdateGitignoreTemplateAsync(content).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Updates the .editorconfig template by copying the contents from the specified file
    /// </summary>
    /// <param name="sourcePath">The full path to the .editorconfig file to read from</param>
    /// <exception cref="ArgumentNullException">Thrown when path is null</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    /// <exception cref="IOException">Thrown when there's an error creating the Templates directory or saving the file</exception>
    public static void UpdateEditorconfigContent(string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"File not found: {sourcePath}");
        }

        // Read the contents from the provided file and update in database
        string content = File.ReadAllText(sourcePath);
        
        using TemplateService service = new();
        service.UpdateEditorConfigTemplateAsync(content).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Resets the template files to their default content
    /// </summary>
    public static void Reset()
    {
        using TemplateService service = new();
        service.ResetTemplatesAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Loads the .gitignore template content from the database or default content
    /// </summary>
    /// <returns>The .gitignore template content</returns>
    private static string LoadGitIgnoreTemplate()
    {
        try
        {
            using var service = new TemplateService();
            TemplateFileContent? template = service.GetTemplateContentAsync().GetAwaiter().GetResult();
            return template?.GitignoreTemplate ?? FileContents.GitIgnoreContent;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading .gitignore template: {ex.Message}");
            return FileContents.GitIgnoreContent;
        }
    }

    /// <summary>
    /// Loads the .editorconfig template content from the database or default content
    /// </summary>
    /// <returns>The .editorconfig template content</returns>
    private static string LoadEditorconfigTemplate()
    {
        try
        {
            using var service = new TemplateService();
            TemplateFileContent? template = service.GetTemplateContentAsync().GetAwaiter().GetResult();
            return template?.EditorconfigTemplate ?? FileContents.EditorConfigContent;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading .editorconfig template: {ex.Message}");
            return FileContents.EditorConfigContent;
        }
    }
}