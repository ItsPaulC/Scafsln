namespace Scafsln.Cli.Dto;

/// <summary>
/// Data Transfer Object for storing template file contents in the database
/// </summary>
public record TemplateFileContent
{
    /// <summary>
    /// Gets or sets the primary key for the template file content
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the .editorconfig template content
    /// </summary>
    public string? EditorconfigTemplate { get; set; }
    
    /// <summary>
    /// Gets or sets the .gitignore template content
    /// </summary>
    public string? GitignoreTemplate { get; set; }
}