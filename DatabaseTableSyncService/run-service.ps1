#!/usr/bin/env pwsh
<#
.SYNOPSIS
	Test runner for Database Table Synchronization Service

.DESCRIPTION
	This script helps you run and test the worker service with various options.

.PARAMETER Environment
	The environment to run in (Development, Staging, Production)

.PARAMETER WaitForDebugger
	If specified, waits for debugger to attach before running

.EXAMPLE
	.\run-service.ps1
	Run in development mode

.EXAMPLE
	.\run-service.ps1 -Environment Production
	Run in production mode

.EXAMPLE
	.\run-service.ps1 -WaitForDebugger
	Run and wait for debugger to attach
#>

param(
	[Parameter()]
	[ValidateSet("Development", "Staging", "Production")]
	[string]$Environment = "Development",

	[Parameter()]
	[switch]$WaitForDebugger
)

# Colors for output
function Write-Banner {
	param([string]$Message)
	Write-Host ""
	Write-Host "================================================" -ForegroundColor Cyan
	Write-Host "  $Message" -ForegroundColor Cyan
	Write-Host "================================================" -ForegroundColor Cyan
	Write-Host ""
}

function Write-Step {
	param([string]$Message)
	Write-Host "▶ $Message" -ForegroundColor Yellow
}

function Write-Success {
	param([string]$Message)
	Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error-Custom {
	param([string]$Message)
	Write-Host "✗ $Message" -ForegroundColor Red
}

# Main script
Write-Banner "Database Table Sync Service - Test Runner"

Write-Step "Environment: $Environment"

# Set environment variable
$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:DOTNET_ENVIRONMENT = $Environment

# Check if configuration exists
Write-Step "Checking configuration..."

if (-not (Test-Path "appsettings.json")) {
	Write-Error-Custom "appsettings.json not found!"
	exit 1
}

Write-Success "Configuration file found"

# Build the project first
Write-Step "Building project..."
dotnet build --configuration Debug --no-restore

if ($LASTEXITCODE -ne 0) {
	Write-Error-Custom "Build failed!"
	exit 1
}

Write-Success "Build succeeded"

# Check connection strings
Write-Step "Validating connection strings..."

$config = Get-Content "appsettings.json" | ConvertFrom-Json

$sourceDb = $config.ConnectionStrings.SourceDatabase
$targetDb = $config.ConnectionStrings.TargetDatabase

if ($sourceDb -like "*SOURCE_SERVER*" -or $sourceDb -like "*SOURCE_DATABASE*") {
	Write-Host ""
	Write-Host "⚠ WARNING: Source database connection string contains placeholders!" -ForegroundColor Yellow
	Write-Host "  Update appsettings.json or use User Secrets before running against real databases." -ForegroundColor Yellow
	Write-Host ""
}

if ($targetDb -like "*TARGET_SERVER*" -or $targetDb -like "*TARGET_DATABASE*") {
	Write-Host ""
	Write-Host "⚠ WARNING: Target database connection string contains placeholders!" -ForegroundColor Yellow
	Write-Host "  Update appsettings.json or use User Secrets before running against real databases." -ForegroundColor Yellow
	Write-Host ""
}

# Wait for debugger if requested
if ($WaitForDebugger) {
	Write-Host ""
	Write-Host "Waiting for debugger to attach..." -ForegroundColor Magenta
	Write-Host "Process ID: $PID" -ForegroundColor Magenta
	Write-Host "Press any key to continue once debugger is attached..." -ForegroundColor Magenta
	$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

# Run the service
Write-Banner "Starting Service"

Write-Host "Press Ctrl+C to stop the service`n" -ForegroundColor Gray

try {
	dotnet run --no-build --environment $Environment

	if ($LASTEXITCODE -eq 0) {
		Write-Banner "Service Completed Successfully"
	} else {
		Write-Banner "Service Exited with Errors"
		exit $LASTEXITCODE
	}
}
catch {
	Write-Error-Custom "Service execution failed: $_"
	exit 1
}
