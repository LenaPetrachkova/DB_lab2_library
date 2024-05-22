using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryDomain.Model;

public partial class Librarian
{
    public int Id { get; set; }
    [Display(Name = "Ім'я")]
    public string FirstName { get; set; } = null!;
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = null!;
    [Display(Name = "Номер телефону")]
    public string PhoneNumber { get; set; } = null!;

    public virtual ICollection<ReaderCard> ReaderCards { get; set; } = new List<ReaderCard>();
}
