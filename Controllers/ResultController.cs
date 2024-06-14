using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMe.Data;
using TestMe.Models;
using TestMe.ViewModels;
using System.Linq;
using System.Threading.Tasks;
namespace TestMe.Controllers
{
	public class ResultController : Controller
	{
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

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
                // TODO :: fix url
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

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
                .ToListAsync();

            // Debugging information
            System.Diagnostics.Debug.WriteLine($"User ID: {user.Id}");
            System.Diagnostics.Debug.WriteLine($"UserTests count: {userTests.Count}");
            if (userTests.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"First Test ID: {userTests[0].Id}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No tests found for this user.");
            }


            return View(userTests);
        }
        // index -> all completed test
        // result/{id} -> readonly (test + usertest) display correct answers
        // question background red (incorrect) green (correct)
    }
}

