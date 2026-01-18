# Best-of-N Sampling

Generate N responses to the same prompt, score them, and return the best one. Improves output quality without retraining the model.

## Scoring Approaches

### Automated Reward Model
- Model generates 5 responses
- Reward model scores each (e.g., 0.82, 0.91, 0.67, 0.88, 0.73)
- Return highest-scoring response (0.91)

### Human Selection
- Show user multiple responses
- User picks preferred one
- Collect preference data for training
- Example: ChatGPT's regenerate button creates implicit preference when users keep regenerating

### AI Judge
- LLM evaluates each response against criteria
- Ranks and selects best one
- Can provide reasoning for selection

## Related Techniques

### RLHF (Reinforcement Learning from Human Feedback)
- Humans compare pairs of responses (A vs B)
- Preference data trains a reward model
- Reward model guides future generation
- Used by ChatGPT, Claude, and most modern LLMs

### RLAIF (Reinforcement Learning from AI Feedback)
- AI provides feedback instead of humans
- Scales better, more consistent
- Still requires good base model for judging
