import-module WebAdministration

class InstallLib {

    $_Config = [object]::new()

    InstallLib($configPath) {
        $this._Config = Get-Content $configPath | ConvertFrom-Json
    }

    [void] InstallGPrc() {
        if(-not ((Get-WindowsFeature Web-Server).InstallState -eq "Installed")) {
            throw "IIS not installed"
        }

        $currentDsc = [System.Collections.Generic.HashSet[string]]::new()

        get-dscresource | select -ExpandProperty ModuleName | Get-Unique | foreach {
            $currentDsc.Add($_)
        }

        @("ActiveDirectoryDsc","cNtfsAccessControl", "WebAdministrationDsc") | foreach {
            if(-not $currentDsc.Contains($_)) {
                install-module $_
            }
        }

        $this.ClearDSC()

        . .\build-iisgprcserver-state.ps1

        if(-not $this._Config.AppPool) {
            $AppPoolName = "GprcAppPool"
        }
        else {
            $AppPoolName = $this._Config.AppPool
        }

        if(-not $this._Config.WebSite) {
            $WebSite = "Default Web Site"
        }
        else {
            $WebSite = $this._Config.WebSite
        }

        if(-not $this._Config.WebAppFolder) {
            (Get-WebFilePath "IIS:\Sites\Default Web Site").FullName + "GprcAppPool"
        }
        else {
            $WebAppFolder = $this._Config.WebAppFolder
        }

        if(-not $this._Config.ListenPort) {
            $ListenPort = 5261
        }
        else {
            $ListenPort = $this._Config.ListenPort
        }

        if(-not $this._Config.AppPoolServiceAccount) {
            $ServiceAccount = "svc-niekoboard"
        }
        else {
            $ServiceAccount = $this._Config.AppPoolServiceAccount
        }

        if(-not $this._Config.WebAppUrl) {
            $webAppUrl = "https://$([System.Net.Dns]::GetHostEntry('').HostName)/niekoboard"        
        }
        else {
            $webAppUrl = $this._Config.WebAppUrl
        }

        $cdata = @{
            AllNodes = @(
                @{
                    NodeName = 'localhost'
                    PSDscAllowPlainTextPassword=$true
                }
            )
        }

        GprcWebSite -AppPoolName $AppPoolName -WebAppFolder $WebAppFolder -ListenPort $ListenPort -ServiceAccount $ServiceAccount -WebSite $WebSite -isGMSA ($true -eq $this._Config.AppPoolAccountIsGMSA) -ConfigurationData $cdata -OutputPath .\mof\gprcwebsite

        Start-DscConfiguration -Path .\mof\gprcwebsite -Wait -Force -Verbose

        $this.ClearDSC()

        if($this._Config.AppPoolAccountIsGMSA)
        {
            Set-ItemProperty "IIS:\AppPools\YourAppPoolName" -Name processModel.identityType -Value "SpecificUser" -Value $ServiceAccount
        }

        $publishTemplate = (Get-Content .\NiekoBoard.Server\Properties\PublishProfiles\IISProfile.pubxml.template) | Out-String
    
        foreach($kvp in @{
            SiteUrl = $webAppUrl
            IISServer = [System.Net.Dns]::GetHostEntry('').HostName
            WebSite = $WebSite
        }.GetEnumerator()) {
            $publishTemplate = $publishTemplate.Replace("[[$($kvp.Key)]]", $kvp.Value)
        }

        Set-Content .\NiekoBoard.Server\Properties\PublishProfiles\IISProfile.pubxml -Value $publishTemplate
    
        $thisLocation = Get-Location
        try {
            set-location .\NiekoBoard.Server
            . dotnet @("publish", "-p:PublishProfile=IISProfile")
        }
        finally  {
            set-location = $thisLocation
        } 
    }

    [void] ClearDSC() {
        foreach($modFolder in (".\mof" | Get-ChildItem -Directory -ErrorAction SilentlyContinue)) {
            Remove-item $modFolder.FullName -Recurse -Force
        }

        Remove-DscConfigurationDocument -Stage Current, Pending, Previous -Verbose
    }
}