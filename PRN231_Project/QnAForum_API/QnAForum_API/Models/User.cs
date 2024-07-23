using System;
using System.Collections.Generic;

namespace QnAForum_API.Models
{
    public partial class User
    {
        public User()
        {
            Answers = new HashSet<Answer>();
            Questions = new HashSet<Question>();
        }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Role { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}
