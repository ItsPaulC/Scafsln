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

#region Main Script

    ##########
    # Capture just the string return value
    $slnPath = (New-Solution-FromProjectPath -startDir $initialDirectory).Trim()
    #########################

    # List deprecated packages in the solution
    Write-Host "Scanning packages for deprecations..." -ForegroundColor Cyan
    $foundDeprecatedPackages = [System.Collections.Generic.List[PSObject]]::new()

    # Run 'dotnet list package --deprecated' command to check for deprecated packages
    $packageListResult = dotnet list $slnPath package --deprecated

    foreach ($line in $packageListResult) {
        # Check if line starts with '>' (after any whitespace)
        if ($line -match '^\s*>\s+(.+?)\s+\S+\s+(\S+)\s+(.+?)(?:\s+(.+))?$') {
            $packageName = $matches[1].Trim()
            $version = $matches[2].Trim()
            $reason = $matches[3].Trim()
            $alternative = if ($matches[4]) { $matches[4].Trim() } else { $null }

            $foundDeprecatedPackages.Add([PSCustomObject]@{
                PackageName = $packageName
                Version = $version
                Reason = $reason
                Alternative = $alternative
            })
        }
    }

    # Display deprecated packages if any were found
    if ($foundDeprecatedPackages.Count -gt 0) {
        Format-TableOutput -Data $foundDeprecatedPackages -Title "Deprecated Packages Found!" -TitleColor Red -RowColor Yellow
        
        # Fail the build because deprecated packages were found
        exit 1
    }
    else {
        Write-Host "No deprecated packages found." -ForegroundColor Green
    }

#endregion Main Script
