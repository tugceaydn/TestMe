using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMe.Data;
using TestMe.Models;
using TestMe.ViewModels;

namespace TestMe.Controllers
{
    public class TestController : Controller
    {
        // Database context to access the data
        private readonly ApplicationDbContext _context;

        // User manager to handle user information
        private readonly UserManager<User> _userManager;

        // Constructor for injecting dependencies
        public TestController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Action method to display the list of tests
        public async Task<IActionResult> Index()
        {
            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);
            var userId = user == null ? "unknown" : user.Id;

            // Redirect admin users to the admin panel
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            // Automatically assign the 'User' role to authenticated users who are not admins or users yet
            if (User.Identity!.IsAuthenticated && user != null &&
                await _userManager.IsInRoleAsync(user, "Admin") == false &&
                await _userManager.IsInRoleAsync(user, "User") == false)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            // Fetch all tests along with their questions and options
            var tests = await _context.Tests
                                      .Include(t => t.Questions)
                                      .ThenInclude(q => q.Options)
                                      .ToListAsync();

            // Retrieve UserTests entries for the current user
            var userTests = await _context.UserTests
                                          .Where(ut => ut.UserId == userId)
                                          .ToListAsync();

            // Create a ViewModel for the tests
            var testListViewModel = tests.Select(t => new TestListViewModel
            {
                Id = t.Id,
                Title = t.Title,
                NumberOfQuestions = t.Questions.Count,
                IsCompletedByMe = userTests.Find(ut => ut.TestId == t.Id) != null,
                CreatorId = t.CreatorId,
                CreationDate = t.CreationDate,
            });

            // Pass the ViewModel to the view
            return View(testListViewModel);
        }

