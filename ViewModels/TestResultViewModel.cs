using System;
using System.ComponentModel.DataAnnotations;
using TestMe.Models;

namespace TestMe.ViewModels
{
    public class TestResultViewModel
    {
        public int TestId { get; set; }
        public string Title { get; set; }
        public List<QuestionResultViewModel> Questions { get; set; }
    }

    public class QuestionResultViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<Option> Options { get; set; }
        public int SelectedIndex { get; set; }
        public int AnswerIndex { get; set; }
    }
}

