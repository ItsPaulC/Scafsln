# FileUtilities.ps1
# Provides utility functions for file operations

<#
.SYNOPSIS
    Finds a file type by looking upwards towards the root.
.DESCRIPTION
    Finds file paths by searching upwards, for specified file patterns, from the specified directory towards the root.
.PARAMETER startDir
    The directory to start the search from.
.PARAMETER filePatterns
    An array of file patterns (wildcards) to search for.
.EXAMPLE
    Find-FileUpwards -startDir "C:\Projects\MyProject\src" -filePatterns @("*.sln", "*.slnx")
.OUTPUTS
    System.String. The full path of the first matching file, or $null if no match is found.
#>
function Find-FileUpwards {
    param(
        [Parameter(Mandatory=$true)]
        [string]$startDir,
        
        [Parameter(Mandatory=$true)]
        [string[]]$filePatterns
    )

    $currentDir = $startDir
    while ($null -ne $currentDir) {
        foreach ($pattern in $filePatterns) {
            # Check the current directory itself for matching files first
            $foundFiles = Get-ChildItem -Path $currentDir -Filter $pattern -File -ErrorAction SilentlyContinue
            if ($foundFiles) {
                return $foundFiles[0].FullName
            }
        }
        
        # Move up one directory
        $parentDir = Split-Path -Path $currentDir -Parent
        if ($parentDir -eq $currentDir) {
            # We've reached the root
            return $null
        }
        $currentDir = $parentDir
    }
    
    return $null
}

<#
.SYNOPSIS
    Creates a new solution file and adds projects to it.
.DESCRIPTION
    Creates a new solution file at the specified location or uses an existing one.
    Finds all .csproj files in the directory and adds them to the solution.
.PARAMETER startDir
    The directory to start from.
.OUTPUTS
    System.String. The full path of the solution file.
#>
function New-Solution-FromProjectPath {
    param(
        [Parameter(Mandatory=$true)]
        [string]$startDir
    )

    # Ensure the start directory exists
    if (-not (Test-Path -Path $startDir)) {
        Write-Error "The specified start directory '$startDir' does not exist."
        exit 1
    }

    # Find .sln or .slnx file searching upwards using the imported function
    $solutionFilePatterns = @("*.sln", "*.slnx")
    $existingSolutionFile = Find-FileUpwards -startDir $startDir -filePatterns $solutionFilePatterns

    if ($existingSolutionFile) {
        Write-Host "Found existing solution file: $existingSolutionFile"
    }
    else {
        Write-Host "No existing solution file found."
    }

    # Create new solution with GUID name
    $newSolutionName = [guid]::NewGuid().ToString() + ".sln"
    
    # Use the directory from $existingSolutionFile if available, otherwise use $startDir
    $solutionDirectory = if ($existingSolutionFile) {
        Split-Path -Path $existingSolutionFile -Parent
    } else {
        $startDir
    }
    
    $newSolutionPath = Join-Path -Path $solutionDirectory -ChildPath $newSolutionName

    Write-Host "Creating new solution: $newSolutionPath"
    # Capture and suppress the output of dotnet new command
    $null = dotnet new sln --name $([System.IO.Path]::GetFileNameWithoutExtension($newSolutionName)) --output $solutionDirectory

    # Verify the new solution was created
    if (-not (Test-Path -Path $newSolutionPath)) {
        Write-Error "Failed to create solution file at $newSolutionPath"
        exit 1
    }

    # Find all .csproj files in subdirectories
    $csprojFiles = Get-ChildItem -Path $startDir -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
    $csprojFilePaths = $csprojFiles.FullName

    if ($csprojFiles.Count -eq 0) {
        Write-Warning "No .csproj files found in $startDir or its subdirectories."
    }
    else {
        Write-Host "Found $($csprojFiles.Count) .csproj files."

        # Add projects to the solution
        Write-Host "Adding projects to solution..."
        foreach ($csprojFile in $csprojFilePaths) {
            Write-Host "  Adding: $csprojFile"
            $null = dotnet sln $newSolutionPath add $csprojFile
        }

        # Restore the solution
        Write-Host "Restoring solution packages..."
        $null = dotnet restore $newSolutionPath

        # Create a global variable that can be accessed by other scripts
        $global:ScanSolutionPath = $newSolutionPath
    }
    
    # Explicitly return just the path string
    return $newSolutionPath
}

function Pre-Scan-Setup {
    # Clear the global variable, just in case
    $global:ScanSolutionPath = $null
    
}

function Post-Scan-Teardown {
    if ($global:ScanSolutionPath -and (Test-Path -Path $global:ScanSolutionPath)) {
        Write-Host "Cleaning up solution file..."
        Remove-Item -Path $global:ScanSolutionPath -Force
    }
    
    # Clear the global variable
    $global:ScanSolutionPath = $null
}
# When the script is dot-sourced, all functions are automatically available to the calling script