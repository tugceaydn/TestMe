using System;
using Microsoft.AspNetCore.Identity;

namespace TestMe.Models
{
    public class Test {
        public int Id { get; set; }
        public string Title { get; set; }

        public string CreatorId { get; set; }

        public DateTime CreationDate { get; set; }

        public List<Question> Questions { get; set; }
    }
}

