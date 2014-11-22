param($file, $ver)

$pattern = 'define Version = "(\d+)\.(\d+)\.(\d+).(\d+)"'
$replacement = 'define Version = "' + $ver + '"'
$content = (Get-Content $file)
$content = $content -replace $pattern, $replacement
$content | Out-File $file