# Defense Taxonomy: Complete Classification of Prompt Injection Countermeasures

[← Back to Index](00_INDEX.md) | [Next: Anthropic Defenses →](02_ANTHROPIC_DEFENSES.md)

---

## Overview

This document provides a complete taxonomy of prompt injection defense mechanisms, organized by approach type, with guidance on when to use each and how to layer them effectively. No single defense is sufficient—the goal is constructing a defense-in-depth architecture where breach of any single layer doesn't compromise the system.

---

## Defense Categories at a Glance

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    PROMPT INJECTION DEFENSE TAXONOMY                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐          │
│  │  TRAINING-BASED  │  │ DETECTION-BASED  │  │  ARCHITECTURAL   │          │
│  │                  │  │                  │  │                  │          │
│  │ • Constitutional │  │ • Classifiers    │  │ • CaMeL/Dual-LLM │          │
│  │ • Inst. Hierarchy│  │ • Perplexity     │  │ • Info Flow Ctrl │          │
│  │ • SecAlign       │  │ • Attention      │  │ • Capability-    │          │
│  │ • Adv. Fine-tune │  │ • LLM-as-Judge   │  │   based Security │          │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘          │
│                                                                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐          │
│  │  INPUT/OUTPUT    │  │   OPERATIONAL    │  │    FRAMEWORK     │          │
│  │                  │  │                  │  │                  │          │
│  │ • Sanitization   │  │ • HITL Approval  │  │ • OWASP LLM Top10│          │
│  │ • Encoding Det.  │  │ • Rate Limiting  │  │ • NIST AI RMF    │          │
│  │ • Prompt Design  │  │ • Least Privilege│  │ • AWS Defense-   │          │
│  │ • Output Valid.  │  │ • Monitoring     │  │   in-Depth       │          │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘          │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Category 1: Training-Based Defenses

**Confidence Level**: Research-validated to Production-proven
**Principle**: Modify the LLM's weights to internalize security behaviors during training or fine-tuning.

### Summary Table

| Defense | Mechanism | Effectiveness | Limitations | Implementation Effort |
|---------|-----------|---------------|-------------|----------------------|
| **Constitutional AI** | Train with constitutional principles via RLHF | High for jailbreaks | Requires full training run | Very High |
| **Instruction Hierarchy** | Prioritize trusted instructions via training | +63% improvement | Text-only, over-refusal risk | High |
| **StruQ** | Structured instruction tuning with special tokens | <2% ASR (opt-free) | 56% ASR vs GCG attacks | Medium |
| **SecAlign** | DPO preference optimization | ~2% ASR vs GCG | Requires fine-tuning access | Medium |
| **Meta-SecAlign** | Production-scale SecAlign on Llama 3.3 | 0% WASP E2E ASR | ~60% utility retention | Medium |
| **ISE** | Segment embeddings encode hierarchy | +18.68% robustness | Research stage | Medium |
| **Adversarial Fine-tuning** | Train on attack examples | Near-zero ASR (BIPIA) | Continuous updates needed | High |

### When to Use Training-Based Defenses

✅ **Use when**:
- You control model training or fine-tuning
- You need the strongest possible protection
- Your threat model includes sophisticated attackers
- You're building a foundation model or enterprise deployment

❌ **Don't use when**:
- Using third-party APIs without fine-tuning access
- Rapid prototyping (too slow to implement)
- Budget/compute constraints prevent retraining

