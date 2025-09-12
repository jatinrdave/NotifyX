# NotifyX Studio - Quick Start Guide

## 🚀 Get Started in 5 Minutes

### Prerequisites
- Docker and Docker Compose
- Git

### 1. Clone and Start

```bash
git clone https://github.com/your-org/notifyx-studio.git
cd notifyx-studio
chmod +x scripts/setup.sh
./scripts/setup.sh
```

### 2. Access the Application

- **Frontend**: http://localhost:4200
- **API**: http://localhost:5000
- **Grafana**: http://localhost:3000 (admin/admin)

### 3. Create Your First Workflow

1. Open http://localhost:4200
2. Click "Create New Workflow"
3. Drag a "Webhook Trigger" to the canvas
4. Add an "HTTP Request" node
5. Connect the nodes
6. Configure the HTTP request URL
7. Save and test

### 4. Test Your Workflow

```bash
curl -X POST http://localhost:5000/webhook/your-webhook-path \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello World"}'
```

## 📚 Next Steps

- [Complete Developer Guide](DEVELOPER_GUIDE.md)
- [Connector Examples](CONNECTOR_EXAMPLES.md)
- [API Reference](API_REFERENCE.md)

## 🆘 Need Help?

- [Documentation](https://docs.notifyxstudio.com)
- [Community Discord](https://discord.gg/notifyxstudio)
- [Email Support](mailto:support@notifyxstudio.com)