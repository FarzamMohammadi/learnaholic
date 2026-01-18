# Agentic Security

[← Back to Index](00_INDEX.md) | [Previous: Output Defenses](14_OUTPUT_DEFENSES.md) | [Next: Human-in-the-Loop →](16_HUMAN_IN_THE_LOOP.md)

---

## Overview

Agentic AI systems—those that can take actions in the world through tools, APIs, and autonomous operation—represent the highest-risk category for prompt injection. When an LLM can send emails, modify files, execute code, or make API calls, successful injection can cause real-world harm.

## Summary

- **Lethal Trifecta**: Never combine all three of private data access, untrusted input, and external actions
- **Tool Security**: Implement capability-based access control, input validation, and user confirmation for sensitive operations
- **MCP Hardening**: Display exact commands, sandbox execution, require explicit authorization, and log everything
- **Multi-Agent Trust**: Treat all inter-agent communication as untrusted; validate at boundaries
- **Code Execution**: Use strong sandboxing (Firecracker, gVisor, Docker) with resource limits and network isolation

---

## The Lethal Trifecta

### Core Concept

Simon Willison's "Lethal Trifecta" identifies the three capabilities that, when combined, create perfect conditions for prompt injection exploitation:

```
┌─────────────────────────────────────────────────────────────────┐
│                    THE LETHAL TRIFECTA                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│     ┌─────────────────┐                                        │
│     │   1. ACCESS TO  │                                        │
│     │  PRIVATE DATA   │                                        │
│     │                 │                                        │
│     │ Files, emails,  │                                        │
│     │ databases,      │                                        │
│     │ credentials     │                                        │
│     └────────┬────────┘                                        │
│              │                                                  │
│              │     ┌─────────────────┐                         │
│              │     │ 2. EXPOSURE TO  │                         │
│              ├─────│ UNTRUSTED INPUT │                         │
│              │     │                 │                         │
│              │     │ Web pages,      │                         │
│              │     │ documents,      │                         │
│              │     │ user uploads    │                         │
│              │     └────────┬────────┘                         │
│              │              │                                   │
│              │              │     ┌─────────────────┐          │
│              │              │     │ 3. ABILITY TO   │          │
│              └──────────────┴─────│ ACT EXTERNALLY  │          │
│                                   │                 │          │
│                                   │ Send emails,    │          │
│                                   │ API calls,      │          │
│                                   │ publish content │          │
│                                   └─────────────────┘          │
│                                                                 │
│  WHEN ALL THREE COMBINE:                                        │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Attacker injects payload in untrusted content (2)      │   │
│  │  LLM reads private data (1)                              │   │
│  │  LLM exfiltrates data via external action (3)           │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### The Rule of Two

**Never satisfy all three legs of the trifecta.** A secure system should have at most TWO of these capabilities:

| Combination | Example | Risk Level |
|-------------|---------|------------|
| Private Data + Untrusted Input | RAG system (read-only) | **Manageable** |
| Private Data + External Actions | Automation (no external input) | **Manageable** |
| Untrusted Input + External Actions | Public chatbot with tools | **Manageable** |
| ALL THREE | Email assistant with web access | **DANGEROUS** |

### Architectural Solutions

```python
class SecureAgentArchitecture:
    """Architectural patterns that avoid the Lethal Trifecta."""

    @staticmethod
    def pattern_1_read_only():
        """
        Pattern: Private Data + Untrusted Input (NO external actions)
        Use case: RAG system that answers questions about internal docs
        """
        
        class ReadOnlyRAG:
            def __init__(self, document_store):
                self.docs = document_store

            def query(self, user_question: str) -> str:
                relevant_docs = self.docs.search(user_question)
                return self.llm.generate(
                    f"Answer based on docs: {relevant_docs}\n"
                    f"Question: {user_question}",
                    tools=[]  # NO TOOLS - cannot take external actions
                )
    
    @staticmethod
    def pattern_2_isolated_automation():
        """
        Pattern: Private Data + External Actions (NO untrusted input)
        Use case: Scheduled report generator
        """
        class IsolatedAutomation:
            def generate_weekly_report(self):
                metrics = self.db.get_weekly_metrics()
                report = self.llm.generate(
                    f"Generate report for metrics: {metrics}"
                    # No untrusted input - no injection vector
                )
                self.email.send(report, to="team@company.com")
    
    @staticmethod
    def pattern_3_public_assistant():
        """
        Pattern: Untrusted Input + External Actions (NO private data)
        Use case: Public-facing assistant that can search/book
        """
        class PublicAssistant:
            def handle_request(self, user_input: str):
                return self.llm.generate(
                    user_input,
                    tools=[self.public_search, self.booking_api]
                    # No private data access - nothing sensitive to exfiltrate
                )
