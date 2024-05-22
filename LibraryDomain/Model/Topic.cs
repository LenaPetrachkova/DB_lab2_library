using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryDomain.Model;

public partial class Topic
{
    public int Id { get; set; }
    [Display(Name = "Назва")]
    public string Name { get; set; } = null!;

    public virtual ICollection<TopicBook> TopicBooks { get; set; } = new List<TopicBook>();
}
