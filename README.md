# Scafsln

A .NET CLI tool for initializing and standardizing .NET solution configurations. Scafsln helps you quickly set up common configuration files and enforce consistent coding standards across your .NET projects.

## Features

- **Central Package Management (CPM)**: Automatically consolidates and manages NuGet package versions across your solution
- **Build Properties**: Sets up common build settings and C# language features (automatically included with CPM)
- **Editor Configuration**: Enforces consistent coding styles through `.editorconfig`
- **Git Integration**: Provides standard `.gitignore` for .NET projects
- **GitHub Copilot Integration**: Creates standardized copilot-instructions.md for consistent AI assistance
- **Configuration Management**: View and update configuration files with template support
- **Template Storage**: Persistent storage of templates using SQLite for improved reliability
- **SemVer Support**: Robust NuGet package version comparison with support for non-standard version formats
- **Directory Context**: Commands automatically use the current directory when no path is specified

## Installation

Currently, the tool can be run directly from the source:

```powershell
dotnet run --project Scafsln.Cli/Scafsln.Cli.csproj
```

## Usage

### Initialize Solution

```powershell
scafsln init-sln [options] [path]
```

The `path` argument is now optional. If not provided, the command will use the current directory.

#### Init Options

- `-c|--cpm`: Convert to Nuget CPM in the solution (automatically includes Directory.Build.props)
- `-g|--gitignore`: Add .gitignore file with .NET-specific patterns
- `-e|--editorconfig`: Add .editorconfig with C# coding style settings
- `-i|--ci`: Add copilot-instructions.md for GitHub Copilot guidance
- `-a|--all`: Add all configuration files to the solution
- `-h|--help`: Show help and usage information

### Manage Configuration Files

```powershell
scafsln config [options] [path]
```

Path is only required when using the change commands.

#### Config Options

- `--show-gitignore`: Display the contents of the current .gitignore template
- `--show-editorconfig`: Display the contents of the current .editorconfig template
- `--show-copilot-instructions`: Display the contents of the current copilot-instructions.md template
- `--change-editorconfig <file>`: Update the .editorconfig template from a new source
- `--change-gitignore <file>`: Update the .gitignore template from a new source
- `--change-copilot-instructions <file>`: Update the copilot-instructions.md template from a new source
- `--reset`: Reset templates to their default values
- `-h|--help`: Show help and usage information

When using the `--change-editorconfig`, `--change-gitignore`, or `--change-copilot-instructions` options, the provided file will be:
- Saved as a template in the SQLite database for persistent storage
- Used as the default content for future operations

### Example Commands

Initialize with all configuration files in the current directory:
```powershell
scafsln init-sln -a
```

Initialize with all configuration files in a specific directory:
```powershell
scafsln init-sln -a "C:\path\to\solution"
```

Add only central package management to current directory:
```powershell
scafsln init-sln -c
```

Add editor config and gitignore:
```powershell
scafsln init-sln -e -g "C:\path\to\solution"
```

Add GitHub Copilot instructions:
```powershell
scafsln init-sln -i
```

Add all configuration files including copilot instructions:
```powershell
scafsln init-sln --all
```

View .gitignore contents:
```powershell
scafsln config --show-gitignore
```

Update .editorconfig template:
```powershell
scafsln config --change-editorconfig "path\to\template.editorconfig"
```

View copilot instructions contents:
```powershell
scafsln config --show-copilot-instructions
```

Update copilot instructions template:
```powershell
scafsln config --change-copilot-instructions "path\to\custom-copilot-instructions.md"
```

Reset templates to default:
```powershell
scafsln config --reset
```

## Generated Files

### Directory.Packages.props
- Implements [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)
- Consolidates package versions across projects
- Automatically detects and uses highest package versions
- Handles complex NuGet version formats and SemVer specifications

### Directory.Build.props
- Automatically created when using the CPM option
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

### .github/copilot-instructions.md
- Provides standardized GitHub Copilot instructions for .NET 8 development
- Enforces modern C# 12 coding practices and idioms
- Includes guidance for performance, security, and testing
- Configures structured logging and telemetry standards
- Ensures consistent AI-assisted code generation across the team

## Technical Implementation

- Built with .NET 8 for cross-platform compatibility
- SQLite database for persistent template storage
- Entity Framework Core for database operations
- Asynchronous API design for improved performance
- Robust error handling throughout

## Requirements

- .NET 8.0 SDK or later
- Windows, macOS, or Linux operating system