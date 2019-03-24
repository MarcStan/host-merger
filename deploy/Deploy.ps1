<#
.SYNOPSIS

Deploys a function app (consumption plan) with all necessary infrastructure.

.PARAMETER ResourceGroupName

Name of the resourcegroup to deploy. Name of resources will be derived from it.

#>

param(
    [Parameter(Mandatory=$true)]
    [string] $ResourceGroupName,
    [string] $ResourceGroupLocation = "westeurope"
)
$ErrorActionPreference = "Stop"

$ResourceGroup = Get-AzureRmResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
if ($ResourceGroup -eq $null) {
    Write-Output "Creating resourcegroup $ResourceGroupName in $ResourceGroupLocation"
    New-AzureRmResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation -Force -ErrorAction Stop
}

Write-Output "Deploying resourcegroup $ResourceGroupName"

$templateFile = Join-Path $PSScriptRoot "deploy.json"
$parameters = @{ }

$date = date -Format "yyyy-MM-dd_HH-mm-ss"
New-AzureRmResourceGroupDeployment `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile $templateFile `
    -TemplateParameterObject $parameters `
    -Name "$date" `
