# Repository Guidelines

## Naming Standards

### Default Convention: `lowercase-hyphenated`

All directories and files should use lowercase letters with hyphens as separators:
- Directories: `my-directory-name/`
- Files: `my-file-name.md`
- Numbered files: `01-file-name.md` (hyphen separator, not underscore)

### Language-Specific Exceptions

Follow established best practices for each language. All existing files in this repo already follow these conventions.

| Language | Convention | Applies To | Example |
|----------|------------|------------|---------|
| C# | PascalCase | Files, directories, projects | `EnhancedLRUCache/`, `CacheStorage.cs` |
| C++ | PascalCase | Files (matches class names) | `BubbleSorter.cpp`, `MergeSorter.cpp` |
| Java | PascalCase | Class files | `MyClass.java` |
| Python | snake_case | Module files | `my_module.py` |
| Go | lowercase | Package directories | `mypackage/` |
| JavaScript/TypeScript | kebab-case or camelCase | Files | `my-component.tsx` or `myComponent.ts` |

**For new file types:** Look up the language's established conventions before adding files.

### Special Cases

- **Acronyms:** Lowercase in directory/file names: `ai/`, `oop/`, `ml/`, `api-docs.md`
- **README files:** Keep uppercase `README.md` (universal convention)
- **Configuration files:** Follow tool conventions (`.gitignore`, `.editorconfig`, `package.json`)
- **License files:** Keep uppercase `LICENSE` or `LICENSE.md`

### What to Avoid

- Spaces in names (use hyphens instead)
- Special characters: `&`, `;`, `'`, etc.
- Mixed conventions in the same directory level
- Underscores (except for Python files or language-specific requirements)

---

## Section READMEs

A section README only exists when it adds information the root README doesn't already provide — naming conventions, file format explanations, organizational patterns unique to that section. Most sections don't need one.

**Never repeat what the parent README or folder name already tells the reader.**

---

## Link Collections

Curated link dumps use `reading-list.md` as the filename. Not `random.md`, not `links.md` -- one convention across the repo.

---

## Documentation Tone

- Reader's time is sacred -- every sentence transfers knowledge
- Active voice, concrete over abstract
- No filler, no fluff, no restating the obvious
- Tables for structured references, not paragraphs
