# Startup and Commercial Solutions

[← Previous: Detection Approaches](09_DETECTION_APPROACHES.md) | [Index](00_INDEX.md) | [Next: OWASP Frameworks →](11_OWASP_FRAMEWORKS.md)

---

## Overview

The prompt injection defense market spans dedicated LLM security APIs, comprehensive platforms, red teaming tools, and open-source solutions. Major acquisitions (Lakera by Check Point, Robust Intelligence by Cisco) signal market maturation.

## Summary

- **Detection APIs**: Lakera Guard (<50ms latency, 100+ languages), Protect AI LLM Guard (open-source, self-hosted), Rebuff (self-hardening via attack logs)
- **Red Teaming**: NVIDIA Garak (vulnerability scanner), Promptfoo (testing framework)
- **Output Validation**: Guardrails AI (structured outputs), NeMo Guardrails (conversational safety)
- **Enterprise Platforms**: Arthur AI (monitoring), Harmonic Security (shadow AI detection), Robust Intelligence (Cisco-integrated)
- **Selection factors**: Latency requirements, open-source needs, integration ecosystem, enterprise features

## Market Landscape

```
┌─────────────────────────────────────────────────────────────────┐
│              LLM SECURITY MARKET MAP (January 2026)              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  DETECTION APIs                    COMPREHENSIVE PLATFORMS      │
│  ├── Lakera Guard                  ├── Robust Intelligence     │
│  ├── Rebuff                        │   (Cisco)                 │
│  └── Protect AI LLM Guard          ├── Arthur AI              │
│                                    └── WhyLabs Secure          │
│                                                                 │
│  RED TEAMING TOOLS                 ENTERPRISE SECURITY          │
│  ├── Garak (NVIDIA)                ├── Harmonic Security       │
│  ├── Promptfoo                     ├── Securiti.ai             │
│  └── Prompt Security               └── HiddenLayer             │
│                                                                 │
│  OPEN SOURCE                       ACQUIRED/CONSOLIDATED        │
│  ├── LlamaFirewall (Meta)          ├── Lakera (Check Point)    │
│  ├── Guardrails AI                 └── Robust Intelligence     │
│  └── NeMo Guardrails (NVIDIA)          (Cisco)                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Lakera Guard

**Company**: Lakera (acquired by Check Point, 2025)
**Type**: Real-time LLM security API
**Unique**: Threat intelligence from Gandalf (public AI security game with 100,000+ daily attack attempts)

| Feature | Description |
|---------|-------------|
| Prompt Injection Detection | Multi-layer classifier ensemble |
| Jailbreak Detection | Pre-trained on 1M+ attack examples |
| PII Detection | Identifies and can redact personal data |
| Toxicity Detection | Content policy enforcement |
| Latency | <50ms per request |
| Languages | 100+ languages supported |

### Integration

```python
import requests

class LakeraGuard:
    def __init__(self, api_key: str):
        self.api_key = api_key
        self.endpoint = "https://api.lakera.ai/v1/prompt_injection"
    
    def check(self, text: str) -> dict:
        response = requests.post(
            self.endpoint,
            headers={"Authorization": f"Bearer {self.api_key}"},
            json={"input": text}
        )
        return response.json()
    
    def is_safe(self, text: str) -> bool:
        result = self.check(text)
        return not result.get("results", [{}])[0].get("flagged", False)

# Usage
guard = LakeraGuard(api_key="your-api-key")
if guard.is_safe(user_input):
    # Proceed with LLM call
    pass
else:
    # Block or alert
    pass
