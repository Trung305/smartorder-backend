using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            var user = await _userService.GetAllAsync();
            return Ok(user);
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var username = User.Identity.Name;
            var user = await _userService.GetByUsernameAsync(username);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Username,
                user.Email,
                user.Role
            });
        }
        [HttpPost]
        public async Task<ActionResult> Create(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _userService.CreateAsync(user);
            return Ok();
        }
    }
}