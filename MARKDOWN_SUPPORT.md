# Markdown Support for Journal Management System

## Overview

Your Journal Management System now supports **Rich Text/Markdown formatting** for journal entries. This allows you to create beautifully formatted entries with support for:

- ✅ **Bold** and _Italic_ text
- ✅ **Headings** (H1, H2, H3)
- ✅ **Lists** (Ordered and Unordered)
- ✅ **Code blocks** and inline code
- ✅ **Blockquotes**
- ✅ **Links** and images
- ✅ **Tables**
- ✅ **Strikethrough** text
- ✅ **Live preview** while editing

## What Changed

### New Components Added

1. **MarkdownEditor.razor** - Advanced editor with toolbar and live preview
   - Formatting toolbar with quick buttons
   - Split-screen editor and preview
   - Comprehensive markdown help guide
   - Location: `Components/MarkdownEditor.razor`

2. **MarkdownDisplay.razor** - Read-only markdown renderer
   - Beautiful rendering of markdown content
   - Syntax-highlighted code blocks
   - Responsive layout
   - Location: `Components/MarkdownDisplay.razor`

3. **MarkdownService.cs** - Backend markdown processing
   - Converts markdown to HTML
   - Text preview generation
   - Safe HTML rendering
   - Location: `Services/MarkdownService.cs`

### Modified Components

- **Journal.razor** - Updated to use new markdown editor and display components
- **MauiProgram.cs** - Registered MarkdownService dependency injection
- **JournalManagementSystem.csproj** - Added Markdig NuGet package

## How to Use

### Writing Markdown in Journal Entries

When creating or editing a journal entry, you'll see:

1. **Formatting Toolbar** - Quick access buttons for common formatting
2. **Editor Area** - Write your content using markdown syntax
3. **Live Preview** - Toggle preview button (👁️) to see formatted output
4. **Help Guide** - Expandable guide with syntax examples

### Markdown Syntax Guide

#### Text Formatting

```markdown
**bold text** → Bold
_italic text_ → Italic
~~strikethrough~~ → Strikethrough
`inline code` → Inline code
```

#### Headings

```markdown
# Heading 1

## Heading 2

### Heading 3

#### Heading 4

##### Heading 5

###### Heading 6
```

#### Lists

**Unordered List:**

```markdown
- Item 1
- Item 2
  - Nested item 2.1
  - Nested item 2.2
- Item 3
```

**Ordered List:**

```markdown
1. First item
2. Second item
3. Third item
```

#### Code Blocks

**Inline Code:**

```markdown
Use `variable` in your code
```

**Code Block:**

````markdown
```
function hello() {
    console.log("Hello World");
}
```
````

**Code Block with Language Highlighting:**

````markdown
```csharp
public class JournalEntry {
    public string Content { get; set; }
}
```
````

#### Links and Images

```markdown
[Link text](https://example.com)
[Link with title](https://example.com "Link Title")
![Alt text](image-url.jpg)
```

#### Blockquotes

```markdown
> This is a blockquote
> It can span multiple lines
>
> And have multiple paragraphs
```

#### Tables

```markdown
| Column 1 | Column 2 | Column 3 |
| -------- | -------- | -------- |
| Data 1   | Data 2   | Data 3   |
| Data 4   | Data 5   | Data 6   |
```

#### Horizontal Rule

```markdown
---

or

---

or

---
```

## Toolbar Features

### Formatting Buttons

