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
            // Retrieve all tests from the database
            var tests = await _context.Tests.Include(t => t.Questions)
                                            .ThenInclude(q => q.Options)
                                            .ToListAsync();
            return View(tests);
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

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View(new CreateTestViewModel());
        }

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

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        //public IActionResult Edit(int id)
        //{
        //    var test = _context.Tests.Find(id);
        //    if (test == null || test.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        //    {
        //        return NotFound();
        //    }
        //    return View(test);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit(int id, Test test)
        //{
        //    if (id != test.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(test);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!_context.Tests.Any(t => t.Id == test.Id))
        //            {
        //                return NotFound();
        //            }
        //            throw;
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(test);
        //}

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


