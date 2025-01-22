using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ksiegarnia.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ksiegarnia.Data;

namespace Ksiegarnia.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly KsiegarniaDbContext _context;

        public ReviewsController(KsiegarniaDbContext context)
        {
            _context = context;
        }

        // GET: Reviews/Create
        public IActionResult Create(int bookId)
        {
            ViewData["BookId"] = bookId;
            return View();
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId, Review review)
        {
            ModelState.Remove(nameof(review.User));
            ModelState.Remove(nameof(review.Book));
            ModelState.Remove(nameof(review.UserId));
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                review.BookId = bookId;
                review.UserId = userId;

                _context.Add(review);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "CustomerBooks", new { id = bookId });
            }
            ViewData["BookId"] = bookId;
            return View(review);
        }

    // GET: Reviews/Edit/5
    public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (review.UserId != userId)
            {
                return Forbid();
            }

            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Review review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            var existingReview = await _context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingReview == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingReview.UserId != userId)
            {
                return Forbid();
            }

            existingReview.Rating = review.Rating;
            ModelState.Remove(nameof(review.User));
            ModelState.Remove(nameof(review.Book));
            ModelState.Remove(nameof(review.UserId));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(existingReview);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reviews.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction("Details", "CustomerBooks", new { id = existingReview.BookId });
            }

            return View(review);
        }

    } 
}

