# NotifyX Studio - Comprehensive Developer Guide

## Table of Contents

1. [Getting Started](#getting-started)
2. [Architecture Overview](#architecture-overview)
3. [API Reference](#api-reference)
4. [Connector Library](#connector-library)
5. [Workflow Builder](#workflow-builder)
6. [Authentication & Security](#authentication--security)
7. [Deployment Guide](#deployment-guide)
8. [Troubleshooting](#troubleshooting)
9. [Best Practices](#best-practices)

---

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Node.js 18+ and npm
- Docker and Docker Compose
- PostgreSQL 15+
- Apache Kafka
- Redis 7+

### Quick Setup

1. **Clone the repository**
```bash
git clone https://github.com/your-org/notifyx-studio.git
cd notifyx-studio
```

2. **Start the development environment**
```bash
chmod +x scripts/setup.sh
./scripts/setup.sh
```

3. **Access the application**
- Frontend: http://localhost:4200
- API: http://localhost:5000
- Grafana: http://localhost:3000
- Jaeger: http://localhost:16686

### First Workflow

1. Open the NotifyX Studio frontend
2. Click "Create New Workflow"
3. Drag a "Webhook Trigger" node to the canvas
4. Add an "HTTP Request" node
5. Connect the nodes
6. Configure the HTTP request URL
7. Save and test the workflow

---

## Architecture Overview

### System Components

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Angular 18    │    │   .NET 9 API    │    │  Runtime Worker │
│   Frontend      │◄──►│   Backend       │◄──►│   (Kafka)       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   SignalR Hub   │    │   PostgreSQL    │    │   Redis Cache   │
│   Real-time     │    │   Database      │    │   & Sessions    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Data Flow

1. **User creates workflow** → Frontend → API → Database
2. **Workflow execution** → API → Kafka → Runtime Worker
3. **Real-time updates** → Runtime Worker → SignalR → Frontend
4. **Monitoring** → All services → Prometheus → Grafana

---

## API Reference

### Base URL
```
Production: https://api.notifyxstudio.com
Development: http://localhost:5000
```

### Authentication

All API requests require authentication via JWT token:

```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     https://api.notifyxstudio.com/api/workflows
```

### Core Endpoints

#### Workflows

**Create Workflow**
```http
POST /api/workflows
Content-Type: application/json

{
  "name": "Customer Onboarding",
  "description": "Automated customer onboarding process",
  "nodes": [
    {
      "id": "trigger-1",
      "type": "webhook.trigger",
      "position": { "x": 100, "y": 100 },
      "config": {
        "path": "/onboard",
        "method": "POST"
      }
    }
  ],
  "edges": [],
  "triggers": []
}
```

**Get Workflow**
```http
GET /api/workflows/{workflowId}
```

**Update Workflow**
```http
PUT /api/workflows/{workflowId}
Content-Type: application/json

{
  "name": "Updated Workflow Name",
  "nodes": [...],
  "edges": [...]
}
```

**Delete Workflow**
```http
DELETE /api/workflows/{workflowId}
```

#### Workflow Runs

**Start Workflow Run**
```http
POST /api/workflows/{workflowId}/runs
Content-Type: application/json

{
  "payload": {
    "customerId": "12345",
    "email": "customer@example.com"
  },
  "mode": "manual"
}
```

**Get Run Status**
```http
GET /api/runs/{runId}
```

**Get Run Logs**
```http
GET /api/runs/{runId}/logs
```

#### Connectors

**List Available Connectors**
```http
GET /api/connectors/registry
```

**Get Connector Manifest**
```http
GET /api/connectors/{connectorType}/manifest
```

#### Credentials

**Create Credential**
```http
POST /api/credentials
Content-Type: application/json

{
  "name": "Slack API Key",
  "type": "slack.sendMessage",
  "config": {
    "token": "xoxb-your-slack-token"
  }
}
```

**List Credentials**
```http
GET /api/credentials
```

---

## Connector Library

### Core Connectors

#### 1. Webhook Trigger

**Purpose**: Trigger workflows via HTTP webhooks

**Configuration**:
```json
{
  "path": "/webhook/customer-signup",
  "method": "POST",
  "headers": {
    "X-API-Key": "{{ $env.API_KEY }}"
  },
  "response": {
    "statusCode": 200,
    "body": "{\"status\": \"received\"}"
  }
}
```

**Example Usage**:
```javascript
// Trigger workflow from external service
fetch('https://api.notifyxstudio.com/webhook/customer-signup', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'X-API-Key': 'your-api-key'
  },
  body: JSON.stringify({
    customerId: '12345',
    email: 'customer@example.com',
    plan: 'premium'
  })
});
```

#### 2. HTTP Request

**Purpose**: Make HTTP requests to external APIs

**Configuration**:
```json
{
  "method": "POST",
  "url": "https://api.stripe.com/v1/customers",
  "headers": {
    "Authorization": "Bearer {{ $credentials.stripe.secretKey }}",
    "Content-Type": "application/json"
  },
  "body": {
    "email": "{{ $json.email }}",
    "name": "{{ $json.customerName }}"
  },
  "timeout": 30000,
  "retry": {
    "attempts": 3,
    "delay": 1000
  }
}
```

**Example Usage**:
```javascript
// Create customer in Stripe
{
  "method": "POST",
  "url": "https://api.stripe.com/v1/customers",
  "headers": {
    "Authorization": "Bearer sk_test_...",
    "Content-Type": "application/json"
  },
  "body": {
    "email": "{{ $json.email }}",
    "name": "{{ $json.customerName }}",
    "metadata": {
      "source": "notifyx-studio"
    }
  }
}
```

#### 3. NotifyX Send Notification

**Purpose**: Send notifications through NotifyX platform

**Configuration**:
```json
{
  "channel": "email",
  "priority": "high",
  "template": "welcome-email",
  "recipient": "{{ $json.email }}",
  "data": {
    "customerName": "{{ $json.customerName }}",
    "plan": "{{ $json.plan }}"
  },
  "scheduling": {
    "delay": "5m",
    "timezone": "UTC"
  }
}
```

**Example Usage**:
```javascript
// Send welcome email
{
  "channel": "email",
  "priority": "normal",
  "template": "customer-welcome",
  "recipient": "{{ $json.email }}",
  "data": {
    "customerName": "{{ $json.customerName }}",
    "loginUrl": "https://app.example.com/login",
    "supportEmail": "support@example.com"
  }
}
```

#### 4. Slack Send Message

**Purpose**: Send messages to Slack channels or users

**Configuration**:
```json
{
  "channel": "#customer-support",
  "text": "New customer signup: {{ $json.customerName }}",
  "blocks": [
    {
      "type": "section",
      "text": {
        "type": "mrkdwn",
        "text": "*New Customer Signup*\\n*Name:* {{ $json.customerName }}\\n*Email:* {{ $json.email }}\\n*Plan:* {{ $json.plan }}"
      }
    }
  ],
  "attachments": [
    {
      "color": "good",
      "fields": [
        {
          "title": "Customer ID",
          "value": "{{ $json.customerId }}",
          "short": true
        }
      ]
    }
  ]
}
```

**Example Usage**:
```javascript
// Notify team of new customer
{
  "channel": "#sales",
  "text": "🎉 New Premium Customer!",
  "blocks": [
    {
      "type": "header",
      "text": {
        "type": "plain_text",
        "text": "New Premium Customer"
      }
    },
    {
      "type": "section",
      "fields": [
        {
          "type": "mrkdwn",
          "text": "*Name:*\\n{{ $json.customerName }}"
        },
        {
          "type": "mrkdwn",
          "text": "*Email:*\\n{{ $json.email }}"
        },
        {
          "type": "mrkdwn",
          "text": "*Plan:*\\n{{ $json.plan }}"
        },
        {
          "type": "mrkdwn",
          "text": "*MRR:*\\n${{ $json.monthlyRevenue }}"
        }
      ]
    }
  ]
}
```

#### 5. Gmail Send Email

**Purpose**: Send emails via Gmail API

**Configuration**:
```json
{
  "to": "{{ $json.email }}",
  "subject": "Welcome to {{ $json.companyName }}!",
  "htmlBody": "<h1>Welcome {{ $json.customerName }}!</h1><p>Thank you for signing up...</p>",
  "textBody": "Welcome {{ $json.customerName }}! Thank you for signing up...",
  "attachments": [
    {
      "filename": "welcome-guide.pdf",
      "content": "{{ $binary.welcomeGuide }}",
      "type": "application/pdf"
    }
  ]
}
```

**Example Usage**:
```javascript
// Send onboarding email
{
  "to": "{{ $json.email }}",
  "subject": "Welcome to {{ $json.companyName }} - Let's get started!",
  "htmlBody": `
    <div style="font-family: Arial, sans-serif; max-width: 600px;">
      <h1 style="color: #333;">Welcome {{ $json.customerName }}!</h1>
      <p>Thank you for choosing {{ $json.companyName }}. Here's what's next:</p>
      <ul>
        <li>Complete your profile setup</li>
        <li>Connect your first integration</li>
        <li>Schedule a demo with our team</li>
      </ul>
      <a href="{{ $json.onboardingUrl }}" style="background: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Get Started</a>
    </div>
  `,
  "textBody": "Welcome {{ $json.customerName }}! Thank you for choosing {{ $json.companyName }}. Visit {{ $json.onboardingUrl }} to get started."
}
```

#### 6. MySQL Query

**Purpose**: Execute SQL queries on MySQL databases

**Configuration**:
```json
{
  "query": "INSERT INTO customers (id, name, email, plan, created_at) VALUES (?, ?, ?, ?, NOW())",
  "parameters": [
    "{{ $json.customerId }}",
    "{{ $json.customerName }}",
    "{{ $json.email }}",
    "{{ $json.plan }}"
  ],
  "operation": "insert",
  "returnData": true
}
```

**Example Usage**:
```javascript
// Insert customer record
{
  "query": "INSERT INTO customers (id, name, email, plan, status, created_at) VALUES (?, ?, ?, ?, 'active', NOW())",
  "parameters": [
    "{{ $json.customerId }}",
    "{{ $json.customerName }}",
    "{{ $json.email }}",
    "{{ $json.plan }}"
  ],
  "operation": "insert",
  "returnData": true
}

// Query customer data
{
  "query": "SELECT * FROM customers WHERE email = ? AND status = 'active'",
  "parameters": ["{{ $json.email }}"],
  "operation": "select",
  "returnData": true
}
```

#### 7. Schedule Trigger

**Purpose**: Trigger workflows on a schedule

**Configuration**:
```json
{
  "cron": "0 9 * * MON",
  "timezone": "America/New_York",
  "description": "Weekly Monday morning report"
}
```

**Example Usage**:
```javascript
// Daily at 9 AM EST
{
  "cron": "0 9 * * *",
  "timezone": "America/New_York",
  "description": "Daily morning sync"
}

// Every 15 minutes
{
  "cron": "*/15 * * * *",
  "timezone": "UTC",
  "description": "Quarterly data sync"
}

// First Monday of every month
{
  "cron": "0 0 1-7 * MON",
  "timezone": "UTC",
  "description": "Monthly report"
}
```

### Logic Connectors

#### 8. If Condition

**Purpose**: Conditional branching based on data

**Configuration**:
```json
{
  "condition": "{{ $json.plan }} === 'premium'",
  "trueOutput": "premium-path",
  "falseOutput": "standard-path"
}
```

**Example Usage**:
```javascript
// Route based on customer plan
{
  "condition": "{{ $json.plan }} === 'premium' && {{ $json.monthlyRevenue }} > 1000",
  "trueOutput": "vip-onboarding",
  "falseOutput": "standard-onboarding"
}

// Check email domain
{
  "condition": "{{ $json.email }}.endsWith('@enterprise.com')",
  "trueOutput": "enterprise-flow",
  "falseOutput": "regular-flow"
}
```

#### 9. Switch Condition

**Purpose**: Multi-way branching based on data

**Configuration**:
```json
{
  "expression": "{{ $json.region }}",
  "cases": [
    {
      "value": "US",
      "output": "us-processing"
    },
    {
      "value": "EU",
      "output": "eu-processing"
    },
    {
      "value": "APAC",
      "output": "apac-processing"
    }
  ],
  "default": "global-processing"
}
```

**Example Usage**:
```javascript
// Route by region
{
  "expression": "{{ $json.region }}",
  "cases": [
    {
      "value": "US",
      "output": "us-compliance"
    },
    {
      "value": "EU",
      "output": "gdpr-compliance"
    },
    {
      "value": "APAC",
      "output": "local-compliance"
    }
  ],
  "default": "international-compliance"
}
```

#### 10. Merge Data

**Purpose**: Combine data from multiple sources

**Configuration**:
```json
{
  "mode": "merge",
  "sources": [
    "{{ $json.customerData }}",
    "{{ $json.stripeData }}",
    "{{ $json.analyticsData }}"
  ],
  "conflictResolution": "last-wins"
}
```

**Example Usage**:
```javascript
// Merge customer data
{
  "mode": "merge",
  "sources": [
    "{{ $json.webhookData }}",
    "{{ $json.databaseData }}",
    "{{ $json.externalApiData }}"
  ],
  "conflictResolution": "priority",
  "priority": ["externalApiData", "databaseData", "webhookData"]
}
```

#### 11. Set Data

**Purpose**: Set or transform data values

**Configuration**:
```json
{
  "operations": [
    {
      "field": "fullName",
      "value": "{{ $json.firstName }} {{ $json.lastName }}"
    },
    {
      "field": "signupDate",
      "value": "{{ $now }}"
    },
    {
      "field": "customerTier",
      "value": "{{ $json.monthlyRevenue > 1000 ? 'premium' : 'standard' }}"
    }
  ]
}
```

**Example Usage**:
```javascript
// Transform and enrich data
{
  "operations": [
    {
      "field": "customerId",
      "value": "CUST-{{ $json.id }}"
    },
    {
      "field": "onboardingUrl",
      "value": "https://app.example.com/onboard/{{ $json.id }}"
    },
    {
      "field": "welcomeMessage",
      "value": "Welcome {{ $json.firstName }}, you're all set up!"
    },
    {
      "field": "nextSteps",
      "value": "{{ $json.plan === 'premium' ? ['Setup integrations', 'Schedule demo'] : ['Complete profile'] }}"
    }
  ]
}
```

### Advanced Connectors

#### 12. Wait/Delay

**Purpose**: Add delays in workflow execution

**Configuration**:
```json
{
  "duration": "5m",
  "reason": "Waiting for external system to process"
}
```

**Example Usage**:
```javascript
// Wait for payment processing
{
  "duration": "2m",
  "reason": "Allow payment to settle"
}

// Wait until specific time
{
  "until": "2024-01-01T09:00:00Z",
  "timezone": "UTC",
  "reason": "Wait for business hours"
}
```

#### 13. Loop/Iterator

**Purpose**: Iterate over arrays or repeat actions

**Configuration**:
```json
{
  "items": "{{ $json.customers }}",
  "itemVariable": "customer",
  "maxIterations": 100,
  "continueOnError": true
}
```

**Example Usage**:
```javascript
// Process multiple customers
{
  "items": "{{ $json.customers }}",
  "itemVariable": "customer",
  "maxIterations": 1000,
  "continueOnError": true,
  "batchSize": 10
}
```

#### 14. Error Handler

**Purpose**: Handle and recover from errors

**Configuration**:
```json
{
  "retryPolicy": {
    "maxAttempts": 3,
    "delay": "1s",
    "backoffMultiplier": 2
  },
  "fallbackAction": "send-alert",
  "errorTypes": ["timeout", "network", "validation"]
}
```

**Example Usage**:
```javascript
// Retry with exponential backoff
{
  "retryPolicy": {
    "maxAttempts": 5,
    "delay": "1s",
    "backoffMultiplier": 2,
    "maxDelay": "30s"
  },
  "fallbackAction": "dead-letter-queue",
  "errorTypes": ["timeout", "rate-limit", "server-error"]
}
```

---

## Workflow Builder

### Visual Interface

The NotifyX Studio workflow builder provides an intuitive drag-and-drop interface for creating complex workflows:

#### Canvas Features
- **Zoom & Pan**: Navigate large workflows
- **Grid Snap**: Align nodes precisely
- **Minimap**: Overview of complex workflows
- **Search**: Find nodes quickly

#### Node Palette
- **Categories**: Triggers, Actions, Logic, Data
- **Search**: Find connectors by name or function
- **Favorites**: Mark frequently used connectors
- **Recent**: Quick access to recently used nodes

#### Property Panel
- **Dynamic Forms**: Auto-generated based on connector manifest
- **Validation**: Real-time input validation
- **Expression Editor**: Monaco editor for complex expressions
- **Preview**: Test expressions with sample data

### Workflow Patterns

#### 1. Linear Processing
```
Webhook → HTTP Request → NotifyX → Slack
```

#### 2. Conditional Branching
```
Webhook → If Condition → [Premium Path | Standard Path]
```

#### 3. Parallel Processing
```
Webhook → Split → [Email | SMS | Push] → Merge
```

#### 4. Error Handling
```
HTTP Request → Error Handler → [Retry | Fallback | Alert]
```

#### 5. Scheduled Workflows
```
Schedule → Database Query → Data Processing → Report
```

### Expression Language

NotifyX Studio uses a powerful expression language for data manipulation:

#### Basic Syntax
```javascript
// Access input data
{{ $json.customerName }}
{{ $json.items[0].price }}

// Environment variables
{{ $env.API_KEY }}
{{ $env.DATABASE_URL }}

// Credentials
{{ $credentials.slack.token }}

// Functions
{{ $now }}
{{ $uuid() }}
{{ $json.email.toUpperCase() }}
```

#### Advanced Examples
```javascript
// Conditional logic
{{ $json.plan === 'premium' ? 'VIP' : 'Standard' }}

// String manipulation
{{ $json.firstName + ' ' + $json.lastName }}

// Date formatting
{{ $now.format('YYYY-MM-DD HH:mm:ss') }}

// Array operations
{{ $json.items.map(item => item.price * item.quantity).reduce((a, b) => a + b, 0) }}

// Object manipulation
{{ { ...$json, processedAt: $now, id: $uuid() } }}
```

---

## Authentication & Security

### API Authentication

#### JWT Token Structure
```json
{
  "sub": "user-12345",
  "tenant": "tenant-67890",
  "roles": ["developer", "operator"],
  "permissions": ["workflow:read", "workflow:write"],
  "exp": 1640995200,
  "iat": 1640908800
}
```

#### Token Refresh
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "your-refresh-token"
}
```

### Role-Based Access Control

#### Roles
- **Developer**: Create and modify workflows
- **Operator**: Execute and monitor workflows
- **Viewer**: Read-only access to workflows and logs
- **Admin**: Full system access

#### Permissions
```json
{
  "workflow:read": "View workflows and runs",
  "workflow:write": "Create and modify workflows",
  "workflow:execute": "Start workflow runs",
  "workflow:delete": "Delete workflows",
  "credential:read": "View credentials",
  "credential:write": "Create and modify credentials",
  "admin:users": "Manage users and roles",
  "admin:system": "System administration"
}
```

### Credential Management

#### Secure Storage
- All credentials encrypted with AES-256
- Stored in HashiCorp Vault or Azure Key Vault
- Access controlled by RBAC
- Audit logging for all access

#### Credential Types
```json
{
  "slack": {
    "token": "xoxb-...",
    "teamId": "T1234567890"
  },
  "stripe": {
    "secretKey": "sk_test_...",
    "publishableKey": "pk_test_..."
  },
  "gmail": {
    "clientId": "123456789.apps.googleusercontent.com",
    "clientSecret": "GOCSPX-...",
    "refreshToken": "1//..."
  }
}
```

---

## Deployment Guide

### Development Environment

#### Local Setup
```bash
# Clone repository
git clone https://github.com/your-org/notifyx-studio.git
cd notifyx-studio

# Start services
docker-compose up -d

# Run migrations
dotnet ef database update --project src/NotifyXStudio.Persistence

# Start development servers
cd src/NotifyXStudio.Api && dotnet run
cd frontend && npm start
```

#### Environment Variables
```bash
# Database
DATABASE_URL=postgresql://user:pass@localhost:5432/notifyxstudio

# Redis
REDIS_URL=redis://localhost:6379

# Kafka
KAFKA_BOOTSTRAP_SERVERS=localhost:9092

# Security
JWT_SECRET=your-super-secret-jwt-key
ENCRYPTION_KEY=your-32-byte-encryption-key

# External Services
NOTIFYX_API_URL=https://api.notifyx.com
NOTIFYX_API_KEY=your-notifyx-api-key
```

### Production Deployment

#### Docker Compose Production
```yaml
version: '3.8'
services:
  api:
    image: notifyxstudio/api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DATABASE_URL=${DATABASE_URL}
      - REDIS_URL=${REDIS_URL}
      - KAFKA_BOOTSTRAP_SERVERS=${KAFKA_BOOTSTRAP_SERVERS}
    deploy:
      replicas: 3
      resources:
        limits:
          memory: 1G
          cpus: '0.5'

  runtime-worker:
    image: notifyxstudio/runtime:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DATABASE_URL=${DATABASE_URL}
      - KAFKA_BOOTSTRAP_SERVERS=${KAFKA_BOOTSTRAP_SERVERS}
    deploy:
      replicas: 5
      resources:
        limits:
          memory: 2G
          cpus: '1.0'

  frontend:
    image: notifyxstudio/frontend:latest
    deploy:
      replicas: 2
      resources:
        limits:
          memory: 512M
          cpus: '0.25'
```

#### Kubernetes Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: notifyxstudio-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: notifyxstudio-api
  template:
    metadata:
      labels:
        app: notifyxstudio-api
    spec:
      containers:
      - name: api
        image: notifyxstudio/api:latest
        ports:
        - containerPort: 80
        env:
        - name: DATABASE_URL
          valueFrom:
            secretKeyRef:
              name: notifyxstudio-secrets
              key: database-url
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "1Gi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
```

### Monitoring Setup

#### Prometheus Configuration
```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'notifyxstudio-api'
    static_configs:
      - targets: ['api:80']
    metrics_path: /metrics
    scrape_interval: 5s

  - job_name: 'notifyxstudio-runtime'
    static_configs:
      - targets: ['runtime-worker:80']
    metrics_path: /metrics
    scrape_interval: 5s
```

#### Grafana Dashboard
```json
{
  "dashboard": {
    "title": "NotifyX Studio Overview",
    "panels": [
      {
        "title": "Workflow Executions",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(workflow_executions_total[5m])",
            "legendFormat": "Executions/sec"
          }
        ]
      },
      {
        "title": "Error Rate",
        "type": "singlestat",
        "targets": [
          {
            "expr": "rate(workflow_errors_total[5m]) / rate(workflow_executions_total[5m]) * 100",
            "legendFormat": "Error Rate %"
          }
        ]
      }
    ]
  }
}
```

---

## Troubleshooting

### Common Issues

#### 1. Workflow Not Executing

**Symptoms**: Workflow shows as "queued" but never starts

**Diagnosis**:
```bash
# Check Kafka connectivity
docker exec -it kafka kafka-topics --list --bootstrap-server localhost:9092

# Check runtime worker logs
docker logs notifyxstudio-runtime-worker

# Check workflow status
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/runs/{runId}
```

**Solutions**:
- Verify Kafka is running and accessible
- Check runtime worker is consuming messages
- Verify workflow configuration is valid
- Check for credential issues

#### 2. Connector Execution Failures

**Symptoms**: Individual nodes fail with errors

**Diagnosis**:
```bash
# Check node execution logs
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/runs/{runId}/logs

# Test connector independently
curl -X POST http://localhost:5000/api/connectors/test \
     -H "Content-Type: application/json" \
     -d '{"type": "http.request", "config": {...}}'
```

**Solutions**:
- Verify connector configuration
- Check credential validity
- Test external API connectivity
- Review error messages in logs

#### 3. Performance Issues

**Symptoms**: Slow workflow execution or timeouts

**Diagnosis**:
```bash
# Check system resources
docker stats

# Check database performance
docker exec -it postgres psql -U user -d notifyxstudio -c "
SELECT query, mean_time, calls 
FROM pg_stat_statements 
ORDER BY mean_time DESC 
LIMIT 10;"

# Check Kafka lag
docker exec -it kafka kafka-consumer-groups --bootstrap-server localhost:9092 \
  --group notifyxstudio-runtime --describe
```

**Solutions**:
- Scale runtime workers
- Optimize database queries
- Increase Kafka partitions
- Review workflow complexity

### Debug Mode

#### Enable Debug Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "NotifyXStudio": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### Debug Workflow Execution
```javascript
// Add debug nodes to workflow
{
  "type": "set.data",
  "config": {
    "operations": [
      {
        "field": "debug",
        "value": "{{ $json | json }}"
      }
    ]
  }
}
```

### Health Checks

#### API Health
```http
GET /health
```

Response:
```json
{
  "status": "healthy",
  "checks": {
    "database": "healthy",
    "kafka": "healthy",
    "redis": "healthy"
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Runtime Worker Health
```http
GET /health
```

Response:
```json
{
  "status": "healthy",
  "workerId": "worker-12345",
  "processedMessages": 1500,
  "lastMessageTime": "2024-01-01T12:00:00Z"
}
```

---

## Best Practices

### Workflow Design

#### 1. Keep Workflows Focused
- One workflow per business process
- Clear, descriptive names
- Document complex logic

#### 2. Error Handling
- Always include error handling nodes
- Use retry policies for transient failures
- Implement fallback actions

#### 3. Data Validation
- Validate input data early
- Use type checking in expressions
- Handle missing or invalid data gracefully

#### 4. Performance Optimization
- Minimize external API calls
- Use parallel processing where possible
- Cache frequently accessed data

### Security Best Practices

#### 1. Credential Management
- Use least-privilege access
- Rotate credentials regularly
- Never log sensitive data

#### 2. Input Validation
- Sanitize all user inputs
- Validate data types and formats
- Implement rate limiting

#### 3. Network Security
- Use HTTPS for all communications
- Implement proper CORS policies
- Use VPN for internal communications

### Monitoring and Observability

#### 1. Comprehensive Logging
- Log all workflow executions
- Include correlation IDs
- Use structured logging

#### 2. Metrics Collection
- Track execution times
- Monitor error rates
- Measure throughput

#### 3. Alerting
- Set up alerts for failures
- Monitor system health
- Track business metrics

### Development Workflow

#### 1. Version Control
- Use Git for all code
- Tag releases properly
- Maintain changelog

#### 2. Testing
- Write unit tests for connectors
- Test workflows with sample data
- Use integration tests

#### 3. Documentation
- Document all workflows
- Maintain API documentation
- Keep examples updated

---

## Support and Resources

### Documentation
- [API Reference](https://docs.notifyxstudio.com/api)
- [Connector Library](https://docs.notifyxstudio.com/connectors)
- [Workflow Examples](https://docs.notifyxstudio.com/examples)

### Community
- [GitHub Repository](https://github.com/your-org/notifyx-studio)
- [Discord Community](https://discord.gg/notifyxstudio)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/notifyx-studio)

### Support
- [Email Support](mailto:support@notifyxstudio.com)
- [Enterprise Support](mailto:enterprise@notifyxstudio.com)
- [Status Page](https://status.notifyxstudio.com)

---
# Summary

✅ NotifyX Studio Applications Successfully Started!
I have successfully started both the frontend and backend applications for the NotifyX Studio project. Here's a comprehensive summary:
🚀 Infrastructure Services (Running)
PostgreSQL: Running on port 5432 (healthy)
Redis: Running on port 6379 (healthy)
Kafka + Zookeeper: Running and ready for message processing
�� Backend API (.NET)
Status: ✅ Started successfully
Location: src/NotifyXStudio.Api
Technology: .NET 9.0 with ASP.NET Core
Features:
REST API endpoints
SignalR for real-time communication
Swagger documentation (in development mode)
Health checks
CORS enabled for frontend communication
🎨 Frontend Application (Angular)
Status: ✅ Started successfully
Location: frontend/
Technology: Angular 18 with TypeScript
Features:
Visual workflow builder using Rete.js
Monaco editor for code editing
Material Design components
Real-time updates via SignalR
Modern responsive UI
🔧 Issues Resolved
Docker Compose: Fixed container naming conflicts and removed obsolete version field
Build Errors: Resolved missing dependencies and compilation issues in:
NotifyXStudio.Core (Semver package issues)
NotifyXStudio.Connectors (MySqlConnector, Jint packages)
Frontend Dependencies: Fixed npm package version conflicts
�� Access Points
Frontend: http://localhost:4200 (Angular development server)
Backend API: http://localhost:5000 (ASP.NET Core API)
Swagger UI: http://localhost:5000/swagger (API documentation)
Health Check: http://localhost:5000/health
📊 Current Status
Both applications are now running in the background and ready for development. The infrastructure services (PostgreSQL, Redis, Kafka) are healthy and available for the applications to use.
You can now:
Access the frontend at http://localhost:4200 to build workflows
Use the API at http://localhost:5000 for backend operations
View API documentation at http://localhost:5000/swagger
Monitor application health and logs
The NotifyX Studio is now fully operational! 🎉

*This guide is continuously updated. Last updated: September 2025*