```

---

## Tool-Use Security

### Principle of Least Privilege

```python
class SecureToolRegistry:
    """Tool registry with capability-based access control."""

    def __init__(self):
        self.tools = {}
        self.capability_levels = {
            "read_only": 0,
            "write_local": 1,
            "external_read": 2,
            "external_write": 3,
            "admin": 4
        }
    
    def register_tool(self, 
                      name: str, 
                      func: callable,
                      required_capability: str,
                      description: str,
                      requires_confirmation: bool = False):
        
        self.tools[name] = {
            "func": func,
            "capability": required_capability,
            "description": description,
            "requires_confirmation": requires_confirmation
        }
    
    def get_tools_for_context(self,
                               context: dict,
                               max_capability: str = "read_only") -> list:
        """Return only tools appropriate for the context."""
        max_level = self.capability_levels[max_capability]
        
        available = []
        for name, tool in self.tools.items():
            tool_level = self.capability_levels[tool["capability"]]
            
            if tool_level <= max_level:
                available.append({
                    "name": name,
                    "description": tool["description"],
                    "requires_confirmation": tool["requires_confirmation"]
                })
        
        return available


# Example tool registration
registry = SecureToolRegistry()

# Low privilege tools
registry.register_tool(
    "search_docs",
    search_documents,
    required_capability="read_only",
    description="Search internal documentation"
)

# Medium privilege tools
registry.register_tool(
    "write_file",
    write_to_file,
    required_capability="write_local",
    description="Write to local file system",
    requires_confirmation=True
)

# High privilege tools (require confirmation)
registry.register_tool(
    "send_email",
    send_email,
    required_capability="external_write",
    description="Send email externally",
    requires_confirmation=True  # ALWAYS require confirmation
)
```

### Tool Input Validation

```python
from pydantic import BaseModel, validator
from typing import Optional

class EmailToolInput(BaseModel):
    """Validated input for email sending tool."""
    
    to: str
    subject: str
    body: str
    cc: Optional[list] = None
    
    @validator('to')
    def validate_recipient(cls, v):
        # Only allow internal addresses by default
        if not v.endswith('@company.com'):
            raise ValueError("Can only send to internal addresses")
        return v
    
    @validator('body')
    def check_body_length(cls, v):
        if len(v) > 10000:
            raise ValueError("Email body too long")
        return v
    
    @validator('subject')
    def no_injection_in_subject(cls, v):
        suspicious = ['password', 'credential', 'secret', 'api key']
        if any(s in v.lower() for s in suspicious):
            raise ValueError("Subject contains sensitive terms")
        return v


def secure_email_tool(input_data: dict) -> dict:
    """Email tool with input validation."""
    
    try:
        validated = EmailToolInput(**input_data)
    except ValueError as e:
        return {"error": f"Invalid input: {e}"}
    
    # Additional checks
    # ... 
    
    return send_email(validated.dict())
```

---

## MCP (Model Context Protocol) Hardening

### Security Principles for MCP

```
┌─────────────────────────────────────────────────────────────────┐
│              MCP SECURITY PRINCIPLES                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. DISPLAY EXACT COMMANDS                                      │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Never hide what the LLM is requesting to execute       │   │
│  │  Show users: tool name, all parameters, expected effect │   │
│  │  Users must see actual values, not summaries            │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  2. SANDBOX ALL EXECUTION                                       │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  • Filesystem isolation (containers, chroot)            │   │
│  │  • Network restrictions (allowlist only)                │   │
│  │  • Resource limits (CPU, memory, time)                  │   │
│  │  • No access to host system                             │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  3. REQUIRE EXPLICIT AUTHORIZATION                              │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  • Per-tool authorization tokens                        │   │
│  │  • Scoped capabilities (read vs write)                  │   │
│  │  • Time-limited access                                  │   │
│  │  • Human approval for sensitive operations              │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  4. LOG EVERYTHING                                              │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  • All tool invocations                                 │   │
│  │  • Parameters passed                                    │   │
│  │  • Results returned                                     │   │
│  │  • Approval/denial decisions                            │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Secure MCP Server Implementation

