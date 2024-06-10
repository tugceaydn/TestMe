using System;
using Microsoft.AspNetCore.Identity;

namespace TestMe.Models
{
    public class UserTest {
        public int Id { get; set; }

        public string UserId { get; set; }
        public int TestId { get; set; }

        public DateTime CompletionDate { get; set; }

        public List<int> UserAnswers { get; set; }
    }
}