// Required namespaces
using Microsoft.AspNetCore.Mvc; // For MVC framework

namespace TestMe.Controllers
{
    /// The AboutController handles requests related to the "About" page of the application.
    public class AboutController : Controller
    {
        /// Returns a ViewResult object that renders the "About" page.
        public IActionResult Index()
        {
            // Return the default view for the "About" page
            return View();
        }
    }
}
