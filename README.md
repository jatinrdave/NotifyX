# NotifyX Studio - Visual Workflow Builder

A powerful, n8n-style visual integration studio that uses NotifyX connectors as first-class nodes, allowing users to wire together 3rd-party connectors (Slack, Jira, HTTP, DB, etc.) into visual workflows.

## 🚀 Features

### Core Functionality
- **Visual Workflow Builder**: Drag-and-drop canvas with pan/zoom, node connections, and property panels
- **NotifyX Connectors**: First-class support for NotifyX notification nodes (Send Notification, Delivery Status, etc.)
- **Third-Party Integrations**: Support for HTTP, Slack, Jira, Database, and other popular connectors
- **Real-time Execution**: Live workflow runs with step-by-step execution traces
- **Dependency Resolution**: Smart connector dependency management with semver versioning
- **Multi-tenant Architecture**: Isolated data, credentials, and workflows per tenant

### Advanced Features
- **Expression Engine**: JavaScript-like expressions and template variables (`{{variable}}`)
- **Retry & Error Handling**: Configurable retry policies, timeouts, and dead letter queues
- **Workflow Versioning**: Snapshot workflows, rollback capabilities, and change tracking
- **Team Collaboration**: Share workflows, assign owners, and manage permissions
- **Export/Import**: JSON-based workflow portability across environments
- **Monitoring & Analytics**: Comprehensive metrics, logs, and performance insights

## 🏗️ Architecture

### High-Level Overview
```
[Angular 18 Frontend] ←→ [.NET 9 Web API] ←→ [Workflow Runtime Workers]
                              ↓
                    [PostgreSQL + Kafka + Redis]
```

### Technology Stack
- **Frontend**: Angular 18, NgRx, Angular Material, rete.js for canvas
- **Backend**: .NET 9, ASP.NET Core Web API, SignalR for real-time updates
- **Runtime**: Background workers with Kafka message queuing
- **Database**: PostgreSQL for persistence, Redis for caching
- **Message Queue**: Apache Kafka for workflow execution
- **Secrets**: Azure Key Vault / AWS KMS / HashiCorp Vault
- **Monitoring**: OpenTelemetry, Prometheus, Grafana

## 📁 Project Structure

```
NotifyXStudio/
├── src/
│   ├── NotifyXStudio.Api/           # Web API controllers and endpoints
│   ├── NotifyXStudio.Core/          # Domain models, interfaces, and business logic
│   ├── NotifyXStudio.Connectors/    # Built-in connector adapters
│   ├── NotifyXStudio.Runtime/       # Workflow execution workers
│   ├── NotifyXStudio.Persistence/   # EF Core data access layer
│   └── NotifyXStudio.SDK/           # Client SDK for external integrations
├── frontend/                        # Angular 18 application
│   ├── src/app/
│   │   ├── components/              # UI components (canvas, palette, etc.)
│   │   ├── services/                # Angular services
│   │   └── models/                  # TypeScript models
├── manifests/                       # Connector manifest definitions
├── registry/                        # Connector registry and metadata
└── tests/                          # Integration and unit tests
```

## 🔌 Connector System

### Connector Manifest Schema
Each connector is defined by a JSON manifest that describes:
- **Inputs/Outputs**: Parameter definitions with types and validation
- **Authentication**: OAuth2, API Key, or JWT configuration
- **UI Configuration**: Colors, icons, and grouping for the visual editor
- **Dependencies**: Runtime and peer dependencies with version constraints
- **Conflict Rules**: Resolution strategies and compatibility rules

### Sample Connector Manifest
```json
{
  "id": "notifyx.sendNotification",
  "name": "Send Notification",
  "type": "action",
  "category": "Notification",
  "inputs": [
    {
      "name": "channel",
      "type": "string",
      "required": true,
      "validation": { "enum": ["email", "sms", "slack", "push"] }
    },
    {
      "name": "recipient",
      "type": "string",
      "required": true
    }
  ],
  "outputs": [
    {
      "name": "notificationId",
      "type": "string"
    }
  ],
  "auth": {
    "type": "apiKey",
    "fields": { "apiKey": "NOTIFYX_API_KEY" }
  }
}
```

### Dependency Resolution
The system includes a sophisticated dependency resolver that:
- Supports semantic versioning (semver) constraints
- Handles runtime (NuGet/npm) and peer dependencies
- Provides conflict resolution strategies (highestCompatible, preferStable, failFast)
- Generates lockfiles for reproducible builds
- Offers detailed diagnostics for resolution failures

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js 18+
- PostgreSQL 14+
- Apache Kafka
- Redis

