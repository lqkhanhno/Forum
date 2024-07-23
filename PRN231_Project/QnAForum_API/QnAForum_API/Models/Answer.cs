using System;
using System.Collections.Generic;

namespace QnAForum_API.Models
{
    public partial class Answer
    {
        public int AnswerId { get; set; }
        public int? QuestionId { get; set; }
        public int? UserId { get; set; }
        public string Body { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual Question? Question { get; set; }
        public virtual User? User { get; set; }
    }
}
