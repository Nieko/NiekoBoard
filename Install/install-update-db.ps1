#requires -RunAsAdministrator

. .\db-lib.ps1

$lib = [DbLib]::new()

$lib.InstallAndUpdate()
$lib.Messages | Out-String