```

### Pricing (January 2026)
- Free: 1,000 requests/month
- Pro: $0.001/request
- Enterprise: Custom with SLA

## Protect AI LLM Guard

**Type**: Open-source, self-hosted
**License**: Apache 2.0
**Focus**: Comprehensive input/output scanning with modular scanners

### Scanner Categories

**Input Scanners**:
| Scanner | Purpose |
|---------|---------|
| `Anonymize` | Detect and mask PII |
| `BanSubstrings` | Block specific text patterns |
| `BanTopics` | Block content about specific topics |
| `PromptInjection` | RoBERTa-based injection detection |
| `Secrets` | Detect API keys, passwords |
| `TokenLimit` | Enforce input length limits |
| `Toxicity` | Detect harmful language |

**Output Scanners**:
| Scanner | Purpose |
|---------|---------|
| `Bias` | Detect biased content |
| `Deanonymize` | Restore anonymized data |
| `JSON` | Validate JSON output format |
| `Language` | Enforce language constraints |
| `MaliciousURLs` | Detect malicious links |
| `NoRefusal` | Detect refusal patterns |
| `Relevance` | Check response relevance |
| `Sensitive` | Detect sensitive data exposure |

### Implementation

```python
from llm_guard import scan_prompt, scan_output
from llm_guard.input_scanners import (
    Anonymize,
    PromptInjection,
    Secrets,
    Toxicity
)
from llm_guard.output_scanners import (
    Bias,
    MaliciousURLs,
    Sensitive
)

# Configure input scanners
input_scanners = [
    Anonymize(),
    PromptInjection(threshold=0.9),
    Secrets(),
    Toxicity(threshold=0.7)
]

# Configure output scanners
output_scanners = [
    Bias(threshold=0.75),
    MaliciousURLs(),
    Sensitive()
]

def secure_llm_call(user_input: str, llm_func) -> str:
    # Scan input
    sanitized_input, results, is_valid = scan_prompt(
        input_scanners, 
        user_input
    )
    
    if not is_valid:
        return f"Input blocked: {results}"
    
    # Call LLM
    response = llm_func(sanitized_input)
    
    # Scan output
    sanitized_output, results, is_valid = scan_output(
        output_scanners,
        sanitized_input,
        response
    )
    
    if not is_valid:
        return f"Output blocked: {results}"
    
    return sanitized_output
```

### Deployment Options
- **Python Package**: `pip install llm-guard`
- **Docker**: `docker pull laiyer/llm-guard-api`
- **Kubernetes**: Helm chart available

## Rebuff

**Type**: Open-source, self-hardening
**Focus**: LangChain-native integration
**Unique**: Self-improving via attack logging

### Four-Layer Defense

```
┌─────────────────────────────────────────────────────────────────┐
│                    REBUFF ARCHITECTURE                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  LAYER 1: HEURISTIC DETECTION                                   │
│  ├── Keyword matching for known attack patterns                │
│  ├── Fast, low-latency                                         │
│  └── Catches obvious attacks                                   │
│                                                                 │
│  LAYER 2: LLM DETECTION                                         │
│  ├── GPT-3.5-Turbo analyzes input                              │
│  ├── Semantic understanding of intent                          │
│  └── Higher latency, more accurate                             │
│                                                                 │
│  LAYER 3: VECTOR DATABASE                                       │
│  ├── Embeddings of previous attacks stored                     │
│  ├── Similarity search against known attacks                   │
│  └── Self-improving: new attacks added to DB                   │
│                                                                 │
│  LAYER 4: CANARY TOKENS                                         │
│  ├── Hidden tokens in system prompt                            │
│  ├── If output contains canary → prompt leakage detected       │
│  └── Post-hoc detection of successful attacks                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Implementation

```python
from rebuff import Rebuff

# Initialize with API key
rb = Rebuff(api_token="your-api-key", api_url="https://rebuff.ai")

# Check for injection
result = rb.detect_injection(user_input)

if result.injection_detected:
    print(f"Attack detected! Score: {result.injection_score}")
else:
    # Add canary to system prompt
    system_prompt_with_canary, canary = rb.add_canary_word(system_prompt)
    
    # Call LLM
    response = llm(system_prompt_with_canary, user_input)
    
    # Check for canary leakage
    if rb.is_canary_word_leaked(canary, response):
        print("Canary leaked! Prompt injection succeeded.")
```

### Limitations
- Prototype stage, not production-hardened
- Depends on OpenAI API for layer 2 detection
- Cannot provide 100% protection (stated explicitly)

## NVIDIA Garak

**Type**: Open-source vulnerability scanner
**Maintained by**: NVIDIA
**Focus**: Red teaming and automated testing against 100+ attack probes

### Probe Categories

