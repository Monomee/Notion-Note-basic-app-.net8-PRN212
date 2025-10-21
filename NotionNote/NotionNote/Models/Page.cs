using System;
using System.Collections.Generic;

namespace NotionNote.Models;

public partial class Page
{
    public int PageId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsPinned { get; set; }

    public int WorkspaceId { get; set; }

    public virtual Workspace Workspace { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
