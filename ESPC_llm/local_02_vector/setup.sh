#!/bin/bash

# Universal Setup Script Launcher
# Detects the operating system and runs the appropriate setup script

echo "🚀 Chat Application Setup"
echo "========================="

# Detect OS
OS="Unknown"
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    OS="Linux"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    OS="macOS"
elif [[ "$OSTYPE" == "cygwin" ]] || [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "win32" ]]; then
    OS="Windows"
fi

echo "Detected OS: $OS"
echo ""

case $OS in
    "Linux")
        echo "Running Linux setup script..."
        if [ -f "./setup-linux.sh" ]; then
            chmod +x ./setup-linux.sh
            ./setup-linux.sh
        else
            echo "Error: setup-linux.sh not found!"
            exit 1
        fi
        ;;
    "macOS")
        echo "Running macOS setup script..."
        if [ -f "./setup-macos.sh" ]; then
            chmod +x ./setup-macos.sh
            ./setup-macos.sh
        else
            echo "Error: setup-macos.sh not found!"
            exit 1
        fi
        ;;
    "Windows")
        echo "Please run setup-windows.bat directly on Windows"
        echo "Or use WSL/Git Bash and run the Linux script"
        exit 1
        ;;
    *)
        echo "Unsupported operating system: $OSTYPE"
        echo ""
        echo "Please run the appropriate script manually:"
        echo "  - Linux: ./setup-linux.sh"
        echo "  - macOS: ./setup-macos.sh"
        echo "  - Windows: setup-windows.bat"
        exit 1
        ;;
esac
