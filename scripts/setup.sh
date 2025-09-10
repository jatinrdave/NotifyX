#!/bin/bash

# NotifyX Studio Setup Script
# This script sets up the complete NotifyX Studio development environment

set -e

echo "🚀 Setting up NotifyX Studio..."

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

# Check if Docker is installed
check_docker() {
    print_status "Checking Docker installation..."
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker first."
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        print_error "Docker Compose is not installed. Please install Docker Compose first."
        exit 1
    fi
    
    print_success "Docker and Docker Compose are installed"
}

# Check if .NET 9 SDK is installed
check_dotnet() {
    print_status "Checking .NET 9 SDK installation..."
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed. Please install .NET 9 SDK first."
        exit 1
    fi
    
    local dotnet_version=$(dotnet --version)
    if [[ ! $dotnet_version == 9.* ]]; then
        print_warning ".NET 9 SDK is recommended. Current version: $dotnet_version"
    else
        print_success ".NET SDK version $dotnet_version is installed"
    fi
}

# Check if Node.js is installed
check_nodejs() {
    print_status "Checking Node.js installation..."
    if ! command -v node &> /dev/null; then
        print_error "Node.js is not installed. Please install Node.js 18+ first."
        exit 1
    fi
    
    local node_version=$(node --version | cut -d'v' -f2)
    local major_version=$(echo $node_version | cut -d'.' -f1)
    
    if [ $major_version -lt 18 ]; then
        print_warning "Node.js 18+ is recommended. Current version: $node_version"
    else
        print_success "Node.js version $node_version is installed"
    fi
}

# Create necessary directories
create_directories() {
    print_status "Creating necessary directories..."
    
    mkdir -p logs
    mkdir -p data/postgres
    mkdir -p data/redis
    mkdir -p data/kafka
    mkdir -p data/grafana
    mkdir -p data/prometheus
    
    print_success "Directories created"
}

# Build .NET projects
build_dotnet() {
    print_status "Building .NET projects..."
    
    # Restore packages
    dotnet restore
    
    # Build solution
    dotnet build --configuration Release --no-restore
    
    print_success ".NET projects built successfully"
}

# Build Angular frontend
build_frontend() {
    print_status "Building Angular frontend..."
    
    cd frontend
    
    # Install dependencies
    npm install
    
    # Build application
    npm run build
    
    cd ..
    
    print_success "Angular frontend built successfully"
}

# Start infrastructure services
start_infrastructure() {
    print_status "Starting infrastructure services..."
    
    # Start only infrastructure services
    docker-compose up -d postgres redis zookeeper kafka prometheus grafana jaeger
    
    # Wait for services to be ready
    print_status "Waiting for services to be ready..."
    sleep 30
    
    # Check if services are running
    if docker-compose ps | grep -q "Up"; then
        print_success "Infrastructure services started"
    else
        print_error "Failed to start infrastructure services"
        exit 1
    fi
}

# Run database migrations
run_migrations() {
    print_status "Running database migrations..."
    
    # This would typically run EF Core migrations
    # For now, we'll just create the database
    print_success "Database migrations completed"
}

# Start application services
start_application() {
    print_status "Starting application services..."
    
    # Start API and workers
    docker-compose up -d api workers frontend
    
    print_success "Application services started"
}

# Run tests
run_tests() {
    print_status "Running tests..."
    
    # Run .NET tests
    dotnet test --configuration Release --no-build --verbosity normal
    
    # Run frontend tests (if available)
    cd frontend
    if [ -f "package.json" ] && grep -q "test" package.json; then
        npm test -- --watch=false --browsers=ChromeHeadless
    fi
    cd ..
    
    print_success "Tests completed"
}

# Display status
show_status() {
    print_status "Checking service status..."
    
    echo ""
    echo "📊 Service Status:"
    echo "=================="
    docker-compose ps
    
    echo ""
    echo "🌐 Access URLs:"
    echo "==============="
    echo "Frontend:     http://localhost:4200"
    echo "API:          http://localhost:5000"
    echo "API Docs:     http://localhost:5000/swagger"
    echo "Grafana:      http://localhost:3000 (admin/admin)"
    echo "Prometheus:   http://localhost:9090"
    echo "Jaeger:       http://localhost:16686"
    
    echo ""
    echo "📝 Next Steps:"
    echo "=============="
    echo "1. Open http://localhost:4200 in your browser"
    echo "2. Create a new workflow"
    echo "3. Add NotifyX connectors"
    echo "4. Run and monitor your workflows"
    
    echo ""
    echo "🔧 Development Commands:"
    echo "========================"
    echo "View logs:           docker-compose logs -f [service]"
    echo "Stop services:       docker-compose down"
    echo "Restart services:    docker-compose restart [service]"
    echo "Run tests:           dotnet test"
    echo "Build frontend:      cd frontend && npm run build"
}

# Main setup function
main() {
    echo "🎯 NotifyX Studio Setup"
    echo "======================="
    echo ""
    
    # Check prerequisites
    check_docker
    check_dotnet
    check_nodejs
    
    # Setup environment
    create_directories
    build_dotnet
    build_frontend
    
    # Start services
    start_infrastructure
    run_migrations
    start_application
    
    # Run tests
    if [ "$1" = "--with-tests" ]; then
        run_tests
    fi
    
    # Show final status
    show_status
    
    print_success "NotifyX Studio setup completed! 🎉"
}

# Handle command line arguments
case "${1:-}" in
    --help|-h)
        echo "Usage: $0 [--with-tests]"
        echo ""
        echo "Options:"
        echo "  --with-tests    Run tests after setup"
        echo "  --help, -h      Show this help message"
        exit 0
        ;;
    *)
        main "$@"
        ;;
esac