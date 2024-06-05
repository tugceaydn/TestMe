using System;
using Microsoft.AspNetCore.Identity;

//public enum Gender { Unknown, Male, Female }

namespace TestMe.Models
{
    public class User : IdentityUser
    {
        public DateTime CreatedOn { get; set; }
    }

}

