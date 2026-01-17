# PDF Export Feature Documentation

## 📄 Overview

Added comprehensive PDF export functionality to the Journal Management System, allowing users to export their journal entries as professionally formatted PDF documents with customizable date range filtering.

## ✨ Features

### 1. **Export by Date Range**

- Select custom start and end dates
- Export all entries if no dates are selected
- Real-time count of entries to be exported
- Smart date filtering

### 2. **Professional PDF Formatting**

- **Header Section**: Journal title with export date
- **Export Summary**: Date range and total entry count
- **Entry Details**:
  - Entry title (if provided)
  - Full date (e.g., "January 17, 2026 (Friday)")
  - Primary mood with emoji
  - Secondary moods (if any)
  - Tags with visual badges
  - Full entry content (markdown converted to plain text)
  - Created/Updated timestamps

### 3. **User Experience**

- Toggle export panel with single click
- Visual feedback during export
- Automatic PDF file opening after generation
- Clear success/error messages
- Entry count preview before export

## 📦 Implementation Details

### New Files Created:

1. **Services/PdfExportService.cs**
   - `IPdfExportService` interface
   - `PdfExportService` implementation
   - PDF document composition using QuestPDF
   - Markdown to plain text conversion
   - Mood emoji mapping

### Files Modified:

1. **JournalManagementSystem.csproj**
   - Added QuestPDF package (v2024.12.3)

2. **MauiProgram.cs**
   - Registered `IPdfExportService` in DI container

3. **Components/Pages/Journal.razor**
   - Added export panel UI
   - Date range selection inputs
   - Export button with loading state
   - Export methods implementation

4. **Components/Pages/Journal.razor.css**
   - Export panel styling
   - Button styles for export actions
   - Responsive design for mobile devices

## 🎨 PDF Layout

```
┌────────────────────────────────────┐
│  📔 My Journal                     │
│  Exported on: January 17, 2026    │
├────────────────────────────────────┤
│  Export Summary                    │
│  Date Range: Jan 1 - Jan 17, 2026 │
│  Total Entries: 15                 │
├────────────────────────────────────┤
│  ┌──────────────────────────────┐ │
│  │ Entry Title                  │ │
│  │ January 15, 2026 (Wednesday) │ │
│  │ 😄 Happy  😌 Relaxed        │ │
│  │ Tags: [work] [productivity]  │ │
│  │                              │ │
│  │ Entry content goes here...   │ │
│  │                              │ │
│  │ Created: Jan 15, 2026 09:30  │ │
│  └──────────────────────────────┘ │
│                                    │
│  [Additional entries...]           │
├────────────────────────────────────┤
│           Page 1 / 3               │
└────────────────────────────────────┘
```

## 🔧 Technical Stack

- **QuestPDF**: Modern PDF generation library
- **Markdig**: Markdown to HTML conversion
- **.NET MAUI**: Cross-platform file handling
- **Entity Framework**: Data retrieval

## 📱 Usage

1. **Access Export**: Click "📄 Export to PDF" button in Journal page
2. **Select Date Range** (Optional):
   - Choose start date
   - Choose end date
   - Leave empty for all entries
3. **Preview Count**: See how many entries will be exported
4. **Export**: Click "📥 Export to PDF" button
5. **View PDF**: PDF opens automatically or find it in app data directory

## 📂 File Location

PDFs are saved to:

```
{AppDataDirectory}/Journal_Export_{yyyyMMdd_HHmmss}.pdf
```

Example:

```
C:\Users\YourName\AppData\Local\Packages\...\LocalState\Journal_Export_20260117_143052.pdf
```

## 🎯 Benefits

- **Backup**: Create permanent copies of journal entries
- **Sharing**: Share entries with therapists or loved ones
- **Archiving**: Organize entries by time periods
- **Offline Access**: Read entries without the app
- **Professional Format**: Clean, readable PDF layout
- **Privacy**: Keep exports secure on your device

## 🔐 Security Note

Exported PDFs are not encrypted. For sensitive content:

- Store PDFs in secure locations
- Use encrypted drives/folders
- Delete exports after use if needed
- Consider password-protecting PDFs externally

## 🚀 Future Enhancements (Potential)

- Password protection for PDF exports
- Custom PDF themes/templates
- Filter by mood, tags, or keywords
- Email PDF directly from app
- Cloud backup integration
- Batch export multiple date ranges
- Include images/attachments in PDF
