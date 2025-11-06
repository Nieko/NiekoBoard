 #requires -RunAsAdministrator

foreach($psFolderRoot in @("System32","SysWOW64")) {
    $fullpath = "$($env:windir)\$psFolderRoot\WindowsPowerShell\v1.0"

    if(Test-PAth $fullpath -ErrorAction SilentlyContinue) {
        $currentSettings = (. "$fullpath\powershell.exe" @("-Command", '"Get-ExecutionPolicy"'))
        if(-not (@("Bypass","RemoteSigned") -contains $currentSettings)) {
            . "$fullpath\powershell.exe" @("-Command", '"Set-ExecutionPolicy RemoteSigned"')
        }
    }
}