using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TestMe.ViewModels;

namespace TestMe.Components;

public class AllTestsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(List<TestListViewModel> tests, string userId)
    {
        var viewModel = new AllTestsViewModel
        {
            Tests = tests,
            UserId = userId
        };

        return View(viewModel);
    }
}

public class AllTestsViewModel
{
    public List<TestListViewModel> Tests { get; set; }
    public string UserId { get; set; }
}
