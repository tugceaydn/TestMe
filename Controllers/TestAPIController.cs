using Microsoft.AspNetCore.Identity; // ASP.NET Core Identity services
using Microsoft.AspNetCore.Mvc; // MVC framework for ASP.NET Core
using Microsoft.EntityFrameworkCore; // Entity Framework Core for database operations
using TestMe.Data; // Application-specific data context
using TestMe.Models; // Application models

namespace TestMe.Controllers
{
    [Route("api/[controller]")] // Route template for API controller
    [ApiController] // Indicates that this is an API controller
    public class TestAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Database context
        private readonly UserManager<User> _userManager; // User manager for managing users

        public TestAPIController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Retrieves the number of users that created at least 1 test in the system
        [HttpGet] // HTTP GET method
        public async Task<ActionResult<int>> GetNumberOfUsersCreatedTest()
        {
            // Retrieve all tests from the database
            var tests = await _context.Tests.ToListAsync();

            // Extract unique user IDs
            var uniqueUserIds = tests.Select(t => t.CreatorId).Distinct().Count();

            // Return the count of unique user IDs as HTTP 200 OK response
            return Ok(uniqueUserIds);
        }
    }
}
