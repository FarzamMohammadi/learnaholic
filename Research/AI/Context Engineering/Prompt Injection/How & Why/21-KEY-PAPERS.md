# 21 - Key Papers: Essential Research on Prompt Injection

## Annotated Bibliography of Foundational and Current Research

---

## Foundational Papers

### 1. "Not what you've signed up for: Compromising Real-World LLM-Integrated Applications with Indirect Prompt Injection"

**Authors**: Greshake, Abdelnabi, Mishra, Endres, Holz, Fritz
**Date**: February 2023
**Venue**: arXiv (later AISec 2023)
**Citation**: arXiv:2302.12173

**Summary**:
The paper that formalized indirect prompt injection. Demonstrates that LLMs processing external content (web pages, documents, emails) can be compromised by hidden instructions in that content.

**Key Contributions**:
- Introduced "indirect prompt injection" terminology
- Demonstrated attacks on Bing Chat, ChatGPT plugins
- Showed RAG system vulnerabilities
- Proposed initial taxonomy

**Critical Findings**:
- LLMs cannot reliably distinguish instructions from data
- Retrieval-augmented systems are particularly vulnerable
- Plugin/tool use dramatically increases risk

**Why It Matters**: Foundational paper that shifted focus from direct to indirect injection.

---

### 2. "Universal and Transferable Adversarial Attacks on Aligned Language Models"

**Authors**: Zou, Wang, Kolter, Carlini, Fredrikson
**Date**: July 2023
**Venue**: arXiv (CMU research)
**Citation**: arXiv:2307.15043

**Summary**:
Introduced GCG (Greedy Coordinate Gradient), showing that algorithmically optimized adversarial suffixes can jailbreak aligned LLMs, and these suffixes transfer across models.

**Key Contributions**:
- First gradient-based attack on aligned LLMs
- Demonstrated transferability to closed-source models
- Introduced automated attack generation methodology
- Achieved 84%+ success on GPT-3.5-turbo

**Critical Findings**:
- Safety alignment is not robust to optimization-based attacks
- Suffixes optimized on open models attack closed models
- The attack surface is much larger than previously thought

**Why It Matters**: Paradigm shift from human-crafted to automated attacks.

---

### 3. "Ignore This Title and HackAPrompt: Exposing Systemic Vulnerabilities of LLMs through a Global Scale Prompt Hacking Competition"

**Authors**: Perez, Ribeiro, et al.
**Date**: 2023
**Venue**: EMNLP 2023

**Summary**:
Describes the HackAPrompt competition at NeurIPS, analyzing 600,000+ prompt injection attempts and identifying systematic vulnerability patterns.

**Key Contributions**:
- Massive dataset of real attack attempts
- Taxonomy of successful attack strategies
- Analysis of defense bypasses
- Competition-based evaluation methodology

**Why It Matters**: Empirical grounding for prompt injection research.

---

## Attack Research

### 4. "Great, Now Write an Article About That: The Crescendo Multi-Turn LLM Jailbreak Attack"

**Authors**: Russinovich, Salem, Eldan
**Date**: 2024
**Venue**: Microsoft Research

**Summary**:
Formalizes multi-turn jailbreak attacks where harmful requests are built up gradually across conversation turns.

**Key Contributions**:
- Formal description of multi-turn attacks
- 29-61% improvement over single-turn attacks
- Works across GPT-4, Claude, Gemini
- Demonstrates conversation-level vulnerability

**Critical Finding**: "Gradual escalation dramatically increases attack success rate."

---

### 5. "Tensor Trust: Interpretable Prompt Injection Attacks from an Online Game"

**Authors**: Toyer, Watkins, Menber, et al.
**Date**: 2023
**Venue**: NeurIPS 2023
**Citation**: arXiv:2311.01011

**Summary**:
Describes TensorTrust, a gamified platform for collecting prompt injection attacks and defenses, resulting in 563,000+ human-generated samples.

**Key Contributions**:
- Massive crowdsourced dataset
- Analysis of attack strategy patterns
- Defense evaluation methodology
- Publicly available benchmark

---

### 6. "Jailbroken: How Does LLM Safety Training Fail?"

**Authors**: Wei, Haghtalab, et al.
**Date**: 2023
**Venue**: NeurIPS 2023

**Summary**:
Systematic analysis of why safety training fails against jailbreaks, categorizing failure modes and attack strategies.

**Key Contributions**:
- Taxonomy of jailbreak categories
- Analysis of safety training limitations
- Competing objectives framework
- Defense recommendations

---

### 7. "AutoDAN: Generating Stealthy Jailbreak Prompts on Aligned Large Language Models"

**Authors**: Liu, Xu, et al.
**Date**: 2023

**Summary**:
Introduces genetic algorithm approach for generating adversarial prompts that are more readable than GCG-style attacks.

**Key Contributions**:
- Automated prompt generation via genetic algorithm
- More human-readable attacks
- High success rates
- Demonstrates attack automation scalability

---

### 8. "PoisonedRAG: Knowledge Corruption Attacks to Retrieval-Augmented Generation of Large Language Models"

**Authors**: Zou, et al.
**Date**: 2024

**Summary**:
Demonstrates that injecting just 5 malicious documents into a corpus of millions achieves 90%+ attack success rate on RAG systems.

**Key Findings**:
- Minimal poisoning achieves high impact
- RAG systems highly vulnerable to corpus poisoning
- Traditional document security insufficient

