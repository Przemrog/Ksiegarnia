using Ksiegarnia.Data;
using Ksiegarnia.Models;
using Ksiegarnia.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Ksiegarnia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly KsiegarniaDbContext _context;

        public BooksController(KsiegarniaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var books = from b in _context.Books.Include(b => b.Category).Include(b => b.Author).Include(b => b.Publisher)
                        select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString));
            }

            return View(await books.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.Include(b => b.Category).Include(b => b.Author).Include(b => b.Publisher).Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        public IActionResult Create()
        {
            var viewModel = new BookViewModel
            {
                Categories = _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToList(),
                Authors = _context.Authors
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    })
                    .ToList(),
                Publishers = _context.Publishers
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToList(),
                Tags = _context.Tags
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    })
                    .ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel viewModel)
        {
            ModelState.Remove(nameof(viewModel.Categories));
            ModelState.Remove(nameof(viewModel.Authors));
            ModelState.Remove(nameof(viewModel.Publishers));
            ModelState.Remove(nameof(viewModel.Tags));
            if (ModelState.IsValid)
            {
                var book = new Book
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    AuthorId = viewModel.AuthorId,
                    PublisherId = viewModel.PublisherId,
                    Price = viewModel.Price,
                    CategoryId = viewModel.CategoryId,
                    ImageUrl = viewModel.ImageUrl,
                    BookTags = viewModel.SelectedTagIds.Select(tagId => new BookTag { TagId = tagId }).ToList()
                };

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.Include(b => b.BookTags).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                AuthorId = book.AuthorId,
                Authors = _context.Authors
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    })
                    .ToList(),
                PublisherId = book.PublisherId,
                Publishers = _context.Publishers
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToList(),
                Price = book.Price,
                CategoryId = book.CategoryId,
                Categories = _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToList(),
                ImageUrl = book.ImageUrl,
                SelectedTagIds = book.BookTags.Select(bt => bt.TagId).ToList(),
                Tags = _context.Tags
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(viewModel.Authors));
            ModelState.Remove(nameof(viewModel.Categories));
            ModelState.Remove(nameof(viewModel.Publishers));
            ModelState.Remove(nameof(viewModel.Tags));

            if (ModelState.IsValid)
            {
                var book = await _context.Books.Include(b => b.BookTags).FirstOrDefaultAsync(b => b.Id == id);
                if (book == null)
                {
                    return NotFound();
                }

                book.Title = viewModel.Title;
                book.Description = viewModel.Description;
                book.AuthorId = viewModel.AuthorId;
                book.PublisherId = viewModel.PublisherId;
                book.Price = viewModel.Price;
                book.CategoryId = viewModel.CategoryId;
                book.ImageUrl = viewModel.ImageUrl;

                book.BookTags.Clear();
                book.BookTags = viewModel.SelectedTagIds.Select(tagId => new BookTag { BookId = book.Id, TagId = tagId }).ToList();

                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Books.Any(b => b.Id == id))
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

            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }


    }
}


