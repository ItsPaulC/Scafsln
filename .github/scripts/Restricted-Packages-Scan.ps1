param(
    [string]$initialDirectory = (Get-Location).Path
)

# Import the utility functions
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$utilityScriptPath = Join-Path -Path $scriptPath -ChildPath "Utilities.ps1"

# Check if the utility script exists and source it
if (-not (Test-Path -Path $utilityScriptPath)) {
    Write-Error "Required utility script not found: $utilityScriptPath"
    exit 1
}

# Dot source the utility script to import the functions
. $utilityScriptPath

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


#region Main Script

# Ensure the start directory exists
if (-not (Test-Path -Path $initialDirectory)) {
    Write-Error "The specified start directory '$initialDirectory' does not exist."
    exit 1
}

# Find .slsn or .slnx file searching upwards using the imported function
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
$csprojFiles = Get-ChildItem -Path $initialDirectory -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
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

    # Display packages in table format using the function from Utilities.ps1
    Format-TableOutput -Data $foundPackages -Title "Found Package References" -TitleColor Cyan

    # Display restricted packages
    Format-TableOutput -Data $restrictedPackages -Title "Searching the code for the following restricted packages" -TitleColor Cyan

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


