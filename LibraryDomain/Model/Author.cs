using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryDomain.Model;

public partial class Author
{
    public int Id { get; set; }
    [Display(Name = "Ім'я")]
    public string FirstName { get; set; } = null!;
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = null!;
    [Display(Name = "По-батькові")]
    public string? FatherName { get; set; }
    public string FullName => $"{LastName} {FirstName} {FatherName}";

    public virtual ICollection<AuthorBook> AuthorBooks { get; set; } = new List<AuthorBook>();
}
