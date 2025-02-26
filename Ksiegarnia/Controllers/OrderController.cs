using Ksiegarnia.Data;
using Ksiegarnia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace Ksiegarnia.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly KsiegarniaDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(KsiegarniaDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c=> c.UserId == user.Id);

            if (cart == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                ShippingAddress = "",
                ShippingCity = "",
                SumPrice = cart.Items.Sum(ci => ci.Book.Price * ci.Count),
                OrderItems = cart.Items.Select(ci => new OrderItem
                {
                    BookId = ci.BookId,
                    Book = ci.Book,
                    Count = ci.Count,
                    UnitPrice = ci.Book.Price
                }).ToList()
            };

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            order.UserId = user.Id;
            order.OrderDate = DateTime.Now;
            order.SumPrice = cart.Items.Sum(ci => ci.Book.Price * ci.Count);
            order.OrderItems = new List<OrderItem>();

            foreach (var item in cart.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    BookId = item.BookId,
                    Count = item.Count,
                    UnitPrice = item.Book.Price
                });
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new {id = order.Id});
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi =>  oi.Book)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
            }

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .Include(o => o.User)
                .ToListAsync();
            return View(orders);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> UserOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> UserOrdersDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                ShippingAddress = "To be filled",
                ShippingCity = "To be filled",
                SumPrice = cart.Items.Sum(ci => ci.Book.Price * ci.Count),
                OrderItems = cart.Items.Select(ci => new OrderItem
                {
                    BookId = ci.BookId,
                    Count = ci.Count,
                    UnitPrice = ci.Book.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = cart.Items.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Book.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Book.Title,
                        },
                    },
                    Quantity = item.Count,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = Url.Action("OrderConfirmation", "Order", new { id = order.Id }, Request.Scheme),
                CancelUrl = Url.Action("Index", "Cart", null, Request.Scheme),
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);


            return Redirect(session.Url);
        }

    }
}
