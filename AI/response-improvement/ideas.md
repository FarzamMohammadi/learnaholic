# Model Output Improvement Techniques

Techniques for improving LLM output quality through sampling, evaluation, and feedback loops.

## Techniques

### [1. Best-of-N Sampling](./1-best-of-n-sampling.md)
Generate multiple responses to the same prompt, score them, and return the best one. A simple way to improve output quality without retraining. Includes scoring via reward models, human selection, or AI judges.

### [2. AI as a Judge](./2-ai-as-a-judge.md)
Use an LLM to systematically evaluate responses against specific criteria. Enables pattern detection, preference pair generation, synthetic training data creation, and regression testing at scale.

### [3. Retroactive AI Evaluation](./3-retroactive-ai-evaluation.md)
Apply AI-as-a-Judge to historical interactions to create training data from existing logs. Generates alternatives for past responses, evaluates them, and extracts high-quality examples without new human labeling.
