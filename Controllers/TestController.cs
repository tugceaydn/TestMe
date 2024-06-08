using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMe.Data;
using TestMe.Models;
using TestMe.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestMe.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TestController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Get user
            var user = await _userManager.GetUserAsync(User);

            // Retrieve all tests from the database
            var tests = await _context.Tests.Include(t => t.Questions)
                                            .ThenInclude(q => q.Options)
                                            .ToListAsync();

            // Get UserTests entries for the current user
            var userTests = await _context.UserTests
                                          .Where(ut => ut.UserId == user!.Id)
                                          .ToListAsync();

            var testListViewModel = tests.Select(t => new TestListViewModel
            {
                Id = t.Id,
                Title = t.Title,
                NumberOfQuestions = t.Questions.Count,
                IsCompletedByMe = userTests.Find(ut => ut.TestId == t.Id) != null,
                CreatorId = t.CreatorId,
                CreationDate = t.CreationDate,
            });

            return View(testListViewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var test = await _context.Tests
                                 .Include(t => t.Questions)
                                    .ThenInclude(q => q.Options)
                                 .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // Create Page UI
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View(new CreateTestViewModel());
        }

        // Create Page Form Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTestViewModel model)
        {
            // Check if has at least 2 questions
            if (model.Questions.Count < 2)
            {
                ModelState.AddModelError(string.Empty, "Your test should have at least 2 questions");
            }

            // Check for empty options and duplicate options within each question
            foreach (var question in model.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    ModelState.AddModelError(string.Empty, "A question has empty text.");
                }

                var emptyOptions = question.Options
                    .Where(o => string.IsNullOrWhiteSpace(o))
                    .ToList();

                if (emptyOptions.Any())
                {
                    ModelState.AddModelError(string.Empty, $"Question '{question.Text}' has empty options.");
                }

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
                // Get user
                var user = await _userManager.GetUserAsync(User);

                // Create new Test entry for DB
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

                _context.Tests.Add(test);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Test");
            }

            return View(model);
        }

        // Edit Page UI
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var test = await _context.Tests
                                 .Include(t => t.Questions)
                                    .ThenInclude(q => q.Options)
                                 .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null || test.CreatorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

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

            return View(model);
        }

        // Edit Page Form Submit
        [HttpPost]
        public async Task<IActionResult> Edit(CreateTestViewModel model)
        {
            System.Diagnostics.Debug.WriteLine(model.Title);
            System.Diagnostics.Debug.WriteLine(model.Questions[0].Text);
            System.Diagnostics.Debug.WriteLine(model.Questions[0].Id);
            System.Diagnostics.Debug.WriteLine(model.Id);

            // Check if has at least 2 questions
            if (model.Questions.Count < 2)
            {
                ModelState.AddModelError(string.Empty, "Your test should have at least 2 questions");
            }

            // Check for empty options and duplicate options within each question
            foreach (var question in model.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    ModelState.AddModelError(string.Empty, "A question has empty text.");
                }

                var emptyOptions = question.Options
                    .Where(o => string.IsNullOrWhiteSpace(o))
                    .ToList();

                if (emptyOptions.Any())
                {
                    ModelState.AddModelError(string.Empty, $"Question '{question.Text}' has empty options.");
                }

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
                // Get user
                var user = await _userManager.GetUserAsync(User);

                // Fetch the existing Test entity from the database
                var existingTest = await _context.Tests
                    .Include(t => t.Questions) // Ensure Questions are loaded
                    .ThenInclude(q => q.Options)
                    .SingleOrDefaultAsync(t => t.Id == model.Id);

                if (existingTest != null)
                {
                    // Update Test entity with values from the model
                    existingTest.Title = model.Title;

                    // Remove Deleted Questions
                    foreach (var question in existingTest.Questions)
                    {
                        if (model.Questions.FirstOrDefault(q => q.Id == question.Id) == null)
                        {
                            _context.Questions.Remove(question);
                        }
                    }

                    // Update Questions
                    foreach (var question in model.Questions)
                    {
                        var existingQuestion = existingTest.Questions.FirstOrDefault(q => q.Id == question.Id);

                        if (existingQuestion != null)
                        {
                            existingQuestion.Text = question.Text;

                            for (int qi = 0; qi < 4; qi++)
                            {
                                existingQuestion.Options[qi].Text = question.Options[qi];
                            }

                            existingQuestion.AnswerIndex = question.AnswerIndex;
                        } else
                        {
                            // Handle the case where the question is not found
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

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Test");
                }
                else
                {
                    // Handle the case where the Test entity is not found
                    // For example, return a NotFoundResult or show an error message
                }

                // Create new Test entry for DB
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

                _context.Tests.Add(test);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Test");
            }

            return View(model);
        }

        //public IActionResult Delete(int id)
        //{
        //    var test = _context.Tests.Find(id);
        //    if (test == null || test.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        //    {
        //        return NotFound();
        //    }
        //    return View(test);
        //}

        //[HttpPost, ActionName("Delete")]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var test = await _context.Tests.FindAsync(id);
        //    _context.Tests.Remove(test);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
    }

}


