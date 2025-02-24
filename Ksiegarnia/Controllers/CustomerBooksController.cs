using Ksiegarnia.Data;
using Ksiegarnia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? tagId)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;

            var tags = await _context.Tags.ToListAsync();
            ViewData["Tags"] = tags;

            var books = _context.Books.Include(b => b.Category).Include(b => b.Author).Include(b => b.Publisher).Include(b => b.BookTags).ThenInclude(bt => bt.Tag).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value);
            }

            if (tagId.HasValue)
            {
                books = books.Where(b => b.BookTags.Any(bt => bt.TagId == tagId.Value));
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

            var book = await _context.Books.Include(b => b.Category).Include(b => b.Author).Include(b => b.Publisher).Include(b => b.Reviews).ThenInclude(r => r.User).Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            ViewData["AverageRating"] = book.Reviews.Any() ? book.Reviews.Average(r => r.Rating) : 0;
            ViewData["ReviewCount"] = book.Reviews.Count;
            ViewData["ImageUrl"] = book.ImageUrl;

            var similarBooks = GetSimilarBooks(book);
            ViewData["SimilarBooks"] = similarBooks;

            return View(book);
        }

        private List<Book> GetSimilarBooks(Book book)
        {
            var allBooks = _context.Books.Include(b => b.BookTags).ThenInclude(bt => bt.Tag).ToList();
            var bookTags = book.BookTags.Select(bt => bt.TagId).ToHashSet();

            var similarBooks = allBooks
                .Where(b => b.Id != book.Id)
                .Select(b => new
                {
                    Book = b,
                    JaccardIndex = CalculateJaccardIndex(bookTags, b.BookTags.Select(bt => bt.TagId).ToHashSet())
                })
                .OrderByDescending(b => b.JaccardIndex)
                .Take(4)
                .Select(b => b.Book)
                .ToList();

            return similarBooks;
        }

        private double CalculateJaccardIndex(HashSet<int> set1, HashSet<int> set2)
        {
            var intersection = set1.Intersect(set2).Count();
            var union = set1.Union(set2).Count();
            return (double)intersection / union;
        }
    }
}
