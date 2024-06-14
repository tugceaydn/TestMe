using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMe.Models;
using TestMe.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using TestMe.Data;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.Select(u => new UserViewModel
        {
            Id = u.Id,
            Email = u.Email
        }).ToListAsync();

        var tests = await _context.Tests.Select(t => new TestListViewModel
        {
            Id = t.Id,
            Title = t.Title,
            CreatorId = t.CreatorId,
            CreationDate = t.CreationDate
        }).ToListAsync();

        var viewModel = new AdminPanelViewModel
        {
            Users = users,
            Tests = tests
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTest(int id)
    {

        // Find the test to be deleted
        var test = await _context.Tests.Include(t => t.Questions)
                                       .ThenInclude(q => q.Options)
                                       .FirstOrDefaultAsync(t => t.Id == id);

        if (test != null)
        {
            // Remove options of each question
            foreach (var question in test.Questions)
            {
                _context.Options.RemoveRange(question.Options);
            }

            // Remove questions
            _context.Questions.RemoveRange(test.Questions);

            // Remove the test
            _context.Tests.Remove(test);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
