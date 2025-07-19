@echo off
setlocal enabledelayedexpansion

REM Windows Setup Script for Chat Application
REM This script checks prerequisites and starts all necessary services

echo 🪟 Windows Setup Script for Chat Application
echo ============================================

REM Function to print colored output (basic)
set "INFO=[INFO]"
set "SUCCESS=[SUCCESS]"
set "WARNING=[WARNING]"
set "ERROR=[ERROR]"

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo %SUCCESS% Running as administrator
) else (
    echo %WARNING% Not running as administrator. Some installations may require elevation.
)

REM Function to check if command exists
where node >nul 2>&1
if %errorLevel% == 0 (
    set NODE_EXISTS=1
) else (
    set NODE_EXISTS=0
)

where dotnet >nul 2>&1
if %errorLevel% == 0 (
    set DOTNET_EXISTS=1
) else (
    set DOTNET_EXISTS=0
)

where ollama >nul 2>&1
if %errorLevel% == 0 (
    set OLLAMA_EXISTS=1
) else (
    set OLLAMA_EXISTS=0
)

where winget >nul 2>&1
if %errorLevel% == 0 (
    set WINGET_EXISTS=1
) else (
    set WINGET_EXISTS=0
)

where choco >nul 2>&1
if %errorLevel% == 0 (
    set CHOCO_EXISTS=1
) else (
    set CHOCO_EXISTS=0
)

echo.
echo %INFO% Starting prerequisite checks...

REM Check Package Manager
echo %INFO% Checking package managers...
if %WINGET_EXISTS% == 1 (
    echo %SUCCESS% WinGet is available
    set PACKAGE_MANAGER=winget
) else if %CHOCO_EXISTS% == 1 (
    echo %SUCCESS% Chocolatey is available
    set PACKAGE_MANAGER=choco
) else (
    echo %WARNING% No package manager found. Installing Chocolatey...
    
    REM Install Chocolatey
    powershell -Command "Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))"
    
    if %errorLevel% == 0 (
        echo %SUCCESS% Chocolatey installed successfully
        set PACKAGE_MANAGER=choco
        set CHOCO_EXISTS=1
    ) else (
        echo %ERROR% Failed to install Chocolatey
        echo Please install Node.js, .NET SDK, and Ollama manually
        pause
        exit /b 1
    )
)

REM Check and install Node.js
echo %INFO% Checking Node.js...
if %NODE_EXISTS% == 1 (
    for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
    echo %SUCCESS% Node.js is installed (!NODE_VERSION!)
    
    REM Extract major version
    for /f "tokens=1 delims=." %%a in ("!NODE_VERSION:v=!") do set MAJOR_VERSION=%%a
    if !MAJOR_VERSION! lss 18 (
        echo %WARNING% Node.js version is !NODE_VERSION!, but v18+ is required
        goto install_node
    )
) else (
    :install_node
    echo %WARNING% Node.js not found or outdated. Installing...
    if "%PACKAGE_MANAGER%" == "winget" (
        winget install OpenJS.NodeJS
    ) else if "%PACKAGE_MANAGER%" == "choco" (
        choco install nodejs -y
    )
    
    if %errorLevel% == 0 (
        echo %SUCCESS% Node.js installed successfully
        set NODE_EXISTS=1
    ) else (
        echo %ERROR% Failed to install Node.js
        pause
        exit /b 1
    )
)

REM Check npm
where npm >nul 2>&1
if %errorLevel% == 0 (
    for /f "tokens=*" %%i in ('npm --version') do set NPM_VERSION=%%i
    echo %SUCCESS% npm is available (!NPM_VERSION!)
) else (
    echo %ERROR% npm not found after Node.js installation
    pause
    exit /b 1
)

REM Check and install .NET SDK
echo %INFO% Checking .NET SDK...
if %DOTNET_EXISTS% == 1 (
    for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
    echo %SUCCESS% .NET SDK is installed (!DOTNET_VERSION!)
    
    REM Extract major version
    for /f "tokens=1 delims=." %%a in ("!DOTNET_VERSION!") do set MAJOR_VERSION=%%a
    if !MAJOR_VERSION! lss 9 (
        echo %WARNING% .NET SDK version is !DOTNET_VERSION!, but 9.0+ is recommended
    )
) else (
    echo %WARNING% .NET SDK not found. Installing...
    if "%PACKAGE_MANAGER%" == "winget" (
        winget install Microsoft.DotNet.SDK.9
    ) else if "%PACKAGE_MANAGER%" == "choco" (
        choco install dotnet-sdk -y
    )
    
    if %errorLevel% == 0 (
        echo %SUCCESS% .NET SDK installed successfully
        set DOTNET_EXISTS=1
    ) else (
        echo %ERROR% Failed to install .NET SDK
        echo Please install .NET 9 SDK manually from https://dotnet.microsoft.com/download
        pause
        exit /b 1
    )
)

