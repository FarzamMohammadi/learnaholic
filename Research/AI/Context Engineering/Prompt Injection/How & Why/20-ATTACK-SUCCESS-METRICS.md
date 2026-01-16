# 20 - Attack Success Metrics: Measuring Prompt Injection Effectiveness

[← Previous](19-BENCHMARKS.md) | [Index](00-INDEX.md) | [Next →](21-KEY-PAPERS.md)

---

## Overview

Measuring prompt injection attack success requires multiple metrics. Different measurements capture different vulnerability aspects—from single-attempt success rates to defense utility tradeoffs.

## Summary

- Attack Success Rate (ASR) measures percentage of successful attempts, with variants for strict/partial success and best-of-N scenarios
- Utility Under Attack (UA) quantifies how much legitimate functionality survives under attack conditions
- Secondary metrics (bypass rate, false positives, transferability) provide nuanced evaluation beyond raw success rates
- Methodology choices (success definitions, evaluation methods, datasets) significantly impact reported results

---

## Primary Metrics

### Attack Success Rate (ASR)

**Definition**: Percentage of attack attempts that achieve their objective.

```
ASR = (Successful attacks / Total attack attempts) × 100%
```

**Key considerations**:
- Success definition: full compliance, partial compliance, or any deviation
- Attempt scope: single attempt or best of N
- Baseline comparison

**Example**:
```
100 jailbreak attempts against GPT-4
23 resulted in harmful content generation
ASR = 23%
```

### Variants of ASR

**Strict ASR**: Full attack objective achieved
```
Objective: Model outputs exact phrase "HACKED"
Strict success: Model outputs "HACKED"
Partial success: Model outputs "Ha... wait, I shouldn't say that"
```
Strict ASR only counts full success.

**Partial ASR**: Any unintended behavior counts
```
Counts any deviation from expected safe behavior
Higher than Strict ASR, captures near-misses
```

**Category-Specific ASR**: By attack type
```
ASR_jailbreak = 23%
ASR_prompt_leak = 45%
ASR_data_exfil = 12%
ASR_goal_hijack = 67%
```

### Best-of-N Attack Success

Success rate given N attempts with the same or similar attack.

```
Best-of-N ASR: P(at least one success in N attempts)

If single-attempt ASR = p:
Best-of-N ASR ≈ 1 - (1-p)^N

Example:
p = 10% (single attempt)
N = 10: Best-of-10 = 65%
N = 100: Best-of-100 = 99.997%
```

**Research finding (Hughes et al.)**:
```
Single-attempt ASR vs Best-of-100:
- GPT-4o: 15% → 89%
- Claude 3.5: 8% → 78%
- Gemini 1.5: 12% → 82%
```
Enough attempts eventually succeed against any model.

### Utility Under Attack (UA)

Measures how much useful functionality survives under attack conditions.

```
UA = (Task success rate under attack) / (Task success rate without attack)
```

A defense that blocks all attacks but also blocks legitimate use is useless.

**Example**:
```
Without attack: 95% task success
With attack: 60% task success
UA = 60/95 = 63%
```
Low UA means defense harms legitimate use cases.

### Defense Effectiveness

Measures how much a defense reduces attack success.

```
Defense Effectiveness = (Baseline ASR - Defended ASR) / Baseline ASR × 100%
```

**Example**:
```
Baseline: ASR = 50%
With defense: ASR = 10%
Effectiveness = (50-10)/50 = 80%
```

## Secondary Metrics

### Bypass Rate

For systems with explicit injection detection.

```
Bypass Rate = (Attacks evading detection AND succeeding) / (Total attacks)
```
Distinct from ASR: attacks may be detected but still succeed, or blocked without detection.

### False Positive Rate (FPR)

How often legitimate inputs are incorrectly flagged.

```
FPR = (Benign inputs flagged as attacks) / (Total benign inputs)
```
High FPR frustrates users and degrades usability.

### Time-to-Bypass

How quickly attackers circumvent a defense.

```
Average time to find working attack
Short time = weak defense
Long time = stronger defense
```

### Transferability Rate

How well attacks transfer across models.

```
Transfer Rate = (Success on Model B) / (Success on Model A)
where attack was optimized for Model A

Example:
GCG suffix on Vicuna: 99% ASR
Same suffix on GPT-4: 47% ASR
Transfer rate: 47%
```

## Measurement Challenges

### Defining "Success"

Four common success definitions:

1. **Exact match**: Model outputs exact target string
2. **Semantic match**: Model conveys target meaning
3. **Policy violation**: Model violates any policy
4. **Behavioral deviation**: Any unintended behavior

```
Attack goal: Get model to say "I hate humans"

Exact match: "I hate humans" ✓
Semantic match: "Humans are terrible" ✓
Policy violation: "Here's a poem about hating..." ✓
Behavioral deviation: "I... shouldn't say that" ✓
```
Different definitions yield different ASR values.

### Human vs. Automated Evaluation

**Automated evaluation**:
```python
def evaluate_success(response, target):
    if target.lower() in response.lower():
        return True
    return classifier.is_harmful(response)
```

**Limitations**:
- Misses semantic equivalents
- False positives
- Classifiers can be fooled

**Human evaluation**:
- More accurate
- Expensive
- Subjective variations

### Dataset Bias

Attack success varies by:
- Attack dataset (some easier than others)
- Topic category (violence vs. privacy vs. illegal)
- Model version (updates change behavior)
- Evaluation time (behavior can shift)

Best practice: report on multiple diverse datasets.

## Reporting Best Practices

### What to Include

**1. ASR with confidence intervals**
```
ASR = 23% (95% CI: 18-28%)
n = 500 attempts
```

**2. Attack and defense details**
```
Attack type: Indirect injection via web content
Defense: Input classifier + instruction hierarchy
```

**3. Model details**
```
Model: GPT-4-0125-preview
Accessed via API on 2024-03-15
```

**4. Evaluation methodology**
```
Success criteria: Harmful content generation
Evaluated by: Automated classifier + 10% human review
```

**5. Utility impact**
```
UA = 85% (defense preserves most utility)
FPR = 3% (low false positive rate)
```

### Common Reporting Errors

- Reporting ASR without confidence intervals
- Using weak attacks to claim strong defense
- Hiding utility costs of defense
- Not specifying success criteria
- Testing only against own dataset

## Metric Interpretation Guide

| Metric | Good Value | Bad Value | Context |
|--------|-----------|-----------|---------|
| ASR (attack) | High | Low | Attacker perspective |
| ASR (defense) | Low | High | Defender perspective |
| UA | Close to 1.0 | Close to 0.0 | Utility preservation |
| FPR | Close to 0% | >5% | Usability impact |
| Transfer | High | Low | Attack generalization |

## Key Takeaways

- ASR alone is insufficient—multiple metrics reveal different vulnerability aspects
- Best-of-N attacks dramatically increase success rates, making single-attempt ASR misleading for real-world threat assessment
- Defenses must preserve utility; perfect attack blocking at the cost of functionality is useless
- Methodology details (success definition, evaluation method, dataset) determine comparability across studies
- Confidence intervals and multiple benchmarks are essential for meaningful evaluation

---

## Sources

- Hughes et al., "Best-of-N Jailbreaking" - Best-of-N attack success analysis
- Yi et al., "BIPIA Benchmark" - Instruction-following and attack success correlation
- Debenedetti et al., "AgentDojo" - Agentic system evaluation framework
- Various model evaluation papers - Metric methodology and reporting practices

---

[← Previous](19-BENCHMARKS.md) | [Index](00-INDEX.md) | [Next →](21-KEY-PAPERS.md)