---

## Defense Research

### 9. "The Instruction Hierarchy: Training LLMs to Prioritize Privileged Instructions"

**Authors**: Wallace, Xiao, et al. (OpenAI)
**Date**: April 2024
**Citation**: arXiv:2404.13208

**Summary**:
OpenAI's approach to teaching models to prioritize system-level instructions over user inputs and tool outputs.

**Key Contributions**:
- Formal instruction hierarchy: System > Developer > User > Tool
- Training methodology for hierarchy enforcement
- Evaluation on injection resistance
- Production deployment considerations

**Limitations Noted**: System prompts can still be extracted, not a complete solution.

---

### 10. "Lessons from Defending Gemini Against Indirect Prompt Injections"

**Authors**: Google DeepMind Team
**Date**: 2025
**Citation**: arXiv:2505.14534

**Summary**:
Google's comprehensive analysis of defenses deployed for Gemini, including four-layer architecture and effectiveness measurements.

**Key Contributions**:
- Four-layer defense architecture
- Adversarial training methodology
- 47% ASR reduction through combined defenses
- Honest assessment of remaining vulnerabilities

---

### 11. "Mitigating the risk of prompt injections in browser use"

**Authors**: Anthropic
**Date**: January 2025

**Summary**:
Anthropic's approach to securing Claude's browser automation capabilities against indirect prompt injection.

**Key Contributions**:
- ~1% attack success rate achieved
- Constitutional classifier approach
- Multi-layer defense architecture
- Explicit acknowledgment of ongoing challenge

---

### 12. "CaMeL: Capability-based Access Control for LLM Agents"

**Authors**: Debenedetti, et al. (Google DeepMind)
**Date**: 2025

**Summary**:
Proposes architectural solution using capability-based security and information flow control rather than relying on model robustness.

**Key Contributions**:
- Control Flow Integrity for LLM agents
- Capability-based security model
- Information flow control implementation
- Near 100% attack blocking, 77% task completion

**Why It Matters**: First architectural (not training-based) defense with strong results.

---

### 13. "SecAlign: Defending Against Prompt Injection with Preference Optimization"

**Authors**: Chen, et al.
**Date**: 2024/2025

**Summary**:
Uses Direct Preference Optimization to train injection resistance, achieving <10% ASR even against sophisticated attacks.

**Key Contribution**: Meta-SecAlign-70B is first open-source LLM with built-in injection defense.

---

## Evaluation and Benchmarking

### 14. "Benchmarking and Defending Against Indirect Prompt Injection Attacks on Large Language Models"

**Authors**: Yi, et al.
**Date**: 2023
**Venue**: KDD 2025 (accepted)
**Citation**: arXiv:2312.14197

**Summary**:
Introduces BIPIA benchmark and demonstrates the counterintuitive finding that more capable models are more vulnerable.

**Key Contribution**: Capability-vulnerability correlation finding.

---

### 15. "AgentDojo: A Dynamic Environment to Evaluate Attacks and Defenses for LLM Agents"

**Authors**: Debenedetti, et al.
**Date**: 2024

**Summary**:
Benchmark specifically for agentic LLM systems with tool use, introducing "Utility Under Attack" metric.

**Key Contribution**: First systematic agent security benchmark.

---

## Mechanistic Research

### 16. "TwinBreak: Jailbreaking LLM Security Alignments based on Contrastive Layer-Wise Analysis"

**Authors**: Krauss, et al.
**Date**: 2025
**Venue**: USENIX Security 2025

**Summary**:
Demonstrates that safety mechanisms are localized to specific neurons/layers and can be ablated without destroying capability.

**Key Finding**: "Safety is a thin layer, not deeply integrated."

**Implications**: 
- Safety training may be less robust than assumed
- Mechanistic attacks possible
- New defense approaches needed

---

### 17. "Attention Tracker: Detecting Prompt Injection Attacks in LLMs"

**Authors**: Xiang, et al.
**Date**: 2025
**Venue**: NAACL Findings 2025

**Summary**:
Analyzes attention patterns during prompt injection, showing characteristic "distraction effects" that can be detected.

**Key Contribution**: Mechanistic detection approach based on attention analysis.

---

## Survey Papers

### 18. "Prompt Injection Attacks in Large Language Models and AI Agent Systems: A Comprehensive Review"

**Authors**: Various
**Date**: 2025 (MDPI Information)

**Summary**:
Comprehensive survey covering attack taxonomy, defense mechanisms, and future directions.

**Useful For**: Overall landscape understanding.

---

## Reading Recommendations

### For Understanding the Problem
1. Greshake et al. (Indirect Injection) - Foundational
2. Zou et al. (GCG) - Automated attacks
3. Wei et al. (Jailbroken) - Why safety fails

### For Defense Approaches
1. Wallace et al. (Instruction Hierarchy) - OpenAI approach
2. Google DeepMind (Gemini Lessons) - Layered defense
3. Debenedetti et al. (CaMeL) - Architectural solution

### For Evaluation
1. Yi et al. (BIPIA) - Indirect injection benchmark
2. Debenedetti et al. (AgentDojo) - Agent benchmark
3. Toyer et al. (TensorTrust) - Crowdsourced dataset

---

## Sources and Access

Most papers available on:
- arXiv.org
- OpenReview.net
- ACL Anthology
- Vendor research blogs (Anthropic, OpenAI, Google, Microsoft)