REM Check and install Ollama
echo %INFO% Checking Ollama...
if %OLLAMA_EXISTS% == 1 (
    echo %SUCCESS% Ollama is installed
    
    REM Check if Ollama is running
    curl -s http://localhost:11434/api/version >nul 2>&1
    if %errorLevel% == 0 (
        echo %SUCCESS% Ollama server is running
    ) else (
        echo %WARNING% Ollama server is not running. Restarting Ollama...
        REM Kill any existing Ollama processes
        taskkill /f /im ollama.exe >nul 2>&1
        timeout /t 2 /nobreak >nul
        start /b ollama serve
        timeout /t 3 /nobreak >nul
        
        curl -s http://localhost:11434/api/version >nul 2>&1
        if %errorLevel% == 0 (
            echo %SUCCESS% Ollama server started successfully
        ) else (
            echo %ERROR% Failed to start Ollama server
            pause
            exit /b 1
        )
    )
    
    REM Check if llama3.2 model is available
    echo %INFO% Checking for llama3.2 model...
    ollama list | findstr "llama3.2" >nul 2>&1
    if %errorLevel% == 0 (
        echo %SUCCESS% llama3.2 model is available
    ) else (
        echo %WARNING% llama3.2 model not found. Pulling model...
        ollama pull llama3.2
        if %errorLevel% == 0 (
            echo %SUCCESS% llama3.2 model downloaded successfully
        ) else (
            echo %ERROR% Failed to download llama3.2 model
            pause
            exit /b 1
        )
    )
) else (
    echo %WARNING% Ollama not found. Installing...
    if "%PACKAGE_MANAGER%" == "winget" (
        winget install Ollama.Ollama
    ) else if "%PACKAGE_MANAGER%" == "choco" (
        choco install ollama -y
    )
    
    if %errorLevel% == 0 (
        echo %SUCCESS% Ollama installed successfully
        
        REM Start Ollama and pull model
        echo %INFO% Starting Ollama server...
        REM Kill any existing Ollama processes
        taskkill /f /im ollama.exe >nul 2>&1
        timeout /t 2 /nobreak >nul
        start /b ollama serve
        timeout /t 3 /nobreak >nul
        
        echo %INFO% Pulling llama3.2 model...
        ollama pull llama3.2
        if %errorLevel% == 0 (
            echo %SUCCESS% llama3.2 model downloaded successfully
        ) else (
            echo %ERROR% Failed to download llama3.2 model
            pause
            exit /b 1
        )
    ) else (
        echo %ERROR% Failed to install Ollama
        echo Please install Ollama manually from https://ollama.com
        pause
        exit /b 1
    )
)

REM Install frontend dependencies
echo.
echo %INFO% Installing dependencies...
echo %INFO% Installing frontend dependencies...
if exist "escp25.local.llm\escp25.local.llm.client" (
    cd escp25.local.llm\escp25.local.llm.client
    if exist "package.json" (
        npm install
        if %errorLevel% == 0 (
            echo %SUCCESS% Frontend dependencies installed successfully
        ) else (
            echo %ERROR% Failed to install frontend dependencies
            pause
            exit /b 1
        )
    ) else (
        echo %ERROR% package.json not found in escp25.local.llm\escp25.local.llm.client
        pause
        exit /b 1
    )
    cd ..\..
) else (
    echo %ERROR% Frontend directory escp25.local.llm\escp25.local.llm.client not found
    pause
    exit /b 1
)

REM Restore backend dependencies
echo %INFO% Restoring backend dependencies...
if exist "escp25.local.llm\escp25.local.llm.Server" (
    cd escp25.local.llm\escp25.local.llm.Server
    if exist "escp25.local.llm.Server.csproj" (
        dotnet restore
        if %errorLevel% == 0 (
            echo %SUCCESS% Backend dependencies restored successfully
        ) else (
            echo %ERROR% Failed to restore backend dependencies
            pause
            exit /b 1
        )
    ) else (
        echo %ERROR% escp25.local.llm.Server.csproj not found
        pause
        exit /b 1
    )
    cd ..\..
) else (
    echo %ERROR% Backend directory escp25.local.llm\escp25.local.llm.Server not found
    pause
    exit /b 1
)

REM Start the application
echo.
echo %INFO% Starting the application...

REM Check if ports are available
netstat -an | findstr ":5001" >nul 2>&1
if %errorLevel% == 0 (
    echo %WARNING% Port 5001 is already in use. Backend might already be running.
) else (
    echo %INFO% Starting backend server...
    cd escp25.local.llm\escp25.local.llm.Server
    start /b dotnet run
    cd ..\..
    echo %SUCCESS% Backend server started on https://localhost:5001
)

netstat -an | findstr ":5173" >nul 2>&1
if %errorLevel% == 0 (
    echo %WARNING% Port 5173 is already in use. Frontend might already be running.
) else (
    echo %INFO% Starting frontend development server...
    cd escp25.local.llm\escp25.local.llm.client
    start /b npm run dev
    cd ..\..
    echo %SUCCESS% Frontend development server started on http://localhost:5173
)

echo.
echo 🎉 Application setup complete!
echo.
echo 📝 Next steps:
echo 1. Configure Azure AD settings in appsettings.json
echo 2. Configure frontend auth in src/authConfig.ts
echo 3. Open http://localhost:5173 for development
echo 4. Open https://localhost:5001 for production
echo.
echo 🔧 Services are running in the background
echo 💡 To stop services, close this window or use Task Manager
echo.

REM Keep window open
echo Press any key to open the application in browser...
pause >nul

REM Open browser
start http://localhost:5173

echo.
echo Application opened in browser.
echo Press any key to exit...
pause >nul
