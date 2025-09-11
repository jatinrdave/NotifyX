# NotifyX Studio - Help & Support

## 🆘 Quick Help

### Getting Started
- [Quick Start Guide](QUICK_START.md) - Get up and running in 5 minutes
- [Developer Guide](DEVELOPER_GUIDE.md) - Complete technical documentation
- [API Reference](API_REFERENCE.md) - Detailed API documentation

### Common Issues
- [Troubleshooting Guide](#troubleshooting)
- [FAQ](#frequently-asked-questions)
- [Error Codes](#error-codes)

### Support Channels
- [Email Support](mailto:support@notifyxstudio.com)
- [Community Discord](https://discord.gg/notifyxstudio)
- [GitHub Issues](https://github.com/your-org/notifyx-studio/issues)

---

## Troubleshooting

### Workflow Not Executing

**Symptoms**: Workflow shows as "queued" but never starts

**Possible Causes**:
- Kafka connectivity issues
- Runtime worker not running
- Invalid workflow configuration
- Credential problems

**Solutions**:
1. Check Kafka connectivity:
```bash
docker exec -it kafka kafka-topics --list --bootstrap-server localhost:9092
```

2. Check runtime worker logs:
```bash
docker logs notifyxstudio-runtime-worker
```

3. Verify workflow configuration:
```bash
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/workflows/{workflowId}
```

4. Test credentials:
```bash
curl -X POST http://localhost:5000/api/connectors/test \
     -H "Content-Type: application/json" \
     -d '{"type": "http.request", "config": {...}}'
```

### Connector Execution Failures

**Symptoms**: Individual nodes fail with errors

**Possible Causes**:
- Invalid connector configuration
- Expired or invalid credentials
- External API connectivity issues
- Data validation errors

**Solutions**:
1. Check node execution logs:
```bash
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/runs/{runId}/logs
```

2. Test connector independently:
```bash
curl -X POST http://localhost:5000/api/connectors/test \
     -H "Content-Type: application/json" \
     -d '{"type": "http.request", "config": {...}}'
```

3. Verify external API connectivity:
```bash
curl -I https://api.external-service.com/health
```

4. Check credential validity:
```bash
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/credentials
```

### Performance Issues

**Symptoms**: Slow workflow execution or timeouts

**Possible Causes**:
- High system load
- Database performance issues
- Kafka lag
- Complex workflow logic

**Solutions**:
1. Check system resources:
```bash
docker stats
```

2. Check database performance:
```bash
docker exec -it postgres psql -U user -d notifyxstudio -c "
SELECT query, mean_time, calls 
FROM pg_stat_statements 
ORDER BY mean_time DESC 
LIMIT 10;"
```

3. Check Kafka lag:
```bash
docker exec -it kafka kafka-consumer-groups --bootstrap-server localhost:9092 \
  --group notifyxstudio-runtime --describe
```

4. Optimize workflow:
- Reduce external API calls
- Use parallel processing
- Implement caching
- Simplify complex logic

### Authentication Issues

**Symptoms**: 401 Unauthorized errors

**Possible Causes**:
- Expired JWT token
- Invalid API key
- Missing authentication header
- Insufficient permissions

**Solutions**:
1. Check token expiration:
```bash
echo $JWT_TOKEN | base64 -d
```

2. Refresh token:
```bash
curl -X POST http://localhost:5000/api/auth/refresh \
     -H "Content-Type: application/json" \
     -d '{"refreshToken": "your-refresh-token"}'
```

3. Verify API key:
```bash
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/auth/me
```

4. Check permissions:
```bash
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/auth/permissions
```

---

## Frequently Asked Questions

### General

**Q: What is NotifyX Studio?**
A: NotifyX Studio is an n8n-style visual integration platform that allows you to create automated workflows by connecting different services and APIs.

**Q: How is it different from n8n?**
A: NotifyX Studio is built specifically for enterprise use cases with enhanced security, scalability, and integration with the NotifyX notification platform.

**Q: Is it open source?**
A: Yes, NotifyX Studio is open source and available under the MIT license.

**Q: What programming languages are supported?**
A: The platform supports JavaScript expressions, and you can build custom connectors in C#, JavaScript, or Python.

### Workflows

**Q: How many workflows can I create?**
A: There's no limit on the number of workflows you can create. The limit is based on your subscription plan and system resources.

**Q: Can I share workflows between users?**
A: Yes, you can share workflows with other users in your organization or export/import them.

**Q: How do I debug a workflow?**
A: Use the run inspector to view real-time execution logs, or add debug nodes to your workflow to log intermediate data.

**Q: Can I schedule workflows?**
A: Yes, use the Schedule Trigger connector to run workflows on a cron schedule.

### Connectors

**Q: How many connectors are available?**
A: NotifyX Studio comes with 50+ pre-built connectors, and you can create custom connectors for any service.

**Q: Can I create custom connectors?**
A: Yes, you can create custom connectors using the IConnectorAdapter interface in C#.

**Q: How do I add new connectors?**
A: Custom connectors can be added by implementing the IConnectorAdapter interface and registering them in the DI container.

**Q: Are connectors free?**
A: All built-in connectors are free. Some third-party connectors may require paid subscriptions to their services.

### Security

**Q: How are credentials stored?**
A: All credentials are encrypted with AES-256 and stored securely in HashiCorp Vault or Azure Key Vault.

**Q: Is my data secure?**
A: Yes, NotifyX Studio implements enterprise-grade security with encryption, RBAC, and audit logging.

**Q: Can I use it on-premises?**
A: Yes, NotifyX Studio can be deployed on-premises using Docker or Kubernetes.

**Q: Is it GDPR compliant?**
A: Yes, NotifyX Studio is designed to be GDPR compliant with data protection and privacy controls.

### Performance

**Q: How many workflows can run simultaneously?**
A: The number depends on your deployment configuration. With default settings, you can run hundreds of workflows simultaneously.

**Q: What's the maximum execution time?**
A: By default, workflows timeout after 30 minutes, but this can be configured.

**Q: How do I scale the platform?**
A: You can scale horizontally by adding more runtime workers or vertically by increasing resource limits.

**Q: What's the throughput?**
A: With proper configuration, NotifyX Studio can process thousands of workflow executions per minute.

---

## Error Codes

### HTTP Status Codes

| Code | Description | Solution |
|------|-------------|----------|
| 200 | Success | - |
| 201 | Created | - |
| 400 | Bad Request | Check request format and parameters |
| 401 | Unauthorized | Verify authentication token |
| 403 | Forbidden | Check user permissions |
| 404 | Not Found | Verify resource exists |
| 409 | Conflict | Resource already exists |
| 422 | Validation Error | Check input data validation |
| 429 | Rate Limited | Wait and retry |
| 500 | Internal Server Error | Contact support |

### Workflow Error Codes

| Code | Description | Solution |
|------|-------------|----------|
| WF001 | Invalid workflow definition | Check workflow JSON schema |
| WF002 | Missing required nodes | Add required nodes |
| WF003 | Circular dependency | Fix node connections |
| WF004 | Invalid node configuration | Check node config schema |
| WF005 | Workflow timeout | Optimize workflow or increase timeout |

### Connector Error Codes

| Code | Description | Solution |
|------|-------------|----------|
| CN001 | Invalid connector type | Use valid connector type |
| CN002 | Missing credentials | Add required credentials |
| CN003 | Invalid configuration | Check connector config |
| CN004 | External API error | Check external service status |
| CN005 | Rate limit exceeded | Implement retry logic |

### Runtime Error Codes

| Code | Description | Solution |
|------|-------------|----------|
| RT001 | Kafka connection failed | Check Kafka connectivity |
| RT002 | Database connection failed | Check database connectivity |
| RT003 | Redis connection failed | Check Redis connectivity |
| RT004 | Worker overloaded | Scale workers or optimize workflows |
| RT005 | Memory limit exceeded | Optimize workflow or increase memory |

---

## Debug Mode

### Enable Debug Logging

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

### Debug Workflow Execution

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

## Support Channels

### Email Support
- **General Support**: [support@notifyxstudio.com](mailto:support@notifyxstudio.com)
- **Enterprise Support**: [enterprise@notifyxstudio.com](mailto:enterprise@notifyxstudio.com)
- **Security Issues**: [security@notifyxstudio.com](mailto:security@notifyxstudio.com)

### Community
- **Discord**: [Join our Discord community](https://discord.gg/notifyxstudio)
- **GitHub**: [Report issues and contribute](https://github.com/your-org/notifyx-studio)
- **Stack Overflow**: [Ask questions](https://stackoverflow.com/questions/tagged/notifyx-studio)

### Documentation
- **API Reference**: [Complete API documentation](API_REFERENCE.md)
- **Connector Examples**: [Professional examples](CONNECTOR_EXAMPLES.md)
- **Developer Guide**: [Technical documentation](DEVELOPER_GUIDE.md)

### Status & Updates
- **Status Page**: [Check system status](https://status.notifyxstudio.com)
- **Release Notes**: [Latest updates](https://github.com/your-org/notifyx-studio/releases)
- **Changelog**: [Version history](CHANGELOG.md)

---

## Training & Resources

### Video Tutorials
- [Getting Started with NotifyX Studio](https://youtube.com/playlist?list=PLxyz)
- [Building Your First Workflow](https://youtube.com/watch?v=xyz)
- [Advanced Workflow Patterns](https://youtube.com/watch?v=xyz)

### Webinars
- [Monthly Feature Updates](https://notifyxstudio.com/webinars)
- [Best Practices for Enterprise](https://notifyxstudio.com/webinars/enterprise)
- [Custom Connector Development](https://notifyxstudio.com/webinars/connectors)

### Documentation
- [Complete API Reference](API_REFERENCE.md)
- [Connector Library](CONNECTOR_EXAMPLES.md)
- [Workflow Patterns](WORKFLOW_PATTERNS.md)
- [Best Practices](BEST_PRACTICES.md)

---

*Need more help? Contact our support team at [support@notifyxstudio.com](mailto:support@notifyxstudio.com) or join our [Discord community](https://discord.gg/notifyxstudio).*