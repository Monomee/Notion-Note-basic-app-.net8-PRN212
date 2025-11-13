using System;
using System.Collections.Generic;

namespace NotionNote.Models;

public partial class Workspace
{
    public int WorkspaceId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int UserId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Page> Pages { get; set; } = new List<Page>();

    public virtual User User { get; set; } = null!;
}
