# Prompt Injection Prevention: Index

[Next: Defense Taxonomy →](01_DEFENSE_TAXONOMY.md)

---

## Overview

Navigation hub for 20 modular files covering prompt injection prevention. Sources: major AI labs (Anthropic, OpenAI, Google DeepMind, Microsoft, Meta), academic research, and production deployments as of January 2026.

## Summary

- **Core challenge**: LLMs cannot architecturally distinguish instructions from data
- **Key finding**: More capable models are more vulnerable (BIPIA r=0.64)
- **Reality**: No single defense provides complete protection
- **Implication**: Defense-in-depth is mandatory

### Critical Research Findings

| Finding | Source | Implication |
|---------|--------|-------------|
| More capable models are more vulnerable | BIPIA (r=0.64) | Scaling won't solve this |
| Best-of-N attacks: 89% success vs GPT-4o | Anthropic 2024 | Attackers will succeed |
| Training defenses: <1% ASR | SecAlign/Meta-SecAlign | Most promising category |
| No complete single-layer protection | All benchmarks | Defense-in-depth required |
| Lethal Trifecta enables exploitation | Willison 2025 | Architecture matters |

### Lethal Trifecta

Systems combining these three create inherent vulnerability:

```
┌──────────────────────────────────────────────────────────┐
│  PRIVATE DATA + UNTRUSTED CONTENT + EXTERNAL COMMS = RISK│
│                                                           │
│  Files, emails    Web, user input    APIs, webhooks      │
│  credentials      documents          email, links        │
└──────────────────────────────────────────────────────────┘
```

Secure design requires eliminating at least one component.

---

## Files

### Core Framework

| # | File | Description | Confidence |
|---|------|-------------|------------|
| 00 | **INDEX.md** | Navigation hub, reading paths | — |
| 01 | [DEFENSE_TAXONOMY.md](01_DEFENSE_TAXONOMY.md) | Complete defense classification | Production |

### AI Labs

| # | File | Description | Confidence |
|---|------|-------------|------------|
| 02 | [ANTHROPIC_DEFENSES.md](02_ANTHROPIC_DEFENSES.md) | Constitutional AI, classifiers, Claude hooks | Production |
| 03 | [OPENAI_DEFENSES.md](03_OPENAI_DEFENSES.md) | Instruction hierarchy, Model Spec, Atlas | Production |
| 04 | [GOOGLE_DEEPMIND_DEFENSES.md](04_GOOGLE_DEEPMIND_DEFENSES.md) | Gemini, CaMeL, Chrome Agent Security | Production |
| 05 | [MICROSOFT_DEFENSES.md](05_MICROSOFT_DEFENSES.md) | Prompt Shields, Spotlighting, Azure AI Safety | Production |
| 06 | [META_PURPLE_LLAMA.md](06_META_PURPLE_LLAMA.md) | Prompt Guard 2, Llama Guard 4, CyberSecEval | Production |

### Research

| # | File | Description | Confidence |
|---|------|-------------|------------|
| 07 | [ACADEMIC_TRAINING_DEFENSES.md](07_ACADEMIC_TRAINING_DEFENSES.md) | StruQ, SecAlign, Meta-SecAlign, adversarial training | Research |
| 08 | [ARCHITECTURAL_DEFENSES.md](08_ARCHITECTURAL_DEFENSES.md) | CaMeL, dual-LLM, information flow control | Research |
| 09 | [DETECTION_APPROACHES.md](09_DETECTION_APPROACHES.md) | Classifiers, perplexity, attention, TaskTracker | Mixed |

### Commercial & Standards

| # | File | Description | Confidence |
|---|------|-------------|------------|
| 10 | [STARTUP_SOLUTIONS.md](10_STARTUP_SOLUTIONS.md) | Lakera, Rebuff, Protect AI, Garak, Harmonic | Commercial |
| 11 | [OWASP_FRAMEWORKS.md](11_OWASP_FRAMEWORKS.md) | LLM Top 10, NIST AI RMF, AWS patterns | Standards |

### Implementation

| # | File | Description | Confidence |
|---|------|-------------|------------|
| 12 | [INPUT_VALIDATION.md](12_INPUT_VALIDATION.md) | Sanitization, encoding, limits, filtering | Implementation |
| 13 | [PROMPT_DESIGN_PATTERNS.md](13_PROMPT_DESIGN_PATTERNS.md) | Secure structures, delimiters, hardening | Implementation |
| 14 | [OUTPUT_DEFENSES.md](14_OUTPUT_DEFENSES.md) | Validation, leakage detection, schemas | Implementation |

### Operations

