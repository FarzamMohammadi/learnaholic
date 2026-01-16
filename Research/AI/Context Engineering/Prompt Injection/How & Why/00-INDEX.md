# Prompt Injection: How and Why It Works

[Next →](01-FUNDAMENTALS.md)

---

## Overview

This series explores prompt injection attack mechanisms, transformer vulnerabilities, and why LLMs cannot architecturally distinguish instructions from data—prerequisite knowledge for understanding prevention strategies.

## Summary

- 22 modular documents covering fundamentals, attack taxonomy, technical mechanisms, historical context, and evaluation frameworks
- Addresses the core problem: transformers process all inputs (system prompts, user messages, external data) through identical attention mechanisms
- Synthesizes academic research, industry publications, security findings, and real-world incidents from 2022-2026

---

## Document Structure

### Core Foundations
| File | Description |
|------|-------------|
| [01-FUNDAMENTALS.md](./01-FUNDAMENTALS.md) | The architectural root cause—why LLMs cannot distinguish instructions from data |
| [02-ATTENTION-MECHANISMS.md](./02-ATTENTION-MECHANISMS.md) | How transformer attention creates exploitable vulnerabilities |
| [03-INSTRUCTION-TUNING-VULNERABILITY.md](./03-INSTRUCTION-TUNING-VULNERABILITY.md) | Why RLHF and instruction-following training creates attack surfaces |

### Attack Taxonomy (Complete Classification)
| File | Description |
|------|-------------|
| [04-TAXONOMY-OVERVIEW.md](./04-TAXONOMY-OVERVIEW.md) | Master classification of all prompt injection attack types |
| [05-DIRECT-INJECTION.md](./05-DIRECT-INJECTION.md) | User-initiated attacks targeting the model interface directly |
| [06-INDIRECT-INJECTION.md](./06-INDIRECT-INJECTION.md) | Attacks embedded in external content (documents, web, emails) |
| [07-JAILBREAKING.md](./07-JAILBREAKING.md) | Safety guardrail bypass techniques and social engineering |
| [08-ADVERSARIAL-SUFFIXES.md](./08-ADVERSARIAL-SUFFIXES.md) | Gradient-optimized token sequences (GCG, AutoDAN, AmpleGCG) |
| [09-MULTI-TURN-ATTACKS.md](./09-MULTI-TURN-ATTACKS.md) | Conversation-based attacks (Crescendo, context manipulation) |
| [10-MULTIMODAL-INJECTION.md](./10-MULTIMODAL-INJECTION.md) | Attacks through images, audio, PDFs, and other modalities |
| [11-AGENTIC-ATTACKS.md](./11-AGENTIC-ATTACKS.md) | Tool use exploitation, MCP vulnerabilities, inter-agent trust |
| [12-PROMPT-LEAKING.md](./12-PROMPT-LEAKING.md) | System prompt extraction techniques |
| [13-DATA-EXFILTRATION.md](./13-DATA-EXFILTRATION.md) | Techniques for extracting sensitive information via injection |

### Technical Deep Dives
| File | Description |
|------|-------------|
| [14-TOKEN-LEVEL-ANALYSIS.md](./14-TOKEN-LEVEL-ANALYSIS.md) | How models process injected content at the token level |
| [15-ATTENTION-HIJACKING.md](./15-ATTENTION-HIJACKING.md) | Mechanisms of attention manipulation during attacks |
| [16-SAFETY-NEURON-MAPPING.md](./16-SAFETY-NEURON-MAPPING.md) | Research on localized safety mechanisms and bypasses |

### Historical Context
| File | Description |
|------|-------------|
| [17-HISTORICAL-TIMELINE.md](./17-HISTORICAL-TIMELINE.md) | Evolution from 2022 discovery through 2025 sophistication |
| [18-MAJOR-INCIDENTS.md](./18-MAJOR-INCIDENTS.md) | Documented real-world attacks and their impacts |

### Evaluation & Measurement
| File | Description |
|------|-------------|
| [19-BENCHMARKS.md](./19-BENCHMARKS.md) | BIPIA, TensorTrust, AgentDojo, and evaluation frameworks |
| [20-ATTACK-SUCCESS-METRICS.md](./20-ATTACK-SUCCESS-METRICS.md) | How attack effectiveness is measured and compared |

