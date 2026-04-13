#!/bin/bash
#
# One-command local testing script for Batch Jobs foundation layer.
#
# Usage:
#   ./test-locally.sh           # Build, start DB, run job
#   ./test-locally.sh logs      # Show logs from running containers
#   ./test-locally.sh stop      # Stop all containers
#   ./test-locally.sh clean     # Clean containers and volumes, then run
#

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
JOB_NAME="HealthCheck"
NO_PROMPT="false"
NATIVE="false"
EXECUTION_MODE=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

function print_header() {
    echo ""
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}"
}

function print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

function print_error() {
    echo -e "${RED}✗ $1${NC}"
}

function print_info() {
    echo -e "${YELLOW}→ $1${NC}"
}

function test_docker() {
    print_info "Checking Docker..."
    if ! command -v docker &> /dev/null; then
        print_error "Docker not found."
        return 1
    fi
    
    if ! docker ps &> /dev/null; then
        print_error "Docker daemon not running."
        return 1
    fi
    
    print_success "Docker is available"
    return 0
}

function detect_execution_mode() {
    if [ "$NATIVE" = "true" ]; then
        echo "native"
        return
    fi

    if ! test_docker; then
        echo "native"
        return
    fi

    DOCKER_OS_TYPE=$(docker info --format '{{.OSType}}' 2>/dev/null || echo "")
    if [ "$DOCKER_OS_TYPE" = "linux" ]; then
        echo "docker"
        return
    fi

    if [ "$DOCKER_OS_TYPE" = "windows" ]; then
        print_info "Docker daemon is running in Windows container mode; native .NET mode will be used instead."
    else
        print_info "Unable to determine Docker container mode; native .NET mode will be used instead."
    fi

    echo "native"
}

function stop_containers() {
    print_header "Stopping Containers"
    pushd "$SCRIPT_DIR" > /dev/null
    docker-compose down 2>/dev/null || true
    popd > /dev/null
    print_success "Containers stopped"
}

function clean_environment() {
    print_header "Cleaning Environment"
    print_info "Removing containers and volumes..."
    pushd "$SCRIPT_DIR" > /dev/null
    docker-compose down -v 2>/dev/null || true
    docker-compose rm -f 2>/dev/null || true
    popd > /dev/null
    print_success "Environment cleaned"
}

function show_logs() {
    print_header "Showing Logs"
    pushd "$SCRIPT_DIR" > /dev/null
    docker-compose logs -f batch-jobs
    popd > /dev/null
}

function build_and_run() {
    print_header "Building Docker Image"
    
    pushd "$SCRIPT_DIR" > /dev/null
    
    if ! docker-compose build; then
        popd > /dev/null
        print_error "Docker build failed"
        exit 1
    fi
    
    print_success "Image built successfully"
    
    print_header "Starting Services with docker-compose"
    echo ""
    print_info "Starting PostgreSQL and Batch Job..."
    print_info "Press Ctrl+C to stop viewing logs (containers keep running)"
    echo ""
    
    docker-compose up --no-build
    popd > /dev/null
}

function run_native() {
    print_header "Running Native .NET Validation"
    pushd "$SCRIPT_DIR" > /dev/null
    dotnet build
    dotnet test ./Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj --no-build
    dotnet run --project BatchJobs.csproj -- "$JOB_NAME"
    popd > /dev/null
}

function show_status() {
    echo ""
    print_header "Container Status Check"
    
    pushd "$SCRIPT_DIR" > /dev/null
    
    if docker-compose ps -q | grep -q .; then
        docker-compose ps
        echo ""
        
        # Try to get exit code of batch-jobs container
        EXIT_CODE=$(docker wait batch-jobs-app 2>/dev/null || echo "")
        if [ -n "$EXIT_CODE" ]; then
            echo "Batch job exit code: $EXIT_CODE"
            if [ "$EXIT_CODE" = "0" ]; then
                print_success "Job completed successfully"
            else
                print_error "Job failed with code: $EXIT_CODE"
            fi
        fi
    else
        print_info "No containers running"
    fi
    
    popd > /dev/null
}

# Main script
while [[ $# -gt 0 ]]; do
    case "$1" in
        stop)
            ACTION="stop"
            shift
            ;;
        logs)
            ACTION="logs"
            shift
            ;;
        clean)
            ACTION="clean"
            shift
            ;;
        --native)
            NATIVE="true"
            shift
            ;;
        --no-prompt)
            NO_PROMPT="true"
            shift
            ;;
        --job)
            JOB_NAME="$2"
            shift 2
            ;;
        *)
            shift
            ;;
    esac
done

clear
echo -e "${CYAN}Batch Jobs - Local Testing Script${NC} $(date '+%Y-%m-%d %H:%M:%S')"

EXECUTION_MODE=$(detect_execution_mode)
print_info "Execution mode: $EXECUTION_MODE"

if [ "$ACTION" = "stop" ]; then
    if [ "$EXECUTION_MODE" = "docker" ]; then
        stop_containers
    else
        print_info "Stop is only applicable in docker mode"
    fi
    exit 0
fi

if [ "$ACTION" = "logs" ]; then
    if [ "$EXECUTION_MODE" = "docker" ]; then
        show_logs
    else
        print_info "Logs are only applicable in docker mode"
    fi
    exit 0
fi

if [ "$ACTION" = "clean" ] && [ "$EXECUTION_MODE" = "docker" ]; then
    clean_environment
fi

echo ""
print_info "This script will:"
if [ "$EXECUTION_MODE" = "docker" ]; then
    print_info "  1. Build Docker image (BatchJobs)"
    print_info "  2. Start PostgreSQL container"
    print_info "  3. Run $JOB_NAME batch job"
    print_info "  4. Stream logs to console"
    print_info "  5. Exit when job completes"
else
    print_info "  1. Build the .NET project"
    print_info "  2. Run the local unit test suite"
    print_info "  3. Run $JOB_NAME natively"
    print_info "  4. Validate worker bootstrap, logging, and job execution"
fi
echo ""
if [ "$EXECUTION_MODE" = "docker" ]; then
    print_info "You can Ctrl+C to stop viewing logs (containers keep running)"
fi
echo ""
if [ "$NO_PROMPT" != "true" ]; then
    read -p "Press Enter to continue..." || true
fi

if [ "$EXECUTION_MODE" = "docker" ]; then
    build_and_run
    show_status
else
    run_native
fi

echo ""
print_info "TIP: View logs again with: ./test-locally.sh logs"
print_info "TIP: Stop containers with: ./test-locally.sh stop"
echo ""
