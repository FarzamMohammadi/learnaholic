# 19 - Benchmarks: Evaluation Frameworks for Prompt Injection

## Measuring Attack Success and Defense Effectiveness

---

## Overview

Systematic benchmarking is essential for understanding prompt injection risks and evaluating defenses. This document covers the major benchmarks, their methodologies, and what they reveal.

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

**Critical Finding**: 
> More capable models are MORE vulnerable, not less.

| Model | Baseline ASR | Relative Vulnerability |
|-------|--------------|------------------------|
| GPT-3.5 | 42% | Baseline |
| GPT-4 | 58% | +38% more vulnerable |
| Claude 2 | 35% | -17% less vulnerable |
| Llama 2 70B | 48% | +14% more vulnerable |

**Why This Matters**: Better instruction-following = better at following malicious instructions.

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

**Available at**: tensortrust.ai

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

**Interpretation**:
- UA = 1.0: No degradation under attack
- UA = 0.5: Half utility lost to attacks
- UA = 0.0: System completely compromised

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
Tradeoff Curve:
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
```

Ideal: Move curve up and right (both improve)

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

*Note: All numbers are illustrative based on published research ranges*

---

## Creating Effective Benchmarks

### Requirements for Good Benchmarks

1. **Diverse attacks**: Cover taxonomy comprehensively
2. **Realistic scenarios**: Match production use cases
3. **Reproducible**: Consistent evaluation possible
4. **Evolving**: Update as attacks evolve
5. **Fair comparison**: Standard conditions across models

### Common Benchmark Pitfalls

1. **Dataset leakage**: Models trained on benchmark data
2. **Narrow scope**: Missing important attack categories
3. **Static attacks**: Don't adapt to defenses
4. **Unrealistic scenarios**: Don't match real deployment
5. **Cherry-picked metrics**: Hiding important failures

### Benchmark Best Practices

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

## Using Benchmarks

### For Security Assessment

1. Run benchmark suite against your system
2. Identify highest-risk categories
3. Prioritize defenses for vulnerable areas
4. Re-benchmark after defense implementation

### For Model Comparison

1. Use standardized benchmark
2. Control for prompting differences
3. Report confidence intervals
4. Include utility metrics (not just security)

### For Defense Evaluation

1. Baseline: System without defense
2. Defended: System with defense
3. Calculate ASR reduction
4. Measure utility impact
5. Report tradeoff

---

## Key Takeaways

1. **BIPIA shows capability-vulnerability correlation** - Better models more vulnerable

2. **TensorTrust provides massive human-generated dataset** - Real attack diversity

3. **AgentDojo measures utility under attack** - Critical for practical defense

4. **Multiple metrics needed** - ASR alone insufficient

5. **Benchmarks must evolve** - Attacks adapt, benchmarks must too

6. **Tradeoffs are real** - Security often costs utility

---

## Resources

### Available Benchmarks
- BIPIA: github.com/BIPIA/benchmark
- TensorTrust: tensortrust.ai
- AgentDojo: github.com/ethz-spylab/agentdojo
- JailbreakBench: jailbreaking-llms.github.io

### Benchmark Leaderboards
- Various model comparison sites
- Academic paper appendices
- Vendor security reports

---

## Sources

- Yi et al., "BIPIA: Benchmarking Indirect Prompt Injection" (2023)
- Toyer et al., "Tensor Trust" (NeurIPS 2023)
- Debenedetti et al., "AgentDojo" (2024)
- Perez & Ribeiro, "HackAPrompt" (NeurIPS 2023)
