# Documentation Refinement Agent

## Overview

The `doc-refiner` agent applies 31 quality principles to improve documentation clarity, brevity, and efficiency. It restructures content, removes fluff, and enforces consistency using templates—without inventing new content.

## Summary

- Processes single files or entire directories of `.md` files
- Three-phase workflow: Analyze → Refine → Verify
- Enforces structure (Overview/Summary sections), brevity (no meta-commentary), and precision (no weasel words)
- Uses templates to ensure consistency across standalone and related documents
- Preserves technical accuracy while eliminating redundancy

---

## Workflow

### 1. Analyze
- Reads document
- Determines type (standalone vs. related/series)
- Scans against quality checklist
- Lists violations

### 2. Refine
Fixes violations in priority order:
1. **Structure** - Add Overview/Summary, fix heading hierarchy
2. **Brevity** - Remove meta-commentary, passive voice, hand-holding
3. **Clarity** - Add concrete examples, enforce active voice
4. **Precision** - Quantify claims, remove weasel words
5. **Navigation** - Add cross-references where relevant

Applies templates:
- **Standalone**: `.doc-templates/standalone.md`
- **Related**: `.doc-templates/related.md`

### 3. Verify
- Re-scans against checklist
- Confirms template compliance
- Outputs change report

## Usage

**Single file:**

Just say:

```
Use doc-refiner on path/to/file.md
```

**Directory:**
```
Use doc-refiner on path/to/directory/
```

The agent processes all `.md` files in the directory and outputs a summary table.

## Configuration

| File | Purpose |
|------|---------|
| `.claude/agents/doc-refiner.md` | Agent definition and workflow |
| `.claude/skills/doc-quality-rules.md` | 31 quality principles and anti-patterns |
| `.doc-templates/standalone.md` | Template for isolated documents |
| `.doc-templates/related.md` | Template for series/related documents |

## Capabilities

- Restructures documents to match template requirements
- Removes meta-commentary, passive voice, and hand-holding transitions
- Converts prose comparisons to tables
- Fixes heading hierarchy (H1 → H2 → H3, no skipping)
- Enforces consistent terminology
- Adds Overview and Summary sections where missing
- Formats inline code for technical terms

## Limitations

- Restructures only existing content (does not generate new material)
- Complex documents may require manual review after processing
- Preserves technical accuracy over stylistic preferences
- Cannot resolve ambiguous or contradictory content
