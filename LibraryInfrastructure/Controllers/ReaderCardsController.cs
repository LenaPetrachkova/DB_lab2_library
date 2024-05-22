using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryDomain.Model;
using LibraryInfrastructure;

namespace LibraryInfrastructure.Controllers
{
    public class ReaderCardsController : Controller
    {
        private readonly Lab2DbContext _context;

        public ReaderCardsController(Lab2DbContext context)
        {
            _context = context;
        }

        // GET: ReaderCards
        public async Task<IActionResult> Index()
        {
            var lab2DbContext = _context.ReaderCards.Include(r => r.Book).Include(r => r.Librarian).Include(r => r.Reader);
            return View(await lab2DbContext.ToListAsync());
        }

        // GET: ReaderCards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readerCard = await _context.ReaderCards
                .Include(r => r.Book)
                .Include(r => r.Librarian)
                .Include(r => r.Reader)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (readerCard == null)
            {
                return NotFound();
            }

            return View(readerCard);
        }

        // GET: ReaderCards/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.Amount > 0), "Id", "Title");

            var librarians = _context.Librarians
                                     .Select(l => new
                                     {
                                         l.Id,
                                         FullName = l.LastName + " " + l.FirstName
                                     })
                                     .ToList();
            ViewData["LibrarianId"] = new SelectList(librarians, "Id", "FullName");

            var readers = _context.Readers
                                  .Select(r => new
                                  {
                                      r.Id,
                                      FullName = r.LastName + " " + r.FirstName
                                  })
                                  .ToList();
            ViewData["ReaderId"] = new SelectList(readers, "Id", "FullName");

            return View();
        }

        // POST: ReaderCards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RentDate,ReturnDate,ReaderId,BookId,LibrarianId")] ReaderCard readerCard)
        {
            ModelState.Remove("Book");
            ModelState.Remove("Librarian");
            ModelState.Remove("Reader");
            if (ModelState.IsValid)
            {
                _context.Add(readerCard);
                await _context.SaveChangesAsync();

                var book = await _context.Books.FindAsync(readerCard.BookId);
                if (book != null && book.Amount > 0)
                {
                    book.Amount -= 1;
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.Amount > 0), "Id", "Title", readerCard.BookId);

            var librarians = _context.Librarians
                                     .Select(l => new
                                     {
                                         l.Id,
                                         FullName = l.LastName + " " + l.FirstName
                                     })
                                     .ToList();
            ViewData["LibrarianId"] = new SelectList(librarians, "Id", "FullName", readerCard.LibrarianId);

            var readers = _context.Readers
                                  .Select(r => new
                                  {
                                      r.Id,
                                      FullName = r.LastName + " " + r.FirstName
                                  })
                                  .ToList();
            ViewData["ReaderId"] = new SelectList(readers, "Id", "FullName", readerCard.ReaderId);

            return View(readerCard);
        }

        // GET: ReaderCards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readerCard = await _context.ReaderCards.FindAsync(id);
            if (readerCard == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", readerCard.BookId);
            ViewData["LibrarianId"] = new SelectList(_context.Librarians, "Id", "FirstName", readerCard.LibrarianId);
            ViewData["ReaderId"] = new SelectList(_context.Readers, "Id", "FullName", readerCard.ReaderId);
            return View(readerCard);
        }

        // POST: ReaderCards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RentDate,ReturnDate,ReaderId,BookId,LibrarianId")] ReaderCard readerCard)
        {
            if (id != readerCard.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(readerCard);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReaderCardExists(readerCard.Id))
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
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", readerCard.BookId);
            ViewData["LibrarianId"] = new SelectList(_context.Librarians, "Id", "FirstName", readerCard.LibrarianId);
            ViewData["ReaderId"] = new SelectList(_context.Readers, "Id", "FullName", readerCard.ReaderId);
            return View(readerCard);
        }

        // GET: ReaderCards/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readerCard = await _context.ReaderCards
                .Include(r => r.Book)
                .Include(r => r.Librarian)
                .Include(r => r.Reader)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (readerCard == null)
            {
                return NotFound();
            }

            return View(readerCard);
        }

        // POST: ReaderCards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var readerCard = await _context.ReaderCards.FindAsync(id);
            if (readerCard != null)
            {
                var book = await _context.Books.FindAsync(readerCard.BookId);
                if (book != null)
                {
                    book.Amount += 1;
                    _context.Update(book);
                }

                _context.ReaderCards.Remove(readerCard);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ReaderCardExists(int id)
        {
            return _context.ReaderCards.Any(e => e.Id == id);
        }
    }
}
