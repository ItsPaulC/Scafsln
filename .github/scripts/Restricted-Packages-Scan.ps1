param(
    [string]$initialDirectory = (Get-Location).Path
)

# .\Find-ProjectsAndCreateSolution.ps1 -startDirectory "E:\repos\Samples\TestPowershell\TestPowershell\"

####################################################################################################
# Add packages to restrict to the $restrictedPackages array below
# The script will check the version of each package found in the solution against the restrictions

# To restrict a version you can do a min/max range, or just a min or max by setting the other to $null
# To restrict a version to a single version, set both min and max to the same version

# Examples: 
# MinVersion       - MaxVersion       - Description
# [version]"7.2.0" - [version]"7.2.0" - 7.2.0 only
# [version]"7.2.0" - [version]"7.3.0" - 7.2.0 to 7.3.0
# $null            - [version]"7.3.0" - 7.3.0 or greater
# [version]"7.2.0" - $null            - 7.2.0 or less


$restrictedPackages = @(
    [PSCustomObject]@{
        PackageName = "FluentAssertions"
        MinVersion = $null
        MaxVersion = [version]"7.1.0"
        Reason = "FluentAssertions 8, and above is a pay product."
    },
    [PSCustomObject]@{
        PackageName = "Moq"
        MinVersion = [version]"4.20.0"
        MaxVersion = [version]"4.20.0"
        Reason = "Moq phones home (SponsorLink) with version 4.20.0."
    }
)

#region Functions

<#
.SYNOPSIS
    Finds a file type by looking upwards towards the root.
.DESCRIPTION
    Finds file paths by searching upwards, for specified file patterns, from the specified directory towards the root.
#>
function Find-FileUpwards {
    param(
        [string]$startDir,
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

function Find-CsprojFiles {
    param(
        [string]$startDir
    )
    
    return Get-ChildItem -Path $startDir -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
}

<#
.SYNOPSIS
    Creates an ASCII table from the provided data.
.DESCRIPTION
    Formats a collection of objects into a readable ASCII table with columns and borders.
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

#endregion Functions

#region Main Script

# Ensure the start directory exists
if (-not (Test-Path -Path $initialDirectory)) {
    Write-Error "The specified start directory '$initialDirectory' does not exist."
    exit 1
}

# Find .slsn or .slnx file searching upwards
$solutionFilePatterns = @("*.sln", "*.slnx")
$existingSolutionFile = Find-FileUpwards -startDir $initialDirectory -filePatterns $solutionFilePatterns

if ($existingSolutionFile) {
    Write-Host "Found existing solution file: $existingSolutionFile"
}
else {
    Write-Host "No existing solution file found."
}

# Create new solution with GUID name
$newSolutionName = [guid]::NewGuid().ToString() + ".sln"
$newSolutionPath = Join-Path -Path $initialDirectory -ChildPath $newSolutionName

Write-Host "Creating new solution: $newSolutionPath"
dotnet new sln --name $([System.IO.Path]::GetFileNameWithoutExtension($newSolutionName)) --output $initialDirectory

# Verify the new solution was created
if (-not (Test-Path -Path $newSolutionPath)) {
    Write-Error "Failed to create solution file at $newSolutionPath"
    exit 1
}

# Find all .csproj files in subdirectories
$csprojFiles = Find-CsprojFiles -startDir $initialDirectory
$csprojFilePaths = $csprojFiles.FullName

if ($csprojFiles.Count -eq 0) {
    Write-Warning "No .csproj files found in $initialDirectory or its subdirectories."
}
else {
    Write-Host "Found $($csprojFiles.Count) .csproj files."
    
    # Add projects to the solution
    Write-Host "Adding projects to solution..."
    foreach ($csprojFile in $csprojFilePaths) {
        Write-Host "  Adding: $csprojFile"
        dotnet sln $newSolutionPath add $csprojFile | Out-Null
    }
    
    # Restore the solution
    Write-Host "Restoring solution packages..."
    dotnet restore $newSolutionPath
    
    # List packages in the solution and save unique results with "Version"
    Write-Host "Listing packages in the solution..." -ForegroundColor Cyan
    $packages = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $foundPackages = [System.Collections.Generic.List[PSObject]]::new()

    $packageListResult = dotnet list $newSolutionPath package

    foreach ($line in $packageListResult) {
        # Check if line starts with '>' (after any whitespace)
        if ($line -match '^\s*>\s*(.+?)\s+(\S+)\s+(\S+)\s*$') {
            $packageName = $matches[1].Trim()
            # The last version is the one we want (target version)
            $version = $matches[3].Trim()
            
            $foundPackages.Add([PSCustomObject]@{
                PackageName = $packageName
                Version = $version
            })
            
            [void]$packages.Add("$packageName $version")
        }
        elseif ($line -match "version" -or $line -match "Version") {
            [void]$packages.Add($line.Trim())
        }
    }

    # Display packages in table format using the function
    Format-TableOutput -Data $foundPackages -Title "Found Package References" -TitleColor Cyan

    # Display resticted packages
    Format-TableOutput -Data $restrictedPackages -Title "Searching the code for the following restriced packates" -TitleColor Cyan

    # Find Restricted Packages in the solution

    $foundRestrictedPackages = [System.Collections.Generic.List[PSObject]]::new()

    foreach ($restrictedPackage in $restrictedPackages) {
        $foundPackage = $foundPackages | Where-Object { $_.PackageName -eq $restrictedPackage.PackageName }
        if ($foundPackage) {
            if ($restrictedPackage.MinVersion -and $foundPackage.Version -lt $restrictedPackage.MinVersion) {

                $foundRestrictedPackages.Add([PSCustomObject]@{
                    PackageName = $foundPackage.PackageName
                    Version = $foundPackage.Version
                    Restriction = "Package '$($foundPackage.PackageName)' version $($foundPackage.Version) is less than the minimum required version $($restrictedPackage.MinVersion)."
                 })
            }
            if ($restrictedPackage.MaxVersion -and $foundPackage.Version -gt $restrictedPackage.MaxVersion) {

                $foundRestrictedPackages.Add([PSCustomObject]@{
                    PackageName = $foundPackage.PackageName
                    Version = $foundPackage.Version
                    Restriction = "Package '$($foundPackage.PackageName)' version $($foundPackage.Version) is greater than the maximum allowed version $($restrictedPackage.MaxVersion)."
                    })            
            }
        }
    }

    # Display restricted packages if any were found
    if ($foundRestrictedPackages.Count -gt 0) {
        Format-TableOutput -Data $foundRestrictedPackages -Title "Restricted Packages Found!" -TitleColor Red -RowColor Yellow
    }
    else {
        Write-Host "No restricted packages found."
    }
}

# Delete the temp sln file
Remove-Item $newSolutionPath

#endregion Main Script


