<#
.SYNOPSIS
    .
.DESCRIPTION
    .
.PARAMETER LogDir
    The LogDir to the .
.PARAMETER

.EXAMPLE
    C:\PS>
    <Description of example>
.NOTES
    Author: Daniel shih
#>
param(
	[Parameter(Mandatory=$true,HelpMessage="log directory")]
	[string]$LogDir,
	[Parameter(HelpMessage="output csv")]
	[string]$Output_csv
)

$logFiles = Get-ChildItem -Path $LogDir -Filter *.log | ForEach-Object { $_.FullName }

# Write-Host $logFiles

# Build the python command
if ($Output_csv) {
    # If Output_csv is provided, include it in the command
    python parse_mysql_slow_query.py $logFiles --output_csv $Output_csv
} else {
    # If Output_csv is not provided, run the command without it
    python parse_mysql_slow_query.py $logFiles
}
