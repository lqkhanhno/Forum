using System;
using System.Collections.Generic;

namespace QnAForum_API.Models
{
    public partial class Question
    {
        public Question()
        {
            Answers = new HashSet<Answer>();
        }

        public int QuestionId { get; set; }
        public int? UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
    }
}
