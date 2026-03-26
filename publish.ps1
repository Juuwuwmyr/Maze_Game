# Maze Quest Publish Automation Script

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   MAZE QUEST: BUILD - TEST - SIGN - PUBLISH    " -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

$publishDir = "./publish"

# 1. Clean previous build
if (Test-Path $publishDir) {
    Write-Host "Cleaning previous publish directory..." -ForegroundColor Gray
    Remove-Item -Path $publishDir -Recurse -Force
}

# 2. Run Tests
Write-Host "[STEP 1/3] Running Unit Tests..." -ForegroundColor Yellow
dotnet test MazeQuest.sln -c Release --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Unit tests failed. Publication aborted." -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "Tests Passed!" -ForegroundColor Green

# 3. Publish Single-File Executable
Write-Host "[STEP 2/3] Publishing Standalone Executable (win-x64)..." -ForegroundColor Yellow
# Settings: 
# -c Release: Optimized build
# -r win-x64: Target Windows 64-bit
# --self-contained: Includes .NET runtime (no install needed for user)
# PublishSingleFile: Merges everything into one EXE
# PublishReadyToRun: Improves startup performance
dotnet publish MazeKc.csproj -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Publish failed." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Publication Successful!" -ForegroundColor Green
Write-Host "Location: $(Resolve-Path $publishDir)" -ForegroundColor White

# 4. Signing Instructions
Write-Host ""
Write-Host "[STEP 3/3] Code Signing (Manual Step)" -ForegroundColor Yellow
Write-Host "To sign the executable for production, use SignTool with your PFX certificate:" -ForegroundColor Gray
Write-Host "signtool sign /f 'path\to\your\cert.pfx' /p 'your_password' /t http://timestamp.digicert.com '$publishDir\MazeKc.exe'" -ForegroundColor Cyan
Write-Host ""
Write-Host "Process Complete!" -ForegroundColor Cyan
