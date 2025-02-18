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
        var bookTags = await _context.BookTags.Where(bt => reviewedBookIds.Contains(bt.BookId)).ToListAsync();

        var reviewedTags = bookTags.Select(bt => new BookRating
        {
            TagId = (uint)bt.TagId,
            Label = (float)userReviews.First(r => r.BookId == bt.BookId).Rating
        }).ToList();

        var trainingData = _mlContext.Data.LoadFromEnumerable(reviewedTags);

        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = nameof(BookRating.TagId),
            MatrixRowIndexColumnName = nameof(BookRating.TagId),
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
                Score = b.BookTags.Sum(bt => predictionEngine.Predict(new BookRating { TagId = (uint)bt.TagId }).Score),
                Reason = b.BookTags
                    .OrderByDescending(bt => predictionEngine.Predict(new BookRating { TagId = (uint)bt.TagId }).Score)
                    .FirstOrDefault()?.Tag.Name
            })
            .OrderByDescending(b => b.Score)
            .Take(5)
            .ToList();

        ViewData["RecommendedBooks"] = recommendedBooks.Select(rb => rb.Book).ToList();
        ViewData["RecommendationReasons"] = recommendedBooks.ToDictionary(rb => rb.Book.Id, rb => rb.Reason);

        return View("Index");
    }

}

public class BookRatingPrediction
{
    public float Score { get; set; }
}
