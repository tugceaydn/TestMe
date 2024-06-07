using System;
using System.ComponentModel.DataAnnotations;

namespace TestMe.ViewModels
{
	public class CreateTestViewModel
	{
        public CreateTestViewModel()
        {
            Title = "";
            Questions = new List<QuestionViewModel>();
        }
        [Required]
        public string Title { get; set; }
        public List<QuestionViewModel> Questions { get; set; }
    }
}

