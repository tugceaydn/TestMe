using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMe.Data;
using TestMe.Models;

namespace TestMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TestAPIController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<int>> GetNumberOfUsersCreatedTest()
        {
            // Retrieve all tests from the database
            var tests = await _context.Tests.ToListAsync();

            // Extract unique user IDs
            var uniqueUserIds = tests.Select(t => t.CreatorId).Distinct().Count();

            // Return the count of unique user IDs
            return Ok(uniqueUserIds);
        }
    }
}

