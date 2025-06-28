# ESPC Session Sample

This repository accompanies the ESPC session **"Developing, Testing, and Deploying SharePoint Framework Solutions with Success"** by Guido and Peter. It demonstrates a real-world SharePoint Framework (SPFx) solution that integrates Microsoft Graph and a custom backend API hosted on Azure Functions. It also showcases testing, configuration management, and CI/CD strategies.

---

## 🧠 Technical Concept

We have a React-based SPFx web part (v1.22) that consumes:

- **Microsoft Graph API**
- **Custom API** hosted in **Azure Functions (C#)** using HTTP triggers

The solution supports full local debugging with the following setup:

1. **Start Azure Function** locally
3. **Build SPFx** (with the tunnel URL injected into `.env`)
4. Open the **Workbench** to run and test (`serve.json`)


## 📁 Folder Structure

/infra → Bicep templates for provisioning required Azure resources
/spfx → SharePoint Framework solution with a React web part
/func → Azure Function project written in C#



## 🧪 Requirements for Local Development

- Node.js 22+
- SharePoint Framework 
- M365 cli (for login/testing)
- .NET SDK (for Azure Functions)
- Dev Tunnel (`devtunnel`) if webhooks are tested
- Microsoft Dev Proxy (optional but recommended)

### Debugging without Dev Tunnel

> For basic local development and testing, the Dev Tunnel is **not strictly required**.  
However, it is **needed when testing cloud integrations** like webhooks or Graph callbacks.

We use **Microsoft Dev Proxy** in 3 configurations:

1. **Monitoring Mode**: Observe live calls to SharePoint and Graph
2. **Static Mock (0% error rate)**: Use predefined responses to simulate APIs
3. **Static Mock (High error rate)**: Simulate flaky network/API to test resilience

## 🚀 Requirements for Deployment

### Manual Deployment

1. Deploy Azure resources via Bicep (`infra` folder)
2. Publish Azure Function:
   ```bash
   func azure functionapp publish <your-func-name>
   ````
Package & deploy SPFx package to App Catalog

Configure environment variables for production usage

CI/CD Deployment (GitHub Actions)
We provide a sample GitHub Actions workflow to automate:
pipeline based on VS code extention SPFX toolkit

Linting & testing

Building the Azure Function and deploying it

Packaging and deploying the SPFx solution

Setting tenant-specific configs via CLI for M365

## 📌 Key Facts
Use devtunnel only when triggering from the cloud (e.g., Graph webhooks or testing inbound API calls from M365)

Use .env files to manage tenant-specific configurations

Dev Proxy helps simulate Graph and SharePoint APIs under various conditions

SPFx 1.22 includes support for environment-specific serveConfigurations

....

Webpart
Warehouse ( API + OpenAPI *to session with Agent Toolkit (...) ).



## 📎 Resources
Microsoft Dev Proxy

SPFx Docs

Azure Functions

GitHub Actions

