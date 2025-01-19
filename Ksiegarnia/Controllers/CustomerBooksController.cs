using Ksiegarnia.Data;
using Ksiegarnia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Ksiegarnia.Controllers
{
    public class CustomerBooksController : Controller
    {
        private readonly KsiegarniaDbContext _context;

        public CustomerBooksController(KsiegarniaDbContext context)
        {
            _context = context;
        }

        // GET: CustomerBooks
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;

            var books = _context.Books.Include(b => b.Category).Include(b => b.Author).Include(b => b.Publisher).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value);
            }

            return View(await books.ToListAsync());
        }


        // GET: CustomerBooks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.Include(b => b.Category).Include(b => b.Author).Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
    }
}
