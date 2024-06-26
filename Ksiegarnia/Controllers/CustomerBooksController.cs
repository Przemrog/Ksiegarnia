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
        public async Task<IActionResult> Index()
        {
            return View(await _context.Books.ToListAsync());
        }

        // GET: CustomerBooks/Details/5
        public async Task<IActionResult> Details(int? id)
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
    }
}