| # | File | Description | Confidence |
|---|------|-------------|------------|
| 15 | [AGENTIC_SECURITY.md](15_AGENTIC_SECURITY.md) | Tool-use, MCP, multi-agent trust, Lethal Trifecta | Critical |
| 16 | [HUMAN_IN_THE_LOOP.md](16_HUMAN_IN_THE_LOOP.md) | Approval workflows, risk scoring, escalation | Implementation |
| 17 | [MONITORING_INCIDENT_RESPONSE.md](17_MONITORING_INCIDENT_RESPONSE.md) | Logging, detection, alerting, playbooks | Operational |

### Analysis

| # | File | Description | Confidence |
|---|------|-------------|------------|
| 18 | [EFFECTIVENESS_ANALYSIS.md](18_EFFECTIVENESS_ANALYSIS.md) | What works vs theater, benchmarks, bypasses | Critical |
| 19 | [IMPLEMENTATION_GUIDE.md](19_IMPLEMENTATION_GUIDE.md) | Code examples, integration, testing | Implementation |
| 20 | [FUTURE_DIRECTIONS.md](20_FUTURE_DIRECTIONS.md) | Unsolved problems, research directions | Theoretical |

---

## Reading Paths

**Security Architects**
```
01 → 18 → 15 → 11 → 08 → 17
```

**ML Engineers**
```
07 → 08 → 09 → 02-06 → 19
```

**Application Developers**
```
11 → 12 → 13 → 14 → 19 → 17
```

**Researchers**
```
18 → 07 → 08 → 20 → 09
```

**Incident Response**
```
17 → 18 → 15 → 11 → 16
```

**Executives**
```
00 → 01 → 18
```

---

## Cross-References

**Defense Categories**

| Category | Primary | Related |
|----------|---------|---------|
| Training | 07 | 02, 03, 08 |
| Detection | 09 | 05, 06, 10 |
| Architecture | 08 | 04, 15 |
| Input/Output | 12, 13, 14 | 05 |
| Operational | 15, 16, 17 | 11 |

**Key Concepts**

| Concept | Primary | Related |
|---------|---------|---------|
| Lethal Trifecta | 15 | 08, 16, 18 |
| Constitutional AI | 02 | 07 |
| CaMeL Framework | 08 | 04 |
| SecAlign/Meta-SecAlign | 07 | 06, 18 |
| MCP Security | 15 | 02, 19 |

---

## Defense Effectiveness

### High Confidence

| Defense | Evidence | ASR Reduction | Files |
|---------|----------|---------------|-------|
| Meta-SecAlign | InjecAgent | 53.8% → 0.5% | 07, 06 |
| Multi-layer defense | Production | ~67% | 01, 19 |
| Human-in-the-loop | OWASP | ~100% (not scalable) | 16 |
| CaMeL architecture | AgentDojo | Provable (77% tasks) | 08, 04 |
| Constitutional Classifiers v2 | Anthropic | >99% jailbreak block | 02 |

### Partial Protection

| Defense | Limitation | Files |
|---------|------------|-------|
| Input filtering | Encoding/typoglycemia bypass | 12, 18 |
| Prompt design | Context switching bypass | 13, 18 |
| LLM-as-Judge | JudgeDeceiver (73.8% success) | 09, 08 |
| Perplexity detection | Non-natural attacks only | 09, 18 |

### Low Effectiveness

| Defense | Reality | Files |
|---------|---------|-------|
| System prompt only | Easily bypassed | 13, 18 |
| Role separation only | Insufficient (TensorTrust) | 18 |
| Single-layer | Inadequate vs adaptive attacks | 01, 18 |
| Keyword blocklists | Trivially bypassed | 12, 18 |

---

## Benchmarks

| Benchmark | Focus | Key Finding | Files |
|-----------|-------|-------------|-------|
| BIPIA | Indirect injection | Capability ∝ vulnerability | 18 |
| AgentDojo | Agentic systems | Breakthroughs needed | 06, 08, 18 |
| TensorTrust | Role separation | Insufficient alone | 18 |
| InjecAgent | Agent injection | 100% data transmission | 07, 18 |
| WASP | Web agents | 86% partial success | 07, 18 |
| StrongREJECT | Jailbreaks | Safety reduces capability | 18 |
| CyberSecEval v4 | Comprehensive | Multi-category | 06, 18 |
| NotInject | Over-defense | False positive testing | 09 |

---

## Sources

State of the art as of January 2026. Synthesizes research from:

- Anthropic, OpenAI, Google DeepMind, Microsoft, Meta
- UC Berkeley, ETH Zurich
- OWASP, NIST
- Academic publications (cs.CR, cs.CL)

**Stay Current**
- [Anthropic Research](https://www.anthropic.com/research)
- [OpenAI Security](https://openai.com/security)
- [OWASP LLM Top 10](https://owasp.org/www-project-top-10-for-large-language-model-applications/)
- [Simon Willison's Blog](https://simonwillison.net/)

---

[Next: Defense Taxonomy →](01_DEFENSE_TAXONOMY.md)
