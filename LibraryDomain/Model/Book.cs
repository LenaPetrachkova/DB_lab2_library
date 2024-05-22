using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryDomain.Model;

public partial class Book
{
    public int Id { get; set; }
    [Required(ErrorMessage ="Поле не повинно бути порожнім")]
    [Display(Name ="Назва")]
    public string Title { get; set; } = null!;
    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "Опис")]
    public string Annotation { get; set; } = null!;
    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "Рік випуску")]
    public int YearOfPublish { get; set; }
    [Required(ErrorMessage = "Поле не повинно бути порожнім")]
    [Display(Name = "В наявності (шт.)")]
    public int Amount { get; set; }

    public virtual ICollection<AuthorBook> AuthorBooks { get; set; } = new List<AuthorBook>();

    public virtual ICollection<ReaderCard> ReaderCards { get; set; } = new List<ReaderCard>();

    public virtual ICollection<TopicBook> TopicBooks { get; set; } = new List<TopicBook>();
}
