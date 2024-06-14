using Microsoft.AspNetCore.Identity; // ASP.NET Core Identity services
using Microsoft.AspNetCore.Mvc; // MVC framework for ASP.NET Core
using Microsoft.EntityFrameworkCore; // Entity Framework Core for database operations
using TestMe.Data; // Application-specific data context
using TestMe.Models; // Application models
using TestMe.ViewModels; // View models for presenting data

namespace TestMe.Controllers
{
    public class ResultController : Controller
    {
        private readonly ApplicationDbContext _context; // Database context
        private readonly UserManager<User> _userManager; // User manager for managing users

        public ResultController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Index action to list the tests solved by the user
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Retrieve user's completed tests with test details
            var userTests = await _context.UserTests
                .Where(ut => ut.UserId == user.Id)
                .Join(_context.Tests,
                    ut => ut.TestId,
                    t => t.Id,
                    (ut, t) => new TestListViewModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        CreatorId = t.CreatorId,
                        CreationDate = t.CreationDate,
                        NumberOfQuestions = t.Questions.Count,
                        IsCompletedByMe = true
                    })
                .OrderByDescending(t => t.CreationDate)
                .ToListAsync();

            return View(userTests);
        }

        // Details action to show the results of a specific test
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Retrieve user's test result
            var userTest = await _context.UserTests
                .FirstOrDefaultAsync(ut => ut.TestId == id && ut.UserId == user.Id);

            if (userTest == null)
            {
                return NotFound();
            }

            // Retrieve the test details including questions and options
            var test = await _context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            // Construct view model for displaying test result
            var viewModel = new TestResultViewModel
            {
                TestId = test.Id,
                Title = test.Title,
                Questions = test.Questions.Select((q, index) => new QuestionResultViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    Options = q.Options,
                    AnswerIndex = q.AnswerIndex,
                    SelectedIndex = userTest.UserAnswers[index]
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
