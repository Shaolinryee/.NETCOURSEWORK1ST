namespace JournalManagementSystem.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // mark whether this tag is one of the pre-built tags
    public bool IsPrebuilt { get; set; } = false;

    public ICollection<JournalEntryTag> JournalEntryTags { get; set; } = new List<JournalEntryTag>();
}
