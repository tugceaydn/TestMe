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

        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public List<QuestionViewModel> Questions { get; set; }
    }
}

