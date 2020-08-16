$location = Get-Location
try {
    Set-Location $PSScriptRoot
    Invoke-WebRequest -Uri "https://gist.githubusercontent.com/s2quake/57ae08b7598f1f4978d8c50326b4086d/raw/e6030100dee0be68e5c0402619fc1f3e524a8443/build.ps1" -OutFile .\.vscode\build.ps1
    .\.vscode\build.ps1 -WorkingPath $PSScriptRoot -PropsPath "base.props"
}
finally {
    Set-Location $location
}
