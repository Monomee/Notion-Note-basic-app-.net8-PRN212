using System;
using System.Collections.Generic;

namespace NotionNote.Models;

public partial class Tag
{
    public int TagId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Page> Pages { get; set; } = new List<Page>();
}
