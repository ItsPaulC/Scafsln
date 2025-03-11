using Microsoft.EntityFrameworkCore;
using Scafsln.Cli.Data;
using Scafsln.Cli.Dto;

namespace Scafsln.Cli.Services;

/// <summary>
/// Service for managing template file contents in the database
/// </summary>
public class TemplateService : IDisposable
{
    private readonly TemplateDbContext _dbContext;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateService"/> class
    /// </summary>
    public TemplateService()
    {
        _dbContext = new TemplateDbContext();
        _dbContext.EnsureSeeded();
    }

    /// <summary>
    /// Gets the current template file content from the database
    /// </summary>
    /// <returns>The template file content</returns>
    public async Task<TemplateFileContent?> GetTemplateContentAsync()
    {
        return await _dbContext.TemplateContents.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Updates the editor config template content in the database
    /// </summary>
    /// <param name="content">The new editor config template content</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task UpdateEditorConfigTemplateAsync(string content)
    {
        TemplateFileContent template = await GetOrCreateTemplateAsync();
        template.EditorconfigTemplate = content;
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Updates the gitignore template content in the database
    /// </summary>
    /// <param name="content">The new gitignore template content</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task UpdateGitignoreTemplateAsync(string content)
    {
        TemplateFileContent template = await GetOrCreateTemplateAsync();
        template.GitignoreTemplate = content;
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Resets the template file content to default values from FileContents
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task ResetTemplatesAsync()
    {
        // Get all templates from database
        List<TemplateFileContent> templates = await _dbContext.TemplateContents.ToListAsync();
        
        if (templates.Any())
        {
            // Delete all existing templates first
            _dbContext.TemplateContents.RemoveRange(templates);
            await _dbContext.SaveChangesAsync();
        }
        
        // Add a new template with default values
        TemplateFileContent newTemplate = new()
        {
            EditorconfigTemplate = FileContents.EditorConfigContent,
            GitignoreTemplate = FileContents.GitIgnoreContent
        };
        
        _dbContext.TemplateContents.Add(newTemplate);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the existing template content or creates a new one if none exists
    /// </summary>
    /// <returns>The template content entity</returns>
    private async Task<TemplateFileContent> GetOrCreateTemplateAsync()
    {
        var template = await _dbContext.TemplateContents.FirstOrDefaultAsync();
        
        if (template == null)
        {
            template = new TemplateFileContent
            {
                EditorconfigTemplate = FileContents.EditorConfigContent,
                GitignoreTemplate = FileContents.GitIgnoreContent
            };
            
            _dbContext.TemplateContents.Add(template);
            await _dbContext.SaveChangesAsync();
        }
        
        return template;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of resources used by this class
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }

            _disposed = true;
        }
    }
}