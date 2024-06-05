using System;
using Microsoft.AspNetCore.Identity;

namespace TestMe.Models
{
    public class UserTest {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int TestId { get; set; }

        public DateTime CompletionDate { get; set; }

        public List<Question> Questions { get; set; }
    }
}

