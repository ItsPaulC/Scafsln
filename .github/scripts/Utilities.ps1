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
    Creates an ASCII table from the provided data.
.DESCRIPTION
    Formats a collection of objects into a readable ASCII table with columns and borders.
.PARAMETER Data
    The collection of objects to format as a table.
.PARAMETER Title
    The title to display above the table.
.PARAMETER TitleColor
    The console color to use for the title. Default is White.
.PARAMETER RowColor
    The console color to use for the data rows. Default is White.
.EXAMPLE
    Format-TableOutput -Data $packages -Title "Package References" -TitleColor Cyan -RowColor Yellow
#>
function Format-TableOutput {
    param(
        [Parameter(Mandatory=$true)]
        [PSObject[]]$Data,
        
        [Parameter(Mandatory=$true)]
        [string]$Title,
        
        [Parameter(Mandatory=$false)]
        [System.ConsoleColor]$TitleColor = [System.ConsoleColor]::White,
        
        [Parameter(Mandatory=$false)]
        [System.ConsoleColor]$RowColor = [System.ConsoleColor]::White
    )
    
    if ($Data.Count -eq 0) {
        return
    }
    
    Write-Host ("`n" + $Title + ":") -ForegroundColor $TitleColor
    
    # Get all property names
    $properties = $Data[0].PSObject.Properties.Name
    
    # Calculate column widths
    $columnWidths = @{}
    foreach ($prop in $properties) {
        $headerLength = $prop.Length
        $maxDataLength = ($Data | ForEach-Object { 
            if ($null -eq $_.$prop) {
                0  # Handle null values
            } else {
                $_.$prop.ToString().Length
            }
        } | Measure-Object -Maximum).Maximum
        $columnWidths[$prop] = [Math]::Max($headerLength, $maxDataLength) + 2
    }
    
    # Create header border
    $headerBorder = "+"
    foreach ($prop in $properties) {
        $headerBorder += ("-" * $columnWidths[$prop]) + "+"
    }
    
    # Create header row
    $headerRow = "|"
    foreach ($prop in $properties) {
        $headerRow += " " + $prop.PadRight($columnWidths[$prop] - 2) + " |"
    }
    
    # Output header
    Write-Host $headerBorder
    Write-Host $headerRow
    Write-Host $headerBorder
    
    # Output rows
    foreach ($item in $Data) {
        $row = "|"
        foreach ($prop in $properties) {
            $value = if ($null -eq $item.$prop) { "∞" } else { $item.$prop.ToString() }
            $row += " " + $value.PadRight($columnWidths[$prop] - 2) + " |"
        }
        Write-Host $row -ForegroundColor $RowColor
    }
    
    # Output footer
    Write-Host $headerBorder
}

function New-Solution-FromProjectPath
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$startDir
    )

    # If $global:ScanSolutionPath is not null, then a solution has already been created. Return the value
    if ($global:ScanSolutionPath)
    {
        Write-Host "Solution already created at $global:ScanSolutionPath"
        return $global:ScanSolutionPath
    }


    # Ensure the start directory exists
    if (-not (Test-Path -Path $initialDirectory))
    {
        Write-Error "The specified start directory '$initialDirectory' does not exist."
        exit 1
    }

    # Find .slsn or .slnx file searching upwards using the imported function
    $solutionFilePatterns = @("*.sln", "*.slnx")
    $existingSolutionFile = Find-FileUpwards -startDir $initialDirectory -filePatterns $solutionFilePatterns

    if ($existingSolutionFile)
    {
        Write-Host "Found existing solution file: $existingSolutionFile"
    }
    else
    {
        Write-Host "No existing solution file found."
    }

    # Create new solution with GUID name
    $newSolutionName = [guid]::NewGuid().ToString() + ".sln"
    $newSolutionPath = Join-Path -Path $initialDirectory -ChildPath $newSolutionName

    Write-Host "Creating new solution: $newSolutionPath"
    dotnet new sln --name $([System.IO.Path]::GetFileNameWithoutExtension($newSolutionName) ) --output $initialDirectory

    # Verify the new solution was created
    if (-not (Test-Path -Path $newSolutionPath))
    {
        Write-Error "Failed to create solution file at $newSolutionPath"
        exit 1
    }

    # Find all .csproj files in subdirectories
    $csprojFiles = Get-ChildItem -Path $initialDirectory -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
    $csprojFilePaths = $csprojFiles.FullName

    if ($csprojFiles.Count -eq 0)
    {
        Write-Warning "No .csproj files found in $initialDirectory or its subdirectories."
    }
    else
    {
        Write-Host "Found $( $csprojFiles.Count ) .csproj files."

        # Add projects to the solution
        Write-Host "Adding projects to solution..."
        foreach ($csprojFile in $csprojFilePaths)
        {
            Write-Host "  Adding: $csprojFile"
            dotnet sln $newSolutionPath add $csprojFile | Out-Null
        }

        # Restore the solution
        Write-Host "Restoring solution packages..."
        dotnet restore $newSolutionPath

        # Create a global variable that can be accessed by other scripts
        $global:ScanSolutionPath = $newSolutionPath

        # Write the new solution path to the Environment Variable for the other steps to use
#        Write-Host "##vso[task.setvariable variable=ScanSolutionPath]$newSolutionPath"

    }
    
    return $global:ScanSolutionPath
}

# When the script is dot-sourced, all functions are automatically available to the calling script