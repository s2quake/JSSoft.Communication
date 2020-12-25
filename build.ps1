$location = Get-Location
try {
    Set-Location $PSScriptRoot
    $buildFile = "./.vscode/build.ps1"
    $outputPath = "bin"
    $propsPath = (
        "../JSSoft.Library/Directory.Build.props",
        "../JSSoft.Library.Commands/Directory.Build.props",
        "./Directory.Build.props"
    ) | ForEach-Object { "`"$_`"" }
    $propsPath = $propsPath -join ","
    $solutionPath = "./JSSoft.Communication.sln"
    if (!(Test-Path $outputPath)) {
        New-Item $outputPath -ItemType Directory
    }
    Invoke-WebRequest -Uri "https://raw.githubusercontent.com/s2quake/build/master/build.ps1" -OutFile $buildFile
    Invoke-Expression "$buildFile $solutionPath $propsPath -Publish -OutputPath $outputPath $args"
}
finally {
    Remove-Item ./.vscode/build.ps1
    Set-Location $location
}
