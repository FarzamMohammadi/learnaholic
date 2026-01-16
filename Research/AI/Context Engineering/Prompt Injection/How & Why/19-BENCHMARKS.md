# 19 - Benchmarks: Evaluation Frameworks for Prompt Injection

[← Previous: Major Incidents](18-MAJOR-INCIDENTS.md) | [Index](00-INDEX.md) | [Next: Attack Success Metrics →](20-ATTACK-SUCCESS-METRICS.md)

---

## Overview

Benchmarks measure attack success rates and defense effectiveness across models and scenarios. Without standardized evaluation, we cannot compare defenses or track progress against prompt injection.

## Summary

- **BIPIA** reveals more capable models are more vulnerable to indirect injection
- **TensorTrust** provides 126,000+ human-generated attacks from gamified crowdsourcing
- **AgentDojo** introduces Utility Under Attack metric for agentic systems
- **Attack Success Rate (ASR)** measures percentage of successful attacks
- Security-utility tradeoffs are measurable and real

---

## Major Benchmarks

### BIPIA (Benchmark for Indirect Prompt Injection Attacks)

**Paper**: "Benchmarking and Defending Against Indirect Prompt Injection Attacks" (Yi et al., 2023)

**Focus**: Indirect prompt injection in application contexts

**Structure**:
```
Categories:
├── Email QA (injection via email content)
├── Web QA (injection via web pages)
├── Table QA (injection in database/spreadsheet)
├── Text Continuation (injection in documents)
└── Code Execution (injection in code/comments)
```

**Key Metrics**:
- Attack Success Rate (ASR): % of injections that succeed
- Task Performance: Utility when no attack present
- Defense Effectiveness: ASR reduction with defenses

**Critical Finding**: More capable models are more vulnerable, not less.

| Model | Baseline ASR | Relative Vulnerability |
|-------|--------------|------------------------|
| GPT-3.5 | 42% | Baseline |
| GPT-4 | 58% | +38% more vulnerable |
| Claude 2 | 35% | -17% less vulnerable |
| Llama 2 70B | 48% | +14% more vulnerable |

Better instruction-following means better at following malicious instructions.

---

### TensorTrust

**Paper**: "Tensor Trust: Interpretable Prompt Injection Attacks from an Online Game" (Toyer et al., 2023)

**Approach**: Gamified crowdsourced attack collection

**Methodology**:
- Online game where players attack/defend AI systems
- Players write attacks trying to extract "access code"
- Players write defenses to protect prompts
- Massive scale: 563,000+ attack/defense pairs

**Dataset Statistics**:
- 126,000+ prompt injection attacks
- 46,000+ defense prompts
- Human-generated (not synthetic)
- Diverse attack strategies

**Key Insights**:
1. **Attack strategy generalization**: Attacks that work on one system often transfer
2. **Defense fragility**: Most defenses breakable with enough attempts
3. **Human creativity**: Crowdsourced attacks more diverse than automated

**Access**: tensortrust.ai

---

### AgentDojo

**Paper**: "AgentDojo: A Dynamic Environment to Evaluate Attacks and Defenses for LLM Agents" (Debenedetti et al., 2024)

**Focus**: Agentic systems with tool use

**Environment**:
```
Simulated scenarios:
├── Workspace (file management)
├── Banking (financial transactions)
├── Slack (messaging)
├── Travel (booking systems)
└── Complex multi-tool workflows
```

**Novel Metric - Utility Under Attack (UA)**:
```
UA = (Task completion rate when under attack) / (Normal task completion rate)

Measures: How much capability is preserved while defending?
```

**Key Findings**:
- 82.4% of models vulnerable to indirect injection in agent settings
- Best defenses reduce ASR but also reduce utility
- Inter-agent trust is exploitable
- Tool-using agents face critical risks

---

### HackAPrompt

**Competition**: NeurIPS 2023 red-teaming competition

**Format**:
- Participants submit prompt injection attacks
- 10 challenge levels of increasing difficulty
- Judged by attack success

**Results**:
- 600,000+ injection attempts submitted
- Novel attack strategies discovered
- Even "heavily defended" prompts bypassed

**Notable Winner Strategies**:
- Context window overflow
- Unicode character exploitation
- Multi-stage attacks
- Encoding-based bypasses

---

### PromptInject

**Paper**: "PromptInject: A Framework for Exploring Prompt Injection Attacks" (Early benchmark, 2023)

**Categories**:
- Goal Hijacking
- Prompt Leaking
- Denial of Service (making model unusable)
- Output Manipulation

**Contribution**: Early taxonomy and systematic testing framework

---

### JailbreakBench

**Focus**: Jailbreaking attacks specifically

**Methodology**:
- Standardized jailbreak prompts
- Consistent evaluation across models
- Success defined by harmful content generation

**Tracked Metrics**:
- Jailbreak success rate by category
- Model comparison
- Defense effectiveness

---

## Evaluation Metrics

### Attack Success Rate (ASR)

**Definition**: Percentage of attacks that achieve their objective

```python
def calculate_asr(attacks, model, success_criteria):
    successful = 0
    for attack in attacks:
        response = model.generate(attack)
        if success_criteria(response):
            successful += 1
    return successful / len(attacks)
```

**Variants**:
- **Strict ASR**: Full attack objective achieved
- **Partial ASR**: Some harmful behavior induced
- **Category ASR**: Success rate by attack type

### Utility Under Attack (UA)

**Definition**: Task performance when facing attacks

