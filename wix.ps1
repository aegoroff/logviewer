param($files, $ver)

$pattern = 'define Version = "(\d+)\.(\d+)\.(\d+).(\d+)"'
$replacement = 'define Version = "' + $ver + '"'

foreach ($file in $files) {
	$content = (Get-Content $file)
	$content = $content -replace $pattern, $replacement
	$content | Out-File $file
}