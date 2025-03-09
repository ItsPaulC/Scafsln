# SlnPrep

A .NET CLI tool for initializing and standardizing .NET solution configurations. SlnPrep helps you quickly set up common configuration files and enforce consistent coding standards across your .NET projects.

## Features

- **Central Package Management (CPM)**: Automatically consolidates and manages NuGet package versions across your solution
- **Build Properties**: Sets up common build settings and C# language features
- **Editor Configuration**: Enforces consistent coding styles through `.editorconfig`
- **Git Integration**: Provides standard `.gitignore` for .NET projects

## Installation

Currently, the tool can be run directly from the source:

```bash
dotnet run --project SlnPrep.Cli/SlnPrep.Cli.csproj
```

## Usage

```bash
slnprep init-sln [options] <path>
```

### Options

- `-c|--cpm`: Convert to Nuget CPM in the solution
- `-b|--buildprops`: Add Directory.Build.props with common build settings
- `-g|--gitignore`: Add .gitignore file with .NET-specific patterns
- `-e|--editorconfig`: Add .editorconfig with C# coding style settings
- `-a|--all`: Add all configuration files to the solution
- `-h|--help`: Show help and usage information

### Example Commands

Add all configuration files:
```bash
slnprep init-sln -a "C:\path\to\solution"
```

Add only central package management:
```bash
slnprep init-sln -c "C:\path\to\solution"
```

Add editor config and gitignore:
```bash
slnprep init-sln -e -g "C:\path\to\solution"
```

## Generated Files

### Directory.Packages.props
- Implements [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)
- Consolidates package versions across projects
- Automatically detects and uses highest package versions

### Directory.Build.props
- Enables latest C# language features
- Configures nullable reference types
- Sets up warning as errors for nullable violations
- Includes .NET analyzers

### .editorconfig
- Enforces consistent coding styles
- Configures C# code formatting rules
- Sets up naming conventions
- Configures file formatting for various file types

### .gitignore
- Excludes common .NET build outputs
- Ignores IDE-specific files
- Handles NuGet package directories
- Excludes sensitive files (*.user, *.pfx)

## Requirements

- .NET 8.0 SDK or later
- Windows, macOS, or Linux operating system