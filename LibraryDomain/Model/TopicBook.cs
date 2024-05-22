using System;
using System.Collections.Generic;

namespace LibraryDomain.Model;

public partial class TopicBook
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int TopicId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Topic Topic { get; set; } = null!;
}
