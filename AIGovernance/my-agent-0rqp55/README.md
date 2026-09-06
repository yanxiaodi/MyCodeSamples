# Basic Hosted Agent (Responses Protocol)

A minimal [Agent Framework](https://github.com/microsoft/agent-framework) agent hosted on Microsoft Foundry using the **Responses protocol**. This sample demonstrates basic request/response interaction and multi-turn conversations.

## How it works

The agent uses `FoundryChatClient` from the Agent Framework and is served via `ResponsesHostServer`, which exposes a REST API compatible with the OpenAI Responses protocol. See [main.py](src/agent-framework-agent-basic-responses/main.py) for the implementation.

## Option 1: Azure Developer CLI (`azd`)

<details>
<summary><strong>Show steps</strong></summary>

### Prerequisites

1. **Azure Developer CLI (`azd`)** — [Install azd](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)
2. Install the AI agent extension:
   ```bash
   azd ext install microsoft.foundry
   ```
3. Authenticate:
   ```bash
   azd auth login
   ```

### Initialize the agent project

No cloning required. Create a new folder and initialize from the manifest:

```bash
mkdir my-basic-agent && cd my-basic-agent

azd ai agent init -m https://github.com/microsoft-foundry/foundry-samples/blob/main/samples/python/hosted-agents/agent-framework/responses/01-basic/azure.yaml
```

Follow the prompts to configure your Foundry project and model deployment. If you don't have an existing Foundry project, `azd ai agent init` will guide you through creating one.

### Provision Azure resources (if needed)

If you don't already have a Foundry project and model deployment:

```bash
azd provision
```

### Run the agent locally

```bash
azd ai agent run
```

The agent host will start on `http://localhost:8088`.

### Invoke the local agent

In a separate terminal, from the project directory:

```bash
azd ai agent invoke --local "Hi"
```

### Deploy to Foundry

Once tested locally, deploy to Microsoft Foundry:

```bash
azd deploy
```

For the full deployment guide, see [Deploy a hosted agent](https://learn.microsoft.com/en-us/azure/foundry/agents/how-to/deploy-hosted-agent).

### Invoke the deployed agent

```bash
azd ai agent invoke "Hi"
```

</details>

## Option 2: VS Code (Foundry Toolkit)

### Prerequisites

1. **VS Code** with the **[Foundry Toolkit](https://marketplace.visualstudio.com/items?itemName=ms-windows-ai-studio.windows-ai-studio)** extension installed.
2. For debugging Python in VS Code, install the **[Python](https://marketplace.visualstudio.com/items?itemName=ms-python.python)** extension pack.

### Set up the Python virtual environment

- Open the Command Palette (`Ctrl+Shift+P`) and run **Python: Create Environment...** to create a virtual environment in the workspace (or **Python: Select Interpreter** to use an existing one).
- Install dependencies in the virtual environment:

  ```bash
  # use uv to accelerate
  pip install uv
  uv pip install -r requirements.txt

  # or pure pip
  pip install -r requirements.txt
  ```

### Run locally in WSL

WSL is recommended for local governance-mode development because the
`agent-control-specification` package provides a Linux wheel. The Windows
installation may otherwise try to build the package from source and require
Rust/Cargo.

Install the **Remote - WSL** extension in VS Code, then reopen this folder in
WSL. From the agent source directory, run:

```bash
cd /mnt/d/dev/MyCodeSamples/AIGovernance/my-agent-0rqp55/src/agent-framework-agent-basic-responses

# Use a Linux virtual environment; do not reuse a Windows .venv.
python3 -m venv ~/.venvs/agent-governance
source ~/.venvs/agent-governance/bin/activate

python -m pip install --upgrade pip
python -m pip install -r requirements.txt

# DefaultAzureCredential uses the Azure CLI session.
az login
```

Create `.env` from `.env.example` if needed and configure the Foundry project,
model deployment, governance flag, Azure Communication Services, and optional
Application Insights values. For this Hosted Agent, `FOUNDRY_PROJECT_ENDPOINT`
must be the Foundry project endpoint (for example,
`https://<resource>.services.ai.azure.com/api/projects/<project>`), not an
OpenAI `/openai/v1` endpoint.

Start the agent manually:

```bash
python main.py
```

The local server listens on `http://localhost:8088`. In VS Code, run
**Foundry Toolkit: Open Agent Inspector** and connect to the local agent. VS
Code normally forwards the WSL localhost port automatically.

To compare both demo modes, change the value in `.env` and restart the agent:

```env
ENABLE_GOVERNANCE=false  # baseline mode
ENABLE_GOVERNANCE=true   # ACS policy plus MAF email approval
```

### Run and debug the agent

Press **F5** to start the agent. The agent starts and the **Agent Inspector** opens automatically. Chat with the agent in the Inspector.

### Or run manually, then open the Inspector

1. Set the required environment variables and sign in to Azure with the Azure CLI (`az login`).
2. Start the agent: `python main.py` (listens on `http://localhost:8088`).
3. Command Palette (`Ctrl+Shift+P`) → **Foundry Toolkit: Open Agent Inspector**, then send a message to test.

### Deploy to Foundry

1. Open the Command Palette (`Ctrl+Shift+P`) and run **Foundry Toolkit: Deploy Hosted Agent**. The extension opens a **Deploy Hosted Agent** wizard and reads `agent.yaml` to auto-populate settings.
2. If prompted, complete **Foundry Project Setup** to select subscription and project.
3. On the **Basics** tab, choose deployment method (**Code** or **Container**) and confirm the agent name.
4. On **Review + Deploy**, confirm runtime details, pick **CPU and Memory** size, and click **Deploy**.
5. After deployment, invoke the agent in the Agent Playground and stream live logs from the **Logs** tab.

## Next steps

## Governance demo

The hosted agent now includes three local tools:

- `lookup_customer_messages` returns deterministic demo messages.
- `send_email` sends through Azure Communication Services after approval.
- `delete_record` is a mock destructive operation that ACS policy blocks when governance is enabled.

The same agent can be run in two modes by setting `ENABLE_GOVERNANCE` in the source `.env` file:

```env
ENABLE_GOVERNANCE=false  # baseline: no ACS middleware and no email approval
ENABLE_GOVERNANCE=true   # ACS policy + MAF email approval
```

Copy [`.env.example`](src/agent-framework-agent-basic-responses/.env.example) to `.env` in the source directory and fill in the Foundry, Azure Communication Services, and (optionally) Application Insights values. The real connection strings stay in `.env` and are not committed.

When `APPLICATIONINSIGHTS_CONNECTION_STRING` is set, the agent initializes Azure Monitor OpenTelemetry at startup. Application Insights receives runtime/dependency telemetry plus non-sensitive demo tool events such as tool name, status, and mock message count; email bodies and business identifiers are not logged.

With governance enabled, ask the agent to:

1. Look up customer `C-1042` and summarize the messages.
2. Send the customer a status email. The Foundry/Responses approval flow should show an approval request before `send_email` executes.
3. Delete record `R-9001`. The ACS pre-tool policy denies it, and the mock tool should not run.

For local development, open `src/agent-framework-agent-basic-responses` in VS Code, install `requirements.txt`, and run `main.py` with the Foundry Toolkit Agent Inspector. Hosted deployment receives the same variables through the `azure.yaml` service configuration.

- [Quickstart: Create a hosted agent](https://learn.microsoft.com/en-us/azure/foundry/agents/quickstarts/quickstart-hosted-agent) — end-to-end walkthrough using `azd`
- [Tool catalog](https://learn.microsoft.com/en-us/azure/foundry/agents/concepts/tool-catalog) — browse available tools to extend your agent (Bing Search, Azure AI Search, file search, code interpreter, and more)
- [Manage hosted agents](https://learn.microsoft.com/en-us/azure/foundry/agents/how-to/manage-hosted-agent) — monitor and manage deployed agents
- [Add tools to your agent](https://github.com/microsoft-foundry/foundry-samples/tree/3d734b93b66f163bea9886d73c6808adc32e68fc/samples/python/hosted-agents/agent-framework/responses/02-tools/) — sample with local tool functions
- [Use Foundry Toolbox](https://github.com/microsoft-foundry/foundry-samples/tree/3d734b93b66f163bea9886d73c6808adc32e68fc/samples/python/hosted-agents/agent-framework/responses/04-foundry-toolbox/) — sample with Azure Foundry Toolbox integration
