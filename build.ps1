param(
    [string]$OutputPath = "",
    [string]$Framework = "netcoreapp3.1",
    [string]$KeyPath = "",
    [string]$LogPath = "",
    [switch]$Force
)

$solutionPath = "./JSSoft.Communication.sln"
$propPaths = (
    "../JSSoft.Library/Directory.Build.props",
    "../JSSoft.Library.Commands/Directory.Build.props",
    "./Directory.Build.props"
)

try {
    $buildFile = "./build-temp.ps1"
    $solutionPath = Join-Path $PSScriptRoot $solutionPath -Resolve
    $propPaths = $propPaths | ForEach-Object { Join-Path $PSScriptRoot $_ -Resolve }
    Invoke-WebRequest -Uri "https://raw.githubusercontent.com/s2quake/build/master/build.ps1" -OutFile $buildFile
    $buildFile = Resolve-Path $buildFile
    & $buildFile $solutionPath $propPaths -Publish -KeyPath $KeyPath -Sign -OutputPath $OutputPath -Framework $Framework -LogPath $LogPath -Force:$Force
}
finally {
    Remove-Item $buildFile
}
