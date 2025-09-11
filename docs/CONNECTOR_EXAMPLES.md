# NotifyX Studio - Connector Examples & Use Cases

## Table of Contents

1. [Core Connectors](#core-connectors)
2. [Communication Connectors](#communication-connectors)
3. [Data Connectors](#data-connectors)
4. [Logic Connectors](#logic-connectors)
5. [Integration Connectors](#integration-connectors)
6. [Advanced Use Cases](#advanced-use-cases)

---

## Core Connectors

### Webhook Trigger

**Use Case**: Customer signup automation

```json
{
  "type": "webhook.trigger",
  "config": {
    "path": "/webhook/signup",
    "method": "POST",
    "headers": {
      "X-API-Key": "{{ $env.WEBHOOK_SECRET }}"
    },
    "response": {
      "statusCode": 200,
      "body": "{\"status\": \"received\", \"id\": \"{{ $uuid() }}\"}"
    }
  }
}
```

**Example Payload**:
```json
{
  "customerId": "12345",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "plan": "premium",
  "source": "website"
}
```

### HTTP Request

**Use Case**: Create customer in Stripe

```json
{
  "type": "http.request",
  "config": {
    "method": "POST",
    "url": "https://api.stripe.com/v1/customers",
    "headers": {
      "Authorization": "Bearer {{ $credentials.stripe.secretKey }}",
      "Content-Type": "application/json"
    },
    "body": {
      "email": "{{ $json.email }}",
      "name": "{{ $json.firstName }} {{ $json.lastName }}",
      "metadata": {
        "source": "notifyx-studio",
        "plan": "{{ $json.plan }}"
      }
    },
    "timeout": 30000
  }
}
```

**Response Handling**:
```json
{
  "id": "cus_1234567890",
  "email": "john@example.com",
  "created": 1640995200,
  "metadata": {
    "source": "notifyx-studio",
    "plan": "premium"
  }
}
```

---

## Communication Connectors

### NotifyX Send Notification

**Use Case**: Welcome email sequence

```json
{
  "type": "notifyx.sendNotification",
  "config": {
    "channel": "email",
    "priority": "normal",
    "template": "customer-welcome",
    "recipient": "{{ $json.email }}",
    "data": {
      "customerName": "{{ $json.firstName }}",
      "loginUrl": "https://app.example.com/login",
      "supportEmail": "support@example.com",
      "plan": "{{ $json.plan }}"
    },
    "scheduling": {
      "delay": "5m"
    }
  }
}
```

**Template Variables**:
```json
{
  "customerName": "John",
  "loginUrl": "https://app.example.com/login",
  "supportEmail": "support@example.com",
  "plan": "premium"
}
```

### Slack Send Message

**Use Case**: Team notification for new premium customers

```json
{
  "type": "slack.sendMessage",
  "config": {
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
            "text": "*Name:*\n{{ $json.firstName }} {{ $json.lastName }}"
          },
          {
            "type": "mrkdwn",
            "text": "*Email:*\n{{ $json.email }}"
          },
          {
            "type": "mrkdwn",
            "text": "*Plan:*\n{{ $json.plan }}"
          },
          {
            "type": "mrkdwn",
            "text": "*MRR:*\n${{ $json.monthlyRevenue }}"
          }
        ]
      },
      {
        "type": "actions",
        "elements": [
          {
            "type": "button",
            "text": {
              "type": "plain_text",
              "text": "View Customer"
            },
            "url": "https://app.example.com/customers/{{ $json.customerId }}"
          }
        ]
      }
    ]
  }
}
```

### Gmail Send Email

**Use Case**: Onboarding email with attachments

```json
{
  "type": "gmail.sendEmail",
  "config": {
    "to": "{{ $json.email }}",
    "subject": "Welcome to {{ $json.companyName }} - Let's get started!",
    "htmlBody": "<div style='font-family: Arial, sans-serif; max-width: 600px;'><h1 style='color: #333;'>Welcome {{ $json.firstName }}!</h1><p>Thank you for choosing {{ $json.companyName }}. Here's what's next:</p><ul><li>Complete your profile setup</li><li>Connect your first integration</li><li>Schedule a demo with our team</li></ul><a href='{{ $json.onboardingUrl }}' style='background: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Get Started</a></div>",
    "textBody": "Welcome {{ $json.firstName }}! Thank you for choosing {{ $json.companyName }}. Visit {{ $json.onboardingUrl }} to get started.",
    "attachments": [
      {
        "filename": "getting-started.pdf",
        "content": "{{ $binary.gettingStartedGuide }}",
        "type": "application/pdf"
      }
    ]
  }
}
```

---

## Data Connectors

### MySQL Query

**Use Case**: Customer data management

```json
{
  "type": "mysql.query",
  "config": {
    "query": "INSERT INTO customers (id, name, email, plan, status, created_at, metadata) VALUES (?, ?, ?, ?, 'active', NOW(), ?)",
    "parameters": [
      "{{ $json.customerId }}",
      "{{ $json.firstName }} {{ $json.lastName }}",
      "{{ $json.email }}",
      "{{ $json.plan }}",
      "{{ $json | json }}"
    ],
    "operation": "insert",
    "returnData": true
  }
}
```

**Query Examples**:
```sql
-- Insert customer
INSERT INTO customers (id, name, email, plan, status, created_at) 
VALUES (?, ?, ?, ?, 'active', NOW())

-- Update customer status
UPDATE customers 
SET status = ?, updated_at = NOW() 
WHERE id = ?

-- Get customer by email
SELECT * FROM customers 
WHERE email = ? AND status = 'active'

-- Get customers by plan
SELECT * FROM customers 
WHERE plan = ? AND created_at >= ?
```

### Schedule Trigger

**Use Case**: Daily data synchronization

```json
{
  "type": "schedule.trigger",
  "config": {
    "cron": "0 2 * * *",
    "timezone": "UTC",
    "description": "Daily data sync at 2 AM UTC"
  }
}
```

**Cron Examples**:
```javascript
// Every 15 minutes
"*/15 * * * *"

// Every hour at minute 0
"0 * * * *"

// Every day at 9 AM EST
"0 9 * * *" (with timezone: "America/New_York")

// Every Monday at 9 AM
"0 9 * * MON"

// First day of every month at midnight
"0 0 1 * *"

// Every weekday at 9 AM
"0 9 * * 1-5"
```

---

## Logic Connectors

### If Condition

**Use Case**: Route customers by plan type

```json
{
  "type": "if.condition",
  "config": {
    "condition": "{{ $json.plan === 'premium' && $json.monthlyRevenue > 1000 }}",
    "trueOutput": "vip-onboarding",
    "falseOutput": "standard-onboarding"
  }
}
```

**Condition Examples**:
```javascript
// Check plan type
"{{ $json.plan === 'premium' }}"

// Check revenue threshold
"{{ $json.monthlyRevenue > 1000 }}"

// Check email domain
"{{ $json.email.endsWith('@enterprise.com') }}"

// Check multiple conditions
"{{ $json.plan === 'premium' && $json.monthlyRevenue > 1000 && $json.region === 'US' }}"

// Check array contains value
"{{ $json.features.includes('advanced-analytics') }}"

// Check date range
"{{ $json.signupDate >= '2024-01-01' && $json.signupDate <= '2024-12-31' }}"
```

### Switch Condition

**Use Case**: Route by geographic region

```json
{
  "type": "switch.condition",
  "config": {
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
}
```

### Merge Data

**Use Case**: Combine customer data from multiple sources

```json
{
  "type": "merge.data",
  "config": {
    "mode": "merge",
    "sources": [
      "{{ $json.webhookData }}",
      "{{ $json.databaseData }}",
      "{{ $json.externalApiData }}"
    ],
    "conflictResolution": "priority",
    "priority": ["externalApiData", "databaseData", "webhookData"]
  }
}
```

### Set Data

**Use Case**: Transform and enrich customer data

```json
{
  "type": "set.data",
  "config": {
    "operations": [
      {
        "field": "customerId",
        "value": "CUST-{{ $json.id }}"
      },
      {
        "field": "fullName",
        "value": "{{ $json.firstName }} {{ $json.lastName }}"
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
        "field": "customerTier",
        "value": "{{ $json.monthlyRevenue > 1000 ? 'premium' : 'standard' }}"
      },
      {
        "field": "nextSteps",
        "value": "{{ $json.plan === 'premium' ? ['Setup integrations', 'Schedule demo'] : ['Complete profile'] }}"
      },
      {
        "field": "processedAt",
        "value": "{{ $now }}"
      }
    ]
  }
}
```

---

## Integration Connectors

### Salesforce Create Lead

**Use Case**: Convert signup to Salesforce lead

```json
{
  "type": "salesforce.createLead",
  "config": {
    "firstName": "{{ $json.firstName }}",
    "lastName": "{{ $json.lastName }}",
    "email": "{{ $json.email }}",
    "company": "{{ $json.company }}",
    "leadSource": "Website",
    "status": "New",
    "customFields": {
      "Plan__c": "{{ $json.plan }}",
      "Monthly_Revenue__c": "{{ $json.monthlyRevenue }}",
      "Signup_Date__c": "{{ $json.signupDate }}"
    }
  }
}
```

### HubSpot Create Contact

**Use Case**: Add customer to HubSpot

```json
{
  "type": "hubspot.createContact",
  "config": {
    "email": "{{ $json.email }}",
    "firstName": "{{ $json.firstName }}",
    "lastName": "{{ $json.lastName }}",
    "company": "{{ $json.company }}",
    "properties": {
      "plan": "{{ $json.plan }}",
      "monthly_revenue": "{{ $json.monthlyRevenue }}",
      "signup_date": "{{ $json.signupDate }}",
      "lifecycle_stage": "customer"
    }
  }
}
```

### Zapier Webhook

**Use Case**: Trigger external Zapier workflows

```json
{
  "type": "zapier.webhook",
  "config": {
    "url": "{{ $credentials.zapier.webhookUrl }}",
    "method": "POST",
    "headers": {
      "Content-Type": "application/json"
    },
    "body": {
      "customer_id": "{{ $json.customerId }}",
      "email": "{{ $json.email }}",
      "plan": "{{ $json.plan }}",
      "signup_date": "{{ $json.signupDate }}",
      "source": "notifyx-studio"
    }
  }
}
```

---

## Advanced Use Cases

### Customer Onboarding Workflow

**Complete workflow for new customer onboarding**:

```json
{
  "name": "Customer Onboarding",
  "description": "Complete onboarding process for new customers",
  "nodes": [
    {
      "id": "webhook-trigger",
      "type": "webhook.trigger",
      "position": { "x": 100, "y": 100 },
      "config": {
        "path": "/webhook/signup",
        "method": "POST"
      }
    },
    {
      "id": "validate-data",
      "type": "if.condition",
      "position": { "x": 300, "y": 100 },
      "config": {
        "condition": "{{ $json.email && $json.firstName && $json.lastName }}",
        "trueOutput": "create-stripe-customer",
        "falseOutput": "send-error-notification"
      }
    },
    {
      "id": "create-stripe-customer",
      "type": "http.request",
      "position": { "x": 500, "y": 100 },
      "config": {
        "method": "POST",
        "url": "https://api.stripe.com/v1/customers",
        "headers": {
          "Authorization": "Bearer {{ $credentials.stripe.secretKey }}",
          "Content-Type": "application/json"
        },
        "body": {
          "email": "{{ $json.email }}",
          "name": "{{ $json.firstName }} {{ $json.lastName }}"
        }
      }
    },
    {
      "id": "save-to-database",
      "type": "mysql.query",
      "position": { "x": 700, "y": 100 },
      "config": {
        "query": "INSERT INTO customers (id, name, email, stripe_id, created_at) VALUES (?, ?, ?, ?, NOW())",
        "parameters": [
          "{{ $json.customerId }}",
          "{{ $json.firstName }} {{ $json.lastName }}",
          "{{ $json.email }}",
          "{{ $json.stripeCustomerId }}"
        ]
      }
    },
    {
      "id": "send-welcome-email",
      "type": "notifyx.sendNotification",
      "position": { "x": 900, "y": 100 },
      "config": {
        "channel": "email",
        "template": "welcome",
        "recipient": "{{ $json.email }}",
        "data": {
          "customerName": "{{ $json.firstName }}",
          "loginUrl": "https://app.example.com/login"
        }
      }
    },
    {
      "id": "notify-team",
      "type": "slack.sendMessage",
      "position": { "x": 1100, "y": 100 },
      "config": {
        "channel": "#sales",
        "text": "New customer: {{ $json.firstName }} {{ $json.lastName }}"
      }
    }
  ],
  "edges": [
    { "from": "webhook-trigger", "to": "validate-data" },
    { "from": "validate-data", "to": "create-stripe-customer" },
    { "from": "create-stripe-customer", "to": "save-to-database" },
    { "from": "save-to-database", "to": "send-welcome-email" },
    { "from": "send-welcome-email", "to": "notify-team" }
  ]
}
```

### Error Handling Workflow

**Robust error handling with retries and fallbacks**:

```json
{
  "name": "API Call with Error Handling",
  "nodes": [
    {
      "id": "api-call",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.external-service.com/data",
        "timeout": 30000,
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
        "fallbackAction": "send-alert",
        "errorTypes": ["timeout", "network", "server-error"]
      }
    },
    {
      "id": "send-alert",
      "type": "slack.sendMessage",
      "config": {
        "channel": "#alerts",
        "text": "API call failed: {{ $error.message }}"
      }
    }
  ]
}
```

### Parallel Processing Workflow

**Process multiple actions simultaneously**:

```json
{
  "name": "Parallel Customer Processing",
  "nodes": [
    {
      "id": "split-data",
      "type": "split.data",
      "config": {
        "field": "customers",
        "itemVariable": "customer"
      }
    },
    {
      "id": "send-email",
      "type": "notifyx.sendNotification",
      "config": {
        "channel": "email",
        "recipient": "{{ $customer.email }}",
        "template": "welcome"
      }
    },
    {
      "id": "create-stripe",
      "type": "http.request",
      "config": {
        "method": "POST",
        "url": "https://api.stripe.com/v1/customers",
        "body": {
          "email": "{{ $customer.email }}",
          "name": "{{ $customer.name }}"
        }
      }
    },
    {
      "id": "save-database",
      "type": "mysql.query",
      "config": {
        "query": "INSERT INTO customers (email, name) VALUES (?, ?)",
        "parameters": ["{{ $customer.email }}", "{{ $customer.name }}"]
      }
    },
    {
      "id": "merge-results",
      "type": "merge.data",
      "config": {
        "sources": ["{{ $json.emailResult }}", "{{ $json.stripeResult }}", "{{ $json.databaseResult }}"]
      }
    }
  ],
  "edges": [
    { "from": "split-data", "to": "send-email" },
    { "from": "split-data", "to": "create-stripe" },
    { "from": "split-data", "to": "save-database" },
    { "from": "send-email", "to": "merge-results" },
    { "from": "create-stripe", "to": "merge-results" },
    { "from": "save-database", "to": "merge-results" }
  ]
}
```

---

## Testing Connectors

### Unit Testing

```javascript
// Test HTTP Request connector
const httpRequest = new HttpRequestAdapter();
const context = new ConnectorExecutionContext(
  tenantId,
  JsonSerializer.SerializeToElement({
    method: "POST",
    url: "https://httpbin.org/post",
    body: { test: "data" }
  }),
  {},
  "",
  {}
);

const result = await httpRequest.ExecuteAsync(context);
assert(result.Success);
assert(result.Output.GetProperty("json").GetProperty("test").GetString() === "data");
```

### Integration Testing

```javascript
// Test complete workflow
const workflow = new Workflow(/* workflow definition */);
const executionEngine = new WorkflowExecutionEngine(/* dependencies */);

const result = await executionEngine.ExecuteWorkflowAsync(
  runId,
  workflow,
  { email: "test@example.com", name: "Test User" }
);

assert(result.Status === "completed");
```

---

*This guide provides comprehensive examples for all NotifyX Studio connectors. For more advanced use cases and custom connectors, refer to the [Developer Guide](DEVELOPER_GUIDE.md).*