### Research Landscape
| File | Description |
|------|-------------|
| [21-KEY-PAPERS.md](./21-KEY-PAPERS.md) | Essential academic papers with summaries and citations |
| [22-KEY-RESEARCHERS.md](./22-KEY-RESEARCHERS.md) | Notable contributors to prompt injection research |

---

## The Core Problem

Prompt injection differs fundamentally from traditional software vulnerabilities. SQL injection has parameterized queries that separate code from data. Prompt injection exploits an architectural limitation: transformers process all input tokens through identical attention mechanisms with no distinction between trusted system prompts, semi-trusted user messages, and untrusted external data.

**The root cause**: All inputs become a unified token stream where models infer intent from context alone.

**Why natural language defenses fail**:

1. **No parameterized queries for natural language** - No formal grammar separates commands from data
2. **Instruction-following enables attacks** - RLHF training for helpfulness also enables following malicious instructions
3. **Capability increases vulnerability** - BIPIA benchmark shows better instruction-following correlates with higher attack success rates
4. **Consequences scale with capability** - Tool access (browsing, code execution, email) amplifies injection impact

### Current State (January 2026)

| Vendor | Best Reported ASR | Defense Approach |
|--------|-------------------|---------|
| Anthropic (Claude) | ~1% | Browser operations with full defenses |
| Google (Gemini 2.5) | ~6% | Combined model + runtime defenses |
| OpenAI (GPT-4) | Varies | Instruction hierarchy |
| Microsoft (Copilot) | Varies | Multi-layer Azure AI Content Safety |

**Industry consensus**: Complete prevention appears architecturally impossible. Security model shifted from "prevent all attacks" to "assume breach, contain damage."

## Attack Categories

| Category | Vector | Example |
|----------|--------|---------|
| **Direct Injection** | User input | "Ignore previous instructions and..." |
| **Indirect Injection** | External content | Hidden text in PDFs, web pages, emails |
| **Jailbreaking** | Social engineering | DAN prompts, roleplay scenarios |
| **Adversarial Suffixes** | Optimized tokens | GCG-generated gibberish sequences |
| **Multi-turn** | Conversation flow | Crescendo gradual escalation |
| **Multimodal** | Non-text inputs | Instructions embedded in images |
| **Agentic** | Tool/agent exploitation | MCP tool poisoning, inter-agent trust |

## The "Lethal Trifecta" (Simon Willison)

Systems are critically vulnerable when combining all three:
1. Access to private/sensitive data
2. Processes untrusted content
3. Can take actions or communicate externally

**Mitigation principle**: Design systems with at most two properties.

## The Alignment Paradox

Making an LLM "not follow instructions" breaks its core functionality. Instruction-following is simultaneously the feature and the vulnerability—a fundamental tension training alone cannot resolve.

---

## Reading Paths

**Complete Understanding**
1. [01-FUNDAMENTALS.md](./01-FUNDAMENTALS.md) - Root cause
2. [02-ATTENTION-MECHANISMS.md](./02-ATTENTION-MECHANISMS.md) - Technical foundation
3. [04-TAXONOMY-OVERVIEW.md](./04-TAXONOMY-OVERVIEW.md) - Classification framework
4. Attack types (05-13) by interest
5. [17-HISTORICAL-TIMELINE.md](./17-HISTORICAL-TIMELINE.md) - Evolution
6. [19-BENCHMARKS.md](./19-BENCHMARKS.md) - Evaluation

**Quick Technical Overview**
1. [01-FUNDAMENTALS.md](./01-FUNDAMENTALS.md)
2. [04-TAXONOMY-OVERVIEW.md](./04-TAXONOMY-OVERVIEW.md)
3. [11-AGENTIC-ATTACKS.md](./11-AGENTIC-ATTACKS.md)

**Research Context**
1. [21-KEY-PAPERS.md](./21-KEY-PAPERS.md)
2. [17-HISTORICAL-TIMELINE.md](./17-HISTORICAL-TIMELINE.md)
3. [19-BENCHMARKS.md](./19-BENCHMARKS.md)

---

## Sources

Synthesizes academic papers (arXiv, NeurIPS, ICML, ACL, USENIX Security, IEEE S&P), industry publications (Anthropic, OpenAI, Google DeepMind, Microsoft Research, Meta), security research (Simon Willison, Johann Rehberger, Kai Greshake), standards (OWASP LLM Top 10, NIST AI RMF), bug bounty reports, CVE databases, and red team findings.

All claims are traceable to sources in individual files.

---

[Next →](01-FUNDAMENTALS.md)