        // Action method to show the create test page
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            // Display a form to create a new test
            return View(new CreateTestViewModel());
        }

        // Action method to handle the submission of the create test form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTestViewModel model)
        {
            // Ensure the test has at least 2 questions
            if (model.Questions.Count < 2)
            {
                ModelState.AddModelError(string.Empty, "Your test should have at least 2 questions");
            }

            // Validate the questions and their options
            foreach (var question in model.Questions)
            {
                // Check for empty question text
                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    ModelState.AddModelError(string.Empty, "A question has empty text.");
                }

                // Check for empty options
                var emptyOptions = question.Options
                    .Where(o => string.IsNullOrWhiteSpace(o))
                    .ToList();

                if (emptyOptions.Any())
                {
                    ModelState.AddModelError(string.Empty, $"Question '{question.Text}' has empty options.");
                }

                // Check for duplicate options
                var duplicateOptions = question.Options
                    .GroupBy(o => o != null ? o.Trim() : "")
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateOptions.Any())
                {
                    ModelState.AddModelError(string.Empty, $"Duplicate options found in question '{question.Text}': {string.Join(", ", duplicateOptions)}");
                }
            }

            // Check for duplicate questions
            var duplicateQuestions = model.Questions
                .GroupBy(q => q.Text != null ? q.Text.Trim() : "")
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateQuestions.Any())
            {
                ModelState.AddModelError(string.Empty, "Duplicate questions found: " + string.Join(", ", duplicateQuestions));
            }

            if (ModelState.IsValid)
            {
                // Retrieve the current user
                var user = await _userManager.GetUserAsync(User);

                // Create a new Test entity to save in the database
                var test = new Test
                {
                    Title = model.Title,
                    CreatorId = user!.Id,
                    CreationDate = DateTime.UtcNow,
                    Questions = model.Questions.Select(q => new Question
                    {
                        Text = q.Text,
                        Options = q.Options.Select(o => new Option
                        {
                            Text = o
                        }).ToList(),
                        AnswerIndex = q.AnswerIndex
                    }).ToList()
                };

                // Add and save the new test to the database
                _context.Tests.Add(test);
                await _context.SaveChangesAsync();

                // Redirect to the list of tests
                return RedirectToAction("Index", "Test");
            }

            // Redisplay the form if the model is invalid
            return View(model);
        }

        // Action method to show the edit test page
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            // Retrieve the test along with its questions and options
            var test = await _context.Tests
                                      .Include(t => t.Questions)
                                      .ThenInclude(q => q.Options)
                                      .FirstOrDefaultAsync(t => t.Id == id);

            // Check if the test exists and if the current user is the creator
            if (test == null || test.CreatorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            // Create a ViewModel to pass to the view
            var model = new CreateTestViewModel()
            {
                Id = test.Id,
                Questions = test.Questions.Select(q => new QuestionViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    Options = q.Options.Select(o => o.Text).ToList(),
                    AnswerIndex = q.AnswerIndex,
                }).ToList(),
                Title = test.Title,
            };

            // Pass the ViewModel to the view
            return View(model);
        }

        // Action method to handle the submission of the edit test form
        [HttpPost]
        public async Task<IActionResult> Edit(CreateTestViewModel model)
        {
            // Ensure the test has at least 2 questions
            if (model.Questions.Count < 2)
            {
                ModelState.AddModelError(string.Empty, "Your test should have at least 2 questions");
            }

            // Validate the questions and their options
            foreach (var question in model.Questions)
            {
                // Check for empty question text
                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    ModelState.AddModelError(string.Empty, "A question has empty text.");
                }

                // Check for empty options
                var emptyOptions = question.Options
                    .Where(o => string.IsNullOrWhiteSpace(o))
                    .ToList();

                if (emptyOptions.Any())
                {
                    ModelState.AddModelError(string.Empty, $"Question '{question.Text}' has empty options.");
                }

                // Check for duplicate options
                var duplicateOptions = question.Options
                    .GroupBy(o => o != null ? o.Trim() : "")
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateOptions.Any())
                {
                    ModelState.AddModelError(string.Empty, $"Duplicate options found in question '{question.Text}': {string.Join(", ", duplicateOptions)}");
                }
            }

            // Check for duplicate questions
            var duplicateQuestions = model.Questions
                .GroupBy(q => q.Text != null ? q.Text.Trim() : "")
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateQuestions.Any())
            {
                ModelState.AddModelError(string.Empty, "Duplicate questions found: " + string.Join(", ", duplicateQuestions));
            }

            if (ModelState.IsValid)
            {
                // Retrieve the current user
                var user = await _userManager.GetUserAsync(User);

                // Fetch the existing Test entity from the database
                var existingTest = await _context.Tests
                    .Include(t => t.Questions) // Ensure Questions are loaded
                    .ThenInclude(q => q.Options)
                    .SingleOrDefaultAsync(t => t.Id == model.Id);

                if (existingTest != null)
                {
                    // Update the existing test with the new values from the model
                    existingTest.Title = model.Title;

                    // Remove deleted questions
                    foreach (var question in existingTest.Questions)
                    {
                        if (model.Questions.FirstOrDefault(q => q.Id == question.Id) == null)
                        {
                            _context.Questions.Remove(question);
                        }
                    }

                    // Update existing questions and add new ones
                    foreach (var question in model.Questions)
                    {
                        var existingQuestion = existingTest.Questions.FirstOrDefault(q => q.Id == question.Id);

                        if (existingQuestion != null)
                        {
                            // Update the existing question
                            existingQuestion.Text = question.Text;

                            // Update the options for the question
                            for (int qi = 0; qi < 4; qi++)
                            {
                                existingQuestion.Options[qi].Text = question.Options[qi];
                            }

                            existingQuestion.AnswerIndex = question.AnswerIndex;
                        }
                        else
                        {
                            // Add a new question if it doesn't already exist
                            existingTest.Questions.Add(new Question
                            {
                                Text = question.Text,
                                Options = question.Options.Select(o => new Option
                                {
                                    Text = o
                                }).ToList(),
                                AnswerIndex = question.AnswerIndex
                            });
                        }
                    }

                    // Save the changes to the database
                    await _context.SaveChangesAsync();

                    // Redirect to the list of tests
                    return RedirectToAction("Index", "Test");
                }
                else
                {
                    // Handle the case where the test is not found
                    // For example, return a NotFoundResult or show an error message
                }

                // Create a new Test entity if the existing test is not found (may need further handling)
                var test = new Test
                {
                    Title = model.Title,
                    CreatorId = user!.Id,
                    CreationDate = DateTime.UtcNow,
                    Questions = model.Questions.Select(q => new Question
                    {
                        Text = q.Text,
                        Options = q.Options.Select(o => new Option
                        {
                            Text = o
                        }).ToList(),
                        AnswerIndex = q.AnswerIndex
                    }).ToList()
                };

                // Add and save the new test to the database
                _context.Tests.Add(test);
                await _context.SaveChangesAsync();

                // Redirect to the list of tests
                return RedirectToAction("Index", "Test");
            }

            // Redisplay the form if the model is invalid
            return View(model);
        }

        // Action method to show the solve test page
        public async Task<IActionResult> Details(int id)
        {
            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);
            var userId = user == null ? "unknown" : user.Id;

            // Check if the user has already solved this test
            var userTest = await _context.UserTests
                .FirstOrDefaultAsync(ut => ut.TestId == id && ut.UserId == userId);

            if (userTest != null)
            {
                // Redirect to the results page if the test has already been solved
                return RedirectToAction("Details", "Result", new { id = userTest.TestId });
            }

            // Retrieve the test along with its questions and options
            var test = await _context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                // Return a 404 error if the test is not found
                return NotFound();
            }

            // Create a ViewModel for the test
            var model = new SolveTestViewModel
            {
                TestId = test.Id,
                CreatorId = test.CreatorId,
                Title = test.Title,
                Questions = test.Questions.Select(q => new QuestionSolveViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    Options = q.Options?.Select(o => o.Text).ToList(),
                }).ToList()
            };

            // Pass the ViewModel to the view
            return View(model);
        }

        // Action method to handle the submission of the solve test form
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Details(SolveTestViewModel model)
        {
            // Check for any model validation errors
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();

            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                // Create a new UserTest entity to save in the database
                var userTest = new UserTest
                {
                    UserId = user!.Id,
                    TestId = model.TestId,
                    CompletionDate = DateTime.UtcNow,
                    UserAnswers = model.Questions.Select(q => q.SelectedOption).ToList()
                };

                // Add and save the UserTest entity to the database
                _context.UserTests.Add(userTest);
                await _context.SaveChangesAsync();

                // Redirect to the results page
                return RedirectToAction("Details", "Result", new { Id = model.TestId });
            }

            // Redisplay the form if the model is invalid
            return View(model);
        }
    }
}