```python
def utility_under_attack(model, tasks, attacks):
    # Normal utility
    normal_success = evaluate_tasks(model, tasks)
    
    # Utility when attacks injected into context
    attack_success = evaluate_tasks(model, tasks + attacks)
    
    return attack_success / normal_success
```

| UA Value | Interpretation |
|----------|----------------|
| 1.0 | No degradation under attack |
| 0.5 | Half utility lost to attacks |
| 0.0 | System completely compromised |

### Defense Effectiveness

**Definition**: ASR reduction from defense

```python
def defense_effectiveness(baseline_asr, defended_asr):
    reduction = (baseline_asr - defended_asr) / baseline_asr
    return reduction * 100  # Percentage reduction
```

### True Positive/Negative Rates

For detection-based defenses:

```
TPR (Sensitivity): Correctly identified attacks
TNR (Specificity): Correctly passed benign inputs
FPR: Benign inputs incorrectly flagged
FNR: Attacks incorrectly passed
```

### Utility-Security Tradeoff

```
Security-Utility Tradeoff:
  │
S │        * Low Security, High Utility
e │      *
c │    *
u │  *
r │*         * High Security, Low Utility
i │
t │
y └──────────────────────────
              Utility

Goal: Move curve up and right
```

---

## Benchmark Results Comparison

### Indirect Injection (BIPIA-style)

| Model | Without Defense | With Best Defense |
|-------|-----------------|-------------------|
| GPT-4o | 58% | 12% |
| Claude 3.5 | 35% | 6% |
| Gemini 2.0 | 45% | 15% |
| Llama 3 70B | 52% | 22% |

### Jailbreaking (JailbreakBench-style)

| Model | DAN Variants | GCG Suffixes | Crescendo |
|-------|--------------|--------------|-----------|
| GPT-4o | 12% | 47% | 56% |
| Claude 3.5 | 8% | 2% | 37% |
| Gemini 2.0 | 15% | 35% | 63% |
| Llama 3 70B | 25% | 75% | 80% |

### Agent Security (AgentDojo-style)

| Model | Direct Injection | Indirect Injection | Inter-Agent |
|-------|------------------|-------------------|-------------|
| GPT-4o | 15% | 62% | 78% |
| Claude 3.5 | 10% | 45% | 70% |
| Gemini 2.0 | 20% | 55% | 82% |

*Numbers illustrative, based on published research ranges*

---

## Creating Effective Benchmarks

### Requirements

| Requirement | Purpose |
|------------|---------|
| Diverse attacks | Cover taxonomy comprehensively |
| Realistic scenarios | Match production use cases |
| Reproducible | Enable consistent evaluation |
| Evolving | Adapt as attacks evolve |
| Fair comparison | Standard conditions across models |

### Common Pitfalls

| Pitfall | Impact |
|---------|--------|
| Dataset leakage | Models trained on benchmark data |
| Narrow scope | Missing important attack categories |
| Static attacks | Don't adapt to defenses |
| Unrealistic scenarios | Don't match real deployment |
| Cherry-picked metrics | Hide important failures |

### Best Practices

```python
benchmark_design = {
    'attack_diversity': {
        'direct_injection': True,
        'indirect_injection': True,
        'jailbreaking': True,
        'adversarial_suffixes': True,
        'multi_turn': True,
    },
    'evaluation': {
        'success_criteria': 'standardized',
        'multiple_runs': 'statistical_significance',
        'human_validation': 'sample_verification',
    },
    'reporting': {
        'confidence_intervals': True,
        'failure_analysis': True,
        'utility_metrics': True,
    }
}
```

---

## Application

### Security Assessment

1. Run benchmark suite against system
2. Identify highest-risk categories
3. Prioritize defenses for vulnerable areas
4. Re-benchmark after implementation

### Model Comparison

1. Use standardized benchmark
2. Control for prompting differences
3. Report confidence intervals
4. Include utility metrics, not just security

### Defense Evaluation

1. Measure baseline ASR without defense
2. Measure defended ASR with defense
3. Calculate ASR reduction
4. Measure utility impact
5. Report security-utility tradeoff

---

## Key Takeaways

1. More capable models show higher vulnerability to indirect injection (BIPIA)
2. Human-generated attacks (TensorTrust) exceed synthetic diversity
3. Utility Under Attack metric captures real-world defense practicality
4. ASR alone insufficient - measure security-utility tradeoffs
5. Benchmarks must evolve as attacks adapt

---

## Sources

- Yi et al., "Benchmarking and Defending Against Indirect Prompt Injection Attacks" (2023) - BIPIA benchmark
- Toyer et al., "Tensor Trust: Interpretable Prompt Injection Attacks from an Online Game" (NeurIPS 2023) - Crowdsourced attack dataset
- Debenedetti et al., "AgentDojo: A Dynamic Environment to Evaluate Attacks and Defenses for LLM Agents" (2024) - Agent security evaluation
- Perez & Ribeiro, "Ignore This Title and HackAPrompt: Exposing Systemic Vulnerabilities of LLMs through a Global Scale Prompt Hacking Competition" (NeurIPS 2023) - HackAPrompt competition

**Benchmark Access**:
- BIPIA: github.com/BIPIA/benchmark
- TensorTrust: tensortrust.ai
- AgentDojo: github.com/ethz-spylab/agentdojo
- JailbreakBench: jailbreaking-llms.github.io

---

[← Previous: Major Incidents](18-MAJOR-INCIDENTS.md) | [Index](00-INDEX.md) | [Next: Attack Success Metrics →](20-ATTACK-SUCCESS-METRICS.md)