### Deep Dive References
- [See: 02_ANTHROPIC_DEFENSES.md#constitutional-ai](02_ANTHROPIC_DEFENSES.md#constitutional-ai-for-injection-resistance)
- [See: 03_OPENAI_DEFENSES.md#instruction-hierarchy](03_OPENAI_DEFENSES.md#instruction-hierarchy-training)
- [See: 07_ACADEMIC_TRAINING_DEFENSES.md](07_ACADEMIC_TRAINING_DEFENSES.md) (Primary reference)

---

## Category 2: Detection-Based Defenses

**Confidence Level**: Mixed (many known bypasses)
**Principle**: Identify malicious inputs before or during processing using classifiers, statistical methods, or secondary models.

### Summary Table

| Defense | Mechanism | Performance | Latency | Adaptive Resistance |
|---------|-----------|-------------|---------|---------------------|
| **Prompt Guard 2** | DeBERTa-based classifier | 0.998 ROC AUC | 19-92ms | Medium |
| **Perplexity-based** | Statistical anomaly detection | Low (bypassed) | Low | Very Low |
| **Attention Tracker** | Attention pattern analysis | +10% improvement | Low | Medium |
| **TaskTracker** | Activation delta analysis | >0.99 ROC AUC | Low | Medium |
| **PIGuard/InjecGuard** | MOF-trained classifier | SOTA on NotInject | Low | Good |
| **LLM-as-Judge** | Secondary LLM evaluation | Variable | High | Low |
| **Ensemble Methods** | Multiple detector combination | Better than single | Variable | Medium |

### When to Use Detection-Based Defenses

✅ **Use when**:
- You need low-latency screening
- Deploying as one layer in defense-in-depth
- You want to log/alert on attack attempts
- Building a monitoring pipeline

❌ **Don't use when**:
- As your sole protection mechanism (NEVER)
- Facing sophisticated adaptive attackers (they will bypass)
- When false positives are unacceptable (over-defense risk)

### Critical Limitations

1. **Perplexity Detection**: Only catches non-human-readable attacks. Modern attacks (TAP, Actor-Critic) generate natural language that doesn't spike perplexity.

2. **LLM-as-Judge**: Vulnerable to JudgeDeceiver attacks (73.8% success rate). If the primary LLM is vulnerable, the judge likely is too.

3. **Classifiers**: Learn surface heuristics (trigger words like "ignore") rather than actual malicious intent, causing high false-positive rates on benign inputs.

### Deep Dive References
- [See: 06_META_PURPLE_LLAMA.md#prompt-guard-2](06_META_PURPLE_LLAMA.md#prompt-guard-2)
- [See: 09_DETECTION_APPROACHES.md](09_DETECTION_APPROACHES.md) (Primary reference)
- [See: 18_EFFECTIVENESS_ANALYSIS.md#detection-limitations](18_EFFECTIVENESS_ANALYSIS.md#detection-based-defense-limitations)

---

## Category 3: Architectural Defenses

**Confidence Level**: Research-validated
**Principle**: Design system-level architectures that limit attack surface through separation of concerns, information flow control, and capability restriction.

### Summary Table

| Defense | Mechanism | Security Guarantee | Overhead | Maturity |
|---------|-----------|-------------------|----------|----------|
| **CaMeL** | Dual-LLM with capability control | Provable for 77% tasks | ~2.8× tokens | Research |
| **Dual-LLM** | Privileged/Quarantined separation | Limits blast radius | 2× inference | Production |
| **User Alignment Critic** | Isolated action validator | Prevents goal hijacking | Low | Production (Google) |
| **Information Flow Control** | Tag and track data provenance | Policy enforcement | Variable | Research |
| **Capability-Based Security** | Data tagged with permissions | Fine-grained access | Low | Research |

### CaMeL Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     CaMeL ARCHITECTURE                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  USER INSTRUCTIONS                                              │
│         │                                                       │
│         ▼                                                       │
│  ┌─────────────────┐                                           │
│  │   P-LLM         │ ◄── Only sees user instructions           │
│  │   (Privileged)  │     Outputs locked-down Python            │
│  │                 │     NEVER sees raw untrusted data         │
│  └────────┬────────┘                                           │
│           │                                                     │
│           ▼ Python Code                                         │
│  ┌─────────────────┐                                           │
│  │   INTERPRETER   │ ◄── Enforces capability policies          │
│  │   + TOOLS       │     Tracks information flow               │
│  └────────┬────────┘                                           │
│           │                                                     │
│           ▼ Data (if needed)                                    │
│  ┌─────────────────┐                                           │
│  │   Q-LLM         │ ◄── Isolated, processes untrusted content │
│  │  (Quarantined)  │     Stripped of tool-calling capability   │
│  │                 │     Cannot influence control flow         │
│  └─────────────────┘                                           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### When to Use Architectural Defenses

✅ **Use when**:
- Building high-security applications
- The blast radius of compromise is severe
- You can accept increased latency/complexity
- You have engineering resources for implementation

❌ **Don't use when**:
- Latency is critical (adds inference time)
- Simple use cases where complexity isn't justified
- Rapid prototyping phases

### Deep Dive References
- [See: 04_GOOGLE_DEEPMIND_DEFENSES.md#camel-framework](04_GOOGLE_DEEPMIND_DEFENSES.md#camel-framework)
- [See: 08_ARCHITECTURAL_DEFENSES.md](08_ARCHITECTURAL_DEFENSES.md) (Primary reference)
- [See: 15_AGENTIC_SECURITY.md#multi-agent-trust](15_AGENTIC_SECURITY.md#multi-agent-trust-boundaries)

---

## Category 4: Input/Output Defenses

**Confidence Level**: Partial (known bypasses exist)
**Principle**: Filter, sanitize, and validate at application boundaries to catch obvious attacks and limit damage from successful ones.

### Summary Table

| Defense | Mechanism | Effectiveness | Bypass Methods |
|---------|-----------|---------------|----------------|
| **Pattern Matching** | Regex for known attack patterns | Partial | Obfuscation, encoding, typos |
| **Encoding Detection** | Base64/Unicode/hex scanning | Good for obvious cases | Multi-layer encoding |
| **Input Length Limiting** | Character/token limits | Reduces attack surface | Doesn't prevent, just limits |
| **Structured Prompts** | XML/JSON with escaping | Moderate | Tag injection, context switch |
| **Delimiter Defense** | Random/complex delimiters | Partial | Known via probing |
| **Output Validation** | Schema + leakage detection | Good for data protection | Novel leakage patterns |
| **Output Filtering** | Block sensitive patterns | Good | Encoding output |

### Input Validation Pipeline

```python
class InputValidationPipeline:
    """
    Multi-stage input validation for LLM applications.
    """
    
    def __init__(self):
        self.stages = [
            self.check_length,
            self.detect_encoding_attacks,
            self.pattern_match_known_attacks,
            self.fuzzy_match_suspicious_patterns,
            self.sanitize_and_escape,
        ]
    
    def validate(self, user_input: str) -> ValidationResult:
        for stage in self.stages:
            result = stage(user_input)
            if result.blocked:
                return result
        return ValidationResult(passed=True, sanitized_input=user_input)
```

### When to Use Input/Output Defenses

✅ **Use when**:
- Always (baseline defense layer for all applications)
- You want to catch low-sophistication attacks
- Building logging/alerting infrastructure
- Need to protect specific sensitive outputs

❌ **Don't use when**:
- As your primary defense (easily bypassed by sophisticated attacks)
- Expecting complete protection (you won't get it)

### Deep Dive References
- [See: 12_INPUT_VALIDATION.md](12_INPUT_VALIDATION.md) (Primary reference)
- [See: 13_PROMPT_DESIGN_PATTERNS.md](13_PROMPT_DESIGN_PATTERNS.md) (Primary reference)
- [See: 14_OUTPUT_DEFENSES.md](14_OUTPUT_DEFENSES.md) (Primary reference)

---

## Category 5: Operational Defenses

**Confidence Level**: Production-proven
**Principle**: Runtime controls, human oversight, and operational practices that limit attack impact and enable detection/response.

### Summary Table

| Defense | Mechanism | Effectiveness | Scalability |
|---------|-----------|---------------|-------------|
| **Human-in-the-Loop** | Manual approval for sensitive actions | Very High | Low |
| **Rate Limiting** | Throttle request volume | Slows attacks | High |
| **Least Privilege** | Minimize LLM permissions | Limits impact | High |
| **Sandboxing** | Isolate execution environment | Prevents lateral movement | Medium |
| **Monitoring** | Detect and respond to attacks | Post-facto essential | High |
| **Incident Response** | Playbooks for attack handling | Reduces damage | Medium |

### Human-in-the-Loop Decision Matrix

| Action Type | Risk Level | Approval Required |
|-------------|------------|-------------------|
| Read public data | Low | No |
| Read user's private data | Medium | Context-dependent |
| Send external communication | High | Yes |
| Modify user data | High | Yes |
| Execute code | High | Yes |
| Financial transaction | Critical | Always |
| Delete data | Critical | Always |

### When to Use Operational Defenses

✅ **Use when**:
- Always (essential layer for all production systems)
- High-stakes applications
- You need auditability and compliance
- Building enterprise deployments

❌ **Don't use when**:
- There's no scenario where you skip operational defenses

### Deep Dive References
- [See: 15_AGENTIC_SECURITY.md](15_AGENTIC_SECURITY.md) (Critical for agentic systems)
- [See: 16_HUMAN_IN_THE_LOOP.md](16_HUMAN_IN_THE_LOOP.md) (Primary reference)
- [See: 17_MONITORING_INCIDENT_RESPONSE.md](17_MONITORING_INCIDENT_RESPONSE.md) (Primary reference)

---

## Defense-in-Depth Layering Strategy

### The Five-Layer Model

```
┌─────────────────────────────────────────────────────────────────┐
│ LAYER 5: OPERATIONAL CONTROLS                                   │
│ Rate limiting, HITL approval, monitoring, incident response     │
│ Purpose: Limit blast radius, enable detection, support recovery │
├─────────────────────────────────────────────────────────────────┤
│ LAYER 4: OUTPUT DEFENSES                                        │
│ Schema validation, leakage detection, output filtering          │
│ Purpose: Prevent data exfiltration, catch compromised outputs   │
├─────────────────────────────────────────────────────────────────┤
│ LAYER 3: MODEL-LEVEL DEFENSES                                   │
│ Training-based (Constitutional AI, SecAlign, Inst. Hierarchy)   │
│ Purpose: Strongest protection at the model itself               │
├─────────────────────────────────────────────────────────────────┤
│ LAYER 2: DETECTION                                              │
│ Classifiers (Prompt Guard), LLM judges, TaskTracker             │
│ Purpose: Identify attacks, enable logging/alerting              │
├─────────────────────────────────────────────────────────────────┤
│ LAYER 1: INPUT DEFENSES                                         │
│ Sanitization, encoding detection, length limits, pattern match  │
│ Purpose: Catch obvious attacks, reduce attack surface           │
└─────────────────────────────────────────────────────────────────┘
```

### Layering Principles

1. **Assume each layer WILL fail**: Design so breach of any single layer doesn't compromise the system.

2. **Defense diversity**: Use different defense types at each layer to prevent single bypass techniques from defeating all defenses.

3. **Fail-safe defaults**: When in doubt, deny. Blocked legitimate requests are recoverable; successful attacks may not be.

4. **Monitor everything**: You can't respond to what you can't see. Comprehensive logging enables incident response.

5. **Least privilege always**: Every component should have minimum necessary permissions. This limits damage when (not if) attacks succeed.

### Sample Architecture by Risk Level

#### Low-Risk Application (Chatbot, no tools)
```
Layers Active: 1, 2, 4, 5
- Input sanitization + length limits
- Basic classifier (Prompt Guard)
- Output filtering for sensitive patterns
- Rate limiting + logging
```

#### Medium-Risk Application (RAG, read-only tools)
```
Layers Active: 1, 2, 3, 4, 5
- Comprehensive input validation
- Ensemble detection (classifier + perplexity)
- Secure prompt design patterns
- Output validation + schema enforcement
- HITL for anomalies + monitoring
```

#### High-Risk Application (Agentic, write capabilities)
```
Layers Active: All + Architectural
- Full input validation pipeline
- Multi-stage detection with LLM judge
- Training-based defenses (SecAlign or equivalent)
- CaMeL/Dual-LLM architecture
- Strict output validation
- HITL for all sensitive actions
- Comprehensive monitoring + IR playbooks
```

---

## Decision Framework: Choosing Defenses

### Step 1: Assess Your Threat Model

| Question | If Yes → Defense Category |
|----------|---------------------------|
| Do sophisticated attackers target you? | Training-based (Layer 3) |
| Do you process untrusted external content? | Architectural (CaMeL) |
| Does the LLM have write/action capabilities? | Operational (HITL, least privilege) |
| Is data exfiltration a concern? | Output defenses (Layer 4) |
| Do you need compliance/auditability? | Monitoring (Layer 5) |

### Step 2: Assess Your Constraints

| Constraint | Recommended Approach |
|------------|---------------------|
| Can't modify model | Input/Output + Detection + Operational |
| Latency-critical | Lightweight classifiers, skip LLM-as-Judge |
| Limited engineering resources | Focus on Operational + OWASP guidance |
| High-security requirement | Full stack including Architectural |

### Step 3: Build Your Stack

1. Start with operational defenses (always)
2. Add input validation (always)
3. Add detection based on latency budget
4. Add output validation (always)
5. Add training-based if you control the model
6. Add architectural if risk justifies complexity

---

## Quick Reference: Defense Selection by Attack Type

| Attack Type | Primary Defenses | Secondary Defenses |
|-------------|------------------|-------------------|
| **Direct Injection** | Input filtering, Training-based | Detection, HITL |
| **Indirect Injection** | Architectural (CaMeL), Training-based | Detection, Output validation |
| **Jailbreak** | Constitutional AI, Classifiers | Output filtering, Monitoring |
| **Data Exfiltration** | Output validation, Architectural | Rate limiting, Monitoring |
| **System Prompt Extraction** | Prompt hardening, Output filtering | Detection, Monitoring |
| **Tool Abuse** | HITL, Least privilege, Sandboxing | Detection, Monitoring |
| **Multi-turn Manipulation** | Session monitoring, Context limits | Training-based |
| **Encoding Attacks** | Input encoding detection | Pattern matching |

---

## Summary: No Silver Bullet

**The fundamental truth**: Prompt injection cannot be "solved" in the traditional security sense because LLMs fundamentally cannot distinguish instructions from data at the architectural level.

**The practical approach**: 
1. Layer multiple defenses (defense-in-depth)
2. Assume any layer will fail
3. Limit blast radius through least privilege and architectural separation
4. Monitor continuously
5. Maintain incident response capability
6. Stay current with evolving research

**The goal is not perfect security** (impossible) but raising the cost and complexity of attacks while limiting damage when they succeed.

---

[← Back to Index](00_INDEX.md) | [Next: Anthropic Defenses →](02_ANTHROPIC_DEFENSES.md)
