# NotifyX Studio - Production-Ready Project Management & Workflow Automation Platform

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/your-org/notifyx-studio)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![API Documentation](https://img.shields.io/badge/API-Documented-green.svg)](/api/docs)

NotifyX Studio is a comprehensive, production-ready project management and workflow automation platform built with .NET 8, featuring advanced security, monitoring, and scalability capabilities.

## 🚀 Features

### Core Functionality
- **Project Management**: Complete project lifecycle management with tasks, epics, and milestones
- **Workflow Automation**: Visual workflow designer with triggers, nodes, and execution tracking
- **Team Collaboration**: User management, roles, permissions, and real-time notifications
- **Integration Platform**: Extensible connector system for third-party services
- **Reporting & Analytics**: Comprehensive reporting with dashboards and metrics

### Production-Ready Features
- **Authentication & Authorization**: JWT-based security with role-based access control
- **Comprehensive Logging**: Structured logging with Serilog, file, console, and Seq outputs
- **Health Monitoring**: Advanced health checks with UI dashboard
- **API Documentation**: Complete OpenAPI/Swagger documentation
- **Request Validation**: FluentValidation-based input validation
- **Error Handling**: Global exception handling with structured error responses
- **Rate Limiting**: Configurable rate limiting for API protection
- **Caching**: Redis-based caching for performance optimization
- **Compression**: Response compression with Gzip and Brotli
- **Security Headers**: Comprehensive security headers implementation
- **Performance Monitoring**: Request/response time tracking and metrics

## 🏗️ Architecture

### Technology Stack
- **Backend**: .NET 8, ASP.NET Core Web API
- **Authentication**: JWT Bearer tokens
- **Logging**: Serilog with multiple sinks
- **Validation**: FluentValidation
- **Documentation**: Swagger/OpenAPI
- **Testing**: xUnit, Moq, FluentAssertions
- **Health Checks**: ASP.NET Core Health Checks with UI
- **Real-time**: SignalR for live updates

### Project Structure
```
src/
├── NotifyXStudio.Api/              # Web API layer
│   ├── Controllers/                # API controllers
│   ├── Middleware/                 # Custom middleware
│   ├── Configuration/              # Service configurations
│   └── Program.cs                  # Application entry point
├── NotifyXStudio.Core/             # Core business logic
│   ├── Models/                     # Domain models
│   ├── Interfaces/                 # Service interfaces
│   └── Services/                   # Service implementations
├── NotifyXStudio.Persistence/      # Data access layer
├── NotifyXStudio.Connectors/       # External integrations
├── NotifyXStudio.Runtime/          # Workflow runtime
└── NotifyXStudio.Api.Tests/        # Test projects
```

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Redis](https://redis.io/) (optional, for caching)
- [Seq](https://datalust.co/seq) (optional, for structured logging)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/notifyx-studio.git
   cd notifyx-studio
   ```

2. **Configure application settings**
   ```bash
   # Update appsettings.json with your configuration
   cp src/NotifyXStudio.Api/appsettings.json src/NotifyXStudio.Api/appsettings.Development.json
   ```

3. **Run the application**
   ```bash
   cd src/NotifyXStudio.Api
   dotnet run
   ```

4. **Access the application**
   - API: https://localhost:7152
   - Swagger Documentation: https://localhost:7152/api/docs
   - Health Checks: https://localhost:7152/health
   - Health UI: https://localhost:7152/health-ui

## 📖 API Documentation

### Authentication
The API uses JWT Bearer tokens for authentication. Include the token in the Authorization header:

```http
Authorization: Bearer <your-jwt-token>
```

### Endpoints Overview
- **Authentication**: `/api/auth/*` - Login, refresh tokens
- **Projects**: `/api/projects/*` - Project management
- **Tasks**: `/api/tasks/*` - Task management
- **Workflows**: `/api/workflows/*` - Workflow automation
- **Users**: `/api/users/*` - User management
- **Health**: `/health/*` - System health monitoring

### Example API Calls

#### Authentication
```bash
# Login
curl -X POST https://localhost:7152/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "password123"
  }'
```

#### Create Project
```bash
# Create a new project
curl -X POST https://localhost:7152/api/projects \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Project",
    "description": "Project description",
    "tenantId": "tenant-guid"
  }'
```

## 🔧 Configuration

### Environment Variables
```bash
# Database
DATABASE_CONNECTION_STRING="Server=localhost;Database=NotifyXStudio;..."

# JWT
JWT_SECRET_KEY="YourSuperSecretKeyThatIsAtLeast32CharactersLong!"

# Redis
REDIS_CONNECTION_STRING="localhost:6379"

# Seq
SEQ_CONNECTION_STRING="http://localhost:5341"
```

### Application Settings
Key configuration sections in `appsettings.json`:

- **JwtSettings**: JWT token configuration
- **ConnectionStrings**: Database and external service connections
- **RateLimiting**: API rate limiting rules
- **Security**: Security headers and policies
- **Logging**: Serilog configuration
- **HealthChecks**: Health monitoring settings

## 🔒 Security

### Authentication & Authorization
- JWT-based authentication with configurable expiry
- Role-based authorization (Admin, Manager, User)
- Tenant-based access control
- Refresh token support

### Security Headers
- Content Security Policy (CSP)
- HTTP Strict Transport Security (HSTS)
- X-Frame-Options
- X-Content-Type-Options
- Referrer Policy
- Permissions Policy

### Rate Limiting
- Per-IP rate limiting
- Different limits for authenticated/unauthenticated users
- Configurable time windows and token buckets

## 📊 Monitoring & Observability

### Health Checks
- Application health: `/health`
- Detailed diagnostics: `/health/detailed`
- Kubernetes readiness: `/health/ready`
- Kubernetes liveness: `/health/live`
- Health UI dashboard: `/health-ui`

### Logging
- Structured logging with Serilog
- Multiple output sinks (Console, File, Seq)
- Correlation IDs for request tracking
- Performance metrics logging
- Security event logging

### Metrics
- Request/response times
- Error rates and types
- Authentication attempts
- API usage statistics

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test src/NotifyXStudio.Api.Tests/
```

### Test Categories
- **Unit Tests**: Business logic and service tests
- **Integration Tests**: API endpoint tests
- **Performance Tests**: Load and stress testing
- **Security Tests**: Authentication and authorization tests

## 🚢 Deployment

### Docker
```dockerfile
# Build image
docker build -t notifyx-studio .

# Run container
docker run -p 8080:8080 notifyx-studio
```

### Kubernetes
```bash
# Apply manifests
kubectl apply -f k8s/

# Check deployment
kubectl get pods -l app=notifyx-studio
```

### Production Checklist
- [ ] Configure production connection strings
- [ ] Set up SSL certificates
- [ ] Configure rate limiting
- [ ] Set up monitoring and alerting
- [ ] Configure backup strategies
- [ ] Review security settings
- [ ] Load test the application

## 🔄 Development Workflow

### Prerequisites
- Visual Studio 2022 or VS Code
- .NET 8 SDK
- Git

### Development Setup
```bash
# Install dependencies
dotnet restore

# Build solution
dotnet build

# Run in development mode
dotnet run --project src/NotifyXStudio.Api --environment Development
```

### Code Standards
- Follow Microsoft C# coding conventions
- Use XML documentation for public APIs
- Implement comprehensive error handling
- Write unit tests for new features
- Follow SOLID principles

## 📚 Additional Resources

- [API Documentation](https://localhost:7152/api/docs)
- [Health Dashboard](https://localhost:7152/health-ui)
- [Development Guide](docs/development.md)
- [Deployment Guide](docs/deployment.md)
- [Security Guide](docs/security.md)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: Check the [API docs](https://localhost:7152/api/docs)
- **Issues**: Report bugs via GitHub Issues
- **Email**: support@notifyx.studio
- **Health Status**: Monitor at `/health-ui`

---

**NotifyX Studio** - Empowering teams with intelligent project management and workflow automation.