| Button     | Function       | Shortcut      |
| ---------- | -------------- | ------------- |
| **B**      | Bold text      | `**text**`    |
| _I_        | Italic text    | `*text*`      |
| S          | Strikethrough  | `~~text~~`    |
| H1         | Heading 1      | `# `          |
| H2         | Heading 2      | `## `         |
| H3         | Heading 3      | `### `        |
| ● List     | Bullet list    | `- `          |
| 1. List    | Numbered list  | `1. `         |
| </>        | Inline code    | `` `code` ``  |
| [Link]     | Insert link    | `[text](url)` |
| Quote      | Blockquote     | `> `          |
| Code Block | Code block     | ` ``` `       |
| 👁️ Preview | Toggle preview | Split view    |

## Advanced Features

### Live Preview

The editor includes a live preview panel that updates as you type. You can toggle it using the "👁️ Preview" button to see how your markdown will be rendered.

### Markdown Help Guide

Click the "📖 Markdown Formatting Guide" dropdown to see:

- Common markdown syntax
- Examples for each format
- Quick reference table

### Smart Formatting

The editor intelligently handles:

- Nested lists
- Code block syntax highlighting
- Table rendering
- Blockquote styling
- Link management

## Technical Details

### Dependencies

- **Markdig** (v0.37.0) - Advanced markdown parser and renderer
  - Supports GitHub Flavored Markdown (GFM)
  - Extended syntax support
  - Safe HTML rendering

### Architecture

```
MarkdownService (IMarkdownService)
    ├── ConvertToHtml(markdown) → HTML
    └── GetPreview(markdown) → Plain text

MarkdownEditor.razor
    ├── Toolbar (Formatting buttons)
    ├── Editor (Textarea with syntax highlighting)
    └── Preview (Live markdown rendering)

MarkdownDisplay.razor
    └── Content renderer (Read-only view)
```

### Database

The markdown content is stored as plain text in the existing `Content` field of the `JournalEntry` table. No database migration is required.

## Styling

All markdown components include professional CSS styling with:

- **Dark mode support** - Components adapt to system theme
- **Responsive design** - Works on desktop, tablet, and mobile
- **Accessible colors** - WCAG AA compliant
- **Smooth animations** - Subtle transitions and hover effects
- **Code highlighting** - Syntax-highlighted code blocks

### Color Scheme

- Primary color: `#512BD4` (Purple - matches app theme)
- Text color: `#333` (Dark gray)
- Background: `#f9f9f9` (Light gray)
- Accent: `#d63384` (Pink for inline code)

## Examples

### Example 1: Simple Journal Entry

````markdown
# My Day Today

## Morning

Had a great **coffee** at the local café. The weather was _amazing_!

## Afternoon

- Completed project documentation
- Reviewed pull requests
- Had a meeting with the team

## Evening

```csharp
// Worked on some code
var entry = new JournalEntry();
entry.Content = "Amazing day!";
```
````

````

### Example 2: Reflective Entry

```markdown
# Reflection on Recent Events

> "The only way to do great work is to love what you do." - Steve Jobs

## Key Learnings

1. **Persistence pays off** - Continued effort leads to better results
2. **Collaboration is key** - Working with others improves outcomes
3. **Documentation matters** - Clear communication prevents confusion

### Mood Indicators
- 😊 Happy
- 💪 Confident
- 🤔 Thoughtful
````

## Performance

- **Editor**: Lightweight and responsive, handles large entries smoothly
- **Rendering**: Markdown conversion is cached for display
- **Preview**: Updates efficiently with debouncing
- **Storage**: Plain text markdown is very storage-efficient

## Troubleshooting

### Preview Not Updating

- Ensure JavaScript is enabled in your browser
- Try refreshing the page
- Check browser console for any errors

### Formatting Not Appearing

- Verify markdown syntax is correct
- Check that special characters are properly escaped
- Use the toolbar buttons for complex formatting

### Code Block Issues

- Use triple backticks for code blocks: ` ``` `
- Specify language after backticks for syntax highlighting
- Ensure backticks aren't nested

## Future Enhancements

Potential improvements for future versions:

- [ ] Custom themes for markdown display
- [ ] Markdown templates/snippets
- [ ] Export to PDF with formatting
- [ ] Rich media embedding (videos, embeds)
- [ ] Collaborative editing features
- [ ] Version history with markdown diff view

## Support

For issues or questions about markdown functionality:

1. Check the markdown syntax against the help guide
2. Verify your browser is up-to-date
3. Clear browser cache and cookies
4. Consult the markdown examples in this document

## License

This markdown support uses the **Markdig** library, which is licensed under the BSD 2-Clause License.

---

**Happy journaling with Markdown!** 📝✨
