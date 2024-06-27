using Ksiegarnia.Data;
using Ksiegarnia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ksiegarnia.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly KsiegarniaDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(KsiegarniaDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            var totalPrice = cart.Items.Sum(ci => ci.Book.Price * ci.Count);
            ViewBag.TotalPrice = totalPrice;

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int count)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            if (cart.Items == null)
            {
                cart.Items = new List<CartItem>();
            }

            var cartItem = cart.Items.FirstOrDefault(ci => ci.BookId == bookId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    BookId = bookId,
                    Count = count
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Count += count;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}