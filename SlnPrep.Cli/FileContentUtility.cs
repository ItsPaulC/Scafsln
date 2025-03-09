namespace SlnPrep.Cli;

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
    /// <param name="path">The full path to the .gitignore file to read from</param>
    /// <exception cref="ArgumentNullException">Thrown when path is null</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    /// <exception cref="IOException">Thrown when there's an error creating the Templates directory or saving the file</exception>
    public static void UpdateGitIgnoreContent(string path)
    {
        UpdateTemplateContent(path, "gitignore-template");
    }

    /// <summary>
    /// Updates the .editorconfig template by copying the contents from the specified file
    /// </summary>
    /// <param name="path">The full path to the .editorconfig file to read from</param>
    /// <exception cref="ArgumentNullException">Thrown when path is null</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    /// <exception cref="IOException">Thrown when there's an error creating the Templates directory or saving the file</exception>
    public static void UpdateEditorconfigContent(string path)
    {
        UpdateTemplateContent(path, "editorconfig-template");
    }

    private static string LoadGitIgnoreTemplate()
    {
        return LoadTemplateContent("gitignore-template", FileContents.GitIgnoreContent);
    }

    private static string LoadEditorconfigTemplate()
    {
        return LoadTemplateContent("editorconfig-template", FileContents.EditorConfigContent);
    }

    private static string LoadTemplateContent(string templateFileName, string defaultContent)
    {
        try
        {
            string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFileName);
            if (File.Exists(templatePath))
            {
                return File.ReadAllText(templatePath);
            }
        }
        catch (IOException)
        {
            // If there's any IO error, fall back to the default content
        }

        return defaultContent;
    }

    private static void UpdateTemplateContent(string sourcePath, string templateFileName)
    {
        if (sourcePath is null)
            throw new ArgumentNullException(nameof(sourcePath));
            
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Path cannot be empty or whitespace", nameof(sourcePath));

        if (!File.Exists(sourcePath))
            throw new FileNotFoundException($"File not found: {sourcePath}");

        // Read the contents of the provided file
        string content = File.ReadAllText(sourcePath);

        // Create Templates directory if it doesn't exist
        string templatesDir = Path.Combine(AppContext.BaseDirectory, "Templates");
        Directory.CreateDirectory(templatesDir);

        // Save the content to the template file
        string templatePath = Path.Combine(templatesDir, templateFileName);
        File.WriteAllText(templatePath, content);
    }
}