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
    if exist "%%D" (
        rd /s /q "%%D"
        if exist "%%D" (
            echo ERROR: failed to remove folder "%%D"
            exit /b 1
        )
    )
)

echo Removing .vs/ folder...
if exist ".vs" (
    rd /s /q ".vs"
    if exist ".vs" (
        echo ERROR: failed to remove .vs folder. The project may still be open.
        exit /b 1
    )
)

echo Removing .dotnet/ folder...
if exist ".dotnet" (
    rd /s /q ".dotnet"
    if exist ".dotnet" (
        echo ERROR: failed to remove .dotnet folder.
        exit /b 1
    )
)

echo Clean complete.
exit /b 0
