using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QnAForum_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace QnAForum_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        QnAForumContext _context = new QnAForumContext();
        [HttpGet]
        public IActionResult getAllUser()
        {
            return Ok(_context.Users.ToList());
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User newUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the username or email already exists
            if (_context.Users.Any(u => u.Username == newUser.Username || u.Email == newUser.Email))
            {
                return Conflict("Username or email already exists.");
            }

            // Set the creation date
            newUser.CreatedAt = DateTime.Now;

            // Add the new user to the database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(newUser);
            //return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserId }, newUser);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(LoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

            if (user == null || user.Password != loginRequest.Password)
            {
                return Unauthorized("Invalid email or password.");
            }
            return user;
        }
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
       

    }
}