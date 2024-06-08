using System;
using System.ComponentModel.DataAnnotations;

namespace TestMe.ViewModels
{
	public class TestListViewModel
	{
        public int Id { get; set; }
        public string Title { get; set; }
        public string CreatorId { get; set; }
        public DateTime CreationDate { get; set; }
        public int NumberOfQuestions { get; set; }
        public bool IsCompletedByMe { get; set; }
    }
}