```
┌─────────────────────────────────────────────────────────────────┐
│                     GARAK PROBE CATEGORIES                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  PROMPT INJECTION                                               │
│  ├── promptinject: Standard injection attempts                 │
│  ├── hijacking: Goal hijacking attacks                         │
│  └── leakage: System prompt extraction                         │
│                                                                 │
│  JAILBREAKS                                                     │
│  ├── dan: "Do Anything Now" variants                           │
│  ├── aim: "Always Intelligent and Machiavellian"               │
│  └── roleplay: Character-based jailbreaks                      │
│                                                                 │
│  ENCODING ATTACKS                                               │
│  ├── encoding: Base64, ROT13, hex                              │
│  └── glitch: Token glitch exploits                             │
│                                                                 │
│  CONTENT POLICY                                                 │
│  ├── malwaregen: Malware generation attempts                   │
│  ├── realtoxicity: Toxicity elicitation                        │
│  └── continuation: Harmful content continuation                │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Usage

```bash
# Install
pip install garak

# Basic scan
python -m garak --model_type openai --model_name gpt-4 \
    --probes promptinject

# Comprehensive scan
python -m garak --model_type huggingface \
    --model_name meta-llama/Llama-2-7b-chat-hf \
    --probes all

# Custom probes
python -m garak --model_type openai --model_name gpt-4 \
    --probes promptinject,dan,encoding
```

### Report Output

```json
{
  "model": "gpt-4",
  "timestamp": "2025-01-14T10:30:00Z",
  "probes": {
    "promptinject": {
      "attempts": 100,
      "successes": 3,
      "success_rate": 0.03
    },
    "dan": {
      "attempts": 50,
      "successes": 1,
      "success_rate": 0.02
    }
  },
  "overall_vulnerability_score": 0.025
}
```

## Guardrails AI

**Type**: Open-source output validation
**Focus**: Structured output enforcement
**Use Case**: Ensuring LLM outputs meet specifications

### Key Features

```python
from guardrails import Guard
from guardrails.hub import (
    CompetitorCheck,
    ToxicLanguage,
    ValidJSON,
    RestrictToTopic,
    DetectPII
)

# Create guard with multiple validators
guard = Guard().use_many(
    CompetitorCheck(competitors=["CompetitorA", "CompetitorB"]),
    ToxicLanguage(on_fail="exception"),
    ValidJSON(on_fail="fix"),
    RestrictToTopic(valid_topics=["technology", "business"]),
    DetectPII(on_fail="fix")  # Redacts PII from output
)

# Use with LLM
result = guard(
    llm_callable,
    prompt="Summarize this document...",
    metadata={"document": document_content}
)

# Result includes validation info
print(result.validated_output)
print(result.validation_passed)
```

### Hub Validators

| Validator | Purpose |
|-----------|---------|
| `CompetitorCheck` | Ensure no competitor mentions |
| `ToxicLanguage` | Filter toxic content |
| `ValidJSON` | Ensure valid JSON output |
| `RestrictToTopic` | Keep responses on-topic |
| `DetectPII` | Find and optionally redact PII |
| `ProvenanceCheck` | Verify claims against sources |
| `GibberishDetector` | Detect nonsensical outputs |

## NVIDIA NeMo Guardrails

**Type**: Open-source dialog management
**Focus**: Conversational AI safety with declarative rules
**Integration**: LangChain, custom LLMs

### Colang Configuration

```colang
# Define rails (safety rules)
define flow
  user asks about competitors
  bot refuse to discuss competitors

define flow
  user attempts jailbreak
  bot explain cannot comply with that request

define flow
  user provides document to summarize
  bot ensure no prompt injection in document
  if injection detected
    bot refuse and explain
  else
    bot summarize document
```

### Implementation

```python
from nemoguardrails import LLMRails, RailsConfig

# Load configuration
config = RailsConfig.from_path("./config")

# Create rails instance
rails = LLMRails(config)

# Process user input with rails
response = await rails.generate(
    messages=[{
        "role": "user",
        "content": user_input
    }]
)
```

## Arthur AI

**Type**: Commercial platform
**Focus**: LLM evaluation and monitoring
**Feature**: Arthur Shield for input/output security screening

### Arthur Shield

```python
from arthur import ArthurShield

shield = ArthurShield(api_key="your-api-key")