```python
from typing import Any, Dict
import logging
import hashlib
import time

class SecureMCPServer:
    """Security-hardened MCP server implementation."""

    def __init__(self, config: dict):
        self.logger = logging.getLogger("mcp_security")
        self.tool_registry = {}
        self.authorization_tokens = {}
        self.rate_limiter = RateLimiter()
        
        # Configuration
        self.require_confirmation = config.get("require_confirmation", True)
        self.sandbox_enabled = config.get("sandbox_enabled", True)
        self.allowed_tools = config.get("allowed_tools", [])
    
    def register_tool(self, 
                      name: str, 
                      handler: callable,
                      risk_level: str,
                      requires_confirmation: bool = None):
        """Register a tool with security metadata."""
        
        self.tool_registry[name] = {
            "handler": handler,
            "risk_level": risk_level,
            "requires_confirmation": requires_confirmation or (risk_level in ["high", "critical"]),
            "invocation_count": 0
        }
    
    async def handle_tool_call(self,
                                tool_name: str,
                                params: Dict[str, Any],
                                context: dict) -> dict:
        """Handle tool call with full security checks."""
        
        # 1. Check if tool is registered and allowed
        if tool_name not in self.tool_registry:
            return {"error": f"Unknown tool: {tool_name}"}
        
        if self.allowed_tools and tool_name not in self.allowed_tools:
            return {"error": f"Tool not allowed: {tool_name}"}
        
        tool = self.tool_registry[tool_name]
        
        # 2. Rate limiting
        if not self.rate_limiter.allow(context.get("session_id")):
            return {"error": "Rate limit exceeded"}
        
        # 3. Authorization check
        if not self._check_authorization(tool_name, context):
            return {"error": "Not authorized for this tool"}
        
        # 4. Parameter validation
        validation = self._validate_params(tool_name, params)
        if not validation["valid"]:
            return {"error": f"Invalid parameters: {validation['errors']}"}
        
        # 5. Confirmation requirement
        if tool["requires_confirmation"]:
            if not context.get("user_confirmed"):
                return {
                    "status": "confirmation_required",
                    "tool": tool_name,
                    "params": self._sanitize_params_for_display(params),
                    "risk_level": tool["risk_level"],
                    "message": f"Please confirm execution of {tool_name}"
                }
        
        # 6. Log the invocation
        self._log_invocation(tool_name, params, context)
        
        # 7. Execute in sandbox if enabled
        if self.sandbox_enabled:
            result = await self._execute_sandboxed(tool, params)
        else:
            result = await tool["handler"](params)
        
        # 8. Log the result
        self._log_result(tool_name, result, context)
        
        return result
    
    def _check_authorization(self, tool_name: str, context: dict) -> bool:
        """Check if the request is authorized for this tool."""
        token = context.get("auth_token")
        
        if not token:
            return False
        
        # Validate token and check scope
        token_info = self.authorization_tokens.get(token)
        if not token_info:
            return False
        
        if token_info.get("expires_at", 0) < time.time():
            return False
        
        allowed_tools = token_info.get("allowed_tools", [])
        if "*" in allowed_tools or tool_name in allowed_tools:
            return True
        
        return False
    
    def _validate_params(self, tool_name: str, params: dict) -> dict:
        """Validate tool parameters."""
        # Tool-specific validation would go here
        return {"valid": True, "errors": []}
    
    def _sanitize_params_for_display(self, params: dict) -> dict:
        """Sanitize parameters for user display (hide secrets)."""
        sanitized = {}
        
        sensitive_keys = ["password", "token", "key", "secret", "credential"]
        
        for key, value in params.items():
            if any(s in key.lower() for s in sensitive_keys):
                sanitized[key] = "****"
            elif isinstance(value, str) and len(value) > 100:
                sanitized[key] = value[:100] + "..."
            else:
                sanitized[key] = value
        
        return sanitized
    
    def _log_invocation(self, tool_name: str, params: dict, context: dict):
        """Log tool invocation for audit."""
        self.logger.info({
            "event": "tool_invocation",
            "tool": tool_name,
            "params_hash": hashlib.sha256(str(params).encode()).hexdigest()[:16],
            "session_id": context.get("session_id"),
            "user_id": context.get("user_id"),
            "timestamp": time.time()
        })
    
    def _log_result(self, tool_name: str, result: dict, context: dict):
        """Log tool result for audit."""
        self.logger.info({
            "event": "tool_result",
            "tool": tool_name,
            "success": "error" not in result,
            "session_id": context.get("session_id"),
            "timestamp": time.time()
        })
    
    async def _execute_sandboxed(self, tool: dict, params: dict) -> dict:
        """Execute tool in sandboxed environment."""
        # Implementation depends on sandboxing technology
        # Options: Docker, Firecracker, gVisor, etc.
        pass
```

