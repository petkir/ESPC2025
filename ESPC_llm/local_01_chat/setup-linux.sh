#!/bin/bash

# Linux Setup Script for Chat Application
# This script checks prerequisites and starts all necessary services

set -e

echo "🐧 Linux Setup Script for Chat Application"
echo "=========================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Detect Linux distribution
detect_distro() {
    if [ -f /etc/os-release ]; then
        . /etc/os-release
        DISTRO=$ID
        VERSION=$VERSION_ID
    else
        print_error "Cannot detect Linux distribution"
        exit 1
    fi
    print_status "Detected Linux distribution: $DISTRO $VERSION"
}

# Update package manager
update_packages() {
    print_status "Updating package manager..."
    case $DISTRO in
        ubuntu|debian)
            sudo apt update
            ;;
        fedora)
            sudo dnf update -y
            ;;
        centos|rhel)
            sudo yum update -y
            ;;
        arch)
            sudo pacman -Sy
            ;;
        *)
            print_warning "Unknown distribution, skipping package update"
            ;;
    esac
}

# Check and install curl
check_curl() {
    print_status "Checking curl..."
    if command_exists curl; then
        print_success "curl is installed"
    else
        print_warning "curl not found. Installing..."
        case $DISTRO in
            ubuntu|debian)
                sudo apt install -y curl
                ;;
            fedora)
                sudo dnf install -y curl
                ;;
            centos|rhel)
                sudo yum install -y curl
                ;;
            arch)
                sudo pacman -S --noconfirm curl
                ;;
            *)
                print_error "Please install curl manually for your distribution"
                exit 1
                ;;
        esac
        print_success "curl installed successfully"
    fi
}

# Check and install Node.js
check_node() {
    print_status "Checking Node.js..."
    if command_exists node; then
        NODE_VERSION=$(node --version)
        print_success "Node.js is installed ($NODE_VERSION)"
        
        # Check if version is 18 or higher
        MAJOR_VERSION=$(echo $NODE_VERSION | cut -d'.' -f1 | cut -d'v' -f2)
        if [ "$MAJOR_VERSION" -lt 18 ]; then
            print_warning "Node.js version is $NODE_VERSION, but v18+ is required"
            install_node
        fi
    else
        print_warning "Node.js not found. Installing..."
        install_node
    fi
    
    # Check npm
    if command_exists npm; then
        print_success "npm is available ($(npm --version))"
    else
        print_error "npm not found after Node.js installation"
        exit 1
    fi
}

# Install Node.js using NodeSource repository
install_node() {
    print_status "Installing Node.js 20 LTS..."
    case $DISTRO in
        ubuntu|debian)
            curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
            sudo apt-get install -y nodejs
            ;;
        fedora)
            curl -fsSL https://rpm.nodesource.com/setup_20.x | sudo bash -
            sudo dnf install -y nodejs npm
            ;;
        centos|rhel)
            curl -fsSL https://rpm.nodesource.com/setup_20.x | sudo bash -
            sudo yum install -y nodejs npm
            ;;
        arch)
            sudo pacman -S --noconfirm nodejs npm
            ;;
        *)
            print_error "Please install Node.js 18+ manually for your distribution"
            exit 1
            ;;
    esac
    print_success "Node.js installed successfully"
}

# Check and install .NET SDK
check_dotnet() {
    print_status "Checking .NET SDK..."
    if command_exists dotnet; then
        DOTNET_VERSION=$(dotnet --version)
        print_success ".NET SDK is installed ($DOTNET_VERSION)"
        
        # Check if version is 9.0 or higher
        MAJOR_VERSION=$(echo $DOTNET_VERSION | cut -d'.' -f1)
        if [ "$MAJOR_VERSION" -lt 9 ]; then
            print_warning ".NET SDK version is $DOTNET_VERSION, but 9.0+ is recommended"
        fi
    else
        print_warning ".NET SDK not found. Installing..."
        install_dotnet
    fi
}

# Install .NET SDK
install_dotnet() {
    print_status "Installing .NET 9 SDK..."
    case $DISTRO in
        ubuntu)
            # Add Microsoft package repository
            wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
            sudo dpkg -i packages-microsoft-prod.deb
            rm packages-microsoft-prod.deb
            sudo apt-get update
            sudo apt-get install -y dotnet-sdk-9.0
            ;;
        debian)
            wget https://packages.microsoft.com/config/debian/$(lsb_release -rs | cut -d'.' -f1)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
            sudo dpkg -i packages-microsoft-prod.deb
            rm packages-microsoft-prod.deb
            sudo apt-get update
            sudo apt-get install -y dotnet-sdk-9.0
            ;;
        fedora)
            sudo dnf install -y dotnet-sdk-9.0
            ;;
        centos|rhel)
            sudo yum install -y dotnet-sdk-9.0
            ;;
        arch)
            sudo pacman -S --noconfirm dotnet-sdk
            ;;
        *)
            print_error "Please install .NET 9 SDK manually for your distribution"
            print_status "Visit: https://dotnet.microsoft.com/download"
            exit 1
            ;;
    esac
    print_success ".NET SDK installed successfully"
}

