using System;
using System.ComponentModel.DataAnnotations;

namespace TestMe.ViewModels
{
	public class SolveTestViewModel
	{
        public int TestId { get; set; }
        public string CreatorId { get; set; }
        public string Title { get; set; }
        public List<QuestionSolveViewModel> Questions { get; set; }
    }
}