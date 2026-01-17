# Markdown Quick Reference Card

## Text Formatting

| Result              | Markdown     | Alternative  |
| ------------------- | ------------ | ------------ |
| **Bold**            | `**text**`   | `__text__`   |
| _Italic_            | `*text*`     | `_text_`     |
| **_Bold & Italic_** | `***text***` | `___text___` |
| ~~Strikethrough~~   | `~~text~~`   | N/A          |
| `Code`              | `` `code` `` | N/A          |

## Headings

```markdown
# H1 Heading

## H2 Heading

### H3 Heading

#### H4 Heading

##### H5 Heading

###### H6 Heading
```

## Lists

### Unordered Lists

```markdown
- Item 1
- Item 2
  - Sub-item 2.1
  - Sub-item 2.2
- Item 3
```

### Ordered Lists

```markdown
1. First
2. Second
3. Third
   1. Nested 3.1
   2. Nested 3.2
```

## Links & Images

```markdown
[Link text](https://example.com)
[Link with title](https://example.com "Title")
![Alt text](image.jpg)
![Alt text](image.jpg "Image Title")
```

## Code

### Inline Code

```markdown
Use `variable` in your text
```

### Code Block

````markdown
```
code here
```
````

### Code Block with Language

````markdown
```csharp
public class Example {
    public string Property { get; set; }
}
```
````

Languages: `csharp`, `javascript`, `python`, `html`, `css`, `json`, `sql`, `xml`, etc.

## Blockquotes

```markdown
> Single line quote

> Multi-line quote
> continued on next line

> Nested quotes
>
> > Inner quote
> > More inner
```

## Horizontal Rules

```markdown
---

or

---

or

---
```

## Tables

```markdown
| Header 1 | Header 2 | Header 3 |
| -------- | -------- | -------- |
| Cell 1   | Cell 2   | Cell 3   |
| Cell 4   | Cell 5   | Cell 6   |

| Left | Center | Right |
| :--- | :----: | ----: |
| L1   |   C1   |    R1 |
| L2   |   C2   |    R2 |
```

## Lists with Mixed Content

```markdown
1. Item with text
   - Sub-bullet point
   - Another sub-point

   And a paragraph within the item

2. Second item
```

with code block

```

```

## Escaping Special Characters

```markdown
\*not italic\*
\[not a link\]
\# not a heading
```

## Line Breaks

```markdown
Soft break (two spaces at end):
Line 1  
Line 2

Hard break (empty line):
Paragraph 1

Paragraph 2
```

## Definition Lists

```markdown
Apple
: A fruit

Orange
: Another fruit
```

## Task Lists

```markdown
- [x] Completed task
- [ ] Incomplete task
- [x] Another done task
```

## Subscript & Superscript

```markdown
H~2~O (subscript)
x^2^ (superscript)
```

## Abbreviations

```markdown
The HTML specification is maintained by the W3C.

_[HTML]: Hyper Text Markup Language
_[W3C]: World Wide Web Consortium
```

## Footnotes

```markdown
Here is a footnote reference[^1].

[^1]: This is the footnote content.
```

## Common Use Cases

### Meeting Notes

```markdown
# Meeting: Q1 Planning

**Date:** January 17, 2026
**Attendees:** John, Sarah, Mike

## Agenda

1. Review Q4 results
2. Plan Q1 initiatives
3. Discuss budget

## Discussion

- Performance was strong ✓
- Team morale is high ✓

## Action Items

- [ ] Create detailed timeline (Sarah)
- [ ] Get budget approval (Mike)
- [ ] Schedule follow-up (John)

**Next Meeting:** January 31, 2026
```

### Project Notes

```markdown
# Project: Website Redesign

## Overview

Redesign the company website for better UX and performance.

## Technical Stack

- Frontend: React
- Backend: Node.js
- Database: PostgreSQL

## Progress

- [x] Wireframes complete
- [x] Design mockups
- [ ] Development
- [ ] Testing
- [ ] Deployment

## Challenges

1. **Challenge 1:** Browser compatibility issues
   > Resolved using polyfills and vendor prefixes
2. **Challenge 2:** Performance concerns
   > Implementing lazy loading and code splitting
```

### Daily Log

````markdown
## Day Log - January 17, 2026

### Morning ☀️

- Team standup at 9:00 AM
- Reviewed PR comments
- Started code refactoring

### Afternoon 🌤️

```code
// Key improvement
const optimizedFunction = () => {
  // More efficient implementation
};
```
````

### Evening 🌙

- Completed documentation
- Code review for 2 PRs

### Stats

| Metric          | Value |
| --------------- | ----- |
| Tasks Completed | 5     |
| Bugs Fixed      | 2     |
| PRs Reviewed    | 3     |

````

## Toolbar Button Reference

| Button | Effect | Shortcut |
|--------|--------|----------|
| **B** | **Bold** | `**text**` |
| *I* | *Italic* | `*text*` |
| S | ~~Strike~~ | `~~text~~` |
| H1 | # Heading | `# ` |
| H2 | ## Heading | `## ` |
| H3 | ### Heading | `### ` |
| ● | • Bullet | `- ` |
| 1. | 1. Number | `1. ` |
| </> | `Code` | `` `code` `` |
| [Link] | [Link](url) | `[text](url)` |
| Quote | > Quote | `> ` |
| Block | ``` Code ``` | ` ``` ` |
| 👁️ | Toggle Preview | Split View |

## Tips & Tricks

1. **Use the preview** - Toggle 👁️ to see formatted output
2. **Click toolbar buttons** - Faster than typing syntax
3. **Check the help** - Click 📖 for detailed reference
4. **Indent code** - Use 4 spaces for code blocks
5. **Nest elements** - Add spaces for nested lists/items
6. **Test formatting** - Use preview before saving
7. **Keep it simple** - Not all markdown features needed
8. **Use consistency** - Stick to similar formatting style

## Common Mistakes

❌ **Wrong:** `**bold *italic***` (mixed delimiters)
✅ **Right:** `***bold and italic***`

❌ **Wrong:** `# Heading#` (hash at end)
✅ **Right:** `# Heading`

❌ **Wrong:** `[text] (url)` (space between)
✅ **Right:** `[text](url)`

❌ **Wrong:** ` `` `code` `` ` (odd number of backticks)
✅ **Right:** `` `code` ``

❌ **Wrong:** `1) First` `2) Second` (parentheses)
✅ **Right:** `1. First` `2. Second`

---

**Pro Tip:** Save this reference card for quick access while writing! 📌
````
