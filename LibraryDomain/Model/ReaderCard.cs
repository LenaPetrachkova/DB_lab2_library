using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryDomain.Model;

public partial class ReaderCard
{
    public int Id { get; set; }
    [Display(Name = "Дата оренди")]
    public DateOnly RentDate { get; set; }
    [Display(Name = "Дата повернення")]
    public DateOnly ReturnDate { get; set; }

    public int ReaderId { get; set; }

    public int BookId { get; set; }

    public int LibrarianId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Librarian Librarian { get; set; } = null!;

    public virtual Reader Reader { get; set; } = null!;
}
