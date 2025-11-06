#requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

$script:config = (Get-Content .\install-settings.json | ConvertFrom-Json)


if(-not (get-module sqlserver -all -erroraction silentlycontinue)) {
    install-module sqlserver -AllowClobber
}

import-module sqlserver

class DbLib {

    $server = ""
    $database = ""
    $apppool = ""
    [System.Collections.Generic.List[string]]$Messages

    DbLib() {
        $this.server = $script:config.database.server
        $this.database = $script:config.database.database
        $this.apppool = $script:config.iis.apppool
        $this.Messages = [System.Collections.Generic.List[string]]::new()
    }

    [bool] CheckExists() {
        return ([array](Invoke-SqlCmd -Query "SELECT Name From sys.databases WHERE name = '$($this.database)'" -TrustServerCertificate -ServerInstance $this.server)).Count -gt 0
    }
    
    [void] CreateDatabase() {
        
        $this.Messages.Add("Checking database exists")

        if($this.CheckExists()) {
            $this.Messages.Add("Database exists - skipping creation")
            return
        }
        
        $createdbtext = Get-Content .\createdatabase.sql | Out-String

        @(@{
            field = "[[niekoboarddb]]"
            value = $this.database
        },
        @{
            field = "[[IIS APPPOOL\AppPoolName]]"
            value = $this.apppool
        }) | foreach {
            $createdbtext = $createdbtext.replace($_.field, $_.value)
        }

        $this.Messages.Add("Creating database")
        Invoke-SqlCmd -Query $createdbtext -ServerInstance $this.server -TrustServerCertificate
        $this.Messages.Add("Creating initial schema tables")
        Invoke-SqlCmd -InputFile .\tabledefs.sql -ServerInstance $this.server -Database $this.database -TrustServerCertificate
    }

    [void] ApplyUpdates() {
        $this.Messages.Add("Getting current version")

        $appliedVersions = [System.Collections.Generic.List[Version]]::new()
        $availableVersions = [System.Collections.Generic.List[Version]]::new()
        
        foreach($versionItem in (Invoke-SqlCmd -Query "SELECT VersionNumber FROM dbo.Version" -ServerInstance $this.server -Database $this.database -TrustServerCertificate)) 
        {
            $appliedVersions.Add([Version]::new($versionItem.VersionNumber))
        }

        $latestVersion = [Version]::new()
        
        if($appliedVersions.Count -gt 0)
        {
            $latestVersion = ($appliedVersions | Sort -Descending | Select -First 1)
        }

        foreach($update in Get-ChildItem "update-*.sql") {
            $availableVersions.Add([Version]::new($update.Name.Replace("update-","").replace(".sql","")))
        }

        $newVersions = ($availableVersions | where {  $latestVersion.CompareTo($_) -lt 0 } | Sort)

        if($newVersions.Count -eq 0) {
            $this.Messages.Add("No new updates found")

            return
        }

        foreach($newVersion in $newVersions) {
            $queryFile = "update-$($newVersion.ToString()).sql"
            $this.Messages.Add("Apply update $($newVersion.ToString()))")
            Invoke-SqlCmd -InputFile $queryFile -ServerInstance $this.server -Database $this.database -TrustServerCertificate
            Invoke-SqlCmd -Query "INSERT INTO dbo.Version(VersionNumber) VALUES('$($newVersion.ToString())')" -ServerInstance $this.server -Database $this.database -TrustServerCertificate
        }
    }

    [void] InstallAndUpdate() {

        try {
            $this.Messages.Add("Running DB installation and/or apply updates")
            $this.Messages.Add("Database Server: $($this.server) Database Name: $($this.database) AppPool: $($this.apppool)")

            $this.CreateDatabase()
            $this.ApplyUpdates()

        }
        catch {
            $this.Messages.Add("")            
            $this.Messages.Add("Failed : $($_)")

            return
        }

            $this.Messages.Add("")
            $this.Messages.Add("Finished")    
    }
}