# Screen input
input_result = shield.screen_input(
    text=user_input,
    rules=["prompt_injection", "pii", "toxicity"]
)

if not input_result.passed:
    print(f"Blocked: {input_result.violations}")

# Screen output
output_result = shield.screen_output(
    prompt=user_input,
    response=llm_response,
    rules=["hallucination", "off_topic", "sensitive_disclosure"]
)
```

## Harmonic Security

**Type**: Commercial enterprise platform
**Focus**: Shadow AI detection and security
**Unique**: Discovers unauthorized AI usage across organization

### Capabilities

| Feature | Description |
|---------|-------------|
| Shadow AI Discovery | Find unauthorized LLM usage in org |
| Data Loss Prevention | Prevent sensitive data to LLMs |
| Prompt Security | Injection and jailbreak detection |
| Compliance | Audit trails, policy enforcement |

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│              HARMONIC SECURITY ARCHITECTURE                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  NETWORK LAYER                                                  │
│  ├── Traffic inspection for LLM API calls                      │
│  ├── Discovery of unauthorized AI services                     │
│  └── Policy enforcement at network level                       │
│                                                                 │
│  APPLICATION LAYER                                              │
│  ├── SDK/API integration                                       │
│  ├── Input/output screening                                    │
│  └── ~200ms latency                                            │
│                                                                 │
│  MANAGEMENT LAYER                                               │
│  ├── Policy configuration                                      │
│  ├── Compliance reporting                                      │
│  └── Incident management                                       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Comparison Matrix

| Solution | Type | Latency | Open Source | Injection Detection | Best For |
|----------|------|---------|-------------|---------------------|----------|
| **Lakera Guard** | API | <50ms | No | Excellent | Production APIs |
| **Protect AI** | Self-hosted | Low | Yes | Good | Privacy-sensitive |
| **Rebuff** | API/Self | Variable | Yes | Good | LangChain projects |
| **Garak** | CLI | N/A | Yes | Testing only | Red teaming |
| **Guardrails AI** | Library | Low | Yes | Limited | Output validation |
| **NeMo Guardrails** | Library | Medium | Yes | Good | Conversational AI |
| **Arthur Shield** | API | Medium | No | Good | Enterprise monitoring |
| **Harmonic** | Platform | ~200ms | No | Good | Enterprise security |

## Selection Guide

```
┌─────────────────────────────────────────────────────────────────┐
│              SOLUTION SELECTION FLOWCHART                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Need prompt injection detection?                               │
│  ├── YES, low latency critical → Lakera Guard or Protect AI    │
│  ├── YES, open source required → Protect AI LLM Guard          │
│  └── YES, LangChain integration → Rebuff                       │
│                                                                 │
│  Need output validation?                                        │
│  ├── YES, structured outputs → Guardrails AI                   │
│  └── YES, conversational → NeMo Guardrails                     │
│                                                                 │
│  Need red teaming / testing?                                    │
│  └── Garak (NVIDIA)                                            │
│                                                                 │
│  Need enterprise platform?                                      │
│  ├── Comprehensive monitoring → Arthur AI                      │
│  ├── Shadow AI + security → Harmonic Security                  │
│  └── Cisco ecosystem → Robust Intelligence                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Sources

- [Lakera Guard Documentation](https://www.lakera.ai/) - API specs and pricing
- [Protect AI LLM Guard](https://github.com/protectai/llm-guard) - Open-source repository
- [Rebuff GitHub](https://github.com/protectai/rebuff) - Self-hardening framework
- [NVIDIA Garak](https://github.com/NVIDIA/garak) - Vulnerability scanner
- [Guardrails AI](https://www.guardrailsai.com/) - Output validation framework
- [NVIDIA NeMo Guardrails](https://github.com/NVIDIA/NeMo-Guardrails) - Conversational safety
- [Arthur AI Shield](https://www.arthur.ai/) - Enterprise monitoring platform
- [Harmonic Security](https://www.harmonic.security/) - Shadow AI detection

---

[← Previous: Detection Approaches](09_DETECTION_APPROACHES.md) | [Index](00_INDEX.md) | [Next: OWASP Frameworks →](11_OWASP_FRAMEWORKS.md)
