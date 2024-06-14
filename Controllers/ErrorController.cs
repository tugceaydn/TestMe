using System.Diagnostics; // For accessing activity tracking and diagnostic information
using Microsoft.AspNetCore.Mvc; // For MVC framework and controller actions
using TestMe.Models; // For application-specific models like ErrorViewModel

// Handles error display and logging functionality in the application.
public class ErrorController : Controller
{
    // Displays error information without caching the response.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        // Renders the error view with error details.
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
