# NotifyX Studio - API Reference

## Base URL
```
Production: https://api.notifyxstudio.com
Development: http://localhost:5000
```

## Authentication

All API requests require a JWT token in the Authorization header:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

## Core Endpoints

### Workflows

#### Create Workflow
```http
POST /api/workflows
Content-Type: application/json

{
  "name": "Customer Onboarding",
  "description": "Automated customer onboarding process",
  "nodes": [...],
  "edges": [...],
  "triggers": [...]
}
```

#### Get Workflow
```http
GET /api/workflows/{workflowId}
```

#### Update Workflow
```http
PUT /api/workflows/{workflowId}
Content-Type: application/json

{
  "name": "Updated Workflow Name",
  "nodes": [...],
  "edges": [...]
}
```

#### Delete Workflow
```http
DELETE /api/workflows/{workflowId}
```

#### List Workflows
```http
GET /api/workflows?page=1&pageSize=50&search=onboarding
```

### Workflow Runs

#### Start Run
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

#### Get Run Status
```http
GET /api/runs/{runId}
```

#### Get Run Logs
```http
GET /api/runs/{runId}/logs?page=1&pageSize=100
```

#### Cancel Run
```http
POST /api/runs/{runId}/cancel
```

### Connectors

#### List Connectors
```http
GET /api/connectors/registry
```

#### Get Connector Manifest
```http
GET /api/connectors/{connectorType}/manifest
```

#### Test Connector
```http
POST /api/connectors/test
Content-Type: application/json

{
  "type": "http.request",
  "config": {
    "method": "GET",
    "url": "https://httpbin.org/get"
  }
}
```

### Credentials

#### Create Credential
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

#### List Credentials
```http
GET /api/credentials?type=slack.sendMessage
```

#### Update Credential
```http
PUT /api/credentials/{credentialId}
Content-Type: application/json

{
  "name": "Updated Slack API Key",
  "config": {
    "token": "xoxb-new-slack-token"
  }
}
```

#### Delete Credential
```http
DELETE /api/credentials/{credentialId}
```

## Response Formats

### Success Response
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully"
}
```

### Error Response
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": {
      "field": "email",
      "reason": "Invalid email format"
    }
  }
}
```

### Pagination
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalCount": 150,
    "totalPages": 3
  }
}
```

## Status Codes

- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `409` - Conflict
- `422` - Validation Error
- `500` - Internal Server Error

## Rate Limits

- **API Calls**: 1000 requests per hour per user
- **Workflow Executions**: 100 executions per hour per tenant
- **Webhook Calls**: 10000 calls per hour per tenant

## Webhooks

### Incoming Webhooks

Trigger workflows via HTTP webhooks:

```http
POST /webhook/{webhookPath}
Content-Type: application/json
X-API-Key: YOUR_WEBHOOK_SECRET

{
  "data": "your payload"
}
```

### Outgoing Webhooks

Receive notifications about workflow events:

```http
POST YOUR_WEBHOOK_URL
Content-Type: application/json

{
  "event": "workflow.completed",
  "workflowId": "workflow-123",
  "runId": "run-456",
  "status": "completed",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## SDKs

### JavaScript/Node.js
```bash
npm install @notifyxstudio/sdk
```

```javascript
import { NotifyXStudio } from '@notifyxstudio/sdk';

const client = new NotifyXStudio({
  apiKey: 'your-api-key',
  baseUrl: 'https://api.notifyxstudio.com'
});

// Create workflow
const workflow = await client.workflows.create({
  name: 'My Workflow',
  nodes: [...],
  edges: [...]
});

// Start run
const run = await client.workflows.startRun(workflow.id, {
  payload: { message: 'Hello World' }
});
```

### Python
```bash
pip install notifyxstudio
```

```python
from notifyxstudio import NotifyXStudio

client = NotifyXStudio(
    api_key='your-api-key',
    base_url='https://api.notifyxstudio.com'
)

# Create workflow
workflow = client.workflows.create({
    'name': 'My Workflow',
    'nodes': [...],
    'edges': [...]
})

# Start run
run = client.workflows.start_run(workflow['id'], {
    'payload': {'message': 'Hello World'}
})
```

### C#
```bash
dotnet add package NotifyXStudio.SDK
```

```csharp
using NotifyXStudio.SDK;

var client = new NotifyXStudioClient("your-api-key", "https://api.notifyxstudio.com");

// Create workflow
var workflow = await client.Workflows.CreateAsync(new CreateWorkflowRequest
{
    Name = "My Workflow",
    Nodes = [...],
    Edges = [...]
});

// Start run
var run = await client.Workflows.StartRunAsync(workflow.Id, new StartRunRequest
{
    Payload = new { message = "Hello World" }
});
```

## Examples

### Complete Customer Onboarding Workflow

```javascript
// Create workflow
const workflow = await client.workflows.create({
  name: "Customer Onboarding",
  description: "Complete onboarding process for new customers",
  nodes: [
    {
      id: "webhook-trigger",
      type: "webhook.trigger",
      position: { x: 100, y: 100 },
      config: {
        path: "/webhook/signup",
        method: "POST"
      }
    },
    {
      id: "create-stripe-customer",
      type: "http.request",
      position: { x: 300, y: 100 },
      config: {
        method: "POST",
        url: "https://api.stripe.com/v1/customers",
        headers: {
          "Authorization": "Bearer {{ $credentials.stripe.secretKey }}",
          "Content-Type": "application/json"
        },
        body: {
          email: "{{ $json.email }}",
          name: "{{ $json.firstName }} {{ $json.lastName }}"
        }
      }
    },
    {
      id: "send-welcome-email",
      type: "notifyx.sendNotification",
      position: { x: 500, y: 100 },
      config: {
        channel: "email",
        template: "welcome",
        recipient: "{{ $json.email }}",
        data: {
          customerName: "{{ $json.firstName }}",
          loginUrl: "https://app.example.com/login"
        }
      }
    }
  ],
  edges: [
    { from: "webhook-trigger", to: "create-stripe-customer" },
    { from: "create-stripe-customer", to: "send-welcome-email" }
  ]
});

// Test the workflow
const run = await client.workflows.startRun(workflow.id, {
  payload: {
    email: "customer@example.com",
    firstName: "John",
    lastName: "Doe"
  }
});

console.log(`Workflow run started: ${run.id}`);
```

### Error Handling

```javascript
try {
  const workflow = await client.workflows.create(workflowData);
} catch (error) {
  if (error.status === 422) {
    console.error('Validation error:', error.details);
  } else if (error.status === 401) {
    console.error('Authentication failed');
  } else {
    console.error('Unexpected error:', error.message);
  }
}
```

## Support

- [Documentation](https://docs.notifyxstudio.com)
- [Community Discord](https://discord.gg/notifyxstudio)
- [Email Support](mailto:support@notifyxstudio.com)