### Backend Setup
```bash
# Clone the repository
git clone https://github.com/your-org/notifyx-studio.git
cd notifyx-studio

# Restore dependencies
dotnet restore

# Update database
dotnet ef database update --project src/NotifyXStudio.Persistence

# Run the API
dotnet run --project src/NotifyXStudio.Api
```

### Frontend Setup
```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm start
```

### Docker Setup
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f
```

## 📖 API Documentation

### Core Endpoints

#### Workflows
- `POST /api/workflows` - Create workflow
- `GET /api/workflows/{id}` - Get workflow
- `PUT /api/workflows/{id}` - Update workflow
- `DELETE /api/workflows/{id}` - Delete workflow
- `POST /api/workflows/{id}/runs` - Start workflow run

#### Connectors
- `GET /api/connectors` - List available connectors
- `GET /api/connectors/{id}` - Get connector manifest
- `POST /api/connectors/resolve` - Resolve dependencies
- `POST /api/connectors/{id}/test` - Test connector

#### Runs
- `GET /api/runs/{runId}` - Get run status
- `GET /api/runs/{runId}/logs` - Get execution logs
- `POST /api/runs/{runId}/replay` - Replay failed run

## 🎨 Frontend Components

### Key Components
- **FlowCanvasComponent**: Main workflow editor with drag-and-drop
- **NodePaletteComponent**: Connector library with search and filtering
- **NodeConfigComponent**: Property panel for node configuration
- **RunInspectorComponent**: Real-time execution monitoring
- **CredentialsManagerComponent**: Secure credential management

### State Management
- **NgRx Store**: Centralized state management for workflows and UI
- **SignalR**: Real-time updates for workflow execution
- **RxJS**: Reactive programming for data flow

## 🔧 Development

### Adding New Connectors
1. Create connector manifest JSON in `manifests/`
2. Implement `IConnectorAdapter` in `NotifyXStudio.Connectors`
3. Register adapter in dependency injection
4. Update connector registry

### Testing
```bash
# Run unit tests
dotnet test

# Run integration tests
dotnet test tests/NotifyXStudio.IntegrationTests

# Run frontend tests
cd frontend && npm test

# Run E2E tests
cd frontend && npm run e2e
```

### Code Quality
- **SOLID Principles**: Clean architecture with separation of concerns
- **Design Patterns**: Builder, Strategy, Factory, Observer patterns
- **Async/Await**: Consistent async programming throughout
- **XML Documentation**: Comprehensive API documentation
- **Unit Testing**: High test coverage with xUnit and Moq

## 🚀 Deployment

### Kubernetes
```bash
# Deploy to Kubernetes
kubectl apply -f k8s/

# Scale workers
kubectl scale deployment notifyx-studio-workers --replicas=5
```

### Environment Variables
```bash
# Database
CONNECTION_STRINGS__DEFAULT="Host=localhost;Database=notifyx_studio;Username=postgres;Password=password"

# Kafka
KAFKA__BOOTSTRAP_SERVERS="localhost:9092"

# Redis
REDIS__CONNECTION_STRING="localhost:6379"

# Secrets
SECRETS__VAULT_URL="https://your-vault.vault.azure.net/"
```

## 📊 Monitoring & Observability

### Metrics
- Workflow execution rates and success/failure ratios
- Node execution times and resource usage
- Queue depths and processing times
- API response times and error rates

### Logging
- Structured logging with correlation IDs
- Node-level execution traces
- Error tracking with stack traces
- Audit logs for workflow changes

### Dashboards
- Grafana dashboards for operational metrics
- Real-time workflow execution monitoring
- Performance analytics and trends
- Alerting for system health

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow SOLID principles and clean code practices
- Write comprehensive unit tests
- Update documentation for new features
- Use conventional commit messages
- Ensure all tests pass before submitting PR

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: [docs.notifyx.dev](https://docs.notifyx.dev)
- **Issues**: [GitHub Issues](https://github.com/your-org/notifyx-studio/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-org/notifyx-studio/discussions)
- **Email**: support@notifyx.dev

## 🗺️ Roadmap

### Phase 1 (Current)
- ✅ Core workflow builder
- ✅ NotifyX connector integration
- ✅ Basic third-party connectors
- ✅ Real-time execution monitoring

### Phase 2 (Q2 2025)
- 🔄 Advanced expression engine
- 🔄 Workflow templates and marketplace
- 🔄 Enhanced error handling and retry policies
- 🔄 Team collaboration features

### Phase 3 (Q3 2025)
- 📋 Custom connector SDK
- 📋 Advanced analytics and reporting
- 📋 Workflow optimization suggestions
- 📋 Enterprise security features

---

**Built with ❤️ by the NotifyX Team**