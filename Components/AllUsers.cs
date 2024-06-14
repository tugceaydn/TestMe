using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TestMe.ViewModels;

namespace TestMe.Components;

public class AllUsersViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(List<UserViewModel> users)
    {
        var viewModel = new AllUsersViewModel
        {
            Users = users,
        };

        return View(viewModel);
    }
}

public class AllUsersViewModel
{
    public List<UserViewModel> Users { get; set; }
}
