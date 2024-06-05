using System;
using Microsoft.AspNetCore.Identity;

namespace TestMe.Models
{
    public class Question {
        public int Id { get; set; }
        public string Text { get; set; }

        public List<Option> Options { get; set; }
        public Option Answer { get; set; }
    }
}

