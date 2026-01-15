# 22 - Key Researchers: Notable Contributors to Prompt Injection Research

## Individuals and Teams Advancing the Field

---

## Independent Researchers

### Simon Willison

**Role**: Developer advocate, AI security researcher
**Affiliation**: Independent (formerly Django co-creator)
**Contribution**: Coined "prompt injection" term, extensive documentation

**Key Work**:
- Named the vulnerability (September 2022)
- Ongoing blog coverage of attacks and defenses
- "Lethal Trifecta" framework for agent risk assessment
- Advocacy for realistic security expectations

**Notable Quotes**:
> "Develop software with the assumption that this issue isn't fixed now and won't be fixed for the foreseeable future."

**Resources**: simonwillison.net, Substack newsletter

---

### Johann Rehberger

**Role**: Security researcher, red teamer
**Affiliation**: Independent / Wunderwuzzi
**Contribution**: "Month of AI Bugs" daily vulnerability disclosures

**Key Work**:
- Discovered markdown image exfiltration
- ChatGPT plugin vulnerabilities
- Cross-plugin attacks
- Ongoing vulnerability research on major systems

**Impact**: Responsible for many patches in production systems

**Resources**: embracethered.com

---

### Riley Goodside

**Role**: Prompt engineer, researcher
**Affiliation**: Scale AI (staff prompt engineer)
**Contribution**: First public prompt injection demonstrations

**Key Work**:
- September 2022 demonstrations that went viral
- Ongoing exploration of prompt engineering limits
- Creative attack discovery

**Impact**: Brought prompt injection to mainstream awareness

---

## Academic Researchers

### Kai Greshake

**Affiliation**: Saarland University
**Contribution**: Lead author of foundational indirect injection paper

**Key Work**:
- "Not what you've signed up for" (2023)
- Formalized indirect prompt injection
- Demonstrated real-world attacks

---

### Andy Zou

**Affiliation**: CMU
**Contribution**: Lead author of GCG adversarial suffix paper

**Key Work**:
- "Universal and Transferable Adversarial Attacks"
- Introduced gradient-based attack methodology
- Demonstrated transfer attacks

**Impact**: Changed the paradigm from manual to automated attacks

---

### Florian Tramèr

**Affiliation**: ETH Zurich
**Contribution**: LLM security research, AgentDojo

**Key Work**:
- AgentDojo benchmark
- CaMeL framework
- Systematic agent security evaluation

---

### Nicholas Carlini

**Affiliation**: Google DeepMind
**Contribution**: Adversarial ML, LLM security

**Key Work**:
- Co-author on GCG paper
- Extensive adversarial ML background
- LLM safety research

---

### Edoardo Debenedetti

**Affiliation**: ETH Zurich / Google DeepMind
**Contribution**: AgentDojo, CaMeL framework

**Key Work**:
- Agent security benchmarking
- Architectural defense proposals
- Capability-based security for LLMs

---

### Sam Toyer

**Affiliation**: UC Berkeley (CHAI)
**Contribution**: TensorTrust benchmark

**Key Work**:
- Crowdsourced attack collection
- Gamification of security research
- Large-scale human-generated dataset

---

## Industry Research Teams

### Anthropic Security Team

**Key Researchers**: Various (Amanda Askell, et al.)
**Contribution**: Constitutional AI, Claude safety

**Key Work**:
- Constitutional AI methodology
- Claude safety training
- Browser use defense paper (2025)
- ~1% ASR achievement

---

### OpenAI Safety Team

**Key Researchers**: Eric Wallace, et al.
**Contribution**: Instruction hierarchy, model spec

**Key Work**:
- Instruction Hierarchy paper (2024)
- Model Spec documentation
- GPT safety training

---

### Google DeepMind Security

**Key Researchers**: Various
**Contribution**: Gemini defenses, CaMeL

**Key Work**:
- "Lessons from Defending Gemini" (2025)
- Adversarial training methodology
- CaMeL architectural defense

---

### Microsoft Security Research

**Key Researchers**: Mark Russinovich, Ahmed Salem, Eldan
**Contribution**: Crescendo, enterprise security

**Key Work**:
- Crescendo multi-turn attack paper
- Azure AI Content Safety
- Prompt Shield

---

### Meta AI Security

**Key Researchers**: Various
**Contribution**: Llama Guard, Purple Llama

**Key Work**:
- Open-source safety tools
- Prompt Guard models
- LlamaFirewall

---

## Research Groups and Labs

### CMU Language Technologies Institute

**Focus**: Adversarial attacks on LLMs
**Key Output**: GCG attack, safety analysis

### ETH Zurich SPY Lab

**Focus**: Agent security, benchmarking
**Key Output**: AgentDojo, CaMeL

### UC Berkeley CHAI (Center for Human-Compatible AI)

**Focus**: AI alignment, safety
**Key Output**: TensorTrust, alignment research

### Stanford HAI

**Focus**: AI safety and policy
**Key Output**: Various safety research

---

## Bug Bounty Contributors

Various security researchers who've reported vulnerabilities through:
- OpenAI bug bounty program
- Anthropic vulnerability reporting
- Google VRP
- Microsoft MSRC

Many operate anonymously but have significantly improved system security.

---

## Following the Field

### Recommended Follows
- Simon Willison's Substack/Blog
- embracethered.com (Rehberger)
- arXiv cs.CR and cs.CL tags
- OWASP LLM project

### Conferences
- NeurIPS (safety track)
- ICML (safety workshop)
- USENIX Security
- IEEE S&P
- ACL/EMNLP
- DEF CON AI Village

### Mailing Lists and Forums
- ML Safety mailing list
- AI Security communities
- OWASP Slack channels
