# NotifyX Studio - Workflow Patterns

## Table of Contents

1. [Basic Patterns](#basic-patterns)
2. [Error Handling Patterns](#error-handling-patterns)
3. [Data Processing Patterns](#data-processing-patterns)
4. [Integration Patterns](#integration-patterns)
5. [Monitoring Patterns](#monitoring-patterns)
6. [Advanced Patterns](#advanced-patterns)

---

## Basic Patterns

### 1. Linear Processing

**Use Case**: Simple sequential workflow

```
Webhook → HTTP Request → Database → Email → Slack
```

**Example**: Customer signup flow
```json
{
  "name": "Customer Signup",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/signup" }
    },
    {
      "id": "create-customer",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.stripe.com/v1/customers",
        "body": { "email": "{{ $json.email }}" }
      }
    },
    {
      "id": "save-db",
      "type": "mysql.query",
      "config": {
        "query": "INSERT INTO customers (email, stripe_id) VALUES (?, ?)",
        "parameters": ["{{ $json.email }}", "{{ $json.stripeCustomerId }}"]
      }
    },
    {
      "id": "welcome-email",
      "type": "notifyx.sendNotification",
      "config": {
        "channel": "email",
        "recipient": "{{ $json.email }}",
        "template": "welcome"
      }
    },
    {
      "id": "slack-notify",
      "type": "slack.sendMessage",
      "config": {
        "channel": "#sales",
        "text": "New customer: {{ $json.email }}"
      }
    }
  ],
  "edges": [
    { "from": "webhook", "to": "create-customer" },
    { "from": "create-customer", "to": "save-db" },
    { "from": "save-db", "to": "welcome-email" },
    { "from": "welcome-email", "to": "slack-notify" }
  ]
}
```

### 2. Conditional Branching

**Use Case**: Route based on conditions

```
Webhook → If Condition → [Premium Path | Standard Path]
```

**Example**: Plan-based onboarding
```json
{
  "name": "Plan-based Onboarding",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/signup" }
    },
    {
      "id": "check-plan",
      "type": "if.condition",
      "config": {
        "condition": "{{ $json.plan === 'premium' }}",
        "trueOutput": "premium-onboarding",
        "falseOutput": "standard-onboarding"
      }
    },
    {
      "id": "premium-onboarding",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "onboardingSteps", "value": ["Setup integrations", "Schedule demo", "Access premium features"] }
        ]
      }
    },
    {
      "id": "standard-onboarding",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "onboardingSteps", "value": ["Complete profile", "Basic setup"] }
        ]
      }
    }
  ],
  "edges": [
    { "from": "webhook", "to": "check-plan" },
    { "from": "check-plan", "to": "premium-onboarding" },
    { "from": "check-plan", "to": "standard-onboarding" }
  ]
}
```

### 3. Parallel Processing

**Use Case**: Execute multiple actions simultaneously

```
Webhook → Split → [Action 1 | Action 2 | Action 3] → Merge
```

**Example**: Multi-channel notification
```json
{
  "name": "Multi-channel Notification",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/alert" }
    },
    {
      "id": "send-email",
      "type": "notifyx.sendNotification",
      "config": {
        "channel": "email",
        "recipient": "{{ $json.email }}",
        "template": "alert"
      }
    },
    {
      "id": "send-sms",
      "type": "notifyx.sendNotification",
      "config": {
        "channel": "sms",
        "recipient": "{{ $json.phone }}",
        "template": "alert"
      }
    },
    {
      "id": "send-slack",
      "type": "slack.sendMessage",
      "config": {
        "channel": "#alerts",
        "text": "Alert: {{ $json.message }}"
      }
    },
    {
      "id": "merge-results",
      "type": "merge.data",
      "config": {
        "sources": ["{{ $json.emailResult }}", "{{ $json.smsResult }}", "{{ $json.slackResult }}"]
      }
    }
  ],
  "edges": [
    { "from": "webhook", "to": "send-email" },
    { "from": "webhook", "to": "send-sms" },
    { "from": "webhook", "to": "send-slack" },
    { "from": "send-email", "to": "merge-results" },
    { "from": "send-sms", "to": "merge-results" },
    { "from": "send-slack", "to": "merge-results" }
  ]
}
```

---

## Error Handling Patterns

### 1. Retry with Exponential Backoff

**Use Case**: Handle transient failures

```json
{
  "name": "Retry Pattern",
  "nodes": [
    {
      "id": "api-call",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.external-service.com/data",
        "retry": {
          "attempts": 3,
          "delay": 1000,
          "backoffMultiplier": 2
        }
      }
    },
    {
      "id": "error-handler",
      "type": "error.handler",
      "config": {
        "retryPolicy": {
          "maxAttempts": 3,
          "delay": "1s",
          "backoffMultiplier": 2
        },
        "fallbackAction": "send-alert"
      }
    },
    {
      "id": "send-alert",
      "type": "slack.sendMessage",
      "config": {
        "channel": "#alerts",
        "text": "API call failed after retries: {{ $error.message }}"
      }
    }
  ]
}
```

### 2. Circuit Breaker

**Use Case**: Prevent cascading failures

```json
{
  "name": "Circuit Breaker Pattern",
  "nodes": [
    {
      "id": "check-circuit",
      "type": "if.condition",
      "config": {
        "condition": "{{ $env.circuitBreakerOpen === false }}",
        "trueOutput": "make-api-call",
        "falseOutput": "circuit-open-handler"
      }
    },
    {
      "id": "make-api-call",
      "type": "http.request",
      "config": {
        "method": "GET",
        "url": "https://api.external-service.com/data"
      }
    },
    {
      "id": "circuit-open-handler",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "error", "value": "Circuit breaker is open" },
          { "field": "fallback", "value": true }
        ]
      }
    }
  ]
}
```

### 3. Dead Letter Queue

**Use Case**: Handle permanently failed messages

```json
{
  "name": "Dead Letter Queue Pattern",
  "nodes": [
    {
      "id": "process-message",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.external-service.com/process"
      }
    },
    {
      "id": "error-handler",
      "type": "error.handler",
      "config": {
        "retryPolicy": {
          "maxAttempts": 3
        },
        "fallbackAction": "dead-letter-queue"
      }
    },
    {
      "id": "dead-letter-queue",
      "type": "mysql.query",
      "config": {
        "query": "INSERT INTO dead_letter_queue (message, error, created_at) VALUES (?, ?, NOW())",
        "parameters": ["{{ $json | json }}", "{{ $error.message }}"]
      }
    }
  ]
}
```

---

## Data Processing Patterns

### 1. Batch Processing

**Use Case**: Process large datasets in batches

```json
{
  "name": "Batch Processing Pattern",
  "nodes": [
    {
      "id": "schedule-trigger",
      "type": "schedule.trigger",
      "config": {
        "cron": "0 2 * * *",
        "description": "Daily batch processing"
      }
    },
    {
      "id": "get-batch",
      "type": "mysql.query",
      "config": {
        "query": "SELECT * FROM pending_orders WHERE status = 'pending' LIMIT 100",
        "operation": "select"
      }
    },
    {
      "id": "process-batch",
      "type": "loop.iterator",
      "config": {
        "items": "{{ $json.orders }}",
        "itemVariable": "order",
        "maxIterations": 100
      }
    },
    {
      "id": "process-order",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.payment-service.com/process",
        "body": {
          "orderId": "{{ $order.id }}",
          "amount": "{{ $order.amount }}"
        }
      }
    }
  ]
}
```

### 2. Data Transformation

**Use Case**: Transform data between systems

```json
{
  "name": "Data Transformation Pattern",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/data-transform" }
    },
    {
      "id": "validate-data",
      "type": "if.condition",
      "config": {
        "condition": "{{ $json.email && $json.name }}",
        "trueOutput": "transform-data",
        "falseOutput": "send-error"
      }
    },
    {
      "id": "transform-data",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "customerId", "value": "CUST-{{ $json.id }}" },
          { "field": "fullName", "value": "{{ $json.firstName }} {{ $json.lastName }}" },
          { "field": "processedAt", "value": "{{ $now }}" },
          { "field": "status", "value": "active" }
        ]
      }
    },
    {
      "id": "send-to-destination",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.destination-system.com/customers",
        "body": "{{ $json }}"
      }
    }
  ]
}
```

### 3. Data Aggregation

**Use Case**: Aggregate data from multiple sources

```json
{
  "name": "Data Aggregation Pattern",
  "nodes": [
    {
      "id": "schedule-trigger",
      "type": "schedule.trigger",
      "config": {
        "cron": "0 0 * * *",
        "description": "Daily aggregation"
      }
    },
    {
      "id": "get-sales-data",
      "type": "mysql.query",
      "config": {
        "query": "SELECT SUM(amount) as total_sales, COUNT(*) as order_count FROM orders WHERE DATE(created_at) = CURDATE()",
        "operation": "select"
      }
    },
    {
      "id": "get-customer-data",
      "type": "mysql.query",
      "config": {
        "query": "SELECT COUNT(*) as new_customers FROM customers WHERE DATE(created_at) = CURDATE()",
        "operation": "select"
      }
    },
    {
      "id": "merge-data",
      "type": "merge.data",
      "config": {
        "sources": ["{{ $json.salesData }}", "{{ $json.customerData }}"]
      }
    },
    {
      "id": "generate-report",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "reportDate", "value": "{{ $now.format('YYYY-MM-DD') }}" },
          { "field": "totalSales", "value": "{{ $json.total_sales }}" },
          { "field": "orderCount", "value": "{{ $json.order_count }}" },
          { "field": "newCustomers", "value": "{{ $json.new_customers }}" }
        ]
      }
    }
  ]
}
```

---

## Integration Patterns

### 1. API Gateway Pattern

**Use Case**: Centralize API access and management

```json
{
  "name": "API Gateway Pattern",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/api-gateway" }
    },
    {
      "id": "authenticate",
      "type": "if.condition",
      "config": {
        "condition": "{{ $headers['Authorization'] }}",
        "trueOutput": "route-request",
        "falseOutput": "unauthorized"
      }
    },
    {
      "id": "route-request",
      "type": "switch.condition",
      "config": {
        "expression": "{{ $json.service }}",
        "cases": [
          { "value": "stripe", "output": "stripe-service" },
          { "value": "slack", "output": "slack-service" },
          { "value": "email", "output": "email-service" }
        ],
        "default": "unknown-service"
      }
    },
    {
      "id": "stripe-service",
      "type": "http.request",
      "config": {
        "method": "{{ $json.method }}",
        "url": "https://api.stripe.com/v1/{{ $json.endpoint }}",
        "headers": {
          "Authorization": "Bearer {{ $credentials.stripe.secretKey }}"
        }
      }
    }
  ]
}
```

### 2. Event Sourcing Pattern

**Use Case**: Track all changes as events

```json
{
  "name": "Event Sourcing Pattern",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/event" }
    },
    {
      "id": "create-event",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "eventId", "value": "{{ $uuid() }}" },
          { "field": "eventType", "value": "{{ $json.type }}" },
          { "field": "aggregateId", "value": "{{ $json.aggregateId }}" },
          { "field": "eventData", "value": "{{ $json.data }}" },
          { "field": "timestamp", "value": "{{ $now }}" },
          { "field": "version", "value": "{{ $json.version }}" }
        ]
      }
    },
    {
      "id": "store-event",
      "type": "mysql.query",
      "config": {
        "query": "INSERT INTO events (id, type, aggregate_id, data, timestamp, version) VALUES (?, ?, ?, ?, ?, ?)",
        "parameters": [
          "{{ $json.eventId }}",
          "{{ $json.eventType }}",
          "{{ $json.aggregateId }}",
          "{{ $json.eventData | json }}",
          "{{ $json.timestamp }}",
          "{{ $json.version }}"
        ]
      }
    },
    {
      "id": "publish-event",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.event-bus.com/events",
        "body": "{{ $json }}"
      }
    }
  ]
}
```

### 3. Saga Pattern

**Use Case**: Manage distributed transactions

```json
{
  "name": "Saga Pattern",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/saga" }
    },
    {
      "id": "step1-create-order",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.order-service.com/orders",
        "body": { "customerId": "{{ $json.customerId }}", "items": "{{ $json.items }}" }
      }
    },
    {
      "id": "step2-reserve-inventory",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.inventory-service.com/reserve",
        "body": { "orderId": "{{ $json.orderId }}", "items": "{{ $json.items }}" }
      }
    },
    {
      "id": "step3-process-payment",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.payment-service.com/charge",
        "body": { "orderId": "{{ $json.orderId }}", "amount": "{{ $json.totalAmount }}" }
      }
    },
    {
      "id": "compensate-payment",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.payment-service.com/refund",
        "body": { "orderId": "{{ $json.orderId }}" }
      }
    },
    {
      "id": "compensate-inventory",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.inventory-service.com/release",
        "body": { "orderId": "{{ $json.orderId }}" }
      }
    },
    {
      "id": "compensate-order",
      "type": "http.request",
      "config": {
        "method": "DELETE",
        "url": "https://api.order-service.com/orders/{{ $json.orderId }}"
      }
    }
  ]
}
```

---

## Monitoring Patterns

### 1. Health Check Pattern

**Use Case**: Monitor system health

```json
{
  "name": "Health Check Pattern",
  "nodes": [
    {
      "id": "schedule-trigger",
      "type": "schedule.trigger",
      "config": {
        "cron": "*/5 * * * *",
        "description": "Health check every 5 minutes"
      }
    },
    {
      "id": "check-database",
      "type": "mysql.query",
      "config": {
        "query": "SELECT 1",
        "operation": "select"
      }
    },
    {
      "id": "check-api",
      "type": "http.request",
      "config": {
        "method": "GET",
        "url": "https://api.external-service.com/health"
      }
    },
    {
      "id": "check-redis",
      "type": "http.request",
      "config": {
        "method": "GET",
        "url": "http://redis:6379/ping"
      }
    },
    {
      "id": "aggregate-health",
      "type": "merge.data",
      "config": {
        "sources": ["{{ $json.databaseHealth }}", "{{ $json.apiHealth }}", "{{ $json.redisHealth }}"]
      }
    },
    {
      "id": "send-alert",
      "type": "if.condition",
      "config": {
        "condition": "{{ $json.databaseHealth.status !== 'healthy' || $json.apiHealth.status !== 'healthy' }}",
        "trueOutput": "alert",
        "falseOutput": "log-health"
      }
    }
  ]
}
```

### 2. Metrics Collection Pattern

**Use Case**: Collect and send metrics

```json
{
  "name": "Metrics Collection Pattern",
  "nodes": [
    {
      "id": "schedule-trigger",
      "type": "schedule.trigger",
      "config": {
        "cron": "*/1 * * * *",
        "description": "Collect metrics every minute"
      }
    },
    {
      "id": "collect-metrics",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "timestamp", "value": "{{ $now }}" },
          { "field": "workflowExecutions", "value": "{{ $env.workflowExecutions }}" },
          { "field": "errorRate", "value": "{{ $env.errorRate }}" },
          { "field": "averageExecutionTime", "value": "{{ $env.averageExecutionTime }}" }
        ]
      }
    },
    {
      "id": "send-to-prometheus",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://prometheus:9091/metrics/job/notifyxstudio",
        "body": "{{ $json }}"
      }
    }
  ]
}
```

---

## Advanced Patterns

### 1. Workflow Orchestration

**Use Case**: Coordinate multiple workflows

```json
{
  "name": "Workflow Orchestration Pattern",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/orchestrate" }
    },
    {
      "id": "start-workflow1",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.notifyxstudio.com/api/workflows/workflow1/runs",
        "body": { "payload": "{{ $json.data1 }}" }
      }
    },
    {
      "id": "start-workflow2",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.notifyxstudio.com/api/workflows/workflow2/runs",
        "body": { "payload": "{{ $json.data2 }}" }
      }
    },
    {
      "id": "wait-for-completion",
      "type": "wait.delay",
      "config": {
        "duration": "5m",
        "reason": "Wait for workflows to complete"
      }
    },
    {
      "id": "check-status",
      "type": "http.request",
      "config": {
        "method": "GET",
        "url": "https://api.notifyxstudio.com/api/runs/{{ $json.runId1 }}"
      }
    }
  ]
}
```

### 2. Dynamic Workflow Generation

**Use Case**: Generate workflows based on configuration

```json
{
  "name": "Dynamic Workflow Generation",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/generate-workflow" }
    },
    {
      "id": "get-config",
      "type": "mysql.query",
      "config": {
        "query": "SELECT * FROM workflow_configs WHERE id = ?",
        "parameters": ["{{ $json.configId }}"]
      }
    },
    {
      "id": "generate-nodes",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "nodes", "value": "{{ $json.config.steps.map(step => ({ id: step.id, type: step.type, config: step.config })) }}" }
        ]
      }
    },
    {
      "id": "create-workflow",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.notifyxstudio.com/api/workflows",
        "body": {
          "name": "{{ $json.config.name }}",
          "nodes": "{{ $json.nodes }}",
          "edges": "{{ $json.config.edges }}"
        }
      }
    }
  ]
}
```

### 3. A/B Testing Pattern

**Use Case**: Test different workflow variations

```json
{
  "name": "A/B Testing Pattern",
  "nodes": [
    {
      "id": "webhook",
      "type": "webhook.trigger",
      "config": { "path": "/ab-test" }
    },
    {
      "id": "determine-variant",
      "type": "if.condition",
      "config": {
        "condition": "{{ $json.userId % 2 === 0 }}",
        "trueOutput": "variant-a",
        "falseOutput": "variant-b"
      }
    },
    {
      "id": "variant-a",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "variant", "value": "A" },
          { "field": "emailTemplate", "value": "welcome-a" },
          { "field": "onboardingSteps", "value": ["Step 1A", "Step 2A"] }
        ]
      }
    },
    {
      "id": "variant-b",
      "type": "set.data",
      "config": {
        "operations": [
          { "field": "variant", "value": "B" },
          { "field": "emailTemplate", "value": "welcome-b" },
          { "field": "onboardingSteps", "value": ["Step 1B", "Step 2B"] }
        ]
      }
    },
    {
      "id": "track-experiment",
      "type": "mysql.query",
      "config": {
        "query": "INSERT INTO ab_test_results (user_id, variant, timestamp) VALUES (?, ?, NOW())",
        "parameters": ["{{ $json.userId }}", "{{ $json.variant }}"]
      }
    }
  ]
}
```

---

*These patterns provide a foundation for building robust, scalable workflows. Combine and adapt them based on your specific use cases and requirements.*