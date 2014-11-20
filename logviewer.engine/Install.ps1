param($installPath, $toolsPath, $package, $project)

$Global:project = $project

Write-Host (Join-Path $installPath "content")


function Edit-ProjectItem($name) {
    Write-Host "Found pattern: " $name
    
    $configItem = $project.ProjectItems.Item($name)

    # set 'Copy To Output Directory' to 'Always'
    $copyToOutput = $configItem.Properties.Item("CopyToOutputDirectory")
    $copyToOutput.Value = 1

    # set 'Build Action' to 'None'
    $buildAction = $configItem.Properties.Item("BuildAction")
    $buildAction.Value = 0
}

Get-ChildItem (Join-Path $installPath "content") -Filter *.patterns | `
Foreach-Object {
    $name = $_.Name
    Edit-ProjectItem $name
}