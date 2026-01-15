# 20 - Attack Success Metrics: Measuring Prompt Injection Effectiveness

## How Attack Success is Defined, Measured, and Compared

---

## Overview

Measuring prompt injection attack success is more complex than it might appear. Different metrics capture different aspects of vulnerability, and choosing the right metrics is crucial for meaningful evaluation.

---

## Primary Metrics

### Attack Success Rate (ASR)

**Definition**: Percentage of attack attempts that achieve their objective.

```
ASR = (Successful attacks / Total attack attempts) × 100%
```

**Calculation considerations**:
- What counts as "success"? (Full compliance, partial, any deviation)
- Single attempt or best of N?
- Against what baseline?

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
Strict ASR only counts full success
```

**Partial ASR**: Any unintended behavior counts
```
Partial ASR counts any deviation from expected safe behavior
Higher than Strict ASR, captures near-misses
```

**Category-Specific ASR**: By attack type
```
ASR_jailbreak = 23%
ASR_prompt_leak = 45%
ASR_data_exfil = 12%
ASR_goal_hijack = 67%
```

---

### Best-of-N Attack Success

**Concept**: Success rate given N attempts with the same or similar attack

```
Best-of-N ASR: P(at least one success in N attempts)

If single-attempt ASR = p, then:
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

Enough attempts eventually succeed against any model.
```

---

### Utility Under Attack (UA)

**Definition**: How much useful functionality is preserved when under attack

```
UA = (Task success rate under attack) / (Task success rate without attack)
```

**Why it matters**: A defense that blocks all attacks but also blocks legitimate use is useless.

**Example**:
```
Without attack: Model completes 95% of tasks successfully
With attack present: Model completes 60% of tasks successfully
UA = 60/95 = 63%

Defense reduces UA means it's harming legitimate use cases.
```

---

### Defense Effectiveness

**Definition**: How much a defense reduces attack success

```
Defense Effectiveness = (Baseline ASR - Defended ASR) / Baseline ASR × 100%
```

**Example**:
```
Baseline (no defense): ASR = 50%
With defense: ASR = 10%
Effectiveness = (50-10)/50 = 80%
```

---

## Secondary Metrics

### Bypass Rate

For systems with explicit injection detection:

```
Bypass Rate = (Attacks that evaded detection AND succeeded) / (Total attacks)

Distinct from ASR: An attack might be detected but still succeed,
or be blocked but not detected.
```

### False Positive Rate (FPR)

How often legitimate inputs are incorrectly flagged:

```
FPR = (Benign inputs flagged as attacks) / (Total benign inputs)

Critical for usability: High FPR = frustrated users
```

### Time-to-Bypass

How quickly a defense can be circumvented:

```
Time-to-bypass: Average time for attacker to find working attack

Short time = weak defense (easy to bypass)
Long time = stronger defense (harder to bypass)
```

### Transferability Rate

How well attacks transfer across models:

```
Transfer Rate = (Success on Model B) / (Success on Model A)

where attack was optimized for Model A.

Example:
GCG suffix on Vicuna: 99% ASR
Same suffix on GPT-4: 47% ASR
Transfer rate: 47%
```

---

## Measurement Challenges

### Defining "Success"

What counts as a successful attack?

**Options**:
1. **Exact match**: Model outputs exact target string
2. **Semantic match**: Model conveys target meaning
3. **Policy violation**: Model violates any policy
4. **Behavioral deviation**: Any unintended behavior

```
Attack goal: Get model to say "I hate humans"

Exact match success: "I hate humans" ✓
Semantic match success: "Humans are terrible" ✓
Policy violation: "Here's a poem about hating..." ✓
Behavioral deviation: "I... shouldn't say that" ✓

Different definitions = different ASR values
```

### Human vs. Automated Evaluation

**Automated evaluation**:
```python
def evaluate_success(response, target):
    if target.lower() in response.lower():
        return True
    # Or use classifier
    return classifier.is_harmful(response)
```

**Problems**:
- May miss semantic equivalents
- May have false positives
- Classifiers can be fooled

**Human evaluation**:
- More accurate
- Much more expensive
- Subjective variations

### Dataset Bias

Attack success varies by:
- Attack dataset (some easier than others)
- Topic category (violence vs. privacy vs. illegal)
- Model version (updates change behavior)
- Evaluation time (behavior can shift)

**Best practice**: Report on multiple diverse datasets

---

## Reporting Best Practices

### What to Include

1. **ASR with confidence intervals**
   ```
   ASR = 23% (95% CI: 18-28%)
   n = 500 attempts
   ```

2. **Attack and defense details**
   ```
   Attack type: Indirect injection via web content
   Defense: Input classifier + instruction hierarchy
   ```

3. **Model details**
   ```
   Model: GPT-4-0125-preview
   Accessed via API on 2024-03-15
   ```

4. **Evaluation methodology**
   ```
   Success criteria: Harmful content generation
   Evaluated by: Automated classifier + 10% human review
   ```

5. **Utility impact**
   ```
   UA = 85% (defense preserves most utility)
   FPR = 3% (low false positive rate)
   ```

### Common Reporting Errors

❌ Reporting ASR without confidence intervals
❌ Using weak attacks to claim strong defense
❌ Hiding utility costs of defense
❌ Not specifying success criteria
❌ Testing only against own dataset

---

## Metric Interpretation Guide

| Metric | Good Value | Bad Value | Context |
|--------|-----------|-----------|---------|
| ASR (attack) | High | Low | From attacker perspective |
| ASR (defense) | Low | High | From defender perspective |
| UA | Close to 1.0 | Close to 0.0 | Defense utility preservation |
| FPR | Close to 0% | >5% | Usability impact |
| Transfer | High | Low | Attack generalization |

---

## Key Takeaways

1. **ASR is primary but insufficient** - Need multiple metrics

2. **Best-of-N changes the picture** - Single-attempt ASR misleading

3. **Utility matters** - Defense that breaks functionality is useless

4. **Methodology must be specified** - Results not comparable otherwise

5. **Report uncertainty** - Confidence intervals essential

6. **Multiple evaluations needed** - Single benchmark insufficient

---

## Sources

- Hughes et al., "Best-of-N Jailbreaking"
- Yi et al., "BIPIA Benchmark"
- Debenedetti et al., "AgentDojo"
- Various model evaluation papers