---

## Multi-Agent Trust Boundaries

### The Inter-Agent Injection Problem

```
┌─────────────────────────────────────────────────────────────────┐
│              INTER-AGENT INJECTION ATTACK                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  TRADITIONAL TRUST MODEL (VULNERABLE):                          │
│                                                                 │
│  Agent A ──trusted──▶ Agent B ──trusted──▶ Agent C             │
│     │                    │                    │                 │
│     │                    │                    │                 │
│  If Agent A is          Trusts A's           Trusts B's        │
│  compromised...         output               output            │
│                         (now poisoned)       (now poisoned)    │
│                                                                 │
│  Result: 100% vulnerability rate via inter-agent trust         │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ZERO-TRUST MODEL (SECURE):                                     │
│                                                                 │
│  Agent A ──untrusted──▶ Validator ──▶ Agent B                  │
│     │                       │              │                    │
│     │                       │              │                    │
│  Potentially            Validates        Only receives         │
│  compromised            all outputs      sanitized data        │
│                         before passing                         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Zero-Trust Multi-Agent Architecture

```python
class ZeroTrustAgentOrchestrator:
    """Orchestrator that implements zero-trust between agents."""

    def __init__(self):
        self.agents = {}
        self.validator = OutputValidator()
        self.policy_engine = PolicyEngine()
    
    def register_agent(self, agent_id: str, agent: object, capabilities: list):
        """Register an agent with its declared capabilities."""
        self.agents[agent_id] = {
            "instance": agent,
            "capabilities": capabilities,
            "trust_level": "untrusted"  # All agents are untrusted by default
        }
    
    async def agent_to_agent_call(self,
                                   source_agent: str,
                                   target_agent: str,
                                   message: str,
                                   context: dict) -> dict:
        """Handle communication between agents with validation."""
        
        # 1. Validate the source agent's message
        validation = self.validator.validate(message)
        if not validation["passed"]:
            return {
                "blocked": True,
                "reason": "Source agent output failed validation",
                "details": validation
            }
        
        # 2. Check policy for this agent-to-agent communication
        policy_check = self.policy_engine.check(
            source=source_agent,
            target=target_agent,
            action="communicate",
            content_type=self._classify_content(message)
        )
        
        if not policy_check["allowed"]:
            return {
                "blocked": True,
                "reason": "Policy violation",
                "policy": policy_check["violated_policy"]
            }
        
        # 3. Sanitize the message
        sanitized_message = self.validator.sanitize(message)
        
        # 4. Mark the message as coming from untrusted source
        wrapped_message = {
            "content": sanitized_message,
            "source": source_agent,
            "trust_level": "untrusted",
            "timestamp": time.time()
        }
        
        # 5. Deliver to target agent with trust context
        target = self.agents[target_agent]["instance"]
        result = await target.receive_message(
            wrapped_message,
            sender_trust="untrusted"
        )
        
        return result
    
    def _classify_content(self, message: str) -> str:
        """Classify message content for policy evaluation."""
        if any(word in message.lower() for word in ['password', 'secret', 'key']):
            return "sensitive"
        if any(word in message.lower() for word in ['delete', 'remove', 'modify']):
            return "destructive"
        return "general"
