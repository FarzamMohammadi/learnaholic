## Summary: Designing AI Experiences People Actually Use

**Source:** https://buzzusborne.com/writing/designing-ai-for-trust/
**Type:** article
**Original length:** ~2,400 words | **Summary:** ~850 words

---

### Core Message

AI-first interfaces are failing not because AI is bad, but because they redistribute cognitive labor onto users without deliberate design. Products that open with blank prompts like "What do you want to make?" force users to articulate intent before exploration begins — inverting decades of interaction design where users learned through doing. Three behavioral forces — trust, value perception, and cognitive effort — determine whether AI features survive or fail, and they amplify one another: low trust increases perceived effort, high effort reduces perceived value, and low value further undermines trust.

---

### Key Points

#### The Core Problem: AI Inverts the Input-Output Sequence

- Traditional SaaS products reduced ambiguity: users supplied constrained inputs, the system handled output, and outcomes emerged through trial-and-error interaction. AI-first interfaces invert this by asking users to declare intent before exploration begins — to "articulate the destination before they hit the road."
- A reporting tool that once let you throw numbers in and experiment now says "Describe the graph you'd like to create," transferring the burden of abstraction onto the user. A writing tool that opens with "What article should we write?" assumes clarity that creative work simply does not start with.
- This is a redistribution of cognitive labor, not a feature addition. When that shift is not deliberately designed for, core elements of the experience erode.

#### Trust

- When users must trust an AI tool before any meaningful interaction, adoption stalls. Trust in AI is also declining year over year — both in output accuracy confidence and ethical data stewardship — across consumers, early adopters, and business audiences alike.
- Users tolerate AI as a suggestion engine far more readily than as an autonomous agent. When a product claims it can write, design, code, or decide for the user, psychological stakes increase.
- Critically, trust does not need to precede adoption — it can emerge through usage. Salesforce's research found that "human validation of outputs is the biggest driver in trusting the outcome, over consistently accurate outputs." Users trust systems they can interrogate, shape, and verify.
- The design implication: rather than building AI that is perfect, build experiences that are controllable. An AI that drafts a response outline, surfaces relevant context, and recommends a teammate — while the human edits, decides, and adds judgment — earns trust through interaction. Delegation becomes possible because it has been earned; you cannot start there.
- Sequencing autonomy gradually also reduces the cost of failure. An incorrect suggestion becomes a minor correction rather than a catastrophe.

#### Value Perception

- The problem is not capability but recognizability — whether the average user can immediately see why an AI feature is worth their time, and whether the perceived effort feels proportionate to the return.
- Perceived ease of use often has a stronger impact on adoption than perceived usefulness. Even powerful systems are abandoned if the cost of engaging feels ambiguous or high. This explains why simpler, constrained alternatives often win (the author contrasts MidJourney vs. Nano Banana).
- Prompt-first interfaces like "Ask us anything about your Inbox" fail because value must be imagined — a cognitively expensive activity that rarely converts. Broad questions require users to simulate a benefit that has not yet been demonstrated.
- The better approach: lead with demonstrated value. Surface a surprising customer trend, highlight a risk, present something concrete — then invite exploration. Show, don't tell. Users form understanding based on demonstrated value, not hypothetical.

#### Cognitive Effort

- Even when users trust a system and believe it is valuable, adoption fails if the mental energy required to shape a desirable outcome is too high.
- Feature complexity is not the biggest risk — the blank page is. Nielsen calls this an "articulation barrier." Google research calls it "open-intent paralysis." Microsoft UX studies found that "unaided, free-form prompting is one of the biggest barriers to mainstream adoption."
- When power is hidden behind a blank prompt, effort increases, value perception drops, and trust erodes. Users feel success depends on their ability to "speak AI" — and most people are not good prompt engineers. Any design that depends on user eloquence is fragile design.
- The fix is often small: structure prompts, offer contextual suggestions, propose starting points. Compare "Tell me your credit card categories and I'll sort your spending" to "Here are 10 categories people like you typically use. Should I sort your spending into these?" — the second proposes structure and invites modification, dramatically reducing cognitive load.
- The principle: design interfaces where users react rather than invent. Absorb structural thinking into the system rather than demanding it from the user.

#### Cost of Failure

- Even with trust, value, and effort well balanced, the first few AI interactions are often mediocre — context is light, preferences unclear, outputs generic. This gap between mediocrity and magic is a huge barrier to entry.
- As AI becomes more autonomous, the cost of failure rises disproportionately. An AI replying incorrectly to a high-value customer or writing code in the wrong framework can cost revenue, reputation, and relationships.
- Assistive "background" AI features carry much lower failure costs. An imperfect suggestion can be ignored; a misaligned recommendation becomes a small calibration moment rather than a public error. Users tolerate imperfection when they retain control.

---

### Specifics Worth Preserving

- **Sources cited:** Pew Research Center (2025), International Journal of Human-Computer Interaction (2022), Frontiers in Psychology (2024), Salesforce State of the Connected Customer (2024), Google People & AI Guidebook (2026), Figma State of the Designer 2026
- **Key quote from IJHCI:** "Perceived ease of use and perceived usefulness were essential determinants for the use of AI technologies, although perceived ease of use had a consistently greater impact on the acceptance of these technologies."
- **Key quote from Salesforce:** "Human validation of outputs is the biggest driver in trusting the outcome, over consistently accurate outputs."
- **Named concepts:** Nielsen's "articulation barrier," Google's "open-intent paralysis," Microsoft's finding that unaided free-form prompting is a top barrier to mainstream adoption
- **DORA's Fostering Trust in AI report** highlights growing skepticism among software engineers specifically
- **Reinforcing loop:** Low trust increases perceived effort. High effort reduces perceived value. Low value further undermines trust.

---

### Actionable Takeaways

- Design AI as an accelerant within human control, not a replacement — earn delegation through demonstrated competence over time.
- Sequence autonomy gradually: start with suggestion/assistive modes, then introduce more autonomous options as trust builds through successful interactions.
- Lead with demonstrated value rather than blank prompts. Surface concrete insights, patterns, or recommendations before asking users to articulate intent.
- Reduce cognitive load by proposing structure users can modify rather than requiring them to generate structure from scratch.
- Keep the cost of failure low by defaulting to assistive modes where mistakes are minor corrections, not public errors.
