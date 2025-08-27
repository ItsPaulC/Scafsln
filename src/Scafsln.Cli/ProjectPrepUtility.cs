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


    ///
    private static string NetAnalyzersVersion = "9.0.0";


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

        // Add Microsoft.CodeAnalysis.NetAnalyzers explicitly with a fixed version
        packageVersions.Add("Microsoft.CodeAnalysis.NetAnalyzers", [new PackageInfo("Microsoft.CodeAnalysis.NetAnalyzers", NetAnalyzersVersion, false)]);

        // Filter out constrained versions and determine the highest versions
        Dictionary<string, string?> highestVersions = packageVersions
            .ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var nonConstrainedVersions = kvp.Value.Where(p => !p.HasVersionConstraint);
                    return nonConstrainedVersions.Any()
                        ? nonConstrainedVersions
                            .OrderByDescending(p => p.Version, new NuGetVersionComparer())
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
        
        // Create Directory.Build.props file
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

    /// <summary>
    /// Creates a copilot-instructions.md file at the specified path with GitHub Copilot instructions for .NET development
    /// </summary>
    /// <param name="conversionPath">The full path where the copilot-instructions.md file should be created</param>
    /// <exception cref="ArgumentNullException">Thrown when conversionPath is null</exception>
    /// <exception cref="ArgumentException">Thrown when conversionPath is empty or whitespace</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist</exception>
    public static void CreateCopilotInstructions(string conversionPath)
    {
        if (conversionPath is null)
            throw new ArgumentNullException(nameof(conversionPath));

        if (string.IsNullOrWhiteSpace(conversionPath))
            throw new ArgumentException("Path cannot be empty or whitespace", nameof(conversionPath));

        if (!Directory.Exists(conversionPath))
            throw new DirectoryNotFoundException($"Directory not found: {conversionPath}");

        string copilotInstructionsPath = Path.Combine(conversionPath, ".github", "copilot-instructions.md");
        
        // Create .github directory if it doesn't exist
        string githubDir = Path.GetDirectoryName(copilotInstructionsPath)!;
        if (!Directory.Exists(githubDir))
        {
            Directory.CreateDirectory(githubDir);
        }
        
        File.WriteAllText(copilotInstructionsPath, FileContentUtility.CopilotInstructionsContent);
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

    /// <summary>
    /// Custom comparer for NuGet package versions that handles non-standard version formats
    /// </summary>
    private class NuGetVersionComparer : IComparer<string>
    {
        private static readonly Regex SemVerRegex = new(@"^(\d+)(?:\.(\d+))?(?:\.(\d+))?(?:\.(\d+))?(?:-([0-9A-Za-z-.]+))?(?:\+([0-9A-Za-z-.]+))?$");

        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Clean up version strings (remove any build metadata)
            string vx = x.Contains('+') ? x.Substring(0, x.IndexOf('+')) : x;
            string vy = y.Contains('+') ? y.Substring(0, y.IndexOf('+')) : y;

            // Try parsing versions as standard versions
            bool xIsStandard = TryParseStandardVersion(vx, out Version? versionX);
            bool yIsStandard = TryParseStandardVersion(vy, out Version? versionY);

            // If both are standard versions, compare them
            if (xIsStandard && yIsStandard && versionX != null && versionY != null)
            {
                return versionX.CompareTo(versionY);
            }

            // If one is standard and one isn't, standard is higher
            if (xIsStandard != yIsStandard)
            {
                return xIsStandard ? 1 : -1;
            }

            // If neither are standard, try semantic versioning approach
            Match matchX = SemVerRegex.Match(vx);
            Match matchY = SemVerRegex.Match(vy);

            if (matchX.Success && matchY.Success)
            {
                // Compare major, minor, patch, revision
                for (int i = 1; i <= 4; i++)
                {
                    int numX = int.TryParse(matchX.Groups[i].Value, out int x1) ? x1 : 0;
                    int numY = int.TryParse(matchY.Groups[i].Value, out int y1) ? y1 : 0;
                    
                    if (numX != numY)
                    {
                        return numX.CompareTo(numY);
                    }
                }

                // Compare pre-release labels (no pre-release > has pre-release)
                bool hasPreReleaseX = matchX.Groups[5].Success;
                bool hasPreReleaseY = matchY.Groups[5].Success;

                if (hasPreReleaseX != hasPreReleaseY)
                {
                    return hasPreReleaseX ? -1 : 1;
                }

                if (hasPreReleaseX && hasPreReleaseY)
                {
                    return string.Compare(matchX.Groups[5].Value, matchY.Groups[5].Value, StringComparison.OrdinalIgnoreCase);
                }
            }

            // Last resort: string comparison
            return string.Compare(vx, vy, StringComparison.OrdinalIgnoreCase);
        }

        private bool TryParseStandardVersion(string versionString, out Version? version)
        {
            // Remove any pre-release suffix for standard version parsing
            if (versionString.Contains('-'))
            {
                versionString = versionString.Substring(0, versionString.IndexOf('-'));
            }

            try
            {
                version = new Version(versionString);
                return true;
            }
            catch
            {
                version = null;
                return false;
            }
        }
    }
}