# Check and install Ollama
check_ollama() {
    print_status "Checking Ollama..."
    if command_exists ollama; then
        print_success "Ollama is installed"
        
        # Check if Ollama is running
        if curl -s http://localhost:11434/api/version >/dev/null 2>&1; then
            print_success "Ollama server is running"
        else
            print_warning "Ollama server is not running. Restarting Ollama..."
            # Kill any existing Ollama processes
            pkill ollama 2>/dev/null || true
            sleep 2
            ollama serve &
            OLLAMA_PID=$!
            sleep 3
            if curl -s http://localhost:11434/api/version >/dev/null 2>&1; then
                print_success "Ollama server started successfully"
            else
                print_error "Failed to start Ollama server"
                exit 1
            fi
        fi
        
        # Check if llama3.2 model is available
        print_status "Checking for llama3.2 model..."
        if ollama list | grep -q "llama3.2"; then
            print_success "llama3.2 model is available"
        else
            print_warning "llama3.2 model not found. Pulling model..."
            ollama pull llama3.2
            print_success "llama3.2 model downloaded successfully"
        fi
    else
        print_warning "Ollama not found. Installing..."
        curl -fsSL https://ollama.com/install.sh | sh
        print_success "Ollama installed successfully"
        
        # Start Ollama and pull model
        print_status "Starting Ollama server..."
        # Kill any existing Ollama processes
        pkill ollama 2>/dev/null || true
        sleep 2
        ollama serve &
        OLLAMA_PID=$!
        sleep 3
        
        print_status "Pulling llama3.2 model..."
        ollama pull llama3.2
        print_success "llama3.2 model downloaded successfully"
    fi
}

# Install frontend dependencies
install_frontend_deps() {
    print_status "Installing frontend dependencies..."
    if [ -d "escp25.local.llm/escp25.local.llm.client" ]; then
        cd escp25.local.llm/escp25.local.llm.client
        if [ -f "package.json" ]; then
            npm install
            print_success "Frontend dependencies installed successfully"
        else
            print_error "package.json not found in escp25.local.llm/escp25.local.llm.client"
            exit 1
        fi
        cd ../..
    else
        print_error "Frontend directory escp25.local.llm/escp25.local.llm.client not found"
        exit 1
    fi
}

# Restore backend dependencies
restore_backend_deps() {
    print_status "Restoring backend dependencies..."
    if [ -d "escp25.local.llm/escp25.local.llm.Server" ]; then
        cd escp25.local.llm/escp25.local.llm.Server
        if [ -f "escp25.local.llm.Server.csproj" ]; then
            dotnet restore
            print_success "Backend dependencies restored successfully"
        else
            print_error "escp25.local.llm.Server.csproj not found"
            exit 1
        fi
        cd ../..
    else
        print_error "Backend directory escp25.local.llm/escp25.local.llm.Server not found"
        exit 1
    fi
}

# Start the application
start_application() {
    print_status "Starting the application..."
    
    # Check if ports are available
    if ss -tlnp | grep :5001 >/dev/null 2>&1; then
        print_warning "Port 5001 is already in use. Backend might already be running."
    else
        print_status "Starting backend server..."
        cd escp25.local.llm/escp25.local.llm.Server
        dotnet run &
        BACKEND_PID=$!
        cd ../..
        print_success "Backend server started on https://localhost:5001"
    fi
    
    if ss -tlnp | grep :5173 >/dev/null 2>&1; then
        print_warning "Port 5173 is already in use. Frontend might already be running."
    else
        print_status "Starting frontend development server..."
        cd escp25.local.llm/escp25.local.llm.client
        npm run dev &
        FRONTEND_PID=$!
        cd ../..
        print_success "Frontend development server started on http://localhost:5173"
    fi
    
    echo ""
    echo "🎉 Application setup complete!"
    echo ""
    echo "📝 Next steps:"
    echo "1. Configure Azure AD settings in appsettings.json"
    echo "2. Configure frontend auth in src/authConfig.ts"
    echo "3. Open http://localhost:5173 for development"
    echo "4. Open https://localhost:5001 for production"
    echo ""
    echo "🔧 Running processes:"
    if [ ! -z "$OLLAMA_PID" ]; then
        echo "   - Ollama server (PID: $OLLAMA_PID)"
    fi
    if [ ! -z "$BACKEND_PID" ]; then
        echo "   - Backend server (PID: $BACKEND_PID)"
    fi
    if [ ! -z "$FRONTEND_PID" ]; then
        echo "   - Frontend dev server (PID: $FRONTEND_PID)"
    fi
    echo ""
    echo "💡 To stop all services, press Ctrl+C"
}

# Cleanup function for graceful shutdown
cleanup() {
    echo ""
    print_status "Shutting down services..."
    if [ ! -z "$OLLAMA_PID" ]; then
        kill $OLLAMA_PID 2>/dev/null || true
    fi
    if [ ! -z "$BACKEND_PID" ]; then
        kill $BACKEND_PID 2>/dev/null || true
    fi
    if [ ! -z "$FRONTEND_PID" ]; then
        kill $FRONTEND_PID 2>/dev/null || true
    fi
    print_success "All services stopped"
    exit 0
}

# Set up signal handlers
trap cleanup SIGINT SIGTERM

# Main execution
main() {
    echo ""
    detect_distro
    
    print_status "Starting prerequisite checks..."
    update_packages
    check_curl
    check_node
    check_dotnet
    check_ollama
    
    echo ""
    print_status "Installing dependencies..."
    install_frontend_deps
    restore_backend_deps
    
    echo ""
    start_application
    
    # Keep script running
    while true; do
        sleep 1
    done
}

# Run main function
main
