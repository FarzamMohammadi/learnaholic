# Research Directory

## Purpose

Research documentation on AI, Context Engineering, Prompt Injection, and related topics.

## Documentation Agent

Use `doc-refiner` for documentation refinement:
- **Single file**: `Use doc-refiner on path/to/file.md`
- **Directory**: `Use doc-refiner on path/to/directory/`
- **Auto-delegation**: Ask to "review" or "refine" documentation

Agent location: `.claude/agents/doc-refiner.md` (includes all 31 quality rules)

## Templates

| Type | Path | Use When |
|------|------|----------|
| Standalone | `.doc-templates/standalone.md` | Isolated files, no series |
| Related | `.doc-templates/related.md` | Part of numbered series |

## Directory Structure

```
Research/
├── AI/Context Engineering/Prompt Injection/
│   ├── How & Why/     # Attack mechanisms (related series)
│   └── Prevention/    # Defense strategies (related series)
├── .claude/
│   └── agents/        # Custom agents
└── .doc-templates/    # Document templates
```
