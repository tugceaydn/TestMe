// Required namespaces
using Microsoft.AspNetCore.Authorization; // For authorization attributes
using Microsoft.AspNetCore.Identity; // For user and role management
using Microsoft.AspNetCore.Mvc; // For MVC framework and controller actions
using Microsoft.EntityFrameworkCore; // For Entity Framework Core
using TestMe.Models; // For application-specific models
using TestMe.ViewModels; // For ViewModels used in the application
using TestMe.Data; // For application data context

// This controller handles actions related to the admin functionalities of the application.
// The [Authorize] attribute restricts access to users with the "Admin" role.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // Fields to store application database context and user manager for dependency injection
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    // Constructor to inject the dependencies: database context and user manager
    public AdminController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// It queries the database for all users and tests, and prepares a ViewModel containing both lists.
    public async Task<IActionResult> Index()
    {
        // Fetch all users from the database and project them into a list of UserViewModel
        var users = await _context.Users.Select(u => new UserViewModel
        {
            Id = u.Id, // User ID
            Email = u.Email // User Email
        }).ToListAsync();

        // Fetch all tests from the database and project them into a list of TestListViewModel
        var tests = await _context.Tests.Select(t => new TestListViewModel
        {
            Id = t.Id, // Test ID
            Title = t.Title, // Test title
            CreatorId = t.CreatorId, // ID of the user who created the test
            CreationDate = t.CreationDate // Date when the test was created
        }).ToListAsync();

        // Prepare a ViewModel containing lists of users and tests
        var viewModel = new AdminPanelViewModel
        {
            Users = users, // List of users
            Tests = tests // List of tests
        };

        // Pass the ViewModel to the view
        return View(viewModel);
    }

    /// This action method handles the deletion of a user.
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        // Find the user by their ID
        var user = await _userManager.FindByIdAsync(id);

        // If the user exists, delete them
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }

        // Redirect to the Index action to update the view
        return RedirectToAction(nameof(Index));
    }

    /// This action method handles the deletion of a test.
    [HttpPost]
    public async Task<IActionResult> DeleteTest(int id)
    {
        // Find the test by its ID, including its related questions and options
        var test = await _context.Tests.Include(t => t.Questions)
                                       .ThenInclude(q => q.Options)
                                       .FirstOrDefaultAsync(t => t.Id == id);

        // If the test exists, delete it and its related entities
        if (test != null)
        {
            // Remove options for each question
            foreach (var question in test.Questions)
            {
                _context.Options.RemoveRange(question.Options);
            }

            // Remove questions
            _context.Questions.RemoveRange(test.Questions);

            // Remove the test itself
            _context.Tests.Remove(test);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        // Redirect to the Index action to update the view
        return RedirectToAction(nameof(Index));
    }
}
