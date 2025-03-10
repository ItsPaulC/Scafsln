using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Scafsln.Cli;

/// <summary>
/// Utility class for project preparation tasks
/// </summary>
public static class ProjectPrepUtility
{
    private record PackageInfo(string Name, string Version, bool HasVersionConstraint);
    private static readonly Regex VersionConstraintPattern = new(@"[\*\>\<\^\~\[\]]");

    /// <summary>
    /// Creates a Directory.Packages.props file at the specified path for central package management
    /// </summary>
    /// <param name="conversionPath">The full path where the Directory.Packages.props file should be created</param>
    /// <exception cref="ArgumentNullException">Thrown when conversionPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when conversionPath is empty or whitespace</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist</exception>
    public static void CreateDirectoryPackagesPropsFile(string conversionPath)
    {
        if (conversionPath is null)
            throw new ArgumentNullException(nameof(conversionPath));

        if (string.IsNullOrWhiteSpace(conversionPath))
            throw new ArgumentException("Path cannot be empty or whitespace", nameof(conversionPath));

        if (!Directory.Exists(conversionPath))
            throw new DirectoryNotFoundException($"Directory not found: {conversionPath}");

        string[] projectFiles = Directory.GetFiles(conversionPath, "*.csproj", SearchOption.AllDirectories);
        Dictionary<string, List<PackageInfo>> packageVersions = new();

        // Collect all package references from all projects
        foreach (string projectFile in projectFiles)
        {
            IEnumerable<PackageInfo> packages = GetPackageReferences(projectFile);
            foreach (PackageInfo package in packages)
            {
                if (!packageVersions.ContainsKey(package.Name))
                {
                    packageVersions[package.Name] = new();
                }
                packageVersions[package.Name].Add(package);
            }
        }

        // Filter out constrained versions and determine highest versions
        Dictionary<string, string?> highestVersions = packageVersions
            .ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var nonConstrainedVersions = kvp.Value.Where(p => !p.HasVersionConstraint);
                    return nonConstrainedVersions.Any()
                        ? nonConstrainedVersions
                            .OrderByDescending(p => new Version(p.Version))
                            .First()
                            .Version
                        : null;
                });

        foreach (string projectFile in projectFiles)
        {
            UpdateProjectFile(projectFile, highestVersions);
        }

        // Create Directory.Packages.props with highest versions (excluding constrained versions)
        string packagesPropsPath = Path.Combine(conversionPath, "Directory.Packages.props");
        string packageRefs = string.Join(Environment.NewLine,
            highestVersions
                .Where(kvp => kvp.Value != null)
                .Select(kvp => $"    <PackageVersion Include=\"{kvp.Key}\" Version=\"{kvp.Value}\" />"));

        string content = $"""
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
{packageRefs}
  </ItemGroup>
</Project>
""";

        File.WriteAllText(packagesPropsPath, content);

        // Call CreateBuildPropsFile to create Directory.Build.props
        CreateBuildPropsFile(conversionPath);
    }

    /// <summary>
    /// Creates a Directory.Build.props file at the specified path with common build settings
    /// </summary>
    /// <param name="conversionPath">The full path where the Directory.Build.props file should be created</param>
    /// <exception cref="ArgumentNullException">Thrown when conversionPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when conversionPath is empty or whitespace</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist</exception>
    private static void CreateBuildPropsFile(string conversionPath)
    {
        if (conversionPath is null)
            throw new ArgumentNullException(nameof(conversionPath));

        if (string.IsNullOrWhiteSpace(conversionPath))
            throw new ArgumentException("Path cannot be empty or whitespace", nameof(conversionPath));

        if (!Directory.Exists(conversionPath))
            throw new DirectoryNotFoundException($"Directory not found: {conversionPath}");

        string buildPropsContent = """
<Project>
    <PropertyGroup>
        <LangVersion>latest</LangVersion>
        <Nullable>enable</Nullable>
        <WarningsAsErrors>Nullable</WarningsAsErrors>
        <ImplicitUsings>enable</ImplicitUsings>
        <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
    </ItemGroup>
</Project>
""";

        string buildPropsPath = Path.Combine(conversionPath, "Directory.Build.props");
        File.WriteAllText(buildPropsPath, buildPropsContent);
    }

    /// <summary>
    /// Creates a .gitignore file at the specified path with common .NET ignore patterns
    /// </summary>
    /// <param name="conversionPath">The full path where the .gitignore file should be created</param>
    /// <exception cref="ArgumentNullException">Thrown when conversionPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when conversionPath is empty or whitespace</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist</exception>
    public static void AddGitIgnore(string conversionPath)
    {
        if (conversionPath is null)
            throw new ArgumentNullException(nameof(conversionPath));

        if (string.IsNullOrWhiteSpace(conversionPath))
            throw new ArgumentException("Path cannot be empty or whitespace", nameof(conversionPath));

        if (!Directory.Exists(conversionPath))
            throw new DirectoryNotFoundException($"Directory not found: {conversionPath}");

        string gitIgnorePath = Path.Combine(conversionPath, ".gitignore");
        File.WriteAllText(gitIgnorePath, FileContentUtility.GitIgnoreContent);
    }

    /// <summary>
    /// Creates an .editorconfig file at the specified path with common C# and .NET coding style settings
    /// </summary>
    /// <param name="conversionPath">The full path where the .editorconfig file should be created</param>
    /// <exception cref="ArgumentNullException">Thrown when conversionPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when conversionPath is empty or whitespace</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist</exception>
    public static void CreateEditorConfig(string conversionPath)
    {
        if (conversionPath is null)
            throw new ArgumentNullException(nameof(conversionPath));

        if (string.IsNullOrWhiteSpace(conversionPath))
            throw new ArgumentException("Path cannot be empty or whitespace", nameof(conversionPath));

        if (!Directory.Exists(conversionPath))
            throw new DirectoryNotFoundException($"Directory not found: {conversionPath}");

        string editorConfigPath = Path.Combine(conversionPath, ".editorconfig");
        File.WriteAllText(editorConfigPath, FileContentUtility.EditorConfigContent);
    }

    private static IEnumerable<PackageInfo> GetPackageReferences(string projectPath)
    {
        XDocument doc = XDocument.Load(projectPath);
        IEnumerable<PackageInfo> packageRefs = doc.Descendants("PackageReference")
            .Where(x => x.Attribute("Include") != null && x.Attribute("Version") != null)
            .Select(x =>
            {
                string version = x.Attribute("Version")!.Value;
                bool hasConstraint = VersionConstraintPattern.IsMatch(version);
                return new PackageInfo(
                    x.Attribute("Include")!.Value,
                    version,
                    hasConstraint
                );
            });

        return packageRefs;
    }

    private static void UpdateProjectFile(string projectPath, Dictionary<string, string?> highestVersions)
    {
        XDocument doc = XDocument.Load(projectPath);
        bool modified = false;

        foreach (XElement packageRef in doc.Descendants("PackageReference").ToList())
        {
            XAttribute? includeAttr = packageRef.Attribute("Include");
            XAttribute? versionAttr = packageRef.Attribute("Version");

            if (includeAttr == null || versionAttr == null) continue;

            string packageName = includeAttr.Value;
            string packageVersion = versionAttr.Value;

            // Check if this package has a version constraint
            bool hasConstraint = VersionConstraintPattern.IsMatch(packageVersion);

            if (hasConstraint)
            {
                // Always change constrained versions to VersionOverride
                versionAttr.Remove();
                packageRef.Add(new XAttribute("VersionOverride", packageVersion));
                modified = true;
            }
            else if (highestVersions.TryGetValue(packageName, out string? highestVersion) && highestVersion != null)
            {
                if (packageVersion == highestVersion)
                {
                    // Remove Version attribute for packages at highest version
                    versionAttr.Remove();
                }
                else
                {
                    // Change Version to VersionOverride for packages not at highest version
                    versionAttr.Remove();
                    packageRef.Add(new XAttribute("VersionOverride", packageVersion));
                }
                modified = true;
            }
        }

        if (modified)
        {
            doc.Save(projectPath);
        }
    }
}