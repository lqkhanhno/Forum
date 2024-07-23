using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QnAForum_API.Models;
using System.Security.Claims;

namespace QnAForum_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        public QnAForumContext _context = new QnAForumContext();
        [HttpPost("question/{questionId}")]
        public async Task<ActionResult<AnswerDTO>> PostAnswer(int questionId, [FromBody] AnswerInputModel inputModel)
        {
            try
            {
                var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

                if (question == null)
                {
                    return NotFound("Câu hỏi không tồn tại.");
                }

                if (string.IsNullOrWhiteSpace(inputModel.Body))
                {
                    return BadRequest("Nội dung câu trả lời không được để trống.");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == inputModel.Username);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var answer = new Answer
                {
                    QuestionId = questionId,
                    UserId = user.UserId,
                    Body = inputModel.Body,
                    CreatedAt = DateTime.Now
                };

                _context.Answers.Add(answer);
                await _context.SaveChangesAsync();

                var answerDTO = new AnswerDTO
                {
                    AnswerId = answer.AnswerId,
                    QuestionId = answer.QuestionId,
                    UserId = answer.UserId,
                    Body = answer.Body,
                    CreatedAt = answer.CreatedAt
                };

                return Ok(answerDTO);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public class AnswerDTO
        {
            public int AnswerId { get; set; }
            public int? QuestionId { get; set; }
            public int? UserId { get; set; }
            public string Body { get; set; } = null!;
            public DateTime? CreatedAt { get; set; }
        }

        public class AnswerInputModel
        {
            public string Username { get; set; } = null!;
            public string Body { get; set; } = null!;
        }

        // GET: api/Answer/question/{questionId}
        [HttpGet("question/{questionId}")]
        public async Task<ActionResult<IEnumerable<Answer>>> GetAnswersForQuestion(int questionId)
        {
            var answers = await _context.Answers
                .Include(a => a.User)
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();

            if (answers == null || answers.Count == 0)
            {
                return NotFound();
            }

            // Remove unnecessary data to prevent serialization issues
            answers.ForEach(answer =>
            {
                answer.User.Answers = null; // Remove Answers navigation from User
                answer.User.Questions = null; // Remove Questions navigation from User
            });

            return answers;
        }


        // PUT: api/Answer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnswer(int id, Answer answer)
        {
            if (id != answer.AnswerId)
            {
                return BadRequest("ID không khớp với câu trả lời.");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var existingAnswer = await _context.Answers.FindAsync(id);

            if (existingAnswer == null)
            {
                return NotFound("Câu trả lời không tồn tại.");
            }

            if (existingAnswer.UserId != userId)
            {
                return Forbid("Bạn không có quyền sửa câu trả lời này.");
            }

            // Update các thông tin cần thiết của câu trả lời
            existingAnswer.Body = answer.Body;
            existingAnswer.CreatedAt = DateTime.UtcNow; // Cập nhật thời gian sửa đổi

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }
        // DELETE: api/Answer/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var answer = await _context.Answers.FindAsync(id);
            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // GET: api/Answer/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Answer>>> GetAnswersByUser(int userId)
        {
            var answers = await _context.Answers
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (answers == null || answers.Count == 0)
            {
                return NotFound("Không tìm thấy câu trả lời cho người dùng này.");
            }

            return answers;
        }
        [HttpGet("questionAnswerCounts")]
        public async Task<ActionResult<IEnumerable<QuestionAnswerCountDto>>> GetQuestionAnswerCounts()
        {
            var answerCounts = await _context.Questions
                .Select(q => new QuestionAnswerCountDto
                {
                    QuestionId = q.QuestionId,
                    AnswerCount = q.Answers.Count()
                })
                .ToListAsync();

            return Ok(answerCounts);
        }
        public class QuestionAnswerCountDto
        {
            public int QuestionId { get; set; }
            public int AnswerCount { get; set; }
        }
    }
}
