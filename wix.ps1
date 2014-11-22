param($files, $ver)

$pattern = 'define Version = "(\d+)\.(\d+)\.(\d+).(\d+)"'
$replacement = 'define Version = "' + $ver + '"'

foreach ($file in $files) {
	(Get-Content $file) | `
	Foreach-Object {$_ -replace $pattern, $replacement} | `
	Set-Content $file
}