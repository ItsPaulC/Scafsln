using Microsoft.EntityFrameworkCore;
using Scafsln.Cli.Dto;
using System.Diagnostics.CodeAnalysis;

namespace Scafsln.Cli.Data;

/// <summary>
/// Database context for managing template file contents
/// </summary>
public class TemplateDbContext : DbContext
{
    /// <summary>
    /// Path to the SQLite database file
    /// </summary>
    private static readonly string s_dbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Scafsln",
        "templates.db");

    /// <summary>
    /// Gets or sets the template file contents
    /// </summary>
    public DbSet<TemplateFileContent> TemplateContents { get; set; } = null!;

    /// <summary>
    /// Configures the database connection
    /// </summary>
    /// <param name="optionsBuilder">The options builder to configure</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Ensure the directory exists
        string dbDirectory = Path.GetDirectoryName(s_dbPath)!;
        if (!Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        optionsBuilder.UseSqlite($"Data Source={s_dbPath}");
    }

    /// <summary>
    /// Seeds the database with default data if it's empty
    /// </summary>
    public void EnsureSeeded()
    {
        try
        {
            this.Database.EnsureCreated();

            // Test if the database has the new schema by trying to access CopilotInstructionsTemplate
            _ = TemplateContents.Select(t => t.CopilotInstructionsTemplate).FirstOrDefault();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("no such column"))
        {
            // Database schema is outdated, recreate it
            this.Database.EnsureDeleted();
            this.Database.EnsureCreated();
        }

        // Only seed if no template content exists
        if (!TemplateContents.Any())
        {
            TemplateContents.Add(new TemplateFileContent
            {
                EditorconfigTemplate = FileContents.EditorConfigContent,
                GitignoreTemplate = FileContents.GitIgnoreContent,
                CopilotInstructionsTemplate = FileContents.CopilotInstructions
            });

            SaveChanges();
        }
    }
}