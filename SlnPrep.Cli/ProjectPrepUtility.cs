using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SlnPrep.Cli;

/// <summary>
/// Utility class for project preparation tasks
/// </summary>
public static class ProjectPrepUtility
{
    private record PackageInfo(string Name, string Version);

    /// <summary>
    /// Creates a Directory.Packages.props file at the specified path for central package management
    /// </summary>
    /// <param name="conversionPath">The full path where the Directory.Packages.props file should be created</param>
    /// <exception cref="ArgumentNullException">Thrown when conversionPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when conversionPath is empty or whitespace</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist</exception>
    public static void CreatePackagesPropsFile(string conversionPath)
    {
        if (conversionPath is null)
            throw new ArgumentNullException(nameof(conversionPath));
            
        if (string.IsNullOrWhiteSpace(conversionPath))
            throw new ArgumentException("Path cannot be empty or whitespace", nameof(conversionPath));
            
        if (!Directory.Exists(conversionPath))
            throw new DirectoryNotFoundException($"Directory not found: {conversionPath}");

        var projectFiles = Directory.GetFiles(conversionPath, "*.csproj", SearchOption.AllDirectories);
        var packageVersions = new Dictionary<string, List<PackageInfo>>();

        // Collect all package references from all projects
        foreach (var projectFile in projectFiles)
        {
            var packages = GetPackageReferences(projectFile);
            foreach (var package in packages)
            {
                if (!packageVersions.ContainsKey(package.Name))
                {
                    packageVersions[package.Name] = new List<PackageInfo>();
                }
                packageVersions[package.Name].Add(package);
            }
        }

        // Determine highest versions and update project files
        var highestVersions = packageVersions.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.OrderByDescending(p => new Version(p.Version)).First().Version
        );

        foreach (var projectFile in projectFiles)
        {
            UpdateProjectFile(projectFile, highestVersions);
        }

        // Create Directory.Packages.props with highest versions
        var packagesPropsPath = Path.Combine(conversionPath, "Directory.Packages.props");
        var packageRefs = string.Join(Environment.NewLine, 
            highestVersions.Select(kvp => $"    <PackageVersion Include=\"{kvp.Key}\" Version=\"{kvp.Value}\" />"));
        
        var content = $"""
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
    }

    /// <summary>
    /// Creates a Directory.Build.props file at the specified path with common build settings
    /// </summary>
    /// <param name="conversionPath">The full path where the Directory.Build.props file should be created</param>
    /// <exception cref="ArgumentNullException">Thrown when conversionPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when conversionPath is empty or whitespace</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist</exception>
    public static void CreateBuildPropsFile(string conversionPath)
    {
        if (conversionPath is null)
            throw new ArgumentNullException(nameof(conversionPath));
            
        if (string.IsNullOrWhiteSpace(conversionPath))
            throw new ArgumentException("Path cannot be empty or whitespace", nameof(conversionPath));
            
        if (!Directory.Exists(conversionPath))
            throw new DirectoryNotFoundException($"Directory not found: {conversionPath}");

        var buildPropsContent = """
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

        var buildPropsPath = Path.Combine(conversionPath, "Directory.Build.props");
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

        var gitIgnoreContent = """
## Visual Studio
.vs/
[Bb]in/
[Oo]bj/
[Dd]ebug/
[Rr]elease/
*.user
*.userosscache
*.suo
*.userprefs
*.dbmdl
*.jfm
*.pfx
*.publishsettings

## Visual Studio Code
.vscode/*
!.vscode/settings.json
!.vscode/tasks.json
!.vscode/launch.json
!.vscode/extensions.json
*.code-workspace

## Rider
.idea/
*.sln.iml
.idea/**/workspace.xml
.idea/**/tasks.xml
.idea/**/usage.statistics.xml
.idea/**/dictionaries
.idea/**/shelf

## .NET Core
project.lock.json
project.fragment.lock.json
artifacts/

## NuGet
*.nupkg
**/packages/*
!**/packages/build/
*.nuget.props
*.nuget.targets

## Build results
[Dd]ist/
[Ll]og/
[Ll]ogs/
msbuild.log
msbuild.err
msbuild.wrn

## Other
*.swp
*.*~
*.bak
""";

        var gitIgnorePath = Path.Combine(conversionPath, ".gitignore");
        File.WriteAllText(gitIgnorePath, gitIgnoreContent);
    }

    private static IEnumerable<PackageInfo> GetPackageReferences(string projectPath)
    {
        var doc = XDocument.Load(projectPath);
        var packageRefs = doc.Descendants("PackageReference")
            .Where(x => x.Attribute("Include") != null && x.Attribute("Version") != null)
            .Select(x => new PackageInfo(
                x.Attribute("Include")!.Value,
                x.Attribute("Version")!.Value
            ));

        return packageRefs;
    }

    private static void UpdateProjectFile(string projectPath, Dictionary<string, string> highestVersions)
    {
        var doc = XDocument.Load(projectPath);
        var modified = false;

        foreach (var packageRef in doc.Descendants("PackageReference").ToList())
        {
            var includeAttr = packageRef.Attribute("Include");
            var versionAttr = packageRef.Attribute("Version");

            if (includeAttr == null || versionAttr == null) continue;

            var packageName = includeAttr.Value;
            var packageVersion = versionAttr.Value;

            if (highestVersions.TryGetValue(packageName, out var highestVersion))
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