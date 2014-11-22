param($file, $ver)

$pattern = 'define Version = "(\d+)\.(\d+)\.(\d+).(\d+)"'
$replacement = 'define Version = "' + $ver + '"'

(Get-Content $file) | 
Foreach-Object {$_ -replace $pattern, $replacement} | 
Set-Content $file