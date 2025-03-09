namespace Scafsln.Cli.Tests;

public class FileContentsUtilityTests
{
    [Fact]
    public async Task TestUpdateAndReset_Gitignore()
    {
        // Load initial .gitignore content
        string initialContent = FileContentUtility.GitIgnoreContent;

        string assetsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        string gitignorePath = Path.Combine(assetsDirectory, ".gitignore");

        string gitignoreFileContent = await File.ReadAllTextAsync(gitignorePath);

        // Update .gitignore content
        FileContentUtility.UpdateGitIgnoreContent(gitignorePath);

        // Load updated .gitignore content
        string updatedContent = FileContentUtility.GitIgnoreContent;

        // Assert that the updated content is as expected
        Assert.Equal(gitignoreFileContent, updatedContent);

        // Reset .gitignore content
        FileContentUtility.Reset();

        // Load reset .gitignore content
        string resetContent = FileContentUtility.GitIgnoreContent;

        // Assert that the reset content matches the initial content
        Assert.Equal(initialContent, resetContent);
    }

    [Fact]
    public async Task TestUpdateAndReset_EditorConfig()
    {
        // Load initial .editorconfig content
        string initialContent = FileContentUtility.EditorConfigContent;

        string assetsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        string editorconfigPath = Path.Combine(assetsDirectory, ".editorconfig");

        string editorconfigFileContent = await File.ReadAllTextAsync(editorconfigPath);

        // Update .editorconfig content
        FileContentUtility.UpdateEditorconfigContent(editorconfigPath);

        // Load updated .editorconfig content
        string updatedContent = FileContentUtility.EditorConfigContent;

        // Assert that the updated content is as expected
        Assert.Equal(editorconfigFileContent, updatedContent);

        // Reset .editorconfig content
        FileContentUtility.Reset();

        // Load reset .editorconfig content
        string resetContent = FileContentUtility.EditorConfigContent;

        // Assert that the reset content matches the initial content
        Assert.Equal(initialContent, resetContent);
    }
}