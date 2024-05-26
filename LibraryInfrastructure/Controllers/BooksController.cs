using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryDomain.Model;
using LibraryInfrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryInfrastructure.Controllers
{
    public class BooksController : Controller
    {
        private readonly Lab2DbContext _context;

        public BooksController(Lab2DbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(int? id, string title, string annotation, int? yearOfPublish, int? amount, string author, string topic)
        {
            var query = _context.Books.AsQueryable();

            if (id.HasValue)
            {
                query = query.Where(b => b.Id == id.Value);
            }

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(b => b.Title.Contains(title));
            }

            if (!string.IsNullOrEmpty(annotation))
            {
                query = query.Where(b => b.Annotation.Contains(annotation));
            }

            if (yearOfPublish.HasValue)
            {
                query = query.Where(b => b.YearOfPublish == yearOfPublish.Value);
            }

            if (amount.HasValue)
            {
                query = query.Where(b => b.Amount == amount.Value);
            }

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(b => b.AuthorBooks.Any(ba => (ba.Author.FirstName + " " + ba.Author.LastName + " " + ba.Author.FatherName).Contains(author)));
            }

            if (!string.IsNullOrEmpty(topic))
            {
                query = query.Where(b => b.TopicBooks.Any(tb => tb.Topic.Name.Contains(topic)));
            }

            var books = await query.ToListAsync();

            foreach (var book in books)
            {
                _context.Entry(book).Collection(b => b.TopicBooks).Query().Include(tb => tb.Topic).Load();
                _context.Entry(book).Collection(b => b.AuthorBooks).Query().Include(ba => ba.Author).Load();
            }

            return View(books);
        }

        // GET: Books/Queries
        public async Task<IActionResult> Queries()
        {
            var authors = await _context.Authors
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.FirstName} {a.LastName} {a.FatherName}"
                })
                .ToListAsync();

            var topics = await _context.Topics
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToListAsync();

            var readers = await _context.Readers
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.FirstName} {r.LastName}"
                })
                .ToListAsync();

            ViewBag.Authors = authors;
            ViewBag.Topics = topics;
            ViewBag.Readers = readers;

            return View();
        }

        // книги за автором
        public async Task<IActionResult> BooksByAuthor(int authorId)
        {
            var sql = @"
                SELECT b.*
                FROM Book b
                JOIN AuthorBook ab ON b.Id = ab.BookId
                JOIN Author a ON ab.AuthorId = a.Id
                WHERE a.Id = @AuthorId";

            var books = await _context.Books
                                      .FromSqlRaw(sql, new SqlParameter("@AuthorId", authorId))
                                      .ToListAsync();

            foreach (var book in books)
            {
                _context.Entry(book).Collection(b => b.TopicBooks).Query().Include(tb => tb.Topic).Load();
                _context.Entry(book).Collection(b => b.AuthorBooks).Query().Include(ba => ba.Author).Load();
            }

            return View("Index", books);
        }

        // книги, що належать певній темі
        public async Task<IActionResult> BooksByTopic(int topicId)
        {
            var sql = @"
                SELECT b.*
                FROM Book b
                JOIN TopicBook tb ON b.Id = tb.BookId
                JOIN Topic t ON tb.TopicId = t.Id
                WHERE t.Id = @TopicId";

            var books = await _context.Books
                                      .FromSqlRaw(sql, new SqlParameter("@TopicId", topicId))
                                      .ToListAsync();

            foreach (var book in books)
            {
                _context.Entry(book).Collection(b => b.TopicBooks).Query().Include(tb => tb.Topic).Load();
                _context.Entry(book).Collection(b => b.AuthorBooks).Query().Include(ba => ba.Author).Load();
            }

            return View("Index", books);
        }

        // книги взяті читачем
        public async Task<IActionResult> BooksByReader(int readerId)
        {
            var sql = @"
                SELECT b.*
                FROM Book b
                JOIN ReaderCard rc ON b.Id = rc.BookId
                JOIN Reader r ON rc.ReaderId = r.Id
                WHERE r.Id = @ReaderId";

            var books = await _context.Books
                                      .FromSqlRaw(sql, new SqlParameter("@ReaderId", readerId))
                                      .ToListAsync();

            foreach (var book in books)
            {
                _context.Entry(book).Collection(b => b.TopicBooks).Query().Include(tb => tb.Topic).Load();
                _context.Entry(book).Collection(b => b.AuthorBooks).Query().Include(ba => ba.Author).Load();
            }

            return View("Index", books);
        }

        // книги, видані у певному році
        public async Task<IActionResult> BooksByYear(int year)
        {
            var sql = @"
                SELECT b.*
                FROM Book b
                WHERE b.YearOfPublish = @Year";

            var books = await _context.Books
                                      .FromSqlRaw(sql, new SqlParameter("@Year", year))
                                      .ToListAsync();

            foreach (var book in books)
            {
                _context.Entry(book).Collection(b => b.TopicBooks).Query().Include(tb => tb.Topic).Load();
                _context.Entry(book).Collection(b => b.AuthorBooks).Query().Include(ba => ba.Author).Load();
            }

            return View("Index", books);
        }

        // книги, що доступні в певній кількості
        public async Task<IActionResult> BooksByAmount(int amount)
        {
            var sql = @"
                SELECT b.*
                FROM Book b
                WHERE b.Amount = @Amount";

            var books = await _context.Books
                                      .FromSqlRaw(sql, new SqlParameter("@Amount", amount))
                                      .ToListAsync();

            foreach (var book in books)
            {
                _context.Entry(book).Collection(b => b.TopicBooks).Query().Include(tb => tb.Topic).Load();
                _context.Entry(book).Collection(b => b.AuthorBooks).Query().Include(ba => ba.Author).Load();
            }

            return View("Index", books);
        }

        // Знайти читачів, які орендували книги, що і читач N (хоча б одну)
        public async Task<IActionResult> UsersWhoRentedAnyBook(int readerId)
        {
            var sql = @"
        SELECT DISTINCT r.*
        FROM Reader r
        JOIN ReaderCard rc ON r.Id = rc.ReaderId
        WHERE rc.BookId IN (
            SELECT rc2.BookId
            FROM ReaderCard rc2
            WHERE rc2.ReaderId = @readerId
        ) AND r.Id != @readerId";

            var readers = await _context.Readers
                .FromSqlRaw(sql, new SqlParameter("@readerId", readerId))
                .ToListAsync();

            foreach (var reader in readers)
            {
                _context.Entry(reader).Collection(r => r.ReaderCards).Query().Include(rc => rc.Book).Load();
            }

            var selectedReader = await _context.Readers
                .Include(r => r.ReaderCards)
                .ThenInclude(rc => rc.Book)
                .FirstOrDefaultAsync(r => r.Id == readerId);

            ViewBag.SelectedReader = selectedReader;

            return View("Readers", readers);
        }

        // Знайти читачів, які орендували всі книги, що і читач N
        public async Task<IActionResult> UsersWhoRentedAllBooks(int readerId)
        {
            var sql = @"
        SELECT r.*
        FROM Reader r
        WHERE r.Id != @readerId
          AND NOT EXISTS (
              SELECT 1
              FROM ReaderCard rc2
              WHERE rc2.ReaderId = @readerId
                AND rc2.BookId NOT IN (
                    SELECT rc.BookId
                    FROM ReaderCard rc
                    WHERE rc.ReaderId = r.Id
                )
          )";

            var readers = await _context.Readers
                .FromSqlRaw(sql, new SqlParameter("@readerId", readerId))
                .ToListAsync();

            foreach (var reader in readers)
            {
                _context.Entry(reader).Collection(r => r.ReaderCards).Query().Include(rc => rc.Book).Load();
            }

            var selectedReader = await _context.Readers
                .Include(r => r.ReaderCards)
                .ThenInclude(rc => rc.Book)
                .FirstOrDefaultAsync(r => r.Id == readerId);

            ViewBag.SelectedReader = selectedReader;

            return View("Readers", readers);
        }

        // Знайти читачів, які орендували всі ті і тільки ті книги, що і читач N
        public async Task<IActionResult> UsersWhoRentedOnlySameBooks(int readerId)
        {
            var sql = @"
        SELECT r.*
        FROM Reader r
        WHERE r.Id != @readerId
          AND NOT EXISTS (
              SELECT 1
              FROM ReaderCard rc2
              WHERE rc2.ReaderId = @readerId
                AND rc2.BookId NOT IN (
                    SELECT rc.BookId
                    FROM ReaderCard rc
                    WHERE rc.ReaderId = r.Id
                )
          )
          AND NOT EXISTS (
              SELECT 1
              FROM ReaderCard rc
              WHERE rc.ReaderId = r.Id
                AND rc.BookId NOT IN (
                    SELECT rc2.BookId
                    FROM ReaderCard rc2
                    WHERE rc2.ReaderId = @readerId
                )
          )";

            var readers = await _context.Readers
                .FromSqlRaw(sql, new SqlParameter("@readerId", readerId))
                .ToListAsync();

            foreach (var reader in readers)
            {
                _context.Entry(reader).Collection(r => r.ReaderCards).Query().Include(rc => rc.Book).Load();
            }

            var selectedReader = await _context.Readers
                .Include(r => r.ReaderCards)
                .ThenInclude(rc => rc.Book)
                .FirstOrDefaultAsync(r => r.Id == readerId);

            ViewBag.SelectedReader = selectedReader;

            return View("Readers", readers);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                                     .Include(b => b.TopicBooks).ThenInclude(tb => tb.Topic)
                                     .Include(b => b.AuthorBooks).ThenInclude(ba => ba.Author)
                                     .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["Topics"] = new SelectList(_context.Topics, "Id", "Name");
            ViewData["Authors"] = new SelectList(_context.Authors, "Id", "FullName");
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Annotation,YearOfPublish,Amount,Status")] Book book, int[] selectedTopics, int[] selectedAuthors)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();

                if (selectedTopics != null)
                {
                    foreach (var topicId in selectedTopics)
                    {
                        _context.TopicBooks.Add(new TopicBook { BookId = book.Id, TopicId = topicId });
                    }
                }

                if (selectedAuthors != null)
                {
                    foreach (var authorId in selectedAuthors)
                    {
                        _context.AuthorBooks.Add(new AuthorBook { BookId = book.Id, AuthorId = authorId });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Topics"] = new SelectList(_context.Topics, "Id", "Name");
            ViewData["Authors"] = new SelectList(_context.Authors, "Id", "FullName");
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                                     .Include(b => b.TopicBooks).ThenInclude(tb => tb.Topic)
                                     .Include(b => b.AuthorBooks).ThenInclude(ba => ba.Author)
                                     .FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
            ViewBag.Authors = new SelectList(_context.Authors, "Id", "Name");
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Annotation,YearOfPublish,Amount,Status")] Book book, int[] selectedTopics, int[] selectedAuthors)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();

                    var existingTopicBooks = _context.TopicBooks.Where(tb => tb.BookId == book.Id);
                    _context.TopicBooks.RemoveRange(existingTopicBooks);
                    foreach (var topicId in selectedTopics)
                    {
                        _context.TopicBooks.Add(new TopicBook { BookId = book.Id, TopicId = topicId });
                    }
                    var existingBookAuthors = _context.AuthorBooks.Where(ba => ba.BookId == book.Id);
                    _context.AuthorBooks.RemoveRange(existingBookAuthors);
                    foreach (var authorId in selectedAuthors)
                    {
                        _context.AuthorBooks.Add(new AuthorBook { BookId = book.Id, AuthorId = authorId });
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Topics = new SelectList(_context.Topics, "Id", "Name");
            ViewBag.Authors = new SelectList(_context.Authors, "Id", "Name");
            return View(book);
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                                     .Include(b => b.TopicBooks).ThenInclude(tb => tb.Topic)
                                     .Include(b => b.AuthorBooks).ThenInclude(ba => ba.Author)
                                     .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books
                                     .Include(b => b.TopicBooks)
                                     .Include(b => b.AuthorBooks)
                                     .Include(b => b.ReaderCards)
                                     .FirstOrDefaultAsync(m => m.Id == id);

            if (book != null)
            {
                _context.ReaderCards.RemoveRange(book.ReaderCards);

                _context.TopicBooks.RemoveRange(book.TopicBooks);
                _context.AuthorBooks.RemoveRange(book.AuthorBooks);

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Books/DeleteOneInstance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOneInstance(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            if (book.Amount > 0)
            {
                book.Amount -= 1;
                _context.Update(book);
                await _context.SaveChangesAsync();

                if (book.Amount == 0)
                {
                    TempData["DeleteOneError"] = "Ви не можете видалити ще один екземпляр, тому що їх більше немає.";
                }
            }
            else
            {
                TempData["DeleteOneError"] = "Ви не можете видалити ще один екземпляр, тому що їх більше немає.";
            }

            return RedirectToAction(nameof(Delete), new { id = book.Id });
        }
    }
}
