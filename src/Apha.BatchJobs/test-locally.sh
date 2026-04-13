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
        print_error "Docker not found. Please install Docker Desktop."
        exit 1
    fi
    
    if ! docker ps &> /dev/null; then
        print_error "Docker daemon not running. Please start Docker."
        exit 1
    fi
    
    print_success "Docker is available"
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
clear
echo -e "${CYAN}Batch Jobs - Local Testing Script${NC} $(date '+%Y-%m-%d %H:%M:%S')"

test_docker

if [ "$1" = "stop" ]; then
    stop_containers
    exit 0
fi

if [ "$1" = "logs" ]; then
    show_logs
    exit 0
fi

if [ "$1" = "clean" ]; then
    clean_environment
fi

echo ""
print_info "This script will:"
print_info "  1. Build Docker image (BatchJobs)"
print_info "  2. Start PostgreSQL container"
print_info "  3. Run HealthCheck batch job"
print_info "  4. Stream logs to console"
print_info "  5. Exit when job completes"
echo ""
print_info "You can Ctrl+C to stop viewing logs (containers keep running)"
echo ""
read -p "Press Enter to continue..." || true

build_and_run

show_status

echo ""
print_info "TIP: View logs again with: ./test-locally.sh logs"
print_info "TIP: Stop containers with: ./test-locally.sh stop"
echo ""
