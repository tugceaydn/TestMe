using System;
using System.ComponentModel.DataAnnotations;

namespace TestMe.ViewModels
{
	public class QuestionViewModel
	{
        public QuestionViewModel()
        {
            Text = "";
            Options = new List<string>();
        }

        [Required]
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int AnswerIndex { get; set; }
    }
}