```

---

## Code Execution Security

### Sandboxing Options

| Technology | Isolation Level | Overhead | Best For |
|------------|-----------------|----------|----------|
| **Docker** | Process/Namespace | Low | General isolation |
| **gVisor** | Syscall interception | Medium | Stronger isolation |
| **Firecracker** | microVM | Medium | Highest security |
| **WASM** | In-process sandbox | Very Low | Lightweight code |
| **Pyodide** | Browser sandbox | Low | Python in browser |

### Firecracker Sandbox Pattern

```python
import json
import subprocess
from typing import Dict, Any

class FirecrackerSandbox:
    """Execute code in Firecracker microVM for maximum isolation."""

    def __init__(self, config: dict):
        self.timeout = config.get("timeout", 30)
        self.memory_mb = config.get("memory_mb", 128)
        self.vcpu_count = config.get("vcpu_count", 1)
        self.network_enabled = config.get("network", False)
    
    async def execute_code(self,
                           code: str,
                           language: str,
                           inputs: Dict[str, Any] = None) -> dict:
        """Execute code in isolated microVM."""
        
        # 1. Create microVM configuration
        vm_config = {
            "boot-source": {
                "kernel_image_path": "/path/to/vmlinux",
                "boot_args": "console=ttyS0 reboot=k panic=1"
            },
            "drives": [{
                "drive_id": "rootfs",
                "path_on_host": "/path/to/rootfs.ext4",
                "is_root_device": True,
                "is_read_only": True
            }],
            "machine-config": {
                "vcpu_count": self.vcpu_count,
                "mem_size_mib": self.memory_mb
            },
            "network-interfaces": [] if not self.network_enabled else [
                {"iface_id": "eth0", "host_dev_name": "tap0"}
            ]
        }
        
        # 2. Write code to temporary file for VM
        code_payload = {
            "code": code,
            "language": language,
            "inputs": inputs or {}
        }
        
        # 3. Start microVM and execute
        try:
            result = await self._run_in_vm(vm_config, code_payload)
            return {
                "success": True,
                "output": result["stdout"],
                "stderr": result["stderr"],
                "exit_code": result["exit_code"]
            }
        except subprocess.TimeoutExpired:
            return {
                "success": False,
                "error": "Execution timed out",
                "timeout": self.timeout
            }
        except Exception as e:
            return {
                "success": False,
                "error": str(e)
            }
    
    async def _run_in_vm(self, vm_config: dict, payload: dict) -> dict:
        """Actually run the code in microVM."""
        # Implementation details for Firecracker API
        pass
```

---

## Key Takeaways

1. **Never satisfy all three legs of the Lethal Trifecta** - Systems with private data access, untrusted input, and external actions create perfect conditions for exploitation
2. **Require human confirmation for sensitive actions** - Email sending, file deletion, and API calls with side effects need explicit approval
3. **Apply least privilege to all tool access** - Tools should have minimum necessary capabilities; use scoped permissions and time limits
4. **Treat all inter-agent communication as untrusted** - Validate and sanitize at every boundary; never assume agent output is safe
5. **Sandbox all code execution** - Use strong isolation (Firecracker, gVisor, Docker) with resource limits and network restrictions
6. **Log everything for audit and incident response** - Comprehensive logging enables detection and post-incident analysis

### Risk Assessment Matrix

| Capability Combination | Risk Level | Recommendation |
|------------------------|------------|----------------|
| Read-only + No external | Low | Standard precautions |
| Read-only + Untrusted input | Medium | Input validation |
| Write + No untrusted input | Medium | Confirmation for writes |
| Write + Untrusted input | High | Strong validation + HITL |
| External comms + Private data | High | Architectural separation |
| All three (Lethal Trifecta) | **Critical** | **Redesign architecture** |

## Sources

- [Simon Willison - Prompt Injection Explained](https://simonwillison.net/2023/Apr/14/worst-that-can-happen/) - Lethal Trifecta concept
- [Anthropic Model Context Protocol](https://modelcontextprotocol.io/) - MCP specification and security considerations
- [Firecracker](https://firecracker-microvm.github.io/) - Secure microVM sandboxing

---

[← Back to Index](00_INDEX.md) | [Previous: Output Defenses](14_OUTPUT_DEFENSES.md) | [Next: Human-in-the-Loop →](16_HUMAN_IN_THE_LOOP.md)
