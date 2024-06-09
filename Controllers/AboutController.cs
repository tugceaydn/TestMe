using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TestMe.Models;

namespace TestMe.Controllers;

public class AboutController : Controller
{
    public IActionResult Index()
    {
       return View();
    }
}

