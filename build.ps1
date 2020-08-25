$location = Get-Location
try {
    Set-Location $PSScriptRoot
    Invoke-WebRequest -Uri "https://raw.githubusercontent.com/s2quake/build/master/build.ps1" -OutFile .\.vscode\build.ps1
    .\.vscode\build.ps1 -WorkingPath $PSScriptRoot -PropsPath "Directory.Build.props"
}
finally {
    Remove-Item .\.vscode\build.ps1
    Set-Location $location
}
