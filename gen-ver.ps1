$majorVersion = 1
$minorVersion = 0
$version = "$majorVersion.$minorVersion"
$fileVersion = "$majorVersion.$minorVersion" + "." + (Get-Date -Format yy) + (Get-Date).DayOfYear + "." + (Get-Date -Format HHmm)
$fileName = "$PSScriptRoot\base.props"

[xml]$doc = Get-Content $fileName -Encoding UTF8
foreach ($obj in $doc.Project.PropertyGroup) {        
    if ($obj.Version) {
        $obj.Version = $fileVersion
    }
    if ($obj.FileVersion) {
        $obj.FileVersion = $fileVersion
    }
    if ($obj.AssemblyVersion) {
        $obj.AssemblyVersion = $version
    }
}
$doc.Save($fileName)

Set-Content (Join-Path $PSScriptRoot version.txt) $fileVersion -NoNewline -Encoding UTF8
Write-Host $fileVersion