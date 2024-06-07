using System;
using System.ComponentModel.DataAnnotations;

namespace TestMe.ViewModels
{
	public class QuestionViewModel
	{
        public QuestionViewModel()
        {
            Id = -1;
            Text = "";
            Options = new List<string>();
        }

        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int AnswerIndex { get; set; }
    }
}

