using Ksiegarnia.Data;
using Ksiegarnia.Models.ViewModels;
using Ksiegarnia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ksiegarnia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuthorController : Controller
    {
        private readonly KsiegarniaDbContext _context;
        public AuthorController(KsiegarniaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var authors = from a in _context.Authors
                          select a;

            if (!String.IsNullOrEmpty(searchString))
            {
                authors = authors.Where(s => s.Name.Contains(searchString));
            }

            return View(await authors.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var author = await _context.Authors.Include(c => c.Books)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        public IActionResult Create()
        {
            var viewModel = new AuthorViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var author = new Author
                {
                    Name = viewModel.Name
                };

                _context.Add(author);
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

            var author = await _context.Authors.Include(c => c.Books).FirstOrDefaultAsync(c => c.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            var viewModel = new AuthorViewModel
            {
                Id = author.Id,
                Name = author.Name
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AuthorViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var author = await _context.Authors.FindAsync(id);
                if (author == null)
                {
                    return NotFound();
                }

                author.Name = viewModel.Name;

                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Authors.Any(c => c.Id == id))
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
            var author = await _context.Authors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
