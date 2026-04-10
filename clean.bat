@echo off
setlocal
cd /d "%~dp0"

echo Cleaning dotnet build outputs...
dotnet clean
if errorlevel 1 (
    echo ERROR: dotnet clean failed
    exit /b 1
)

echo Removing bin/ and obj/ folders...
for /d /r %%D in (bin,obj) do (
    if exist "%%D" rd /s /q "%%D"
)

echo Removing .vs/ folder...
if exist ".vs" rd /s /q ".vs"

echo Clean complete.