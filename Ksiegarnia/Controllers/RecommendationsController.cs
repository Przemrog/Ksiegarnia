using Ksiegarnia.Data;
using Ksiegarnia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using System.Linq;
using System.Threading.Tasks;

namespace Ksiegarnia.Controllers
{
    [Authorize]
    public class RecommendationsController : Controller
    {
        private readonly KsiegarniaDbContext _context;
        private readonly MLContext _mlContext;

        public RecommendationsController(KsiegarniaDbContext context)
        {
            _context = context;
            _mlContext = new MLContext();
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userReviews = await _context.Reviews.Where(r => r.UserId == userId).ToListAsync();

            if (userReviews.Count < 10)
            {
                ViewData["Message"] = "You need to have at least 10 reviews to get recommendations.";
                return View();
            }

            var bookRatings = userReviews.Select(r => new BookRating
            {
                UserId = float.Parse(r.UserId),
                BookId = r.BookId,
                Label = (float)r.Rating
            }).ToList();

            var trainingData = _mlContext.Data.LoadFromEnumerable(bookRatings);

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(BookRating.UserId),
                MatrixRowIndexColumnName = nameof(BookRating.BookId),
                LabelColumnName = nameof(BookRating.Label),
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var estimator = _mlContext.Recommendation().Trainers.MatrixFactorization(options);
            var model = estimator.Fit(trainingData);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<BookRating, BookRatingPrediction>(model);

            var allBooks = await _context.Books.Include(b => b.BookTags).ThenInclude(bt => bt.Tag).ToListAsync();
            var recommendedBooks = allBooks
                .Select(b => new
                {
                    Book = b,
                    Score = predictionEngine.Predict(new BookRating { UserId = float.Parse(userId), BookId = b.Id }).Score
                })
                .OrderByDescending(b => b.Score)
                .Take(10)
                .Select(b => b.Book)
                .ToList();

            ViewData["RecommendedBooks"] = recommendedBooks;

            return View();
        }
    }

    public class BookRatingPrediction
    {
        public float Score { get; set; }
    }
}
