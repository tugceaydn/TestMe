﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMe.Data;
using TestMe.Models;
using TestMe.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestMe.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly Test TEST_DATA;

        Test getDummyTest()
        {

            var OPTION1 = new Option
            {
                Id = 567,
                Text = "Option1"
            };

            var OPTION2 = new Option
            {
                Id = 563217,
                Text = "Option2"
            };

            var OPTION3 = new Option
            {
                Id = 5672,
                Text = "Option3"
            };

            var OPTION4 = new Option
            {
                Id = 562167,
                Text = "Option4"
            };

            var SAMPLE_TEST = new Test
            {
                Id = 123,
                Title = "Test Title",
                CreatorId = 123456,
                CreationDate = new DateTime(),
                Questions = new List<Question>{
                    new Question {
                        Id = 126123,
                        Text = "Question is here",
                        Answer = OPTION2,
                        Options = new List<Option>{ OPTION1, OPTION2, OPTION3, OPTION4 }
                    },
                    new Question {
                        Id = 66126123,
                        Text = "Question2 is here",
                        Answer = OPTION3,
                        Options = new List<Option>{ OPTION1, OPTION2, OPTION3, OPTION4 }
                    },
                    new Question {
                        Id = 265226123,
                        Text = "Question3 is here",
                        Answer = OPTION1,
                        Options = new List<Option>{ OPTION1, OPTION2, OPTION3, OPTION4 }
                    }
                }
            };

            return SAMPLE_TEST;
        }

        public TestController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
            TEST_DATA = getDummyTest();
        }

        public IActionResult Index()
        {
            var tests = new List<Test>
            {
                TEST_DATA
            };

            return View(tests);
        }

        public IActionResult Details(int id)
        {
            var test = TEST_DATA;

            if (test == null)
            {
                return NotFound();
            }
            return View(test);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTestViewModel model)
        {
            System.Diagnostics.Debug.WriteLine(ModelState.IsValid);
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                System.Diagnostics.Debug.WriteLine(user);

                model.Questions ??= new List<QuestionViewModel>();
                var test = new Test
                {
                    Title = model.Title,
                    CreatorId = Int32.Parse(user!.Id),
                    CreationDate = DateTime.UtcNow,
                    Questions = model.Questions.Select(q => new Question
                    {
                        Text = q.Text,
                        Options = q.Options.Select(o => new Option
                        {
                            Text = o
                        }).ToList(),
                        Answer = new Option
                        {
                            Text = q.Options[q.AnswerId]
                        }
                    }).ToList()
                };

                _context.Tests.Add(test);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        //public IActionResult Edit(int id)
        //{
        //    var test = _context.Tests.Find(id);
        //    if (test == null || test.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        //    {
        //        return NotFound();
        //    }
        //    return View(test);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit(int id, Test test)
        //{
        //    if (id != test.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(test);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!_context.Tests.Any(t => t.Id == test.Id))
        //            {
        //                return NotFound();
        //            }
        //            throw;
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(test);
        //}

        //public IActionResult Delete(int id)
        //{
        //    var test = _context.Tests.Find(id);
        //    if (test == null || test.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        //    {
        //        return NotFound();
        //    }
        //    return View(test);
        //}

        //[HttpPost, ActionName("Delete")]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var test = await _context.Tests.FindAsync(id);
        //    _context.Tests.Remove(test);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
    }

}


