param(
    [string]$initialDirectory = (Get-Location).Path
)

# Import the utility functions
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$generalUtilityScriptPath = Join-Path -Path $scriptPath -ChildPath "General-Utilities.ps1"
$scanUtilityScriptPath = Join-Path -Path $scriptPath -ChildPath "Scan-Utilities.ps1"

# Check if the utility script exists and source it
if (-not (Test-Path -Path $generalUtilityScriptPath)) {
    Write-Error "Required General-Utilities script not found: $generalUtilityScriptPath"
    exit 1
}

if (-not (Test-Path -Path $scanUtilityScriptPath)) {
    Write-Error "Required Scan-Utilities script not found: $scanUtilityScriptPath"
    exit 1
}


# Dot source the utility scripts to import the functions
. $generalUtilityScriptPath
. $scanUtilityScriptPath


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

##########
# Capture just the string return value
$slnPath = (New-Solution-FromProjectPath -startDir $initialDirectory).Trim()

# If you receive an array, you can select the last element which is likely the actual path
# Uncomment the following line if the above approach doesn't work
# $slnPath = @(New-Solution-FromProjectPath -startDir $initialDirectory)[-1].Trim()
#########################

# List packages in the solution and save unique results with "Version"
Write-Host "Listing packages in the solution..." -ForegroundColor Cyan
$packages = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$foundPackages = [System.Collections.Generic.List[PSObject]]::new()

$packageListResult = dotnet list $slnPath package

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

    # Fail the build becasue Restricted Packages were found
    exit 1
}
else {
    Write-Host "No restricted packages found."
}

#endregion Main Script