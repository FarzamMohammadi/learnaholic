# AI as a Judge (LLM-as-a-Judge)

Use an LLM to systematically evaluate responses against specific criteria.

## Example Evaluation Criteria

For a customer support bot:
- Directed user to correct page/action
- Response time under 3 seconds
- All required links included
- No hallucinations or incorrect info
- Appropriate tone (professional, empathetic)
- Solved user's problem

## Example Judge Prompt

```
Evaluate this customer support interaction:

User: "I can't access my account"
Response: "I can help! Try resetting your password at settings.com/reset"

Criteria:
1. Correct solution provided? (Yes/No + explanation)
2. All necessary links included? (List any missing)
3. Appropriate tone? (1-5 scale)
4. Overall quality? (1-5 scale)

Provide scores in JSON format.
```

## Applications

### Pattern Detection
- Analyze 1000s of interactions
- Find systematic failures (e.g., "model never includes refund policy link")
- Fix prompt or fine-tune

### Preference Pair Generation
- Good response (score 4.5) vs Bad response (score 2.1)
- Creates training data: Good > Bad
- Use for RLHF/fine-tuning

### Synthetic Training Data
- Extract all high-scoring responses (4.5+)
- Use as examples in few-shot prompts
- Or create fine-tuning dataset

### Regression Testing
- Evaluate new model versions on same test cases
- Ensure quality doesn't degrade
- Example: "New model scores 4.2 avg vs old 4.5 - investigate"

## Pro Tips

- **Structured outputs**: Use JSON so you can aggregate scores programmatically
- **Multi-judge**: Run 2-3 judges, take consensus (reduces bias/variance)
- **Validate your judge**: Spot-check 10% manually to ensure judge is reliable
- **Include negative examples**: Show judge what bad responses look like
