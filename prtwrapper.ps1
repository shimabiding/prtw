Set-StrictMode -Version 2.0

$fileName = $MyInvocation.MyCommand.Name
$fileName = $fileName.Remove($fileName.LastIndexOf('.'))
$fileName

$refAssem = @(
    "System.Data"
    ".\bin\Npgsql.dll"
    "UIAutomationTypes"
    "UIAutomationClient"
)

Add-Type `
    -Path $fileName".cs" `
    -OutputAssembly .\bin\$fileName".exe" `
    -ReferencedAssemblies $refAssem
