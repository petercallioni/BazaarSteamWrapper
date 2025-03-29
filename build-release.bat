@echo off
setlocal

REM Set the output directory from the first argument
if "%~1"=="" (
    echo Error: No output directory specified.
    exit /b 1
)

set OUTPUT_DIR=%~1

REM Check if the output directory exists
if not exist "%OUTPUT_DIR%" (
    echo Error: Output directory does not exist: %OUTPUT_DIR%
    exit /b 1
)

REM Get the repository root
for /f "delims=" %%i in ('git rev-parse --show-toplevel') do set REPO_ROOT=%%i

if "%REPO_ROOT%"=="" (
    echo Error: Unable to retrieve repository root.
    exit /b 1
)

cd /d "%REPO_ROOT%"

REM Get the current tag
for /f "delims=" %%i in ('git describe --tags --abbrev=0 2^>nul') do set TAG_NAME=%%i

if "%TAG_NAME%"=="" (
    echo Warning: No tag found. Using 'latest' as the default tag.
    set TAG_NAME=latest
) else (
    echo Tag found: %TAG_NAME%
)

REM Get the repository name
for /f "delims=" %%i in ('git rev-parse --show-toplevel') do set REPO_ROOT=%%i
for /f "tokens=*" %%i in ("%REPO_ROOT%") do set REPO_NAME=%%~nxi

REM Define the zip file name
set ZIP_FILE_NAME=%REPO_NAME%-%TAG_NAME%.zip

REM Create the zip file using PowerShell
powershell -Command "Compress-Archive -Path '%OUTPUT_DIR%\*' -DestinationPath '%ZIP_FILE_NAME%'"

if %errorlevel% neq 0 (
    echo Error: Failed to create zip file.
    exit /b %errorlevel%
)

echo Successfully created zip file: %ZIP_FILE_NAME%

endlocal