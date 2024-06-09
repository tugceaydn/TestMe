using System;
using System.ComponentModel.DataAnnotations;

namespace TestMe.ViewModels
{
	public class QuestionSolveViewModel
	{
        public int Id { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int SelectedOption { get; set; }
    }
}