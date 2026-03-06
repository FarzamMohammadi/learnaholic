---
name: summarize
description: Distill files, URLs, and videos into thorough summaries that preserve every valuable detail while cutting fluff. Use when user wants to summarize, get a tl;dr, extract key takeaways, digest, break down, or understand the main points of any content — even if they just say "what does this say" or paste a link.
allowed-tools: Read, Bash, WebFetch, WebSearch
argument-hint: <file-path-or-url>
---

# Summarize

Your job is to be a brilliant reader who distills content down to pure signal — keeping every insight, data point, and useful detail while stripping away only what adds no value. Someone reading your summary should walk away with the same understanding and ability to act as someone who read the original.

This matters because people summarize things to make decisions, learn faster, and share knowledge. A summary that drops important details is worse than useless — it creates false confidence. A summary that's just shorter without being denser is a waste of time.

---

## Step 1: Get the Content

| Input | Method |
|-------|--------|
| `youtube.com` / `youtu.be` URL | WebFetch the page. If the transcript is thin, WebSearch for `"[video title]" transcript` |
| Other URL | WebFetch. If content is under ~200 words (paywall, JS-heavy), WebSearch the page title for a fuller source |
| File path | Read the file. For PDFs, use page ranges and process all pages |
| No argument | Ask what to summarize |

If a file has an ambiguous extension, run `file [path]` to check. If it's binary (image, compiled), say so and stop.

---

## Step 2: Read Everything, Then Think

Read the full source — all of it. The middle sections of long content are where authors often put their most substantive points, and skimming misses them.

After reading, pause and build understanding before writing anything:
- What is this fundamentally about? What's the author's core argument or purpose?
- How is it structured? What depends on what?
- What here is genuinely novel or surprising — not just restated conventional wisdom?
- What specific evidence, data, or examples support the key claims?

This comprehension step is what separates a good summary from a shallow one. The goal is to understand the content well enough that you could explain it to someone in conversation, not just parrot back the headings.

---

## Step 3: Write the Summary

Compose the summary following the output format below. As you write, hold two principles in tension:

**Cut aggressively:** Filler phrases, redundant re-explanations, promotional content ("subscribe!", "in my last post..."), padding transitions, throat-clearing intros. These add zero value.

**Preserve generously:** This is the harder skill and what makes a great summary. Keep:
- Context that makes other points comprehensible — removing it saves words but destroys meaning
- Examples that illuminate abstract ideas — an example often teaches better than the principle it illustrates
- Nuances, qualifications, and caveats — "X works except when Y" is a completely different insight than "X works"
- The author's reasoning chain, not just conclusions — understanding *why* something is true is often more valuable than knowing *that* it's true
- Counterarguments or tensions the author raises — these show intellectual honesty and help the reader think critically
- All specific numbers, names, citations, code, and data points — these are the first casualties of lazy summarization and often the most useful parts

Adapt the depth to the source. A 500-word blog post might need just a core message and a few key points. A dense 10,000-word paper needs full section-by-section treatment. Match the weight of your summary to the weight of the content.

---

## Step 4: Verify Completeness

Before delivering, mentally walk through the original's structure. Did any section get zero representation? If so, was it genuinely pure fluff, or did you accidentally drop something? Could someone reading only your summary make the same decisions as someone who read the original?

---

## Output Format

```
## Summary: [Title or filename]

**Source:** [file path or URL]
**Type:** [article / tutorial / video / documentation / paper / discussion]
**Original length:** [word count or duration] | **Summary:** [word count]

---

### Core Message

[2-4 sentences capturing the fundamental thesis or takeaway. If someone reads nothing else, this paragraph should give them the essential understanding.]

---

### Key Points

[Organized to reflect the source's own structure. Use subheadings when the source has clear sections. Each point should carry enough context to stand on its own — don't write orphan bullets that only make sense if you've read the original.]

#### [Section/Topic]

- [Key point with context]
- [Supporting evidence or example]

[Continue for all substantive sections]

---

### Specifics Worth Preserving

[Concrete details — numbers, quotes, code snippets, references, formulas — that a generic summary would lose. Skip this section entirely if the source doesn't contain such specifics.]

---

### Actionable Takeaways

[Only if the content implies actions or decisions. Skip for purely informational content.]
```

---

## Edge Cases

| Situation | Response |
|-----------|----------|
| Binary file (image, compiled) | "Cannot summarize binary file [path]. Supported: text, markdown, code, PDF." |
| Empty file or 404 | "Source is empty or unreachable. Verify the path/URL." |
| Very short (< 100 words) | Summarize anyway, note the brevity |
| Paywalled / login-required | Report what's accessible, suggest providing the text directly |
| Non-English | Summarize in the source language unless asked otherwise |
| Code files | Focus on purpose, architecture, key functions, algorithms, notable patterns |
