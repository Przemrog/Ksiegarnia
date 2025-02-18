using Ksiegarnia.Data;
using Ksiegarnia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.Trainers;
using Microsoft.ML;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.Data;

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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["RecommendedBooks"] = new List<Book>();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GenerateRecommendations()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userReviews = await _context.Reviews.Where(r => r.UserId == userId).ToListAsync();

        if (userReviews.Count < 10)
        {
            ViewData["Message"] = "You need to have at least 10 reviews to get recommendations.";
            return View("Index");
        }

        var reviewedBookIds = userReviews.Select(r => r.BookId).ToHashSet();

        var bookRatings = userReviews.Select(r => new BookRating
        {
            BookId = (uint)r.BookId,
            Label = (float)r.Rating
        }).ToList();

        var trainingData = _mlContext.Data.LoadFromEnumerable(bookRatings);

        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = nameof(BookRating.BookId),
            MatrixRowIndexColumnName = nameof(BookRating.BookId),
            LabelColumnName = nameof(BookRating.Label),
            NumberOfIterations = 20,
            ApproximationRank = 100,
        };

        var estimator = _mlContext.Recommendation().Trainers.MatrixFactorization(options);
        var model = estimator.Fit(trainingData);

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<BookRating, BookRatingPrediction>(model);

        var allBooks = await _context.Books.Include(b => b.BookTags).ThenInclude(bt => bt.Tag).ToListAsync();
        var recommendedBooks = allBooks
            .Where(b => !reviewedBookIds.Contains(b.Id))
            .Select(b => new
            {
                Book = b,
                Score = predictionEngine.Predict(new BookRating { BookId = (uint)b.Id }).Score
            })
            .OrderByDescending(b => b.Score)
            .Take(5)
            .Select(b => b.Book)
            .ToList();

        ViewData["RecommendedBooks"] = recommendedBooks;

        return View("Index");
    }
}

public class BookRatingPrediction
{
    public float Score { get; set; }
}
