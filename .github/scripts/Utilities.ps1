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

# Export the function to make it available when the script is dot-sourced
Export-ModuleMember -Function Find-FileUpwards