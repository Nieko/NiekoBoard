Configuration GprcWebSite 
{
    param 
    (
        [Parameter(Mandatory = $true)]
        [string]
        $AppPoolName,

        [Parameter(Mandatory = $true)]
        [string]
        $WebAppFolder,

        [Parameter(Mandatory = $true)]
        [string]
        $ListenPort,

        [Parameter(Mandatory = $true)]
        [string]
        $ServiceAccount,

        [Parameter(Mandatory = $true)]
        [string]
        $WebSite,

        [Parameter(Mandatory = $true)]
        [bool]
        $isGMSA = $false
    )

    Import-DscResource -Module PsDesiredStateConfiguration
    Import-DscResource -Module ActiveDirectoryDsc
    Import-DscResource -Module cNtfsAccessControl
    Import-DscResource -Module WebAdministrationDsc

    $localvalue = [System.Guid]::newguid().ToString().Replace("-","")

    Node localhost
    {
        if(-not $isGMSA) {
            User "localappuser"
            {
                UserName = $ServiceAccount
                Description = "NiekoBoard app account"
                FullName = $ServiceAccount
                Password = [pscredential]::new($ServiceAccount, ($localvalue | ConvertTo-SecureString -Force -AsPlainText))
                PasswordNeverExpires = $true
                Ensure = "Present"
            }

            Group "localappuserelevate"
            {
                GroupName = "Administrators"
                MembersToInclude = $ServiceAccount
            }
        }

        File "NiekoBoardWebFolder"
        {
            Ensure = "Present"
            DestinationPath = $WebAppFolder
            TYpe = "Directory"
        }

        cNtfsPermissionEntry IISRootPermissions
        {
            Ensure = 'Present'
            Path = $WebAppFolder
            Principal = $ServiceAccount
            AccessControlInformation = @(
                cNtfsAccessControlInformation
                {
                    AccessControlType = 'Allow'
                    FileSystemRights = 'ReadAndExecute'
                    Inheritance = 'ThisFolderSubfoldersAndFiles'
                    NoPropagateInherit = $false
                }
            )
        }

        if($isGMSA)
        {
            WebAppPool 'NiekoBoardAppPool'
            {
                Name = $AppPoolName
                Ensure = 'Present'
                State = 'Started'
                autoStart = $true
                managedPipelineMode            = 'Integrated'
                managedRuntimeVersion          = 'v4.0'
                passAnonymousToken             = $true
                startMode                      = 'OnDemand'
                identityType                   = 'ApplicationPoolIdentity'
            }
        }
        else {
            WebAppPool 'NiekoBoardAppPool'
            {
                Name = $AppPoolName
                Ensure = 'Present'
                State = 'Started'
                autoStart = $true
                managedPipelineMode            = 'Integrated'
                managedRuntimeVersion          = 'v4.0'
                passAnonymousToken             = $true
                startMode                      = 'OnDemand'
                IdentityType  = 'SpecificUser'
                Credential    = [PSCredential]::new($ServiceAccount, ($localvalue | ConvertTo-SecureString -AsPlainText -Force))
            }
        }

        WebApplication "NiekoBoardWebApp"
        {
            Ensure = "Present"
            Name = "NiekoBoardServer"
            WebAppPool = $AppPoolName
            WebSite = $WebSite
            ServiceAutoStartEnabled = $true
            AuthenticationInfo = DSC_WebApplicationAuthenticationInformation
            {
                Anonymous   = $true
                Basic       = $false
                Digest      = $false
                Windows     = $true
            }
            PhysicalPath = $WebAppFolder
        }
    }
}