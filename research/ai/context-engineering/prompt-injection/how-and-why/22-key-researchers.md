# 22 - Key Researchers

[← Previous](21-KEY-PAPERS.md) | [Index](00-INDEX.md)

---

## Overview

This document catalogs the individuals, teams, and institutions driving prompt injection research. From independent researchers who first identified the vulnerability to industry labs developing defenses, these contributors shape our understanding of LLM security.

## Summary

- Independent researchers coined the term and demonstrated first attacks
- Academic labs formalized attack methodologies and created benchmarks
- Industry teams develop defenses while disclosing vulnerabilities through responsible channels
- Research spans security, ML safety, and adversarial machine learning communities

---

## Independent Researchers

### Simon Willison

Independent researcher and Django co-creator. Coined "prompt injection" in September 2022 and maintains extensive documentation through simonwillison.net and Substack. Developed the "Lethal Trifecta" framework for agent risk assessment. Advocates for developing software with the assumption that prompt injection "isn't fixed now and won't be fixed for the foreseeable future."

**Impact**: Established terminology and raised mainstream awareness.

### Johann Rehberger

Security researcher at Wunderwuzzi. Runs "Month of AI Bugs" with daily vulnerability disclosures. Discovered markdown image exfiltration and cross-plugin attacks in ChatGPT. Multiple responsible disclosures led to patches in production systems.

**Resources**: embracethered.com

### Riley Goodside

Staff prompt engineer at Scale AI. Posted first viral prompt injection demonstrations in September 2022. Ongoing exploration of prompt engineering limits and creative attack discovery brought the vulnerability to mainstream awareness.

---

## Academic Researchers

### Kai Greshake
*Saarland University*

Lead author of "Not what you've signed up for" (2023), which formalized indirect prompt injection and demonstrated real-world attack vectors.

### Andy Zou
*Carnegie Mellon University*

Lead author of "Universal and Transferable Adversarial Attacks on Aligned Language Models." Introduced gradient-based GCG attack methodology, shifting the paradigm from manual to automated attacks.

### Florian Tramèr
*ETH Zurich*

LLM security researcher. Created AgentDojo benchmark and CaMeL framework for systematic agent security evaluation.

### Nicholas Carlini
*Google DeepMind*

Adversarial ML researcher. Co-author on GCG paper. Extensive background in adversarial machine learning applied to LLM safety.

### Edoardo Debenedetti
*ETH Zurich / Google DeepMind*

Developer of AgentDojo and CaMeL framework. Proposes architectural defenses and capability-based security for LLMs.

### Sam Toyer
*UC Berkeley CHAI*

Creator of TensorTrust benchmark. Pioneered crowdsourced attack collection through gamification, producing large-scale human-generated datasets.

---

## Industry Research Teams

### Anthropic Security Team
*Key contributors: Amanda Askell, et al.*

Developed Constitutional AI methodology and Claude safety training. Published browser use defense paper (2025) achieving ~1% attack success rate.

### OpenAI Safety Team
*Key contributors: Eric Wallace, et al.*

Produced Instruction Hierarchy paper (2024) and Model Spec documentation. Develops GPT safety training approaches.

### Google DeepMind Security

Published "Lessons from Defending Gemini Against Prompt Injection" (2025). Developed adversarial training methodology and CaMeL architectural defense.

### Microsoft Security Research
*Key contributors: Mark Russinovich, Ahmed Salem, Eldan*

Published Crescendo multi-turn attack paper. Develops Azure AI Content Safety and Prompt Shield for enterprise deployments.

### Meta AI Security

Creates open-source safety tools including Llama Guard, Purple Llama, Prompt Guard models, and LlamaFirewall.

---

## Research Institutions

| Institution | Focus | Key Output |
|-------------|-------|------------|
| CMU Language Technologies Institute | Adversarial attacks on LLMs | GCG attack, safety analysis |
| ETH Zurich SPY Lab | Agent security, benchmarking | AgentDojo, CaMeL |
| UC Berkeley CHAI | AI alignment, safety | TensorTrust, alignment research |
| Stanford HAI | AI safety and policy | Cross-domain safety research |

---

## Bug Bounty Community

Anonymous and named security researchers report vulnerabilities through:
- OpenAI bug bounty program
- Anthropic vulnerability reporting
- Google Vulnerability Reward Program
- Microsoft Security Response Center

Many operate anonymously but have driven significant security improvements in production systems.

---

## Key Takeaways

- Research spans three communities: independent security researchers (attack discovery), academic labs (formalization and benchmarks), and industry teams (defensive development)
- Simon Willison's naming and advocacy established the field's foundation in September 2022
- CMU's GCG attack and ETH's AgentDojo represent paradigm shifts in methodology
- Bug bounty programs surface real-world vulnerabilities faster than academic publication cycles

## Sources

- Simon Willison - [simonwillison.net](https://simonwillison.net)
- Johann Rehberger - [embracethered.com](https://embracethered.com)
- Zou et al. - "Universal and Transferable Adversarial Attacks" (2023)
- Greshake et al. - "Not what you've signed up for" (2023)
- Anthropic - Browser use defense paper (2025)
- OpenAI - Instruction Hierarchy paper (2024)
- Google DeepMind - "Lessons from Defending Gemini" (2025)
- Microsoft - Crescendo paper
- OWASP LLM Project
- Various conference proceedings: NeurIPS, ICML, USENIX Security, IEEE S&P, ACL/EMNLP, DEF CON AI Village

---

[← Previous](21-KEY-PAPERS.md) | [Index](00-INDEX.md)
