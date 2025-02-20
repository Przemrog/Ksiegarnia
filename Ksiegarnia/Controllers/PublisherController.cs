using Ksiegarnia.Data;
using Ksiegarnia.Models.ViewModels;
using Ksiegarnia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace Ksiegarnia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PublisherController : Controller
    {
        private readonly KsiegarniaDbContext _context;
        public PublisherController(KsiegarniaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var publishers = from p in _context.Publishers
                             select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                publishers = publishers.Where(s => s.Name.Contains(searchString));
            }

            return View(await publishers.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var author = await _context.Publishers.Include(c => c.Books)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        public IActionResult Create()
        {
            var viewModel = new PublisherViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PublisherViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var publisher = new Models.Publisher
                {
                    Name = viewModel.Name
                };

                _context.Add(publisher);
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

            var publisher = await _context.Publishers.Include(c => c.Books).FirstOrDefaultAsync(c => c.Id == id);
            if (publisher == null)
            {
                return NotFound();
            }

            var viewModel = new PublisherViewModel
            {
                Id = publisher.Id,
                Name = publisher.Name
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PublisherViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var publisher = await _context.Publishers.FindAsync(id);
                if (publisher == null)
                {
                    return NotFound();
                }

                publisher.Name = viewModel.Name;

                try
                {
                    _context.Update(publisher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Publishers.Any(c => c.Id == id))
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
            var publisher = await _context.Publishers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return NotFound();
            }
            return View(publisher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
