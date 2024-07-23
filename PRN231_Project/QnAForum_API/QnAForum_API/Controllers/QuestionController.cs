using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QnAForum_API.Models;
using System.Security.Claims;

namespace QnAForum_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    public class QuestionController : ControllerBase
    {
        public QnAForumContext _context = new QnAForumContext();
        // GET: api/Question
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionResponseDto>>> GetQuestions()
        {
            var questions = await _context.Questions
                .Include(q => q.User)
                .Select(q => new QuestionResponseDto
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Body = q.Body,
                    CreatedAt = q.CreatedAt ?? DateTime.MinValue,
                    Username = q.User.Username
                })
                .ToListAsync();

            return Ok(questions);
        }
        // GET: api/Question/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.User) // Include User information
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            // Remove any circular references or unnecessary data
            question.User.Questions = null; // Remove Questions navigation from User

            return question;
        }
        [HttpPost]
        public async Task<ActionResult<QuestionResponseDto>> CreateQuestion(QuestionDto questionDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == questionDto.Username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var addQuestion = new Question
            {
                UserId = user.UserId, // Ensure this matches the property name in your User model
                Title = questionDto.Title,
                Body = questionDto.Body,
                CreatedAt = DateTime.Now
            };
            _context.Questions.Add(addQuestion);
            await _context.SaveChangesAsync();

            var responseDto = new QuestionResponseDto
            {
                QuestionId = addQuestion.QuestionId,
                Title = addQuestion.Title,
                Body = addQuestion.Body,
                CreatedAt = DateTime.Now,
                Username = user.Username
            };

            return Ok(responseDto);
        }

        public class QuestionResponseDto
        {
            public int QuestionId { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Username { get; set; }
        }

        public class QuestionDto
        {
            public string Username { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
        }

        // PUT: api/Question/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, Question question)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingQuestion = await _context.Questions.FindAsync(id);

            if (existingQuestion == null || existingQuestion.UserId != userId)
            {
                return BadRequest();
            }

            existingQuestion.Title = question.Title;
            existingQuestion.Body = question.Body;

            _context.Entry(existingQuestion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Question/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Question>> DeleteQuestion(int id)
        {
            var question = await _context.Questions
                .Include(x => x.Answers)
                .FirstOrDefaultAsync(x => x.QuestionId == id);

            if (question == null)
            {
                return BadRequest();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.QuestionId == id);
        }
    }
}



