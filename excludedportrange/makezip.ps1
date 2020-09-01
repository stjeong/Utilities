
$zipFileName = "$($env:PSTargetName).zip"

Compress-Archive $env:PSTargetDir*.* $zipFileName -Force