#requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

. .\install-lib.ps1

$lib = [InstallLib]::new(".\NiekoBoard.Server\appsettings.json")

$lib.InstallGPrc()
