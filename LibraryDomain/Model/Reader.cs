using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryDomain.Model;

public partial class Reader
{
    public int Id { get; set; }
    [Display(Name = "Ім'я")]
    public string FirstName { get; set; } = null!;
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = null!;
    [Display(Name = "Факультет")]
    public string Faculty { get; set; } = null!;
    [Display(Name = "Кафедра")]
    public string? Department { get; set; }
    [Display(Name = "Номер телефону")]
    public string PhoneNumber { get; set; } = null!;

    public virtual ICollection<ReaderCard> ReaderCards { get; set; } = new List<ReaderCard>();
}
